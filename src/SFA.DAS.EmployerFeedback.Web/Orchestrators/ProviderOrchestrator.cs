using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerFeedback.Application.Queries.GetAllQuestionAttributes;
using SFA.DAS.EmployerFeedback.Domain.Types;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.UserService;
using SFA.DAS.EmployerFeedback.Services;
using SFA.DAS.EmployerFeedback.Web.Controllers;
using SFA.DAS.EmployerFeedback.Web.Exceptions;
using SFA.DAS.EmployerFeedback.Web.Models.Provider;
using SFA.DAS.EmployerFeedback.Web.Models.Questions;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerFeedback.Web.Paging;
using SFA.DAS.EmployerFeedback.Web.Services;
using SFA.DAS.EmployerFeedback.Web.Services.SessionStorage;

namespace SFA.DAS.EmployerFeedback.Web.Orchestrators
{
    public class ProviderOrchestrator : BaseOrchestrator, IProviderOrchestrator
    {
        private readonly ISessionStorageService _sessionService;
        private readonly ITrainingProviderService _trainingProviderService;
        private readonly IMediator _mediator;
        private readonly IAccountsLinkService _accountsLinkService;
        private readonly IValidator<ProviderConfirmViewModel> _providerConfirmViewModelValidator;

        public ProviderOrchestrator(ISessionStorageService sessionService,
            IUserService userService,
            ILogger<ProviderOrchestrator> logger,
            ITrainingProviderService trainingProviderService,
            IMediator mediator,
            IAccountsLinkService accountsLinkService,
            IValidator<ProviderConfirmViewModel> providerConfirmViewModelValidator)
            : base(logger, userService)
        {
            _sessionService = sessionService;
            _trainingProviderService = trainingProviderService;
            _mediator = mediator;
            _accountsLinkService = accountsLinkService;
            _providerConfirmViewModelValidator = providerConfirmViewModelValidator;
        }

        public async Task<ProviderSearchViewModel> GetProviderSearchViewModel(ProviderSearchRequestModel model)
        {
            var userId = GetUserId();
            var pagingState = await _sessionService.GetPagingState(userId);

            var viewModel = await _trainingProviderService.GetTrainingProviderSearchViewModel(
                model.AccountId,
                model.EncodedAccountId,
                userId,
                pagingState.SelectedProviderName,
                pagingState.SelectedFeedbackStatus,
                pagingState.PageSize,
                pagingState.PageIndex,
                pagingState.SortColumn,
                pagingState.SortOrder);

            viewModel.ChangePageRouteName = ProviderController.ProviderSearchGet;
            viewModel.BackUrl = _accountsLinkService.AccountsHome(model.EncodedAccountId);

            return viewModel;
        }

        public async Task SetProviderSearchPageIndex(int pageIndex)
        {
            await _sessionService.UpdatePagingState(GetUserId(), (pagingState) =>
            {
                pagingState.PageIndex = pageIndex;
            });
        }

        public async Task SetProviders(List<ProviderSearchViewModel.EmployerTrainingProvider> providers)
        {
            await _sessionService.SetProviders(GetUserId(), providers);
        }

        public async Task SetFeedbackSource(FeedbackSource feedbackSource)
        {
            await _sessionService.SetFeedbackSource(GetUserId(), feedbackSource);
        }

        public async Task UpdateProviderSearchFilters(ProviderSearchViewModel viewModel)
        {
            await _sessionService.UpdatePagingState(GetUserId(), (PagingState pagingState) =>
            {
                pagingState.PageIndex = PagingState.DefaultPageIndex;
                pagingState.SelectedProviderName = viewModel.SelectedProviderName;
                pagingState.SelectedFeedbackStatus = viewModel.SelectedFeedbackStatus;
            });
        }

        public async Task ClearProviderSearchFilters()
        {
            await _sessionService.SetPagingState(GetUserId(), new PagingState());
        }

        public async Task SortProviderSearch(SortColumn sortColumn, SortOrder sortOrder)
        {
            await _sessionService.UpdatePagingState(GetUserId(), (PagingState pagingState) =>
            {
                pagingState.SortColumn = sortColumn;
                pagingState.SortOrder = sortOrder;
            });
        }

        public async Task<ProviderConfirmViewModel> GetProviderConfirmViewModel(ProviderConfirmRequestModel model)
        {
            var providers = await _sessionService.GetProviders(GetUserId());
            var providerName = providers?.FirstOrDefault(p => p.ProviderId == model.ProviderId)?.ProviderName;

            if (!string.IsNullOrEmpty(providerName))
            {
                return new ProviderConfirmViewModel
                {
                    EncodedAccountId = model.EncodedAccountId,
                    ProviderId = model.ProviderId,
                    ProviderName = providerName
                };
            }

            return null;
        }

        public async Task<bool> ValidateProviderConfirmViewModel(ProviderConfirmViewModel viewModel, ModelStateDictionary modelState)
        {
            return await ValidateViewModel(_providerConfirmViewModelValidator, viewModel, modelState);
        }

        public async Task CreateNewSurvey(ProviderConfirmViewModel viewModel)
        {
            var questionAttributes = await _mediator.Send(new GetAllQuestionAttributesQuery());
            if (questionAttributes == null)
            {
                throw new EmployerFeedbackException("Unable to load question attributes");
            }

            var providerAttributes = questionAttributes.Select(s => new ProviderAttributeModel { AttributeId = s.AttributeId, Name = s.AttributeName }).ToList();
            var userId = GetUserId();
            var feedbackSource = await _sessionService.GetFeedbackSource(userId);

            if (!System.Enum.IsDefined(typeof(FeedbackSource), feedbackSource))
            {
                feedbackSource = FeedbackSource.AdHoc;
                await _sessionService.SetFeedbackSource(userId, feedbackSource);
            }

            var survey = new SurveyModel
            {
                AccountId = viewModel.AccountId,
                EncodedAccountId = viewModel.EncodedAccountId,
                Ukprn = viewModel.ProviderId,
                UserRef = userId,
                ProviderName = viewModel.ProviderName,
                Attributes = providerAttributes,
                FeedbackSource = feedbackSource
            };

            await _sessionService.SetSurveyModel(userId, survey);
        }
    }
}
