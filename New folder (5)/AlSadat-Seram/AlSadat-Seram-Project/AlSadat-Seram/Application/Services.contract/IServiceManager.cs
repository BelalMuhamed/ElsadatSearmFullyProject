using Application.Services.contract.AuthService;
using Application.Services.contract.BillDiscountsServiceContract;
using Application.Services.contract.ChangeLogService;
using Application.Services.contract.CollectionRepresentiveRateService;
using Application.Services.contract.CoponCollectionRepresentiveRateService;
using Application.Services.contract.CopounServiceContract;
using Application.Services.contract.CurrentUserService;
using Application.Services.contract.DepartmentService;
using Application.Services.contract.EmployeeAttendanceService;
using Application.Services.contract.EmployeeBonus;
using Application.Services.contract.EmployeeLeave;
using Application.Services.contract.EmployeeLoan;
using Application.Services.contract.EmployeePayroll;
using Application.Services.contract.EmployeeSalaryAdjustment;
using Application.Services.contract.EmployeeService;
using Application.Services.contract.Finance;
using Application.Services.contract.GoogleAuthService;
using Application.Services.contract.JwtService;
using Application.Services.contract.LeaveType;
using Application.Services.contract.NotificationService;
using Application.Services.contract.PayrollDeduction;
using Application.Services.contract.ProfileService;
using Application.Services.contract.PublicHolidayService;
using Application.Services.contract.RepresentativeAttendanceService;
using Application.Services.contract.RepresentativeService;
using Application.Services.contract.SalesInvoiceService;


namespace Application.Services.contract
{
    public interface IServiceManager
    {
        //  Define all the services that the service manager will provide
        public IAuthService AuthService { get; }
        public IChangeLogService ChangeLogService { get; }
        public ICurrentUserService CurrentUserService { get; }
        public IGoogleAuthService GoogleAuthService { get; }
        public IJwtService JwtService { get; }
        public INotificationService NotificationService { get; }
        //public INotificationDispatcher NotificationDispatcher { get; }
        public IProfileService ProfileService { get; }
        public ICoponCollectionRepresentiveRateService CoponCollectionRepresentiveRateService { get; }
        public IsalesInvoiceService SalesInvoiceService { get; }
        public IEmployeeAttendanceService EmployeeAttendanceService { get; }
        public ICopounService CopounService { get; }
        public IBillDiscount BillService { get; }
        public IEmployeeService EmployeeService { get; }
        public IProductService ProductService { get; }
        public IGovernrateCaontract  GovernrateService { get; }
        public ICityContract CityContract { get; }
        public IDistributorsAndMerchantsService DistributorsAndMerchantsService { get; }
        public ICollectionRepresentiveRateService CollectionRepresentiveRateService { get; }
        public IPublicHolidayService PublicHolidayService { get; }
        public IStoreTransactionService storeTransactionService { get; }
        public IDepartmentService DepartmentService { get; }
        public IStore storeService { get; }
        public IStockService stockService { get; }
        public ITreeAccounts treeService { get; }
        public ISupplierContract supplierService { get; }

        public IEmployeeBonusService EmployeeBonusService { get; } // خدمة مكافآت الموظفين
        public IEmployeeLeaveService EmployeeLeaveService { get; } // خدمة إجازات الموظفين
        public IEmployeeLoanService EmployeeLoanService { get; } // خدمة قروض الموظفين
        public IEmployeePayrollService EmployeePayrollService { get; } // خدمة الرواتب الموظفين
        public IEmployeeSalaryAdjustmentService EmployeeSalaryAdjustmentService { get; } // خدمة تعديل رواتب الموظفين
        public IPayrollDeductionService PayrollDeductionService { get; } // خدمة استقطاعات الرواتب
        public ILeaveTypeService LeaveTypeService { get; } // خدمة أنواع الإجازات

        public IRepresentativeService _RepresentativeService { get; }
        public IRepresentativeAttendanceService RepresentativeAttendanceService { get; }

        public IPurchaseInvoiceContract purchaseInvoiceService { get; }
        public IjournalEntryDetails journalEntryDeatils { get; }
        public IJounalEntryContract journalEntry { get; }
        public IWarehouseInventoryReportService warehouseInventoryReportService { get; }

      public  IFinancialReportsService financialReports { get; }
      public  ISystemAccountGuard systemAccountGuard { get; }
       public IStoreTransactionValidator storeTransactionValidator { get; }
        /// <summary>Plumber master-data service (no chart-of-accounts integration).</summary>
       public IPlumberContract plumberService { get; }



    }
}
