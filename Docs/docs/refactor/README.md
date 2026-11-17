# متابعة الـ Refactor — اقرا ده الأول

الفولدر ده فيه الخطة وطريقة متابعتها:

| الملف | فيه إيه |
|---|---|
| `PLAN.md` | الخطة كاملة: المشاكل الحرجة C1–C16، الأساس F0–F16، الموديولات M1–M14، القرارات D0–D15 |
| `TRACKER.xlsx` | المتابعة: شيت لكل جزء من الخطة، وكل سطر = session بهدفها وحالتها |
| `F0.md` (وبعدين `F1.md`، `M1.md`…) | تفاصيل الخطوة الحالية بالملفات والسطور |
| `README.md` (الملف ده) | شرح كل تعديل: اتعمل ليه، اتغير فيه إيه، وإزاي تتأكد منه |

حالة المشروع لحظة بلحظة (الـ Checkpoint) في `PROJECT_CONTEXT.md` في أول الريبو.

---

## طريقة الشغل

1. **قبل أي session:** Claude بيشرحلك الـ business والوضع الحالي والتعديلات والقرارات المطلوبة (بتأثير كل قرار)، ويستنى موافقتك.
2. **التنفيذ:** Claude بيكتب الكود على نسخة الريبو اللي عنده، ويعمل commit و push على `fix/backend` بعد كل جزء صغير.
3. **البيلد والتجربة عندك:** بيئة Claude مقفول عليها `api.nuget.org`، فمش بتقدر تعمل restore للـ packages ولا build. فإنت بتعمل `git pull` وتبني وتشغّل التستات في Visual Studio أو بالأوامر دي:
   ```powershell
   cd "C:\Users\b.zakaria\Desktop\SR\New folder (5)\AlSadat-Seram\AlSadat-Seram-Project\AlSadat-Seram"
   dotnet build AlSadat-Seram.slnx > "C:\Users\b.zakaria\Desktop\SR\build.log" 2>&1
   dotnet test AlSadat-Seram.slnx > "C:\Users\b.zakaria\Desktop\SR\test.log" 2>&1
   ```
   أو أسهل: شغّل `sync-and-build.ps1` اللي في أول الريبو (كليك يمين › Run with PowerShell) — بيعمل pull وbuild وtest ويكتب `pull.log` و`build.log` و`test.log`.
   لو فيه أخطاء، Claude بيقرا الملفات دي من فولدر `SR` على جهازك ويصلّح. (الملفات دي في `.gitignore` مش هتترفع.)
4. **بعد كل جزء:** Claude بيحدّث 3 حاجات: الـ Checkpoint في `PROJECT_CONTEXT.md`، حالة الـ session في `TRACKER.xlsx`، وشرح التعديل هنا تحت كود الـ session.

### حالات الـ session في الـ tracker
| الحالة | معناها |
|---|---|
| لم يبدأ | لسه |
| جاري | شغالين فيها دلوقتي (أو اتقطعت في النص — شوف الـ Checkpoint) |
| متوقف | مستنية قرار أو حاجة من برا |
| خلص | الهدف اتحقق، البيلد والتستات خضرا عندك، واتعمل push |

---

## سجل التعديلات

كل session ليها قسم بكودها (نفس الكود اللي في الـ tracker). القسم بيتكتب **قبل** التنفيذ كـ «مخطط»، وبيتحدّث **بعده** باللي اتعمل فعلاً والـ commits.

### SETUP — تجهيز طريقة الشغل · خلص · 2026-11-16 · commit `87d7ef0`

**اتعمل ليه:** علشان الخطة والتصميم والمتابعة يبقوا في الريبو نفسه، وأي session (من أي جهاز أو أكونت) تكمّل من نفس النقطة.

**اللي اتضاف** (مفيش سطر كود اتغير):
- `CLAUDE.md` — التعليمات اللي Claude Code بيقراها لوحده: المسارات، القرارات، قواعد الكود، الشرح قبل أي جزء، تأثير كل قرار، والـ checkpoints.
- `PROJECT_CONTEXT.md` — الحالة والـ Checkpoint وسجل الـ sessions.
- `.claude/commands/start.md` و`handoff.md` — أوامر `/start` و`/handoff`.
- `Docs/` — التصميم الجديد (الشاشات، الصور، الـ preview، الـ tokens)، الحكايات، الخطة، `TRACKER.xlsx`، والملف ده.

**إزاي تتأكد:** بعد الـ pull، افتح `Docs/docs/refactor/TRACKER.xlsx` وشوف شيت «الملخص» (١١١ session، كلها «لم يبدأ»)، وافتح Claude Code من فولدر `SR` واكتب `/start`.

---

### F0-S1 — الشغل المعلّق + C1 + C2 · متوقف (مستني قرارات)

**الـ business:**
- **C1:** موظف الـ HR اللي عنده صلاحية «إضافة موظف» المفروض يضيف موظف وبس. النهارده يقدر يبعت في الطلب `RolesName = ["Admin"]` فيعمل حساب أدمن بكل الصلاحيات — من غير ما حد يعرف.
- **C2:** كل عميل جديد (موزّع/تاجر/وكيل) بيتعمل بكلمة سر واحدة مكتوبة في الكود. أي حد يعرف رقم موبايل عميل يقدر يدخل بحسابه.

