/*
 * Manages the cookie consent workflow:
 * - On page load, checks for an existing "cookieConsent" cookie and opens the consent modal if missing.
 * - Provides functions to open/close the modal, read/write cookies, and send consent choices to the server.
 * - Supports “Accept All” and “Accept Selected” flows with persistence for 90 or 365 days.
 * - This Code is based on Hans code in the Video about Cookie Consent
 */

document.addEventListener("DOMContentLoaded", function () {
    // Show the cookie consent modal if no consent cookie is present
    if (!getCookie("cookieConsent")) {
        openCookieModal()
    }
})

// Function to open the cookie consent modal and pre-populate it with existing consent values
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

// Function to get a cookie by name
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

// Function to close the cookie consent modal
function closeCookieModal() {
    const modal = document.getElementById("cookieModal");
    if (modal) modal.style.display = "none";
}

// Function to set a cookie with a specified name, value, and expiration in days
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

//  Accepts all categories (essential, functional, marketing),
// Persists the choice for 90 days, sends it to the server, and closes the modal.

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

// Accepts only the categories selected by the user in the form,
// Persists the choice for 365 days, sends it to the server, and closes the modal.
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

// Function to send the consent data to the server
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

// Allow opening the consent modal again via a settings link
document.addEventListener("DOMContentLoaded", function () {
    const settingsLink = document.getElementById("cookieSettingsLink");
    if (settingsLink) {
        settingsLink.addEventListener("click", function (e) {
            e.preventDefault();
            openCookieModal();
        });
    }
});