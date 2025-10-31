namespace SFA.DAS.EmployerFeedback.Infrastructure.Configuration;

// NOSONAR
public static class EmployerClaims
{
    public static string UserId => "http://das/employer/identity/claims/id";
    public static string EmailAddress => "http://das/employer/identity/claims/email_address";
    public static string GivenName => "http://das/employer/identity/claims/given_name";
    public static string FamilyName => "http://das/employer/identity/claims/family_name";
    public static string AssociatedAccounts => "http://das/employer/identity/claims/associatedAccounts";

    public const string UserDisplayName = "http://das/employer/identity/claims/display_name";

    public const string UserAccount = "http://das/employer/identity/claims/account";
}