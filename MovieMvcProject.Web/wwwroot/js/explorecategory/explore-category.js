document.addEventListener("DOMContentLoaded", function () {

   

    // 1. Kartlara tıklama
    const clickableCards = document.querySelectorAll('.clickable-card');
    clickableCards.forEach(card => {
        card.addEventListener('click', function () {
            const url = this.getAttribute('data-url');
            if (url) window.location.href = url;
        });
    });

    // 2. Ok butonları (Slider)
    const slider = document.getElementById('trendingList');
    const nextBtn = document.getElementById('slideNext');
    const prevBtn = document.getElementById('slidePrev');

    if (slider && nextBtn && prevBtn) {
        console.log("✅ Slider butonları bulundu!"); 

        const scrollStep = 320; 

        nextBtn.addEventListener('click', () => {
            slider.scrollBy({ left: scrollStep, behavior: 'smooth' });
        });

        prevBtn.addEventListener('click', () => {
            slider.scrollBy({ left: -scrollStep, behavior: 'smooth' });
        });
    } else {
        console.error("❌ Slider veya butonlar bulunamadı!");
    }
});