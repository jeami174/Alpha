// ----------------------------------------
// 1. Hjälpfunktioner
// ----------------------------------------

function getRequestVerificationToken() {
    const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
    return tokenElement ? tokenElement.value : '';
}

function clearErrorMessages(form) {
    form.querySelectorAll('[data-val="true"]').forEach(input => {
        input.classList.remove('input-validation-error');
    });

    form.querySelectorAll('[data-valmsg-for]').forEach(span => {
        span.innerText = '';
        span.classList.remove('field-validation-error', 'extended-field-validation-error');
    });

    const formMessage = form.querySelector('.form-message');
    if (formMessage) {
        formMessage.innerText = '';
        formMessage.style.display = 'none';
    }
}

async function loadImage(file) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onerror = () => reject(new Error("Failed to load file."));
        reader.onload = (e) => {
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
    } catch (error) {
        console.error('Failed on image processing', error);
    }
}

function bindImagePreviewers() {
    document.querySelectorAll('.image-previewer').forEach(previewer => {
        const fileInput = previewer.querySelector('input[type="file"]');
        const imagePreview = previewer.querySelector('.image-preview');

        previewer.addEventListener('click', () => fileInput.click());

        fileInput.addEventListener('change', ({ target: { files } }) => {
            const file = files[0];
            if (file) processImage(file, imagePreview, previewer);
        });
    });
}

function bindCloseButtons() {
    document.querySelectorAll('[data-close="true"]').forEach(button => {
        button.addEventListener('click', () => {
            const modal = button.closest('.modal');
            if (modal) {
                modal.style.display = 'none';
                modal.querySelectorAll('form').forEach(form => {
                    form.reset();
                    clearErrorMessages(form);
                    form.querySelectorAll('input[type="file"]').forEach(input => input.value = '');
                    const imagePreview = form.querySelector('.image-preview');
                    if (imagePreview) imagePreview.src = '';
                    const imagePreviewer = form.querySelector('.image-previewer');
                    if (imagePreviewer) imagePreviewer.classList.remove('selected');
                });
            }
        });
    });
}

function bindTogglePassword() {
    const togglePassword = document.getElementById('togglePassword');
    if (togglePassword) {
        togglePassword.addEventListener('click', function () {
            const passwordField = document.getElementById('passwordField');
            if (passwordField) {
                const type = passwordField.getAttribute('type') === 'password' ? 'text' : 'password';
                passwordField.setAttribute('type', type);
            }
        });
    }

    const toggleConfirmPassword = document.getElementById('toggleConfirmPassword');
    if (toggleConfirmPassword) {
        toggleConfirmPassword.addEventListener('click', function () {
            const confirmPasswordField = document.getElementById('confirmPasswordField');
            if (confirmPasswordField) {
                const type = confirmPasswordField.getAttribute('type') === 'password' ? 'text' : 'password';
                confirmPasswordField.setAttribute('type', type);
            }
        });
    }
}

// ----------------------------------------
// 2. Dynamisk Partial Loader Funktion
// ----------------------------------------

function loadPartialView(url, containerId) {
    fetch(url)
        .then(response => response.text())
        .then(html => {
            const container = document.getElementById(containerId);
            if (!container) return;

            container.innerHTML = html;

            // 🛑 NU: Kör alla scripts manuellt
            container.querySelectorAll('script').forEach(oldScript => {
                const newScript = document.createElement('script');
                if (oldScript.src) {
                    newScript.src = oldScript.src;
                } else {
                    newScript.textContent = oldScript.textContent;
                }
                document.body.appendChild(newScript);
                oldScript.remove(); // rensa gamla script
            });

            const modal = container.querySelector('.modal');
            if (modal) {
                modal.style.display = 'flex';
            }

            bindImagePreviewers();
            bindCloseButtons();
            bindFormSubmitHandlers();
        })
        .catch(error => console.error('Error loading partial view:', error));
}


// ----------------------------------------
// 3. Form Submit Handler
// ----------------------------------------

