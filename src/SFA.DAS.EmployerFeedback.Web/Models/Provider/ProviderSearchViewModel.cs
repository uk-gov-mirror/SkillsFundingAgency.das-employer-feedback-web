using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFeedback.Paging;

namespace SFA.DAS.EmployerFeedback.Web.Models.Shared
{
    public class ProviderSearchViewModel : AccountModel
    {
        [Display(Name = "Training provider")]
        public string SelectedProviderName { get; set; }
        public IEnumerable<string> ProviderNameFilter { get; set; }

        [Display(Name = "Feedback status")]
        public string SelectedFeedbackStatus { get; set; }
        public IEnumerable<string> FeedbackStatusFilter { get; set; }

        public PaginatedList<EmployerTrainingProvider> Providers { get; set; }
        public string Fragment { get; set; }
        public string SortColumn { get; set; }
        public string SortDirection { get; set; }
        public int UnfilteredTotalRecordCount { get; set; }
        public string ChangePageRouteName { get; set; }

        public string BackUrl { get; set; }

        public class EmployerTrainingProvider
        {
            public long ProviderId { get; set; }
            public string ProviderName { get; set; }
            public string FeedbackStatus { get; set; }
            public DateTime? DateSubmitted { get; set; }
            public bool CanSubmitFeedback { get; set; }
        }
    }
}
