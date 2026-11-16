# Sadat Searm ERP — UI Redesign Handoff

**Status:** Design complete, 78 screens + 2 shared components (80 artboards).
**Canvas (source of truth):** https://claude.ai/artifact/Lk3VW1qXKeBe3x75JW4tZ6 (published version `1792583970-5a8e`, 21 Oct 2026)
**Inputs:** `Sadat-Searm-Customer-Stories.pdf` (84 stories) · visual direction from `Calimly-UI-Prototype-v3.pdf` (inventory system) · old Angular UI in `New folder (6)/AlsadatUi`.
**Companion file:** `claude/design/sr-tokens.css` — the shared stylesheet every artboard uses (tokens + components).

---

## 1. Purpose of this doc

The redesign was done in a claude.ai chat. This doc carries every decision from that chat so the work can continue in Claude Code (or any session) without the conversation. Read it before touching `AlsadatUi`.

---

## 2. Design system

### 2.1 Direction
- Dark "espresso + gold" theme by default, with a light theme (`DashboardLight` shows the mapping).
- RTL Arabic first; English is the secondary language (toggle in the top bar).
- Arabic-Indic digits in the UI (`٠١٢٣٤٥٦٧٨٩`). Document numbers (`INV-1042`, `INS-0412`) stay Latin, wrapped in `<bdi dir="ltr">` with the mono font.
- Tablet and phone use the **same web app**, made responsive. No separate native app was designed. The plumber mobile app (story E17) is out of scope here.

### 2.2 Typography
- UI: **IBM Plex Sans Arabic** 400/500/600/700.
- Codes and numbers: **IBM Plex Mono** 400/500.
- Scale: H1 22/700 · H2 15/700 · body 13–13.5 · small 12 · KPI 20–28/700.

### 2.3 Colour tokens (dark)
| Role | Value |
|---|---|
| Page background | `#0F0C08` → gradient `#17130D → #0E0B08` |
| Sidebar | `#100D09` |
| Card | gradient `rgba(42,35,25,.9) → rgba(22,19,14,.85)`, border `rgba(255,232,190,.08)` |
| Highlighted card | border `rgba(240,200,80,.22)` |
| Text: primary / soft / dim / muted / faint | `#F6F1E7` / `#CFC5B2` / `#BDB29E` / `#978C78` / `#6E6556` |
| **Gold (primary)** | gradient `#F3D35A → #E0B43A → #C8961C`; link `#F0C850` |
| Focus ring | `3px solid #F3D35A`, offset 2px |
| Success | `#5FD9A2` (bg `rgba(60,201,138,.12)`) |
| Pending / warning | `#F5A862` (bg `rgba(242,153,74,.12)`) |
| Danger | `#F28683` (bg `rgba(239,100,97,.10)`) |
| Info | `#7DBBF4` (bg `rgba(90,169,240,.12)`) |
| **Points / coupons** | teal `#5ED3D0` (bg `rgba(57,195,192,.12)`) |

### 2.4 Rules
1. **Gold is scarce.** Only the single primary action on a screen and the active nav item use it. The old UI used `--gold` everywhere; remove that.
2. **Pending is always orange.** Points and coupons are always teal. Never mix them with money colours.
3. Labels sit **above** fields. Required fields have a red `*`. Errors appear under the field, in text, not only in colour.
4. Destructive or irreversible actions show their **consequence** before the confirm (e.g. "reverse entry + stock returns to Cairo store + 420 points deducted").
5. Disabled buttons explain why, linked via `aria-describedby`.
6. Live calculations appear **while typing**, not after save (deductions, commissions, invoice totals).
7. Inline SVG icons only. No emoji.
8. Every table has: a filter row, a sticky header, a footer with the count ("١–٦ من ٤١٢"), and a horizontal scroll wrapper at small widths.

