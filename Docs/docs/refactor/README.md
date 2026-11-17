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

### F0-S1 — الشغل المعلّق + C1 + C2 · جاري (الكود خلص، مستني build وتجربة)

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

### F0-S2 — C3 + C15 + C16 + Google · جاري (الكود خلص، مستني build وتجربة)

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

**إزاي تتأكد:**
1. `dotnet build` يعدّي من غير errors (أهم حاجة: مفيش حاجة كانت معتمدة على Google)، وفولدر `publish/` اختفى بعد الـ pull.
2. C3: سجّل دخول بأدمن أو محاسب، وابعت `PUT /api/DistAndMerch/edit/{userId}` بنفس بيانات العميل ومعاها `"PointsBalance": 999999, "cashBalance": 999999, "indebtedness": 999999`. الرد نجاح، و`GET /api/DistAndMerch/get/{userId}` يرجّع الأرصدة زي ما كانت.
