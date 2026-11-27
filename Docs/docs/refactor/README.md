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

> **من 2026-11-25 الشغل الأساسي بقى على Claude Code على جهازك.** Claude بيشتغل جوه `C:\Users\b.zakaria\Desktop\SR` مباشرة، وبيعمل build وtest بنفسه ويقرا النتيجة، فمش محتاج `sync-and-build.ps1` ولا ملفات الـ log. ابدأ أي session بـ `/start` واقفلها بـ `/handoff`. الخطوات اللي تحت دي لسه شغالة لو اشتغلت من claude.ai.

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
4. **بعد كل جزء:** Claude بيحدّث 3 حاجات: الـ Checkpoint في `PROJECT_CONTEXT.md`، وحالة الـ session في `TRACKER.xlsx` (دايمًا بـ `tools/tracker.py`، علشان الـ formulas والتنسيق ما يبوظوش)، وشرح التعديل هنا تحت كود الـ session.

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

### F0-S1 — الشغل المعلّق + C1 + C2 · خلص · 2026-11-17

**الـ business:**
- **C1:** موظف الـ HR اللي عنده صلاحية «إضافة موظف» كان يقدر يبعت `RolesName = ["Admin"]` ويعمل حساب أدمن بكل الصلاحيات.
- **C2:** كل عميل جديد كان بيتعمل بكلمة سر واحدة مكتوبة في الكود (`12345678Ss+`)، فأي حد يعرف رقم عميل يدخل بحسابه.
- **إلغاء كل الجلسات:** الأدمن (أو صاحب الصلاحية) يقدر يطلّع مستخدم من كل الأجهزة في لحظة — مثلًا لو موبايل موظف اتسرق.

**القرارات (2026-11-17):** الشغل المعلّق يكمل · C1: الأدمن بس هو اللي يعمل أدمن · C2: كلمة السر = رقم التليفون + أول حرف من الاسم، وتغيير إجباري أول دخول · الـ roles هتتلغي خالص في M1 (D16).

**اللي اتعمل:**

1. **إلغاء كل الجلسات · commit `2e6a489`** — الشغل اللي كان على جهازك، بعد تصليح ملاحظات الـ review:
   - `Domain/Common/EmployeePermissions.cs`: صلاحية جديدة `HR.Employees.RevokeSessions` (بتتضاف للداتابيز لوحدها وقت التشغيل). الـ namespace والـ XML doc رجعوا.
   - `Infrastructure/Services/AuthServices/UserSessionService.cs`: بيلغي كل الـ refresh tokens ويغيّر الـ security stamp في transaction واحدة، ولو تغيير الـ stamp فشل بيرجّع كل حاجة زي ما كانت (`Auth.RevokeSessionsFailed`)، وبيسجّل في `AuthAuditLogs` مين عمل كده ومن أنهي IP.
   - `AlSadat-Seram.Api/Controllers/AuthController.cs`: `POST api/auth/revoke-sessions/{targetUserId}` بالـ `CancellationToken`، والـ controller مبقاش بيعتمد على Infrastructure.
   - `ServiceManager.cs` **ماتغيرش** — فضل بياخد الـ service من الـ DI (التعديل المحلي اللي كان بيعمل نسخة تانية اتلغى).
2. **C1 · commit `9f08ad2`** — `Domain/Common/RoleAssignmentPolicy.cs` (جديد) و`ICurrentUserService.IsSuperAdmin` (جديد):
   - إضافة أو تعديل موظف/مندوب بـ role الـ `Admin` محتاج إن اللي بيعمل الطلب يكون مدير نظام (`super_admin`).
   - تعديل بيانات مستخدم هو أصلًا أدمن محتاج مدير نظام برضه.
   - غير كده: `403` برسالة `Role.AdminAssignmentForbidden`.
   - الملفات: `EmployeeService.cs` (Add + Update)، `RepresentativeService.cs` (Add + Update).
3. **C2 · commit `fce72a0`:**
   - `Domain/Common/InitialPasswordPolicy.cs` (جديد): كلمة السر الأولى = التليفون + أول حرف من الاسم زي ما هو مكتوب. مثال: `01012345678` و«محمود علي» ← `01012345678م`.
   - `DistributorsAndMerchantsService.AddNewDistributorOrMerchant`: بيستخدمها بدل الـ `12345678Ss+`، وبيحط على الحساب علامة «لازم يغيّر كلمة السر» (user claim `must_change_password` — مؤقتًا لحد ما M1 يضيف عمود، علشان مانحتاجش migration دلوقتي).
   - الدخول والـ refresh: الـ token فيه العلامة، والرد فيه `mustChangePassword: true`.
   - `AlSadat-Seram.Api/Filters/PasswordChangeRequiredFilter.cs` (جديد): طول ما العلامة موجودة، أي طلب بيرجع `403` برسالة `Auth.PasswordChangeRequired`، ماعدا تغيير كلمة السر والخروج.
   - `ProfileService.ChangePasswordAsync`: بعد التغيير العلامة بتتشال والـ security stamp بيتغير، فالعميل بيدخل تاني بكلمة سره الجديدة.
   - التليفون كان أصلًا إجباري وعليه unique index على `AspNetUsers`، والدخول بيدوّر بالتليفون الأول.

**ماتغيرش عمدًا:** الـ methods القديمة دي لسه رسايلها نص مش localization keys (الجزء الجديد بس اللي بالـ keys)، لأنها هتتبني من جديد في M5 وM9 وM11.

**تنبيه أمني على C2:** كلمة السر الأولى ممكن تتخمّن لو حد يعرف تليفون العميل واسمه، لحد ما العميل يدخل أول مرة ويغيّرها. اللي بيقلل الخطر: قفل الحساب بعد ٥ محاولات غلط، وإن الحساب مايقدرش يعمل أي حاجة غير تغيير كلمة السر.

**إزاي تتأكد:**
1. `sync-and-build.ps1` ← `build.log` من غير errors و`test.log` كله ناجح.
2. **إلغاء الجلسات:** ادخل بأدمن، و`POST /api/auth/revoke-sessions/{id}` لمستخدم داخل من جهاز تاني ← رد نجاح، و`refresh` من الجهاز التاني يرجّع 401.
3. **C1:** ادخل بمستخدم HR مش أدمن، وابعت إضافة موظف بـ `"RolesName": ["Admin"]` ← `403`. نفس الطلب بأدمن ← نجاح.
4. **C2:** اعمل عميل جديد بتليفون `01000000001` واسم «تجربة» ← ادخل بـ `01000000001` وكلمة سر `01000000001ت` ← الرد فيه `mustChangePassword: true` ← أي endpoint تاني (مثلًا `GET /api/Profile`) يرجّع `403` ← `PUT /api/Profile/change-password` ينجح ← ادخل بكلمة السر الجديدة ← `mustChangePassword: false` وكل حاجة شغالة.

### F0-S2 — C3 + C15 + C16 + Google · خلص · 2026-11-17

**الـ business:**
- **C3:** رصيد العميل (نقاط، فلوس، مديونية) المفروض يتغير من الفواتير والسندات بس. النهارده شاشة تعديل بيانات العميل تقدر تكتب الرصيد بإيدها.
- **C15:** فولدر `publish/` القديم مرفوع على الريبو وفيه مفتاح توقيع الـ JWT.
- **C16:** الـ access token عمره ١٠ ساعات، فالحساب اللي يتوقف يفضل شغال لحد ١٠ ساعات.

