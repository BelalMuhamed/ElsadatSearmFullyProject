# Payroll Component Documentation

## نظرة عامة

تم تطوير مكون **Payroll Component** لإدارة كشوف الرواتب والمرتبات مع تكامل كامل مع نظام المحاسبة.

## الملفات المُنشأة

### 1. Models (IPayroll.ts)
- **المسار**: `src/app/models/IPayroll.ts`
- **الوصف**: يحتوي على جميع DTOs (Data Transfer Objects) التي تعكس بيانات الـ Backend

#### DTOs المتضمنة:
- `GeneratePayrollRequestDto` - طلب إنشاء كشف راتب فردي
- `GenerateBulkPayrollRequestDto` - طلب إنشاء كشوف رواتب جماعي
- `PayrollResponseDto` - استجابة بيانات الراتب
- `BulkPayrollResultDto` - نتائج العملية الجماعية
- `PayrollFilterDto` - معايير التصفية
- `PayrollSummaryDto` - ملخص الرواتب
- `MarkPayrollPaidDto` - تحديث حالة الدفع

### 2. Service (payroll.service.ts)
- **المسار**: `src/app/Services/payroll.service.ts`
- **الوصف**: طبقة الخدمة التي تتعامل مع جميع عمليات HTTP

#### Methods المتاحة:
- `generatePayroll()` - إنشاء كشف راتب فردي
- `generateBulkPayroll()` - إنشاء رواتب جماعي
- `postToAccounting()` - تسجيل في المحاسبة (فردي)
- `postBulkToAccounting()` - تسجيل جماعي في المحاسبة
- `markAsPaid()` - تعيين كمدفوع (فردي)
- `markBulkAsPaid()` - تعيين جماعي كمدفوع
- `getPayrolls()` - جلب كشوف مع فلترة
- `exportToExcel()` - تصدير إلى Excel
- `getSummary()` - جلب ملخص الرواتب
- `getPayrollById()` - جلب كشف محدد
- `getEmployeePayrolls()` - جلب كشوف الموظف

### 3. Component (payroll-component)
- **المسار**: `src/Components/payroll-component/`
- **الملفات**: 
  - `payroll-component.ts` - منطق المكون
  - `payroll-component.html` - القالب
  - `payroll-component.css` - الأنماط

#### Features:
- 5 تبويبات رئيسية للعمليات المختلفة
- إنشاء كشوف راتب فردية وجماعية
- فلترة وبحث متقدم
- تسجيل محاسبي (فردي وجماعي)
- تحديث حالة الدفع
- تصدير إلى Excel
- ملخص شامل للرواتب
- دعم الأنماط الفاتحة والداكنة

## التبويبات الرئيسية

### Tab 1: إنشاء كشف راتب فردي
- إدخال كود الموظف
- اختيار الشهر والسنة
- إنشاء الكشف مباشرة

### Tab 2: إنشاء رواتب جماعي
- اختيار الشهر والسنة
- اختيار طريقة الدفع
- خيارات للموظفين النشطين فقط
- تسجيل تلقائي في المحاسبة (اختياري)
- عرض تفاصيل العملية

### Tab 3: قائمة الرواتب
- فلترة متقدمة (شهر، سنة، حالة، الحد الأدنى/الأقصى للراتب)
- عرض قائمة مع الترقيم
- اختيار متعدد للعمليات الجماعية
- عمليات فردية (تسجيل محاسبي، دفع)
- تصدير إلى Excel

### Tab 4: الدفع والمحاسبة
- عرض إحصائيات الحالات
- شرح خطوات عملية الدفع
- معلومات عن الكشوف المعلقة

### Tab 5: تفاصيل الكشف
- عرض جميع بيانات الكشف المختار
- التفاصيل الكاملة للراتب

## حالات الكشف (Status)

1. **Generated** - تم الإنشاء
   - الحالة الأولية بعد إنشاء الكشف
   - يمكن تسجيله محاسبياً

2. **Posted** - مسجل محاسبياً
   - تم تسجيله في شجرة الحسابات
   - يمكن تعيينه كمدفوع

