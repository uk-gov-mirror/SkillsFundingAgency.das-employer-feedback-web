using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.Employer.Shared.UI;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.SessionStorage;
using SFA.DAS.EmployerFeedback.Infrastructure;
using SFA.DAS.EmployerFeedback.Web.Authorization;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration.Routing;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFeedback.Web.Controllers
{
    [Authorize(Policy = nameof(PolicyNames.OwnerRole))]
    [Route(RoutePrefixPaths.FeedbackRoutePath)]
    [ServiceFilter(typeof(EnsureSessionExists))]
    public class ConfirmationController : Controller
    {
        private readonly ISessionStorageService _sessionService;
        private readonly ILogger<ConfirmationController> _logger;
        private readonly EmployerFeedbackWebConfiguration _config;
        private readonly UrlBuilder _urlBuilder;
        
        public ConfirmationController(
            ISessionStorageService sessionService,
            EmployerFeedbackWebConfiguration config,
            UrlBuilder urlBuilder,
            ILogger<ConfirmationController> logger)
        {
            _sessionService = sessionService;
            _logger = logger;
            _config = config;
            _urlBuilder = urlBuilder;
        }

        [HttpGet("feedback-confirmation", Name = RouteNames.Confirmation_Get)]
        public async Task<IActionResult> Index(string encodedAccountId)
        {
            var idClaim = HttpContext.User.FindFirst(EmployerClaims.UserId);
            var surveyModel = await _sessionService.Get<SurveyModel>(idClaim.Value);
            var providerCount = await _sessionService.Get<int>($"{idClaim.Value}_ProviderCount");
            await _sessionService.Remove($"{idClaim.Value}_PagingState");  // remove paging state incase we loop round for another provider
            var hasMultipleProviders = providerCount > 0;
            

            var confirmationVm = new ConfirmationViewModel
            {
                ProviderName = surveyModel.ProviderName,
                FeedbackRating = surveyModel.Rating.Value,
                FatUrl = _config.ExternalLinks.FindApprenticeshipTrainingSiteUrl,
                ComplaintSiteUrl = _config.ExternalLinks.ComplaintSiteUrl,
                ComplaintToProviderSiteUrl = _config.ExternalLinks.ComplaintToProviderSiteUrl,
                HasMultipleProviders = hasMultipleProviders,
                EncodedAccountId = encodedAccountId,
                EmployerAccountsHomeUrl = _urlBuilder.AccountsLink("AccountsHome", encodedAccountId)
            };

            return View(confirmationVm);
        }
    }
}