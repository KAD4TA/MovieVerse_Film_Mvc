

using FluentValidation;
using MovieMvcProject.Application.Interfaces.ILocalization;
using MovieMvcProject.Web.Models;

namespace MovieMvcProject.Web.ViewModelValidators
{
    public class EmailViewModelValidator : AbstractValidator<EmailViewModel>
    {
        private readonly ILocalizationService _localizationService;

        public EmailViewModelValidator(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
            RuleFor(x => x.Email)
           
            .NotEmpty().WithMessage(x => _localizationService.GetLocalizedHtmlString("ValidationResource", "EmailRequired").Value)
            .EmailAddress().WithMessage(x => _localizationService.GetLocalizedHtmlString("ValidationResource", "EmailInvalid").Value);
        }
    }
}