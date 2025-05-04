/*
 * Initializes a Quill rich-text editor with a custom toolbar and integrates it
 * with a hidden input field for form submission.
 *
 * - Locates the editor container, toolbar container, and hidden input by ID.
 * - Logs a warning and aborts if any element is missing.
 * - Inserts toolbar buttons for formatting (bold, italic, underline),
 *   alignment, lists, and links.
 * - Creates a Quill instance with the ‘snow’ theme and attaches the toolbar.
 * - If the hidden input already contains HTML, loads it into the editor.
 * - On form submission, writes the editor’s HTML back into the hidden input
 *   so it can be sent to the server.
 * - I got the inspiration for this from Hans video on Quill.js, and have also read
 *  the documentation on https://quilljs.com/docs/quickstart
 * I also got coached by ChatGPT to get the toolbar placement to work.
 */

window.initQuillEditor = function ({ editorId, toolbarId, hiddenInputId }) {
    const quillContainer = document.getElementById(editorId);
    const toolbarContainer = document.getElementById(toolbarId);
    const hiddenInput = document.getElementById(hiddenInputId);

    if (!quillContainer || !toolbarContainer || !hiddenInput) {
        console.warn('[Quill] Saknar element:', { editorId, toolbarId, hiddenInputId });
        return;
    }

    // Render the toolbar HTML
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

    // Initialize Quill with the custom toolbar and placeholder
    const quill = new Quill(quillContainer, {
        theme: 'snow',
        modules: {
            toolbar: toolbarContainer.querySelector('.ql-toolbar')
        },
        placeholder: 'Type something...'
    });

    // If there's existing HTML, load it into the editor
    if (hiddenInput.value && hiddenInput.value.trim() !== '') {
        quill.clipboard.dangerouslyPasteHTML(hiddenInput.value);
    }

    // On form submit, sync the editor's HTML back to the hidden input
    const form = quillContainer.closest('form');
    if (form) {
        form.addEventListener('submit', () => {
            hiddenInput.value = quill.root.innerHTML;
        }, true);
    }
};
