// site.js - Global JavaScript
// Manages multiple core functionalities across the site:

// ----------------------------------------
// 1. Global Quill configurations
//  Quill editor setup for rich-text fields on create/edit forms.
// ----------------------------------------
const quillConfigs = [
    {
        editorId: 'quill-editor',
        toolbarId: 'quill-toolbar',
        hiddenInputId: 'FormData_Form_Description'
    },
    {
        editorId: 'quill-editor-edit',
        toolbarId: 'quill-toolbar-edit',
        hiddenInputId: 'FormData_Form_Description_Edit'
    }
];

// ----------------------------------------
// 2. Global click delegate for all [data-fetch-url] elements
// Delegated click handling for elements with [data - fetch - url] to load partial views via AJAX.
// ----------------------------------------
document.addEventListener('click', e => {
    const btn = e.target.closest('[data-fetch-url]');
    if (!btn) return;

    e.preventDefault();
    if (typeof closeAllDropdowns === 'function') {
        closeAllDropdowns();
    }

    const url = btn.getAttribute('data-fetch-url');
    const container = btn.getAttribute('data-container');
    if (!url || !container) {
        console.error('Saknar data-fetch-url eller data-container på', btn);
        return;
    }
    console.log('[DEBUG] loadPartialView →', url, container);
    loadPartialView(url, container);
});

// ----------------------------------------
// 3. Helper utilities:
// Retrieving the anti - forgery token
// Clearing validation error messages
// Asynchronously loading and processing image previews
// ----------------------------------------
function getRequestVerificationToken() {
    const t = document.querySelector('input[name="__RequestVerificationToken"]');
    return t ? t.value : '';
}

function clearErrorMessages(form) {
    form.querySelectorAll('[data-val="true"]').forEach(i => i.classList.remove('input-validation-error'));
    form.querySelectorAll('[data-valmsg-for]').forEach(s => {
        s.innerText = '';
        s.classList.remove('field-validation-error', 'extended-field-validation-error');
    });
    const msg = form.querySelector('.form-message');
    if (msg) { msg.innerText = ''; msg.style.display = 'none'; }
}

async function loadImage(file) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onerror = () => reject(new Error("Failed to load file."));
        reader.onload = e => {
            const img = new Image();
            img.onerror = () => reject(new Error("Failed to load image."));
            img.onload = () => resolve(img);
            img.src = e.target.result;
        };
        reader.readAsDataURL(file);
    });
}

async function processImage(file, imagePreview, previewer, previewSize = 150) {
    try {
        const img = await loadImage(file);
        const canvas = document.createElement('canvas');
        canvas.width = previewSize;
        canvas.height = previewSize;
        const ctx = canvas.getContext('2d');
        ctx.drawImage(img, 0, 0, previewSize, previewSize);
        imagePreview.src = canvas.toDataURL('image/jpeg');
        previewer.classList.add('selected');
    } catch (err) {
        console.error('Failed on image processing', err);
    }
}

function bindImagePreviewers() {
    document.querySelectorAll('.image-previewer').forEach(previewer => {
        const fileInput = previewer.querySelector('input[type="file"]');
        const imagePreview = previewer.querySelector('.image-preview');
        previewer.addEventListener('click', () => fileInput.click());
        fileInput.addEventListener('change', ({ target: { files } }) => {
            if (files[0]) processImage(files[0], imagePreview, previewer);
        });
    });
}

function bindCloseButtons() {
    document.querySelectorAll('[data-close="true"]').forEach(btn => {
        btn.addEventListener('click', () => {
            const modal = btn.closest('.modal');
            if (!modal) return;
            modal.style.display = 'none';
            modal.querySelectorAll('form').forEach(f => {
                f.reset();
                clearErrorMessages(f);
                f.querySelectorAll('input[type="file"]').forEach(i => i.value = '');
                const prev = f.querySelector('.image-preview'); if (prev) prev.src = '';
                const prevw = f.querySelector('.image-previewer'); if (prevw) prevw.classList.remove('selected');
            });
        });
    });
}