**اللي اتعمل (2026-11-17):**

1. **C3 · commit `71cf2bc`** — `Infrastructure/Services/DistributorsAndMerchantsService.cs` › `EditDistributorOrMerchant`: اتشالت السطور اللي كانت بتكتب `CashBalance` و`Balance` (النقاط) و`Indebtedness` من الطلب. لو الفرونت بعت أرصدة، بتتجاهل. الأرصدة بتتغير من المستندات بس.
   - **ماتغيرش عمدًا:** الـ method لسه رسايلها مكتوبة نص (مش localization keys) والـ controller لسه `[Authorize(Roles=...)]`، لأن الموديول ده كله هيتبني من جديد في M5 (جدول `Customers` جديد)، وكود صلاحيات العملاء لسه ماتحددش. ده استثناء متعمد من قواعد `CLAUDE.md` ومتسجل هنا.
2. **شيل Google · commit `70b1f24`** — اتمسحوا `GoogleAuthService.cs` و`IGoogleAuthService.cs` و`GoogleSignInVM.cs` (مكانوش متسجلين ولا حد بيناديهم)، والـ package `Google.Apis.Auth` من `Infrastructure.csproj`، و`using Google;` من `UnitOfWork.cs`، وسطر «Google token» من `ExceptionHandlingMiddleware.cs`.
   - **فاضل:** مفتاح الترجمة `Auth.InvalidGoogleToken` في `LocalizationResources.cs` — مسبتوش علشان الملف ده متعدّل عندك ومش متعمله commit؛ هيتشال مع قرار الشغل المعلّق.

3. **C15 · commit `3867582`** — فولدر `publish/` اتشال من الريبو واتضاف لـ `.gitignore` الباك إند (إنت أكدت إن مفيش سيرفر شغال منه). الـ API بيقرا المفتاح من الـ user secrets. لما نرفع على Azure هنستخدم مفتاح جديد خالص.
4. **C16** — إنت غيّرت `JwtSettings:DurationInMinutes` لـ ١٥ في الـ user secrets (2026-11-17). مفيش تغيير كود.
5. مفتاح `Auth.InvalidGoogleToken` اتشال مع commit `2e6a489`.
6. **أول build عندك (2026-11-17)** طلّع error واحد: `Newtonsoft` في `salesInvoiceService.cs` — كان جاي من package جوجل اللي اتشالت، والـ `using` أصلًا مش مستخدم. اتشال في commit `809b555`.

**النتيجة عندك (2026-11-17):** build ناجح (0 errors) و١٥ من ١٥ test ناجحين. التجربة اليدوية لسه (محتاجة الـ API يشتغل — أسهل بعد F1).

**إزاي تتأكد:**
1. `dotnet build` يعدّي من غير errors (أهم حاجة: مفيش حاجة كانت معتمدة على Google)، وفولدر `publish/` اختفى بعد الـ pull.
2. C3: سجّل دخول بأدمن أو محاسب، وابعت `PUT /api/DistAndMerch/edit/{userId}` بنفس بيانات العميل ومعاها `"PointsBalance": 999999, "cashBalance": 999999, "indebtedness": 999999`. الرد نجاح، و`GET /api/DistAndMerch/get/{userId}` يرجّع الأرصدة زي ما كانت.

---

### F1-S1 — الترقية لـ .NET 10 · ✔ خلص (2026-11-17)

**الـ business:** مفيش حاجة المستخدم هيشوفها. ده تجديد للأساس: دعم .NET 9 بينتهي في ١٠ نوفمبر ٢٠٢٦ (يعني خلص)، وجهازك أصلًا مفيهوش .NET 9 — عليه 8 و10 بس — فده اللي مخلّي التستات والـ API محتاجين `DOTNET_ROLL_FORWARD`.

**الوضع الحالي:** كل المشاريع `net9.0`، وEF Core و Identity و JwtBearer على 9.0.10، وفيه packages مش مستخدمة أو قديمة، و AutoMapper 12.0.1 عليه ثغرات معروفة (تحذير NU1903 في البيلد).

**التعديلات (المخطط):**
- كل الـ `csproj` ← `net10.0`، وكل packages مايكروسوفت (EF Core، Identity، JwtBearer، OpenApi، Design، Tools) ← آخر 10.0.x، و`dotnet-ef` في `dotnet-tools.json` ← 10.0.x.
- packages التستات (Test SDK، xunit runner، coverlet) ← آخر نسخ ثابتة.
- `Microsoft.AspNetCore.Http.Features` 5.0.17 (قديمة جدًا) ← تتشال، و`Application` ياخد `FrameworkReference` لـ `Microsoft.AspNetCore.App` (علشان `IFormFile`).
- `FluentValidation` (مش مستخدمة) ← تتشال.
- AutoMapper ← **اتشال (D19 = 1)** — ✔ (الجزء الأول، شوف تحت).
- `sync-and-build.ps1` ← يتشال منه `DOTNET_ROLL_FORWARD`.

**القرارات:** D19 اتقرر 2026-11-17 → (1) نشيل AutoMapper ونكتب الـ mapping بإيدينا.

#### الجزء الأول — شيل AutoMapper ✔ (2026-11-17)
- `Application/Mappings/MappingProfile.cs` ← اتمسح. `Application/Mappings/ProfileMappings.cs` ← جديد (`ToProfileDto()`، بيرجّع نفس شكل الـ response بالظبط).
- `ProfileService` و`NotificationService` و`ServiceManager` ← اتشال منهم `IMapper`. map الإشعارات مكانش حد بيستخدمه، لأن الكود اللي كان بيستخدمه متعلّق عليه.
- `Program.cs` ← اتشال `AddAutoMapper`. `Application.csproj` و`AlSadat-Seram.Api.csproj` ← اتشالت packages الـ AutoMapper، وبكده التحذير NU1903 مش هيظهر تاني.
- ملاحظة: رسايل `ProfileService` لسه نص ثابت، لأن `ProfileController` مش بيستخدم `HandleResult`. هتتصلح مع الكنترولر في F2.

**إزاي تتأكد:** `sync-and-build.ps1` ← 0 errors ومن غير NU1903، والتستات ١٥/١٥. و`GET /api/Profile` لازم يرجّع نفس الـ JSON اللي كان بيرجع قبل كده.
✔ اتأكد عند بلال 2026-11-17: 0 errors، من غير NU1903، والتستات ١٥/١٥.

