using Application.Services.contract.LocalizationService;
using Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace AlSadat_Seram.Api.Controllers
{
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        protected readonly ILocalizationService Localization;

        protected BaseApiController(ILocalizationService localization)
        {
            Localization = localization;
        }

        /// <summary>
        /// Resolves MessageKey (if set) to localized text, then returns the result
        /// with its own StatusCode — success and failure both flow through here,
        /// no branching needed at the call site.
        /// </summary>
        protected IActionResult HandleResult<T>(Result<T> result)
        {
            if (!string.IsNullOrWhiteSpace(result.MessageKey))
                result.Message = Localization.Resolve(result.MessageKey);

            return StatusCode((int)result.StatusCode, result);
        }
    }
}