function bindTogglePassword() {
    const t1 = document.getElementById('togglePassword');
    if (t1) t1.addEventListener('click', () => {
        const p = document.getElementById('passwordField');
        if (p) p.type = p.type === 'password' ? 'text' : 'password';
    });
    const t2 = document.getElementById('toggleConfirmPassword');
    if (t2) t2.addEventListener('click', () => {
        const p = document.getElementById('confirmPasswordField');
        if (p) p.type = p.type === 'password' ? 'text' : 'password';
    });
}

function updateRelativeTimes() {
    const elements = document.querySelectorAll('.notification-time');
    const now = new Date();

    elements.forEach(el => {
        const createdUtc = new Date(el.getAttribute('data-created'));
        if (isNaN(createdUtc.getTime())) return;

        const createdLocal = new Date(createdUtc.getTime() - (createdUtc.getTimezoneOffset() * 60000));
        const diff = now - createdLocal;
        const diffSeconds = Math.floor(diff / 1000);
        const diffMinutes = Math.floor(diffSeconds / 60);
        const diffHours = Math.floor(diffMinutes / 60);
        const diffDays = Math.floor(diffHours / 24);
        const diffWeeks = Math.floor(diffDays / 7);

        let relativeTime = '';

        if (diffMinutes < 1) relativeTime = 'just now';
        else if (diffMinutes < 60) relativeTime = `${diffMinutes} min ago`;
        else if (diffHours < 24) relativeTime = `${diffHours} h ago`;
        else if (diffDays < 7) relativeTime = `${diffDays} d ago`;
        else relativeTime = `${diffWeeks} w ago`;

        el.textContent = relativeTime;
    });
}

function updateNotificationCount() {
    const notificationsList = document.querySelector('.notification-list');
    const count = notificationsList.querySelectorAll('.notification-item').length;

    let countDisplay = document.querySelector('.notification-count');
    if (!countDisplay) {
        const header = document.querySelector('.notifications-header');
        if (header) {
            countDisplay = document.createElement('div');
            countDisplay.className = 'notification-count';
            header.appendChild(countDisplay);
        }
    }

    if (countDisplay) {
        countDisplay.textContent = count;
        countDisplay.style.display = count > 0 ? 'inline-block' : 'none';
    }

    let dot = document.querySelector('.notification-dot');
    if (!dot) {
        const wrapper = document.querySelector('.notification-wrapper');
        if (wrapper) {
            dot = document.createElement('span');
            dot.className = 'notification-dot';
            wrapper.appendChild(dot);
        }
    }

    if (dot) {
        if (count > 0) dot.classList.add('active');
        else dot.classList.remove('active');
    }
}

// ----------------------------------------
// 4. Dynamic partial view loader that:
// Fetches HTML fragments
// Injects and executes embedded scripts
// Binds image previewers, form handlers, password toggles, dropdowns, and Quill editors
// ----------------------------------------
function loadPartialView(url, containerId) {
    if (typeof closeAllDropdowns === 'function') {
        closeAllDropdowns();
    }

    console.log('[DEBUG] loadPartialView called with', url, containerId);
    fetch(url)
        .then(res => {
            if (!res.ok) throw new Error('HTTP ' + res.status);
            return res.text();
        })
        .then(html => {
            const c = document.getElementById(containerId);
            if (!c) {
                console.error('#' + containerId + ' saknas');
                return;
            }
            c.innerHTML = html;

            Array.from(c.querySelectorAll('script')).forEach(old => {
                const ns = document.createElement('script');
                if (old.src) {
                    ns.src = old.src;
                    ns.async = false;
                } else {
                    ns.textContent = old.textContent;
                }
                document.body.appendChild(ns);
                old.remove();
            });

            const m = c.querySelector('.modal');
            if (m) m.style.display = 'flex';

            bindImagePreviewers();
            bindCloseButtons();
            bindFormSubmitHandlers();
            bindTogglePassword();
            setupDropdownToggles();

            quillConfigs.forEach(config => {
                const ed = document.getElementById(config.editorId);
                const tb = document.getElementById(config.toolbarId);
                const hi = document.getElementById(config.hiddenInputId);
                if (ed && tb && hi) initQuillEditor(config);
            });
        })
        .catch(err => {
            console.error('Error loading partial view:', err);
        });
}

