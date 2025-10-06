using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.Employer.Shared.UI;
using SFA.DAS.EmployerFeedback.Domain.Types;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.SessionStorage;
using SFA.DAS.EmployerFeedback.Web.Authorization;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerFeedback.Web.Paging;
using SFA.DAS.EmployerFeedback.Web.ViewModels;
using SFA.DAS.EmployerProvideFeedback.Services;
using SFA.DAS.Encoding;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFeedback.Web.Controllers
{


    [Authorize(Policy = nameof(PolicyNames.NoneRole))]
    public class ProviderController : Controller
    {
        private readonly ISessionStorageService _sessionService;
        private readonly ITrainingProviderService _trainingProviderService;
        private readonly ILogger<ProviderController> _logger;
        private readonly IEncodingService _encodingService;
        private readonly IEmployerFeedbackOuterApi _employerFeedbackOuterApi;
        private readonly UrlBuilder _urlBuilder;
        public ProviderController(ISessionStorageService sessionService,
            ITrainingProviderService trainingProviderService,
            IEncodingService encodingService, 
            ILogger<ProviderController> logger,
            IEmployerFeedbackOuterApi employerFeedbackOuterApi,
            UrlBuilder urlBuilder
            )
        {
            _sessionService = sessionService;
            _trainingProviderService = trainingProviderService;
            _encodingService = encodingService;
            _logger = logger;
            _employerFeedbackOuterApi = employerFeedbackOuterApi;
            _urlBuilder = urlBuilder;
        }

        [HttpGet]
        [Route("/{encodedAccountId}/providers")]
        public async Task<IActionResult> Index(GetProvidersForFeedbackRequest request, int pageIndex = PagingState.DefaultPageIndex, FeedbackSource source = FeedbackSource.AdHoc)
        {
            var idClaim = HttpContext.User.FindFirst(EmployerClaims.UserId);
            var pagingState = await _sessionService.Get<PagingState>($"{idClaim.Value}_PagingState");
            if(null == pagingState)
            {
                pagingState = new PagingState();
            }
            pagingState.PageIndex = pageIndex;
            await _sessionService.Set($"{idClaim.Value}_PagingState", pagingState);

            var model = await _trainingProviderService.GetTrainingProviderSearchViewModel(
                request.EncodedAccountId,
                Guid.Parse(idClaim.Value),
                pagingState.SelectedProviderName,
                pagingState.SelectedFeedbackStatus,
                pagingState.PageSize,
                pagingState.PageIndex,
                pagingState.SortColumn,
                pagingState.SortDirection);
            model.ChangePageAction = nameof(Index);

            ViewBag.EmployerAccountsHomeUrl = _urlBuilder.AccountsLink("AccountsHome", request.EncodedAccountId);

            await _sessionService.Set($"{idClaim.Value}_ProviderCount", model.TrainingProviders.TotalRecordCount);
            await _sessionService.Set($"{idClaim.Value}_FeedbackSource", source);


            return View(model);
        }

        [HttpPost]
        [Route("/{encodedAccountId}/providers")]
        public async Task<IActionResult> Filter(ProviderSearchViewModel postedModel)
        {
            var idClaim = HttpContext.User.FindFirst(EmployerClaims.UserId);
            var pagingState = await _sessionService.Get<PagingState>($"{idClaim.Value}_PagingState");
            if (null == pagingState)
            {
                pagingState = new PagingState();
            }
            pagingState.PageIndex = PagingState.DefaultPageIndex; // applying filter resets the paging
            pagingState.SelectedProviderName = postedModel.SelectedProviderName;
            pagingState.SelectedFeedbackStatus = postedModel.SelectedFeedbackStatus;
            await _sessionService.Set($"{idClaim.Value}_PagingState", pagingState);

            var model = await _trainingProviderService.GetTrainingProviderSearchViewModel(
                postedModel.EncodedAccountId,
                Guid.Parse(idClaim.Value),
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
            var idClaim = HttpContext.User.FindFirst(EmployerClaims.UserId);
            var pagingState = await _sessionService.Get<PagingState>($"{idClaim.Value}_PagingState");
            if (null == pagingState)
            {
                pagingState = new PagingState();
            }
            pagingState.SortColumn = sortColumn;
            pagingState.SortDirection = sortDirection;
            await _sessionService.Set($"{idClaim.Value}_PagingState", pagingState);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Route("/{encodedAccountId}/providers/unfilter")]
        public async Task<IActionResult> ClearFilters(string encodedAccountId)
        {
            var idClaim = HttpContext.User.FindFirst(EmployerClaims.UserId);
            var pagingState = await _sessionService.Get<PagingState>($"{idClaim.Value}_PagingState");
            if (null == pagingState)
            {
                pagingState = new PagingState();
            }
            pagingState.SelectedProviderName = string.Empty;
            pagingState.SelectedFeedbackStatus = string.Empty;
            await _sessionService.Set($"{idClaim.Value}_PagingState", pagingState);

            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        [Route("/{encodedAccountId}/providers/{providerId}")]
        public async Task<IActionResult> ConfirmProvider(string encodedAccountId, long providerId)
        {
            var idClaim = HttpContext.User.FindFirst(EmployerClaims.UserId);
            if (null == idClaim)
            {
                _logger.LogError($"User id not found in user claims.");
                return RedirectToAction("Error", "Error");
            }

            var accountId = _encodingService.Decode(encodedAccountId, EncodingType.AccountId); // validate the account id

            var model = await _trainingProviderService.GetTrainingProviderConfirmationViewModel(accountId, Guid.Parse(idClaim?.Value), providerId);

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
                return RedirectToAction("Index");
            }


            var providerAttributes = await _employerFeedbackOuterApi.GetAllAttributes();
            if (providerAttributes == null)
            {
                _logger.LogError($"Unable to load Provider Attributes from the database.");
                return RedirectToAction("Error", "Error");
            }

            var providerAttributesModel = providerAttributes.Select(s => new Models.Shared.ProviderAttributeModel { Name = s.AttributeName }).ToList();

            var idClaim = HttpContext.User.FindFirst(EmployerClaims.UserId);
            if (null == idClaim)
            {
                _logger.LogError($"User id not found in user claims.");
                return RedirectToAction("Error", "Error");
            }

            var feedbackSource = await _sessionService.Get<FeedbackSource>($"{idClaim.Value}_FeedbackSource");

            var newSurveyModel = new SurveyModel
            {
                AccountId = _encodingService.Decode(postedModel.EncodedAccountId, EncodingType.AccountId),
                Ukprn = postedModel.ProviderId,
                UserRef = new Guid(idClaim?.Value),
                Submitted = false,
                ProviderName = postedModel.ProviderName,
                Attributes = providerAttributesModel,
                FeedbackSource = feedbackSource
            };

            await _sessionService.Set(idClaim.Value, newSurveyModel);

            return RedirectToAction("Index", "Home", new { postedModel.EncodedAccountId });
        }
    }
}