#### الجزء التاني — net10.0 والـ packages ✔ (2026-11-17)
- كل الـ `csproj` الخمسة (Api، Application، Domain، Infrastructure، UnitTests) ← `net10.0`.
- packages مايكروسوفت ← **10.0.15**، ودي آخر نسخة 10.0.x على NuGet يوم 2026-11-17: EF Core، SqlServer، Design، Tools، Identity.EntityFrameworkCore (في Domain وInfrastructure)، JwtBearer، و`System.Drawing.Common`. و`dotnet-ef` في `AlSadat-Seram.Api/dotnet-tools.json` ← 10.0.15.
- `System.IdentityModel.Tokens.Jwt` 8.14.0 ← 8.23.0، علشان نسخ مكتبات `Microsoft.IdentityModel.*` تفضل متطابقة مع JwtBearer 10.
- التستات: `Microsoft.NET.Test.Sdk` 18.12.0، و`xunit` 2.9.3، و`xunit.runner.visualstudio` 4.0.1، و`coverlet.collector` 10.1.1.
- **اتشال:** `Microsoft.AspNetCore.OpenApi` (مكانش مستخدم، الـ Swagger شغال بـ Swashbuckle)، و`FluentValidation` (مكانش مستخدم)، و`Microsoft.AspNetCore.Http.Features` 5.0.17 (اتحط مكانها `FrameworkReference Microsoft.AspNetCore.App` في `Application` علشان `IFormFile`).
- `sync-and-build.ps1` ← اتشال منه `DOTNET_ROLL_FORWARD`، لأن الـ runtime بقى 10 خلاص.
- أول بيلد على net10 طلع NU1903 على `System.Security.Cryptography.Xml` 10.0.0. الـ package دي بتيجي من EPPlus 8.4.1، والإصلاح بتاعها موجود من 10.0.10. علشان كده اتثبتت في `Infrastructure.csproj` على 10.0.15، ومعاها comment إنها تتشال لما EPPlus يطلب نسخة متصلّحة.
- **فضل زي ما هو عن قصد:** `Swashbuckle.AspNetCore` 9.0.6. النسخة 10 بتستخدم `Microsoft.OpenApi` 2، ودي محتاجة تعديل في كود الـ Swagger في `Program.cs`، فهتبقى في جزء لوحدها بعدين.

✔ عند بلال 2026-11-17: 0 errors، والتستات ١٥/١٥ على `net10.0`. وبعد تثبيت `Cryptography.Xml` (`d8bf24f`): 0 errors، من غير أي NU1903، والتستات ١٥/١٥.

**الـ commits:** `0d0ce70`، `feeea02`، `d8bf24f`.
**إزاي تتأكد:** `sync-and-build.ps1` ← لازم البيلد يطلع 0 errors ومن غير أي NU1903، والتستات ١٥/١٥ والسطر يقول `net10.0`. وبعدها شغّل الـ API من Visual Studio وافتح `/swagger` ← لازم يفتح ويعمل login بالتوكن عادي.

---

### F1-S2 — migration نظيفة واحدة (D0) · ✔ خلص (2026-11-25) · commit `8e4bb28`

**الـ business:** مفيش حاجة المستخدم هيشوفها. الفايدة إن أي حد يعمل clone (أو بيئة Azure بعدين) يشغّل الـ API، فالداتابيز تتعمل من الصفر لوحدها والـ seed يملاها: المحافظات والمدن، والأدمنز، والصلاحيات، وشجرة الحسابات، والخصومات، وإعدادات الكوبونات. ومن هنا كل موديول بيضيف migration صغيرة فوق نقطة بداية نضيفة.

**الوضع الحالي:**
- فولدر `Infrastructure/Migrations` كان فيه 4 migrations (`nhbikhb`، `AddPreferredLanguageToApplicationUser`، `AddPermissionSystem`، `auth-refactor`) وملفات الـ Designer بتاعتهم والـ snapshot، يعني 9 ملفات. ده اللي كان مكتوب في الملاحظات القديمة على إنه «9 migrations».
- مفيش أي `Sql` ولا `InsertData` جوه الـ migrations، فمفيش data logic بيضيع. الـ seed كله في `Infrastructure/Data/DbInitializer.cs`.
- الـ API مكانش بيعمل `Migrate` خالص: الداتابيز بتتعمل يدوي بـ `dotnet ef database update`، وبعدين الـ seed بيشتغل مع كل startup.

**القرارات (2026-11-17):**
- التوليد: سكريبت `make-baseline.ps1` يتشغّل على جهاز بلال، وClaude يراجع الـ SQL ويعمل push (1-أ).
- **D20:** الـ migrations بتتطبّق لوحدها مع تشغيل الـ API في كل البيئات (2-ج). EF Core 9 وما بعده بياخد lock على الداتابيز وقت الـ `Migrate`، فلو أكتر من نسخة قامت مع بعض مش هيحصل تضارب. الثمن إن يوزر الداتابيز بتاع الـ API محتاج صلاحية DDL في الإنتاج — هنرجعله وقت الـ pipeline على Azure.
- الـ baseline بتطلع من الـ model زي ما هو، وكل جدول ملوش لازمة بيتشال في الموديول بتاعه (3-أ).
- الداتابيز الجديدة باسم جديد `SadatSearmErp` بدل ما نمسح القديمة، فالقديمة بتفضل زي ما هي.
- السكريبتات القديمة (`apply_phase1.py`، `apply_phase2.py`، `phase1-core.patch`) تتمسح.

#### الجزء الأول — مسح القديم وتجهيز السكريبت ✔ (2026-11-17)
- اتمسحت ملفات `Infrastructure/Migrations` التسعة، والسكريبتات القديمة التلاتة.
- اتضاف `make-baseline.ps1` في أول الريبو (مؤقت، بيتمسح في آخر الـ session)، واتضاف `/_out/` و`/make-baseline.log` لـ `.gitignore`.

#### الجزء التاني — الـ migration والـ auto-migrate ✔ (2026-11-25)

**المشكلة اللي ظهرت:** الـ `InitialCreate` من الـ model زي ما هو ما عدّتش على SQL Server (error 1785): جدول `PointTransactions` ليه FK-ين بـ cascade على `AspNetUsers`. السبب إن الـ migration القديمة `nhbikhb` كان فيها `NoAction` بدل `Cascade` في 37 FK (متعدّلة بالإيد)، فالداتابيز القديمة فيها 3 cascades والـ model بيطلب 40.

**القرار D21 (اختيار b):** كل FK بـ `Cascade` بقى `ClientCascade`: الداتابيز بترفض مسح سجل ليه تابعين، وEF بيمسح التابعين بس لو الكود محمّلهم. المستثنى: `UserPermissions` (2) و`Permissions`→`Modules`. مفيش service اتغيّر.

**اللي اتغيّر:**
- `Infrastructure/Data/AppDbContext.cs`: ميثود `DisableDatabaseCascades`، وآخر سطر في `OnModelCreating` بيناديها.
- `Infrastructure/Migrations/20261125161145_InitialCreate.cs` والـ Designer والـ snapshot: 57 جدول.
- `Infrastructure/Data/ApplicationBuilderExtensions.cs`: `SeedDatabaseAsync` بقت `MigrateAndSeedDatabaseAsync` وبتعمل `MigrateAsync()` قبل الـ seed (D20). `Program.cs` بينادي الاسم الجديد.
- اتمسح `make-baseline.ps1` وسطوره في `.gitignore`.

**اتأكدنا من:** build بـ 0 errors، و15/15 tests. الـ API اشتغل مرتين على `SadatSearmErp`: الأولى عملت الجداول والـ seed (5 يوزرز، 14 حساب، 22 محافظة، 72 مدينة، 15 صلاحية)، والتانية ما غيّرتش حاجة. والأعمدة (585) والـ indexes (151) والـ FKs (78) زي الداتابيز القديمة بالظبط.

