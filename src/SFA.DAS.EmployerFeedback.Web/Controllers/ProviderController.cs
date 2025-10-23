using System;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration.Routing;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.UserService;
using SFA.DAS.EmployerFeedback.Services;
using SFA.DAS.EmployerFeedback.Web.Authorization;
using SFA.DAS.EmployerFeedback.Web.Exceptions;
using SFA.DAS.EmployerFeedback.Web.Models.Provider;
using SFA.DAS.EmployerFeedback.Web.Models.Questions;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerFeedback.Web.Paging;
using SFA.DAS.EmployerFeedback.Web.Services;
using SFA.DAS.EmployerFeedback.Web.Services.SessionStorage;
using SFA.DAS.EmployerFeedback.Web.Validators.Questions;

namespace SFA.DAS.EmployerFeedback.Web.Controllers
{
    [Authorize(Policy = nameof(PolicyNames.NoneRole))]
    [Route(RoutePrefixPaths.FeedbackRoutePath)]
    public class ProviderController : ControllerBase
    {
        #region Routes
        public const string ProviderSearchGet = nameof(ProviderSearchGet);
        public const string ProviderSearchPost = nameof(ProviderSearchPost);
        public const string ProviderConfirmGet = nameof(ProviderConfirmGet);
        public const string ProviderConfirmPost = nameof(ProviderConfirmPost);
        public const string SortProvidersGet = nameof(SortProvidersGet);
        public const string ClearFiltersGet = nameof(ClearFiltersGet);
        #endregion

        private readonly ISessionStorageService _sessionService;
        private readonly ILogger<ProviderController> _logger;
        private readonly ITrainingProviderService _trainingProviderService;
        private readonly IEmployerFeedbackOuterApi _employerFeedbackOuterApi;
        private readonly IAccountsLinkService _accountsLinkService;
        private readonly IValidator<ProviderConfirmViewModel> _providerConfirmViewModelValidator;

        public ProviderController(ISessionStorageService sessionService,
            IUserService userService,
            ILogger<ProviderController> logger,
            ITrainingProviderService trainingProviderService,
            IEmployerFeedbackOuterApi employerFeedbackOuterApi,
            IAccountsLinkService accountsLinkService,
            IValidator<ProviderConfirmViewModel> providerConfirmViewModelValidator) 
            : base(userService, logger)
        {
            _sessionService = sessionService;
            _logger = logger;
            _trainingProviderService = trainingProviderService;
            _employerFeedbackOuterApi = employerFeedbackOuterApi;
            _accountsLinkService = accountsLinkService;
            _providerConfirmViewModelValidator = providerConfirmViewModelValidator;
        }

        [HttpGet]
        [Route("providers", Name = ProviderSearchGet)]
        public async Task<IActionResult> ProviderSearch(ProviderSearchRequestModel model, int pageIndex = PagingState.DefaultPageIndex)
        {
            var userId = GetUserId();
            var pagingState = await _sessionService.UpdatePagingState(userId, (PagingState pagingState) =>
            {
                pagingState.PageIndex = pageIndex;
            });

            var viewModel = await _trainingProviderService.GetTrainingProviderSearchViewModel(
                model.AccountId,
                model.EncodedAccountId,
                userId,
                pagingState.SelectedProviderName,
                pagingState.SelectedFeedbackStatus,
                pagingState.PageSize,
                pagingState.PageIndex,
                pagingState.SortColumn,
                pagingState.SortDirection);

            viewModel.ChangePageAction = nameof(ProviderSearch);
            viewModel.BackUrl = _accountsLinkService.AccountsHome(model.EncodedAccountId);

            await _sessionService.SetProviders(userId, viewModel.Providers.Items);
            await _sessionService.SetFeedbackSource(userId, model.FeedbackSource);

            return View(viewModel);
        }

        [HttpPost]
        [Route("providers", Name = ProviderSearchPost)]
        public async Task<IActionResult> ProviderSearch(ProviderSearchViewModel viewModel)
        {
            await _sessionService.UpdatePagingState(GetUserId(), (PagingState pagingState) =>
            {
                pagingState.PageIndex = PagingState.DefaultPageIndex;
                pagingState.SelectedProviderName = viewModel.SelectedProviderName;
                pagingState.SelectedFeedbackStatus = viewModel.SelectedFeedbackStatus;
            });

            return RedirectToRoute(ProviderSearchGet, new { viewModel.EncodedAccountId });
        }

        [HttpGet]
        [Route("providers/sort", Name = SortProvidersGet)]
        public async Task<IActionResult> SortProviders(AccountModel model, string sortColumn, string sortDirection)
        {
            await _sessionService.UpdatePagingState(GetUserId(), (PagingState pagingState) =>
            {
                pagingState.SortColumn = sortColumn;
                pagingState.SortDirection = sortDirection;
            });

            return RedirectToRoute(ProviderSearchGet, new { model.EncodedAccountId });
        }

        [HttpGet]
        [Route("providers/clearfilters", Name = ClearFiltersGet)]
        public async Task<IActionResult> ClearFilters(AccountModel model)
        {
            await _sessionService.SetPagingState(GetUserId(), new PagingState());
            return RedirectToRoute(ProviderSearchGet, new { model.EncodedAccountId });
        }

        [HttpGet]
        [Route("providers/{providerId}", Name = ProviderConfirmGet)]
        public async Task<IActionResult> ProviderConfirm(ProviderConfirmRequestModel model)
        {
            var providers = await _sessionService.GetProviders(GetUserId());
            var provider = providers?.FirstOrDefault(p => p.ProviderId == model.ProviderId);
            
            if (provider == null)
                return RedirectToRoute(ProviderSearchGet, new { encodedAccountId = model.EncodedAccountId });

            var viewModel = new ProviderConfirmViewModel
            {
                EncodedAccountId = model.EncodedAccountId,
                ProviderId = model.ProviderId,
                ProviderName = provider.ProviderName
            };

            return View(viewModel);
        }

        [HttpPost]
        [Route("providers/{providerId}", Name = ProviderConfirmPost)]
        public async Task<IActionResult> ProviderConfirm(ProviderConfirmViewModel viewModel)
        {
            if (!await ViewModelIsValid(_providerConfirmViewModelValidator, viewModel, ModelState))
                return RedirectToRoute(ProviderConfirmGet, new { encodedAccountId = viewModel.EncodedAccountId, providerId = viewModel.ProviderId });

            if (!viewModel.Confirmed.Value)
            {
                return RedirectToRoute(ProviderSearchGet, new { encodedAccountId = viewModel.EncodedAccountId });
            }

            var providerAttributes = await _employerFeedbackOuterApi.GetAllAttributes();
            if (providerAttributes == null)
            {
                throw new EmployerFeedbackException("Unable to load Provider Attributes from the database.");
            }

            var providerAttributesModel = providerAttributes.Select(s => new ProviderAttributeModel { Name = s.AttributeName }).ToList();
            var userId = GetUserId();
            var feedbackSource = await _sessionService.GetFeedbackSource(userId);

            var newSurveyModel = new SurveyModel
            {
                AccountId = viewModel.AccountId,
                EncodedAccountId = viewModel.EncodedAccountId,
                Ukprn = viewModel.ProviderId,
                UserRef = userId,
                ProviderName = viewModel.ProviderName,
                Attributes = providerAttributesModel,
                FeedbackSource = feedbackSource
            };

            await _sessionService.SetSurveyModel(userId, newSurveyModel);
            return RedirectToRoute(QuestionsController.StartFeedbackGet, new { viewModel.EncodedAccountId });
        }
    }
}