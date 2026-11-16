# Sadat Searm — Design Kit

ده كل شغل إعادة التصميم في فولدر واحد، عشان تكمّل في Claude Code من الـ terminal من غير ما تحتاج المحادثة.

## جوه إيه
| المسار | فيه إيه |
|---|---|
| `CLAUDE.md` | الـ context اللي Claude Code بيقراه لوحده: القواعد والقرارات وترتيب القراية |
| `docs/sadat-searm-ui-redesign.md` | ملف التسليم: الـ design system، الشاشات، قواعد البيزنس، وخطة التنفيذ في Angular |
| `docs/chat-history.md` | سجل المحادثة (الكلام بس، من غير الكود) |
| `docs/inputs/` | الـ customer stories PDF وتحليل الباك إند |
| `design/preview/index.html` | افتحه في المتصفح تشوف الـ ٨٠ شاشة، وبيشتغل من غير نت |
| `design/screenshots/` | صورة PNG لكل شاشة، وClaude Code يقدر يشوفها |
| `design/source/` | ملفات الـ canvas الأصلية (`.dc.html` + `canvas.json`) |
| `design/sr.css` | الـ tokens والـ classes المشتركة |

## تشغّله إزاي مع Claude Code
1. فك الـ zip **جوه الريبو**، مثلاً `ElsadatSearmFullyProject/design-kit/`.
2. لو عايز Claude Code يقرا الـ context أول ما يفتح من غير ما تقوله، انقل `CLAUDE.md` لجذر الريبو وغيّر المسارات اللي فيه لتبدأ بـ `design-kit/`. أو سيبه مكانه وقوله في أول رسالة: «اقرا design-kit/CLAUDE.md».
3. من الـ terminal:
   ```
   cd ElsadatSearmFullyProject
   claude
   ```
   وأول طلب ممكن يبقى: «اقرا design-kit/CLAUDE.md وابدأ بالخطوة ١ من خطة التنفيذ: tokens + shell».

## لما الـ limit يخلص
كل حاجة هنا ملفات على جهازك. أي session جديدة، سواء Claude Code أو claude.ai، اديها `CLAUDE.md` وهتكمّل من نفس النقطة. والـ canvas الأصلي فاضل موجود على claude.ai لو حبيت تعدّل في التصميم نفسه:
https://claude.ai/artifact/Lk3VW1qXKeBe3x75JW4tZ6
