# Sadat Searm ERP — Project Context
_Last updated: 2026-11-27 (F7-S1 in progress) · Read this first in any new session · Update it with `/handoff` at the end of every session._

## Checkpoint (overwrite after every small part — see "Checkpoints" in CLAUDE.md)
- **Step:** F7-S1 — approvals engine (D4). All parts done; waiting for Belal to confirm, then `/handoff` (tracker «خلص» + commit list).
- **Done in this step:**
  - [x] plan + D39–D42 (`37f3811`)
  - [x] part 1 `ApprovalType`/`ApprovalGroup`/`ApprovalStatus`/`ApprovalTypes.GroupOf`, `ApprovalRequest`, `AuditAreas.Approvals`, `ConfigureApprovals`, migration `AddApprovalRequests` (`0a669af`)
  - [x] part 2 `IApprovalHandler`, `IApprovalService`, `ApprovalDtos`, `ApprovalRules` + 11 tests (`ca6440b`)
  - [x] part 3 `ApprovalErrorCodes`, `Approval.*` keys, `ApprovalService`, DI (`e21ca43`)
  - [x] part 4 `ApprovalsController` (`9914418`)
  - [x] part 5 40 checks on throwaway DB `SadatSearmErp_ApprovalTest` (dropped), README rewritten
- **In progress / next part:** Belal verifies (README › F7-S1 › إزاي تتأكد; restart the API so `AddApprovalRequests` applies). Then `/handoff`: tracker F7-S1 «خلص» with commits, Session log line, checkpoint → next session (`py -3 tools/tracker.py next`, expected F8-S1).
- **State on Belal's PC:** user secrets `ConnectionStrings:con` → `SadatSearmErp`. Throwaway-DB checks: .NET 10 file-based harness (`#:project` + `#:property PublishAot=false` + `#:property Configuration=Release`), dropped afterwards; test users need `PhoneNumber` and must exist (AuditEntries FK). Close `TRACKER.xlsx` in Excel before `tracker.py` writes. A running API locks `bin/Debug` → build/ef with `-c Release`.
- **Uncommitted changes:** none after the docs commit. Release build 0 errors, 64/64 tests (2026-11-27).
- **Decisions taken in this step (2026-11-27):** D39 (a) card built live by the handler; D40 nobody decides their own request except `super_admin`; D41 (a) bulk = one transaction per request + per-item result; D42 (a) engine only, real types with their modules.
- **Open questions:** draft vs confirm numbering — decide in M6/M7, recommended: follow the UI.

