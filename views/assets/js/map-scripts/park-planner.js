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
  const watchKey = `sunshine-ride-watches:${config.parkId}`;
  let watchedRideIds;
  try { watchedRideIds = new Set(JSON.parse(localStorage.getItem(watchKey) || "[]")); } catch (_) { watchedRideIds = new Set(); }
  const watchedRideStates = new Map();
  let hasLoadedWaits = false;
  const skippedRideIds = new Set();
  let latestRecommendations = [];
  let parkModeOpen = false;
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
    updateMarkers(); renderPlanB(); renderRideWatches();
  };

  function isReportedOpen(value) { return value === undefined || value === null || value === true || value === 1 || value === "true"; }
  function rideDataFor(ride) {
    const key = normalize(ride.name);
    return liveWaits.get(key) || liveWaits.get(aliases[key]) || null;
  }
  function waitFor(ride) { const data = rideDataFor(ride); return data && data.isOpen ? data.wait : null; }
  function isClosed(ride) { return rideDataFor(ride)?.isOpen === false; }
  function updateMarkers() { markers.forEach(({ ride, marker }) => { const wait = waitFor(ride); marker.setLabel({ text: isClosed(ride) ? "Closed" : wait === null ? "?" : `${wait}m`, color: "#17324d", fontWeight: "700", fontSize: "11px" }); }); }
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
      liveWaits = new Map((data.lands || []).flatMap(land => land.rides || []).filter(ride => ride.name).map(ride => [normalize(ride.name), { wait: toMinutes(ride.wait_time), isOpen: isReportedOpen(ride.is_open) }]));
      updateMarkers();
      updateRideWatches();
      renderPlanB();
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
      map.panTo(userPosition); const state = document.getElementById("location-state"); state.textContent = "Location on"; state.dataset.active = "true"; button.textContent = "Update my location"; button.disabled = false; recommend(); renderPlanB();
    }, error => { setStatus(error.code === error.PERMISSION_DENIED ? "Location access was denied. Allow it in your browser settings to get nearby recommendations." : "We could not determine your location. Try again while you are at the park.", "error"); button.disabled = false; }, { enableHighAccuracy: true, timeout: 10000, maximumAge: 30000 });
  }

  function recommend() {
    const pool = markers.filter(({ ride }) => !completedRideIds.has(ride.pageId) && !skippedRideIds.has(ride.pageId) && matchesFilters(ride) && (plannedRideIds.size === 0 || plannedRideIds.has(ride.pageId)));
    const candidates = pool.map(({ ride }) => { const wait = waitFor(ride); return wait === null ? null : { ...ride, wait, distance: distance(userPosition, ride) }; }).filter(Boolean);
    const list = document.getElementById("recommendation-list"), empty = document.getElementById("recommendation-empty");
    if (!candidates.length) { latestRecommendations = []; renderParkMode(); list.hidden = true; empty.hidden = false; empty.textContent = activeFilters.size ? "No matching rides have live wait times right now. Try another filter or refresh." : "Location found, but live wait data is unavailable. Refresh waits and try again."; return; }
    const maxWait = Math.max(...candidates.map(ride => ride.wait), 1), maxDistance = Math.max(...candidates.map(ride => ride.distance), 1);
    candidates.forEach(ride => ride.score = (ride.wait / maxWait) * .65 + (ride.distance / maxDistance) * .35);
    latestRecommendations = candidates.sort((a, b) => a.score - b.score); renderParkMode(); empty.hidden = true; list.hidden = false; list.replaceChildren(...latestRecommendations.slice(0, 3).map((ride, index) => recommendationNode(ride, index)));
    setStatus("Recommendations use the latest wait times and your current distance from each ride.", "success");
  }
  function openParkMode() { parkModeOpen = true; document.getElementById("park-mode").hidden = false; renderParkMode(); if (!userPosition) shareLocation(); else recommend(); }
  function closeParkMode() { parkModeOpen = false; document.getElementById("park-mode").hidden = true; }
  function skipCurrentRide() { const ride = latestRecommendations[0]; if (!ride) return; skippedRideIds.add(ride.pageId); latestRecommendations = []; setStatus(`${ride.name} is skipped for now. Finding another option...`, "success"); recommend(); }
  function renderParkMode() { if (!parkModeOpen) return; const location = document.getElementById("park-mode-location"), navigate = document.getElementById("park-mode-navigate"), done = document.getElementById("park-mode-done"), skip = document.getElementById("park-mode-skip"), metrics = document.getElementById("park-mode-metrics"), title = document.getElementById("park-mode-title"), reason = document.getElementById("park-mode-reason"), progress = document.getElementById("park-mode-progress"), overline = document.getElementById("park-mode-overline"); if (!userPosition) { overline.textContent = "ONE QUICK STEP"; title.textContent = "Let Sunny see where you are"; reason.textContent = "Share your location and Sunny will choose the best next ride based on walking time and live waits."; location.hidden = false; navigate.hidden = done.hidden = skip.hidden = metrics.hidden = true; progress.textContent = "Your location stays on this device."; return; } if (!latestRecommendations.length) { overline.textContent = "RECALCULATING"; title.textContent = "Looking for an open next step"; reason.textContent = "Sunny is waiting for live wait data or another ride choice."; location.hidden = true; navigate.hidden = done.hidden = skip.hidden = metrics.hidden = true; progress.textContent = "Try refreshing waits or adjust your ride filters."; return; } const ride = latestRecommendations[0], remaining = plannedRideIds.size; overline.textContent = "YOUR NEXT MOVE"; title.textContent = ride.name; reason.textContent = `Sunny picked this because it has a ${ride.wait}-minute wait and is only ${formatDistance(ride.distance)} away.`; document.getElementById("park-mode-wait").textContent = `${ride.wait} min wait`; document.getElementById("park-mode-distance").textContent = `${formatDistance(ride.distance)} walk`; navigate.href = navigationUrl(ride); location.hidden = true; navigate.hidden = done.hidden = skip.hidden = metrics.hidden = false; progress.textContent = remaining ? `${remaining} planned ride${remaining === 1 ? "" : "s"} remaining. Sunny will adapt if a ride closes or you skip one.` : "Live route mode is on. Sunny will keep choosing the best open ride nearby."; done.onclick = () => markDone(ride); skip.onclick = skipCurrentRide; }
  function recommendationNode(ride, index) { const node = document.createElement("li"); node.className = "planner-rec"; node.innerHTML = `<span class="planner-rank">${index + 1}</span><div><a href="/page/${encodeURIComponent(ride.pageId)}">${escapeHtml(ride.name)}</a><div class="planner-metrics"><span class="planner-metric">${ride.wait} min wait</span><span class="planner-metric">${formatDistance(ride.distance)} away</span></div><div class="planner-rec-actions"><a class="planner-navigate" href="${navigationUrl(ride)}" target="_blank" rel="noopener">Navigate</a><button type="button" class="planner-done">Mark done</button></div></div>`; node.querySelector(".planner-done").addEventListener("click", event => { event.stopPropagation(); markDone(ride); }); node.addEventListener("click", event => { if (event.target.closest("a") || event.target.closest("button")) return; const found = markers.find(entry => entry.ride.name === ride.name); if (found) { map.panTo(found.marker.getPosition()); map.setZoom(18); showRide(found.ride, found.marker); } }); return node; }
  function showRide(ride, marker) { const wait = waitFor(ride), rideDistance = userPosition ? distance(userPosition, ride) : null, closed = isClosed(ride), waitText = closed ? "<b>Currently closed</b>" : wait === null ? "Wait unavailable" : `Current wait: <b>${wait} min</b>`; infoWindow.setContent(`<div style="padding:.35rem .2rem;font-family:Karla,Arial,sans-serif;min-width:180px"><strong style="font-size:1rem">${escapeHtml(ride.name)}</strong><p style="margin:.45rem 0">${waitText}</p>${rideDistance === null ? "" : `<p style="margin:0">About ${formatDistance(rideDistance)} away</p>`}${closed ? `<button id="watch-ride" type="button" style="margin-top:.7rem;border:0;border-radius:.35rem;background:#174577;color:#fff;padding:.45rem .55rem;font-weight:700;cursor:pointer">${watchedRideIds.has(ride.pageId) ? "Watching for reopening" : "Watch for reopening"}</button>` : ""}<a style="display:inline-block;margin-top:.7rem;margin-left:${closed ? ".45rem" : "0"};color:#174577;font-weight:700" href="${navigationUrl(ride)}" target="_blank" rel="noopener">Directions</a></div>`); google.maps.event.addListenerOnce(infoWindow, "domready", () => document.getElementById("watch-ride")?.addEventListener("click", () => watchRide(ride))); infoWindow.open({ map, anchor: marker }); }
  function navigationUrl(ride) { return `https://www.google.com/maps/dir/?api=1&destination=${encodeURIComponent(`${ride.lat},${ride.lng}`)}&travelmode=walking`; }
  function markDone(ride) { if (!dayPlan) dayPlan = { parkId: config.parkId, createdAt: new Date().toISOString(), rides: [] }; let savedRide = dayPlan.rides.find(item => item.id === ride.pageId); if (!savedRide) { savedRide = { id: ride.pageId, name: ride.name, done: false }; dayPlan.rides.push(savedRide); } savedRide.done = true; plannedRideIds.delete(ride.pageId); completedRideIds.add(ride.pageId); saveDayPlan(); renderDayPlanSummary(); renderPlanB(); recommend(); setStatus(`${ride.name} marked complete. Great job!`, "success"); }
  function renderDayPlanSummary() { const summary = document.getElementById("my-day-summary"); if (!summary) return; const completed = completedRideIds.size, remaining = plannedRideIds.size; if (!dayPlan?.rides?.length) { summary.hidden = true; return; } summary.hidden = false; document.getElementById("my-day-count").textContent = `${remaining} ride${remaining === 1 ? "" : "s"} remaining${completed ? ` · ${completed} done` : ""}.`; document.getElementById("edit-my-day").href = `/plan?park=${encodeURIComponent(config.parkId)}`; }
  function saveDayPlan() { localStorage.setItem(planKey, JSON.stringify(dayPlan)); }
  function addToPlan(ride) { if (!dayPlan) dayPlan = { parkId: config.parkId, createdAt: new Date().toISOString(), rides: [] }; if (!dayPlan.rides.some(item => item.id === ride.pageId)) dayPlan.rides.push({ id: ride.pageId, name: ride.name, done: false }); plannedRideIds.add(ride.pageId); completedRideIds.delete(ride.pageId); saveDayPlan(); renderDayPlanSummary(); renderPlanB(); if (userPosition) recommend(); setStatus(`${ride.name} was added to your day plan.`, "success"); }
  function watchRide(ride) { watchedRideIds.add(ride.pageId); watchedRideStates.set(ride.pageId, false); localStorage.setItem(watchKey, JSON.stringify([...watchedRideIds])); renderRideWatches(); setStatus(`Watching ${ride.name}. We'll tell you here when it reopens.`, "success"); infoWindow?.close(); }
  function removeWatch(id) { watchedRideIds.delete(id); watchedRideStates.delete(id); localStorage.setItem(watchKey, JSON.stringify([...watchedRideIds])); renderRideWatches(); }
  function updateRideWatches() { const reopened = []; watchedRideIds.forEach(id => { const ride = rides.find(item => item.pageId === id), isOpen = ride ? rideDataFor(ride)?.isOpen === true : false, previous = watchedRideStates.get(id); if (hasLoadedWaits && previous === false && isOpen) reopened.push(ride); watchedRideStates.set(id, isOpen); }); hasLoadedWaits = true; renderRideWatches(); if (reopened.length) { const names = reopened.map(ride => ride.name).join(", "); setStatus(`${names} ${reopened.length === 1 ? "is" : "are"} open again!`, "success"); if (window.Notification?.permission === "granted") new Notification("Ride is open again", { body: `${names} is now reported open.` }); } }
  function renderRideWatches() { const list = document.getElementById("ride-watch-list"); if (!list) return; if (!watchedRideIds.size) { list.innerHTML = '<li class="planner-watch-empty">No rides are being watched yet.</li>'; return; } list.replaceChildren(...[...watchedRideIds].map(id => { const ride = rides.find(item => item.pageId === id); if (!ride) return document.createElement("li"); const item = document.createElement("li"), open = rideDataFor(ride)?.isOpen === true; item.className = "planner-watch-item"; item.innerHTML = `<div><strong>${escapeHtml(ride.name)}</strong><span>${open ? "Open now" : "Watching for reopening"}</span></div><button type="button">Remove</button>`; item.querySelector("button").addEventListener("click", () => removeWatch(id)); return item; })); }
  function renderPlanB() { const panel = document.getElementById("plan-b"), list = document.getElementById("plan-b-list"); if (!panel || !list) return; const closed = rides.filter(ride => plannedRideIds.has(ride.pageId) && isClosed(ride)); if (!closed.length) { panel.hidden = true; return; } panel.hidden = false; document.getElementById("plan-b-heading").textContent = `${closed.map(ride => ride.name).join(", ")} ${closed.length === 1 ? "is" : "are"} currently closed`; const message = document.getElementById("plan-b-message"); if (!userPosition) { message.textContent = "Use your location and Sunny will find the best nearby replacement."; list.innerHTML = ""; return; } const alternatives = markers.filter(({ ride }) => !completedRideIds.has(ride.pageId) && !closed.some(item => item.pageId === ride.pageId) && !isClosed(ride)).map(({ ride }) => { const wait = waitFor(ride); return wait === null ? null : { ...ride, wait, distance: distance(userPosition, ride) }; }).filter(Boolean); if (!alternatives.length) { message.textContent = "No open alternatives have live wait times right now. Refresh in a moment."; list.innerHTML = ""; return; } const maxWait = Math.max(...alternatives.map(ride => ride.wait), 1), maxDistance = Math.max(...alternatives.map(ride => ride.distance), 1); alternatives.forEach(ride => ride.score = (ride.wait / maxWait) * .65 + (ride.distance / maxDistance) * .35); message.textContent = "Here are Sunny's closest low-wait swaps."; list.replaceChildren(...alternatives.sort((a, b) => a.score - b.score).slice(0, 2).map(ride => { const item = document.createElement("li"); item.className = "planner-plan-b-item"; item.innerHTML = `<div><strong>${escapeHtml(ride.name)}</strong><span>${ride.wait} min wait · ${formatDistance(ride.distance)} away</span></div><button type="button">Add to plan</button>`; item.querySelector("button").addEventListener("click", () => addToPlan(ride)); return item; })); }
  function distance(a, b) { const radians = degrees => degrees * Math.PI / 180, dLat = radians(b.lat - a.lat), dLng = radians(b.lng - a.lng); return 2 * 6371000 * Math.asin(Math.sqrt(Math.sin(dLat / 2) ** 2 + Math.cos(radians(a.lat)) * Math.cos(radians(b.lat)) * Math.sin(dLng / 2) ** 2)); }
  function formatDistance(meters) { return meters < 1000 ? `${Math.round(meters)} m` : `${(meters / 1000).toFixed(1)} km`; }
  function escapeHtml(value) { const element = document.createElement("div"); element.textContent = value; return element.innerHTML; }
  document.addEventListener("DOMContentLoaded", () => { renderDayPlanSummary(); renderRideWatches(); document.getElementById("start-park-mode")?.addEventListener("click", openParkMode); document.getElementById("exit-park-mode")?.addEventListener("click", closeParkMode); document.getElementById("park-mode-location")?.addEventListener("click", shareLocation); document.getElementById("share-location")?.addEventListener("click", shareLocation); document.getElementById("refresh-waits")?.addEventListener("click", loadWaits); document.querySelectorAll(".planner-filter input").forEach(input => input.addEventListener("change", event => { const filter = event.currentTarget.value; event.currentTarget.checked ? activeFilters.add(filter) : activeFilters.delete(filter); applyFilters(); })); document.getElementById("clear-filters")?.addEventListener("click", () => { activeFilters.clear(); document.querySelectorAll(".planner-filter input").forEach(input => input.checked = false); applyFilters(); }); loadWaits(); window.setInterval(loadWaits, 120000); });
})();