**حاجة حصلت:** أول تجربة تشغيل راحت للداتابيز القديمة `AlSadateSeram`، لأن `ConnectionStrings:con` كان اتشال من الـ user secrets. الـ migration وقعت في أول جملة واترجعت (rollback)، واتأكدنا إن مفيش حاجة اتغيّرت فيها.

**إزاي تتأكد:**
1. خلّي `ConnectionStrings:con` يشاور على `SadatSearmErp` (user secrets أو `appsettings.json`).
2. شغّل الـ API، وبعدين في SSMS شغّل ده: `SELECT * FROM SadatSearmErp.dbo.__EFMigrationsHistory` — المفروض يطلع صف واحد `20261125161145_InitialCreate`.
3. لو عايز تجرّب من الصفر، اكتب اسم داتابيز مش موجودة: الـ API هيعملها ويملاها لوحده.

---

### F2-S1 — الـ response الموحد (الهيكل) · ✔ خلص (2026-11-25) · commits `66146b5`، `0725cde`، `2e14728`

**الـ business:** كل الشاشات، وبالذات شاشة «الحالات» (14): الفرونت يلاقي أي خطأ بنفس الشكل، ويعرض للمستخدم كود يبعته لمسؤول النظام (`ERR-503 · الوقت`).

**قبل كده:** 400 و415 كانوا `ProblemDetails`؛ 401 بحروف كبيرة و«Unauthorized»؛ 404 و405 و429 رد فاضي؛ والـ middleware بيكتب PascalCase.

**اللي اتغيّر:**
- **D22:** `Domain/Common/ErrorCodes.cs` (جديد) و`Result<T>`: كل failure فيه `code` و`traceId`. `messageKey` مبقاش يطلع في الـ JSON. الرسالة الحرفية القديمة بتاخد كود عام حسب الـ status.
- `AlSadat-Seram.Api/Responses/ApiErrorWriter.cs` (جديد): مكان واحد بيكتب 400 (model validation) و401 و403 و404 و405 و415 و429 و500 بالعربي والإنجليزي. أسامي الحقول camelCase، ونص الـ JSON parser مبيوصلش للعميل.
- `ExceptionHandlingMiddleware`: الـ 409 للتعارض وتكرار القيم والـ FK بس، والباقي 500. الطلب اللي العميل قفله مبيتسجلش كخطأ.
- `Program.cs`: `UseStatusCodePages`، و`Retry-After` في الـ 429، و`/api/...` الغلط مبيرجعش `index.html`. أحداث الـ JWT اليدوية اتشالت.
- `PasswordChangeRequiredFilter` بيستخدم `ApiErrorWriter`.
- **D23:** `PagedResult<T>` و`PageRequest` (`page`، `pageSize`، أقصى 100) و`ToPagedResultAsync`. لسه محدش بيستخدمهم؛ كل موديول ينقل قوايمه عليهم (D26). `PaginationParams` القديمة أخدت نفس الحدود.
- **D25:** `AppDbContext.OnConfiguring` اتشالت، فنسيان migration بيوقّف التشغيل.
- اتمسح `Domain/Exceptions/CustomExceptions.cs`.
- **D24:** `ex.Message` في الـ services متأجل لكل موديول لما الـ service تتكتب من جديد، فهدف «مفيش ex.Message للعميل» متحقق في الـ framework والـ middleware بس. والـ 26 بتوع الـ controllers كمان مع موديولاتهم (D26).

**اتأكدنا من:** build بـ 0 errors و42/42 test (27 جداد). طلبات حقيقية على الـ API (400، 401، 404، 405، 415، 429)، و403 و409 و500 على host تجريبي بنفس الـ middleware. `ToPagedResultAsync` والـ login الناجح متجربوش.

**إزاي تتأكد:**
1. من Swagger: `POST /api/auth/login` بـ `{}` → 400 بـ `code: Common.ValidationFailed` و`errors`.
2. `GET /api/Product` من غير token → 401 بـ `code: Common.Unauthorized`. جرّب `Accept-Language: en`.
3. افتح `/api/xyz` → 404 بنفس الشكل.

**ملاحظة:** GET على endpoint بتاع POST بيرجع 404 مش 405 (زي الأول)؛ باقي الـ methods بترجع 405.

---

### F2-S2 — نقل الـ controllers · اتلغت (D26) · 2026-11-25

**القرار (بلال):** مفيش نقل للـ 30 controller مرة واحدة. كل موديول يراجع الـ controllers بتاعته، وينقل بس الـ endpoints اللي الشاشات محتاجاها بعد ما تتكتب بالشكل الصحيح، والباقي يتمسح.

**التأثير:**
- **الداتا:** مفيش.
- **الكود:** الـ controllers القديمة تفضل بشكلها لحد موديولها. `PagedList` و`ApiResponse` و`PaginationParams` يتمسحوا مع آخر موديول بيستخدمهم.
- **الـ UI:** مفيش، لأن الفرونت الجديد بيتبني موديول موديول على الـ endpoints الجديدة.
- **الأمان:** الـ 26 `ex.Message` في الـ controllers والـ 73 في الـ services يفضلوا لحد موديولاتهم.
- **الخطة:** «تعريف الخلاص» لأي موديول في `PLAN.md` بقى فيه بند صريح بالنقل ده، علشان مفيش controller يتنسي. F3-S1 وFE-S1 بيعتمدوا على F2 (الهيكل) فمش متأثرين.

---

### F3-S1 — Audit وسجل التغييرات · خلص · 2026-11-26

**الـ business:** شاشة «سجل التغييرات» (78) والشريط الزمني و«آخر تعديل»: مين غيّر إيه وإمتى، قبل ← بعد، والسجل مبيتمسحش ولا يتعدل. ومحدش يقدر يعمل عملية باسم حد تاني (C7).

**القرارات (2026-11-26، كلها «a»):** D27 الأعمدة القديمة بتتملي بالاسم من الـ token؛ D28 سطر لكل تغيير + JSON قبل/بعد + أحداث من الـ service؛ D29 بالتعليم `[Audited]`، دلوقتي `Products` و`Supplier`؛ D30 الكود + trigger في الداتابيز.

**اللي اتعمل:**
1. **مين وإمتى من الـ token (D27)** — `Infrastructure/Data/Interceptors/AuditFieldsInterceptor.cs`: مع كل `SaveChanges` أي entity فيها عمود `CreateBy`/`UpdateBy`/`DeleteBy` (أو أسامي `IAuditedEntity` الجديدة) بيتملي من `ICurrentUserService`، والوقت من `TimeProvider` بـ UTC.
   - إضافة: `CreateBy` = المستخدم، و`UpdateBy`/`DeleteBy` بيتمسحوا (لو الفرونت بعتهم).
   - تعديل: `CreateBy`/`CreateAt` مبيتكتبوش تاني. `UpdateBy`/`UpdateAt` من الـ token.
   - `IsDeleted` من false لـ true: `DeleteBy`/`DeleteAt` بيتملوا. ومن true لـ false: بيتمسحوا. غير كده مبيتكتبوش.
   - برّه أي request (الـ seed أو job): القيمة اللي الكود حطها بتفضل زي `"System"`.
   - الـ entity اللي بتتعمل `State = Modified` من غير ما تتقري (detached): `EntityEntryOriginalValues.cs` بيقرا الصف من الداتابيز الأول علشان القيم القديمة تبقى صح.
   - العقد الجديد للـ entities الجديدة: `Domain/Common/IAuditedEntity.cs`.
