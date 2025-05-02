// Quill.js

window.initQuillEditor = function ({ editorId, toolbarId, hiddenInputId }) {
    const quillContainer = document.getElementById(editorId);
    const toolbarContainer = document.getElementById(toolbarId);
    const hiddenInput = document.getElementById(hiddenInputId);

    if (!quillContainer || !toolbarContainer || !hiddenInput) {
        console.warn('[Quill] Saknar element:', { editorId, toolbarId, hiddenInputId });
        return;
    }

    toolbarContainer.innerHTML = `
        <div class="ql-toolbar ql-snow">
            <span class="ql-formats">
                <button class="ql-bold"></button>
                <button class="ql-italic"></button>
                <button class="ql-underline"></button>
            </span>
            <span class="ql-formats">
                <button class="ql-align" value=""></button>
                <button class="ql-align" value="center"></button>
                <button class="ql-align" value="right"></button>
            </span>
            <span class="ql-formats">
                <button class="ql-list" value="ordered"></button>
                <button class="ql-list" value="bullet"></button>
            </span>
            <span class="ql-formats">
                <button class="ql-link"></button>
            </span>
        </div>
    `;

    const quill = new Quill(quillContainer, {
        theme: 'snow',
        modules: {
            toolbar: toolbarContainer.querySelector('.ql-toolbar')
        },
        placeholder: 'Type something...'
    });

    if (hiddenInput.value && hiddenInput.value.trim() !== '') {
        quill.clipboard.dangerouslyPasteHTML(hiddenInput.value);
    }

    const form = quillContainer.closest('form');
    if (form) {
        form.addEventListener('submit', () => {
            hiddenInput.value = quill.root.innerHTML;
        }, true);
    }
};
