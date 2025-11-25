using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerFeedback.Web.Models.Error;

namespace SFA.DAS.EmployerFeedback.Web.Controllers
{
    [Route("")]
    public class ErrorController : Controller
    {
        #region Routes
        public const string ErrorGet = nameof(ErrorGet);
        #endregion

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(IHttpContextAccessor httpContextAccessor, ILogger<ErrorController> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        [HttpGet]
        [Route("error/{id?}", Name = ErrorGet)]
        public IActionResult Error(int? id)
        {
            var feature = HttpContext.Features.Get<IExceptionHandlerFeature>();

            if (feature?.Error != null)
            {
                _logger.LogError(feature.Error, "Unhandled exception");
            }

            if (id == 404)
            {
                Response.StatusCode = StatusCodes.Status404NotFound;
                return View("PageNotFound");
            }

            Response.StatusCode = id ?? StatusCodes.Status500InternalServerError;
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? _httpContextAccessor.HttpContext.TraceIdentifier });
        }
    }
}