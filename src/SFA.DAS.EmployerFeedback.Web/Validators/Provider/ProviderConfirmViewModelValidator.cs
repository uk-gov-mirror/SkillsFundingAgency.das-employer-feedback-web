using FluentValidation;
using SFA.DAS.EmployerFeedback.Web.Models.Provider;

namespace SFA.DAS.EmployerFeedback.Web.Validators
{
    public class ProviderConfirmViewModelValidator : AbstractValidator<ProviderConfirmViewModel>
    {
        public ProviderConfirmViewModelValidator()
        {
            RuleFor(x => x.Confirmed)
                .NotNull()
                .WithMessage("Please choose an option");
        }
    }
}
