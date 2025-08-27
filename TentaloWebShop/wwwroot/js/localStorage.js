window.localStore = {
    get: (key) => {
        const v = localStorage.getItem(key);
        try { return JSON.parse(v); } catch { return v; }
    },
    set: (key, value) => localStorage.setItem(key, JSON.stringify(value)),
    remove: (key) => localStorage.removeItem(key)
}