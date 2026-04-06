

using Microsoft.Extensions.Localization;

namespace MovieMvcProject.Application.Commons.Exceptions
{
    public class ForbiddenAccessException : Exception
    {
        public ForbiddenAccessException(IStringLocalizer? localizer = null)
            : base(localizer?["Forbidden"] ?? "You do not have permission to access this resource.")
        {
        }
    }


}
