using Application.Services.contract.CurrentUserService;
using Application.Services.contract.LocalizationService;
using Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace AlSadat_Seram.Api.Controllers
{
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        protected readonly ILocalizationService Localization;
        private readonly ICurrentUserService _currentUser;

        protected BaseApiController(ILocalizationService localization, ICurrentUserService currentUser)
        {
            Localization = localization;
            _currentUser = currentUser;
        }

        /// <summary>
        /// The authenticated user's id, resolved exclusively from the token via
        /// ICurrentUserService. Null only on an [AllowAnonymous] action that was
        /// actually reached anonymously.
        /// </summary>
        protected string? CurrentUserId => _currentUser.UserId;

        /// <summary>
        /// Same as <see cref="CurrentUserId"/> but throws if the request is
        /// unauthenticated. Use only in actions that require [Authorize].
        /// </summary>
        protected string RequireCurrentUserId() => _currentUser.RequireUserId();

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