$(document).ready(function () {
    
    const config = window.aiConfig;

    // UI Elementleri
    const $input = $('#aiSearchInput');
    const $btn = $('#btnAskAI');
    const $dropdown = $('#aiDropdown');
    const $results = $('#movieResultsContainer');
    const $wrapper = $('#movieResultsWrapper');
    const $aiText = $('#aiResponseText');
    const $icon = $('#aiIcon');

    // Yatay Kaydırma Kontrolleri
    $('#scrollRight').on('click', function () { $wrapper.animate({ scrollLeft: '+=350' }, 400); });
    $('#scrollLeft').on('click', function () { $wrapper.animate({ scrollLeft: '-=350' }, 400); });

    /**
     * Ana Arama Fonksiyonu
     * @param {string} query - Kullanıcının girdiği metin
     */
    function performSearch(query) {
        if (!query || query.trim() === "") return;

        // 1. UI Hazırlığı (Loading Durumu)
        $icon.addClass('animate-pulse text-primary');
        $btn.prop('disabled', true).addClass('opacity-50');
        $aiText.text(config.localizer.analyzing);

        // Önceki sonuçları temizle ama dropdown'ı hemen kapatma (akıcılık için)
        $results.empty();

        // 2. AJAX İsteği
        $.post(config.urls.getSuggestions, { query: query })
            .done(function (data) {
                console.log("AI Response:", data); 

                $wrapper.scrollLeft(0);

                if (data && data.length > 0) {
                    // Veri geldiğinde kartları oluşturma
                    data.forEach(movie => {
                        const card = `
                            <div onclick="window.location.href='/Movie/${movie.movieId}'" class="group/card relative w-48 flex flex-col gap-3 cursor-pointer hover:-translate-y-1 transition-transform duration-300">
                                <div class="relative aspect-[2/3] w-full rounded-xl overflow-hidden shadow-lg border border-white/5">
                                    <div class="absolute top-2 right-2 bg-black/80 backdrop-blur-sm text-primary text-xs font-bold px-2 py-1 rounded-full flex items-center gap-1 z-10">
                                        <span class="material-symbols-outlined text-[14px]">star</span> ${movie.rating || '0.0'}
                                    </div>
                                    <img src="${movie.posterUrl || '/img/no-poster.jpg'}" 
                                         class="absolute inset-0 w-full h-full object-cover transition-transform duration-700 group-hover/card:scale-110" 
                                         onerror="this.src='/img/no-poster.jpg'" />
                                    <div class="absolute bottom-0 left-0 right-0 p-3">
                                        <button class="w-full py-2 bg-primary text-white text-xs font-bold rounded-lg opacity-0 group-hover/card:opacity-100 translate-y-2 group-hover/card:translate-y-0 transition-all">
                                            ${config.localizer.inspectButton}
                                        </button>
                                    </div>
                                </div>
                                <div>
                                    <h3 class="text-white font-bold text-sm truncate">${movie.title}</h3>
                                    <p class="text-gray-500 text-xs">${movie.year}</p>
                                </div>
                            </div>`;
                        $results.append(card);
                    });

                    $aiText.text(config.localizer.analysisComplete);
                    $dropdown.removeClass('hidden').addClass('flex opacity-100 translate-y-0');
                } else {
                    // Veri boş döndüyse (200 OK ama sonuç yok)
                    toastr.info(config.localizer.noMoviesFoundAI);
                    $aiText.text(config.localizer.noMoviesFoundAI);
                    $dropdown.addClass('hidden');
                }
            })
            .fail(function (xhr) {
                // 3. Hata Yönetimi
                console.error("Search Error:", xhr);

                if (xhr.status === 429) {
                    // Rate Limit Hatası
                    toastr.warning(config.localizer.rateLimitWarning);
                    $aiText.text(config.localizer.rateLimitAIResponse);
                } else if (xhr.status === 404) {
                    // Bulunamadı Hatası
                    toastr.info(config.localizer.noMoviesFoundAI);
                    $aiText.text(config.localizer.noMoviesFoundAI);
                } else {
                    // Genel Bağlantı Hatası
                    toastr.error(config.localizer.connectionError);
                    $aiText.text("Bir hata oluştu.");
                }

                $dropdown.addClass('hidden');
            })
            .always(function () {
                // 4. UI Reset (İşlem bitince)
                $icon.removeClass('animate-pulse');
                $btn.prop('disabled', false).removeClass('opacity-50');
            });
    }

    // --- Event Listeners ---

    // Buton Tıklama
    $btn.on('click', function () {
        performSearch($input.val());
    });

    // Enter Tuşu
    $input.on('keypress', function (e) {
        if (e.which === 13) {
            performSearch($(this).val());
        }
    });

    // Hazır Öneri Çipleri
    $('.suggestion-chip').on('click', function () {
        const q = $(this).data('query');
        $input.val(q);
        performSearch(q);
    });
});