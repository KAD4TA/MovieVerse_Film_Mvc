

using Microsoft.Extensions.Localization;

namespace MovieMvcProject.Application.Interfaces.ILocalization
{
    public interface ILocalizationService
    {
        LocalizedString GetLocalizedString(string key);
        LocalizedString GetLocalizedHtmlString(string resourceName, string key);
        IStringLocalizer GetLocalizer(string resourceName);
        
        string GetCurrentLanguageCode();
    }
}