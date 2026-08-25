#!/usr/bin/env python3
"""
Phase 2 patch applier — Audit Implementation (backend only, zero DB migrations).

SCOPE (confirmed with the user: Layers 1+2+3, backend only, ignore frontend):

  Layer 1 — Harden ICurrentUserService:
    * Remove the settable IsAuthenticated (it was already a no-op setter —
      `set { }` — so removing it changes no behavior, only removes a footgun).
    * Add RequireUserId() for call sites that cannot proceed anonymously.
    * Cache the UserId resolution once per access instead of re-walking every
      claim and logging on every single call (this was a hot path).
    * Trim logging so claim VALUES are never written to logs.

  Layer 2 — BaseApiController gets a CurrentUserId accessor, exactly as
    requested. EmployeeController and PermissionController (the two existing
    BaseApiController consumers) get their constructors updated to match.
    ProfileController is migrated onto BaseApiController.

  Layer 3 — Fix the THREE confirmed instances of client-supplied audit
    identity, using dependencies that ALREADY EXIST in each service (no new
    constructor parameters except where noted, no new DB columns):
    * ProductServcie.EditProduct / AddNewProduct — used dto.updateBy /
      dto.createBy (client string) despite ICurrentUserService already being
      injected and unused.
    * PurchaseInvoiceService.EditPurchaseInvoice — used dto.updatedBy.
      REQUIRES a constructor change (ICurrentUserService was not previously
      injected here) — see the ServiceManager note below.
    * salesInvoiceService — resolved the acting user via
      `req.updateBy.Contains(u.FullName)`, a substring match against a
      client-supplied string, instead of using the ICurrentUserService that
      was already injected and already unused for this purpose.

DELIBERATELY NOT IN THIS PATCH (see PHASE2_DEFERRED.md for why):
  * StoreTransactionService.MakeTransactionUser — the exact master-row
    creation block was not retrievable with full confidence; patching it
    blind risks corrupting a working money/stock code path. Needs a
    dedicated follow-up with the real file open.
  * Full IAuditableEntity adoption across BaseInvoice (would need new
    DeleteBy/DeleteAt columns + a migration) and a SaveChangesInterceptor.
    This is real future work, scoped out of THIS delivery to keep it
    zero-migration and low-risk, per the same discipline that worked for
    Phase 1.

USAGE:
    python apply_phase2.py --repo "C:\\path\\to\\AlSadat-Seram" --dry-run
    python apply_phase2.py --repo "C:\\path\\to\\AlSadat-Seram"

Same anchor-based, fail-loud design as apply_phase1.py: every replacement is
matched against text taken verbatim from what was retrieved from the repo.
If your file doesn't contain that exact block, the script stops and tells
you which file and section — it never guesses or partially applies a hunk.
"""
import argparse
import sys
from pathlib import Path

# ============================================================================
# FULL FILE REWRITES — small files rewritten wholesale rather than patched,
# because the changes touch nearly the whole file (ICurrentUserService and
# its implementation). Guarded by a content check so re-running is safe.
# ============================================================================

