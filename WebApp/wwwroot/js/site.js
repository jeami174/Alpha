document.addEventListener('DOMContentLoaded', () => {

    const previewSize = 150

    // open modal
    const modalButtons = document.querySelectorAll('[data-modal="true"]')
    modalButtons.forEach(button => {
        button.addEventListener('click', () => {
            const modalTarget = button.getAttribute('data-target')
            const modal = document.querySelector(modalTarget)

            if (modal)
                modal.style.display = 'flex';
        })
    })

    // close modal
    const closeButtons = document.querySelectorAll('[data-close="true"]')
    closeButtons.forEach(button => {
        button.addEventListener('click', () => {
            const modal = button.closest('.modal')
            if (modal) {
                modal.style.display = 'none'

                modal.querySelectorAll('form').forEach(form => {
                    form.reset()

                    // Lägg till denna rad för att rensa upp felmeddelanden
                    clearErrorMessages(form)

                    const imagePreview = form.querySelector('.image-preview')
                    if (imagePreview)
                        imagePreview.src = ''

                    const imagePreviewer = form.querySelector('.image-previewer')
                    if (imagePreviewer)
                        imagePreviewer.classList.remove('selected')
                })
            }
        })
    })

    // Handle image-previewer

    document.querySelectorAll('.image-previewer').forEach(previewer => {
        const fileInput = previewer.querySelector('input[type="file"]')
        const imagePreview = previewer.querySelector('.image-preview')

        previewer.addEventListener('click', () => fileInput.click())

        fileInput.addEventListener('change', ({ target: { files } }) => {
            const file = files[0]
            if (file) 
                processImage(file, imagePreview, previewer, previewSize)
           
        })
    })

    // Handle form submit

    const forms = document.querySelectorAll('form')
    forms.forEach(form => {
        form.addEventListener('submit', async (e) => {
            e.preventDefault()

            clearErrorMessages(form)

            const formData = new FormData(form)

            try {
                const res = await fetch(form.action, {
                    method: 'post',
                    body: formData
                })

                if (res.ok) {
                    const modal = form.closest('.modal')
                    if (modal)
                        modal.style.display = 'none';

                        window.location.reload()
                    
                }

                else if (res.status === 400) {
                    const data = await res.json();

                    if (data.errors) {
                        Object.keys(data.errors).forEach(key => {
                            let input = form.querySelector(`[name="${key}"]`);
                            if (input) {
                                input.classList.add('input-validation-error');
                            }

                            let span = form.querySelector(`[data-valmsg-for="${key}"]`);
                            if (span) {
                                span.innerText = data.errors[key].join('\n');
                                span.classList.add('field-validation-error');
                            }
                        });
                    }
                }
            }
            catch (error) {
                console.log('Failed to submit form')
            }
        })
    })
 })

function clearErrorMessages(form) {
    form.querySelectorAll('[data-val="true"]').forEach(input => {
        input.classList.remove('input-validation-error')

    })

    form.querySelectorAll('[data-valmsg-for]').forEach(span => {
        span.innerText = ''
        span.classList.remove('field-validation-error')
    })
}

//Add error message function

function addErrorMessage(key, errorMessage) {

}

//Ladda bilden funktion

async function loadImage(file) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader()

        reader.onerror = () => reject(new Error("failed to load file."))
        reader.onload = (e) => {
            const img = new Image()
            img.onerror = () => reject(new Error("failed to load image."))
            img.onload = () => resolve(img)
            img.src = e.target.result
        }

        reader.readAsDataURL(file)
    })
}

async function processImage(file, imagePreview, previewer, previewSize = 150) {
    try {
        const img = await loadImage(file)
        const canvas = document.createElement('canvas')
        canvas.width = previewSize
        canvas.height = previewSize

        const ctx = canvas.getContext('2d')
        ctx.drawImage(img, 0, 0, previewSize, previewSize)
        imagePreview.src = canvas.toDataURL('image/jpeg')
        previewer.classList.add('selected')
    }
    catch (error) {
        console.error('Failed on image processing', error)
    }
}

//Funktion för att bygga ihop DateOfBirth för de formulär som har det
document.addEventListener('DOMContentLoaded', () => {
    const form = document.querySelector('#addMemberModal form');
    if (!form) return;

    form.addEventListener('submit', () => {
        const day = document.getElementById('dobDay')?.value;
        const month = document.getElementById('dobMonth')?.value;
        const year = document.getElementById('dobYear')?.value;

        const hiddenDobInput = form.querySelector('input[name="DateOfBirth"]');
        if (!hiddenDobInput) return;

        if (!day || !month || !year) {
            hiddenDobInput.value = '';
            return;
        }

        const dayPadded = day.padStart(2, '0');
        const monthPadded = month.padStart(2, '0');
        hiddenDobInput.value = `${year}-${monthPadded}-${dayPadded}`;
    });
});