using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration;
using SFA.DAS.EmployerFeedback.Paging;
using SFA.DAS.EmployerFeedback.Web.Models.Provider;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerFeedback.Web.Paging;
using SFA.DAS.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace SFA.DAS.EmployerFeedback.Services
{
    public class TrainingProviderService : ITrainingProviderService
    {
        private readonly IEncodingService _encodingService;
        private readonly EmployerFeedbackWebConfiguration _config;
        private readonly IEmployerFeedbackOuterApi _employerFeedbackOuterApi;
        private const string NOT_YET_SUBMITTED = "Not yet submitted";

        public TrainingProviderService(IEncodingService encodingService, EmployerFeedbackWebConfiguration config, IEmployerFeedbackOuterApi employerFeedbackOuterApi)
        {
            _encodingService = encodingService;
            _config = config;
            _employerFeedbackOuterApi = employerFeedbackOuterApi;
        }

        public async Task<ProviderSearchViewModel> GetTrainingProviderSearchViewModel(
            long accountId,
            string encodedAccountId,
            Guid userRef,
            string selectedProviderName,
            string selectedFeedbackStatus,
            int pageSize,
            int pageIndex,
            string sortColumn,
            string sortDirection)
        {
            ProviderSearchViewModel model = new ProviderSearchViewModel();
            model.AccountId = accountId;
            model.EncodedAccountId = encodedAccountId;
            model.SelectedProviderName = selectedProviderName;
            model.SelectedFeedbackStatus = selectedFeedbackStatus;
            model.SortColumn = sortColumn;
            model.SortDirection = sortDirection;

            // Select all the providers for this employer.
            var providers = await SelectAllProvidersForAccount(model.AccountId, userRef);

            // Initialise the filter options.

            model.UnfilteredTotalRecordCount = providers.Count;
            model.ProviderNameFilter = providers.Select(p => p.ProviderName).OrderBy(p => p).ToList();
            model.FeedbackStatusFilter = new string[] { NOT_YET_SUBMITTED };

            // Apply filters.

            var filteredProviders = providers.AsQueryable();
            filteredProviders = ApplyProviderNameFilter(filteredProviders, selectedProviderName);
            filteredProviders = ApplyFeedbackStatusFilter(filteredProviders, selectedFeedbackStatus);

            // Sort
            if (PagingState.SortDescending == model.SortDirection)
            {
                if (!string.IsNullOrWhiteSpace(model.SortColumn) && model.SortColumn.Equals("FeedbackStatus", StringComparison.InvariantCultureIgnoreCase))
                {
                    filteredProviders = filteredProviders.OrderByDescending(p => p.FeedbackStatus);
                }
                else if (!string.IsNullOrWhiteSpace(model.SortColumn) && model.SortColumn.Equals("DateSubmitted", StringComparison.InvariantCultureIgnoreCase))
                {
                    filteredProviders = filteredProviders.OrderByDescending(p => p.DateSubmitted);
                }
                else
                {
                    filteredProviders = filteredProviders.OrderByDescending(p => p.ProviderName);
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(model.SortColumn) && model.SortColumn.Equals("FeedbackStatus", StringComparison.InvariantCultureIgnoreCase))
                {
                    filteredProviders = filteredProviders.OrderBy(p => p.FeedbackStatus);
                }
                else if (!string.IsNullOrWhiteSpace(model.SortColumn) && model.SortColumn.Equals("DateSubmitted", StringComparison.InvariantCultureIgnoreCase))
                {
                    filteredProviders = filteredProviders.OrderBy(p => p.DateSubmitted);
                }
                else
                {
                    filteredProviders = filteredProviders.OrderBy(p => p.ProviderName);
                }
            }

            // Page
            var pagedFilteredProviders = filteredProviders.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            model.Providers = new PaginatedList<ProviderSearchViewModel.EmployerTrainingProvider>(pagedFilteredProviders, filteredProviders.Count(), pageIndex, pageSize, 6);
            return model;
        }
        
        private async Task<List<ProviderSearchViewModel.EmployerTrainingProvider>> SelectAllProvidersForAccount(long accountId, Guid userref)
        {
            // Select all 
            var apprenticeshipsResponse = await _employerFeedbackOuterApi.GetTrainingProviderSearch(accountId, userref);

            var providers = apprenticeshipsResponse.Providers.GroupBy(p => p.Ukprn)
                .Select(a => new ProviderSearchViewModel.EmployerTrainingProvider()
                {
                    ProviderId = a.First().Ukprn,
                    ProviderName = a.First().ProviderName,
                    FeedbackStatus = a.First().Feedback == null ? NOT_YET_SUBMITTED : "Submitted",
                    DateSubmitted = a.First().Feedback?.DateTimeCompleted,
                    CanSubmitFeedback = CanSubmitFeedback(a.First().Feedback?.DateTimeCompleted ?? null)
                })
                .ToList();

            return providers;
        }
        public bool CanSubmitFeedback(DateTime? dateTimeCompleted)
        {
            if (!dateTimeCompleted.HasValue)
            {
                return true;
            }
            if ((DateTime.UtcNow - dateTimeCompleted.Value).TotalDays < _config.FeedbackWaitPeriodDays)
            {
                return false;
            }
            return true;
        }

        private IQueryable<ProviderSearchViewModel.EmployerTrainingProvider> ApplyProviderNameFilter(IQueryable<ProviderSearchViewModel.EmployerTrainingProvider> providers, string providerName)
        {
            if (!string.IsNullOrWhiteSpace(providerName) && providerName != "All")
            {
                providers = providers.Where(p => p.ProviderName == providerName);
            }
            return providers;
        }

        private IQueryable<ProviderSearchViewModel.EmployerTrainingProvider> ApplyFeedbackStatusFilter(IQueryable<ProviderSearchViewModel.EmployerTrainingProvider> providers, string feedbackStatus)
        {
            if (feedbackStatus == NOT_YET_SUBMITTED)
            {
                providers = providers.Where(p => null == p.DateSubmitted || !p.DateSubmitted.HasValue);
            }
            return providers;
        }
    }
}
