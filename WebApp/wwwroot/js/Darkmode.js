/*
 * Manages the application’s light/dark theme:
 * - On load, determines the user’s preferred theme by checking (in order):
 *   1. Server-stored preference (if authenticated)
 *   2. LocalStorage
 *   3. System preference (prefers-color-scheme)
 * - Applies the chosen theme by toggling the "dark" class on <html>.
 * - Keeps a checkbox in sync for manual toggling.
 * - Persists user changes to LocalStorage and, if authenticated, to the server.
 */

document.addEventListener('DOMContentLoaded', () => {
    const darkToggle = document.getElementById('darkmode-switch');

//Applies the given theme by adding / removing the "dark" class on<html>
//and setting the state of the toggle checkbox.

    const applyTheme = theme => {
        if (theme === 'dark') {
            document.documentElement.classList.add('dark');
            if (darkToggle) darkToggle.checked = true;
        } else {
            document.documentElement.classList.remove('dark');
            if (darkToggle) darkToggle.checked = false;
        }
    };

    // Saves the theme choice in localStorage for future visits.
    // The code is my own , but got coached by ChatGPT
    const saveLocal = theme => localStorage.setItem('darkmode', theme);

    // Saves the theme choice on the server if the user is authenticated.
    const saveServer = async theme => {
        try {
            await fetch('/api/user/theme', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ theme })
            });
        } catch (err) {
            console.error('Kunde inte spara tema på servern:', err);
        }
    };

/**
 * Initializes the theme on page load:
 * 1. If authenticated, try to fetch server preference
 * 2. Fallback to localStorage
 * 3. Fallback to system preference
 * Then applies the resolved theme.
 * The code is my own , but got coached by ChatGPT
 */
    const init = async () => {
        let theme = null;

        if (window.appConfig.isAuthenticated) {
            try {
                const res = await fetch('/api/user/theme');
                if (res.ok) {
                    const data = await res.json();
                    theme = data.theme;
                }
            } catch (err) {
                console.warn('Fel vid hämtning av server-tema, fallback till lokal/system', err);
            }
        }

        // If server didn't provide a theme, check localStorage
        if (!theme) {
            theme = localStorage.getItem('darkmode');
        }

        // If still no theme, use system preference
        if (!theme) {
            const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
            theme = prefersDark ? 'dark' : 'light';
        }

        applyTheme(theme);
    };

    init();

    // When the user toggles the switch, update theme and persist choice
    if (darkToggle) {
        darkToggle.addEventListener('change', () => {
            const newTheme = darkToggle.checked ? 'dark' : 'light';
            applyTheme(newTheme);
            saveLocal(newTheme);
            if (window.appConfig.isAuthenticated) {
                saveServer(newTheme);
            }
        });
    }
});

