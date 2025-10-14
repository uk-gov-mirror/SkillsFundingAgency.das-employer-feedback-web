using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.EmployerFeedback.Domain.Types
{
    public enum ProviderRating
    {
        [Display(Name = "Very Poor")]
        VeryPoor = 1,
        Poor = 2,
        Good = 3,
        Excellent = 4
    }
}