FULL_REWRITES = {

"Application/Services.contract/CurrentUserService/ICurrentUserService.cs": '''using System.Security.Claims;

namespace Application.Services.contract.CurrentUserService;

/// <summary>
/// Resolves the authenticated user EXCLUSIVELY from the request's claims
/// principal (the JWT). Nothing in this codebase should identify "who is
/// acting" any other way — not a DTO field, not a display name matched
/// against the database, not localStorage. Services depend on this
/// interface directly; BaseApiController exposes a thin accessor over it
/// for controller-level concerns.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>ApplicationUser.Id resolved from the token's NameIdentifier /
    /// sub claim. Null when the request is unauthenticated.</summary>
    string? UserId { get; }

    ClaimsPrincipal? UserPrincipal { get; }

    /// <summary>True when the current request carries an authenticated principal.</summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Returns <see cref="UserId"/> or throws <see cref="System.InvalidOperationException"/>
    /// when it is null. For call sites where proceeding without an authenticated
    /// user would be a bug, not a business-rule failure to report politely.
    /// </summary>
    string RequireUserId();
}
''',

"Infrastructure/Services/CurrentUserServices/CurrentUserService.cs": '''using Application.Services.contract.CurrentUserService;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace Infrastructure.Services.CurrentUserServices;

/// <summary>
/// Resolves the current user from <see cref="IHttpContextAccessor"/> only.
/// <para>
/// Resolution runs at most once per instance (this service is registered
/// per-request/scoped via the ServiceManager's Lazy&lt;T&gt; — see
/// ServiceManager.cs) and the result is cached in <see cref="_userId"/>,
/// so repeated reads within one request do not re-walk every claim or
/// re-log on every access.
/// </para>
/// <para>
/// Claim VALUES are never logged — only whether resolution succeeded —
/// because this is a hot path and claim values are exactly the kind of
/// data that should not end up in application logs.
/// </para>
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CurrentUserService> _logger;

    private bool _resolved;
    private string? _userId;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        ILogger<CurrentUserService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public string? UserId
    {
        get
        {
            if (_resolved)
                return _userId;

            _resolved = true;

            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
                return _userId = null;

            var claims = user.Claims;

            _userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
                   ?? claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value
                   ?? claims.FirstOrDefault(c => c.Type == "UserId")?.Value
                   ?? claims.FirstOrDefault(c => c.Type ==
                        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            if (_userId is null)
                _logger.LogWarning("CurrentUserService: authenticated principal had no resolvable UserId claim.");

            return _userId;
        }
    }

    public bool IsAuthenticated
        => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public ClaimsPrincipal? UserPrincipal
        => _httpContextAccessor.HttpContext?.User;

    public string RequireUserId()
        => UserId ?? throw new InvalidOperationException(
            "CurrentUserService.RequireUserId() was called without an authenticated user. " +
            "This path must be reached only from an [Authorize]-protected action.");
}
''',
}

# ============================================================================
# ANCHOR-BASED REPLACEMENTS — old text taken verbatim from what was
# retrieved from the repository.
# ============================================================================

