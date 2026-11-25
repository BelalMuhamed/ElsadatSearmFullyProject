# Sadat Searm ERP — repo root (branch `fix/backend`)

ERP for a sanitary-ware manufacturer: sales/purchases, multi-warehouse inventory, double-entry accounting, HR/payroll, sales reps, and a plumber loyalty flow (inspection → warranty → points → coupons → payout).

## Paths (Claude Code is opened from the repo root)
| Name | Path |
|---|---|
| **Backend solution** (`BACKEND`) | `New folder (5)/AlSadat-Seram/AlSadat-Seram-Project/AlSadat-Seram/` (contains `AlSadat-Seram.slnx`) |
| Handoff / status | `PROJECT_CONTEXT.md` (repo root) |
| Refactor plan | `Docs/docs/refactor/PLAN.md` |
| Current step brief | the step's section in `Docs/docs/refactor/README.md` + `Docs/docs/refactor/PLAN.md` (`F0.md` is the only standalone brief) |
| **Tracker** (sessions + status) | `Docs/docs/refactor/TRACKER.xlsx` — one sheet per plan part, one row per session (goal, outputs, status, dates, commit) + a decisions sheet |
| **Change log** (explanation of every change) | `Docs/docs/refactor/README.md` — one section per session code |
| UI contract (text) | `Docs/docs/ui/README.md`, `Docs/docs/ui/SCREENS.md`, `Docs/docs/ui/screens/<Name>.dc.html` |
| UI contract (visual) | `Docs/design/screenshots/<Name>.png` (you can view these), `Docs/design/preview/index.html` (open in a browser) |
| Design tokens | `Docs/docs/ui/sr-tokens.css` (same as `Docs/design/sr.css`) |
| Requirements | `Docs/docs/requirements/` and `Docs/docs/inputs/` (customer stories PDF, `BACKEND_MODULE_ANALYSIS.md` — older analysis, the plan supersedes it) |
| Old Angular app | `New folder (6)/AlsadatUi` — **dropped**, never use it to infer behaviour |

Inside the docs, a path written as `docs/...` means `Docs/docs/...`, and code paths like `Infrastructure/...` are relative to `BACKEND`.

## Talk to Belal like this
- **Replies go to an HTML page** (the console breaks Arabic; agreed 2026-11-25): write every full reply to `claude-reply/index.html` (git-ignored, RTL, `code` isolated LTR) and change the version in both that page and `claude-reply/version.js`, so the open page reloads itself. In the console leave only a short English summary, the page path, and how to answer (e.g. `ok 1a 2b`).
- Reply in **Egyptian Arabic**; keep technical terms, code, file paths and identifiers in English.
- Explain step by step, in simple words. Give exact file paths for every change. Prefer surgical edits anchored to existing lines over whole-file rewrites.

### Before starting ANY part (an F-step, a module, or a sub-part of one) — explain first, then wait
Before touching code, send Belal a short explanation and wait for his OK:
1. **الـ business:** what this part does for the real users (who uses it, which screens in `Docs/docs/ui`, which stories), in plain words with a concrete example (e.g. "المحاسب بيأكد فاتورة INV-1040 → المخزون بيقل والقيد بيتسجل").
2. **الوضع الحالي:** what the code does today and what's wrong or missing (with file paths).
3. **التعديلات:** what will change — entities/tables, endpoints, services, behaviour — and what gets removed.
4. **القرارات المطلوبة** (if any), each in the decision format below.

### Every decision — always show its impact
When a decision is needed, never decide silently. For each option give:
- **الاختيار:** what it is, in one line.
- **تأثيره على:** the database/data (tables, migrations, existing rows), the code (which layers/modules change), the UI/screens and the users' workflow, the other modules that depend on it, performance/security, and **how hard it is to change later** (easy / costly / practically irreversible).
- **ميزته وعيبه** in one line each.
Then give **your recommendation + what would change your mind**, and wait for his choice. After he decides, record it (see checkpoints).

## Read before any work (in this order)
1. `PROJECT_CONTEXT.md` — status, decisions, in-progress work, known issues. **Update it at the end of every session** (`/handoff`).
2. `Docs/docs/refactor/PLAN.md` — critical issues C1–C16, foundation F0–F16, modules M1–M14, decisions D0–D15. The "القرارات" section is authoritative.
3. The current step: its section in `Docs/docs/refactor/README.md` and in `PLAN.md` (`python tools/tracker.py next` shows the session).
4. For anything screen-related: `Docs/docs/ui/README.md` → grep `Docs/docs/ui/SCREENS.md` → open the `.dc.html` / screenshot.

