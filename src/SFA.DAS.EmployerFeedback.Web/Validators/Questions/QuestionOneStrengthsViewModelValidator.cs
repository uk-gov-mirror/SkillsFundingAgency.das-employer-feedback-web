using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using SFA.DAS.EmployerFeedback.Web.Models.Questions;

namespace SFA.DAS.EmployerFeedback.Web.Validators.Questions
{
    public class QuestionOneStrengthsViewModelValidator : AbstractValidator<QuestionOneStrengthsViewModel>
    {
        public QuestionOneStrengthsViewModelValidator()
        {
            RuleFor(x => x.Attributes)
                .ValidateAttributes();
        }
    }

    public static class QuestionOneStrengthsViewModelValidatorRules
    {
        public static IRuleBuilderOptionsConditions<T, List<ProviderAttributeModel>> ValidateAttributes<T>(this IRuleBuilder<T, List<ProviderAttributeModel>> ruleBuilder)
            where T : QuestionOneStrengthsViewModel
        {
            return ruleBuilder.Custom((value, context) =>
            {
                var model = context.InstanceToValidate;

                if(model.Attributes.Count(a => a.Good) > 3)
                {
                    context.AddFailure($"Attributes", $"Choose up to 3 options");

                    // add individual errors to ensure that array elements are not discarded from AttemptedValues
                    var strengths = model.Attributes.Select((a, i) => new { a, i }).Where(x => x.a.Good).ToList();
                    strengths.ForEach((good) => { context.AddFailure($"Attributes[{good.i}].Good", $"Choose up to 3 options"); });
                }
            });
        }
    }
}
