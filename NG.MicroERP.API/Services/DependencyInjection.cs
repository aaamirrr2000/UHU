using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using MudBlazor;
using MudBlazor.Services;

using NG.MicroERP.API.Services.Services;
using NG.MicroERP.Shared.Helper;

using Serilog;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NG.MicroERP.API.Services;

public static class DependencyInjection
{
    public static void AddServices(this IServiceCollection services)
    {
        services.AddHttpClient();

        services.AddSingleton<IAreasService, AreasService>();
        services.AddSingleton<IBankService, BankService>();
        services.AddSingleton<ICategoriesService, CategoriesService>();
        services.AddSingleton<IChartOfAccountsService, ChartOfAccountsService>();
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
        services.AddSingleton<IServiceChargesService, ServiceChargesService>();
        services.AddSingleton<IShiftsService, ShiftsService>();
        services.AddSingleton<ITaxMasterService, TaxMasterService>();
        services.AddSingleton<ITaxService, TaxService>();
        services.AddSingleton<IUsersService, UsersService>();
        services.AddSingleton<ICurrenciesService, CurrenciesService>();

        services.AddScoped<Globals>();
        services.AddScoped<FileUploadService>();
    }
}
