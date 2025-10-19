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

        services.AddSingleton<IUsersService, UsersService>();
        services.AddSingleton<IOrganizationsService, OrganizationsService>();
        services.AddSingleton<IMyMenuService, MyMenuService>();
        services.AddSingleton<IGroupsService, GroupsService>();
        services.AddSingleton<ILocationsService, LocationsService>();
        services.AddSingleton<IEmployeesService, EmployeesService>();
        services.AddSingleton<IGroupsService, GroupsService>();
        services.AddSingleton<IAreasService, AreasService>();
        services.AddSingleton<ILeaveTypesService, LeaveTypesService>();

        services.AddScoped<Globals>();
        services.AddScoped<FileUploadService>();
    }
}
