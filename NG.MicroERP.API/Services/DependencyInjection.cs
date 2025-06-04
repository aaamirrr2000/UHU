using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MudBlazor;
using MudBlazor.Services;
using NG.MicroERP.Shared.Helper;
using Serilog;


namespace NG.MicroERP.API.Services;

public static class DependencyInjection
{
    public static void AddServices(this IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddSingleton<IUsersService, UsersService>();
        services.AddSingleton<IOrganizationsService, OrganizationsService>();
        services.AddSingleton<IMyMenuService, MyMenuService>();
        services.AddSingleton<IGroupsService, GroupsService>();
        services.AddSingleton<ILeavesService, LeavesService>();
        services.AddSingleton<ILocationsService, LocationsService>();
        services.AddSingleton<ICategoriesService, CategoriesService>();
        services.AddSingleton<IEmployeesService, EmployeesService>();
        services.AddSingleton<IGroupsService, GroupsService>();
        services.AddSingleton<IEmployeesDevicesService, EmployeesDevicesService>();
        services.AddSingleton<IChartOfAccountsService, ChartOfAccountsService>();
        services.AddSingleton<IStockReceivingService, StockReceivingService>();
        services.AddSingleton<IBillService, BillService>();
        services.AddSingleton<IBillDetailService, BillDetailService>();
        services.AddSingleton<IRestaurantTablesService, RestaurantTablesService>();
        services.AddSingleton<IDineinOrderStatusService, DineinOrderStatusService>();

        services.AddScoped<Globals>();
        services.AddScoped<FileUploadService>();
    }
}
