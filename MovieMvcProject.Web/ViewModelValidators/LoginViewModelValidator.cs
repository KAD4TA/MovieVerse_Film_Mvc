using FluentValidation;
using MovieMvcProject.Application.Interfaces.ILocalization;
using MovieMvcProject.Web.Models;


namespace MovieMvcProject.Web.ViewModelValidators
{
    public class LoginViewModelValidator : AbstractValidator<LoginViewModel>
    {

        public LoginViewModelValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(localizationService.GetLocalizedHtmlString(
                    "ValidationResource", "EmailRequired"))
                .EmailAddress().WithMessage(localizationService.GetLocalizedHtmlString(
                    "ValidationResource", "EmailInvalid"));

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(localizationService.GetLocalizedHtmlString(
                    "ValidationResource", "PasswordRequired"))
                .MinimumLength(6).WithMessage(localizationService.GetLocalizedHtmlString(
                    "ValidationResource", "PasswordMinLength"));


        }
    }
}
