
using Microsoft.Extensions.Localization;

namespace MovieMvcProject.Application.Commons.Exceptions
{
    public class InternalServerException : Exception
    {
        public InternalServerException(string message, IStringLocalizer? localizer = null)
            : base(localizer?["InternalError"] ?? message)
        {
        }


        public InternalServerException(string message, Exception? innerException, IStringLocalizer? localizer = null)
            : base(localizer?["InternalError"] ?? message, innerException)
        {
        }
    }
}