## Source of truth
- **The new UI is the contract.** The backend is refactored to serve it, module by module (backend complete → that module's frontend → next module).
- If code, plan and UI disagree: the UI wins for *what*, the plan wins for *how*; flag the conflict before coding.

## Backend layout (`BACKEND`)
```
AlSadat-Seram.slnx
├─ Domain/                 entities, enums, Common/ (Result<T>, PermissionCatalog, *Permissions) — no dependencies
├─ Application/            DTOs, Services.contract/ (interfaces)
├─ Infrastructure/         Data/ (AppDbContext, DbInitializer), Migrations/, Repositories/, UnitOfWork/, Services/ (ServiceManager, LocalizationServices/…), Authorization/
├─ AlSadat-Seram.Api/      Controllers (BaseApiController), Middlewares/, Program.cs
└─ Tests/Application.UnitTests/
```
Onion rule: dependencies point inward.

## Commands (run inside `BACKEND`)
```bash
cd "New folder (5)/AlSadat-Seram/AlSadat-Seram-Project/AlSadat-Seram"
dotnet build AlSadat-Seram.slnx
dotnet test AlSadat-Seram.slnx
dotnet tool restore                      # dotnet-ef pinned in AlSadat-Seram.Api/dotnet-tools.json
dotnet ef migrations add <Name> -p Infrastructure -s AlSadat-Seram.Api
```
Secrets (JWT key, connection string `ConnectionStrings:con`, seed password) come from **user secrets** — never from committed `appsettings*.json`.

## Decisions already taken (do not reopen without Belal)
- **D0** No live data → one fresh baseline migration; old migrations and unused tables get dropped.
- **D1** Target **.NET 10 LTS** — done in F1-S1 (all projects `net10.0`, Microsoft packages 10.0.x).
- **D2** Keep **`ServiceManager` + `GenericRepository` + `UnitOfWork`**. No MediatR. **No AutoMapper** (removed in F1-S1, D19): map by hand with extension methods in `Application/Mappings`, prefer `.Select()` projections for reads.
- **D3** Permissions: token carries `userId` + `SecurityStamp` only. `PermissionAuthorizationHandler` reads {stamp, isActive, effective permissions} per request from `HybridCache` (memory L1 + Redis L2). Permission change → evict that user's entry (no logout). Stamp changes only on password change/reset, deactivate, lock, "logout all devices" → kills sessions. TTL 5 min safety net. Redis down → read DB, never allow-all. Access token 10–15 min. Frontend reads `GET /me/permissions`.
- **D4** One `ApprovalRequests` table + one handler per request type.
- **D9** Account codes stay dotted (`1.1.3`) with numeric sort. Use `SystemAccountCode` lookups, never magic ids (`8`, `10`).
- **D16** Roles get removed entirely in M1 (system not in production yet). Until then only a super_admin may create or edit an Admin (`RoleAssignmentPolicy`).
- **D17** Customer initial password = phone number + first letter of the full name (as written); phone is required, unique and the login id; the user must change it at first login (`must_change_password` claim + `PasswordChangeRequiredFilter`, column comes in M1).
- **D20** Migrations are applied automatically at API startup in **every** environment (`MigrateAsync()` before the seed). EF Core 9+ takes an exclusive DB lock during `Migrate`, so several instances starting together are safe; the price is that the API's DB user needs DDL rights in prod (revisit when the Azure pipeline is set up).
- **Hosting** Azure (Azure Managed Redis in prod). Dev: API + Redis on the same machine (Redis in Docker/WSL).

## Mandatory coding rules (every touched method complies — no grandfathering)
1. **Result pattern:** every endpoint returns `Result<T>` via `BaseApiController.HandleResult`. No business exceptions; unexpected errors go through `ExceptionHandlingMiddleware`.
2. **Localization:** every user-facing message is a key in `LocalizationResources` with **both `ar` and `en`**. No hardcoded strings.
3. **Identity from the token, never the client:** no `createdBy`/`updateBy`/`userId`/`employeeEmail`/role names from DTOs.
4. **Authorization:** a touched controller moves from `[Authorize(Roles=...)]` to `[Authorize(Policy = XPermissions.Y)]` in the same change, or you flag why not. `super_admin` claim is the bypass. `Permission.Code` is immutable. Don't invent permission codes for a module before it's reviewed with Belal.
5. **DTOs only** in/out of controllers; reads use `.Select()` projections, no N+1.
6. **Money and stock are atomic with their journal entry** (one transaction). Confirmed documents are reversed, never edited or hard-deleted.
7. Validation with DataAnnotations (FluentValidation is referenced but unused — ask before using it).
8. Async all the way, constructor injection only, XML `<summary>` on public interfaces/methods, no magic numbers, no dead code.
9. EF Core: never set an FK from an unsaved entity's navigation — insert first, then link.
10. Excel: ClosedXML; pre-format text columns before writing; sanitize at export, not at save.

## Way of working
- One step per session. For each step: `git status` → read the code it touches → **explain business + changes + decisions (above) and wait for OK** → implement in small parts → build + test → `/handoff`.
- Quality over speed. Read the actual code before every recommendation; don't trust notes over code.
- **Who does what — two modes, same rules.** Claude **commits and pushes to `fix/backend` after every finished part** (Belal approved this as the standard way of working). Never claim a build passed unless a real build output says so.
  - **Claude Code on Belal's PC (main mode since 2026-11-25):** Claude works directly in `C:\Users\b.zakaria\Desktop\SR`. Start with `git status` + `git pull origin fix/backend` (stop and ask if there are local changes you didn't make). Claude runs `dotnet build` / `dotnet test` / `dotnet ef` itself, from `BACKEND`, and reads the output — no log files needed. Scripts meant for Belal (e.g. `make-baseline.ps1`) can be run by Claude with `powershell -ExecutionPolicy Bypass -File <script>` after Belal's OK. Commit and push with Belal's git (never force-push).
  - **claude.ai / cloud session:** Claude works on its own clone. If NuGet is blocked there, Belal runs `sync-and-build.ps1` and Claude reads `pull.log` / `build.log` / `test.log` (UTF-16) from the PC.
