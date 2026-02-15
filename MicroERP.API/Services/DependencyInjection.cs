using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using MudBlazor;
using MudBlazor.Services;

using MicroERP.API.Services.Services;
using MicroERP.Shared.Helper;

using Serilog;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MicroERP.API.Services;

public static class DependencyInjection
{
    public static void AddServices(this IServiceCollection services)
    {
        services.AddHttpClient();

        services.AddSingleton<IAreasService, AreasService>();
        services.AddSingleton<IBankService, BankService>();
        services.AddSingleton<IBankReconciliationService, BankReconciliationService>();
        services.AddSingleton<IChartOfAccountsService, ChartOfAccountsService>();
        services.AddSingleton<IGeneralLedgerService, GeneralLedgerService>();
        services.AddSingleton<IPeriodCloseService, PeriodCloseService>();
        services.AddSingleton<IDailyAttendanceService, DailyAttendanceService>();
        services.AddSingleton<IDepartmentsService, DepartmentsService>();
        services.AddSingleton<IDesignationsService, DesignationsService>();
        services.AddSingleton<IEmployeesService, EmployeesService>();
        services.AddSingleton<IGroupsService, GroupsService>();
        services.AddSingleton<IHolidayCalendarService, HolidayCalendarService>();
        services.AddSingleton<ILeaveRequestsService, LeaveRequestsService>();
        services.AddSingleton<ILeaveTypesService, LeaveTypesService>();
        services.AddSingleton<ILocationsService, LocationsService>();
        services.AddSingleton<IMyMenuService, MyMenuService>();
        services.AddSingleton<IOrganizationsService, OrganizationsService>();
        services.AddSingleton<IPartiesService, PartiesService>();
        services.AddSingleton<IPartyBankDetailsService, PartyBankDetailsService>();
        services.AddSingleton<IPartyContactsService, PartyContactsService>();
        services.AddSingleton<IPartyDocumentsService, PartyDocumentsService>();
        services.AddSingleton<IPartyFinancialsService, PartyFinancialsService>();
        services.AddSingleton<IPartyVehiclesService, PartyVehiclesService>();
        services.AddSingleton<IScannerDevicesService, ScannerDevicesService>();
        services.AddSingleton<IChargesRulesService, ChargesRulesService>();
        services.AddSingleton<IShiftsService, ShiftsService>();
        services.AddSingleton<IRestaurantTablesService, RestaurantTablesService>();

        services.AddSingleton<ITaxMasterService, TaxMasterService>();
        services.AddSingleton<ITaxRuleService, TaxRuleService>();
        services.AddSingleton<ITaxRuleDetailService, TaxRuleDetailService>();
        
        services.AddSingleton<IUsersService, UsersService>();
        services.AddSingleton<ICurrenciesService, CurrenciesService>();
        services.AddSingleton<IBackupService, BackupService>();

        services.AddSingleton<IInvoiceService, InvoiceService>();
        services.AddSingleton<IInvoiceDetailService, InvoiceDetailService>();
        services.AddSingleton<IPaymentTermsService, PaymentTermsService>();
 
        services.AddSingleton<ICategoriesService, CategoriesService>();
        services.AddSingleton<IItemsService, ItemsService>();
        services.AddSingleton<IPriceListService, PriceListService>();
        services.AddSingleton<ITaxCalculationService, TaxCalculationService>();
        
        // Stock Management Services
        services.AddSingleton<IInventoryService, InventoryService>();
        services.AddSingleton<IShipmentService, ShipmentService>();
        services.AddSingleton<IStockMovementService, StockMovementService>();
        services.AddSingleton<ISerializedItemService, SerializedItemService>();
        
        services.AddScoped<Globals>();
        services.AddScoped<FileUploadService>();
    }
}

