using Application.Services.contract;
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
using Application.Services.contract.NotificationDispatcher;
using Application.Services.contract.NotificationService;
using Application.Services.contract.PayrollDeduction;
using Application.Services.contract.ProfileService;
using Application.Services.contract.PublicHolidayService;
using Application.Services.contract.RepresentativeAttendanceService;
using Application.Services.contract.RepresentativeService;
using Application.Services.contract.SalesInvoiceService;
using AutoMapper;
using Domain.Common;
using Domain.Entities.Invoices;
using Domain.Entities.Users;
using Domain.UnitOfWork.Contract;
using Infrastructure.Data;
using Infrastructure.Services.AuthServices;
using Infrastructure.Services.ChangeLogServices;
using Infrastructure.Services.CollectionRepresentiveRateServices;
using Infrastructure.Services.CoponCollectionRepresentiveRateServices;
using Infrastructure.Services.CopounServices;
using Infrastructure.Services.CurrentUserServices;
using Infrastructure.Services.DepartmentServices;
using Infrastructure.Services.EmployeeAttendanceServices;
using Infrastructure.Services.EmployeeBonusServices;
using Infrastructure.Services.EmployeeLeaveServices;
using Infrastructure.Services.EmployeeLoanServices;
using Infrastructure.Services.EmployeePayrollServices;
using Infrastructure.Services.EmployeeSalaryAdjustmentServices;
using Infrastructure.Services.EmployeeServices;
using Infrastructure.Services.FinanceService;
using Infrastructure.Services.GoogleAuthServices;
using Infrastructure.Services.JwtServices;
using Infrastructure.Services.LeaveTypeServices;
using Infrastructure.Services.NotificationServices;
using Infrastructure.Services.PayrollDeductionServices;
using Infrastructure.Services.ProfileServices;
using Infrastructure.Services.PublicHolidayServices;
using Infrastructure.Services.RepresentativeAttendanceServices;
using Infrastructure.Services.RepresentativeServices;
using Infrastructure.Services.SalesInvoiceService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace Infrastructure.Services;
public class ServiceManager: IServiceManager
{
    // Lazy loading of services to improve performance
    // This allows the services to be created only when they are accessed for the first time.
    // This can help reduce the startup time of the application and improve overall performance.
    // It also helps to avoid unnecessary instantiation of services that may not be used.
    private readonly Lazy<IAuthService> _AuthService;
    private readonly Lazy<IChangeLogService> _ChangeLogService;
    private readonly Lazy<ICurrentUserService> _CurrentUserService;
    private readonly Lazy<IGoogleAuthService> _GoogleAuthService;
    private readonly Lazy<IJwtService> _JwtService;
    private readonly Lazy<INotificationService> _NotificationService;
    //private readonly Lazy<INotificationDispatcher> _NotificationDispatcher;
    private readonly Lazy<IProfileService> _ProfileService;
    private readonly Lazy<ICoponCollectionRepresentiveRateService> _CoponCollectionRepresentiveRateService;
    private readonly Lazy<IsalesInvoiceService> _salesInvoiceService;
    private readonly Lazy<IEmployeeAttendanceService> _EmployeeAttendanceService;
    private readonly Lazy<ICopounService> _CopounService;
    private readonly Lazy<IBillDiscount> _BillDiscountService;
    private readonly Lazy<IProductService> _ProductService;
    private readonly Lazy<IGovernrateCaontract> _GovernrateService;
    private readonly Lazy<ICityContract> _CityService;
    private readonly Lazy<IDistributorsAndMerchantsService> _DistributorsAndMerchantsService;
    private readonly Lazy<IEmployeeService> _EmployeeService;
    private readonly Lazy<ICollectionRepresentiveRateService> _CollectionRepresentiveRateService;
    private readonly Lazy<IPublicHolidayService> _PublicHolidayService;
    private readonly Lazy<IDepartmentService> _DepartmentService;
    private readonly Lazy<IStoreTransactionService> _StoreTransactionService;
    private readonly Lazy<IStore> _StoreService;
    private readonly Lazy<IStockService> _stockService;
    private readonly Lazy<ITreeAccounts> _treeService;
    private readonly Lazy<ISupplierContract> _supplierService;

