
using Microsoft.Extensions.Localization;

namespace MovieMvcProject.Application.Commons.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string? message) : base(message)
        {
        }

        public NotFoundException(string name, object key, IStringLocalizer? localizer = null)
            : base(localizer?["NotFound"] ?? $"{name} with identifier ({key}) was not found.")
        {
        }
    }


}
