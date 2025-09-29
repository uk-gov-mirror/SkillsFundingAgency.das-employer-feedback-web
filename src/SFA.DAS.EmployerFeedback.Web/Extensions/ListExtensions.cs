using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace SFA.DAS.EmployerFeedback.Web.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class ListExtensions
    {
        public static bool IsComplete(this List<string> list)
        {
            return list != null && list.Any();
        }
    }
}