**الوضع الحالي:**
- على جهازك فيه شغل «إلغاء كل جلسات مستخدم» لسه مش متعمله commit (٦ ملفات مختلفة عن الريبو: `EmployeePermissions.cs`، `ServiceManager.cs`، `AuthController.cs`، `UserSessionService.cs`، `IUserSessionService.cs`، `LocalizationResources.cs`)، وعليه ٤ ملاحظات من الـ review.
- C1 أعقد من «شيل الـ role من الـ DTO»: نص الـ controllers لسه بتتحكم بالـ roles (`HR`، `Accountant`، `StockManager`، `Employee`…)، فالموظف لازم ياخد role علشان يشتغل لحد M1. والأدمن نفسه (`super_admin`) بيتحسب من role الـ `Admin`.
- C1: `EmployeeService.cs` (~سطر 78–86 و478–496) و`RepresentativeService.cs` (~سطر 71–79 و225–231) بيعملوا `AddToRole(s)Async` بالاسم اللي جاي من الـ DTO.
- C2: `DistributorsAndMerchantsService.cs` سطر 91.

**التعديلات (المخطط):**
- الشغل المعلّق: حسب قرارك (نكمّله ونصلّح ملاحظاته / نأجله لـ M1 / نمسحه).
- C1: حسب قرارك — قايمة roles مسموحة من غير `Admin` (المقترح)، أو role ثابت لكل نوع مستخدم.
- C2: حسب قرارك (العميل يتعمل من غير دخول لحد ما الأدمن يفعّله بكلمة سر مؤقتة، أو بديل تاني).

**القرارات المطلوبة:** هتتعرض عليك بتأثيرها قبل التنفيذ.

**إزاي تتأكد:** هيتكتب بعد التنفيذ.

### F0-S2 — C3 + C15 + C16 + Google · جاري

**الـ business:**
- **C3:** رصيد العميل (نقاط، فلوس، مديونية) المفروض يتغير من الفواتير والسندات بس. النهارده شاشة تعديل بيانات العميل تقدر تكتب الرصيد بإيدها.
- **C15:** فولدر `publish/` القديم مرفوع على الريبو وفيه مفتاح توقيع الـ JWT.
- **C16:** الـ access token عمره ١٠ ساعات، فالحساب اللي يتوقف يفضل شغال لحد ١٠ ساعات.

**اللي اتعمل (2026-11-17):**

1. **C3 · commit `71cf2bc`** — `Infrastructure/Services/DistributorsAndMerchantsService.cs` › `EditDistributorOrMerchant`: اتشالت السطور اللي كانت بتكتب `CashBalance` و`Balance` (النقاط) و`Indebtedness` من الطلب. لو الفرونت بعت أرصدة، بتتجاهل. الأرصدة بتتغير من المستندات بس.
   - **ماتغيرش عمدًا:** الـ method لسه رسايلها مكتوبة نص (مش localization keys) والـ controller لسه `[Authorize(Roles=...)]`، لأن الموديول ده كله هيتبني من جديد في M5 (جدول `Customers` جديد)، وكود صلاحيات العملاء لسه ماتحددش. ده استثناء متعمد من قواعد `CLAUDE.md` ومتسجل هنا.
2. **شيل Google · commit `70b1f24`** — اتمسحوا `GoogleAuthService.cs` و`IGoogleAuthService.cs` و`GoogleSignInVM.cs` (مكانوش متسجلين ولا حد بيناديهم)، والـ package `Google.Apis.Auth` من `Infrastructure.csproj`، و`using Google;` من `UnitOfWork.cs`، وسطر «Google token» من `ExceptionHandlingMiddleware.cs`.
   - **فاضل:** مفتاح الترجمة `Auth.InvalidGoogleToken` في `LocalizationResources.cs` — مسبتوش علشان الملف ده متعدّل عندك ومش متعمله commit؛ هيتشال مع قرار الشغل المعلّق.

**فاضل في F0-S2:**
- **C15:** مستني إجابتك: فيه سيرفر شغال من فولدر `publish/`؟ (شيله من الريبو هيمسحه من جهازك كمان مع الـ pull.)
- **C16:** عمر الـ token بيتقري من `JwtSettings:DurationInMinutes` في الـ user secrets عندك (مش في الريبو). الأمر:
  ```powershell
  cd "C:\Users\b.zakaria\Desktop\SR\New folder (5)\AlSadat-Seram\AlSadat-Seram-Project\AlSadat-Seram"
  dotnet user-secrets set "JwtSettings:DurationInMinutes" "15" --project AlSadat-Seram.Api
  ```

**إزاي تتأكد:**
1. `dotnet build` يعدّي من غير errors (أهم حاجة: مفيش حاجة كانت معتمدة على Google).
2. C3: سجّل دخول بأدمن أو محاسب، وابعت `PUT /api/DistAndMerch/edit/{userId}` بنفس بيانات العميل ومعاها `"PointsBalance": 999999, "cashBalance": 999999, "indebtedness": 999999`. الرد نجاح، و`GET /api/DistAndMerch/get/{userId}` يرجّع الأرصدة زي ما كانت.
