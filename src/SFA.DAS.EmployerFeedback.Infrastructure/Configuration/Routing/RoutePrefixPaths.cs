namespace SFA.DAS.EmployerFeedback.Infrastructure.Configuration.Routing
{
    public static class RoutePrefixPaths
    {
        public const string FeedbackFromEmailRoutePath = "{uniqueCode:guid}";
        public const string FeedbackRoutePath = "{encodedAccountId}";  // /{uniqueCode:guid}
    }
}
