using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.Employer.Shared.UI;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration.Routing;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.UserService;
using SFA.DAS.EmployerFeedback.Web.Authorization;
using SFA.DAS.EmployerFeedback.Web.Models.Confirmation;
using SFA.DAS.EmployerFeedback.Web.Services;
using SFA.DAS.EmployerFeedback.Web.Services.SessionStorage;

namespace SFA.DAS.EmployerFeedback.Web.Controllers
{
    [Authorize(Policy = nameof(PolicyNames.NoneRole))]
    [Route(RoutePrefixPaths.FeedbackRoutePath)]
    [ServiceFilter(typeof(EnsureSessionExistsAttribute))]
    public class ConfirmationController : ControllerBase
    {
        #region Routes
        public const string ConfirmationGet = nameof(ConfirmationGet);
        #endregion

        private readonly ISessionStorageService _sessionService;
        private readonly ILogger<ConfirmationController> _logger;
        private readonly EmployerFeedbackWebConfiguration _config;
        private readonly IAccountsLinkService _accountsLinkService;

        public ConfirmationController(
            ISessionStorageService sessionService,
            EmployerFeedbackWebConfiguration config,
            IAccountsLinkService accountsLinkService,
            ILogger<ConfirmationController> logger,
            IUserService userService) : base(userService, logger)
        {
            _sessionService = sessionService;
            _logger = logger;
            _config = config;
            _accountsLinkService = accountsLinkService;
        }

        [HttpGet]
        [Route("feedback-confirmation", Name = ConfirmationGet)]
        public async Task<IActionResult> Index(string encodedAccountId)
        {
            var userId = GetUserId();

            var surveyModel = await _sessionService.GetSurveyModel(userId);
            var providers = await _sessionService.GetProviders(userId);
            await _sessionService.SetPagingState(userId, null);
            var hasMultipleProviders = providers.Count > 0;

            var confirmationVm = new ConfirmationViewModel
            {
                ProviderName = surveyModel.ProviderName,
                FeedbackRating = surveyModel.Rating.Value,
                FatUrl = _config.ExternalLinks.FindApprenticeshipTrainingSiteUrl,
                ComplaintSiteUrl = _config.ExternalLinks.ComplaintSiteUrl,
                ComplaintToProviderSiteUrl = _config.ExternalLinks.ComplaintToProviderSiteUrl,
                HasMultipleProviders = hasMultipleProviders,
                EncodedAccountId = encodedAccountId,
                EmployerAccountsHomeUrl = _accountsLinkService.AccountsHome(encodedAccountId)
            };

            return View(confirmationVm);
        }
    }
}