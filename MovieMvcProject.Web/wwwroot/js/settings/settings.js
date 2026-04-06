$(document).ready(function () {
    const tabs = $(".settings-tab");
    const contents = $(".tab-content");
    const activeInput = $("#ActiveTab");

    function activate(tab) {
        tabs.each(function () {
            const isTarget = $(this).data("tab") === tab;
            $(this).toggleClass("bg-red-600 text-white", isTarget);
        });

        contents.each(function () {
            const isTarget = this.id === `tab-${tab}`;
            $(this).toggleClass("hidden", !isTarget);
        });

        if (activeInput.length) activeInput.val(tab);
    }

    const config = window.SettingsConfig || {};
    activate(config.activeTab || "profile");

    tabs.on("click", function (e) {
        e.preventDefault();
        activate($(this).data("tab"));
    });
});

// MODAL FONKSİYONLARI (Global Scope)
function showDeleteModal() {
    const modal = document.getElementById('deleteConfirmModal');
    if (modal) {
        modal.classList.remove('hidden');
        modal.classList.add('flex');
        document.body.style.overflow = 'hidden';
    }
}

function hideDeleteModal() {
    const modal = document.getElementById('deleteConfirmModal');
    if (modal) {
        modal.classList.add('hidden');
        modal.classList.remove('flex');
        document.body.style.overflow = 'auto';
    }
}

function executeDelete() {
    const form = document.getElementById('deleteAccountForm');
    if (form) {
        form.submit();
    }
}

function handleProfileImageChange(input) {
    const file = input.files[0];
    const errorEl = document.getElementById("imageError");
    const preview = document.getElementById("profileImagePreview");

    if (!file) return;

    if (file.size > 5 * 1024 * 1024) {
        if (errorEl) {
            errorEl.textContent = window.SettingsConfig.messages.imageSizeError;
            errorEl.classList.remove("hidden");
        }
        input.value = "";
        return;
    }

    const reader = new FileReader();
    reader.onload = function (e) {
        if (preview) preview.src = e.target.result;
    };
    reader.readAsDataURL(file);
}