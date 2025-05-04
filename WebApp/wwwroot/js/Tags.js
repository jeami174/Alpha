/*
 * tags.js – Tag Selector with optional rootElement scoping
 *
 * Initializes a tag picker UI that:
 * - Fetches suggestions via AJAX as the user types
 * - Renders a dropdown of search results
 * - Allows selecting items to turn them into “tags”
 * - Maintains a hidden input with the comma-separated list of selected IDs
 * - Supports keyboard navigation and preselected items
 * - This is inspired by Hans Video, the code is mine/Hans but have got coached from ChatGPT (especialy on Debounced search)
 */

function initTagSelector(config) {
    // Determine the root context: either a specific element or the whole document
    const root = (config.rootElement instanceof HTMLElement)
        ? config.rootElement
        : document;

    // Locate the main elements within the root:
    // container: holds the tags and the input
    // input: where the user types to search
    // results: dropdown container for saerch suggestions
    // hidden: actual <input> element that will hold selected IDs
    const container = root.querySelector('#' + config.containerId);
    const input = root.querySelector('#' + config.inputId);
    const results = root.querySelector('#' + config.resultsId);
    const hidden = root.querySelector('#' + config.selectedInputIds);

    // If any element is missing, abort initialization
    if (!container || !input || !results || !hidden) {
        console.error('initTagSelector saknar element', config);
        return;
    }

    // Internal state:
    let selectedIds = []; // Array of selected item IDs (as strings)
    let activeIndex = -1; // Index of the currently highlighted dropdown item
    let debounceTimer; // Timer handle for input debounce

    /*
     * Writes the current selectedIds array back into the hidden input
     * as a comma-separated string (or clears it if no selections).
     */
    function updateHidden() {
        if (selectedIds.length === 0) {
            hidden.value = '';
        } else {
            hidden.value = selectedIds.join(',');
        }
    }

    /*
     * Adds a tag to the UI for the given item and updates internal state:
     * - Skips duplicates
     * - Renders the tag markup (with optional avatar image)
     * - Attaches a remove button to allow unselecting
     */
    function addTag(item) {
        const id = String(item.id);
        if (selectedIds.includes(id)) return;  // Already selected
        selectedIds.push(id);
        updateHidden();

        // Create the tag element
        const tag = document.createElement('div');
        tag.classList.add(config.tagClass || 'tag');
        // If an image property is configured, include avatar markup
        tag.innerHTML = config.imageProperty && item[config.imageProperty]
            ? `<img class="user-avatar" src="${config.avatarFolder}${item[config.imageProperty]}" />
               <span>${item[config.displayProperty]}</span>`
            : `<span>${item[config.displayProperty]}</span>`;

        // Create the remove (“×”) button
        const btn = document.createElement('span');
        btn.classList.add('btn-remove');
        btn.textContent = '×';

        // On click: remove from selectedIds, update hidden, remove tag UI, and hide dropdown
        btn.addEventListener('click', () => {
            selectedIds = selectedIds.filter(x => x !== id);
            updateHidden();
            tag.remove();
            results.innerHTML = '';
            results.style.display = 'none';
        });

        tag.appendChild(btn);
        // Insert the new tag before the input box
        container.insertBefore(tag, input);
    }

    /*
     * Renders a list of suggestion items in the dropdown.
     * - Clears previous results
     * - Shows a “No results” message if none returned
     * - Otherwise creates a div per item, skipping already selected IDs
     * - Attaches click handlers to select an item as a tag
     */
    function renderResults(data) {
        results.innerHTML = '';
        if (!data.length) {
            results.innerHTML = `<div class="search-item">
                ${config.emptyMessage || 'No results'}
            </div>`;
        } else {
            data.forEach(item => {
                const id = String(item.id);
                if (!selectedIds.includes(id)) {
                    const div = document.createElement('div');
                    div.classList.add('search-item');
                    div.innerHTML = config.imageProperty && item[config.imageProperty]
                        ? `<img class="user-avatar" src="${config.avatarFolder}${item[config.imageProperty]}" />
                           <span>${item[config.displayProperty]}</span>`
                        : `<span>${item[config.displayProperty]}</span>`;

                    div.addEventListener('click', () => {
                        addTag(item);
                        results.innerHTML = '';
                        results.style.display = 'none';
                        input.value = '';
                    });

                    results.appendChild(div);
                }
            });
        }
        results.style.display = 'block';
    }

    // -------------------------------------------
    // Debounced search: wait 300ms after typing
    // -------------------------------------------
    input.addEventListener('input', () => {
        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(() => {
            const q = input.value.trim();
            activeIndex = -1;

            if (!q) {
                // If input empty, hide results
                results.innerHTML = '';
                results.style.display = 'none';
                return;
            }

            // Fetch suggestions from the configured search URL
            fetch(config.searchUrl(q))
                .then(r => r.json())
                .then(renderResults)
                .catch(err => console.error('initTagSelector fetch error:', err));
        }, 300);
    });

    // -------------------------------------------
    // Keyboard support: navigate & select & delete
    // -------------------------------------------
    input.addEventListener('keydown', e => {
        const items = results.querySelectorAll('.search-item');
        if (!items.length) return;

        switch (e.key) {
            case 'ArrowDown':
                e.preventDefault();
                // Move highlight down
                activeIndex = (activeIndex + 1) % items.length;
                items.forEach(i => i.classList.remove('active'));
                items[activeIndex].classList.add('active');
                items[activeIndex].scrollIntoView({ block: 'nearest' });
                break;
            case 'ArrowUp':
                e.preventDefault();
                // Move highlight up
                activeIndex = (activeIndex - 1 + items.length) % items.length;
                items.forEach(i => i.classList.remove('active'));
                items[activeIndex].classList.add('active');
                items[activeIndex].scrollIntoView({ block: 'nearest' });
                break;
            case 'Enter':
                // If an item is highlighted, select it
                if (activeIndex >= 0) {
                    e.preventDefault();
                    items[activeIndex].click();
                }
                break;
            case 'Backspace':
                // If input is empty, remove the last tag
                if (input.value === '') {
                    const tags = container.querySelectorAll('.' + (config.tagClass || 'tag'));
                    if (tags.length) {
                        tags[tags.length - 1].querySelector('.btn-remove').click();
                    }
                }
                break;
        }
    });

    // ------------------------------------------------
    // Prepopulate tags if config.preselected is provided
    // ------------------------------------------------
    if (Array.isArray(config.preselected)) {
        config.preselected.forEach(addTag);
        updateHidden();
    }
}

// Expose globally so it can be called after dynamic loads (got the ide from ChatGPT)
window.initTagSelector = initTagSelector;