    private readonly Lazy<IPayrollDeductionService> _PayrollDeductionService; // خدمة استقطاعات الرواتب
    private readonly Lazy<ILeaveTypeService> _LeaveTypeService; // خدمة أنواع الإجازات
    private readonly Lazy<IEmployeeSalaryAdjustmentService> _EmployeeSalaryAdjustmentService; // خدمة تعديل رواتب الموظفين
    private readonly Lazy<IEmployeePayrollService> _EmployeePayrollService; // خدمة الرواتب الموظفين
    private readonly Lazy<IEmployeeLoanService> _EmployeeLoanService; // خدمة قروض الموظفين
    private readonly Lazy<IEmployeeLeaveService> _EmployeeLeaveService; // خدمة إجازات الموظفين
    private readonly Lazy<IEmployeeBonusService> _EmployeeBonusService; // خدمة مكافآت الموظفين

    private readonly Lazy<IRepresentativeService> _RepresentativeService;
    private readonly Lazy<IRepresentativeAttendanceService> _RepresentativeAttendanceService;
    private readonly Lazy<IPurchaseInvoiceContract> _purchaseInvoiceService;
    private readonly Lazy<IjournalEntryDetails> _journalEntryDetails;
    private readonly Lazy<IJounalEntryContract> _journalEntry;
    private readonly IExcelReaderService excelReader;
    private readonly Lazy<IWarehouseInventoryReportService> _warehouseInventoryReportService;

    private readonly Lazy<IFinancialReportsService> _financialReports;
    private readonly Lazy<ISystemAccountGuard> _systemGuard;

