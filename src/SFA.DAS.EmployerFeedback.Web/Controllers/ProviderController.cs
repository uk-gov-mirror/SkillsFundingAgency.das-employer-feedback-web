using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SFA.DAS.Employer.Shared.UI;
using SFA.DAS.EmployerFeedback.Domain.Types;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration.Routing;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.SessionStorage;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.UserService;
using SFA.DAS.EmployerFeedback.Web.Authorization;
using SFA.DAS.EmployerFeedback.Web.Configuration.Routing;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerFeedback.Web.Paging;
using SFA.DAS.EmployerFeedback.Web.ViewModels;
using SFA.DAS.EmployerFeedback.Services;
using System;
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
            try
            {
                var userId = GetUserId().Value;
                var pagingState = GetPagingState();
                pagingState.PageIndex = pageIndex;
                await SetPagingState(pagingState);

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Index");
                return RedirectToAction("Error", "Error");
            }

        }

        [HttpPost]
        [Route("/{encodedAccountId}/providers")]
        public async Task<IActionResult> Filter(ProviderSearchViewModel postedModel)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Filter");
                return RedirectToAction("Error", "Error");
            }
        }

        [HttpGet]
        [Route("/{encodedAccountId}/providers/sort")]
        public async Task<IActionResult> SortProviders(string encodedAccountId, string sortColumn, string sortDirection)
        {
            try
            {
                var pagingState = GetPagingState();
                pagingState.SortColumn = sortColumn;
                pagingState.SortDirection = sortDirection;
                await SetPagingState(pagingState);
                return RedirectToAction(nameof(Index), new { encodedAccountId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Provider Controller - SortProviders");
                return RedirectToAction("Error", "Error");
            }

        }

        [HttpGet]
        [Route("/{encodedAccountId}/providers/unfilter")]
        public async Task<IActionResult> ClearFilters(string encodedAccountId)
        {
            try
            {
                var pagingState = GetPagingState();
                pagingState.SelectedProviderName = string.Empty;
                pagingState.SelectedFeedbackStatus = string.Empty;
                await SetPagingState(pagingState);

                return RedirectToAction(nameof(Index), new { encodedAccountId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Provider Controller - ClearFilters");
                return RedirectToAction("Error", "Error");
            }
        }

        [HttpGet]
        [Route("/{encodedAccountId}/providers/{providerId}")]
        public async Task<IActionResult> ConfirmProvider(ProviderSearchConfirmationViewModel postedModel)
        {
            try
            {
                var userId = GetUserId().Value;
                var model = await _trainingProviderService.GetTrainingProviderConfirmationViewModel(postedModel.AccountId, userId, postedModel.ProviderId);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Provider Controller - ConfirmProvider");
                return RedirectToAction("Error", "Error");
            }

        }

        [HttpPost]
        [Route("/{encodedAccountId}/providers/{providerId}")]
        public async Task<IActionResult> ProviderConfirmed(ProviderSearchConfirmationViewModel postedModel)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Provider Controller - ProviderConfirmed");
                return RedirectToAction("Error", "Error");
            }
        }


        [Authorize(Policy = nameof(PolicyNames.NoneRole))]
        [Route("/{encodedAccountId}/landing", Name = RouteNames.Landing_Get)]
        [HttpGet]
        public async Task<IActionResult> StartFeedback()
        {
            try
            {
                _logger.LogInformation("StartFeedback called");
                var userId = GetUserId().Value;
                var surveyModel = await _sessionService.Get<SurveyModel>(userId.ToString());

                if (surveyModel == null)
                {
                    return NotFound();
                }

                ViewData.Add("ProviderName", surveyModel.ProviderName);
                return View("StartFeedback");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Provider Controller - StartFeedback");
                return RedirectToAction("Error", "Error");
            }
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

        private async Task SetPagingState(PagingState pagingState)
        {
            var userId = _userService.GetUserId();
            await _sessionService.Set($"{userId}_PagingState", pagingState);
        }
    }
}
