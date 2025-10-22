using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using SFA.DAS.EmployerFeedback.Web.Models.Questions;

namespace SFA.DAS.EmployerFeedback.Web.Validators.Questions
{
    public class QuestionTwoWeaknessesViewModelValidator : AbstractValidator<QuestionTwoWeaknessesViewModel>
    {
        public QuestionTwoWeaknessesViewModelValidator()
        {
            RuleFor(x => x.Attributes)
                .ValidateAttributes();
        }
    }

    public static class QuestionTwoWeaknessesViewModelValidatorRules
    {
        public static IRuleBuilderOptionsConditions<T, List<ProviderAttributeModel>> ValidateAttributes<T>(this IRuleBuilder<T, List<ProviderAttributeModel>> ruleBuilder)
            where T : QuestionTwoWeaknessesViewModel
        {
            return ruleBuilder.Custom((value, context) =>
            {
                var model = context.InstanceToValidate;

                if(model.Attributes.Count(a => a.Bad) > 3)
                {
                    context.AddFailure($"Attributes", $"Choose up to 3 options");

                    // add individual errors to ensure that array elements are not discarded from AttemptedValues
                    var weaknesses = model.Attributes.Select((a, i) => new { a, i }).Where(x => x.a.Bad).ToList();
                    weaknesses.ForEach((bad) => { context.AddFailure($"Attributes[{bad.i}].Bad", $"Choose up to 3 options"); });
                }
            });
        }
    }
}
