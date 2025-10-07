using Microsoft.AspNetCore.Http;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerFeedback.Web.Paging;
using SFA.DAS.EmployerProvideFeedback.Paging;
using SFA.DAS.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerProvideFeedback.Services
{
    public interface ITrainingProviderService
    {
        Task<ProviderSearchViewModel> GetTrainingProviderSearchViewModel(string encodedAccountId, Guid userRef, string selectedProviderName, string selectedFeedbackStatus, int pageSize, int pageIndex, string sortColumn, string sortDirection);

        Task<ProviderSearchConfirmationViewModel> GetTrainingProviderConfirmationViewModel(long accountId, Guid userref, long providerId);

    }



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
            model.AccountId = _encodingService.Decode(encodedAccountId, EncodingType.AccountId);
            model.EncodedAccountId = encodedAccountId;
            model.SelectedProviderName = selectedProviderName;
            model.SelectedFeedbackStatus = selectedFeedbackStatus;
            model.SortColumn = sortColumn;
            model.SortDirection = sortDirection;

            // Select all the providers for this employer.
            var providers = await SelectAllProvidersForAccount(model.AccountId, userRef);

            // Augment the provider records with feedback data. Urgh.
            // We need to do this so that the date filtering will work.

            await AugmentProviderRecordsWithFeedbackStatus(model.AccountId, providers);

            // Initialise the filter options.

            model.UnfilteredTotalRecordCount = providers.Count;
            model.ProviderNameFilter = providers.Select(p => p.ProviderName).OrderBy(p => p).ToList();
            model.FeedbackStatusFilter = new string[] { NOT_YET_SUBMITTED };

            // Apply filters.

            var filteredProviders = providers.AsQueryable();
            filteredProviders = ApplyProviderNameFilter(filteredProviders, selectedProviderName);
            filteredProviders = ApplyFeedbackStatusFilter(filteredProviders, selectedFeedbackStatus);

            // Sort

            if(PagingState.SortDescending == model.SortDirection)
            {
                if(!string.IsNullOrWhiteSpace(model.SortColumn) && model.SortColumn.Equals("FeedbackStatus", StringComparison.InvariantCultureIgnoreCase))
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
            model.TrainingProviders = new PaginatedList<ProviderSearchViewModel.EmployerTrainingProvider>(pagedFilteredProviders, filteredProviders.Count(), pageIndex, pageSize, 6);

            return model;
        }

        public async Task<ProviderSearchConfirmationViewModel> GetTrainingProviderConfirmationViewModel(long accountId, Guid userref, long providerId)
        {
            var response = await _employerFeedbackOuterApi.GetTrainingProviderSearch(accountId, userref);

            if (null == response)
            {
                return null;
            }

            var provider  = response.Providers.FirstOrDefault(p => p.Ukprn == providerId);
            if (null == provider)
            {
                return null;
            }

            var model = new ProviderSearchConfirmationViewModel();
            model.ProviderId = provider.Ukprn;
            model.ProviderName = provider.ProviderName;

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
                    DateSubmitted = a.First().Feedback?.DateTimeCompleted
                })
                .ToList();

            return providers;
        }

        private async Task AugmentProviderRecordsWithFeedbackStatus(long accountId, List<ProviderSearchViewModel.EmployerTrainingProvider> providers)
        {
            foreach (var provider in providers)
            {
                if (provider.FeedbackStatus == null)
                {
                    provider.FeedbackStatus = NOT_YET_SUBMITTED;
                    provider.DateSubmitted = null;
                }

                provider.CanSubmitFeedback = true;
                if (provider.DateSubmitted.HasValue && (DateTime.UtcNow - provider.DateSubmitted.Value).TotalDays < _config.FeedbackWaitPeriodDays)
                {
                    provider.CanSubmitFeedback = false;
                }
            }
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
