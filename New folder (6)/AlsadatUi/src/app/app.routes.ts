import { UnauthorizedComponent } from './../Components/unauthorized/unauthorized.component';
import { NotFoundComponent } from './../Components/not-found/not-found.component';
import { AddEditJournalEntryComponent } from './../Components/add-edit-journal-entry/add-edit-journal-entry.component';
import { JournalEntriesComponent } from './../Components/journal-entries/journal-entries.component';
import { AddEditAccountInTreeComponent } from './../Components/add-edit-account-in-tree/add-edit-account-in-tree.component';
import { PurchaseInvoiceDetailsComponent } from './../Components/purchase-invoice-details/purchase-invoice-details.component';
import { SalesInvoiceDetailsComponent } from './../Components/sales-invoice-details/sales-invoice-details.component';
import { SalesInvoiceDetails } from './models/IsalesInvoice';
import { InvoiceConfirmationComponent } from './../Components/invoice-confirmation/invoice-confirmation.component';
import { AddEditSalesInvoiceComponent } from './../Components/add-edit-sales-invoice/add-edit-sales-invoice.component';
import { PurchaseInvoicesComponent } from './../Components/purchase-invoices/purchase-invoices.component';
import { AddEditSupplierComponent } from './../Components/add-edit-supplier/add-edit-supplier.component';
import { Routes } from '@angular/router';
import { AuthLayout } from '../Layouts/auth-layout/auth-layout';
import { SideBarComponent } from '../Layouts/side-bar-component/side-bar-component';
import { SalesInvoicesComponent } from '../Components/sales-invoices-component/sales-invoices-component';
import { authGuard } from '../Guards/auth-gard-guard';
import { CopounComponent } from '../Components/copoun-component/copoun-component';
import { BillDiscountComponent } from '../Components/bill-discount-component/bill-discount-component';
import { ProductComponent } from '../Components/product/product.component';
import { GovernrateComponent } from '../Components/governrate/governrate.component';
import { CityComponent } from '../Components/city/city.component';
import { DisAndMerchantComponent } from '../Components/dis-and-merchant/dis-and-merchant.component';
import { HrAttendanceComponent } from '../Components/hr-attendance-component/hr-attendance-component';
import { HrAttendanceRecordComponent } from '../Components/hr-attendance-record-component/hr-attendance-record-component';
import { RepresentativeAttendanceComponent } from '../Components/representative-attendance-component/representative-attendance-component';
import { TransactionsComponent } from '../Components/transactions/transactions.component';
import { StoresComponent } from '../Components/stores/stores.component';
import { EmployeesListComponent } from '../Components/employees-list-component/employees-list-component';
import { RolesComponent } from '../Components/roles-component/roles-component';
import { EmployeeAddComponent } from '../Components/employee-add-component/employee-add-component';
import { EmployeeSalaryComponent } from '../Components/employee-salary-component/employee-salary-component';
import { PayrollComponent } from '../Components/payroll-component/payroll-component';
import { StockComponent } from '../Components/stock/stock.component';
import { SalarySearchComponent } from '../Components/salary-search-component/salary-search-component';
import { QuickAttendanceComponent } from '../Components/quick-attendance-component/quick-attendance-component';
import { DepartmentComponent } from '../Components/department-component/department-component';
import { CollectionRepresentiveRateComponent } from '../Components/collection-representive-rate-component/collection-representive-rate-component';
import { CoponCollectionRepresentiveRateComponent } from '../Components/copon-collection-representive-rate-component/copon-collection-representive-rate-component';
import { PublicHolidayComponent } from '../Components/public-holiday-component/public-holiday-component';
import { EmployeeLoanComponent } from '../Components/employee-loan-component/employee-loan-component';

import { LeaveRequestsComponent } from '../Components/leave/leave-requests-component/leave-requests-component';
import { CreateLeaveRequestComponent } from '../Components/leave/create-leave-request/create-leave-request';
import { PendingLeaveRequestsComponent } from '../Components/leave/pending-leave-requests/pending-leave-requests';
import { LeaveBalanceComponent } from '../Components/leave/leave-balance/leave-balance-component';
import { LeaveTypesComponent } from '../Components/leave/leave-types/leave-types.component';
import { HrCreateLeaveComponent } from '../Components/leave/hr-create-leave/hr-create-leave.component';
import { AllLeaveRequestsComponent } from '../Components/leave/all-leave-requests/all-leave-requests';
import { PayrollDeductionsComponent } from '../Components/payroll-deductions-component/payroll-deductions-component';
import { EmployeeDeductionsSummaryComponent } from '../Components/payroll-deductions-summary/employee-deductions-summary.component';

import { TreeAccountsComponent } from '../Components/tree-accounts/tree-accounts.component';
import { SupplierComponent } from '../Components/supplier/supplier.component';
import { RepresentativesListComponent } from '../Components/representatives-list-component/representatives-list-component';
import { RepresentativeAddComponent } from '../Components/representative-add-component/representative-add-component';
import { RepresentativeCheckInComponent } from '../Components/representative-check-in-component/representative-check-in-component';
import { roleGuard } from '../Guards/role.guard';

