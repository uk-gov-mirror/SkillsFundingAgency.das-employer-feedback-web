using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace SFA.DAS.EmployerFeedback.Web.Controllers
{
    [Route("")]
    public class ErrorController : Controller
    {
        #region Routes
        public const string ErrorGet = nameof(ErrorGet);
        #endregion

        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("error/{id?}", Name = ErrorGet)]
        public IActionResult Error(int id)
        {
            switch (id)
            {
                case 404:
                    return PageNotFound();
                default:
                    break;
            }

            return View();
        }

        private IActionResult PageNotFound()
        {
            Response.StatusCode = (int)HttpStatusCode.NotFound;
            return View("PageNotFound");
        }
    }
}