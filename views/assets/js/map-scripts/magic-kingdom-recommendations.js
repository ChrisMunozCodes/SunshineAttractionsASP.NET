(() => {
  "use strict";

  const parkCenter = { lat: 28.41874, lng: -81.58121 };
  // Coordinates are used only in the browser to calculate a straight-line walking estimate.
  const rides = [
    ["Space Mountain", "space-mountain", 28.4190, -81.5775],
    ["Haunted Mansion", "haunted-mansion", 28.4209, -81.5829],
    ["Big Thunder Mountain Railroad", "thunder-mountain", 28.4198, -81.5856],
    ["Pirates of the Caribbean", "pirates-of-the-caribbean", 28.4180, -81.5845],
    ["Jungle Cruise", "jungle-cruise", 28.4194, -81.5833],
    ["Peter Pan's Flight", "peter-pans-flight", 28.4204, -81.5812],
    ["Seven Dwarfs Mine Train", "seven-dwarfs-mine-train", 28.4206, -81.5785],
    ["it's a small world", "its-a-small-world", 28.4206, -81.5823],
    ["Dumbo the Flying Elephant", "dumbo-the-flying-elephant", 28.4187, -81.5761],
    ["The Barnstormer", "the-barnstormer", 28.4194, -81.5756],
    ["The Magic Carpets of Aladdin", "the-magic-carpets-of-aladdin", 28.4187, -81.5838],
    ["Tomorrowland Transit Authority PeopleMover", "tomorrowland-transit-authority-peoplemover", 28.4173, -81.5765],
    ["Under the Sea ~ Journey of The Little Mermaid", "under-the-sea-journey-of-the-little-mermaid", 28.4214, -81.5796],
    ["Buzz Lightyear's Space Ranger Spin", "buzz-lightyears-space-ranger-spin", 28.4177, -81.5769],
    ["Walt Disney's Enchanted Tiki Room", "enchanted-tiki-room", 28.4187, -81.5838],
    ["Walt Disney's Carousel of Progress", "walt-disneys-carousel-of-progress", 28.4172, -81.5765],
    ["Mad Tea Party", "mad-tea-party", 28.4199, -81.5780],
    ["Tomorrowland Speedway", "tomorrowland-speedway", 28.4172, -81.5776]
  ].map(([name, pageId, lat, lng]) => ({
    name, pageId, lat, lng,
    categories: ({
      "space-mountain": ["coaster", "dark"],
      "thunder-mountain": ["coaster", "adventure", "family"],
      "seven-dwarfs-mine-train": ["coaster", "family"],
      "the-barnstormer": ["coaster", "family", "gentle"],
      "haunted-mansion": ["dark", "family"],
      "pirates-of-the-caribbean": ["dark", "adventure", "family"],
      "peter-pans-flight": ["dark", "family", "gentle"],
      "under-the-sea-journey-of-the-little-mermaid": ["dark", "family", "gentle"],
      "jungle-cruise": ["adventure", "family", "gentle"],
      "the-magic-carpets-of-aladdin": ["adventure", "family", "gentle"],
      "dumbo-the-flying-elephant": ["family", "gentle"],
      "its-a-small-world": ["family", "gentle"],
      "tomorrowland-transit-authority-peoplemover": ["family", "gentle"],
      "enchanted-tiki-room": ["family", "gentle"],
      "walt-disneys-carousel-of-progress": ["family", "gentle"],
      "mad-tea-party": ["family", "gentle"],
      "tomorrowland-speedway": ["family", "gentle"],
      "buzz-lightyears-space-ranger-spin": ["dark", "family"]
    })[pageId] || []
  }));

  let dayPlan;
  try { dayPlan = JSON.parse(localStorage.getItem("sunshine-my-day:magic-kingdom") || "null"); } catch (_) { dayPlan = null; }
  const plannedRideIds = new Set((dayPlan?.rides || []).filter(ride => !ride.done).map(ride => ride.id));
  const completedRideIds = new Set((dayPlan?.rides || []).filter(ride => ride.done).map(ride => ride.id));

  let map, infoWindow, userMarker, userPosition;
  let liveWaits = new Map();
  let rideMarkers = [];
  const activeFilters = new Set();
  const config = window.magicKingdomMapConfig || { markers: [] };
  const finderPlaces = (Array.isArray(config.places) ? config.places : []).map(place => ({ name: place.name ?? place.Name, type: place.type ?? place.Type, description: place.description ?? place.Description, tags: place.tags ?? place.Tags ?? [], latitude: place.latitude ?? place.Latitude, longitude: place.longitude ?? place.Longitude, navigationQuery: place.navigationQuery ?? place.NavigationQuery }));
  let finderFilter = "all";
  let dietFilter = "all";
  const normalize = value => String(value || "").toLowerCase().replace(/[^a-z0-9]/g, "");
  const status = () => document.getElementById("planner-status");
  const setStatus = (message, state = "") => { const node = status(); if (node) { node.textContent = message; node.dataset.state = state; } };

  window.initMap = () => {
    map = new google.maps.Map(document.getElementById("map"), {
      center: parkCenter, zoom: 16, mapTypeControl: false, fullscreenControl: true, streetViewControl: false, clickableIcons: false
    });
    infoWindow = new google.maps.InfoWindow();
    createMarkers();
    loadWaitTimes();
  };

  function createMarkers() {
    const stored = new Map((config.markers || []).filter(marker => marker.lat && marker.lng).map(marker => [normalize(marker.name), marker]));
    rideMarkers = rides.map(ride => {
      const merged = { ...ride, ...(stored.get(normalize(ride.name)) || {}) };
      const marker = new google.maps.Marker({
        map, position: { lat: Number(merged.lat), lng: Number(merged.lng) }, title: merged.name,
        label: { text: "…", color: "#17324d", fontWeight: "700", fontSize: "11px" }
      });
      marker.addListener("click", () => showRideInfo(merged, marker));
      return { ride: merged, marker };
    });
  }

  async function loadWaitTimes() {
    const refresh = document.getElementById("refresh-waits");
    if (refresh) refresh.disabled = true;
    setStatus("Refreshing live wait times…");
    try {
      const controller = new AbortController();
      const timeout = setTimeout(() => controller.abort(), 12000);
      const response = await fetch("/wait-times", { signal: controller.signal, headers: { Accept: "application/json" } });
      clearTimeout(timeout);
      if (!response.ok) throw new Error("Wait-time service is unavailable.");
      const data = await response.json();
      liveWaits = new Map((data.lands || []).flatMap(land => land.rides || []).filter(ride => ride.name).map(ride => [normalize(ride.name), { wait: toMinutes(ride.wait_time), isOpen: ride.is_open !== false }]));
      updateMarkers();
      renderSmartTimeline();
      const openAttractions = [...liveWaits.values()].filter(ride => ride.isOpen).length;
      setStatus(openAttractions ? `Live wait times updated ${new Date().toLocaleTimeString([], { hour: "numeric", minute: "2-digit" })}.` : "Queue-Times currently reports that Magic Kingdom attractions are closed.", openAttractions ? "success" : "");
      if (userPosition) recommendRides();
    } catch (_) {
      setStatus("Live wait times are unavailable right now. You can still view ride locations.", "error");
      updateMarkers();
    } finally { if (refresh) refresh.disabled = false; }
  }

  function toMinutes(wait) { const value = Number.parseInt(wait, 10); return Number.isFinite(value) && value >= 0 ? value : null; }
  // A saved marker's wait value is not live data. Never render it as a current wait.
  function liveDataFor(ride) {
    const key = normalize(ride.name);
    const seasonalAliases = { junglecruise: "jinglecruise" };
    return liveWaits.get(key) ?? liveWaits.get(seasonalAliases[key]) ?? null;
  }
  function waitFor(ride) { const liveData = liveDataFor(ride); return liveData?.isOpen ? liveData.wait : null; }
  function isClosed(ride) { return liveDataFor(ride)?.isOpen === false; }
  function updateMarkers() { rideMarkers.forEach(({ ride, marker }) => { const wait = waitFor(ride); marker.setLabel({ text: isClosed(ride) ? "×" : wait === null ? "?" : `${wait}m`, color: "#17324d", fontWeight: "700", fontSize: "11px" }); }); }
  function matchesFilters(ride) { return activeFilters.size === 0 || ride.categories.some(category => activeFilters.has(category)); }
  function applyFilters() {
    rideMarkers.forEach(({ ride, marker }) => marker.setVisible(matchesFilters(ride)));
    const clearButton = document.getElementById("clear-filters");
    if (clearButton) clearButton.hidden = activeFilters.size === 0;
    if (userPosition) recommendRides();
  }

  function showRideInfo(ride, marker) {
    const wait = waitFor(ride);
    const distance = userPosition ? haversineMeters(userPosition, ride) : null;
    const waitText = isClosed(ride) ? "<b>Currently closed</b>" : wait === null ? "Wait unavailable" : `Current wait: <b>${wait} min</b>`;
    infoWindow.setContent(`<div style="padding:.35rem .2rem;font-family:Karla,Arial,sans-serif;min-width:180px"><strong style="font-size:1rem">${escapeHtml(ride.name)}</strong><p style="margin:.45rem 0">${waitText}</p>${distance === null ? "" : `<p style="margin:0">About ${formatDistance(distance)} away</p>`}<a style="display:inline-block;margin-top:.7rem;color:#174577;font-weight:700" href="${navigationUrl(ride)}" target="_blank" rel="noopener">Open walking directions</a></div>`);
    infoWindow.open({ map, anchor: marker });
  }

  function requestLocation() {
    const button = document.getElementById("share-location");
    if (!navigator.geolocation) return setStatus("This browser does not support location sharing.", "error");
    button.disabled = true;
    setStatus("Waiting for your location permission…");
    navigator.geolocation.getCurrentPosition(position => {
      userPosition = { lat: position.coords.latitude, lng: position.coords.longitude };
      if (userMarker) userMarker.setMap(null);
      userMarker = new google.maps.Marker({ map, position: userPosition, title: "Your location", icon: { path: google.maps.SymbolPath.CIRCLE, scale: 9, fillColor: "#0b72e7", fillOpacity: 1, strokeColor: "#fff", strokeWeight: 3 } });
      map.panTo(userPosition);
      const locationState = document.getElementById("location-state");
      locationState.textContent = "Location on"; locationState.dataset.active = "true";
      button.textContent = "Update my location"; button.disabled = false;
      recommendRides(); renderSmartTimeline(); renderFinder();
    }, error => {
      setStatus(error.code === error.PERMISSION_DENIED ? "Location access was denied. Allow it in your browser settings to get nearby recommendations." : "We could not determine your location. Try again while you are at the park.", "error");
      button.disabled = false;
    }, { enableHighAccuracy: true, timeout: 10000, maximumAge: 30000 });
  }

  function recommendRides() {
    const pool = rideMarkers.filter(({ ride }) => !completedRideIds.has(ride.pageId) && matchesFilters(ride) && (plannedRideIds.size === 0 || plannedRideIds.has(ride.pageId)));
    const candidates = pool.map(({ ride }) => { const wait = waitFor(ride); return wait === null ? null : { ...ride, wait, distance: haversineMeters(userPosition, ride) }; }).filter(Boolean);
    if (!candidates.length) {
      const list = document.getElementById("recommendation-list");
      const empty = document.getElementById("recommendation-empty");
      list.hidden = true; empty.hidden = false;
      empty.textContent = activeFilters.size ? "No rides matching these filters have live wait times right now. Try a different filter or refresh waits." : "Location found, but live wait data is unavailable. Refresh waits and try again.";
      return setStatus(empty.textContent, "error");
    }
    const maxWait = Math.max(...candidates.map(ride => ride.wait), 1);
    const maxDistance = Math.max(...candidates.map(ride => ride.distance), 1);
    candidates.forEach(ride => { ride.score = (ride.wait / maxWait) * .65 + (ride.distance / maxDistance) * .35; });
    renderRecommendations(candidates.sort((a, b) => a.score - b.score).slice(0, 3));
    setStatus("Recommendations use the latest wait times and your current distance from each ride.", "success");
  }

  function renderRecommendations(recommendations) {
    const list = document.getElementById("recommendation-list");
    document.getElementById("recommendation-empty").hidden = true;
    list.hidden = false;
    list.replaceChildren(...recommendations.map((ride, index) => {
      const item = document.createElement("li");
      item.className = "mk-rec";
      item.innerHTML = `<span class="mk-rank">${index + 1}</span><div><a href="/page/${encodeURIComponent(ride.pageId)}">${escapeHtml(ride.name)}</a><div class="mk-metrics"><span class="mk-metric">${ride.wait} min wait</span><span class="mk-metric">${formatDistance(ride.distance)} away</span></div><div class="mk-rec-actions"><a class="mk-navigate" href="${navigationUrl(ride)}" target="_blank" rel="noopener">Navigate</a><button type="button" class="mk-done">Mark done</button></div></div>`;
      item.querySelector(".mk-done").addEventListener("click", event => { event.stopPropagation(); markDone(ride); });
      item.addEventListener("click", event => { if (event.target.closest("a") || event.target.closest("button")) return; const match = rideMarkers.find(entry => entry.ride.name === ride.name); if (match) { map.panTo(match.marker.getPosition()); map.setZoom(18); showRideInfo(match.ride, match.marker); } });
      return item;
    }));
  }

  function haversineMeters(a, b) { const rad = degrees => degrees * Math.PI / 180; const dLat = rad(b.lat - a.lat); const dLng = rad(b.lng - a.lng); const value = Math.sin(dLat / 2) ** 2 + Math.cos(rad(a.lat)) * Math.cos(rad(b.lat)) * Math.sin(dLng / 2) ** 2; return 2 * 6371000 * Math.asin(Math.sqrt(value)); }
  function formatDistance(meters) { return meters < 1000 ? `${Math.round(meters)} m` : `${(meters / 1000).toFixed(1)} km`; }
  function navigationUrl(ride) { return `https://www.google.com/maps/dir/?api=1&destination=${encodeURIComponent(`${ride.lat},${ride.lng}`)}&travelmode=walking`; }
  function markDone(ride) { if (!dayPlan) dayPlan = { parkId: "magic-kingdom", createdAt: new Date().toISOString(), rides: [] }; let savedRide = dayPlan.rides.find(item => item.id === ride.pageId); if (!savedRide) { savedRide = { id: ride.pageId, name: ride.name, done: false }; dayPlan.rides.push(savedRide); } savedRide.done = true; plannedRideIds.delete(ride.pageId); completedRideIds.add(ride.pageId); localStorage.setItem("sunshine-my-day:magic-kingdom", JSON.stringify(dayPlan)); renderDayPlanSummary(); renderSmartTimeline(); recommendRides(); setStatus(`${ride.name} marked complete. Great job!`, "success"); }
  function renderDayPlanSummary() { const summary = document.getElementById("my-day-summary"); if (!summary) return; const completed = completedRideIds.size, remaining = plannedRideIds.size; if (!dayPlan?.rides?.length) { summary.hidden = true; return; } summary.hidden = false; document.getElementById("my-day-count").textContent = `${remaining} ride${remaining === 1 ? "" : "s"} remaining${completed ? ` · ${completed} done` : ""}.`; }
  function smartRoute() { const pending = rides.filter(ride => plannedRideIds.has(ride.pageId) && !completedRideIds.has(ride.pageId)); const ordered = []; let from = userPosition || parkCenter; while (pending.length) { const ranked = pending.map(ride => { const walk = haversineMeters(from, ride), wait = waitFor(ride), closed = isClosed(ride); return { ride, walk, wait, closed, score: (closed ? 10000 : wait === null ? 85 : wait * 1.25) + walk / 75 }; }).sort((a, b) => a.score - b.score); const next = ranked[0]; ordered.push(next); pending.splice(pending.findIndex(ride => ride.pageId === next.ride.pageId), 1); from = next.ride; } return ordered; }
  function renderSmartTimeline() { const list = document.getElementById("mk-timeline-list"), copy = document.getElementById("mk-timeline-copy"), state = document.getElementById("mk-timeline-state"), optimize = document.getElementById("mk-timeline-optimize"), note = document.getElementById("mk-timeline-note"); if (!list) return; if (!dayPlan?.rides?.length) { state.textContent = "Build a plan"; copy.textContent = "Choose your must-do rides and Sunny will turn them into a live, walk-aware route."; list.hidden = true; optimize.hidden = true; note.textContent = "Your plan stays on this device. You can change it anytime."; return; } const route = smartRoute(); optimize.hidden = false; optimize.textContent = userPosition ? "Refresh my route" : "Use my location"; state.textContent = userPosition ? "Live route" : "Park-start route"; if (!route.length) { copy.textContent = "You finished every ride in your plan. Nice work!"; list.hidden = true; note.textContent = "Want another round? Edit your must-dos or explore food and essentials."; return; } copy.textContent = userPosition ? "Sunny ordered your remaining must-dos using live waits and your current location." : "This starter route begins from the park center. Share location to make the walking order personal."; list.hidden = false; const visible = route.slice(0, 4); list.replaceChildren(...visible.map((entry, index) => { const item = document.createElement("li"), label = index === 0 ? "NOW" : index === 1 ? "NEXT" : "LATER", wait = entry.closed ? "Currently closed" : entry.wait === null ? "Wait updating" : `${entry.wait} min wait`, walk = userPosition || index > 0 ? `${formatDistance(entry.walk)} walk` : "From park center"; item.className = `mk-timeline-item${entry.closed ? " is-closed" : ""}`; item.innerHTML = `<span class="mk-timeline-step">${index + 1}</span><div><h3><span>${label}</span>${escapeHtml(entry.ride.name)}</h3><p>${entry.closed ? "Sunny moved this to the end of your route until it reopens." : index === 0 ? "Best next move based on the latest waits and walking time." : "Ordered to keep the day moving with less backtracking."}</p><div class="mk-timeline-metrics"><span>${wait}</span><span>${walk}</span></div><div class="mk-rec-actions"><a class="mk-navigate" href="${navigationUrl(entry.ride)}" target="_blank" rel="noopener">Navigate</a><button type="button" class="mk-done">Mark done</button></div></div>`; item.querySelector(".mk-done").addEventListener("click", () => markDone(entry.ride)); return item; })); note.textContent = route.length > visible.length ? `${route.length - visible.length} more planned ride${route.length - visible.length === 1 ? "" : "s"} will follow as your day changes.` : "Sunny recalculates this route when waits refresh, a ride closes, or you mark a ride done."; }
  function openFinder() { document.getElementById("mk-finder").hidden = false; renderFinder(); }
  function renderFinder() { const finder = document.getElementById("mk-finder"); if (!finder || finder.hidden) return; const list = document.getElementById("mk-finder-list"), empty = document.getElementById("mk-finder-empty"), label = document.getElementById("mk-finder-location"); label.textContent = userPosition ? "Closest locations are shown first using your current position." : "Use your location to put the closest spots first."; const matches = finderPlaces.filter(place => (finderFilter === "all" || place.type === finderFilter) && (dietFilter === "all" || (place.type === "food" && (place.tags || []).includes(dietFilter)))).map(place => ({ ...place, distance: userPosition ? haversineMeters(userPosition, { lat: place.latitude, lng: place.longitude }) : null })).sort((a, b) => a.distance === null || b.distance === null ? 0 : a.distance - b.distance); if (!matches.length) { list.replaceChildren(); empty.hidden = false; empty.textContent = dietFilter === "all" ? "No locations match that filter." : "No featured food locations match that dietary preference yet."; return; } empty.hidden = true; list.replaceChildren(...matches.map(place => { const item = document.createElement("li"), icon = ({ food: "🍽️", restroom: "🚻", "first-aid": "✚", locker: "🔒" })[place.type] || "📍", destination = place.navigationQuery || `${place.latitude},${place.longitude}`; item.className = "mk-finder-card"; item.innerHTML = `<span>${icon}</span><div><h3>${escapeHtml(place.name)}</h3><p>${escapeHtml(place.description)}</p>${place.distance === null ? "" : `<em>${formatDistance(place.distance)} away</em>`}</div><a href="https://www.google.com/maps/dir/?api=1&destination=${encodeURIComponent(destination)}&travelmode=walking" target="_blank" rel="noopener">Directions</a>`; return item; })); }
  function escapeHtml(value) { const element = document.createElement("div"); element.textContent = value; return element.innerHTML; }
  document.addEventListener("DOMContentLoaded", () => {
    renderDayPlanSummary();
    renderSmartTimeline();
    document.getElementById("open-mk-finder")?.addEventListener("click", openFinder);
    document.getElementById("close-mk-finder")?.addEventListener("click", () => document.getElementById("mk-finder").hidden = true);
    document.getElementById("mk-finder-location-button")?.addEventListener("click", requestLocation);
    document.getElementById("mk-timeline-optimize")?.addEventListener("click", () => { if (!userPosition) return requestLocation(); loadWaitTimes(); renderSmartTimeline(); });
    document.getElementById("mk-timeline-food")?.addEventListener("click", () => { finderFilter = "food"; document.querySelectorAll("[data-mk-finder-filter]").forEach(item => item.setAttribute("aria-pressed", String(item.dataset.mkFinderFilter === "food"))); openFinder(); });
    document.querySelectorAll("[data-mk-finder-filter]").forEach(button => button.addEventListener("click", () => { finderFilter = button.dataset.mkFinderFilter; document.querySelectorAll("[data-mk-finder-filter]").forEach(item => item.setAttribute("aria-pressed", String(item === button))); renderFinder(); }));
    document.querySelectorAll("[data-mk-diet-filter]").forEach(button => button.addEventListener("click", () => { dietFilter = button.dataset.mkDietFilter; document.querySelectorAll("[data-mk-diet-filter]").forEach(item => item.setAttribute("aria-pressed", String(item === button))); renderFinder(); }));
    document.getElementById("share-location")?.addEventListener("click", requestLocation);
    document.getElementById("refresh-waits")?.addEventListener("click", loadWaitTimes);
    document.querySelectorAll(".mk-filter input").forEach(input => input.addEventListener("change", event => {
      const filter = event.currentTarget.value;
      event.currentTarget.checked ? activeFilters.add(filter) : activeFilters.delete(filter);
      applyFilters();
    }));
    document.getElementById("clear-filters")?.addEventListener("click", () => {
      activeFilters.clear();
      document.querySelectorAll(".mk-filter input").forEach(input => input.checked = false);
      applyFilters();
    });
  });
})();