2. **جدول `AuditEntries` (D28، D30)** — `Domain/Entities/Audit/AuditEntry.cs` + `AuditAction`، والإعدادات في `AppDbContext.ConfigureAuditEntries`: 5 `index` (الوقت، المستخدم، الشاشة، المستند، الـ reference)، و`FK` على المستخدمين من غير cascade. الـ migration هي `20261126161447_AddAuditEntries`، وفيها trigger `TR_AuditEntries_AppendOnly` بيرفض أي `UPDATE` أو `DELETE` حتى من SSMS (الخطأ 50030).
3. **الالتقاط (D28، D29)** — `AuditEntriesInterceptor.cs`: أي entity عليها `[Audited(AuditAreas.X, ReferenceProperty = ...)]` بتكتب سطر لكل إضافة، وتعديل (الحقول اللي اتغيرت بس، قبل وبعد)، و`soft delete`، ورجوع، ومسح. الأعمدة بتاعة مين وإمتى مش بتظهر في القايمة، وكمان أي حقل عليه `[NotAudited]`.
   - السطر فيه: `UserId` واسمه وقتها، والوقت، والـ IP، والـ `traceId`.
   - نفس الـ transaction: لو العملية رجعت (rollback) السطر بيرجع معاها، ولو الحفظ فشل مفيش سطر بيفضل معلّق لحفظ بعده. الصف الجديد بياخد الـ id بتاعه بعد الحفظ، فسطره بيتكتب في حفظ تاني جوه نفس الـ transaction، ولو مفيش transaction مفتوحة الـ interceptor بيفتح واحدة حوالين الاتنين.
   - التعديل أو المسح لسطر في السجل من EF بيترمي `InvalidOperationException`.
   - الأحداث اللي الـ diff مش بيوصفها («وقف حساب»، «تأكيد فاتورة»): `IAuditTrail.Record(new AuditEvent(...))` في `Application/Services.contract/AuditTrail/`، والتنفيذ في `Infrastructure/Services/AuditServices/AuditTrail.cs`. الحدث بيتكتب مع الحفظ الجاي بتاع الـ service.
   - التسجيل في `Program.cs`: `TimeProvider.System`، والـ interceptor بتاع الأعمدة الأول وبعده بتاع السجل، و`IAuditTrail`.
4. **اتمسح:** `ChangeLogService` و`IChangeLogService` و`IAuditableEntity`، والـ members بتوعهم في `IServiceManager` و`ServiceManager` (محدش كان بيستخدمهم).

**برّه الجلسة:** endpoint شاشة 78 (M2)؛ ترجمة أسامي الحقول والشاشات (مع كل موديول)؛ تعليم باقي الـ entities بـ `[Audited]` (كل موديول بتاعه)؛ `ApprovedBy` و`MakeTransactionUser` و`DateTime.Now` (كل موديول). `ExecuteUpdate`/`ExecuteDelete` مبيعدّوش على الـ interceptors، فممنوع نستخدمهم على entity عليها `[Audited]`.

**التجربة:** اتعملت على داتابيز مؤقتة `SadatSearmErp_AuditTest` بـ harness برّه الـ repo (14 + 4 + 19 حالة، كلها نجحت)، وبعدين الداتابيز اتمسحت. الـ build نضيف و42/42 test.

**Commits:** `09d4faf` (الأعمدة)، `f15202c` (الجدول والـ trigger)، `9b63d50` (السجل)، `2e1bb96` (المسح والـ README). بلال أكّد إن كله شغال (2026-11-26).

**إزاي تتأكد:**
1. شغّل الـ API، والـ migration `AddAuditEntries` هتتطبق لوحدها (D20).
2. سجّل دخول وعدّل منتج من Swagger بـ `PUT /api/Product` (غيّر `sellingPrice`، وحط في `createBy` و`deleteBy` أي كلام).
3. في SSMS شغّل ده:
   ```sql
   SELECT TOP 10 OccurredAt, UserName, Area, Action, EntityId, Reference, Changes
   FROM SadatSearmErp.dbo.AuditEntries ORDER BY Id DESC;
   SELECT Id, CreateBy, UpdateBy, UpdateAt, DeleteBy FROM SadatSearmErp.dbo.Products WHERE Id = <id>;
   ```
   المفروض تلاقي سطر `Action = 2` وفيه `[{"field":"SellingPrice","old":…,"new":…}]` باسمك، و`UpdateBy` هو الـ id بتاعك، و`CreateBy` زي ما هو، والكلام اللي بعته متكتبش.
4. جرّب `DELETE FROM SadatSearmErp.dbo.AuditEntries;` — المفروض يترفض بالرسالة `AuditEntries is append-only`.

---

### F4F5-S1 — ترقيم المستندات والتعارض · خلص · 2026-11-26

**الـ business:** كل مستند بياخد رقم الناس بتتكلم بيه (`INV-1038`، `PUR-0224`، `RC-0368`، `RET-0044`، `TR-0094`، `CNT-0013`، `INS-0412`، `W-2231`، `JE-4471`) من غير تكرار، حتى لو اتنين أكّدوا في نفس الثانية. ولو اتنين عدّلوا نفس رصيد المخزن في نفس اللحظة، التاني بيترفض بدل ما يكتب فوق الأول.

**القرارات (2026-11-26، كلها «a»):** D31 ترقيم من غير فراغات جوه transaction المستند؛ D32 عداد واحد مستمر من غير تصفير سنوي؛ D33 `rowversion` على `Stock` و`SalesInvoices` و`PurchaseInvoice`، والمحافظ مع جداولها في M5 وM11؛ D34 الترقيم من 1 بـ 4 خانات على الأقل.

**اللي اتعمل:**
1. **الترقيم (F4):**
   - جداول وأنواع: `Domain/Enums/DocumentType.cs` فيه الـ 9 أنواع (`tinyint`، ومحدش يغيّر قيمة موجودة). و`Domain/Entities/Numbering/DocumentSequence.cs` فيه النوع كـ key، والبادئة، وآخر رقم، وأقل عدد خانات، والصفوف الافتراضية في `Defaults`.
   - الإعداد في `AppDbContext.ConfigureDocumentSequences` بـ `HasData`، والـ migration هي `20261126170225_AddDocumentSequences` وفيها 9 صفوف كلها من صفر.
   - الخدمة: `Application/Services.contract/Numbering/IDocumentNumberService.cs` و`Infrastructure/Services/NumberingServices/DocumentNumberService.cs`، ومتسجلة في `Program.cs`. بتعمل `UPDATE … OUTPUT` واحد: SQL Server بيقفل الصف لحد ما الـ transaction بتاعة الـ service تخلص، فالطلبات اللي في نفس اللحظة بتقف في طابور، وكل واحد بياخد الرقم اللي بعده. ولو العملية رجعت (rollback) الرقم بيرجع. ولو اتندهت من غير transaction بترمي `InvalidOperationException`.
