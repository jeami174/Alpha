/**
 * tags.js – Tag Selector med rootElement-stöd
 */
function initTagSelector(config) {
    const root = (config.rootElement instanceof HTMLElement)
        ? config.rootElement
        : document;
    const container = root.querySelector('#' + config.containerId);
    const input = root.querySelector('#' + config.inputId);
    const results = root.querySelector('#' + config.resultsId);
    const hidden = root.querySelector('#' + config.selectedInputIds);

    if (!container || !input || !results || !hidden) {
        console.error('initTagSelector saknar element', config);
        return;
    }

    let selectedIds = [];
    let activeIndex = -1;
    let debounceTimer;

    // Uppdatera det dolda input-fältet, nytt
    function updateHidden() {
        if (selectedIds.length === 0) {
            hidden.value = '';
        } else {
            hidden.value = selectedIds.join(',');
        }
    }

    // Lägg till en tagg i UI och intern array
    function addTag(item) {
        const id = String(item.id);
        if (selectedIds.includes(id)) return;
        selectedIds.push(id);
        updateHidden();

        const tag = document.createElement('div');
        tag.classList.add(config.tagClass || 'tag');
        tag.innerHTML = config.imageProperty && item[config.imageProperty]
            ? `<img class="user-avatar" src="${config.avatarFolder}${item[config.imageProperty]}" />
               <span>${item[config.displayProperty]}</span>`
            : `<span>${item[config.displayProperty]}</span>`;

        const btn = document.createElement('span');
        btn.classList.add('btn-remove');
        btn.textContent = '×';

        btn.addEventListener('click', () => {
            // Ta bort id, uppdatera hidden, ta bort element, och göm results
            selectedIds = selectedIds.filter(x => x !== id);
            updateHidden();
            tag.remove();
            results.innerHTML = '';
            results.style.display = 'none';
        });

        tag.appendChild(btn);
        container.insertBefore(tag, input);
    }

    // Rendera sökresultat i dropp-listan
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

    // Debounced fetch + render
    input.addEventListener('input', () => {
        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(() => {
            const q = input.value.trim();
            activeIndex = -1;

            if (!q) {
                results.innerHTML = '';
                results.style.display = 'none';
                return;
            }

            fetch(config.searchUrl(q))
                .then(r => r.json())
                .then(renderResults)
                .catch(err => console.error('initTagSelector fetch error:', err));
        }, 300);
    });

    // Piltangenter & Enter & Backspace
    input.addEventListener('keydown', e => {
        const items = results.querySelectorAll('.search-item');
        if (!items.length) return;

        switch (e.key) {
            case 'ArrowDown':
                e.preventDefault();
                activeIndex = (activeIndex + 1) % items.length;
                items.forEach(i => i.classList.remove('active'));
                items[activeIndex].classList.add('active');
                items[activeIndex].scrollIntoView({ block: 'nearest' });
                break;
            case 'ArrowUp':
                e.preventDefault();
                activeIndex = (activeIndex - 1 + items.length) % items.length;
                items.forEach(i => i.classList.remove('active'));
                items[activeIndex].classList.add('active');
                items[activeIndex].scrollIntoView({ block: 'nearest' });
                break;
            case 'Enter':
                if (activeIndex >= 0) {
                    e.preventDefault();
                    items[activeIndex].click();
                }
                break;
            case 'Backspace':
                if (input.value === '') {
                    const tags = container.querySelectorAll('.' + (config.tagClass || 'tag'));
                    if (tags.length) {
                        tags[tags.length - 1].querySelector('.btn-remove').click();
                    }
                }
                break;
        }
    });

    // Förvalda tags (preselected)
    if (Array.isArray(config.preselected)) {
        config.preselected.forEach(addTag);
        updateHidden();
    }
}

// Exponera globalt
window.initTagSelector = initTagSelector;
