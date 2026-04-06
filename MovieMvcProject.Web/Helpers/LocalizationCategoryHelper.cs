using Microsoft.Extensions.Localization;

namespace MovieMvcProject.Web.Helpers
{
    public static class LocalizationCategoryHelper
    {
        public static string GetLocalizedCategory(this IStringLocalizer localizer, string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName)) return string.Empty;
            return localizer[$"Category_{categoryName}"];
        }
    }
}
