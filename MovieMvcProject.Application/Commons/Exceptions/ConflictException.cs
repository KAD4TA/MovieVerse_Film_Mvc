

using Microsoft.Extensions.Localization;

namespace MovieMvcProject.Application.Commons.Exceptions
{
    public class ConflictException : Exception
    {
        public ConflictException(string message, IStringLocalizer? MovieMvcProject = null)
            : base(MovieMvcProject?["Conflict"] ?? message)
        {
        }
    }
}
