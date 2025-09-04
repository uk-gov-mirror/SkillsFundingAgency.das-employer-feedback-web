using SFA.DAS.EmployerProvideFeedback.Paging;
using System.Collections.Generic;

namespace SFA.DAS.EmployerFeedback.Web.Models;

public class PaginationLinksViewModel
{
    public PaginatedList PaginatedList { get; set; }
    public string ChangePageAction { get; set; }
    public string ChangePageController { get; set; }
    public IDictionary<string,string> RouteValues { get; set; }
    public string Fragment { get; set; }
}
