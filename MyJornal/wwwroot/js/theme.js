// Unified theme setter: sets attribute AND persists preference to localStorage.
// Call: setTheme("dark") or setTheme("light")
window.setTheme = (theme) => {
    try {
        const t = (theme === "dark") ? "dark" : "light";
        // persist so on next page load the preference is re-applied
        try { localStorage.setItem('theme', t); } catch (e) { /* ignore storage errors */ }
        document.documentElement.setAttribute('data-theme', t);
    } catch (e) {
        // fail silently but log for debugging
        if (console && console.warn) console.warn("setTheme failed", e);
    }
};