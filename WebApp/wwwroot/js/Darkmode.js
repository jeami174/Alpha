document.addEventListener('DOMContentLoaded', () => {
    const darkToggle = document.getElementById('darkmode-switch');

    const applyTheme = theme => {
        if (theme === 'dark') {
            document.documentElement.classList.add('dark');
            if (darkToggle) darkToggle.checked = true;
        } else {
            document.documentElement.classList.remove('dark');
            if (darkToggle) darkToggle.checked = false;
        }
    };

    const saveLocal = theme => localStorage.setItem('darkmode', theme);

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

        if (!theme) {
            theme = localStorage.getItem('darkmode');
        }

        if (!theme) {
            const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
            theme = prefersDark ? 'dark' : 'light';
        }

        applyTheme(theme);
    };

    init();

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