REPLACEMENTS = {

# ----------------------------------------------------------------------
# LAYER 2 — BaseApiController
# ----------------------------------------------------------------------
"AlSadat-Seram.Api/Controllers/BaseApiController.cs": [
(
'''using Application.Services.contract.LocalizationService;
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
}''',
'''using Application.Services.contract.CurrentUserService;
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
}''',
),
],

# ----------------------------------------------------------------------
# LAYER 2 — existing BaseApiController consumers: constructor updates
# ----------------------------------------------------------------------
"AlSadat-Seram.Api/Controllers/EmployeeController.cs": [
(
'''using Application.CommonPagination;
using Application.DTOs.EmployeeSalary;
using Application.Helper;
using Application.Services.contract;
using Application.Services.contract.LocalizationService;
using Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

// new
namespace AlSadat_Seram.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class EmployeeController : BaseApiController
{
    private readonly IServiceManager _ServiceManager;

    public EmployeeController(IServiceManager serviceManager, ILocalizationService localization)
        : base(localization)
    {
        _ServiceManager = serviceManager;
    }''',
'''using Application.CommonPagination;
using Application.DTOs.EmployeeSalary;
using Application.Helper;
using Application.Services.contract;
using Application.Services.contract.CurrentUserService;
using Application.Services.contract.LocalizationService;
using Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

// new
namespace AlSadat_Seram.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class EmployeeController : BaseApiController
{
    private readonly IServiceManager _ServiceManager;

    public EmployeeController(
        IServiceManager serviceManager,
        ILocalizationService localization,
        ICurrentUserService currentUser)
        : base(localization, currentUser)
    {
        _ServiceManager = serviceManager;
    }''',
),
],

"AlSadat-Seram.Api/Controllers/PermissionController.cs": [
(
'''using Application.DTOs.Authorization;
using Application.Services.contract;
using Application.Services.contract.LocalizationService;
using Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlSadat_Seram.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionController : BaseApiController
    {
        private readonly IServiceManager _serviceManager;

        public PermissionController(IServiceManager serviceManager, ILocalizationService localization)
            : base(localization)
        {
            _serviceManager = serviceManager;
        }''',
'''using Application.DTOs.Authorization;
using Application.Services.contract;
using Application.Services.contract.CurrentUserService;
using Application.Services.contract.LocalizationService;
using Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlSadat_Seram.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionController : BaseApiController
    {
        private readonly IServiceManager _serviceManager;

        public PermissionController(
            IServiceManager serviceManager,
            ILocalizationService localization,
            ICurrentUserService currentUser)
            : base(localization, currentUser)
        {
            _serviceManager = serviceManager;
        }''',
),
],

# ----------------------------------------------------------------------
# LAYER 2 — ProfileController migrated onto BaseApiController.
# Only the class declaration + constructor change. Action bodies are left
# exactly as-is (they already use their own IsSuccess ? Ok : StatusCode
# pattern, which still works fine on BaseApiController — HandleResult is
# available for future use but not forced here).
# ----------------------------------------------------------------------
"AlSadat-Seram.Api/Controllers/ProfileController.cs": [
(
'''using Application.DTOs.Profile;
using Application.Services.contract;
using Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlSadat_Seram.Api.Controllers
{
    /// <summary>
    /// Endpoints for the currently-authenticated user to manage their own profile.
    /// <para>
    /// No role restriction — every authenticated user has a profile. There is
    /// intentionally no admin-edit-other-user path here; that belongs to a
    /// separate UserAdminController if/when needed.
    /// </para>
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;

        public ProfileController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }''',
'''using Application.DTOs.Profile;
using Application.Services.contract;
using Application.Services.contract.CurrentUserService;
using Application.Services.contract.LocalizationService;
using Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlSadat_Seram.Api.Controllers
{
    /// <summary>
    /// Endpoints for the currently-authenticated user to manage their own profile.
    /// <para>
    /// No role restriction — every authenticated user has a profile. There is
    /// intentionally no admin-edit-other-user path here; that belongs to a
    /// separate UserAdminController if/when needed.
    /// </para>
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : BaseApiController
    {
        private readonly IServiceManager _serviceManager;

        public ProfileController(
            IServiceManager serviceManager,
            ILocalizationService localization,
            ICurrentUserService currentUser)
            : base(localization, currentUser)
        {
            _serviceManager = serviceManager;
        }''',
),
],

# ----------------------------------------------------------------------
# LAYER 3 — ProductServcie: stop trusting dto.updateBy / dto.createBy
# ----------------------------------------------------------------------
"Infrastructure/Services/ProductServcie.cs": [
(
'''        public async Task AddNewProduct(ProductDto product)
        {
            var AddedProduct = FromDto(product);


            await unitOfWork.GetRepository<Products, int>().AddAsync(AddedProduct);
        }

        public async Task EditProduct(ProductDto dto)
        {
            var UpdatedProduct= await unitOfWork.GetRepository<Products, int>().FindAsync(c => c.Id == dto.id);
            UpdatedProduct.Name = dto.name;
            UpdatedProduct.SellingPrice = dto.sellingPrice;
            UpdatedProduct.PointPerUnit = dto.pointPerUnit;
            UpdatedProduct.UpdateBy = dto.updateBy;
            UpdatedProduct.UpdateAt = dto.updateAt;
            UpdatedProduct.IsDeleted = dto.isDeleted;
            UpdatedProduct.productCode = dto.productCode;''',
'''        public async Task AddNewProduct(ProductDto product)
        {
            var AddedProduct = FromDto(product);

            // Audit identity is resolved EXCLUSIVELY from the token, never from the
            // DTO. AddedProduct.CreateAt already defaults via the entity's own
            // property initializer, but we set it explicitly here too so the
            // create and edit paths are symmetric and neither depends on a
            // client-supplied timestamp.
            AddedProduct.CreateBy = currentUserService.UserId;
            AddedProduct.CreateAt = DateTime.UtcNow;

            await unitOfWork.GetRepository<Products, int>().AddAsync(AddedProduct);
        }

        public async Task EditProduct(ProductDto dto)
        {
            var UpdatedProduct= await unitOfWork.GetRepository<Products, int>().FindAsync(c => c.Id == dto.id);
            UpdatedProduct.Name = dto.name;
            UpdatedProduct.SellingPrice = dto.sellingPrice;
            UpdatedProduct.PointPerUnit = dto.pointPerUnit;
            // Audit identity resolved from the token (ICurrentUserService), never
            // from dto.updateBy — that field was previously a raw client-supplied
            // string (localStorage userName|userEmail on the Angular side).
            UpdatedProduct.UpdateBy = currentUserService.UserId;
            UpdatedProduct.UpdateAt = DateTime.UtcNow;
            UpdatedProduct.IsDeleted = dto.isDeleted;
            UpdatedProduct.productCode = dto.productCode;''',
),
],

# ----------------------------------------------------------------------
# LAYER 3 — PurchaseInvoiceService: inject ICurrentUserService, stop
# trusting dto.updatedBy.
# ----------------------------------------------------------------------
"Infrastructure/Services/PurchaseInvoiceService.cs": [
(
'''    public class PurchaseInvoiceService : IPurchaseInvoiceContract
    {
        private readonly IUnitOfWork unitOfWork;
        public PurchaseInvoiceService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }''',
'''    public class PurchaseInvoiceService : IPurchaseInvoiceContract
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly Application.Services.contract.CurrentUserService.ICurrentUserService _currentUserService;
        public PurchaseInvoiceService(
            IUnitOfWork unitOfWork,
            Application.Services.contract.CurrentUserService.ICurrentUserService currentUserService)
        {
            this.unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }''',
),
(
'''                invoice.UpdateBy = dto.updatedBy;
                invoice.UpdateAt = DateTime.UtcNow;
                // Remove old items''',
'''                // Audit identity resolved from the token, never from dto.updatedBy
                // (that field was a raw client-supplied "userName|userEmail" string).
                invoice.UpdateBy = _currentUserService.UserId;
                invoice.UpdateAt = DateTime.UtcNow;
                // Remove old items''',
),
],

# ----------------------------------------------------------------------
# LAYER 3 — salesInvoiceService: stop resolving the acting user by
# substring-matching a client-supplied string against every user's FullName.
# ----------------------------------------------------------------------
"Infrastructure/Services/SalesInvoiceService/salesInvoiceService.cs": [
(
'''                var CurrentUser = await unitOfWork.GetRepository<ApplicationUser, string>().FindAsync(u => req.updateBy.Contains(u.FullName));
                if(CurrentUser == null)
                    return Result<string>.Failure("لا يمكن ايجاد المستخدم الحالي في قاعدة البيانات", HttpStatusCode.BadRequest);''',
'''                // Audit identity resolved from the token via the already-injected
                // ICurrentUserService, never from req.updateBy — that field was a
                // client-supplied string previously matched with a Contains() scan
                // over every user's FullName, which is both wrong (substring match,
                // first-match-wins) and forgeable (the client controls req.updateBy).
                var currentUserId = currentUserService.UserId;
                if (string.IsNullOrEmpty(currentUserId))
                    return Result<string>.Failure("يجب تسجيل الدخول أولاً", HttpStatusCode.Unauthorized);

                var CurrentUser = await unitOfWork.GetRepository<ApplicationUser, string>().GetByIdAsync(currentUserId);
                if(CurrentUser == null)
                    return Result<string>.Failure("لا يمكن ايجاد المستخدم الحالي في قاعدة البيانات", HttpStatusCode.BadRequest);''',
),
],

}