// ----------------------------------------
// 5. Form submission handlers for all forms (excluding .no-js-submit), with:
// Binds submission handlers to all forms (except those with .no-js-submit) to:
// Prevent double submissions via a `busy` flag
// Clear existing validation messages
// Format a Date of Birth from separate day/month/year fields
// Enforce that at least one member is selected when required
// Perform client-side required-field validation based on data-val attributes
// Submit the form via AJAX (Fetch API)
// Handle success responses (redirect or close modal + reload)
// Handle validation errors returned from the server (400 status)
// Got coached by ChatGPT on this one, some lines är from ChatGPT and some are from me.
// I also read the documentation on this pages: https://api.jquery.com/ and https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide
// ----------------------------------------
function bindFormSubmitHandlers() {
    // Select all forms that should use JS submission
    document.querySelectorAll('form:not(.no-js-submit)').forEach(form => {
        let busy = false; // Prevents multiple simultaneous submissions
        form.addEventListener('submit', async e => {
            e.preventDefault();
            if (busy) return; // If a submission is already in progress, abort
            busy = true;

            // Remove any previous client-side validation messages/styles
            clearErrorMessages(form);

            // -----------------------------
            // 1. Format Date of Birth field
            // -----------------------------
            const d = form.querySelector('#dobDay')?.value;
            const mo = form.querySelector('#dobMonth')?.value;
            const y = form.querySelector('#dobYear')?.value;
            const hd = form.querySelector('input[name="DateOfBirth"]');
            // If day, month, and year are all provided, assemble YYYY-MM-DD
            if (hd) hd.value = (d && mo && y) ? `${y}-${mo.padStart(2, '0')}-${d.padStart(2, '0')}` : '';

            // -------------------------------------------------
            // 2. Ensure at least one member is selected (if used)
            // -------------------------------------------------
            const selectedMembersInput = form.querySelector('input[name="FormData.SelectedMemberIdsRaw"]');
            if (selectedMembersInput && selectedMembersInput.value.trim() === '') {
                // Highlight the project-tags field group and show error message
                const membersFieldGroup = form.querySelector('#project-tags');
                if (membersFieldGroup) {
                    const errorSpan = form.querySelector('[data-valmsg-for="FormData.SelectedMemberIdsRaw"]');
                    if (errorSpan) {
                        errorSpan.innerText = 'You must select at least one member.';
                        errorSpan.classList.add('field-validation-error');
                    }
                    membersFieldGroup.classList.add('input-validation-error');
                }
                // Reset busy flag and restore submit button state
                busy = false;
                const btn = form.querySelector('button[type="submit"]');
                if (btn) {
                    btn.disabled = false;
                    btn.innerText = btn.getAttribute('data-original-text') || btn.innerText;
                }
                return; // Abort submission
            }

            // ---------------------------------------------------
            // 3. Client-side required-field validation using data-val
            // ---------------------------------------------------
            let hasError = false;
            form.querySelectorAll('[data-val="true"]').forEach(i => {
                if (!i.value.trim()) {
                    hasError = true;
                    i.classList.add('input-validation-error');
                    const sp = form.querySelector(`[data-valmsg-for="${i.name}"]`);
                    if (sp) {
                        // Extended validation shows custom message, otherwise generic
                        sp.innerText = form.classList.contains('extended-validation')
                            ? i.getAttribute('data-val-required') || 'This field is required'
                            : 'Field is required';
                        sp.classList.add('field-validation-error');
                    }
                }
            });

            // Store reference to submit button & its original text
            const btn = form.querySelector('button[type="submit"]');
            const txt = btn?.getAttribute('data-original-text') || btn?.innerText;

            if (hasError) {
                // If any required fields are empty, reset busy and button, then abort
                busy = false;
                if (btn) { btn.disabled = false; btn.innerText = txt; }
                return;
            }

            // ---------------------------------
            // 4. Submit form data via AJAX POST
            // ---------------------------------
            try {
                const res = await fetch(form.action, { method: 'POST', body: new FormData(form) });
                const j = await res.json();

                if (res.ok) {
                    // On success: either redirect or close modal + reload
                    if (j.redirectUrl) window.location.href = j.redirectUrl;
                    else {
                        const mo = form.closest('.modal');
                        if (mo) mo.style.display = 'none';
                        window.location.reload();
                    }
                } else if (res.status === 400) {

                    // ----------------------------------------------------------------
                    // 5. Server-side validation errors: map error messages to fields
                    // ----------------------------------------------------------------

                    if (j.errors) {
                        Object.keys(j.errors).forEach(k => {
                            const i = form.querySelector(`[name="${k}"]`);
                            if (i) i.classList.add('input-validation-error');
                            const sp = form.querySelector(`[data-valmsg-for="${k}"]`);
                            if (sp) {
                                sp.innerText = j.errors[k].join('\n');
                                sp.classList.add('field-validation-error');
                            }
                        });
                    }
                    // General form-level error (e.g. authentication failure)
                    if (j.error) {
                        const msg = form.querySelector('.form-message');
                        if (msg) {
                            msg.innerText = j.error;
                            msg.style.display = 'block';
                        }
                    }
                    // Reset busy flag and restore submit button
                    busy = false;
                    if (btn) { btn.disabled = false; btn.innerText = txt; }
                }
            } catch (err) {
                // Network or unexpected error: log and reset state
                console.error('Submit failed', err);
                if (btn) { btn.disabled = false; btn.innerText = txt; }
                busy = false;
            }
        });
    });
}

