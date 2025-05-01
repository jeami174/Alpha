document.addEventListener("DOMContentLoaded", function () {
    if (!getCookie("cookieConsent")) {
        openCookieModal()
    }
})

function openCookieModal() {
    const modal = document.getElementById("cookieModal");
    if (modal) modal.style.display = "flex";

    const prePopulatedConsent = getCookie("cookieConsent");
    if (!prePopulatedConsent) return

    try {
        const consent = JSON.parse(prePopulatedConsent);
        document.getElementById("cookieFunctional").checked = consent.functional;
        document.getElementById("cookieMarketing").checked = consent.marketing;
    }
    catch (error) {
        console.error("Error parsing cookie consent:", error);
    }
}

function getCookie(name) {
    const nameEQ = name + "=";
    const cookies = document.cookie.split(';');
    for (let cookie of cookies) {
        cookie = cookie.trim();
        if (cookie.indexOf(nameEQ) === 0) {
            return decodeURIComponent(cookie.substring(nameEQ.length))
        }
    }
    return null;
}

function closeCookieModal() {
    const modal = document.getElementById("cookieModal");
    if (modal) modal.style.display = "none";
}

function setCookie(name, value, days) {
    let expires = ""
    if (days) {
        const date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        expires = "; expires=" + date.toUTCString();
    }

    const encodedValue = encodeURIComponent(value || "");
    document.cookie = `${name}=${encodedValue}${expires}; path=/; SameSite=Lax`
}

async function acceptAll() {
    const consent = {
        essential: true,
        functional: true,
        marketing: true
    }

    setCookie("cookieConsent", JSON.stringify(consent), 90)
    await setConsent(consent)
    closeCookieModal()
}

async function acceptSelected() {
    const form = document.getElementById("cookieConsentForm");
    const formData = new FormData(form);

    const consent = {
        essential: true,
        functional: formData.get("functional") === "on",
        marketing: formData.get("marketing") === "on"
    }
    setCookie("cookieConsent", JSON.stringify(consent), 365)
    await setConsent(consent)
    closeCookieModal()
}

async function setConsent(consent) {
    try {
        const res = await fetch("/cookies/setcookies", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(consent)
        });
        if (!res.ok) {
            console.error("Failed to set cookie consent", await res.text());
        }
    }
    catch (error) {
        console.error("Error setting cookie consent:", error);
    }
}

document.addEventListener("DOMContentLoaded", function () {
    const settingsLink = document.getElementById("cookieSettingsLink");
    if (settingsLink) {
        settingsLink.addEventListener("click", function (e) {
            e.preventDefault();
            openCookieModal();
        });
    }
});