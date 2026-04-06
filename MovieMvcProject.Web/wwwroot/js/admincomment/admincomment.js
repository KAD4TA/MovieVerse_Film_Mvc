document.addEventListener('DOMContentLoaded', () => {
    const selectAll = document.getElementById('select-all');
    const toolbar = document.getElementById('bulk-toolbar');
    const countSpan = document.getElementById('selected-count');
    const totalCountDisplay = document.getElementById('total-comments-count');

    // 1. Toplu Seçim Mantığı
    selectAll?.addEventListener('change', (e) => {
        document.querySelectorAll('.comment-checkbox:not(:disabled)').forEach(cb => {
            cb.checked = e.target.checked;
        });
        updateToolbar();
    });

    document.addEventListener('change', (e) => {
        if (e.target.classList.contains('comment-checkbox')) {
            updateToolbar();
            const allCheckboxes = Array.from(document.querySelectorAll('.comment-checkbox:not(:disabled)'));
            const allChecked = allCheckboxes.length > 0 && allCheckboxes.every(cb => cb.checked);
            if (selectAll) selectAll.checked = allChecked;
        }
    });

    function updateToolbar() {
        const checked = document.querySelectorAll('.comment-checkbox:checked');
        if (toolbar) {
            toolbar.classList.toggle('hidden', checked.length === 0);
            if (checked.length > 0) toolbar.classList.add('flex');
            countSpan.textContent = checked.length;
        }
    }

    // 2. Sayaç Güncelleme
    window.decreaseTotalCount = function (amount = 1) {
        if (!totalCountDisplay) return;
        let currentTotal = parseInt(totalCountDisplay.innerText.replace(/[^0-9]/g, '')) || 0;
        let newTotal = Math.max(0, currentTotal - amount);
        totalCountDisplay.innerText = newTotal;
    };

    // 3. Modal Yönetimi
    window.modalState = { isBulk: false, action: '', commentId: null, movieId: null };
    const modal = document.getElementById('confirmModal');
    const container = document.getElementById('modalContainer');

    window.openConfirmModal = function (isBulk, action, id = null, movie = null) {
        window.modalState = { isBulk, action, commentId: id, movieId: movie };

        const titleEl = document.getElementById('modalTitle');
        const desc = document.getElementById('modalDesc');
        const icon = document.getElementById('modalIcon');
        const iconBg = document.getElementById('modalIconBg');
        const btn = document.getElementById('modalConfirmBtn');

        // Modal içeriğini aksiyona göre dinamikleştirme
        if (action === 'delete') {
            titleEl.innerText = isBulk ? window.localizedStrings.bulkDeleteTitle : window.localizedStrings.singleDeleteTitle;
            desc.innerText = window.localizedStrings.deleteWarning;
            icon.innerText = "delete_forever";
            iconBg.className = "w-20 h-20 bg-red-500 rounded-full flex items-center justify-center mx-auto mb-6 animate-pulse";
            btn.className = "flex-1 py-4 bg-red-600 hover:bg-red-700 text-white font-bold rounded-2xl shadow-red-600/30 shadow-lg transition-all";
            btn.innerText = window.localizedStrings.yesDelete;
        } else if (action === 'approve') {
            titleEl.innerText = isBulk ? window.localizedStrings.bulkApproveTitle : window.localizedStrings.approveTitle;
            desc.innerText = window.localizedStrings.approveDescription;
            icon.innerText = "done_all";
            iconBg.className = "w-20 h-20 bg-green-500 rounded-full flex items-center justify-center mx-auto mb-6";
            btn.className = "flex-1 py-4 bg-green-600 hover:bg-green-700 text-white font-bold rounded-2xl shadow-green-600/30 shadow-lg transition-all";
            btn.innerText = window.localizedStrings.yesApprove;
        } else if (action === 'reject') {
            titleEl.innerText = isBulk ? window.localizedStrings.bulkRejectTitle : window.localizedStrings.rejectTitle;
            desc.innerText = window.localizedStrings.rejectDescription;
            icon.innerText = "block";
            iconBg.className = "w-20 h-20 bg-orange-500 rounded-full flex items-center justify-center mx-auto mb-6";
            btn.className = "flex-1 py-4 bg-orange-600 hover:bg-orange-700 text-white font-bold rounded-2xl shadow-orange-600/30 shadow-lg transition-all";
            btn.innerText = window.localizedStrings.yesReject;
        }

        modal.classList.remove('hidden');
        modal.classList.add('flex');
        setTimeout(() => {
            container.classList.remove('scale-95', 'opacity-0');
            container.classList.add('scale-100', 'opacity-100');
        }, 10);
    };

    window.closeConfirmModal = function () {
        container.classList.add('scale-95', 'opacity-0');
        setTimeout(() => {
            modal.classList.add('hidden');
            modal.classList.remove('flex');
        }, 200);
    };

    document.getElementById('modalConfirmBtn')?.addEventListener('click', () => {
        if (window.modalState.isBulk) {
            window.processBulkAction(window.modalState.action);
        } else {
            window.singleAction(window.modalState.commentId, window.modalState.action, window.modalState.movieId);
        }
        window.closeConfirmModal();
    });

    function getAntiForgeryToken() {
        return document.querySelector('#antiForgeryForm input[name="__RequestVerificationToken"]')?.value;
    }

    // 4. Tekli İşlem (Approve/Reject/Delete)
    window.singleAction = async function (commentId, action, movieId) {
        const formData = new FormData();
        formData.append('commentId', commentId);
        formData.append('movieId', movieId);
        formData.append('__RequestVerificationToken', getAntiForgeryToken());

        // Action 'delete' ise direkt silme endpointine, değilse Approve/Reject controller'ına
        const url = action === 'delete' ? `/Admin/AdminComments/Delete` : `/Admin/AdminComments/${action}`;

        try {
            const response = await fetch(url, { method: 'POST', body: formData });
            const result = await response.json();

            if (result.success) {
                iziToast.success({ title: 'Başarılı', message: result.message, position: 'topRight' });
                updateDOMForRow(commentId, action, movieId);
            } else {
                iziToast.error({ title: 'Hata', message: result.message || 'İşlem başarısız.', position: 'topRight' });
            }
        } catch (err) {
            iziToast.error({ title: 'Hata', message: 'Sunucu hatası oluştu.', position: 'topRight' });
        }
    };

    // 5. Toplu İşlem
    window.processBulkAction = async function (action) {
        const checkedBoxes = document.querySelectorAll('.comment-checkbox:checked');
        if (checkedBoxes.length === 0) return;

        const formData = new FormData();
        formData.append('action', action.toLowerCase());
        formData.append('__RequestVerificationToken', getAntiForgeryToken());

        checkedBoxes.forEach(cb => {
            const cid = cb.value;
            // HTML yapısındaki .movie-id-for- hidden inputu buluyoruz
            const mid = document.querySelector(`.movie-id-for-${cid}`)?.value;

            formData.append('selectedComments', cid);
            formData.append('selectedMovieIds', mid || "");
        });

        try {
            const response = await fetch(`/Admin/AdminComments/BulkAction`, { method: 'POST', body: formData });
            const result = await response.json();

            if (result.success) {
                iziToast.success({ title: 'Başarılı', message: result.message, position: 'topRight' });
                checkedBoxes.forEach(cb => {
                    const cid = cb.value;
                    const mid = document.querySelector(`.movie-id-for-${cid}`)?.value;
                    updateDOMForRow(cid, action, mid);
                });
                if (selectAll) selectAll.checked = false;
                updateToolbar();
            } else {
                iziToast.error({ title: 'Hata', message: result.message, position: 'topRight' });
            }
        } catch (err) {
            iziToast.error({ title: 'Hata', message: 'Toplu işlem sırasında hata.', position: 'topRight' });
        }
    };

    // 6. DOM Güncelleme (Sayfayı Yenilemeden Satırı Değiştirme)
    function updateDOMForRow(commentId, action, movieId) {
        const row = document.querySelector(`.comment-row[data-id="${commentId}"]`);
        if (!row) return;

        if (action.toLowerCase() === 'delete') {
            row.style.transition = "all 0.4s ease";
            row.style.opacity = '0';
            row.style.transform = "translateX(30px)";
            setTimeout(() => {
                row.remove();
                window.decreaseTotalCount(1);
            }, 400);
        } else {
            // Durum Badge'ini güncelle
            const statusCell = row.querySelector('.status-cell');
            if (statusCell) {
                let badgeClass = "";
                let badgeText = "";

                if (action === 'approve') {
                    badgeClass = "bg-green-500/10 text-green-600 border-green-500/20";
                    badgeText = window.localizedStrings.approvedStatus;
                } else {
                    badgeClass = "bg-orange-500/10 text-orange-600 border-orange-500/20";
                    badgeText = window.localizedStrings.rejectedStatus;
                }

                statusCell.innerHTML = `
                    <span class="${badgeClass} text-[10px] font-black uppercase tracking-widest px-3 py-1 rounded-full border">
                        ${badgeText}
                    </span>`;
            }

            // Butonları güncelle 
            const actionsContainer = row.querySelector('.actions-container');
            if (actionsContainer) {
                actionsContainer.innerHTML = `
                    <button onclick="openConfirmModal(false, 'delete', '${commentId}', '${movieId}')"
                            class="p-2 bg-red-500/10 text-red-600 dark:text-red-500 hover:bg-red-600 hover:text-white rounded-xl transition-all border border-red-500/20"
                            title="${window.localizedStrings.deleteTitleForBtn || 'Sil'}">
                        <span class="material-icons-outlined text-lg">delete_outline</span>
                    </button>`;
            }
        }
    }
});