// ----------------------------------------
// 6. Administrator email validation via server lookup before allowing login.
// On blur or before submit, sends a GET request to / Auth / IsAdmin ? email =…
// to verify that the entered email belongs to an administrator, disabling
// the submit button and showing an error message if not.
// Got coached by ChatGPT on this one, some lines är from ChatGPT and some are from me.
// ----------------------------------------
function bindAdminEmailValidation() {
    // Find the admin login form by its ID
    const form = document.getElementById('admin-login-form');
    if (!form) return;

    // Locate the email input, error message span, and submit button
    const emailInput = form.querySelector('#admin-email');
    const errorSpan = form.querySelector('#admin-email-error');
    const submitBtn = form.querySelector('button[type="submit"]');

    // Track the last email we checked to avoid duplicate network calls
    let lastChecked = '';

    /*
     * Checks with the server whether the given email is an admin.
     * - Clears any previous error if email is empty.
     * - Skips the check if the email hasn't changed since last time.
     * - On a non-admin response, shows an error and disables the submit button.
     * - Re-enables the button and clears the error if admin or on network failure.
     */
    async function checkAdmin(email) {
        // If the input is empty, clear errors and enable submit
        if (!email) {
            errorSpan.innerText = '';
            submitBtn.disabled = false;
            return;
        }
        // If the same email was just checked, skip the network request
        if (email === lastChecked) return;
        lastChecked = email;

        try {
        // Send GET request to the server endpoint
            const res = await fetch(`/Auth/IsAdmin?email=${encodeURIComponent(email)}`, {
                method: 'GET',
                credentials: 'same-origin'
            });
            if (!res.ok) throw new Error(res.status); // Treat non-2xx as error
            const json = await res.json();
            if (!json.isAdmin) {
                // Not an admin: display message and disable submission
                errorSpan.innerText = 'You must be an administrator to log in here.';
                submitBtn.disabled = true;
            } else {
                // Valid admin: clear any error and re-enable button
                errorSpan.innerText = '';
                submitBtn.disabled = false;
            }
        } catch (err) {
            // On network or parsing error, log it and allow the user to procee
            console.error('Admin-check failed', err);
            errorSpan.innerText = '';
            submitBtn.disabled = false;
        }
    }

    // When the email field loses focus, trigger an admin check
    emailInput.addEventListener('blur', e => checkAdmin(e.target.value));
    // When the user types again, clear any existing error and re-enable the button
    emailInput.addEventListener('input', e => {
        errorSpan.innerText = '';
        submitBtn.disabled = false;
    });

/*
 * On form submission:
 * - If the email hasn't been checked yet, prevent default submit,
 *   perform the check, and then submit or keep disabled based on result.
 */
    form.addEventListener('submit', async e => {
        // If there's an email and we haven't just checked it
        const email = emailInput.value.trim();
        if (email && email !== lastChecked) {
            e.preventDefault(); // Stop the form from submitting
            await checkAdmin(email); // Perform the admin check
            if (submitBtn.disabled) return; // If still disabled, abort submit
            form.submit(); // Otherwise, proceed with submission
        }
    });
}

