
using Microsoft.AspNetCore.Mvc.Rendering;
using MovieMvcProject.Application.Interfaces.ILocalization;


namespace MovieMvcProject.Web.Helpers
{
    public static class EnumSelectListHelper
    {
        
        public static IEnumerable<SelectListItem> GetLocalizedEnumSelectList<TEnum>(
    this ILocalizationService localizationService,
    string selectedValue = null)
    where TEnum : struct, Enum
        {
            var enumTypeName = typeof(TEnum).Name;

            var items = Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(e =>
                {
                    var key = $"{enumTypeName}_{e}";
                    var text = localizationService.GetLocalizedHtmlString("EnumResource", key).Value;

                    if (string.IsNullOrEmpty(text) || text.Contains("[Missing"))
                        text = e.ToString();

                    return new SelectListItem
                    {
                        Value = e.ToString(),
                        Text = text,
                        Selected = string.Equals(selectedValue, e.ToString(), StringComparison.OrdinalIgnoreCase)
                    };
                })
                .ToList();

            

            return items;
        }
    }
}
