

using FluentValidation.Results; 
using Microsoft.Extensions.Localization;

namespace MovieMvcProject.Application.Commons.Exceptions
{
    public class ValidationException : Exception
    {
        
        public IDictionary<string, string[]> Errors { get; }

        
        public ValidationException(IStringLocalizer? localizer = null)
            : base(localizer?["Validation"].Value ?? "One or more validation failures have occurred.")
        {
            Errors = new Dictionary<string, string[]>();
        }

        
        public ValidationException(string propertyName, string errorMessage, IStringLocalizer? localizer = null)
            : base(localizer?["Validation"].Value ?? "One or more validation failures have occurred.")
        {
            Errors = new Dictionary<string, string[]>
            {
                { propertyName, new[] { errorMessage } }
            };
        }

        
        public ValidationException(IDictionary<string, string[]> errors, IStringLocalizer? localizer = null)
            : base(localizer?["Validation"].Value ?? "One or more validation failures have occurred.")
        {
            Errors = errors;
        }

        
        public ValidationException(IEnumerable<ValidationFailure> failures, IStringLocalizer? localizer = null)
            : base(localizer?["Validation"].Value ?? "One or more validation failures have occurred.")
        {
            if (failures == null)
            {
                Errors = new Dictionary<string, string[]>();
                return;
            }

           
            Errors = failures
                .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
        }

       
    }
}