// ----------------------------------------
// 7. DOMContentLoaded initialization:
// Modal triggers
// Image previewers, close buttons, form binds, password toggles, dropdowns
// Quill editor instantiation
// Logout button AJAX
// Notification list loading and time updates
// ----------------------------------------
document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('[data-modal="true"]').forEach(b => {
        b.addEventListener('click', () => {
            const t = b.getAttribute('data-target');
            const m = document.querySelector(t);
            if (m) m.style.display = 'flex';
        });
    });

    bindImagePreviewers();
    bindCloseButtons();
    bindAdminEmailValidation(); 
    bindFormSubmitHandlers();
    bindTogglePassword();
    setupDropdownToggles();

    quillConfigs.forEach(config => {
        const editorEl = document.getElementById(config.editorId);
        const toolbarEl = document.getElementById(config.toolbarId);
        const inputEl = document.getElementById(config.hiddenInputId);
        if (editorEl && toolbarEl && inputEl) {
            initQuillEditor(config);
        }
    });

    const lo = document.getElementById('logoutButton');
    if (lo) {
        lo.addEventListener('click', async e => {
            e.preventDefault();
            try {
                const r = await fetch('/Auth/LogOut', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/x-www-form-urlencoded',
                        'RequestVerificationToken': getRequestVerificationToken()
                    }
                });
                const d = await r.json();
                if (r.ok && d.redirectUrl) window.location.href = d.redirectUrl;
                else window.location.reload();
            } catch (err) {
                console.error('Logout failed', err);
            }
        });
    }

    if (document.getElementById('notification-list')) {
        loadNotifications();
        setInterval(updateRelativeTimes, 60000);
        updateRelativeTimes();
    }
});

// ----------------------------------------
// 8. Dropdown menu open/close logic and delete confirmation dialogs.
// ----------------------------------------
function setupDropdownToggles() {
    document.querySelectorAll('.dropdown-toggle').forEach(toggle => {
        const dd = document.getElementById(toggle.getAttribute('data-dropdown'));
        if (!dd) return;
        toggle.addEventListener('click', e => {
            e.stopPropagation();
            const wasHidden = dd.classList.contains('hidden');
            closeAllDropdowns();
            if (wasHidden) dd.classList.remove('hidden');
        });
    });
    document.querySelectorAll('[data-delete-url]').forEach(btn => {
        btn.addEventListener('click', async e => {
            e.stopPropagation();
            if (!confirm('Are you sure you want to delete this project?')) return;
            try {
                const r = await fetch(btn.getAttribute('data-delete-url'), {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/x-www-form-urlencoded',
                        'RequestVerificationToken': getRequestVerificationToken()
                    }
                });
                const d = await r.json();
                if (r.ok && d.success) window.location.reload();
                else alert(d.error || 'Could not delete project.');
            } catch (err) {
                console.error(err);
                alert('An unexpected error occurred.');
            }
        });
    });
    document.addEventListener('click', e => {
        if (!e.target.closest('.dropdown-menu') && !e.target.closest('.dropdown-toggle')) {
            closeAllDropdowns();
        }
    });
}

function closeAllDropdowns() {
    document.querySelectorAll('.dropdown-menu').forEach(m => m.classList.add('hidden'));
}

async function loadNotifications() {
    try {
        const response = await fetch('/api/notifications');

        const contentType = response.headers.get("content-type");
        if (!response.ok || !contentType || !contentType.includes("application/json")) {
            console.warn("Användaren är troligen inte inloggad – hoppar över notification-laddning.");
            return;
        }

        const notifications = await response.json();
        const list = document.getElementById('notification-list');
        list.innerHTML = '';

        notifications.forEach(notification => {
            const item = document.createElement('div');
            item.className = 'notification-item';
            item.setAttribute('data-id', notification.id);

            item.innerHTML = `
                <img src="${notification.imagePath}" alt="User" class="notification-avatar" />
                <div class="notification-content">
                    <div class="notification-text">${notification.message}</div>
                    <div class="notification-time" data-created="${new Date(notification.created).toISOString()}"></div>
                </div>
                <button class="notification-close" onclick="dismissNotification('${notification.id}')">×</button>
            `;

            list.appendChild(item);
        });

        updateNotificationCount();
        updateRelativeTimes();
    } catch (err) {
        console.error('Fel vid hämtning av notifications:', err);
    }
}