function bindFormSubmitHandlers() {
    document.querySelectorAll('form').forEach(form => {
        let isSubmitting = false;

        form.addEventListener('submit', async (e) => {
            e.preventDefault();
            if (isSubmitting) return;
            isSubmitting = true;

            clearErrorMessages(form);

            const day = form.querySelector('#dobDay')?.value;
            const month = form.querySelector('#dobMonth')?.value;
            const year = form.querySelector('#dobYear')?.value;
            const hiddenDobInput = form.querySelector('input[name="DateOfBirth"]');

            if (hiddenDobInput) {
                hiddenDobInput.value = (!day || !month || !year)
                    ? ''
                    : `${year}-${month.padStart(2, '0')}-${day.padStart(2, '0')}`;
            }

            let hasError = false;
            form.querySelectorAll('[data-val="true"]').forEach(input => {
                if (!input.value.trim()) {
                    hasError = true;
                    input.classList.add('input-validation-error');
                    const span = form.querySelector(`[data-valmsg-for="${input.name}"]`);
                    if (span) {
                        const customMessage = form.classList.contains('extended-validation')
                            ? input.getAttribute('data-val-required') || 'This field is required'
                            : 'Field is required';
                        span.innerText = customMessage;
                        span.classList.add('field-validation-error');
                    }
                }
            });

            const submitButton = form.querySelector('button[type="submit"]');
            const originalText = submitButton?.getAttribute("data-original-text") || submitButton?.innerText;

            if (hasError) {
                isSubmitting = false;
                if (submitButton) {
                    submitButton.disabled = false;
                    submitButton.innerText = originalText;
                }
                return;
            }

            const formData = new FormData(form);

            try {
                const res = await fetch(form.action, {
                    method: 'post',
                    body: formData
                });

                const data = await res.json();

                if (res.ok) {
                    if (data.redirectUrl) {
                        window.location.href = data.redirectUrl;
                    } else {
                        const modal = form.closest('.modal');
                        if (modal) modal.style.display = 'none';
                        window.location.reload();
                    }
                } else if (res.status === 400) {
                    if (data.errors) {
                        Object.keys(data.errors).forEach(key => {
                            const input = form.querySelector(`[name="${key}"]`);
                            if (input) input.classList.add('input-validation-error');
                            const span = form.querySelector(`[data-valmsg-for="${key}"]`);
                            if (span) {
                                span.innerText = data.errors[key].join('\n');
                                span.classList.add('field-validation-error');
                            }
                        });
                    }

                    if (data.error) {
                        const summary = form.querySelector('.form-message');
                        if (summary) {
                            summary.innerText = data.error;
                            summary.style.display = 'block';
                        }
                    }

                    if (submitButton) {
                        submitButton.disabled = false;
                        submitButton.innerText = originalText;
                    }

                    isSubmitting = false;
                }
            } catch (error) {
                console.log('Failed to submit form', error);
                if (submitButton) {
                    submitButton.disabled = false;
                    submitButton.innerText = originalText;
                }
                isSubmitting = false;
            }
        });
    });
}

// ----------------------------------------
// 4. Init – DOMContentLoaded
// ----------------------------------------

document.addEventListener('DOMContentLoaded', () => {
    const modalButtons = document.querySelectorAll('[data-modal="true"]');
    modalButtons.forEach(button => {
        button.addEventListener('click', () => {
            const modalTarget = button.getAttribute('data-target');
            const modal = document.querySelector(modalTarget);
            if (modal) modal.style.display = 'flex';
        });
    });

    document.querySelectorAll('[data-fetch-url]').forEach(element => {
        element.addEventListener('click', (e) => {
            closeAllDropdowns();

            const url = element.getAttribute('data-fetch-url');
            const container = element.getAttribute('data-container');
            if (url && container) {
                loadPartialView(url, container);
            }
        });
    });

    bindImagePreviewers();
    bindCloseButtons();
    bindFormSubmitHandlers();
    bindTogglePassword();
    setupDropdownToggles();

    const logoutButton = document.getElementById('logoutButton');

    if (logoutButton) {
        logoutButton.addEventListener('click', async (e) => {
            e.preventDefault();

            try {
                const res = await fetch('/Auth/LogOut', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/x-www-form-urlencoded',
                        'RequestVerificationToken': getRequestVerificationToken()
                    }
                });

                const data = await res.json();

                if (res.ok && data.redirectUrl) {
                    window.location.href = data.redirectUrl;
                } else {
                    console.warn('Unexpected logout response:', data);
                    window.location.reload();
                }
            } catch (error) {
                console.error('Logout failed', error);
            }
        });
    }
});

// ----------------------------------------
// 5. Dropdown Toggle (modulär för flera)
// ----------------------------------------

function setupDropdownToggles() {
    const toggles = document.querySelectorAll('.dropdown-toggle');

    toggles.forEach(toggle => {
        const dropdownId = toggle.getAttribute('data-dropdown');
        const dropdown = document.getElementById(dropdownId);

        if (dropdown) {
            toggle.addEventListener('click', (e) => {
                e.stopPropagation();
                if (dropdown.classList.contains('hidden')) {
                    closeAllDropdowns();
                    dropdown.classList.remove('hidden');
                } else {
                    dropdown.classList.add('hidden');
                }
            });
        }
    });


    document.querySelectorAll('[data-delete-url]').forEach(btn => {
        btn.addEventListener('click', async function (e) {
            e.stopPropagation();
            const url = this.getAttribute('data-delete-url');

            if (!confirm('Are you sure you want to delete this project?')) return;

            try {
                const res = await fetch(url, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/x-www-form-urlencoded',
                        'RequestVerificationToken': getRequestVerificationToken()
                    },
                });
                const data = await res.json();

                if (res.ok && data.success) {
                    window.location.reload();
                } else {
                    alert(data.error || 'Could not delete project.');
                }
            } catch (err) {
                console.error(err);
                alert('An unexpected error occurred.');
            }
        });
    });


    document.addEventListener('click', (e) => {
        if (!e.target.closest('.dropdown-menu') && !e.target.closest('.dropdown-toggle')) {
            closeAllDropdowns();
        }
    });
}

function closeAllDropdowns() {
    document.querySelectorAll('.dropdown-menu').forEach(menu => {
        menu.classList.add('hidden');
    });
}

