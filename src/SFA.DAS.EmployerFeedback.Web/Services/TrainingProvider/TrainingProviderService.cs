using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerFeedback.Domain.Entities.Models;
using SFA.DAS.EmployerFeedback.Domain.Types;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration;
using SFA.DAS.EmployerFeedback.Paging;
using SFA.DAS.EmployerFeedback.Web.Extensions;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerFeedback.Web.Paging;

namespace SFA.DAS.EmployerFeedback.Services
{
    public class TrainingProviderService : ITrainingProviderService
    {
        private readonly IEmployerFeedbackOuterApi _employerFeedbackOuterApi;
        private readonly ILogger<TrainingProviderService> _logger;
        private readonly EmployerFeedbackWebConfiguration _config;

        private const string NOT_YET_SUBMITTED = "Not yet submitted";

        public TrainingProviderService(IEmployerFeedbackOuterApi employerFeedbackOuterApi, ILogger<TrainingProviderService> logger, EmployerFeedbackWebConfiguration config)
        {
            _employerFeedbackOuterApi = employerFeedbackOuterApi;
            _logger = logger;
            _config = config;
        }

        public async Task<ProviderSearchViewModel> GetTrainingProviderSearchViewModel(
            long accountId,
            string encodedAccountId,
            Guid userRef,
            string selectedProviderName,
            string selectedFeedbackStatus,
            int pageSize,
            int pageIndex,
            SortColumn sortColumn,
            SortOrder sortOrder)
        {
            ProviderSearchViewModel model = new ProviderSearchViewModel();
            model.AccountId = accountId;
            model.EncodedAccountId = encodedAccountId;
            model.SelectedProviderName = selectedProviderName;
            model.SelectedFeedbackStatus = selectedFeedbackStatus;
            model.SortColumn = sortColumn;
            model.SortOrder = sortOrder;

            // select all the providers for this employer
            var providers = await SelectAllProvidersForAccount(model.AccountId, userRef);

            // initialise the filter options
            model.UnfilteredTotalRecordCount = providers.Count;
            model.ProviderNameFilter = providers.Select(p => p.ProviderName).OrderBy(p => p).ToList();
            model.FeedbackStatusFilter = new string[] { NOT_YET_SUBMITTED };

            // filter providers
            var filteredProviders = providers.AsQueryable();
            filteredProviders = ApplyProviderNameFilter(filteredProviders, selectedProviderName);
            filteredProviders = ApplyFeedbackStatusFilter(filteredProviders, selectedFeedbackStatus);

            // sort filtered providers
            if (SortOrder.Descending == model.SortOrder)
            {
                if (model.SortColumn == SortColumn.FeedbackStatus)
                {
                    filteredProviders = filteredProviders.OrderByDescending(p => p.FeedbackStatus);
                }
                else if (model.SortColumn == SortColumn.DateSubmitted)
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
                if (model.SortColumn == SortColumn.FeedbackStatus)
                {
                    filteredProviders = filteredProviders.OrderBy(p => p.FeedbackStatus);
                }
                else if (model.SortColumn == SortColumn.DateSubmitted)
                {
                    filteredProviders = filteredProviders.OrderBy(p => p.DateSubmitted);
                }
                else
                {
                    filteredProviders = filteredProviders.OrderBy(p => p.ProviderName);
                }
            }

            // take the single page of filtered sorted providers
            var pagedFilteredProviders = filteredProviders.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            model.Providers = new PaginatedList<ProviderSearchViewModel.EmployerTrainingProvider>(pagedFilteredProviders, filteredProviders.Count(), pageIndex, pageSize, 6);
            return model;
        }
        
        private async Task<List<ProviderSearchViewModel.EmployerTrainingProvider>> SelectAllProvidersForAccount(long accountId, Guid userref)
        {
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

        public async Task<bool> CanSubmitFeedback(SurveyModel surveyModel, Guid userId)
        {
            var trainingProviders = await _employerFeedbackOuterApi.GetTrainingProviderSearch(surveyModel.AccountId, userId);
            var dateTimeCompleted = trainingProviders
                .Providers
                .FirstOrDefault(x => x.Ukprn == surveyModel.Ukprn)?.Feedback?.DateTimeCompleted;

            return CanSubmitFeedback(dateTimeCompleted);
        }

        private bool CanSubmitFeedback(DateTime? dateTimeCompleted)
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

        

        private static IQueryable<ProviderSearchViewModel.EmployerTrainingProvider> ApplyProviderNameFilter(IQueryable<ProviderSearchViewModel.EmployerTrainingProvider> providers, string providerName)
        {
            if (!string.IsNullOrWhiteSpace(providerName) && providerName != "All")
            {
                providers = providers.Where(p => p.ProviderName == providerName);
            }
            return providers;
        }

        private static IQueryable<ProviderSearchViewModel.EmployerTrainingProvider> ApplyFeedbackStatusFilter(IQueryable<ProviderSearchViewModel.EmployerTrainingProvider> providers, string feedbackStatus)
        {
            if (feedbackStatus == NOT_YET_SUBMITTED)
            {
                providers = providers.Where(p => null == p.DateSubmitted || !p.DateSubmitted.HasValue);
            }
            return providers;
        }
    }
}
