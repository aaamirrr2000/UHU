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


namespace NG.MicroERP.Shared.Services;

public static class DependencyInjectionShared
{
    public static void AddServices(this IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddScoped<Globals>();
        services.AddScoped<FileUploadService>();

        Log.Logger = new LoggerConfiguration()
            .WriteTo.File("logs/log-.txt",
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true)
            .CreateLogger();       
    }
}
