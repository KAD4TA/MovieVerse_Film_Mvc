
using AutoMapper;
using MovieMvcProject.Application.DTOs.ResponseDto;

namespace MovieMvcProject.Application.Mapping.Resolvers
{
    public class TranslationDescriptionResolver : IValueResolver<Movie, MovieDetailDto, string>
    {
        public string Resolve(Movie source, MovieDetailDto destination, string destMember, ResolutionContext context)
        {
            // 1. Dil Kodunu Güvenli Şekilde Alma
            string languageCode = "tr"; 

            // TryGetItems koleksiyonun başlatılıp başlatılmadığını kontrol eder
            if (context.TryGetItems(out var items) && items.TryGetValue("LanguageCode", out var langObj))
            {
                languageCode = langObj?.ToString() ?? "tr";
            }

            // 2. Çeviriyi Arama
            var translation = source.Translations?.FirstOrDefault(t =>
                t.LanguageCode.Equals(languageCode, System.StringComparison.OrdinalIgnoreCase));

            // 3. Sonucu Dönme (Çeviri yoksa veya boşsa ana entity'deki DescriptionTr'yi dönecek)
            if (translation != null && !string.IsNullOrWhiteSpace(translation.Description))
            {
                return translation.Description;
            }

            return source.DescriptionTr ?? string.Empty;
        }
    }
}