function removeFromWishlist(movieId) {
    const token = $('input[name="__RequestVerificationToken"]').val();

    $.ajax({
        url: '/Wishlist/Remove',
        type: 'POST',
        data: {
            movieId: movieId,
            __RequestVerificationToken: token
        },
        success: function (res) {
            if (res.success) {
                // Başarılı Toast
                Toastify({
                    text: res.message || "Film listenizden çıkarıldı ✓",
                    duration: 2800,
                    gravity: "top",
                    position: "right",
                    backgroundColor: "#10b981",
                    className: "toast-success",
                    stopOnFocus: true,
                    close: true,
                    offset: { x: 20, y: 80 }
                }).showToast();

                $(`#wishlist-item-${movieId}`).fadeOut(400, function () {
                    $(this).remove();

                    if ($('.wishlist-item').length === 0) {
                        setTimeout(() => location.reload(), 600);
                    }
                });
            } else {
                //  Hata Toast
                Toastify({
                    text: res.message || "Bir hata oluştu",
                    duration: 4000,
                    gravity: "top",
                    position: "right",
                    backgroundColor: "#ef4444",
                    className: "toast-error",
                    stopOnFocus: true,
                    close: true
                }).showToast();
            }
        },
        error: function () {
            Toastify({
                text: "İşlem gerçekleştirilemedi. Lütfen internet bağlantınızı kontrol edin.",
                duration: 4000,
                gravity: "top",
                position: "right",
                backgroundColor: "#ef4444",
                stopOnFocus: true,
                close: true
            }).showToast();
        }
    });
}