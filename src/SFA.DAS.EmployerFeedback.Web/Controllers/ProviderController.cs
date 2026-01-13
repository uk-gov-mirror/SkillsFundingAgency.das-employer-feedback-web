using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration.Routing;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.UserService;
using SFA.DAS.EmployerFeedback.Web.Authorization;
using SFA.DAS.EmployerFeedback.Web.Models.Provider;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerFeedback.Web.Orchestrators;

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

        private readonly IProviderOrchestrator _providerOrchestrator;

        public ProviderController(
            IUserService userService,
            ILogger<ProviderController> logger,
            IProviderOrchestrator providerOrchestrator) 
            : base(userService, logger)
        {
            _providerOrchestrator = providerOrchestrator;
        }

        [HttpGet]
        [Route("providers", Name = ProviderSearchGet)]
        public async Task<IActionResult> ProviderSearch(ProviderSearchRequestModel model)
        {
            if (model.FeedbackSource.HasValue)
            {
                await _providerOrchestrator.SetFeedbackSource(model.FeedbackSource.Value);
            }

            await _providerOrchestrator.SetProviderSearchPageIndex(model.PageIndex);

            var viewModel = await _providerOrchestrator.GetProviderSearchViewModel(model);
            await _providerOrchestrator.SetProviders(viewModel.Providers.Items);
            
            return View(viewModel);
        }

        [HttpPost]
        [Route("providers", Name = ProviderSearchPost)]
        public async Task<IActionResult> ProviderSearch(ProviderSearchViewModel viewModel)
        {
            await _providerOrchestrator.UpdateProviderSearchFilters(viewModel);
            return RedirectToRoute(ProviderSearchGet, new { viewModel.EncodedAccountId });
        }

        [HttpGet]
        [Route("providers/sort", Name = SortProvidersGet)]
        public async Task<IActionResult> SortProviders(ProviderSearchSortRequestModel model)
        {
            await _providerOrchestrator.SortProviderSearch(model.SortColumn, model.SortOrder);
            return RedirectToRoute(ProviderSearchGet, new { model.EncodedAccountId, model.SortColumn, model.SortOrder });
        }

        [HttpGet]
        [Route("providers/clearfilters", Name = ClearFiltersGet)]
        public async Task<IActionResult> ClearFilters(AccountModel model)
        {
            await _providerOrchestrator.ClearProviderSearchFilters();
            return RedirectToRoute(ProviderSearchGet, new { model.EncodedAccountId });
        }

        [HttpGet]
        [Route("providers/{providerId}", Name = ProviderConfirmGet)]
        public async Task<IActionResult> ProviderConfirm(ProviderConfirmRequestModel model)
        {
            var viewModel = await _providerOrchestrator.GetProviderConfirmViewModel(model);
            if(viewModel == null)
            {
                return RedirectToRoute(ProviderSearchGet, new { encodedAccountId = model.EncodedAccountId });
            }

            return View(viewModel);
        }

        [HttpPost]
        [Route("providers/{providerId}", Name = ProviderConfirmPost)]
        public async Task<IActionResult> ProviderConfirm(ProviderConfirmViewModel viewModel)
        {
            if (!await _providerOrchestrator.ValidateProviderConfirmViewModel(viewModel, ModelState))
            {
                return RedirectToRoute(ProviderConfirmGet, new { encodedAccountId = viewModel.EncodedAccountId, providerId = viewModel.ProviderId });
            }

            if (!viewModel.Confirmed.Value)
            {
                return RedirectToRoute(ProviderSearchGet, new { encodedAccountId = viewModel.EncodedAccountId });
            }

            await _providerOrchestrator.CreateNewSurvey(viewModel);
            return RedirectToRoute(QuestionsController.StartFeedbackGet, new { viewModel.EncodedAccountId });
        }
    }
}