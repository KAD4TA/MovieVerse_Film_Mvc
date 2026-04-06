
using Microsoft.Extensions.Localization;

namespace MovieMvcProject.Application.Commons.Exceptions
{
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message, IStringLocalizer? localizer = null)
            : base(localizer?["Unauthorized"] ?? message)
        {
        }
    }
}