## Overview
ERP for Sadat Searm (sanitary-ware / plumbing products manufacturer & distributor): sales/purchase invoices, stock & warehouses,
treasury & journal entries, HR (employees, attendance, leaves, loans, payroll, deductions),
sales reps, merchants/distributors, plumbers loyalty (inspection → points → coupons → gifts/cash).
Phase: **backend refactor to serve the new UI** (the new UI in `Docs/` is the source of truth; the old Angular app is dropped).
Plan: `Docs/docs/refactor/PLAN.md` (online original: https://claude.ai/artifact/BbH94DNQysKSDn5vQnkR6w).

## Stack
- Backend: .NET 10 (`net10.0`, nullable on — since F1-S1), ASP.NET Core, EF Core 10.0.15 code-first, SQL Server,
  ASP.NET Identity + JWT bearer + refresh tokens, no AutoMapper (D19), no FluentValidation (removed F1-S1), QuestPDF,
  ClosedXML/EPPlus/ExcelDataReader. Target after F1: **.NET 10 LTS**.
- Frontend: Angular 17.3 (old, `New folder (6)/AlsadatUi`) — to be rebuilt per the redesign, module by module.
- Infra: Azure (prod, decided 2026-10-22); Redis = Azure Managed Redis in prod, local Docker/WSL in dev. `publish/` folder in the backend is stale build output (see F0). No CI found yet.
- Single repo `BelalMuhamed/ElsadatSearmFullyProject`, working branch `fix/backend`, PRs to `main`.

## Layout
- Backend solution: `New folder (5)/AlSadat-Seram/AlSadat-Seram-Project/AlSadat-Seram/AlSadat-Seram.slnx`
  - `Domain` → `Application` (`Services.contract/*` interfaces, DTOs) → `Infrastructure`
    (`Data/AppDbContext`, `Data/DbInitializer`, `Services/*`) → `AlSadat-Seram.Api` (controllers).
  - `Tests/Application.UnitTests`
- Frontend (old, dropped): `New folder (6)/AlsadatUi`
- Docs: see the paths table in `CLAUDE.md` (plan, step briefs, UI contract, screenshots, requirements).

## Architecture & conventions (backend, observed)
- Onion/Clean: dependencies point inward; DTOs only, never entities.
- Services return `Domain.Common.Result<T>`; no business exceptions.
- Controllers inherit `BaseApiController` and return `HandleResult(result)`, which resolves
  `MessageKey` via `ILocalizationService` and returns `StatusCode((int)result.StatusCode, result)`.
  (30 of 33 controllers don't follow this yet — each one is rewritten in its module, D26.)
- Messages are localization keys (`Result<T>.SuccessKey/FailureKey`) defined in
  `Infrastructure/Services/LocalizationServices/LocalizationResources.cs` as `(ar, en)` tuples.
- Response shape (the `Result<T>` serialized as-is):
  ```json
  { "isSuccess": true, "message": "...", "messageKey": "...", "statusCode": 200, "data": {}, "errors": null }
  { "isSuccess": false, "message": "...", "messageKey": "Common.ValidationFailed", "statusCode": 400, "data": null,
    "errors": { "field": ["..."] } }
  ```
  Paged-list shape: 4 different shapes today → `PagedResult<T>` (F2-S1), adopted module by module (D26). Framework errors: same envelope since F2-S1.
- Services are reached through `IServiceManager` (lazy-constructed in
  `Infrastructure/Services/ServiceManager.cs`); some auth services are also DI-registered in
  `Program.cs` and injected directly into controllers. **Decision D2: keep this pattern.**
- Repository + UnitOfWork for domain data; auth services use `AppDbContext` + `UserManager` directly.
- Permissions today: code-defined constants per area (e.g. `Domain/Common/EmployeePermissions.cs`),
  aggregated in `Domain/Common/PermissionCatalog.cs`. `Program.cs` registers one policy per code;
  `DbInitializer.SeedPermissionCatalogAsync` upserts by code at startup (never deletes).
  Use `[Authorize(Policy = XPermissions.Y)]`, never a literal string. Every permission is currently a JWT claim → replaced by D3 in M1.
- How to run: `dotnet build AlSadat-Seram.slnx` (built clean, 0 errors on 2026-10-21).
  Connection string name: `ConnectionStrings:con`. Secrets in user secrets.

## Architecture & conventions (frontend, target)
- Standalone, feature-based folders, smart containers + dumb UI components, signals, no nested
  subscribes, no `any`, services only talk to the API.
- Design rules (RTL, gold usage, colours, fonts, icons): `Docs/docs/ui/README.md`.

## Domain & business rules
- Confirmed: plumber gift balance is in coupons, not money; sales reps have no fixed hours (no
  lateness/hour deductions); dashboard has profit/expense cards + top reps by collection.
- Unconfirmed — ask before hard-coding: 60 points = 1 coupon; cash coupon 1,200 EGP; mixed
  coupon 900 EGP + stars; gifts priced in coupons; VAT 14% (D10); commission tiers; 2026 Eid dates.

## Decisions
| Date | Decision | Why | Alternatives considered |
|---|---|---|---|
| (before 2026-10-21) | Auth redesign: refresh-token sessions, security-stamp snapshot check, code-defined permission catalog upserted at startup | per commits `ef38c16`…`d650cbb` | — |
| 2026-10-22 | **D0** No live data → fresh baseline migration | avoids data-migration scripts | incremental migrations + transfer scripts |
| 2026-10-22 | **D1** Upgrade to .NET 10 LTS (F1) | .NET 9 support ends 10 Nov 2026 | stay on 9 |
| 2026-10-22 | **D2** Keep `ServiceManager` + `GenericRepository` | consistency with existing code | feature-folder services; MediatR (commercial) |
| 2026-10-22 | **D3** Permissions cached per user in Redis via `HybridCache`; token = userId + SecurityStamp; permission change evicts cache (no logout); stamp change (password/deactivate/lock/logout-all) kills sessions; TTL 5 min; Redis down → DB | instant effect, small token | all claims in token; bitmask claim |
| 2026-10-22 | **D4** One `ApprovalRequests` table + handler per type | reject-with-reason, bulk approve, notify written once | per-module statuses + SQL view |
| 2026-10-22 | **D9** Dotted account codes (`1.1.3`) with numeric sort | matches stories | fixed-width numeric codes |
| 2026-10-22 | Hosting on Azure; dev local | — | — |
| 2026-11-17 | **D16** Roles removed entirely in M1; until then only super_admin creates/edits an Admin (C1) | system not in production | role allow-list; fixed role per user type |
| 2026-11-17 | **D17** Customer initial password = phone + first letter of name; phone required + unique + login id; forced change at first login (claim `must_change_password` until M1 column) | no shared password | no login until activation; random one-time password |
| 2026-11-17 | **D18** Finish the uncommitted revoke-sessions work in F0-S1 | needed for M1/D3 | park for M1; drop |
| 2026-11-17 | **D19** Remove AutoMapper; hand-written mapping in `Application/Mappings` | 12.0.1 has a high-severity DoS advisory; only 1 map really used | commercial v15/16; keep 12.0.1 |
| 2026-11-17 | **D20** Migrations applied at API startup in every environment (`MigrateAsync()` before the seed) | one way everywhere; EF Core 9+ locks the DB during Migrate so parallel instances are safe | manual `database update`; auto in Development only + pipeline in prod |
| 2026-11-25 | **D21** Database never cascades deletes: every `Cascade` FK becomes `ClientCascade` in `AppDbContext.DisableDatabaseCascades`, except `UserPermissions` ×2 and `Permissions`→`Modules` | same schema as the old DB (its migration was hand-edited to `NoAction`); no financial/stock rows deleted by accident; no service changes | fix `PointTransactions` only (38 cascades); `Restrict` everywhere (breaks journal-entry editing) |
| 2026-11-25 | **D22** Failures carry `code` (= message key, or a general code per status) and `traceId` | no second catalog; the UI shows `ERR-<status>` + time | numbered codes; status only |
| 2026-11-25 | **D23** One `PagedResult<T>` (`items, page, pageSize, totalCount, totalPages`), query `page`/`pageSize`, max page size 100 | one shape for every table; exports go through F10 | keep `PagedList`; 500 / no limit |
| 2026-11-25 | **D24** `ex.Message` in services is removed when each module rewrites its service | those services are rewritten anyway | rethrow now; generic message now |
| 2026-11-25 | **D25** `PendingModelChangesWarning` is no longer ignored | a forgotten migration stops startup instead of failing later | keep ignoring it |
| 2026-11-25 | **D26** F2-S2 cancelled: each module reviews its controllers and rewrites only what its screens need; the rest are deleted | a bulk move would touch code that is rewritten or deleted later | move all 30 controllers in F2-S2 |
| 2026-11-26 | **D27** Legacy `CreateBy`/`UpdateBy`/`DeleteBy` (+ dates) are filled by name from the token in `AuditFieldsInterceptor`, the client can't set them; new entities use `IAuditedEntity`; names are unified per module | closes C7 now without touching 18 services | rename all 23 entities now; new entities only |
| 2026-11-26 | **D28** `AuditEntries`: one row per change (user id + name at the time, UTC time, area, action, entity, reference, JSON before/after) + optional service events via `IAuditTrail` | matches screen 78 (field before → after and sentences) | one row per field; manual events only |
| 2026-11-26 | **D29** Auditing is opt-in with `[Audited]`; F3-S1 marks `Products` and `Supplier`, each module marks its own | no tokens/salaries in the log | everything automatic; none yet |
| 2026-11-26 | **D30** `AuditEntries` is append-only: interceptor guard + SQL trigger rejecting `UPDATE`/`DELETE` | screen 78 promises it can't be edited | code guard only |
| 2026-11-26 | **D31** Document numbers are gapless: one `DocumentSequences` row per type, incremented with `UPDATE … OUTPUT` inside the document's own transaction (rollback gives the number back) | auditors see no missing numbers; low concurrency | SQL `SEQUENCE` (no waiting, gaps on failure) |
| 2026-11-26 | **D32** One running counter per type, no yearly reset (`INV-1038`) | matches the UI | yearly `INV-2026-0001` |
| 2026-11-26 | **D33** `rowversion` now on `Stock`, `SalesInvoices`, `PurchaseInvoice`; wallets get it with their new tables (M5/M11) | protects stock today without touching services | also the old balance columns; none now |
| 2026-11-26 | **D34** Numbering starts at 1 with at least 4 digits (`INV-0001`), grows past 9999 | no paper numbers to continue | start numbers from the old books |
| 2026-11-27 | **D35** Old services that write journal entries (sales, purchases, payroll, loans) stay as they are; each module moves them to `IJournalPostingService`; `EntryNumber`/`SourceType` stay nullable until the last one moves (M3 makes them required) | those services are rewritten in M6/M7/M9 anyway | move all of them now |
| 2026-11-27 | **D36** Posted journal entries and their lines are never modified or deleted: guard in `SaveChanges` (`PostedJournalGuardInterceptor`); old purchase-invoice delete with a posted entry now fails (500) until M6 | protects the ledger from every code path | engine-only checks; + DB trigger |
| 2026-11-27 | **D37** A reversal entry is dated on the day of the reversal (caller passes it) | closed months don't change | same date as the original |
| 2026-11-27 | **D38** Engine tests: pure unit tests for the rules + a throwaway-DB harness for the SQL parts; an integration test project comes with CI | no new packages now | integration project now |
| 2026-11-27 | **D39** The approvals card (title, details, why approve is blocked) is built by the type's `IApprovalHandler` when the list is read, one batch per type | balances on the card are always current | snapshot text at submit time |
| 2026-11-27 | **D40** Nobody decides their own approval request (403 `Approval.OwnRequest`), except `super_admin` | separation of duties; the owner can still act | allow everyone |
| 2026-11-27 | **D41** Bulk approve: each request in its own transaction, the reply lists each result | one blocked request doesn't stop the rest | all or nothing |
| 2026-11-27 | **D42** F7-S1 builds the engine only (tested with a throwaway handler); every real request type comes with its module (M1, M5, M6, M7, M9, M11) | no work done twice | move purchase `DeleteStatus` now |

## Pending decisions (decide when their module comes)
D5 SignalR · D6 BackgroundService jobs · D7 ClosedXML only · D8 `IFileStorage` (local dev / Azure Blob prod — recommended) · D10 tax · D11 wallet+ledger · D12 HR questions · D13 loyalty cash expense account · D14 warranty duration · D15 purchases before sales.

## Refactor status
Detailed per-session status: `Docs/docs/refactor/TRACKER.xlsx` (110 sessions). Change explanations: `Docs/docs/refactor/README.md`.
Order: F0 → F1 → F2+F3 → F4+F5+F6 → F7+F8 → (F9 in M1; F10–F15 wired per module; F16 throughout) → M1 → M2+M3 → M4+M5 → M6 → M7 → M8 → M9+M10 → M11 → M12 → M13+M14.
- [x] Backend review + refactor plan + blocking decisions (claude.ai, 2026-10-21/22)
- [x] **F0 — security & critical fixes** (2026-11-17)
- [x] **F1-S1 — .NET 10 + EF Core 10 upgrade** (2026-11-17)
- [x] **F1-S2 — fresh baseline migration (D0)** (2026-11-25)
- [x] **F2-S1 — unified response (structure)** (2026-11-25)
- F2-S2 cancelled (D26): controllers move with their module
- [x] **F3-S1 — audit + change log** (2026-11-26)
- [x] **F4F5-S1 — document numbering + rowversion** (2026-11-26)
- [x] **F6-S1 — journal posting engine** (2026-11-27)
- [ ] **F7-S1 — approvals engine** ← next

## Modules / features status
| Module | Backend | Frontend | Notes |
|---|---|---|---|
| Auth & permissions | phases 0–6 committed; admin revoke-sessions was in progress (uncommitted on 2026-10-21) | old UI | permissions model changes in M1 (D3) |
| Everything else | exists, to be refactored per plan | old UI, redesign pending | |

## Current work
- 2026-11-17: the revoke-sessions work below is now committed (`2e6a489`) — the note is kept for history.
- As of 2026-10-21 (uncommitted on `fix/backend`, **verify with `git status`**): admin "revoke all sessions" —
  `AuthController.RevokeSessions` (`POST api/auth/revoke-sessions/{targetUserId}`, policy
  `HR.Employees.RevokeSessions`), `IUserSessionService.AdminRevokeSessionsAsync`,
  `UserSessionService` (ExecuteUpdate revoke + audit log + security-stamp bump in one transaction),
  new permission in `EmployeePermissions`, localization key `Permission.SessionsRevoked`,
  `ServiceManager` change. Fits D3 ("logout all devices" bumps the stamp). Finish or park it before F0 (see F0 step 0).

## Known issues & tech debt
- Uncommitted revoke-sessions diff (2026-10-21 review):
  - `Domain/Common/EmployeePermissions.cs`: `namespace Domain.Common;` was replaced by `using Domain.Common;` → class in global namespace; XML doc removed.
  - `ServiceManager.cs`: `_UserSessionService` news up its own `UserSessionService` while the injected `userSessionService` ctor parameter is unused.
  - `UserSessionService.AdminRevokeSessionsAsync`: `IdentityResult` of `UpdateSecurityStampAsync` ignored.
  - `AuthController.RevokeSessions`: no `CancellationToken`; `RemoteIpAddress` is the proxy IP unless forwarded headers are configured.
- `ExecuteUpdate`/`ExecuteDelete` bypass the audit interceptors (F3-S1): never use them on `[Audited]` entities or on entities with `CreateBy`/`UpdateBy` columns.
- F6-S1 (D35/D36): `PostedJournalGuardInterceptor` blocks modifying/deleting posted entries and their lines, but not **adding** a line to a posted entry (old writers save the header, then the lines) — close it in M3. Old `DeletePurchaseInvoice` on an invoice with a posted entry now fails with 500 instead of deleting the ledger (C10) until M6. Old writers still use code strings / `Random` refs and leave `EntryNumber`/`SourceType` null until M6/M7/M9 move them to `IJournalPostingService`; `ExecuteUpdate` on journal tables bypasses the guard. Screen testing of posting/reversal deferred to M3 (manual-entry screen), Belal's call 2026-11-27.
- F7-S1 (D39–D42): approvals engine has no real request type yet, so the center is empty until M1; per-type permission is read from JWT claims (`ApprovalService.AllowedTypes`) until M1 moves it to the cache (D3); the rejection reason is stored but no notification is sent until F8. The approve race check passes, but which guard catches it (status check or `RowVersion`) depends on timing.
- `ProductService.EditProduct`/`FromDto` still copy `deleteBy`/`deleteAt`/`createBy` from the DTO; the interceptor overwrites them (D27) — remove the copies in M4.
- Sales/purchase numbers are still `SO{Id}`/`PO{Id}` and journal references `Random` (`EmployeePayrollService`, `EmployeeLoanService`): switch to `IDocumentNumberService` when M6/M7/F6 rewrite them; add a unique index on `InvoiceNumber` then. Decide there whether drafts get their number at save (UI) or at confirm (PLAN).
- `ProfileService` returns hardcoded Arabic messages — `ProfileController` doesn't use `HandleResult`; switch both together in M1 (D26).
- `PayrollDeductionService.cs` lines 84/96: CS8619 nullability warnings.
- Build has ~108 warnings (mostly nullability) — not blocking; to be cleaned per module.
- Swashbuckle stays 9.0.6 (Microsoft.OpenApi 1.x); moving to 10.x needs the Swagger setup in `Program.cs` rewritten for Microsoft.OpenApi 2.
- Tests use xunit v2 (2.9.3), which is deprecated (security fixes only) → move to `xunit.v3` later.
- `Application/Helper/BaseInvoicePdfDocument.cs` uses `System.Drawing` (Windows-only, CA1416 ×72 warnings) → will break on Azure Linux; replace before deployment.
- Model warnings seen while generating the baseline (left as-is per 3-أ, fix in each module): 25 decimal properties without a precision → default `decimal(18,2)` (`Representatives.OvertimeRatePerHour` vs `decimal(18,4)` in `Employee`); shadow FKs `Previews.ApplicationUserId1/2`; `Payroll`↔`EmployeeLoanPayments` and `Previews`↔`WarrantCertificates` split into two relationships each.
- `ex.Message` reaches the client from 73 places in 12 services (+7 per-row messages): remove it in each module when its service is rewritten (D24).
- Rate limiter (`Program.cs`) counts per `RemoteIpAddress`, 100/min: behind an Azure proxy all users may share one IP → needs forwarded headers / per-user partition before deployment.
- `UnitOfWork.LogError` writes `Logs/errors.log` into a zip with a hardcoded password → replace with `ILogger`.
- DataAnnotations messages in DTOs are still English literals (e.g. `LoginDto`): move them to localization keys in each module (`ApiErrorWriter` already resolves keys).
- `SupplierService` has its own page-size limit (50): align with D23 in M5 (D26).
- D20 (auto-migrate everywhere): in prod the API's DB user needs DDL rights — revisit when the Azure pipeline is set up (alternative: migration bundle in the pipeline).
- Full list of critical issues C1–C16 and where each is fixed: `Docs/docs/refactor/PLAN.md` ("مشاكل حرجة").
- `Infrastructure/Services/GoogleAuthServices/GoogleAuthService.cs` still exists although Google login was removed — verify and delete (F0).

## Session log
- 2026-10-21 — Created this file; read repo layout, `Docs/` handoff, and the uncommitted revoke-sessions diff; solution builds with 0 errors.
- 2026-10-21/22 (claude.ai) — Full backend review vs new UI; refactor plan; decisions D0–D4, D9, hosting.
- 2026-10-24 (claude.ai) — Claude Code kit placed: root `CLAUDE.md`, `.claude/commands/{start,handoff}.md`, `Docs/docs/{refactor,ui,requirements}`; merged the two context files into this one.
- 2026-11-16 (claude.ai) — Work mode confirmed: one account at a time (no parallel work). Added to CLAUDE.md: checkpoint after every small part, explain business + changes before any part, impact analysis for every decision. Rough estimate: ~100–140 sessions total.
- 2026-11-17 (claude.ai) — F0 done: revoke-sessions, C1, C2, C3, C15, C16, Google removed. Build 0 errors, 15/15 tests on Belal's PC.
- 2026-11-17 (claude.ai) — F1-S1 done: AutoMapper removed (D19), net10.0, Microsoft packages 10.0.15, unused packages removed, `System.Security.Cryptography.Xml` pinned (NU1903 via EPPlus). Build 0 errors, 15/15 on Belal's PC.
- 2026-11-16 (claude.ai) — Docs, CLAUDE.md, PROJECT_CONTEXT.md, .claude/commands pushed to `fix/backend`; added TRACKER.xlsx (110 sessions) + refactor README (change log). New workflow: Claude implements + pushes, Belal pulls + builds/tests (cloud can't reach nuget.org).
- 2026-11-25 (claude.ai) — Moved the main way of working to Claude Code on Belal's PC: `tools/tracker.py` (tracker updates keeping formulas/validation), `.claude/settings.json` (allowed build/test/git commands, no force-push), `/start` + `/handoff` rewritten, CLAUDE.md "Who does what" has both modes. F1-S2 unchanged: still waiting for `make-baseline.ps1`.
- 2026-11-25 (Claude Code) — F1-S2 done: `InitialCreate` baseline, D20 auto-migrate, D21 no database cascades (`AppDbContext.cs`, `ApplicationBuilderExtensions.cs`, `Program.cs`, `Infrastructure/Migrations`); old Angular app removed; replies on `claude-reply/index.html`. First startup test hit the old DB (rolled back, nothing changed). Build 0 errors, 15/15, confirmed by Belal. Next: F2-S1.
- 2026-11-25 (Claude Code) — F2-S1 done and confirmed: `code`/`traceId`, `ApiErrorWriter`, `PagedResult`, D22–D25; F2-S2 cancelled (D26) and its row removed with `tracker.py remove`. Next: F3-S1.
- 2026-11-26 (Claude Code) — F3-S1 done and confirmed: D27–D30; `AuditFieldsInterceptor` (who/when from the token, UTC), `AuditEntries` + migration `AddAuditEntries` + append-only trigger, `AuditEntriesInterceptor` + `[Audited]` on `Products`/`Supplier` + `IAuditTrail`, `ChangeLogService`/`IAuditableEntity` deleted (`Infrastructure/Data/Interceptors/*`, `Domain/Entities/Audit`, `Domain/Common`, `Program.cs`, `ServiceManager`). 37 checks on a throwaway DB; build 0 errors, 42/42. Left for modules: screen-78 endpoint (M2), marking other entities, field-name translations. Next: F4F5-S1.
- 2026-11-26 (Claude Code) — F4F5-S1 done and confirmed: D31–D34; `DocumentSequences` + `IDocumentNumberService` (`Domain/Enums/DocumentType.cs`, `Domain/Entities/Numbering`, `Infrastructure/Services/NumberingServices`, migration `AddDocumentSequences`), `RowVersion` on `Stock`/`SalesInvoices`/`PurchaseInvoice` (migration `AddRowVersions`) + 409 `Common.ConcurrencyConflict` (`ErrorCodes`, `LocalizationResources`, `ExceptionHandlingMiddleware`). 12 checks on a throwaway DB; build 0 errors, 42/42. Left: `SO{Id}`/`PO{Id}`/`Random` replaced in M6/M7/F6. Next: F6-S1.
- 2026-11-27 (Claude Code) — F6-S1 done and confirmed: D35–D38; `JournalSourceType`, journal entry number/source/reversal link/line note + `IAuditedEntity`/`[Audited(Accounting)]` (migration `AddJournalPosting`), `JournalDraft` + `JournalDraftRules` (11 tests), `IJournalPostingService`/`JournalPostingService` (`PostAsync`/`ReverseAsync`, `JournalErrorCodes`, `Journal.*` keys), `PostedJournalGuardInterceptor` (`Program.cs`). 32 checks on a throwaway DB; build 0 errors, 53/53. `tools/tracker.py --commit` now splits commas and dedupes. Left: engine has no caller yet (M3 manual entry, M6/M7/M9 move old writers), guard misses added lines (M3), screen testing in M3. Next: F7-S1.
