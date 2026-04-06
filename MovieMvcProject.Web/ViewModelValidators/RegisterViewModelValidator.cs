
using FluentValidation;
using MovieMvcProject.Application.Common;
using MovieMvcProject.Application.Interfaces.ILocalization;
using MovieMvcProject.Web.Models;


namespace MovieMvcProject.Web.ViewModelValidators
{
    
    public class RegisterViewModelValidator : AbstractValidator<RegisterViewModel>
    {
        private readonly ILocalizationService _localizationService;

        public RegisterViewModelValidator(ILocalizationService localizationService)
        {
            _localizationService = localizationService;

            

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(x => _localizationService.GetLocalizedHtmlString("ValidationResource", "EmailRequired").Value)
                .EmailAddress().WithMessage(x => _localizationService.GetLocalizedHtmlString("ValidationResource", "EmailInvalid").Value);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(x => _localizationService.GetLocalizedHtmlString("ValidationResource", "PasswordRequired").Value)
                .MinimumLength(8).WithMessage(x => _localizationService.GetLocalizedHtmlString("ValidationResource", "PasswordMinLength").Value)
                .Matches(ValidationHelpers.StrongPasswordRegex).WithMessage(x => _localizationService.GetLocalizedHtmlString("ValidationResource", "PasswordComplexity").Value)
                .Must(ValidationHelpers.NotCommonPassword).WithMessage(x => _localizationService.GetLocalizedHtmlString("ValidationResource", "CommonPassword").Value);

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage(x => _localizationService.GetLocalizedHtmlString("ValidationResource", "ConfirmPasswordRequired").Value)
                .Equal(x => x.Password).WithMessage(x => _localizationService.GetLocalizedHtmlString("ValidationResource", "PasswordsMismatch").Value);

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage(x => _localizationService.GetLocalizedHtmlString("ValidationResource", "FullNameRequired").Value);

            RuleFor(x => x.Gender)
                .IsInEnum().WithMessage(x => _localizationService.GetLocalizedHtmlString("ValidationResource", "GenderInvalid").Value);

            RuleFor(x => x.ProfileImageFile)
                .Must(ValidationHelpers.BeValidFile).WithMessage(x => _localizationService.GetLocalizedHtmlString("ValidationResource", "ProfileImageInvalid").Value)
                .When(x => x.ProfileImageFile != null);

            RuleFor(x => x.BirthDate)
                .LessThan(DateTime.Today).WithMessage(x => _localizationService.GetLocalizedHtmlString("ValidationResource", "BirthDateInvalid").Value)
                .When(x => x.BirthDate.HasValue);

            
            RuleFor(x => x.InstagramUrl)
                .Must(ValidationHelpers.BeAValidUrl).WithMessage(x => _localizationService.GetLocalizedHtmlString("ValidationResource", "InvalidUrl").Value)
                .When(x => !string.IsNullOrEmpty(x.InstagramUrl));

            RuleFor(x => x.TwitterUrl)
                .Must(ValidationHelpers.BeAValidUrl).WithMessage(x => _localizationService.GetLocalizedHtmlString("ValidationResource", "InvalidUrl").Value)
                .When(x => !string.IsNullOrEmpty(x.TwitterUrl));
        }
    }
}