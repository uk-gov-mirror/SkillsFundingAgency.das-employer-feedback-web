using FluentValidation;
using SFA.DAS.EmployerFeedback.Domain.Types;
using SFA.DAS.EmployerFeedback.Web.Models.Questions;

namespace SFA.DAS.EmployerFeedback.Web.Validators.Questions
{
    public class QuestionThreeRatingViewModelValidator : AbstractValidator<QuestionThreeRatingViewModel>
    {
        public QuestionThreeRatingViewModelValidator()
        {
            RuleFor(x => x.Rating)
                .ValidateRating();
        }
    }

    public static class QuestionThreeRatingViewModelValidatorRules
    {
        public static IRuleBuilderOptionsConditions<T, ProviderRating?> ValidateRating<T>(this IRuleBuilder<T, ProviderRating?> ruleBuilder)
            where T : QuestionThreeRatingViewModel
        {
            return ruleBuilder.Custom((value, context) =>
            {
                var model = context.InstanceToValidate;
                if (model.Rating == null)
                {
                    context.AddFailure("Rating", $"Please rate {model.ProviderName}");
                }
            });
        }
    }
}