# ============================================================================
# BEST-EFFORT — the ServiceManager registration line for PurchaseInvoiceService
# was not confirmed verbatim from the repository (only the field declaration
# was). This is attempted with a reasonable guess at the exact text; if it
# doesn't match, the script reports it cleanly and PHASE2_DEFERRED.md has the
# one-line manual fix.
# ============================================================================

SERVICEMANAGER_PATH = "Infrastructure/Services/ServiceManager.cs"
SERVICEMANAGER_OLD_GUESS = (
    "_purchaseInvoiceService = new Lazy<IPurchaseInvoiceContract>"
    "(() => new PurchaseInvoiceService(UnitOfWork));"
)
SERVICEMANAGER_NEW = (
    "_purchaseInvoiceService = new Lazy<IPurchaseInvoiceContract>"
    "(() => new PurchaseInvoiceService(UnitOfWork, _CurrentUserService.Value));"
)


def write_full_rewrite(repo: Path, rel_path: str, content: str, dry_run: bool) -> str:
    target = repo / rel_path
    if not target.exists():
        return f"MISSING FILE — cannot rewrite  {rel_path}"
    existing = target.read_text(encoding="utf-8")
    if existing.strip() == content.strip():
        return f"SKIP  (already rewritten, identical)  {rel_path}"
    if not dry_run:
        target.write_text(content, encoding="utf-8", newline="\n")
    return f"{'WOULD REWRITE' if dry_run else 'REWROTE'}  {rel_path}"


