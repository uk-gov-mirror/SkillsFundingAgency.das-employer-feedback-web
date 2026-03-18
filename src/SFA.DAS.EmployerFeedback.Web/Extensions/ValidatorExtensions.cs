using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SFA.DAS.EmployerFeedback.Web.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class IValidatorExtensions
    {
        public static ValidationResult ValidateAndAddModelErrors<T>(this IValidator<T> validator, T model, ModelStateDictionary modelState)
        {
            var result = validator.Validate(model);
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    if (!modelState.ContainsKey(error.PropertyName) || modelState[error.PropertyName].Errors.Count == 0)
                        modelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
            }

            return result;
        }
    }
}
