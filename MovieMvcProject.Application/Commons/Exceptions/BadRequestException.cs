

using Microsoft.Extensions.Localization;

namespace MovieMvcProject.Application.Commons.Exceptions
{
    public class BadRequestException : Exception
    {
        public IDictionary<string, string[]>? Errors { get; }

        public BadRequestException(string message, IStringLocalizer? localizer = null)
            : base(localizer?["BadRequest"] ?? message)
        {
        }

        public BadRequestException(IDictionary<string, string[]> errors, IStringLocalizer? localizer = null)
            : base(localizer?["BadRequest"] ?? "One or more invalid requests have been made.")
        {
            Errors = errors;
        }
    }
}