export const routes: Routes = [
  { path: 'login', component: AuthLayout },

  { path: '', redirectTo: 'login', pathMatch: 'full' },

  {
    path: '',
    component: SideBarComponent,
    children: [


      //#region  Accountatnt
      { path: 'SalesInvoices', component: SalesInvoicesComponent,canActivate: [authGuard, roleGuard], data: { roles: [ 'Accountant','Admin'] }},
      { path: 'Copouns', component: CopounComponent,   canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'Accountant'] }},
      { path: 'general-setting/bill-discounts', component: BillDiscountComponent,  canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'Accountant'] }},
      { path: 'DistributorsAndMerchants', component: DisAndMerchantComponent,  canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'Accountant']}},
      {
  path: 'supplier/all',
  loadComponent: () =>
    import('../Components/supplier/supplier.component')
      .then(c => c.SupplierComponent),
  canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'Accountant'] }

},
{
  path: 'supplier/add',
  loadComponent: () =>
    import('../Components/add-edit-supplier/add-edit-supplier.component')
      .then(c => c.AddEditSupplierComponent),
  canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'Accountant'] }

},
{
  path: 'supplier/edit/:id',
  loadComponent: () =>
    import('../Components/add-edit-supplier/add-edit-supplier.component')
      .then(m => m.AddEditSupplierComponent),
  canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'Accountant'] }

},
{
  path: 'purchase-invoice/add',
  loadComponent: () =>
    import('../Components/add-edit-purchase-invoice/add-edit-purchase-invoice.component')
      .then(c => c.AddEditPurchaseInvoiceComponent),
   canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'Accountant'] }

},
{
  path: 'purchase-invoice/all',
  loadComponent: () =>
    import('../Components/purchase-invoices/purchase-invoices.component')
      .then(c => c.PurchaseInvoicesComponent),
    canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'Accountant'] }

},
{
  path: 'purchase-invoice/edit/:id',
  loadComponent: () =>
    import('../Components/add-edit-purchase-invoice/add-edit-purchase-invoice.component')
      .then(c => c.AddEditPurchaseInvoiceComponent),
  canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'Accountant'] }

},
{
  path: 'purchase-invoice/:id/details',
  loadComponent: () =>
    import('../Components/purchase-invoice-details/purchase-invoice-details.component')
      .then(c => c.PurchaseInvoiceDetailsComponent),
   canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'Accountant'] }

}
,
{
  path: 'sales-invoice/add',
  loadComponent: () =>
    import('../Components/add-edit-sales-invoice/add-edit-sales-invoice.component')
      .then(c => c.AddEditSalesInvoiceComponent),
  canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'Accountant'] }

},
{
  path: 'sales-invoice/edit/:id',
  loadComponent: () =>
    import('../Components/add-edit-sales-invoice/add-edit-sales-invoice.component')
      .then(c => c.AddEditSalesInvoiceComponent),
    canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'Accountant'] }

},
{
  path: 'sales-invoice/confirmation/:id',
  loadComponent: () =>
    import('../Components/invoice-confirmation/invoice-confirmation.component')
      .then(c => c.InvoiceConfirmationComponent),
   canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'Accountant'] }

},
{
  path: 'sales-invoice/:id/details',
  loadComponent: () =>
    import('../Components/sales-invoice-details/sales-invoice-details.component')
      .then(c => c.SalesInvoiceDetailsComponent),
    canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'Accountant'] }

},
{
  path: 'TreeAccounts/add',
  loadComponent: () =>
    import('../Components/add-edit-account-in-tree/add-edit-account-in-tree.component')
      .then(c => c.AddEditAccountInTreeComponent),
  canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'Accountant'] }

},
{
  path: 'TreeAccounts/edit/:id',
  loadComponent: () =>
    import('../Components/add-edit-account-in-tree/add-edit-account-in-tree.component')
      .then(c => c.AddEditAccountInTreeComponent),
  canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'Accountant'] }

}
,
{
  path: 'TreeAccounts/details/:id',
  loadComponent: () =>
    import('../Components/account-details/account-details.component')
      .then(c => c.AccountDetailsComponent),
  canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'Accountant'] }

},
{
  path: 'journal-entries',
  loadComponent: () =>
    import('../Components/journal-entries/journal-entries.component')
      .then(c => c.JournalEntriesComponent),
   canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'Accountant'] }

},
{
  path: 'journal-entry/add',
  loadComponent: () =>
    import('../Components/add-edit-journal-entry/add-edit-journal-entry.component')
      .then(c => c.AddEditJournalEntryComponent),
   canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'Accountant'] }

},
{
  path: 'journal-entry/edit/:id',
  loadComponent: () =>
    import('../Components/add-edit-journal-entry/add-edit-journal-entry.component')
      .then(c => c.AddEditJournalEntryComponent),
   canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'Accountant'] }

},
{
  path: 'journal-entry/confirm/:id',
  loadComponent: () =>
    import('../Components/add-edit-journal-entry/add-edit-journal-entry.component')
      .then(c => c.AddEditJournalEntryComponent),
  canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'Accountant'] }

},
{
  path: 'journal-entry/view/:id',
  loadComponent: () =>
    import('../Components/add-edit-journal-entry/add-edit-journal-entry.component')
      .then(c => c.AddEditJournalEntryComponent),
  canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'Accountant'] }
},
      //#endregion

      //#region Hr
      { path: 'hr/attendance', component: HrAttendanceComponent, canActivate: [authGuard, roleGuard], data: { roles: ['Admin','HR']}},
      { path: 'hr/attendance-record', component: HrAttendanceRecordComponent,  canActivate: [authGuard, roleGuard], data: { roles: ['Admin','HR']}},
      { path: 'hr/representative-attendance', component: RepresentativeAttendanceComponent, canActivate: [authGuard, roleGuard], data: { roles: ['Admin','HR']}},
      { path: 'hr/representative-check-in', component: RepresentativeCheckInComponent,  canActivate: [authGuard, roleGuard], data: { roles: ['Admin','HR']}},
      //#endregion

      //#region  stock mangment
      { path: 'Products', component: ProductComponent, canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'StockManager'] }},
      { path: 'transactions/all', component: TransactionsComponent,canActivate: [authGuard, roleGuard], data: { roles: [ 'StockManager','Admin'] }},
      { path: 'stores/all', component: StoresComponent,canActivate: [authGuard, roleGuard], data: { roles: [ 'StockManager','Admin'] }},
      //#endregion

      //#region  shared
      { path: 'general-setting/Governrates', component: GovernrateComponent,  canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'Accountant','HR']}},
      { path: 'general-setting/cities', component: CityComponent, canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'Accountant','HR']}},
      //#endregion




 {
        path: 'stocks',
        loadComponent: () => import('../Components/stock/stock.component')
          .then(m => m.StockComponent),
       canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'StockManager'] }
      },
      
      { path: 'hr/employees', component: EmployeesListComponent, canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'HR'] }},
      { path: 'hr/employee-salary/:empCode', component: EmployeeSalaryComponent, canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'HR'] }},
      { path: 'hr/roles', component: RolesComponent, canActivate: [authGuard, roleGuard], data: { roles: ['Admin'] }},
      { path: 'hr/employees/add', component: EmployeeAddComponent, canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'HR'] }},
      { path: 'hr/salaries', component: SalarySearchComponent, canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'HR'] }},
      { path: 'hr/quick-attendance', component: QuickAttendanceComponent, canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'HR'] }},
      { path: 'hr/departments', component: DepartmentComponent, canActivate: [authGuard, roleGuard], data: { roles: ['Admin'] }},
      { path: 'hr/collection-rates', component: CollectionRepresentiveRateComponent, canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'HR'] }},
      { path: 'hr/copon-collection-rates', component: CoponCollectionRepresentiveRateComponent, canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'HR'] }},
      { path: 'hr/employee-loans', component: EmployeeLoanComponent, canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'HR'] }},
      { path: 'hr/my-leave-requests', component: LeaveRequestsComponent, canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'HR'] }},
      { path: 'hr/leave-request/create', component: CreateLeaveRequestComponent, canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'HR'] }},
      { path: 'hr/leave-request/create-hr', component: HrCreateLeaveComponent, canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'HR'] }},
      { path: 'hr/pending-leave-requests', component: PendingLeaveRequestsComponent, canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'HR'] }},
      { path: 'hr/leave-balance', component: LeaveBalanceComponent, canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'HR'] }},
      { path: 'hr/leave-wallets', loadComponent: () => import('../Components/leave/leave-wallets/leave-wallets.component').then(m => m.LeaveWalletsComponent), canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'HR'] }},
      { path: 'hr/leave-types', component: LeaveTypesComponent, canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'HR'] }},
      { path: 'hr/all-leave-requests', component: AllLeaveRequestsComponent, canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'HR'] }},
      { path: 'hr/payroll-deductions', component: PayrollDeductionsComponent, canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'HR'] }},
      { path: 'hr/employee-deductions', component: EmployeeDeductionsSummaryComponent, canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'HR'] }},
      { path: 'hr/payroll', component: PayrollComponent, canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'HR'] }},

      {
        path: 'hr/employee-loan-summary',
        loadComponent: () => import('../Components/employee-loan-component/employee-summary-page/employee-summary-page.component')
          .then(m => m.EmployeeSummaryPageComponent),
       canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'HR'] }
      },
      { path: 'hr/public-holidays', component: PublicHolidayComponent, canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'HR'] }},
      { path: 'tree', component: TreeAccountsComponent, canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'Accountant'] }},
      { path: 'sales/representatives', component: RepresentativesListComponent, canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'Accountant'] }},
      { path: 'sales/representatives/add', component: RepresentativeAddComponent, canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'Accountant'] }},
    ],canActivate:[authGuard]
  },
  {path :'unauthorized',component:UnauthorizedComponent}
,
  { path: '**', component:NotFoundComponent }
];
