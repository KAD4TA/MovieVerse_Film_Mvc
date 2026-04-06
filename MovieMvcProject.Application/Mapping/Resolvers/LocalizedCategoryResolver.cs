
using AutoMapper;
using Microsoft.Extensions.Localization;
using MovieMvcProject.Domain.Resources;
using System.Globalization;

namespace MovieMvcProject.Application.Mapping.Resolvers
{
    
    public class LocalizedCategoryResolver : IMemberValueResolver<object, object, string, string>
    {
        
        private readonly IStringLocalizer<EnumResource> _localizer;

        public LocalizedCategoryResolver(IStringLocalizer<EnumResource> localizer)
        {
            _localizer = localizer;
        }

        public string Resolve(object source, object destination, string sourceMember, string destMember, ResolutionContext context)
        {
            if (string.IsNullOrWhiteSpace(sourceMember))
                return string.Empty;

            // 1. Handler'dan (veya controller'dan) gönderilen dili alma
            var languageCode = context.Items["LanguageCode"] as string;

            // 2. Thread kültürünü geçici olarak ayarlama (Localizer'ın doğru dili bulması için)
            if (!string.IsNullOrWhiteSpace(languageCode))
            {
                var culture = new CultureInfo(languageCode);
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;
            }

            // 3. Kaynak değeri (örn: "Action") Resource dosyasında aratma ve çevirisini dönme
            return _localizer[sourceMember];
        }
    }
}