2. **التعارض (F5):**
   - `[Timestamp] RowVersion` على `Stock` و`SalesInvoices` و`PurchaseInvoice`، والـ migration هي `20261126170501_AddRowVersions`.
   - `ErrorCodes.ConcurrencyConflict` مع مفتاح `Common.ConcurrencyConflict` بالعربي («حد تاني عدّل البيانات دي قبلك. حدّث الصفحة وحاول تاني») والإنجليزي.
   - `ExceptionHandlingMiddleware.ToCode`: أي `DbUpdateConcurrencyException` بيرجع 409 بالكود ده.
   - كل الأماكن القديمة اللي بتعدّل `Stock` والفواتير بتقرا الصف الأول، فمفيش service اتلمست.

**التجربة:** اتعملت على داتابيز مؤقتة `SadatSearmErp_SeqTest` بـ harness برّه الـ repo (7 + 5 حالات، كلها نجحت)، وبعدين اتمسحت:
- 50 طلب في نفس اللحظة طلعوا `INV-0001` لحد `INV-0050` بالظبط.
- الـ rollback رجّع الرقم.
- الأنواع مستقلة عن بعض، وبعد `9999` الرقم بيبقى `PUR-10000`.
- من غير transaction بيترمي خطأ.
- سحبتين من نسخة قديمة لنفس الـ `Stock`: التانية اترفضت والرصيد فضل 70 مش 80.
- طريقة الكود القديم (`State = Modified` بعد القراية) لسه شغالة.

الـ build نضيف و42/42 test. الـ 409 بالكود الجديد متجرّبش من HTTP، لأن مفيش endpoint النهارده بيسيب الـ exception يطلع (الـ services القديمة بتمسكه، D24)، بس هو نفس الـ mapping اللي اتجرّب في F2-S1.

**برّه الجلسة (في الـ Known issues):**
- `SO{Id}` و`PO{Id}` والرقم العشوائي للقيود بيتشالوا لما الـ services تتكتب من جديد (M6، M7، F6/M3).
- الـ `unique index` على رقم الفاتورة.
- قرار «الرقم مع المسودة ولا مع التأكيد» (M6/M7).

**Commits:** `4f36242` (الترقيم)، `7c85055` (التعارض والـ README). بلال أكّد إن كله شغال (2026-11-26).

**إزاي تتأكد:**
1. شغّل الـ API، والـ migrations هتتطبق لوحدها.
2. في SSMS شغّل ده:
   ```sql
   SELECT * FROM SadatSearmErp.dbo.DocumentSequences;   -- 9 صفوف، LastNumber = 0
   SELECT TOP 1 RowVersion FROM SadatSearmErp.dbo.Stocks; -- عمود rowversion موجود
   ```
3. جرّب الترقيم:
   ```sql
   BEGIN TRAN;
   UPDATE SadatSearmErp.dbo.DocumentSequences SET LastNumber = LastNumber + 1
   OUTPUT inserted.Prefix, inserted.LastNumber WHERE DocumentType = 1;
   ROLLBACK;
   ```
   المفروض يطلع `INV` و`1`، وبعد الـ `ROLLBACK` الـ `LastNumber` يرجع `0`.

### F6-S1 — محرك الترحيل المحاسبي · خلص · 2026-11-27

**الـ business:** أي عملية فيها فلوس (تأكيد بيع، استلام شراء، سند، مصروف، رواتب، جرد) هتكتب قيدها من مكان واحد بس: `IJournalPostingService`.

مثال: تأكيد `INV-0040` بيبعت للمحرك:
- مدين العميل.
- دائن المبيعات.
- مدين تكلفة البضاعة.
- دائن المخزون.
- المصدر: فاتورة بيع 40.

فيطلع `JE-0001` مرحّل جوه transaction التأكيد. ولو الفاتورة اتلغت، `ReverseAsync` بيعمل `JE-0002` بالعكس ومربوط بالأصلي، ومينفعش يتعكس مرتين. والقيد المرحّل مبيتعدلش ولا يتمسح من أي مكان في الكود.

**الوضع قبلها:**
- كل service بتبني قيدها بإيدها بأكواد نصية زي `"1.1.3"` و`"4.1"` و`"5.2"`.
- القيد مكانش فيه رقم، ولا مصدر صريح، ولا علامة إنه نظامي، ولا ربط بالقيد العكسي.
- العكسي بتاع البيع ممكن يتعمل مرتين (C6)، وحذف فاتورة الشراء كان بيمسح قيدها المرحّل (C10).

**القرارات (2026-11-27، كلها «a»):**
- **D35:** الـ services القديمة فضلت زي ما هي، وكل موديول هيحوّلها للمحرك. لحد كده `EntryNumber` و`SourceType` nullable.
- **D36:** حارس في `SaveChanges` على القيد المرحّل.
- **D37:** القيد العكسي بياخد تاريخ العكس نفسه.
- **D38:** unit tests للقواعد، وharness على DB مؤقتة.

**اللي اتعمل:**
1. **الجدول** (commit `159ffeb`):
   - `Domain/Enums/JournalSourceType.cs` فيه 13 مصدر بنوع `tinyint`: يدوي، وفاتورة بيع، ومرتجع بيع، وفاتورة شراء، ومرتجع شراء، وسند قبض، وسند صرف، ومصروف، ورواتب، وقرض، وعهدة، وجرد، وهدايا.
   - `Domain/Entities/Finance/JournalEntries.cs` اتضاف له:
     - `EntryNumber`، وعليه unique.
     - `SourceType` و`SourceId`، وعليهم index.
     - `IsSystemGenerated`.
     - `ReversalOfEntryId`: FK على نفس الجدول، وعليه unique filtered index، فالقيد ميتعكسش مرتين حتى لو طلبين جم في نفس اللحظة.
     - `IAuditedEntity`، فبيتسجل مين سجّل القيد وإمتى.
     - `[Audited(Accounting)]`، ومرجعه رقم القيد.
     - `Desc` بقى طوله لحد 500 حرف.
   - `JournalEntryDetails.Description`: بيان السطر، لحد 250 حرف.
   - الإعدادات في `AppDbContext.ConfigureJournalPosting`، والـ migration هي `20261127122501_AddJournalPosting`.
2. **القواعد** (commit `0a786ee`):
   - الـ DTOs في `Application/DTOs/FinanceDtos/Posting/JournalDraft.cs`، وفيها `JournalDraft` و`JournalDraftLine` (`DebitTo` و`CreditTo` بـ `SystemAccountCode` أو بـ id) و`PostedJournalEntry`.
   - `Application/Common/JournalDraftRules.cs` فيه القواعد دي:
     - سطرين على الأقل.
     - كل سطر على حساب واحد، وجنب واحد بس، والمبلغ أكبر من صفر.
     - المبلغ رقمين عشريين بالكتير.
     - المدين يساوي الدائن.
     - التاريخ والبيان موجودين.
     - القيد النظامي لازم يكون له `SourceId`، واليدوي ملوش.
     - `Reversed` بيقلب السطور.
   - 11 test جديد في `Tests/Application.UnitTests/Common/JournalDraftRulesTests.cs`.
