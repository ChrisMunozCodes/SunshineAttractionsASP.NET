(() => {
  "use strict";
  const config = window.parkCircleConfig;
  if (!config) return;

  let map, infoWindow, watchId = null, sharing = false, lastSentAt = 0;
  const markers = new Map();
  const endpoint = `/circle/api/${encodeURIComponent(config.id)}`;
  const status = () => document.getElementById("circle-status");
  const setStatus = (message, state = "") => { const node = status(); if (node) { node.textContent = message; node.dataset.state = state; } };
  const initials = name => String(name || "?").trim().split(/\s+/).slice(0, 2).map(word => word[0]).join("").toUpperCase();
  const timeAgo = value => { if (!value) return "Location off"; const seconds = Math.max(0, Math.round((Date.now() - new Date(value).getTime()) / 1000)); return seconds < 15 ? "Updated just now" : seconds < 60 ? `Updated ${seconds}s ago` : `Updated ${Math.round(seconds / 60)}m ago`; };

  window.initCircleMap = () => {
    map = new google.maps.Map(document.getElementById("circle-map"), { center: { lat: Number(config.park.latitude), lng: Number(config.park.longitude) }, zoom: 16, mapTypeControl: false, fullscreenControl: true, streetViewControl: false, clickableIcons: false });
    infoWindow = new google.maps.InfoWindow();
    refreshCircle();
  };

  function renderMembers(members) {
    const list = document.getElementById("circle-member-list");
    list.replaceChildren(...members.map(member => { const item = document.createElement("li"); item.className = "circle-member"; item.innerHTML = `<span class="circle-member-avatar">${initials(member.name)}</span><div><strong>${escapeHtml(member.name)}</strong><span>${member.isFresh ? timeAgo(member.updatedAt) : member.isSharing ? "Location paused — update needed" : "Location off"}</span></div><i data-sharing="${member.isFresh}"></i>`; return item; }));
  }

  function renderMarkers(members) {
    if (!map) return;
    const active = members.filter(member => member.isFresh && Number.isFinite(member.latitude) && Number.isFinite(member.longitude));
    const activeIds = new Set(active.map(member => member.id));
    markers.forEach((marker, id) => { if (!activeIds.has(id)) { marker.setMap(null); markers.delete(id); } });
    active.forEach(member => {
      let marker = markers.get(member.id);
      if (!marker) { marker = new google.maps.Marker({ map, title: member.name, label: { text: initials(member.name), color: "#17324d", fontWeight: "700", fontSize: "10px" } }); marker.addListener("click", () => infoWindow.open({ map, anchor: marker })); markers.set(member.id, marker); }
      marker.setPosition({ lat: Number(member.latitude), lng: Number(member.longitude) });
      marker.setTitle(member.name);
      marker.__circleMember = member;
      google.maps.event.clearListeners(marker, "click");
      marker.addListener("click", () => { const current = marker.__circleMember; infoWindow.setContent(`<div style="font-family:Karla,Arial,sans-serif;padding:.25rem"><strong>${escapeHtml(current.name)}</strong><p style="margin:.35rem 0 0">${timeAgo(current.updatedAt)}</p><a style="display:inline-block;margin-top:.55rem;color:#174577;font-weight:700" target="_blank" rel="noopener" href="https://www.google.com/maps/dir/?api=1&destination=${encodeURIComponent(`${current.latitude},${current.longitude}`)}&travelmode=walking">Walk to them</a></div>`); infoWindow.open({ map, anchor: marker }); });
    });
  }

  async function refreshCircle() {
    try { const response = await fetch(endpoint, { headers: { Accept: "application/json" } }); if (!response.ok) throw new Error(); const circle = await response.json(); renderMembers(circle.members || []); renderMarkers(circle.members || []); if (!sharing) setStatus("Location is off for you. You can still see members who choose to share."); } catch (_) { setStatus("We could not refresh this Circle. Check your connection and try again.", "error"); }
  }

  async function sendLocation(position) {
    const now = Date.now();
    if (now - lastSentAt < 15000) return;
    lastSentAt = now;
    const response = await fetch(`${endpoint}/location`, { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ isSharing: true, latitude: position.coords.latitude, longitude: position.coords.longitude }) });
    if (!response.ok) throw new Error();
    sharing = true;
    document.getElementById("circle-share-location").hidden = true;
    document.getElementById("circle-stop-location").hidden = false;
    setStatus("You are sharing your latest location while this page stays open.", "success");
    refreshCircle();
  }

  function startSharing() {
    if (!navigator.geolocation) return setStatus("This browser does not support location sharing.", "error");
    setStatus("Waiting for your location permission…");
    navigator.geolocation.getCurrentPosition(async position => {
      try { lastSentAt = 0; await sendLocation(position); watchId = navigator.geolocation.watchPosition(next => sendLocation(next).catch(() => setStatus("We could not update your location. Try sharing again.", "error")), () => setStatus("Location sharing stopped. Check your browser permission.", "error"), { enableHighAccuracy: true, maximumAge: 15000, timeout: 15000 }); } catch (_) { setStatus("We could not start location sharing. Try again in a moment.", "error"); }
    }, error => setStatus(error.code === error.PERMISSION_DENIED ? "Location permission was denied. You can turn it on in your browser settings." : "We could not determine your location. Try again while you are at the park.", "error"), { enableHighAccuracy: true, maximumAge: 15000, timeout: 15000 });
  }

  async function stopSharing() {
    if (watchId !== null) { navigator.geolocation.clearWatch(watchId); watchId = null; }
    try { await fetch(`${endpoint}/location`, { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ isSharing: false }) }); } catch (_) { }
    sharing = false;
    document.getElementById("circle-share-location").hidden = false;
    document.getElementById("circle-stop-location").hidden = true;
    setStatus("Location sharing is off. Your last location was removed from this Circle.", "success");
    refreshCircle();
  }

  async function copyInvite() {
    const url = config.inviteUrl;
    try { if (navigator.share) await navigator.share({ title: "Join my Sunshine Park Circle", text: "Join our private park group.", url }); else if (navigator.clipboard?.writeText) await navigator.clipboard.writeText(url); else window.prompt("Copy this invite link:", url); setStatus("Invite ready to send. Each person must sign in before joining.", "success"); } catch (error) { if (error?.name !== "AbortError") window.prompt("Copy this invite link:", url); }
  }

  async function leaveCircle() { if (!window.confirm("Leave this Circle? Your shared location will be removed.")) return; await stopSharing(); try { await fetch(`${endpoint}/leave`, { method: "POST" }); } finally { window.location.assign("/circle"); } }
  function escapeHtml(value) { const element = document.createElement("div"); element.textContent = value || ""; return element.innerHTML; }

  document.addEventListener("DOMContentLoaded", () => {
    document.getElementById("circle-share-location")?.addEventListener("click", startSharing);
    document.getElementById("circle-stop-location")?.addEventListener("click", stopSharing);
    document.getElementById("circle-copy-invite")?.addEventListener("click", copyInvite);
    document.getElementById("circle-leave")?.addEventListener("click", leaveCircle);
    refreshCircle();
    window.setInterval(refreshCircle, 15000);
    window.addEventListener("beforeunload", () => { if (sharing) navigator.sendBeacon(`${endpoint}/location`, new Blob([JSON.stringify({ isSharing: false })], { type: "application/json" })); });
  });
})();
