using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RestEase;
using SFA.DAS.EmployerFeedback.Infrastructure.Api;

namespace SFA.DAS.EmployerFeedback.Web.Controllers
{
    public class TestController : Controller
    {
        private readonly ILogger<TestController> _logger;
        private readonly ICommitmentsOuterApi _outerApi;
        public TestController(ICommitmentsOuterApi outerApi, ILogger<TestController> logger)
        {
            this._outerApi = outerApi;
            this._logger = logger;
        }

        [Get("")]
        public IActionResult Index()
        {
            _logger.LogInformation("TestController Index called");
            var result = this._outerApi.GetLearners(1,10).GetAwaiter().GetResult();
            return View();
        }
    }
}
