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