3. **المحرك** (commit `ec8a25b`):
   - الـ interface في `Application/Services.contract/Finance/IJournalPostingService.cs`، والتنفيذ في `Infrastructure/Services/FinanceService/JournalPostingService.cs`، ومتسجل في `Program.cs`.
   - **`PostAsync`:**
     - بيتأكد من القواعد.
     - بيرفض الشغل لو مفيش transaction.
     - بيجيب كل الحسابات في query واحدة.
     - الحساب النظامي الناقص بيرمي exception.
     - الحساب المقفول أو الأب بيرجع `Journal.AccountNotPostable`.
     - بياخد رقم `JE-` ويحفظ الرأس والسطور في save واحد، والقيد بيبقى مرحّل.
   - **`ReverseAsync`:**
     - القيد اللي مش موجود بيرجع `Journal.EntryNotFound` (404).
     - القيد اللي مش مرحّل بيرجع `Journal.NotPosted` (409).
     - القيد العكسي بيرجع `Journal.CannotReverseReversal` (409).
     - القيد اللي اتعكس قبل كده بيرجع `Journal.AlreadyReversed` (409).
     - التاريخ اللي قبل الأصل بيرمي exception.
     - العكس بيشتغل حتى لو حساب العميل اتقفل بعد البيع.
     - القيد القديم اللي ملوش مصدر بيتعكس كقيد يدوي مربوط بالأصلي.
   - المفاتيح في `Domain/Common/JournalErrorCodes.cs`، والنص بالعربي والإنجليزي في `LocalizationResources.cs`.
4. **الحارس** (commit `ab0e147`):
   - `Infrastructure/Data/Interceptors/PostedJournalGuardInterceptor.cs`، ومتسجل أول واحد قبل الـ audit.
   - أي تعديل أو حذف لقيد مرحّل أو لسطر من سطوره بيرمي exception، فالطلب بيرجع 500. حالة «مرحّل» بتتقري من الـ DB، فالـ attach و`State = Modified` والحذف بالـ id بيتمسكوا كلهم.
   - المسودة بتتعدّل وتترحّل عادي، وبعد الترحيل بتتقفل.

**التجربة:** اتعملت على DB مؤقتة `SadatSearmErp_JournalTest` بـ harness برّه الـ repo. الـ 32 حالة نجحت، وبعدين الـ DB اتمسحت. الحالات شملت:
- الترحيل والرقم.
- الأخطاء: من غير transaction، وقيد مش متوازن، وحساب نظامي ناقص، وحساب مقفول أو أب.
- الـ rollback بيرجّع الرقم.
- العكس وكل رفضه.
- طلبين عكس في نفس اللحظة: واحد بس اتسجل.
- الحارس في 4 طرق تعديل وحذف.
- المسودة بتتعدّل وتترحّل.
- الطريقة القديمة (الرأس الأول وبعده السطور) لسه شغالة، والقيد القديم بيتعكس.

الـ build نضيف، و53/53 test.

**برّه الجلسة:**
- حسابات النظام الجديدة (الضريبة، والخزنة، والقروض، والرواتب المستحقة…) في M3.
- شاشة القيد اليدوي وكشف الحساب، و`IsPosted` → `Status`، في M3.
- تحويل البيع والشراء والرواتب والقروض للمحرك في M6 وM7 وM9، وبعدها `EntryNumber` و`SourceType` يبقوا required.
- الحارس لسه مبيمنعش **إضافة** سطر لقيد مرحّل، لأن الـ services القديمة بتحفظ الرأس الأول وبعده السطور. ده بيتقفل في M3.
- حذف فاتورة شراء عليها قيد مرحّل بقى بيرجع 500 بدل ما يمسح القيد، لحد ما M6 يحوّله لإلغاء.

**إزاي تتأكد:**
1. شغّل الـ API، والـ migration هتتطبق لوحدها.
2. في SSMS:
   ```sql
   SELECT TOP 5 Id, EntryNumber, SourceType, SourceId, IsSystemGenerated, ReversalOfEntryId, CreatedAt
   FROM SadatSearmErp.dbo.journalEntries ORDER BY Id DESC;   -- الأعمدة الجديدة موجودة
   SELECT name, is_unique, filter_definition FROM SadatSearmErp.sys.indexes
   WHERE object_id = OBJECT_ID('SadatSearmErp.dbo.journalEntries');  -- EntryNumber و ReversalOfEntryId unique
   ```
3. من Swagger اعمل قيد يدوي ورحّله (`JournalEntry` → add ثم post)، وبعدين جرّب تعدّله بـ update: هيرجع رفض زي الأول. ولو في أي مكان تاني عدّلت قيد مرحّل، الطلب هيرجع 500 والـ log هيقول `D36`.
4. شغّل `dotnet test`: لازم يطلع 53/53.

**التأكيد:** بلال أكّد يوم 2026-11-27 إن الـ tests نجحت عنده (53/53). والتجربة من الشاشة (الترحيل والعكس) اتأجلت لـ M3، لما شاشة القيد اليدوي تبقى جاهزة. وفي الـ session دي كمان اتصلّح `tools/tracker.py`: الـ `--commit` بقى بيقبل hashes مفصولة بفاصلة، ومبيكررش حاجة موجودة.

---
### F7-S1 — محرك الموافقات · مستني تأكيدك · 2026-11-27

**الـ business:** شاشة «مركز الموافقات» (شاشة 9) فيها كل حاجة مستنية قرار في مكان واحد، والأقدم بيظهر الأول. أمثلة:
- طلب إلغاء `INV-1038`، ومعاه اللي هيحصل لو اتوافق عليه.
- تحويل نقاط، والموافقة بتبقى مقفولة لو رصيد التاجر مش كفاية.
- تأكيد خزنة، أو إجازة، أو قرض، أو طلب صلاحية.

الرفض لازم يبقى معاه سبب، والسبب بيوصل لصاحب الطلب. والموافقة الجماعية بتظهر لما تفتح تاب نوع واحد. والزرار «مستني موافقتك» في التوب بار بيعرض العدد.

**الوضع قبلها:**
- كل موديول ليه حالة بشكله: `DeleteStatus` في `BaseInvoice`، و`Status`/`ApprovedBy`/`RejectionReason` في `EmployeeLeaveRequest`.
- مفيش مكان واحد للموافقات.
- مفيش حاجة بتمنع موافقتين في نفس اللحظة، ولا طلبين لنفس الحاجة.

**القرارات (2026-11-27):**
- **D39 (أ):** الـ handler بيبني الكارت وقت القراية، دفعة واحدة لكل نوع.
- **D40:** مينفعش حد يوافق على طلبه أو يرفضه، إلا `super_admin`.
- **D41 (أ):** الموافقة الجماعية بتعمل transaction لكل طلب، وبترجع نتيجة كل واحد.
- **D42 (أ):** المحرك بس، وبيتجرّب بـ handler تجريبي. والأنواع الحقيقية بتيجي مع موديولاتها.

