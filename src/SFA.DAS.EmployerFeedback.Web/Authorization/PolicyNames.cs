using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.EmployerFeedback.Web.Authorization
{
    [ExcludeFromCodeCoverage]
    public static class PolicyNames
    {
        public const string OwnerRole = nameof(OwnerRole);
        public const string TransactorRole = nameof(TransactorRole);
        public const string ViewerRole = nameof(ViewerRole);
        public const string NoneRole = nameof(NoneRole);
        public const string IsAuthenticated = nameof(IsAuthenticated);
    }
}