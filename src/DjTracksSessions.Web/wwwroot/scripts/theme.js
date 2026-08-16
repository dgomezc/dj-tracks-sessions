window.djTracksSessionsTheme = {
    init() {
        const storedTheme = window.localStorage.getItem("dj-tracks-sessions.theme");
        const prefersDark = window.matchMedia("(prefers-color-scheme: dark)").matches;
        const isDark = storedTheme ? storedTheme === "dark" : prefersDark;

        document.documentElement.classList.toggle("dark", isDark);
        window.localStorage.setItem("dj-tracks-sessions.theme", isDark ? "dark" : "light");

        return isDark;
    },

    isDark() {
        return document.documentElement.classList.contains("dark");
    },

    setDark(isDark) {
        document.documentElement.classList.toggle("dark", isDark);
        window.localStorage.setItem("dj-tracks-sessions.theme", isDark ? "dark" : "light");
    }
};
