# Sadat Searm ERP — Project Context
_Last updated: 2026-11-16 · Read this first in any new session · Update it with `/handoff` at the end of every session._

## Checkpoint (overwrite after every small part — see "Checkpoints" in CLAUDE.md)
- **Step:** F0. F0-S1 paused (waiting for decisions); F0-S2 in progress.
- **Done in this step:** [x] C3 — `DistributorsAndMerchantsService.EditDistributorOrMerchant` ignores balances (`71cf2bc`). [x] Google sign-in code + `Google.Apis.Auth` removed (`70b1f24`). Both pushed, **not built yet** (cloud can't reach nuget.org) — waiting for Belal's build.
- **In progress / next part:** read `build.log` from Belal's PC when he builds; then F0-S2 C15 (after his answer) and C16 (he sets the user secret); then F0-S1 after his decisions.
- **Uncommitted changes:** none in the cloud clone. On Belal's PC: revoke-sessions work in 6 files (the 5 known + `Infrastructure/Services/LocalizationServices/LocalizationResources.cs`). Don't touch those files until he decides.
- **Decisions taken in this step:** —
- **Open questions:** (1) revoke-sessions work: finish / park / drop? (2) C2: customers created without login (rec.) or one-time random password? (3) C1: allow-list of non-admin roles (rec.) or fixed role per user type? (4) C15: does any server run from `publish/`?

## Overview
ERP for Sadat Searm (sanitary-ware / plumbing products manufacturer & distributor): sales/purchase invoices, stock & warehouses,
treasury & journal entries, HR (employees, attendance, leaves, loans, payroll, deductions),
sales reps, merchants/distributors, plumbers loyalty (inspection → points → coupons → gifts/cash).
Phase: **backend refactor to serve the new UI** (the new UI in `Docs/` is the source of truth; the old Angular app is dropped).
Plan: `Docs/docs/refactor/PLAN.md` (online original: https://claude.ai/artifact/BbH94DNQysKSDn5vQnkR6w).

## Stack
- Backend: .NET 9 (`net9.0`, nullable on), ASP.NET Core, EF Core 9.0.10 code-first, SQL Server,
  ASP.NET Identity + JWT bearer + refresh tokens, AutoMapper 12, FluentValidation 12 (unused), QuestPDF,
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

## Pending decisions (decide when their module comes)
D5 SignalR · D6 BackgroundService jobs · D7 ClosedXML only · D8 `IFileStorage` (local dev / Azure Blob prod — recommended) · D10 tax · D11 wallet+ledger · D12 HR questions · D13 loyalty cash expense account · D14 warranty duration · D15 purchases before sales.

## Refactor status
Detailed per-session status: `Docs/docs/refactor/TRACKER.xlsx` (111 sessions). Change explanations: `Docs/docs/refactor/README.md`.
Order: F0 → F1 → F2+F3 → F4+F5+F6 → F7+F8 → (F9 in M1; F10–F15 wired per module; F16 throughout) → M1 → M2+M3 → M4+M5 → M6 → M7 → M8 → M9+M10 → M11 → M12 → M13+M14.
- [x] Backend review + refactor plan + blocking decisions (claude.ai, 2026-10-21/22)
- [ ] **F0 — security & critical fixes** ← next (brief: `Docs/docs/refactor/F0.md`)
- [ ] F1 — .NET 10 + EF Core 10 upgrade, fresh baseline migration

## Modules / features status
| Module | Backend | Frontend | Notes |
|---|---|---|---|
| Auth & permissions | phases 0–6 committed; admin revoke-sessions was in progress (uncommitted on 2026-10-21) | old UI | permissions model changes in M1 (D3) |
| Everything else | exists, to be refactored per plan | old UI, redesign pending | |

## Current work
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
- `PayrollDeductionService.cs` lines 84/96: CS8619 nullability warnings.
- Full list of critical issues C1–C16 and where each is fixed: `Docs/docs/refactor/PLAN.md` ("مشاكل حرجة").
- `Infrastructure/Services/GoogleAuthServices/GoogleAuthService.cs` still exists although Google login was removed — verify and delete (F0).

## Session log
- 2026-10-21 — Created this file; read repo layout, `Docs/` handoff, and the uncommitted revoke-sessions diff; solution builds with 0 errors.
- 2026-10-21/22 (claude.ai) — Full backend review vs new UI; refactor plan; decisions D0–D4, D9, hosting.
- 2026-10-24 (claude.ai) — Claude Code kit placed: root `CLAUDE.md`, `.claude/commands/{start,handoff}.md`, `Docs/docs/{refactor,ui,requirements}`; merged the two context files into this one.
- 2026-11-16 (claude.ai) — Work mode confirmed: one account at a time (no parallel work). Added to CLAUDE.md: checkpoint after every small part, explain business + changes before any part, impact analysis for every decision. Rough estimate: ~100–140 sessions total.
- 2026-11-16 (claude.ai) — Docs, CLAUDE.md, PROJECT_CONTEXT.md, .claude/commands pushed to `fix/backend`; added TRACKER.xlsx (111 sessions) + refactor README (change log). New workflow: Claude implements + pushes, Belal pulls + builds/tests (cloud can't reach nuget.org).
