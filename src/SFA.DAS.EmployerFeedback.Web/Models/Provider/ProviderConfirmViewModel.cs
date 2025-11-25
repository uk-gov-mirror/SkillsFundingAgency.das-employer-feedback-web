using SFA.DAS.EmployerFeedback.Web.Models.Shared;

namespace SFA.DAS.EmployerFeedback.Web.Models.Provider
{
    public class ProviderConfirmViewModel : AccountModel
    {
        public long ProviderId { get; set; }
        public string ProviderName { get; set; }
        public bool? Confirmed { get; set; }
    }
}
