

namespace MovieMvcProject.Domain.Enums.Extensions
{
    public static class EnumExtensions
    {
        
        public static string GetLocalizedName<TEnum>(this TEnum enumValue, Func<string, string> localizer)
    where TEnum : struct, Enum
        {
            if (localizer == null) return enumValue.ToString();

            var enumTypeName = typeof(TEnum).Name;
            var key = $"{enumTypeName}_{enumValue}";

            var result = localizer(key);

            return string.IsNullOrEmpty(result) || result.Contains($"{enumTypeName}_")
                ? enumValue.ToString()
                : result;
        }
    }
}