### 2.5 Component classes (`sr-tokens.css`)
- Layout: `.sr`, `.sr-side`, `.sr-col`, `.sr-main`, `.head`, `.split`, `.grow`, `.aside`, `.stack(.g8/.g12/.g16)`, `.row`, `.between`.
- Surfaces: `.card(.hi/.clip)`, `.pad`, `.pad-h`, `.drawer`, `.modal`, `.overlay`, `.mobile`.
- Actions: `.btn` plus `.p` (primary gold), `.d` (danger), `.g` (ghost), `.sm`, `.lg`, `.w`; `.ibtn`; `.filter(.on)`; `.seg`; `.tabs/.tab`; `.switch`.
- Data: `.tbl`, `.tbl-wrap`, `.tfoot`, `.num`, `.kpis/.kpi`, `.bar`, `.li`, `.map/.pin`.
- Status: `.chip` plus `.c-ok`, `.c-warn`, `.c-bad`, `.c-info`, `.c-neu`, `.c-pts`, `.c-gold`; `.note` plus `.ok`, `.warn`, `.bad`, `.info`, `.neu`.
- Forms: `.field`, `.lbl`, `.inp(.focus/.err)`, `.hint`, `.errt`, `.req`, `.search`.
- Text: `.h1`, `.h2`, `.sub`, `.crumb`, `.muted`, `.dim`, `.soft`, `.faint`, `.ok`, `.warn`, `.bad`, `.info`, `.pts`, `.gold`, `.mono`, `.sr-only`.

**Angular mapping:** move these tokens into CSS custom properties in `src/styles/_tokens.scss` (dark + light under `[data-theme]`). Build the classes as standalone presentational components: `sr-button`, `sr-chip`, `sr-card`, `sr-kpi`, `sr-table`, `sr-field`, `sr-drawer`, `sr-modal`, `sr-note`.

---

## 3. Screen inventory (80 artboards)

File names match the canvas artboards (`<Name>.dc.html`).

| Module | Screens |
|---|---|
| Login / account / look | Main (login), ForgotPassword, Dashboard, DashboardLight, DashboardCustomize, GlobalSearch (Ctrl K), UserProfile, DesignSystem |
| Approvals / users / permissions | Approvals (one inbox for all approvals), Users, UserPermissions, Permissions, PermissionCategoryNew, States (empty / loading / error / offline) |
| Sales | SalesInvoices, InvoiceCreate, InvoiceConfirm, InvoiceDetail, InvoicePrint, SalesReturn, GeneralDiscounts |
| Purchases / customers / suppliers | PurchaseInvoices, PurchaseCreate, PurchaseConfirm, PurchaseReturn, Customers, CustomerProfile, Suppliers, SupplierProfile |
| Stores | Stock (product × warehouse matrix), Products, ProductCard, TransferTablet (1024 wide), StockCount, Warehouses |
| Accounts | ReceiptVoucher, PaymentVoucher, JournalEntry, ChartOfAccounts, Expenses |
| Loyalty | Inspections, InspectionCreate, InspectionDetail, PointsRequests, PointsTransfer, PlumberProfile, GiftsStock, GiftsSettlement, Warranty, WarrantyVerify (390 wide, public), CouponSettings |
| Sales reps | RepProfile, Treasury (rep custody + cash handover), RepsMap, Commissions |
| HR | EmployeeProfile, Attendance, Leaves, Payroll, Loans, Deductions |
| My space (mobile web) | MySpaceMobile, LeaveMobile, PayslipMobile |
| Notifications / messages / offers | Notifications (panel), NotificationsAll, Messages, OfferCreate, OfferDetail |
| Reports / import | Reports (hub), ReportsFinancial, ReportsSales, ReportsHR, ImportReview (Excel import with row validation) |
| Settings | CompanySettings, Regions (governorates + cities on one page, bilingual), LeaveTypes (+ official holidays), ChangeLog (audit, immutable) |
| Shared components | Sidebar (11 groups), Topbar (search Ctrl K, approvals, messages, notifications, theme and language toggles) |

Desktop artboards are 1440 × 900–1400. Mobile artboards are 390 wide.

---

## 4. Business rules captured in the design

### Confirmed by the user in chat
- **Plumber gift balance is in coupons, not money.** Show it as a coupon count in teal.
- **Sales reps have no fixed working hours.** No lateness and no hour-based deductions for reps. Rep "attendance" means they opened the app and checked in from a location. Lateness and deductions apply to office and factory employees only (Attendance, Deductions, Payroll, ReportsHR all reflect this).
- **Dashboard must include:** a profit card, an expenses card, a "top reps by collection" list showing each rep's amount, and a separate card for the single top collector.
- **Stock matrix:** each product's total sits in a coloured status box (ok / low / out).
- A **"new permission category"** page exists (PermissionCategoryNew).