3. **Paid** - مدفوع
   - تم دفع الراتب
   - الحالة النهائية

## الترجمة والتحديثات

- جميع النصوص والواجهات باللغة العربية
- دعم الاتجاه من اليمين لليسار (RTL)
- صيغ الأرقام والتواريخ بالصيغة العربية

## التكامل مع النظام

### الوصول إلى الصفحة:
```
http://localhost:4200/hr/payroll
```

### الصلاحيات المطلوبة:
- `HR` - إدارة الموارد البشرية
- `PayrollManager` - مدير الرواتب
- `Accountant` - المحاسب
- `Admin` - المسؤول

## المميزات المتقدمة

### 1. العمليات الجماعية
- إنشاء رواتب لعدد كبير من الموظفين دفعة واحدة
- تسجيل جماعي في المحاسبة
- دفع جماعي

### 2. التقارير والتصدير
- تصدير إلى Excel مع التنسيق
- ملخصات مفصلة
- إحصائيات شاملة

### 3. التسجيل المحاسبي التلقائي
- اختيار تسجيل تلقائي عند الإنشاء الجماعي
- ربط تلقائي مع شجرة الحسابات

### 4. الفلترة المتقدمة
- فلترة حسب الشهر والسنة
- فلترة حسب الحالة
- فلترة حسب نطاق الراتب
- فلترة حسب القسم

## تطبيق الأسلوب

يتبع هذا المكون نفس أسلوب وتصميم `EmployeeSalaryComponent`:

- ✅ نفس نمط التبويبات (tabs)
- ✅ نفس تنسيق البطاقات والشبكات
- ✅ نفس نظام الألوان والذهب (gold theme)
- ✅ نفس الدعم للأنماط الفاتحة والداكنة
- ✅ نفس التصميم المستجيب (responsive design)
- ✅ نفس تنسيق الجداول والقوائم
- ✅ نفس المرافق التنسيقية (formatting utilities)

## نقاط الاتصال بـ Backend

| الدالة | المسار | الطريقة |
|-------|-------|--------|
| generatePayroll | Payroll/GeneratePayroll | POST |
| generateBulkPayroll | Payroll/GenerateBulkPayroll | POST |
| postToAccounting | Payroll/PostToAccounting | POST |
| postBulkToAccounting | Payroll/PostBulkToAccounting | POST |
| markAsPaid | Payroll/MarkAsPaid | PUT |
| markBulkAsPaid | Payroll/MarkBulkAsPaid | PUT |
| getPayrolls | Payroll/GetPayrolls | GET |
| exportToExcel | Payroll/ExportToExcel | GET |
| getSummary | Payroll/GetSummary | GET |
| getPayrollById | Payroll/GetPayrollById | GET |
| getEmployeePayrolls | Payroll/GetEmployeePayrolls | GET |

## متطلبات Material

تم استخدام مكتبة Material الموجودة:
- MatTabsModule
- MatCardModule
- MatFormFieldModule
- MatSelectModule
- MatButtonModule
- MatIconModule
- MatTableModule
- MatCheckboxModule
- MatSlideToggleModule
- MatExpansionModule

## الملاحظات المهمة

1. **الفحوصات الأمنية**: جميع العمليات تتطلب صلاحيات محددة من الـ Backend
2. **معالجة الأخطاء**: جميع العمليات تتعامل مع الأخطاء باستخدام SweetAlert2
3. **الحالات الفارغة**: يتم عرض رسالة عندما لا تكون هناك بيانات
4. **الترقيم**: يتم دعم الترقيم للقوائم الكبيرة
5. **التوافقية**: يعمل على جميع الأجهزة والشاشات

## التطوير المستقبلي

- [ ] إضافة رسوم بيانية للإحصائيات
- [ ] إضافة تقارير متقدمة
- [ ] دعم الجداول المتقدمة (sorting, filtering on table)
- [ ] إضافة التنبيهات التلقائية
- [ ] دعم الدفع المتعدد

---

**آخر تحديث**: يناير 2026
**الإصدار**: 1.0
**الحالة**: جاهز للإنتاج ✓