- **Tracker updates:** always through `tools/tracker.py` (it keeps the formatting, validation and summary formulas): `python tools/tracker.py next | show <code> | session <code> --status … --start today --end today --commit <hash> --notes "…" | decision <Dn> --status اتقرر --choice "…" --date today --session <code> --impact "…"`. On Windows use `py -3` if `python` is missing; it needs `pip install openpyxl` once. If Python isn't installed, ask Belal to install it (`winget install Python.Python.3.12`) — don't edit the xlsx another way.
- Commit messages: `<session code>: <what changed>` (e.g. `F0-S1: stop accepting role names from employee DTOs`). Never force-push. Don't commit files outside the session's scope.
- Never overwrite or commit Belal's uncommitted local work; if a part needs a file he has changed locally, stop and ask first.

## Checkpoints — save progress little by little (the usage limit can end a session at any moment)
Work in **small parts** (one issue, one entity, one endpoint, one screen — roughly ≤ 30 minutes of work each). **After every part**, before starting the next one, update the `## Checkpoint` section at the top of `PROJECT_CONTEXT.md` — without being asked:
- **Step:** the current step (e.g. `F0`) and the part just finished.
- **Done in this step:** a ticked list of finished parts, with the files touched.
- **In progress / next part:** exactly what comes next, specific enough that a fresh session can continue without asking (file, method, what's left).
- **Uncommitted changes:** files changed and not committed yet, and whether the build passes.
- **Decisions taken in this step:** decision + chosen option + date (also add lasting ones to the Decisions table and to this file).
- **Open questions** waiting for Belal.
**With every checkpoint also update:**
- `Docs/docs/refactor/TRACKER.xlsx` — via `tools/tracker.py`: the session's row: الحالة (لم يبدأ / جاري / متوقف / خلص), dates, Commit, notes; the decisions sheet if a decision was taken. Never retype the summary formulas.
- `Docs/docs/refactor/README.md` — the session's section: before implementing write it as «مخطط» (business, current state, planned changes, decisions); after implementing rewrite it with what actually changed (files, behaviour, removed things), the commits, and **how Belal can verify it** (endpoint + request, or screen + steps). Add a section for any session that doesn't have one.
- A session is «خلص» only when its goal in the tracker is met and Belal confirmed the build/tests pass on his side.
Also checkpoint right after Belal answers a decision, and before any long operation (migration, package upgrade, big refactor).
Keep the checkpoint short — overwrite it, don't append history. `/handoff` at the end of the session moves a one-line summary into the Session log and clears the checkpoint for the next step.
A new session always starts by reading the Checkpoint and continuing from "In progress / next part" — never redo finished parts.