### Assumed in the design — confirm before building
| Item | Value used | Where |
|---|---|---|
| Points per coupon | 60 (editable in settings) | CouponSettings, PlumberProfile, PointsTransfer |
| Cash coupon value | 1,200 EGP | CouponSettings, Treasury, Approvals |
| Mixed coupon | 900 EGP cash + stars | CouponSettings |
| Gift coupons | redeemed for items priced in coupons | GiftsStock, GiftsSettlement |
| VAT | 14% | InvoiceCreate, InvoiceConfirm, InvoicePrint |
| Collection commission tiers | 0–50k → 1% · 50,001–150k → 1.5% · 150,001+ → 2% | Commissions |
| Coupon-payout commission | 20+ coupons → 500 EGP · 50+ → 1,500 EGP | Commissions |
| 2026 holidays | Eid al-Fitr 20–22 Mar, Eid al-Adha 26–29 May (+ fixed national days) | LeaveTypes |
| Hourly rate for deductions | monthly salary ÷ (24 days × 8 h) | Deductions |

### Workflow rules shown in the screens
- Points become a coupon **only after** the inspection is approved by a sales rep (InspectionDetail → PointsTransfer → Approvals).
- A points transfer from merchant to plumber is blocked if the merchant's balance is too low. The approve button is disabled and explains why.
- Cash paid by a rep to a plumber goes into the rep's **custody** (account 1105xx) until the treasury confirms it.
- Cancelling a confirmed invoice = reverse journal entry + stock return + points reversal. It goes through Approvals, and a rejection needs a reason.
- Commission tiers cannot overlap. Save is blocked until fixed.
- Cities that have customers cannot be deleted, only deactivated.
- The ChangeLog cannot be edited or deleted.
- System accounts in the chart of accounts (locked icon) cannot be deleted or have their code changed. Each new rep gets a sub-account automatically.

---

## 5. Sample data reconciled across screens
Keep these consistent if you reuse the mock data for seed or fixtures:
- INV-1040 total = 121,859 EGP.
- Treasury pending total = 41,700 EGP; Mahmoud's custody after confirmation = 8,400 EGP.
- September: COGS 828,400 · profit 202,100 · sales growth 8.3%.
- Gift stock value = 45,600 EGP · stock count variance = −926.
- Payroll October = 412,800 EGP across 42 people.
- Rep custody accounts: كريم سامي 18,000 · أحمد فتحي 25,000 · محمود عبد الله 30,000 (total 73,000).

---

## 6. Implementation plan for `AlsadatUi` (Angular 17, standalone)

1. **Tokens first.** Add `_tokens.scss` (dark + light) and the global base: fonts, focus ring, RTL. Remove hard-coded `--gold` uses.
2. **Shell.** Build `SidebarComponent` (11 groups, permission-aware) and `TopbarComponent` (search, approvals badge, notifications, theme and language). Layout: `ShellComponent` with `<router-outlet>`.
3. **UI kit** (`shared/ui/`): button, chip, card, kpi, table (sorting, paging, empty / loading / error states from `States`), field, drawer, modal, note. These are dumb components with signal inputs.
4. **Features in priority order:** Approvals → Sales invoices (list / create / confirm) → Inspections + Points → Treasury → Stock → the rest. Each feature is a lazy route with a container component plus presentational components, and an API service.
5. **Backend contract per screen.** Derive DTOs from each screen's visible fields (see `BACKEND_MODULE_ANALYSIS.md`). Flag missing endpoints, e.g. the unified approvals inbox and rep check-in location.
6. **Accessibility pass.** Check every table and form against rule set 2.4.

---

## 7. Getting the files
The 80 `.dc.html` files plus `canvas.json` are downloadable from the canvas artifact. They are static HTML mockups, not Angular code. Use them as a pixel and behaviour reference. The stylesheet is in `claude/design/sr-tokens.css`.
