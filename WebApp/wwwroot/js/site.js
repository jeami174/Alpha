// ----------------------------------------
// 1. Hjälpfunktioner
// ----------------------------------------

function clearErrorMessages(form) {
    form.querySelectorAll('[data-val="true"]').forEach(input => {
        input.classList.remove('input-validation-error');
    });

    form.querySelectorAll('[data-valmsg-for]').forEach(span => {
        span.innerText = '';
        span.classList.remove('field-validation-error', 'extended-field-validation-error');
    });
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

            const modal = container.querySelector('.modal');
            if (modal) {
                modal.style.display = 'flex';

                const previewer = modal.querySelector('.image-previewer');
                const imagePreview = previewer?.querySelector('.image-preview');

                if (imagePreview?.src && !imagePreview.src.includes('base64')) {
                    const imageUrl = imagePreview.src;
                    fetch(imageUrl)
                        .then(res => res.blob())
                        .then(blob => {
                            const file = new File([blob], "preview.jpg", { type: blob.type });
                            processImage(file, imagePreview, previewer);
                        });
                }
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
    document.querySelectorAll('.modal form').forEach(form => {
        form.addEventListener('submit', async (e) => {
            e.preventDefault();
            clearErrorMessages(form);

            const day = form.querySelector('#dobDay')?.value;
            const month = form.querySelector('#dobMonth')?.value;
            const year = form.querySelector('#dobYear')?.value;
            const hiddenDobInput = form.querySelector('input[name="DateOfBirth"]');

            if (hiddenDobInput) {
                if (!day || !month || !year) {
                    hiddenDobInput.value = '';
                } else {
                    hiddenDobInput.value = `${year}-${month.padStart(2, '0')}-${day.padStart(2, '0')}`;
                }
                console.log("DOB set to:", hiddenDobInput.value);
            }

            let hasError = false;
            form.querySelectorAll('[data-val="true"]').forEach(input => {
                if (!input.value.trim()) {
                    hasError = true;
                    input.classList.add('input-validation-error');
                    const span = form.querySelector(`[data-valmsg-for="${input.name}"]`);
                    if (span) {
                        span.innerText = 'Field is required';
                        span.classList.add('field-validation-error');
                    }
                }
            });

            if (hasError) return;

            const formData = new FormData(form);

            try {
                const res = await fetch(form.action, {
                    method: 'post',
                    body: formData
                });

                if (res.ok) {
                    const modal = form.closest('.modal');
                    if (modal) modal.style.display = 'none';
                    window.location.reload();
                } else if (res.status === 400) {
                    const data = await res.json();
                    if (data.errors) {
                        Object.keys(data.errors).forEach(key => {
                            let input = form.querySelector(`[name="${key}"]`);
                            if (input) input.classList.add('input-validation-error');

                            let span = form.querySelector(`[data-valmsg-for="${key}"]`);
                            if (span) {
                                span.innerText = data.errors[key].join('\n');

                                if (form.classList.contains('extended-validation')) {
                                    span.classList.add('extended-field-validation-error');
                                } else {
                                    span.classList.add('field-validation-error');
                                }
                            }
                        });
                    }
                }
            } catch (error) {
                console.log('Failed to submit form', error);
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
        element.addEventListener('click', () => {
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
});
