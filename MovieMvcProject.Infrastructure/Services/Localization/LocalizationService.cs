
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using MovieMvcProject.Application.Interfaces.ILocalization;
using MovieMvcProject.Domain.Resources;
using System.Globalization;

namespace MovieMvcProject.Infrastructure.Services.Localization
{
    public sealed class LocalizationService : ILocalizationService
    {
        private readonly IStringLocalizerFactory _factory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Dictionary<string, IStringLocalizer> _localizers = new();
        private const string DefaultResourceName = "UserResource";

        public LocalizationService(
            IStringLocalizerFactory factory,
            IHttpContextAccessor httpContextAccessor)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

            // Kaynak dosyalarını (Resources) ön belleğe alıyoruz
            _localizers["EnumResource"] = _factory.Create(typeof(EnumResource));
            _localizers["ExceptionResource"] = _factory.Create(typeof(ExceptionResource));
            _localizers["MenuResource"] = _factory.Create(typeof(MenuResource));
            _localizers["MovieResource"] = _factory.Create(typeof(MovieResource));
            _localizers["RegisterResource"] = _factory.Create(typeof(RegisterResource));
            _localizers[DefaultResourceName] = _factory.Create(typeof(UserResource));
            _localizers["ValidationResource"] = _factory.Create(typeof(ValidationResource));
            System.Diagnostics.Debug.WriteLine("✅ LocalizationService initialized - ValidationResource registered");
        }

        
        // Mevcut aktif dili "tr" veya "en" formatında döner.
        
        public string GetCurrentLanguageCode()
        {
            //  İstek bazlı kültür özelliğini alma
            var feature = _httpContextAccessor.HttpContext?.Features.Get<IRequestCultureFeature>();

            //  Eğer feature yoksa sistemin o anki UICulture değerini alma
            var culture = feature?.RequestCulture.UICulture ?? CultureInfo.CurrentUICulture;

            //  İki harfli ISO kodunu alma (Örn: "tr-TR" -> "tr")
            var langCode = culture.TwoLetterISOLanguageName.ToLowerInvariant();

            //  Sadece desteklenen dilleri filtreleme (Güvenlik duvarı)
            return langCode switch
            {
                "tr" => "tr",
                "en" => "en",
                _ => "tr" // Bilinmeyen bir dil gelirse varsayılan olarak Türkçe dönme
            };
        }

        public LocalizedString GetLocalizedString(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return new LocalizedString("EmptyKey", string.Empty, true);

            return _localizers.TryGetValue(DefaultResourceName, out var localizer)
                ? localizer[key]
                : new LocalizedString(key, $"[Missing: {DefaultResourceName}]", true);
        }

        

        public LocalizedString GetLocalizedHtmlString(string resourceName, string key)
        {
            if (string.IsNullOrWhiteSpace(resourceName) || string.IsNullOrWhiteSpace(key))
                return new LocalizedString(key ?? "Unknown", key ?? "Unknown", true);

            // Aktif kültürü alma
            var currentCulture = System.Globalization.CultureInfo.CurrentUICulture;

            if (_localizers.TryGetValue(resourceName, out var localizer))
            {
                // Localizer'ın o anki kültürdeki karşılığını döndürme
                return localizer[key];
            }

            return new LocalizedString(key, $"[Missing: {resourceName}]", true);
        }

        public IStringLocalizer GetLocalizer(string resourceName)
        {
            if (string.IsNullOrWhiteSpace(resourceName))
                throw new ArgumentNullException(nameof(resourceName));

            return _localizers.TryGetValue(resourceName, out var localizer)
                ? localizer
                : throw new KeyNotFoundException($"'{resourceName}' localizer is not registered.");
        }
    }
}