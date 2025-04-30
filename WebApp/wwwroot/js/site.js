// ----------------------------------------
// site.js - Global JavaScript
// ----------------------------------------

// 0. Global click-delegat för alla [data-fetch-url]
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


// 1. Hjälpfunktioner
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

        if (diffMinutes < 1) {
            relativeTime = 'just now';
        } else if (diffMinutes < 60) {
            relativeTime = `${diffMinutes} min ago`;
        } else if (diffHours < 24) {
            relativeTime = `${diffHours} h ago`;
        } else if (diffDays < 7) {
            relativeTime = `${diffDays} d ago`;
        } else {
            relativeTime = `${diffWeeks} w ago`;
        }

        el.textContent = relativeTime;
    });
}


function updateNotificationCount() {
    const notificationsList = document.querySelector('.notification-list');
    const countDisplay = document.querySelector('.notification-count');
    const dot = document.querySelector('.notification-dot');

    const count = notificationsList.querySelectorAll('.notification-item').length;

    if (countDisplay) {
        countDisplay.textContent = count;
    }

    if (dot) {
        if (count > 0) {
            dot.classList.add('active');
        } else {
            dot.classList.remove('active');
        }
    }
}


// 2. Dynamisk Partial Loader
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
            if (m) {
                m.style.display = 'flex';
            }


            bindImagePreviewers();
            bindCloseButtons();
            bindFormSubmitHandlers();
            bindTogglePassword();
            setupDropdownToggles();
        })
        .catch(err => {
            console.error('Error loading partial view:', err);
        });
}

// 3. Form Submit Handlers
function bindFormSubmitHandlers() {
    document.querySelectorAll('form').forEach(form => {
        let busy = false;
        form.addEventListener('submit', async e => {
            e.preventDefault();
            if (busy) return;
            busy = true;

            clearErrorMessages(form);

            const d = form.querySelector('#dobDay')?.value;
            const mo = form.querySelector('#dobMonth')?.value;
            const y = form.querySelector('#dobYear')?.value;
            const hd = form.querySelector('input[name="DateOfBirth"]');
            if (hd) hd.value = (d && mo && y) ? `${y}-${mo.padStart(2, '0')}-${d.padStart(2, '0')}` : '';

            const selectedMembersInput = form.querySelector('input[name="FormData.SelectedMemberIdsRaw"]');
            if (selectedMembersInput && selectedMembersInput.value.trim() === '') {
                const membersFieldGroup = form.querySelector('#project-tags');
                if (membersFieldGroup) {
                    const errorSpan = form.querySelector('[data-valmsg-for="FormData.SelectedMemberIdsRaw"]');
                    if (errorSpan) {
                        errorSpan.innerText = 'You must select at least one member.';
                        errorSpan.classList.add('field-validation-error');
                    }
                    membersFieldGroup.classList.add('input-validation-error');
                }
                busy = false;
                const btn = form.querySelector('button[type="submit"]');
                if (btn) {
                    btn.disabled = false;
                    btn.innerText = btn.getAttribute('data-original-text') || btn.innerText;
                }
                return;
            }

            let hasError = false;
            form.querySelectorAll('[data-val="true"]').forEach(i => {
                if (!i.value.trim()) {
                    hasError = true;
                    i.classList.add('input-validation-error');
                    const sp = form.querySelector(`[data-valmsg-for="${i.name}"]`);
                    if (sp) {
                        sp.innerText = form.classList.contains('extended-validation')
                            ? i.getAttribute('data-val-required') || 'This field is required'
                            : 'Field is required';
                        sp.classList.add('field-validation-error');
                    }
                }
            });

            const btn = form.querySelector('button[type="submit"]');
            const txt = btn?.getAttribute('data-original-text') || btn?.innerText;

            if (hasError) {
                busy = false;
                if (btn) { btn.disabled = false; btn.innerText = txt; }
                return;
            }

            try {
                const res = await fetch(form.action, { method: 'POST', body: new FormData(form) });
                const j = await res.json();

                if (res.ok) {
                    if (j.redirectUrl) window.location.href = j.redirectUrl;
                    else {
                        const mo = form.closest('.modal');
                        if (mo) mo.style.display = 'none';
                        window.location.reload();
                    }
                } else if (res.status === 400) {

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
                    if (j.error) {
                        const msg = form.querySelector('.form-message');
                        if (msg) {
                            msg.innerText = j.error;
                            msg.style.display = 'block';
                        }
                    }
                    busy = false;
                    if (btn) { btn.disabled = false; btn.innerText = txt; }
                }
            } catch (err) {
                console.error('Submit failed', err);
                if (btn) { btn.disabled = false; btn.innerText = txt; }
                busy = false;
            }
        });
    });
}

// 4. Init på laddning
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
    bindFormSubmitHandlers();
    bindTogglePassword();
    setupDropdownToggles();

    const lo = document.getElementById('logoutButton');
    if (lo) lo.addEventListener('click', async e => {
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


    loadNotifications();
    setInterval(updateRelativeTimes, 60000);
    updateRelativeTimes();
});


// 5. Dropdown-menyer
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

// Ladda alla aktuella notifications från API:t
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
