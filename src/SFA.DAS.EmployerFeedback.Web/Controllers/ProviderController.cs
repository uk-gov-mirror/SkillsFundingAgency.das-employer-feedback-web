using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SFA.DAS.Employer.Shared.UI;
using SFA.DAS.EmployerFeedback.Domain.Types;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration.Routing;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.SessionStorage;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.UserService;
using SFA.DAS.EmployerFeedback.Web.Authorization;
using SFA.DAS.EmployerFeedback.Web.Configuration.Routing;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerFeedback.Web.Paging;
using SFA.DAS.EmployerFeedback.Web.ViewModels;
using SFA.DAS.EmployerProvideFeedback.Services;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFeedback.Web.Controllers
{


    [Authorize(Policy = nameof(PolicyNames.NoneRole))]
    public class ProviderController : ControllerBase
    {
        private readonly ISessionStorageService _sessionService;
        private readonly ITrainingProviderService _trainingProviderService;
        private readonly ILogger<ProviderController> _logger;
        private readonly IEmployerFeedbackOuterApi _employerFeedbackOuterApi;
        private readonly UrlBuilder _urlBuilder;

        public ProviderController(ISessionStorageService sessionService,
            ITrainingProviderService trainingProviderService,
            ILogger<ProviderController> logger,
            IEmployerFeedbackOuterApi employerFeedbackOuterApi,
            UrlBuilder urlBuilder,
            IUserService userService
            ) : base(userService, logger)
        {
            _sessionService = sessionService;
            _trainingProviderService = trainingProviderService;
            _logger = logger;
            _employerFeedbackOuterApi = employerFeedbackOuterApi;
            _urlBuilder = urlBuilder;
        }

        [HttpGet]
        [Route("/{encodedAccountId}/providers")]
        public async Task<IActionResult> Index(GetProvidersForFeedbackRequest request, int pageIndex = PagingState.DefaultPageIndex)
        {
            var userId = GetUserId().Value;
            var pagingState = GetPagingState();
            pagingState.PageIndex = pageIndex;
            SetPagingState(pagingState);

            var model = await _trainingProviderService.GetTrainingProviderSearchViewModel(
                request.EncodedAccountId,
                userId,
                pagingState.SelectedProviderName,
                pagingState.SelectedFeedbackStatus,
                pagingState.PageSize,
                pagingState.PageIndex,
                pagingState.SortColumn,
                pagingState.SortDirection);
            model.ChangePageAction = nameof(Index);

            ViewBag.EmployerAccountsHomeUrl = _urlBuilder.AccountsLink("AccountsHome", request.EncodedAccountId);

            await _sessionService.Set($"{userId}_ProviderCount", model.TrainingProviders.TotalRecordCount);
            await _sessionService.Set($"{userId}_FeedbackSource", request.FeedbackSource);


            return View(model);
        }

        [HttpPost]
        [Route("/{encodedAccountId}/providers")]
        public async Task<IActionResult> Filter(ProviderSearchViewModel postedModel)
        {
            var userId = GetUserId().Value;
            var pagingState = await _sessionService.Get<PagingState>($"{userId}_PagingState");
            if (null == pagingState)
            {
                pagingState = new PagingState();
            }
            pagingState.PageIndex = PagingState.DefaultPageIndex; // applying filter resets the paging
            pagingState.SelectedProviderName = postedModel.SelectedProviderName;
            pagingState.SelectedFeedbackStatus = postedModel.SelectedFeedbackStatus;
            await _sessionService.Set($"{userId}_PagingState", pagingState);

            var model = await _trainingProviderService.GetTrainingProviderSearchViewModel(
                postedModel.EncodedAccountId,
                userId,
                pagingState.SelectedProviderName,
                pagingState.SelectedFeedbackStatus,
                pagingState.PageSize,
                pagingState.PageIndex,
                pagingState.SortColumn,
                pagingState.SortDirection);

            ViewBag.EmployerAccountsHomeUrl = _urlBuilder.AccountsLink("AccountsHome", postedModel.EncodedAccountId);
            return View("Index", model);
        }

        [HttpGet]
        [Route("/{encodedAccountId}/providers/sort")]
        public async Task<IActionResult> SortProviders(string encodedAccountId, string sortColumn, string sortDirection)
        {
            var pagingState = GetPagingState();
            pagingState.SortColumn = sortColumn;
            pagingState.SortDirection = sortDirection;
            SetPagingState(pagingState);

            return RedirectToAction(nameof(Index), new { encodedAccountId });
        }

        [HttpGet]
        [Route("/{encodedAccountId}/providers/unfilter")]
        public async Task<IActionResult> ClearFilters(string encodedAccountId)
        {
            var pagingState = GetPagingState();
            pagingState.SelectedProviderName = string.Empty;
            pagingState.SelectedFeedbackStatus = string.Empty;
            SetPagingState(pagingState);

            return RedirectToAction(nameof(Index), new { encodedAccountId = encodedAccountId });
        }


        [HttpGet]
        [Route("/{encodedAccountId}/providers/{providerId}")]
        public async Task<IActionResult> ConfirmProvider(ProviderSearchConfirmationViewModel postedModel)
        {
            var userId = GetUserId().Value;
            //var accountId = _encodingService.Decode(encodedAccountId, EncodingType.AccountId); // validate the account id

            var model = await _trainingProviderService.GetTrainingProviderConfirmationViewModel(postedModel.AccountId, userId, postedModel.ProviderId);

            return View(model);
        }

        [HttpPost]
        [Route("/{encodedAccountId}/providers/{providerId}")]
        public async Task<IActionResult> ProviderConfirmed(ProviderSearchConfirmationViewModel postedModel)
        {
            if (!postedModel.Confirmed.HasValue)
            {
                ModelState.AddModelError("Confirmation", "Please choose an option");
                return View("ConfirmProvider", postedModel);
            }

            if (!postedModel.Confirmed.Value)
            {
                var accountId = HttpContext.GetRouteData().Values[RouteValueKeys.EncodedAccountId] as string;
                long ukprn = postedModel.ProviderId;
                return RedirectToAction("Index", new { encodedAccountId = accountId, providerId = ukprn });
            }


            var providerAttributes = await _employerFeedbackOuterApi.GetAllAttributes();
            if (providerAttributes == null)
            {
                _logger.LogError($"Unable to load Provider Attributes from the database.");
                return RedirectToAction("Error", "Error");
            }

            var providerAttributesModel = providerAttributes.Select(s => new ProviderAttributeModel { Name = s.AttributeName }).ToList();

            var userId = GetUserId().Value;

            var feedbackSource = await _sessionService.Get<FeedbackSource>($"{userId}_FeedbackSource");

            var newSurveyModel = new SurveyModel
            {
                AccountId = postedModel.AccountId,
                Ukprn = postedModel.ProviderId,
                UserRef = userId,
                Submitted = false,
                ProviderName = postedModel.ProviderName,
                Attributes = providerAttributesModel,
                FeedbackSource = feedbackSource
            };

            await _sessionService.Set(userId.ToString(), newSurveyModel);

            return RedirectToAction("StartFeedback", new { postedModel.EncodedAccountId });
        }

        [Authorize(Policy = nameof(PolicyNames.NoneRole))]
        [Route("/{encodedAccountId}/landing", Name = RouteNames.Landing_Get)]
        [HttpGet]
        public async Task<IActionResult> StartFeedback()
        {
            _logger.LogInformation("StartFeedback called");
            var surveyModel = await _sessionService.Get<SurveyModel>(User.FindFirst(EmployerClaims.UserId).Value);

            if (surveyModel == null)
            {
                return NotFound();
            }

            ViewData.Add("ProviderName", surveyModel.ProviderName);
            return View("StartFeedback");
        }

        private PagingState GetPagingState()
        {
            var userId = _userService.GetUserId();
            var pagingState = _sessionService.Get<PagingState>($"{userId}_PagingState").Result;
            if (null == pagingState)
            {
                pagingState = new PagingState();
            }
            return pagingState;
        }

        private async void SetPagingState(PagingState pagingState)
        {
            var userId = _userService.GetUserId();
            await _sessionService.Set($"{userId}_PagingState", pagingState);
        }
    }
}
