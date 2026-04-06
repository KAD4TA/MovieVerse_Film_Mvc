document.addEventListener('DOMContentLoaded', function () {
    const els = {
        posterInput: document.getElementById('PosterUrlInput'),
        posterImg: document.getElementById('poster-preview'),
        posterPlaceholder: document.getElementById('poster-placeholder'),
        dInput: document.getElementById('director-search-input'),
        dResultsContainer: document.getElementById('d-results'),
        dResultsList: document.getElementById('d-results-list'),
        dAddNewTrigger: document.getElementById('d-add-new-trigger'),
        dCard: document.getElementById('selected-director-card'),
        dArea: document.getElementById('director-selection-area'),
        dManual: document.getElementById('manual-director-form'),
        aInput: document.getElementById('actor-search-input'),
        aResultsContainer: document.getElementById('a-results'),
        aResultsList: document.getElementById('a-results-list'),
        aAddNewTrigger: document.getElementById('a-add-new-trigger'),
        aManual: document.getElementById('manual-actor-form'),
        castList: document.getElementById('cast-list'),
        castCount: document.getElementById('cast-count')
    };

    // Poster Preview
    if (els.posterInput) {
        els.posterInput.addEventListener('input', e => {
            const url = e.target.value.trim();
            if (url) {
                els.posterImg.src = url;
                els.posterImg.classList.remove('hidden');
                els.posterPlaceholder.classList.add('hidden');
            } else {
                els.posterImg.classList.add('hidden');
                els.posterPlaceholder.classList.remove('hidden');
            }
        });
        els.posterImg.onerror = () => {
            els.posterImg.classList.add('hidden');
            els.posterPlaceholder.classList.remove('hidden');
        };
    }

    // ==================== YÖNETMEN SEARCH ====================
    if (els.dInput) {
        els.dInput.addEventListener('input', async function () {
            const query = this.value.trim();
            if (query.length < 2) { els.dResultsContainer.style.display = 'none'; return; }
            document.querySelector('.search-term-display').innerText = query;
            try {
                const res = await fetch(`/Admin/Admin/DirectorSearch?query=${encodeURIComponent(query)}`);
                if (res.ok) {
                    const data = await res.json();
                    els.dResultsList.innerHTML = '';
                    data.forEach(item => {
                        const div = document.createElement('div');
                        div.className = 'flex items-center gap-3 p-3 cursor-pointer hover:bg-yellow-500 hover:text-black transition text-sm dark:text-white border-b border-slate-100 dark:border-white/5 last:border-0';
                        div.innerHTML = `<img src="${item.photoUrl || '/profile-images/default-profile.png'}" class="w-8 h-8 rounded-full object-cover bg-slate-200 dark:bg-white/10"> ${item.name}`;
                        div.onclick = () => selectDirector(item.id, item.name, item.photoUrl, false);
                        els.dResultsList.appendChild(div);
                    });
                    els.dResultsContainer.style.display = 'block';
                }
            } catch (e) { console.error(e); }
        });

        els.dAddNewTrigger.onclick = () => {
            els.dResultsContainer.style.display = 'none';
            els.dManual.classList.remove('hidden');
            document.getElementById('new-d-name').value = els.dInput.value;
        };
    }

    // ==================== OYUNCU SEARCH ====================
    if (els.aInput) {
        els.aInput.addEventListener('input', async function () {
            const query = this.value.trim();
            if (query.length < 2) { els.aResultsContainer.style.display = 'none'; return; }
            document.querySelector('.search-term-display-actor').innerText = query;
            try {
                const res = await fetch(`/Admin/Admin/ActorSearch?query=${encodeURIComponent(query)}`);
                if (res.ok) {
                    const data = await res.json();
                    els.aResultsList.innerHTML = '';
                    data.forEach(item => {
                        const div = document.createElement('div');
                        div.className = 'flex items-center gap-3 p-3 cursor-pointer hover:bg-yellow-500 hover:text-black transition text-sm dark:text-white border-b border-slate-100 dark:border-white/5 last:border-0';
                        div.innerHTML = `<img src="${item.photoUrl || '/profile-images/default-profile.png'}" class="w-8 h-8 rounded-full object-cover bg-slate-200 dark:bg-white/10"> ${item.name}`;
                        div.onclick = () => addActor(item.id, item.name, item.photoUrl, true);
                        els.aResultsList.appendChild(div);
                    });
                    els.aResultsContainer.style.display = 'block';
                }
            } catch (e) { console.error(e); }
        });

        els.aAddNewTrigger.onclick = () => {
            els.aResultsContainer.style.display = 'none';
            els.aManual.classList.remove('hidden');
            document.getElementById('new-a-name').value = els.aInput.value;
        };
    }

    // ====================== FONKSİYONLAR ======================
    window.selectDirector = (id, name, photo, isNew, height = null, birthDate = null, birthPlace = null) => {
        document.getElementById('ExistingDirectorId').value = id || '';
        document.getElementById('ExistingDirectorName').value = isNew ? '' : name;
        document.getElementById('DirectorName').value = isNew ? name : '';
        document.getElementById('DirectorPhotoUrl').value = photo || '';
        document.getElementById('DirectorBirthPlace').value = birthPlace || '';
        document.getElementById('DirectorHeightCm').value = height || '';
        document.getElementById('DirectorBirthDate').value = birthDate || '';

        const info = [height ? height + ' cm' : '', birthPlace].filter(Boolean).join(' • ');
        els.dCard.innerHTML = `
            <div class="flex items-center gap-3">
                <img src="${photo || '/profile-images/default-profile.png'}" class="w-10 h-10 rounded-full border-2 border-yellow-500/50 object-cover">
                <div>
                    <p class="font-bold text-sm">${name}</p>
                    <div class="flex items-center gap-2">
                        <p class="text-[9px] text-yellow-600 uppercase font-black">${isNew ? 'YENİ' : 'MEVCUT'}</p>
                        ${info ? `<span class="text-[9px] text-slate-400">•</span><p class="text-[9px] text-slate-400">${info}</p>` : ''}
                    </div>
                </div>
            </div>
            <button type="button" onclick="removeDirector()" class="text-red-500 p-2 hover:bg-slate-100 dark:hover:bg-white/5 rounded-full">
                <span class="material-symbols-outlined">close</span>
            </button>`;
        els.dCard.classList.remove('hidden'); els.dCard.classList.add('flex');
        els.dArea.classList.add('hidden'); els.dManual.classList.add('hidden');
        els.dResultsContainer.style.display = 'none';
    };

    window.removeDirector = () => {
        ['ExistingDirectorId', 'ExistingDirectorName', 'DirectorName', 'DirectorPhotoUrl', 'DirectorBirthPlace', 'DirectorHeightCm', 'DirectorBirthDate']
            .forEach(id => { const el = document.getElementById(id); if (el) el.value = ''; });
        els.dCard.classList.replace('flex', 'hidden');
        els.dArea.classList.remove('hidden');
        els.dInput.value = '';
    };

    window.setNewDirector = () => {
        const name = document.getElementById('new-d-name').value.trim();
        if (!name) return alert("Yönetmen adı giriniz");
        const photo = document.getElementById('new-d-photo').value.trim();
        const place = document.getElementById('new-d-birthplace').value.trim();
        const height = document.getElementById('new-d-height').value.trim();
        const date = document.getElementById('new-d-birthdate').value.trim();
        selectDirector(null, name, photo, true, height, date, place);
    };

    window.addActor = (id, name, photo, isExisting, height = '', birthDate = '', birthPlace = '') => {
        const div = document.createElement('div');
        div.className = 'actor-item group flex items-center justify-between p-3 rounded-xl bg-slate-50 dark:bg-white/5 border border-slate-200 dark:border-white/5 hover:border-yellow-500/30 transition-all';
        const info = [height ? height + ' cm' : '', birthPlace].filter(Boolean).join(' • ');
        div.innerHTML = `
            <div class="flex items-center gap-3">
                <img src="${photo || '/profile-images/default-profile.png'}" class="w-10 h-10 rounded-full object-cover">
                <div>
                    <p class="text-sm font-bold">${name}</p>
                    <div class="flex items-center gap-2">
                        <p class="text-[9px] text-slate-400 uppercase">${isExisting ? 'KAYITLI' : 'YENİ'}</p>
                        ${info ? `<span class="text-[9px]">•</span><p class="text-[9px] text-slate-400">${info}</p>` : ''}
                    </div>
                </div>
            </div>
            <button type="button" class="remove-actor text-red-500 p-2 hover:bg-slate-200 dark:hover:bg-white/5 rounded-full">
                <span class="material-symbols-outlined">delete</span>
            </button>
            <input type="hidden" class="actor-id-input" value="${id || ''}" />
            <input type="hidden" class="actor-name-input" value="${name}" />
            <input type="hidden" class="actor-photo-input" value="${photo || ''}" />
            <input type="hidden" class="actor-height-input" value="${height}" />
            <input type="hidden" class="actor-birthplace-input" value="${birthPlace}" />
            <input type="hidden" class="actor-birthdate-input" value="${birthDate}" />
        `;
        div.querySelector('.remove-actor').onclick = () => { div.remove(); reindexActors(); };
        els.castList.appendChild(div);
        reindexActors();
        els.aInput.value = ''; els.aResultsContainer.style.display = 'none'; els.aManual.classList.add('hidden');
    };

    window.setManualActor = () => {
        const name = document.getElementById('new-a-name').value.trim();
        if (!name) return alert("Oyuncu adı giriniz");
        const photo = document.getElementById('new-a-photo').value.trim();
        const place = document.getElementById('new-a-birthplace').value.trim();
        const height = document.getElementById('new-a-height').value.trim();
        const date = document.getElementById('new-a-birthdate').value.trim();
        addActor(null, name, photo, false, height, date, place);
    };

    window.closeManualForm = (type) => {
        if (type === 'director') els.dManual.classList.add('hidden');
        else els.aManual.classList.add('hidden');
    };

    function reindexActors() {
        const items = els.castList.querySelectorAll('.actor-item');
        els.castCount.innerText = items.length;
        items.forEach((item, i) => {
            item.querySelector('.actor-id-input').name = `Actors[${i}].ActorId`;
            item.querySelector('.actor-name-input').name = `Actors[${i}].Name`;
            item.querySelector('.actor-photo-input').name = `Actors[${i}].AvatarUrl`;
            item.querySelector('.actor-height-input').name = `Actors[${i}].Height`;
            item.querySelector('.actor-birthplace-input').name = `Actors[${i}].BirthPlace`;
            item.querySelector('.actor-birthdate-input').name = `Actors[${i}].BirthDate`;
        });
    }

   
});