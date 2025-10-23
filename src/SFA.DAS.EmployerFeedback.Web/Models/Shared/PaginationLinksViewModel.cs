using SFA.DAS.EmployerFeedback.Paging;
using System.Collections.Generic;

namespace SFA.DAS.EmployerFeedback.Web.Models;

public class PaginationLinksViewModel
{
    public PaginatedList PaginatedList { get; set; }
    public string RouteName { get; set; }
    public IDictionary<string, string> RouteValues { get; set; }
    public string Fragment { get; set; }
}