    public ServiceManager(
    IUnitOfWork UnitOfWork,
    RoleManager<ApplicationRole> RoleManager,
    AppDbContext Context,
    UserManager<ApplicationUser> UserManager,
    IHttpContextAccessor HttpContextAccessor,
    IOptions<JwtSettings> jwtSettings,
    IMapper Mapper,
    ILoggerFactory loggerFactory, IExcelReaderService ExcelReader)
    {
        // Initialize the services using Lazy<T> to defer their creation until they are accessed  
        _CurrentUserService = new Lazy<ICurrentUserService>(() => new CurrentUserService(HttpContextAccessor,loggerFactory.CreateLogger<CurrentUserService>()));
        _JwtService = new Lazy<IJwtService>(() => new JwtService(jwtSettings,UserManager));
        _GoogleAuthService = new Lazy<IGoogleAuthService>(() => new GoogleAuthService(UserManager));
        _AuthService = new Lazy<IAuthService>(() => new AuthService(UserManager,_JwtService.Value,Context,_GoogleAuthService.Value , RoleManager , _CurrentUserService.Value));
        _ChangeLogService = new Lazy<IChangeLogService>(()=> new ChangeLogService(_CurrentUserService.Value));
        _NotificationService = new Lazy<INotificationService>(() => new NotificationService(UnitOfWork,Mapper,_CurrentUserService.Value));
        _PayrollDeductionService = new Lazy<IPayrollDeductionService>(() => new PayrollDeductionService(UnitOfWork, _CurrentUserService.Value)); 
        //_NotificationDispatcher = new Lazy<INotificationDispatcher>(() => new NotificationDispatcher(UnitOfWork,Mapper,_CurrentUserService.Value));
        _ProfileService = new Lazy<IProfileService>(()=> new ProfileService(Mapper,UserManager,_CurrentUserService.Value));
        _salesInvoiceService = new Lazy<IsalesInvoiceService>(() => new salesInvoiceService(UnitOfWork, _CurrentUserService.Value));
        _CopounService = new Lazy<ICopounService>(() => new CopounService(UnitOfWork));
        _BillDiscountService = new Lazy<IBillDiscount>(() => new BillsDiscountSr(UnitOfWork));
        _ProductService = new Lazy<IProductService>(() => new ProductServcie(UnitOfWork,_CurrentUserService.Value,excelReader));
        _StoreTransactionService = new Lazy<IStoreTransactionService>(() => new StoreTransactionService(UnitOfWork));
        _StoreService = new Lazy<IStore>(() => new StoreService(UnitOfWork));
        _GovernrateService = new Lazy<IGovernrateCaontract>(() => new GovernrateService(UnitOfWork));
        _CityService = new Lazy<ICityContract>(() => new CityService(UnitOfWork));
        _stockService = new Lazy<IStockService>(() => new StockService(UnitOfWork));
        _systemGuard = new Lazy<ISystemAccountGuard>(() => new SystemAccountGuard(UnitOfWork));
        _financialReports = new Lazy<IFinancialReportsService>(
            () => new FinancialReportsService(UnitOfWork, _systemGuard.Value));
        _treeService = new Lazy<ITreeAccounts>(()=> new TreeAccountsService(UnitOfWork, _systemGuard.Value));
        _supplierService = new Lazy<ISupplierContract>(
    () => new SupplierService(UnitOfWork, ExcelReader, this));
        _journalEntry = new Lazy<IJounalEntryContract>(() => new JournalEntryService(UnitOfWork));
        _journalEntryDetails = new Lazy<IjournalEntryDetails>(() => new JournalEntryDetailsService(UnitOfWork));
        _purchaseInvoiceService = new Lazy<IPurchaseInvoiceContract>(() => new PurchaseInvoiceService(UnitOfWork));
       
        _DistributorsAndMerchantsService = new Lazy<IDistributorsAndMerchantsService>(() => new DistributorsAndMerchantsService(UnitOfWork,UserManager,this, _CurrentUserService.Value, ExcelReader));
        _CoponCollectionRepresentiveRateService = new Lazy<ICoponCollectionRepresentiveRateService>(() => new CoponCollectionRepresentiveRateService(UnitOfWork , _CurrentUserService.Value));
        _EmployeeAttendanceService = new Lazy<IEmployeeAttendanceService>(() => new EmployeeAttendanceService(UnitOfWork,_CurrentUserService.Value,UserManager));
        _EmployeeService = new Lazy<IEmployeeService>(() => new EmployeeService(UserManager,_CurrentUserService.Value,UnitOfWork,RoleManager));
        _RepresentativeService = new Lazy<IRepresentativeService>(() => new RepresentativeService(UnitOfWork,_CurrentUserService.Value,UserManager,RoleManager));
        _CollectionRepresentiveRateService = new Lazy<ICollectionRepresentiveRateService> (() => new CollectionRepresentiveRateService(UnitOfWork,_CurrentUserService.Value));
        _PublicHolidayService = new Lazy<IPublicHolidayService>(() => new PublicHolidayService(UnitOfWork, _CurrentUserService.Value));
        _DepartmentService = new Lazy<IDepartmentService>(() => new DepartmentService(UnitOfWork, _CurrentUserService.Value));
        _LeaveTypeService = new Lazy<ILeaveTypeService>(() => new LeaveTypeService(UnitOfWork, _CurrentUserService.Value));
        _EmployeeSalaryAdjustmentService = new Lazy<IEmployeeSalaryAdjustmentService>(() => new EmployeeSalaryAdjustmentService(UnitOfWork, _CurrentUserService.Value));
        _EmployeePayrollService = new Lazy<IEmployeePayrollService>(() => new PayrollService(UnitOfWork, _CurrentUserService.Value,_EmployeeService.Value,loggerFactory.CreateLogger<PayrollService>(),_RepresentativeService.Value));
        _EmployeeLoanService = new Lazy<IEmployeeLoanService>(() => new EmployeeLoanService(UnitOfWork, _CurrentUserService.Value,loggerFactory.CreateLogger<EmployeeLoanService>()));
        _EmployeeLeaveService = new Lazy<IEmployeeLeaveService>(() => new EmployeeLeaveService(UnitOfWork,_CurrentUserService.Value,UserManager));
        _EmployeeBonusService = new Lazy<IEmployeeBonusService>(() => new EmployeeBonusService(UnitOfWork, _CurrentUserService.Value));    
        _RepresentativeAttendanceService = new Lazy<IRepresentativeAttendanceService>(() => new RepresentativeAttendanceService(UnitOfWork,_CurrentUserService.Value,UserManager));
        _warehouseInventoryReportService = new Lazy<IWarehouseInventoryReportService>(
            () => new WarehouseInventoryReportService(UnitOfWork));
        excelReader = ExcelReader;
    }
    // Properties to access the services
    public IAuthService AuthService => _AuthService.Value;
    public IChangeLogService ChangeLogService => _ChangeLogService.Value;
    public ICurrentUserService CurrentUserService => _CurrentUserService.Value;
    public IGoogleAuthService GoogleAuthService => _GoogleAuthService.Value;
    public IJwtService JwtService => _JwtService.Value;
    public INotificationService NotificationService => _NotificationService.Value;
    //public INotificationDispatcher NotificationDispatcher => _NotificationDispatcher.Value;
    public IProfileService ProfileService => _ProfileService.Value;
    public ICoponCollectionRepresentiveRateService CoponCollectionRepresentiveRateService => _CoponCollectionRepresentiveRateService.Value;
    public IsalesInvoiceService SalesInvoiceService => _salesInvoiceService.Value;
    public IEmployeeAttendanceService EmployeeAttendanceService => _EmployeeAttendanceService.Value;
    public ICopounService CopounService => _CopounService.Value;
    public IBillDiscount BillService => _BillDiscountService.Value;
    public IProductService ProductService => _ProductService.Value;
    public IGovernrateCaontract GovernrateService => _GovernrateService.Value;
    public ICityContract CityContract => _CityService.Value;
    public IDistributorsAndMerchantsService DistributorsAndMerchantsService => _DistributorsAndMerchantsService.Value;
    public IEmployeeService EmployeeService => _EmployeeService.Value;
    public ICollectionRepresentiveRateService CollectionRepresentiveRateService => _CollectionRepresentiveRateService.Value;
    public IPublicHolidayService PublicHolidayService => _PublicHolidayService.Value;
    public IDepartmentService DepartmentService => _DepartmentService.Value;
    public IStoreTransactionService storeTransactionService => _StoreTransactionService.Value;
    public IStore storeService => _StoreService.Value;
    public IStockService stockService => _stockService.Value;
    public ITreeAccounts treeService => _treeService.Value;
    public ISupplierContract supplierService => _supplierService.Value;
    public IPurchaseInvoiceContract purchaseInvoiceService => _purchaseInvoiceService.Value;
    public IjournalEntryDetails journalEntryDeatils => _journalEntryDetails.Value;
    public IJounalEntryContract journalEntry => _journalEntry.Value;




    public IEmployeeBonusService EmployeeBonusService => _EmployeeBonusService.Value;

    public IEmployeeLeaveService EmployeeLeaveService => _EmployeeLeaveService.Value;

    public IEmployeeLoanService EmployeeLoanService => _EmployeeLoanService.Value;

    public IEmployeePayrollService EmployeePayrollService => _EmployeePayrollService.Value;

    public IEmployeeSalaryAdjustmentService EmployeeSalaryAdjustmentService => _EmployeeSalaryAdjustmentService.Value;

    public IPayrollDeductionService PayrollDeductionService => _PayrollDeductionService.Value;

    public ILeaveTypeService LeaveTypeService => _LeaveTypeService.Value;

    IRepresentativeService IServiceManager._RepresentativeService => _RepresentativeService.Value;
    public IRepresentativeAttendanceService RepresentativeAttendanceService => _RepresentativeAttendanceService.Value;

    public IWarehouseInventoryReportService warehouseInventoryReportService
          => _warehouseInventoryReportService.Value;

    public IFinancialReportsService financialReports => _financialReports.Value;
    public ISystemAccountGuard systemAccountGuard => _systemGuard.Value;
}