**اللي اتعمل:**
1. **الجدول** (commit `0a669af`):
   - `Domain/Enums/ApprovalType.cs`، وفيه:
     - `ApprovalType`: إلغاء بيع، وإلغاء شراء، وتحويل نقاط، وتأكيد خزنة، وإجازة، وقرض، وطلب صلاحية، ونسيت كلمة السر. كل نوع ليه رقم ثابت.
     - `ApprovalGroup`: التابات الخمسة.
     - `ApprovalStatus`: مستني، واتوافق، واترفض، واتسحب.
     - `ApprovalTypes.GroupOf`: بيحدد كل نوع في أنهي تاب.
   - `Domain/Entities/Approvals/ApprovalRequest.cs`، وفيه:
     - النوع، و`SubjectId`، و`Payload` (لحد 2000 حرف)، والسبب (إجباري ولحد 500 حرف)، والحالة.
     - مين قرر وإمتى وسببه.
     - `RowVersion`.
     - مين طلب وإمتى من `IAuditedEntity`، يعني من الـ token.
     - `[Audited(Approvals)]`.
   - الإعدادات في `AppDbContext.ConfigureApprovals`:
     - index على (الحالة، النوع، وقت الطلب) للقايمة.
     - index على (مين طلب، الحالة).
     - **unique filtered** على (النوع، `SubjectId`) للطلبات المستنية بس.
   - الـ migration: `20261127181225_AddApprovalRequests`، وبتعمل جدول جديد بس.
2. **العقد والقواعد** (commit `ca6440b`):
   - `Application/Services.contract/Approvals/IApprovalHandler.cs`: كل موديول بيسجّل handler لكل نوع عنده. فيه:
     - `Type`.
     - `DecidePermission`.
     - `ValidateSubmitAsync`.
     - `DescribeAsync`، ودي بتاخد دفعة طلبات مرة واحدة.
     - `ApproveAsync`، ودي بتشتغل جوه transaction المحرك.
     - `ReleaseAsync`، ودي بتشتغل لما الطلب يترفض أو يتسحب.
   - `IApprovalService.cs`.
   - الـ DTOs في `Application/DTOs/Approvals/ApprovalDtos.cs`.
   - `Application/Common/ApprovalRules.cs`:
     - مين يقدر يقرر (D40).
     - الطلب بيتأخر بعد 24 ساعة.
     - السبب.
     - أقصى عدد في الموافقة الجماعية 100.
   - 11 test في `Tests/Application.UnitTests/Common/ApprovalRulesTests.cs`.
3. **الـ service** (commit `e21ca43`):
   - `Infrastructure/Services/ApprovalServices/ApprovalService.cs`، ومتسجل في `Program.cs`.
   - **الطلب** (`SubmitAsync`):
     - الموديول بينده عليه من الـ endpoint بتاعه.
     - السبب الفاضي بيرجع `Approval.ReasonRequired`.
     - الطلب التاني لنفس الحاجة بيرجع 409 `Approval.AlreadyPending`، حتى لو الاتنين جم في نفس اللحظة، لأن الـ index بيمسكها.
     - النوع اللي ملوش handler بيرمي exception.
   - **القايمة والأرقام:**
     - بتظهر بس الأنواع اللي عندك صلاحيتها، والـ `super_admin` بيشوف كله.
     - الطلبات بتاعتك مش بتظهر عندك، إلا لو انت `super_admin`.
     - الأقدم الأول.
     - كل نوع بياخد query واحدة للكروت.
   - **القرار:** transaction واحدة لكل طلب:
     - الطلب بيتحجز الأول، بحالته الجديدة والـ `RowVersion`.
     - بعدين الـ handler بيشتغل.
     - بعدين الاتنين بيتحفظوا مع بعض.
     - لو الـ handler فشل، كله بيرجع والطلب بيفضل مستني.
     - لو اتنين قرروا في نفس اللحظة، التاني بياخد 409 `Approval.AlreadyDecided`.
   - **الرفض** لازم يبقى معاه سبب. و**السحب** لصاحب الطلب بس.
   - **الموافقة الجماعية:** كل طلب لوحده، والرد فيه نتيجة كل واحد ورسالته.
   - المفاتيح في `Domain/Common/ApprovalErrorCodes.cs`، والنص بالعربي والإنجليزي في `LocalizationResources.cs`.
4. **الـ endpoints** (commit `9914418`):
   - `AlSadat-Seram.Api/Controllers/ApprovalsController.cs`، على `BaseApiController.HandleResult`.
   - عليه `[Authorize]` بس، لأن صلاحية كل نوع بتتشيك جوه الـ service.

**التجربة:** اتعملت على DB مؤقتة `SadatSearmErp_ApprovalTest` بـ harness برّه الـ repo. استعملت handler تجريبي لنوعين و5 يوزرز: اللي بيطلب، ومدير، ويوزر من غير صلاحية، ويوزر عنده صلاحية القروض بس، و`super_admin`. الـ 40 حالة نجحت، وبعدين الـ DB اتمسحت. الحالات شملت:
- الطلب، والطلب المكرر، وطلبين في نفس اللحظة.
- القايمة والفلترة والـ paging والأرقام لكل يوزر.
- الموافقة، والموافقة على طلبك، والموافقة اللي الـ handler رفضها وإن مفيش حاجة اتحفظت، وموافقتين في نفس اللحظة والـ handler اشتغل مرة واحدة.
- الرفض والسحب والتفاصيل.
- الموافقة الجماعية بخمس حالات.
- الـ index نفسه.

الـ build نضيف (Release)، و64/64 test.

**برّه الجلسة:**
- إشعار الرفض في F8.
- الأنواع الحقيقية مع موديولاتها: M1 (طلب صلاحية، ونسيت كلمة السر)، وM5 (النقاط)، وM6 (إلغاء البيع)، وM7 (إلغاء الشراء)، وM9 (الإجازات والقروض)، وM11 (الخزنة).
- `DeleteStatus` القديم بيتشال في M6/M7.
- صلاحية كل نوع بتتقري من الـ JWT لحد ما M1 يحوّلها للـ cache (D3).
- ضم العداد لـ `/me/badges` في M1.

**إزاي تتأكد:**
1. اقفل الـ API وشغّله تاني، والـ migration `AddApprovalRequests` هتتطبق لوحدها.
2. من Swagger، بعد الـ login:
   - `GET /api/approvals/summary` لازم يرجع `{ "total": 0, "overdue": 0, "byType": [] }`.
   - `GET /api/approvals` لازم يرجع قايمة فاضية بشكل `PagedResult`.
   - مفيش أنواع حقيقية لسه، وده حسب D42.
3. جرّب `POST /api/approvals/1/approve`، والمفروض يرجع 404 ومعاه `Approval.NotFound`.
4. جرّب `POST /api/approvals/1/reject` بـ body فاضي `{}`، والمفروض يرجع 400 من الـ validation.
5. في SSMS:
   ```sql
   SELECT name, is_unique, filter_definition FROM SadatSearmErp.sys.indexes
   WHERE object_id = OBJECT_ID('SadatSearmErp.dbo.ApprovalRequests');
   ```
   لازم تلاقي `IX_ApprovalRequests_Type_SubjectId` وهو unique ومعاه `([Status]=(1) AND [SubjectId] IS NOT NULL)`.
6. شغّل `dotnet test`، ولازم يطلع 64/64.
