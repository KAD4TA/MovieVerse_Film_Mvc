const EMPTY_GUID = '00000000-0000-0000-0000-000000000000';

// Global Fonksiyonlar (Razor sayfasından ve inline onclick'lerden erişilebilmesi için)
window.setDirector = (id, name, photo, isNew) => {
    document.getElementById('hfDirectorId').value = id;
    document.getElementById('hfNewDirectorName').value = isNew ? name : '';
    document.getElementById('hfNewDirectorPhoto').value = photo || '';
    document.getElementById('d-name').textContent = name;
    document.getElementById('d-img').src = photo || '/profile-images/default-profile.png';
    document.getElementById('d-status').textContent = isNew ? "Yeni" : "Sistem";
    document.getElementById('director-selected').classList.remove('hidden');
    document.getElementById('director-search-container').classList.add('hidden');
};

window.resetDirector = () => {
    document.getElementById('hfDirectorId').value = EMPTY_GUID;
    document.getElementById('director-selected').classList.add('hidden');
    document.getElementById('director-search-container').classList.remove('hidden');
};

window.openManualDirector = () => {
    document.getElementById('manual-d-name').value = document.getElementById('d-input').value;
    document.getElementById('manual-director-form').classList.remove('hidden');
    document.getElementById('d-results').classList.add('hidden');
};

window.confirmManualDirector = () => {
    const n = document.getElementById('manual-d-name').value;
    const p = document.getElementById('manual-d-photo').value;
    if (n) setDirector(EMPTY_GUID, n, p, true);
    document.getElementById('manual-director-form').classList.add('hidden');
};

window.addActorCard = (id, name, photo) => {
    const list = document.getElementById('actor-list');
    const div = document.createElement('div');
    div.className = "actor-item flex items-center justify-between p-3 bg-white dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700";
    div.innerHTML = `
        <div class="flex items-center gap-3">
            <img src="${photo || '/profile-images/default-profile.png'}" class="w-10 h-10 rounded-full object-cover">
            <div><p class="text-xs font-black">${name}</p></div>
        </div>
        <button type="button" onclick="this.parentElement.remove(); reindex();" class="text-red-400">
            <span class="material-symbols-outlined text-sm">close</span>
        </button>
        <input type="hidden" class="a-id" value="${id}">
        <input type="hidden" class="a-name" value="${name}">
        <input type="hidden" class="a-photo" value="${photo || ''}">
    `;
    list.appendChild(div);
    reindex();
};

window.reindex = () => {
    document.querySelectorAll('.actor-item').forEach((item, i) => {
        item.querySelector('.a-id').name = `Actors[${i}].ActorId`;
        item.querySelector('.a-name').name = `Actors[${i}].Name`;
        item.querySelector('.a-photo').name = `Actors[${i}].AvatarUrl`;
    });
};

window.openManualActor = () => {
    document.getElementById('manual-a-name').value = document.getElementById('a-input').value;
    document.getElementById('manual-actor-form').classList.remove('hidden');
    document.getElementById('a-results').classList.add('hidden');
};

window.confirmManualActor = () => {
    const n = document.getElementById('manual-a-name').value;
    const p = document.getElementById('manual-a-photo').value;
    if (n) addActorCard(EMPTY_GUID, n, p);
    document.getElementById('manual-actor-form').classList.add('hidden');
    document.getElementById('a-input').value = '';
};

// DOM Yüklendikten Sonra Çalışacak Event Listener'lar
document.addEventListener('DOMContentLoaded', function () {
    // Poster Preview
    const pinp = document.getElementById('PosterUrlInput');
    const pimg = document.getElementById('poster-preview');
    pinp.addEventListener('input', () => {
        if (pinp.value) {
            pimg.src = pinp.value;
            pimg.classList.remove('hidden');
        } else {
            pimg.classList.add('hidden');
        }
    });

    const setupSearch = (inputId, resultsId, listId, qClass, type) => {
        const inp = document.getElementById(inputId);
        const res = document.getElementById(resultsId);
        const list = document.getElementById(listId);

        inp.addEventListener('input', async () => {
            const q = inp.value.trim();
            if (q.length < 2) {
                res.classList.add('hidden');
                return;
            }
            document.querySelectorAll('.' + qClass).forEach(el => el.textContent = q);
            res.classList.remove('hidden');
            try {
                const response = await fetch(`/Admin/Admin/${type}Search?query=${encodeURIComponent(q)}`);
                const data = await response.json();
                list.innerHTML = '';
                data.forEach(item => {
                    const d = document.createElement('div');
                    d.className = "search-item";
                    d.innerHTML = `<img src="${item.photoUrl || '/profile-images/default-profile.png'}" class="w-8 h-8 rounded-full object-cover"><span class="text-sm font-bold">${item.name}</span>`;
                    d.onclick = () => {
                        if (type === 'Director') setDirector(item.id, item.name, item.photoUrl, false);
                        else addActorCard(item.id, item.name, item.photoUrl);
                        res.classList.add('hidden');
                        inp.value = '';
                    };
                    list.appendChild(d);
                });
            } catch (e) { console.error("Arama hatası:", e); }
        });
    };

    setupSearch('d-input', 'd-results', 'd-list', 'q-text', 'Director');
    setupSearch('a-input', 'a-results', 'a-list', 'q-text-a', 'Actor');
});

// Sayfa geneli tıklamalar
window.onclick = (e) => {
    if (!e.target.closest('#d-input')) {
        const dRes = document.getElementById('d-results');
        if (dRes) dRes.classList.add('hidden');
    }
    if (!e.target.closest('#a-input')) {
        const aRes = document.getElementById('a-results');
        if (aRes) aRes.classList.add('hidden');
    }
};