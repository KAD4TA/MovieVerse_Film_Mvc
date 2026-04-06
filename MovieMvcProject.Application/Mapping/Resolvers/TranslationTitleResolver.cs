

using AutoMapper;
using MovieMvcProject.Application.Interfaces.ILocalization;

namespace MovieMvcProject.Application.Mapping.Resolvers
{
    
    public class TranslationTitleResolver<TSource, TDestination> : IMemberValueResolver<TSource, TDestination, Movie, string>
    {
        private readonly ILocalizationService _localizationService;

        public TranslationTitleResolver(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        public string Resolve(TSource source, TDestination destination, Movie sourceMember, string destMember, ResolutionContext context)
        {
            if (sourceMember == null) return "No Title Provided";

            string? lang = null;

            
            if (context.TryGetItems(out var items) && items.TryGetValue("LanguageCode", out var langObj))
            {
                lang = langObj?.ToString();
            }

            lang ??= _localizationService.GetCurrentLanguageCode() ?? "tr";

            if (sourceMember.Translations == null || !sourceMember.Translations.Any())
                return "No Title Provided";

            var translation = sourceMember.Translations.FirstOrDefault(t =>
                lang.StartsWith(t.LanguageCode, StringComparison.OrdinalIgnoreCase))
                ?? sourceMember.Translations.FirstOrDefault(t => t.LanguageCode.Equals("en", StringComparison.OrdinalIgnoreCase))
                ?? sourceMember.Translations.FirstOrDefault();

            return translation?.Title ?? "No Title Provided";
        }
    }
}
