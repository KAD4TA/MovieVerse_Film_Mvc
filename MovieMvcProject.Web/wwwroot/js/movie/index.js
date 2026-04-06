
document.addEventListener('DOMContentLoaded', () => {
    const container = document.getElementById('featuredMovieContainer');
    if (!container) return;

    const movies = JSON.parse(container.dataset.movies);
    const featuredMovieContent = document.getElementById('featuredMovieContent');
    const watchNowLink = document.getElementById('watchNowLink');

    let currentIndex = 0;
    const intervalTime = 5000;

   
    const yearLabel = container.dataset.yearLabel || "Yıl";

    function updateFeaturedMovie(index) {
        const movie = movies[index];
        const yearText = movie.Year || "2024";
        const ratingText = movie.Rating || "0.0";

        container.style.backgroundImage = `linear-gradient(to top, rgba(17,24,39,1) 0%, rgba(17,24,39,0.3) 50%, rgba(17,24,39,0.1) 100%), url('${movie.PosterUrl}')`;

        featuredMovieContent.innerHTML = `
            <h1 class="text-white text-4xl md:text-6xl font-black mb-4 drop-shadow-lg animate-fade-in">
                ${movie.Title}
            </h1>
            <p class="text-gray-300 text-lg md:text-xl mb-6 animate-fade-in">
                ${yearLabel}: ${yearText}   ★ ${ratingText}
            </p>
        `;
      
        watchNowLink.href = `/Movie/${movie.MovieId}`;
    }

    if (movies.length > 1) {
        setInterval(() => {
            currentIndex = (currentIndex + 1) % movies.length;
            updateFeaturedMovie(currentIndex);
        }, intervalTime);
    }
});