def apply_replacement(repo: Path, rel_path: str, old: str, new: str, dry_run: bool) -> str:
    target = repo / rel_path
    if not target.exists():
        return f"MISSING FILE — cannot patch  {rel_path}"

    text = target.read_text(encoding="utf-8")

    if new.strip() in text:
        return f"SKIP  (already patched)  {rel_path}"

    if old.strip() not in text:
        return f"ANCHOR NOT FOUND — file has diverged, patch manually  {rel_path}"

    if text.count(old.strip()) > 1:
        return f"AMBIGUOUS — anchor appears more than once, patch manually  {rel_path}"

    new_text = text.replace(old, new, 1) if old in text else text
    if new_text == text:
        return f"ANCHOR WHITESPACE MISMATCH — patch manually  {rel_path}"

    if not dry_run:
        target.write_text(new_text, encoding="utf-8", newline="\n")
    return f"{'WOULD PATCH' if dry_run else 'PATCHED'}  {rel_path}"


def apply_servicemanager_best_effort(repo: Path, dry_run: bool) -> str:
    target = repo / SERVICEMANAGER_PATH
    if not target.exists():
        return f"MISSING FILE — apply manually per PHASE2_DEFERRED.md  {SERVICEMANAGER_PATH}"

    text = target.read_text(encoding="utf-8")

    if SERVICEMANAGER_NEW in text:
        return f"SKIP  (already patched)  {SERVICEMANAGER_PATH}"

    if SERVICEMANAGER_OLD_GUESS not in text:
        return (f"BEST-EFFORT ANCHOR NOT FOUND (expected) — apply the one-line fix "
                f"in PHASE2_DEFERRED.md by hand  {SERVICEMANAGER_PATH}")

    new_text = text.replace(SERVICEMANAGER_OLD_GUESS, SERVICEMANAGER_NEW, 1)
    if not dry_run:
        target.write_text(new_text, encoding="utf-8", newline="\n")
    return f"{'WOULD PATCH' if dry_run else 'PATCHED'}  {SERVICEMANAGER_PATH}"


def main():
    parser = argparse.ArgumentParser(description="Apply Phase 2 (audit implementation, backend only) to the repo.")
    parser.add_argument("--repo", required=True, help="Path to the repo root (folder containing AlSadat-Seram.slnx)")
    parser.add_argument("--dry-run", action="store_true", help="Report what would happen without writing files")
    args = parser.parse_args()

    repo = Path(args.repo)
    if not (repo / "AlSadat-Seram.slnx").exists():
        print(f"ERROR: {repo} does not contain AlSadat-Seram.slnx — check --repo path.")
        sys.exit(1)

    results = []

    for rel_path, content in FULL_REWRITES.items():
        results.append(write_full_rewrite(repo, rel_path, content, args.dry_run))

    for rel_path, hunks in REPLACEMENTS.items():
        for old, new in hunks:
            results.append(apply_replacement(repo, rel_path, old, new, args.dry_run))

    results.append(apply_servicemanager_best_effort(repo, args.dry_run))

    print("\n".join(results))

    problems = [r for r in results if any(
        tag in r for tag in ("ANCHOR NOT FOUND", "MISSING FILE", "AMBIGUOUS", "MISMATCH", "BEST-EFFORT")
    )]
    print()
    if problems:
        print(f"{len(problems)} item(s) need manual attention — see PHASE2_DEFERRED.md.")
        sys.exit(2)
    else:
        print("All Phase 2 changes applied cleanly." if not args.dry_run else "Dry run: all anchors found, nothing written.")


if __name__ == "__main__":
    main()
