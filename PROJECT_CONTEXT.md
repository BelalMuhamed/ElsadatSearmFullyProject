# Sadat Searm ERP — Project Context
_Last updated: 2026-11-25 (F1-S2 in progress) · Read this first in any new session · Update it with `/handoff` at the end of every session._

## Checkpoint (overwrite after every small part — see "Checkpoints" in CLAUDE.md)
- **Step:** F1-S2 — fresh baseline migration (D0). Code finished and verified on Belal's PC by Claude; waiting for Belal's confirmation to mark it «خلص».
- **Done in this step:**
  - [x] Part 1: old migrations + leftover scripts deleted.
  - [x] Part 2: D21 in `Infrastructure/Data/AppDbContext.cs` (`DisableDatabaseCascades`); `Infrastructure/Migrations/20261125161145_InitialCreate` + snapshot; `MigrateAndSeedDatabaseAsync` in `Infrastructure/Data/ApplicationBuilderExtensions.cs` (D20) called from `Program.cs`.
  - [x] Part 3 (verification): build 0 errors, 15/15 tests; API started twice against `SadatSearmErp` (first start migrated + seeded, second changed nothing); columns (585), indexes (151) and FKs (78, 3 cascades) identical to `AlSadateSeram`. `make-baseline.ps1`, `_out` and their `.gitignore` lines removed.
- **In progress / next part:** Belal answers the connection question and confirms; then mark F1-S2 «خلص» in the tracker, `/handoff`, next session from `python tools/tracker.py next`.
- **State on Belal's PC:** `ConnectionStrings:con` is not in user secrets any more (removed 2026-11-25 18:05), so the API reads `appsettings.json` → `AlSadateSeram`, and startup stops with "There is already an object named 'Accounts'" (rolled back, nothing changes). It must point to `SadatSearmErp`.
- **Incident 2026-11-25:** Claude's first startup test hit `AlSadateSeram` because of that removed secret; the migration failed on its first statement and was rolled back. Checked: 57 tables, the same 4 history rows, no object created/modified.
- **Uncommitted changes:** none.
- **Decisions taken in this step:** 2026-11-17: `make-baseline.ps1` (1-أ), **D20** (2-ج), model as-is (3-أ), new DB name. 2026-11-25: **D21** delete behaviour (b); Claude runs the script; old Angular deletions committed; replies on `claude-reply/index.html`.
- **Open questions:** point `ConnectionStrings:con` to `SadatSearmErp` through user secrets (Claude) or by Belal in `appsettings.json`?

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
  (30 of 33 controllers don't follow this yet — fixed in F2.)
- Messages are localization keys (`Result<T>.SuccessKey/FailureKey`) defined in
  `Infrastructure/Services/LocalizationServices/LocalizationResources.cs` as `(ar, en)` tuples.
- Response shape (the `Result<T>` serialized as-is):
  ```json
  { "isSuccess": true, "message": "...", "messageKey": "...", "statusCode": 200, "data": {}, "errors": null }
  { "isSuccess": false, "message": "...", "messageKey": "Common.ValidationFailed", "statusCode": 400, "data": null,
    "errors": { "field": ["..."] } }
  ```
  Paged-list shape: 4 different shapes today → one `PagedResult<T>` in F2. Framework 401/403/500 shape: verify in F2.
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

## Pending decisions (decide when their module comes)
D5 SignalR · D6 BackgroundService jobs · D7 ClosedXML only · D8 `IFileStorage` (local dev / Azure Blob prod — recommended) · D10 tax · D11 wallet+ledger · D12 HR questions · D13 loyalty cash expense account · D14 warranty duration · D15 purchases before sales.

## Refactor status
Detailed per-session status: `Docs/docs/refactor/TRACKER.xlsx` (111 sessions). Change explanations: `Docs/docs/refactor/README.md`.
Order: F0 → F1 → F2+F3 → F4+F5+F6 → F7+F8 → (F9 in M1; F10–F15 wired per module; F16 throughout) → M1 → M2+M3 → M4+M5 → M6 → M7 → M8 → M9+M10 → M11 → M12 → M13+M14.
- [x] Backend review + refactor plan + blocking decisions (claude.ai, 2026-10-21/22)
- [x] **F0 — security & critical fixes** (2026-11-17)
- [x] **F1-S1 — .NET 10 + EF Core 10 upgrade** (2026-11-17)
- [ ] **F1-S2 — fresh baseline migration (D0)** ← in progress

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
- `ProfileService` returns hardcoded Arabic messages — `ProfileController` doesn't use `HandleResult`; switch both together in F2.
- `PayrollDeductionService.cs` lines 84/96: CS8619 nullability warnings.
- Build has ~108 warnings (mostly nullability) — not blocking; to be cleaned per module.
- Swashbuckle stays 9.0.6 (Microsoft.OpenApi 1.x); moving to 10.x needs the Swagger setup in `Program.cs` rewritten for Microsoft.OpenApi 2.
- Tests use xunit v2 (2.9.3), which is deprecated (security fixes only) → move to `xunit.v3` later.
- `Application/Helper/BaseInvoicePdfDocument.cs` uses `System.Drawing` (Windows-only, CA1416 ×72 warnings) → will break on Azure Linux; replace before deployment.
- Model warnings seen while generating the baseline (left as-is per 3-أ, fix in each module): 25 decimal properties without a precision → default `decimal(18,2)` (`Representatives.OvertimeRatePerHour` vs `decimal(18,4)` in `Employee`); shadow FKs `Previews.ApplicationUserId1/2`; `Payroll`↔`EmployeeLoanPayments` and `Previews`↔`WarrantCertificates` split into two relationships each.
- `AppDbContext.OnConfiguring` ignores `PendingModelChangesWarning`, so a model change without a migration is silent at startup; with D20 it should probably be an error — decide in F2 or the next model change.
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
- 2026-11-16 (claude.ai) — Docs, CLAUDE.md, PROJECT_CONTEXT.md, .claude/commands pushed to `fix/backend`; added TRACKER.xlsx (111 sessions) + refactor README (change log). New workflow: Claude implements + pushes, Belal pulls + builds/tests (cloud can't reach nuget.org).
- 2026-11-25 (claude.ai) — Moved the main way of working to Claude Code on Belal's PC: `tools/tracker.py` (tracker updates keeping formulas/validation), `.claude/settings.json` (allowed build/test/git commands, no force-push), `/start` + `/handoff` rewritten, CLAUDE.md "Who does what" has both modes. F1-S2 unchanged: still waiting for `make-baseline.ps1`.
