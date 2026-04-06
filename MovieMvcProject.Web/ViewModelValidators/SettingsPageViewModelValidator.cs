
using FluentValidation;
using MovieMvcProject.Application.Common;
using MovieMvcProject.Application.Interfaces.ILocalization;
using MovieMvcProject.Web.Models;

namespace MovieMvcProject.Web.ViewModelValidators
{
    public class SettingsPageViewModelValidator
        : AbstractValidator<SettingsPageViewModel>
    {
        private readonly ILocalizationService _localizer;

        public SettingsPageViewModelValidator(
            ILocalizationService localizationService)
        {
            _localizer = localizationService;

            RuleFor(x => x.FullName)
                .NotEmpty()
                .WithMessage(_localizer.GetLocalizedHtmlString(
                    "ValidationResource", "FullNameRequired"));

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage(_localizer.GetLocalizedHtmlString(
                    "ValidationResource", "EmailRequired"))
                .EmailAddress()
                .WithMessage(_localizer.GetLocalizedHtmlString(
                    "ValidationResource", "EmailInvalid"));

            // 🔐 Şifre kuralları (sadece yeni şifre varsa)
            When(x => !string.IsNullOrWhiteSpace(x.NewPassword), () =>
            {
                RuleFor(x => x.CurrentPassword)
                    .NotEmpty()
                    .WithMessage(_localizer.GetLocalizedHtmlString(
                        "ValidationResource", "CurrentPasswordRequired"))
                    .MinimumLength(8)
                    .WithMessage(_localizer.GetLocalizedHtmlString(
                        "ValidationResource", "PasswordMinLength"));

                RuleFor(x => x.NewPassword)
                    .MinimumLength(8)
                    .WithMessage(_localizer.GetLocalizedHtmlString(
                        "ValidationResource", "PasswordMinLength"))
                    .Matches(ValidationHelpers.StrongPasswordRegex)
                    .WithMessage(_localizer.GetLocalizedHtmlString(
                        "ValidationResource", "PasswordComplexity"))
                    .Must(ValidationHelpers.NotCommonPassword)
                    .WithMessage(_localizer.GetLocalizedHtmlString(
                        "ValidationResource", "CommonPassword"));

                RuleFor(x => x.ConfirmNewPassword)
                    .Equal(x => x.NewPassword)
                    .WithMessage(_localizer.GetLocalizedHtmlString(
                        "ValidationResource", "PasswordsMismatch"));
            });

            RuleFor(x => x.Gender)
                .IsInEnum()
                .WithMessage(_localizer.GetLocalizedHtmlString(
                    "ValidationResource", "GenderInvalid"));

            
            RuleFor(x => x.ProfileImageFile)
                .Must(ValidationHelpers.BeValidFile)
                .WithMessage(_localizer.GetLocalizedHtmlString(
                    "ValidationResource", "ProfileImageInvalid"));

            
            RuleFor(x => x.BirthDate)
                .Must(d => !d.HasValue || d.Value < DateTime.Today)
                .WithMessage(_localizer.GetLocalizedHtmlString(
                    "ValidationResource", "BirthDateInvalid"));

            RuleFor(x => x.InstagramUrl)
                .Must(ValidationHelpers.BeAValidUrl)
                .When(x => !string.IsNullOrWhiteSpace(x.InstagramUrl))
                .WithMessage(_localizer.GetLocalizedHtmlString(
                    "ValidationResource", "InvalidUrl"));

            RuleFor(x => x.TwitterUrl)
                .Must(ValidationHelpers.BeAValidUrl)
                .When(x => !string.IsNullOrWhiteSpace(x.TwitterUrl))
                .WithMessage(_localizer.GetLocalizedHtmlString(
                    "ValidationResource", "InvalidUrl"));
        }
    }
}
