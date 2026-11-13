(() => {
  "use strict";
  const config = window.parkPlannerConfig;
  if (!config) return;

  const rideData = {
    epcot: [
      ["Guardians of the Galaxy: Cosmic Rewind", "guardians-of-the-galaxy-cosmic-rewind", 28.3758, -81.5481, ["coaster", "dark", "thrill"]],
      ["Test Track", "test-track", 28.3732, -81.5460, ["coaster", "family", "adventure"]],
      ["Soarin' Across America", "soarin-around-the-world", 28.3718, -81.5522, ["family", "gentle", "adventure"]],
      ["Remy's Ratatouille Adventure", "remys-ratatouille-adventure", 28.3706, -81.5543, ["dark", "family"]],
      ["Frozen Ever After", "frozen-ever-after", 28.3714, -81.5505, ["family", "gentle"]],
      ["Spaceship Earth", "spaceship-earth", 28.3758, -81.5493, ["dark", "family", "gentle"]],
      ["Living with the Land", "living-with-the-land", 28.3713, -81.5525, ["family", "gentle"]],
      ["Mission: SPACE", "mission-space", 28.3734, -81.5469, ["family", "adventure"]]
    ],
    "hollywood-studios": [
      ["Star Wars: Rise of the Resistance", "star-wars-rise-of-the-resistance", 28.3547, -81.5613, ["dark", "family", "adventure"]],
      ["Mickey & Minnie's Runaway Railway", "mickeys-and-minnies-runaway-railway", 28.3568, -81.5615, ["dark", "family", "gentle"]],
      ["Slinky Dog Dash", "slinky-dog-dash", 28.3556, -81.5635, ["coaster", "family"]],
      ["The Twilight Zone Tower of Terror", "tower-of-terror", 28.3592, -81.5590, ["dark", "adventure"]],
      ["Rock 'n' Roller Coaster", "rock-n-roller-coaster", 28.3590, -81.5582, ["coaster", "dark", "adventure"]],
      ["Millennium Falcon: Smugglers Run", "millennium-falcon-smugglers-run", 28.3533, -81.5610, ["family", "adventure"]],
      ["Toy Story Mania!", "toy-story-mania", 28.3564, -81.5630, ["dark", "family", "gentle"]]
    ],
    "animal-kingdom": [
      ["Avatar Flight of Passage", "avatar-flight-of-passage", 28.3548, -81.5927, ["dark", "adventure"]],
      ["Na'vi River Journey", "navi-river-journey", 28.3540, -81.5920, ["dark", "family", "gentle"]],
      ["Expedition Everest - Legend of the Forbidden Mountain", "expedition-everest", 28.3587, -81.5876, ["coaster", "adventure"]],
      ["Kilimanjaro Safaris", "kilimanjaro-safaris", 28.3572, -81.5908, ["family", "gentle", "adventure"]],
      ["Kali River Rapids", "kali-river-rapids", 28.3594, -81.5882, ["family", "adventure"]],
      ["Festival of the Lion King", "festival-of-the-lion-king", 28.3590, -81.5903, ["family", "gentle"]],
      ["Finding Nemo: The Big Blue... and Beyond!", "finding-nemo-the-big-blue-and-beyond", 28.3559, -81.5889, ["family", "gentle"]]
    ],
    "seaworld-orlando": [
      ["Mako", "mako", 28.4102, -81.4635, ["coaster", "adventure"]],
      ["Kraken", "kraken-unleashed", 28.4091, -81.4656, ["coaster", "adventure"]],
      ["Manta", "manta", 28.4059, -81.4608, ["coaster", "adventure"]],
      ["Ice Breaker", "ice-breaker", 28.4126, -81.4627, ["coaster", "family", "adventure"]],
      ["Pipeline: The Surf Coaster", "pipeline-the-surf-coaster", 28.4106, -81.4623, ["coaster", "adventure"]],
      ["Journey to Atlantis", "journey-to-atlantis", 28.4092, -81.4622, ["family", "adventure"]],
      ["Penguin Trek", "penguin-trek", 28.4121, -81.4605, ["coaster", "family"]],
      ["Infinity Falls", "infinity-falls", 28.4073, -81.4618, ["family", "adventure"]],
      ["Super Grover's Box Car Derby", "super-grovers-box-car-derby", 28.4107, -81.4601, ["coaster", "family", "gentle"]]
    ]
  };
  const rides = (rideData[config.parkId] || []).map(([name, pageId, lat, lng, categories]) => ({ name, pageId, lat, lng, categories }));
  const planKey = `sunshine-my-day:${config.parkId}`;
  let dayPlan;
  try { dayPlan = JSON.parse(localStorage.getItem(planKey) || "null"); } catch (_) { dayPlan = null; }
  const plannedRideIds = new Set((dayPlan?.rides || []).filter(ride => !ride.done).map(ride => ride.id));
  const completedRideIds = new Set((dayPlan?.rides || []).filter(ride => ride.done).map(ride => ride.id));
  const normalize = value => String(value || "").toLowerCase().replace(/[^a-z0-9]/g, "");
  const aliases = { soarinacrossamerica: "soarinaroundtheworld", expeditioneverestlegendoftheforbiddenmountain: "expeditioneverest" };
  const activeFilters = new Set();
  let map, infoWindow, userMarker, userPosition, liveWaits = new Map(), markers = [];
  const status = () => document.getElementById("planner-status");
  const setStatus = (message, state = "") => { const element = status(); if (element) { element.textContent = message; element.dataset.state = state; } };
  const toMinutes = value => { const parsed = Number.parseInt(value, 10); return Number.isFinite(parsed) && parsed >= 0 ? parsed : null; };

  window.initParkPlannerMap = () => {
    map = new google.maps.Map(document.getElementById("map"), { center: { lat: config.latitude, lng: config.longitude }, zoom: 16, mapTypeControl: false, fullscreenControl: true, streetViewControl: false, clickableIcons: false });
    infoWindow = new google.maps.InfoWindow();
    markers = rides.map(ride => {
      const marker = new google.maps.Marker({ map, position: { lat: ride.lat, lng: ride.lng }, title: ride.name, label: { text: "...", color: "#17324d", fontWeight: "700", fontSize: "11px" } });
      marker.addListener("click", () => showRide(ride, marker));
      return { ride, marker };
    });
    updateMarkers();
  };

  function rideDataFor(ride) {
    const key = normalize(ride.name);
    return liveWaits.get(key) || liveWaits.get(aliases[key]) || null;
  }
  function waitFor(ride) { const data = rideDataFor(ride); return data && data.isOpen ? data.wait : null; }
  function isClosed(ride) { return rideDataFor(ride)?.isOpen === false; }
  function updateMarkers() { markers.forEach(({ ride, marker }) => { const wait = waitFor(ride); marker.setLabel({ text: isClosed(ride) ? "x" : wait === null ? "?" : `${wait}m`, color: "#17324d", fontWeight: "700", fontSize: "11px" }); }); }
  function matchesFilters(ride) { return activeFilters.size === 0 || ride.categories.some(category => activeFilters.has(category)); }
  function applyFilters() { markers.forEach(({ ride, marker }) => marker.setVisible(matchesFilters(ride))); document.getElementById("clear-filters").hidden = activeFilters.size === 0; if (userPosition) recommend(); }

  async function loadWaits() {
    const refresh = document.getElementById("refresh-waits");
    if (refresh) refresh.disabled = true;
    setStatus("Refreshing live wait times...");
    try {
      const controller = new AbortController(); const timeout = setTimeout(() => controller.abort(), 12000);
      const response = await fetch(config.waitEndpoint, { signal: controller.signal, headers: { Accept: "application/json" } }); clearTimeout(timeout);
      if (!response.ok) throw new Error("unavailable");
      const data = await response.json();
      liveWaits = new Map((data.lands || []).flatMap(land => land.rides || []).filter(ride => ride.name).map(ride => [normalize(ride.name), { wait: toMinutes(ride.wait_time), isOpen: ride.is_open !== false }]));
      updateMarkers();
      const openRides = [...liveWaits.values()].filter(ride => ride.isOpen).length;
      setStatus(openRides ? `Live wait times updated ${new Date().toLocaleTimeString([], { hour: "numeric", minute: "2-digit" })}.` : `${config.parkName} attractions are currently reported closed.`, openRides ? "success" : "");
      if (userPosition) recommend();
    } catch (_) { setStatus("Live wait times are unavailable right now. You can still view ride locations.", "error"); updateMarkers(); }
    finally { if (refresh) refresh.disabled = false; }
  }

  function shareLocation() {
    const button = document.getElementById("share-location");
    if (!navigator.geolocation) return setStatus("This browser does not support location sharing.", "error");
    button.disabled = true; setStatus("Waiting for your location permission...");
    navigator.geolocation.getCurrentPosition(position => {
      userPosition = { lat: position.coords.latitude, lng: position.coords.longitude };
      if (userMarker) userMarker.setMap(null);
      userMarker = new google.maps.Marker({ map, position: userPosition, title: "Your location", icon: { path: google.maps.SymbolPath.CIRCLE, scale: 9, fillColor: "#0b72e7", fillOpacity: 1, strokeColor: "#fff", strokeWeight: 3 } });
      map.panTo(userPosition); const state = document.getElementById("location-state"); state.textContent = "Location on"; state.dataset.active = "true"; button.textContent = "Update my location"; button.disabled = false; recommend();
    }, error => { setStatus(error.code === error.PERMISSION_DENIED ? "Location access was denied. Allow it in your browser settings to get nearby recommendations." : "We could not determine your location. Try again while you are at the park.", "error"); button.disabled = false; }, { enableHighAccuracy: true, timeout: 10000, maximumAge: 30000 });
  }

  function recommend() {
    const pool = markers.filter(({ ride }) => !completedRideIds.has(ride.pageId) && matchesFilters(ride) && (plannedRideIds.size === 0 || plannedRideIds.has(ride.pageId)));
    const candidates = pool.map(({ ride }) => { const wait = waitFor(ride); return wait === null ? null : { ...ride, wait, distance: distance(userPosition, ride) }; }).filter(Boolean);
    const list = document.getElementById("recommendation-list"), empty = document.getElementById("recommendation-empty");
    if (!candidates.length) { list.hidden = true; empty.hidden = false; empty.textContent = activeFilters.size ? "No matching rides have live wait times right now. Try another filter or refresh." : "Location found, but live wait data is unavailable. Refresh waits and try again."; return; }
    const maxWait = Math.max(...candidates.map(ride => ride.wait), 1), maxDistance = Math.max(...candidates.map(ride => ride.distance), 1);
    candidates.forEach(ride => ride.score = (ride.wait / maxWait) * .65 + (ride.distance / maxDistance) * .35);
    empty.hidden = true; list.hidden = false; list.replaceChildren(...candidates.sort((a, b) => a.score - b.score).slice(0, 3).map((ride, index) => recommendationNode(ride, index)));
    setStatus("Recommendations use the latest wait times and your current distance from each ride.", "success");
  }
  function recommendationNode(ride, index) { const node = document.createElement("li"); node.className = "planner-rec"; node.innerHTML = `<span class="planner-rank">${index + 1}</span><div><a href="/page/${encodeURIComponent(ride.pageId)}">${escapeHtml(ride.name)}</a><div class="planner-metrics"><span class="planner-metric">${ride.wait} min wait</span><span class="planner-metric">${formatDistance(ride.distance)} away</span></div><div class="planner-rec-actions"><a class="planner-navigate" href="${navigationUrl(ride)}" target="_blank" rel="noopener">Navigate</a><button type="button" class="planner-done">Mark done</button></div></div>`; node.querySelector(".planner-done").addEventListener("click", event => { event.stopPropagation(); markDone(ride); }); node.addEventListener("click", event => { if (event.target.closest("a") || event.target.closest("button")) return; const found = markers.find(entry => entry.ride.name === ride.name); if (found) { map.panTo(found.marker.getPosition()); map.setZoom(18); showRide(found.ride, found.marker); } }); return node; }
  function showRide(ride, marker) { const wait = waitFor(ride), rideDistance = userPosition ? distance(userPosition, ride) : null, waitText = isClosed(ride) ? "<b>Currently closed</b>" : wait === null ? "Wait unavailable" : `Current wait: <b>${wait} min</b>`; infoWindow.setContent(`<div style="padding:.35rem .2rem;font-family:Karla,Arial,sans-serif;min-width:180px"><strong style="font-size:1rem">${escapeHtml(ride.name)}</strong><p style="margin:.45rem 0">${waitText}</p>${rideDistance === null ? "" : `<p style="margin:0">About ${formatDistance(rideDistance)} away</p>`}<a style="display:inline-block;margin-top:.7rem;color:#174577;font-weight:700" href="${navigationUrl(ride)}" target="_blank" rel="noopener">Open walking directions</a></div>`); infoWindow.open({ map, anchor: marker }); }
  function navigationUrl(ride) { return `https://www.google.com/maps/dir/?api=1&destination=${encodeURIComponent(`${ride.lat},${ride.lng}`)}&travelmode=walking`; }
  function markDone(ride) { if (!dayPlan) dayPlan = { parkId: config.parkId, createdAt: new Date().toISOString(), rides: [] }; let savedRide = dayPlan.rides.find(item => item.id === ride.pageId); if (!savedRide) { savedRide = { id: ride.pageId, name: ride.name, done: false }; dayPlan.rides.push(savedRide); } savedRide.done = true; plannedRideIds.delete(ride.pageId); completedRideIds.add(ride.pageId); localStorage.setItem(planKey, JSON.stringify(dayPlan)); renderDayPlanSummary(); recommend(); setStatus(`${ride.name} marked complete. Great job!`, "success"); }
  function renderDayPlanSummary() { const summary = document.getElementById("my-day-summary"); if (!summary) return; const completed = completedRideIds.size, remaining = plannedRideIds.size; if (!dayPlan?.rides?.length) { summary.hidden = true; return; } summary.hidden = false; document.getElementById("my-day-count").textContent = `${remaining} ride${remaining === 1 ? "" : "s"} remaining${completed ? ` · ${completed} done` : ""}.`; document.getElementById("edit-my-day").href = `/plan?park=${encodeURIComponent(config.parkId)}`; }
  function distance(a, b) { const radians = degrees => degrees * Math.PI / 180, dLat = radians(b.lat - a.lat), dLng = radians(b.lng - a.lng); return 2 * 6371000 * Math.asin(Math.sqrt(Math.sin(dLat / 2) ** 2 + Math.cos(radians(a.lat)) * Math.cos(radians(b.lat)) * Math.sin(dLng / 2) ** 2)); }
  function formatDistance(meters) { return meters < 1000 ? `${Math.round(meters)} m` : `${(meters / 1000).toFixed(1)} km`; }
  function escapeHtml(value) { const element = document.createElement("div"); element.textContent = value; return element.innerHTML; }
  document.addEventListener("DOMContentLoaded", () => { renderDayPlanSummary(); document.getElementById("share-location")?.addEventListener("click", shareLocation); document.getElementById("refresh-waits")?.addEventListener("click", loadWaits); document.querySelectorAll(".planner-filter input").forEach(input => input.addEventListener("change", event => { const filter = event.currentTarget.value; event.currentTarget.checked ? activeFilters.add(filter) : activeFilters.delete(filter); applyFilters(); })); document.getElementById("clear-filters")?.addEventListener("click", () => { activeFilters.clear(); document.querySelectorAll(".planner-filter input").forEach(input => input.checked = false); applyFilters(); }); loadWaits(); });
})();
