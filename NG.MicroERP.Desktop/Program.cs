using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using MudBlazor;
using MudBlazor.Services;
using NG.MicroERP.Desktop.Forms;
using NG.MicroERP.Desktop.Services;
using NG.MicroERP.Shared.Helper;
using NG.MicroERP.Shared.Services;
using System.Windows.Forms;

namespace NG.MicroERP.Desktop
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            var services = new ServiceCollection();
            ConfigureServices(services);

            using var serviceProvider = services.BuildServiceProvider();
            var mainForm = serviceProvider.GetRequiredService<MainForm>();
            Application.Run(mainForm);
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddWindowsFormsBlazorWebView();
            services.AddMudServices(config =>
            {
                config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopRight;
                config.SnackbarConfiguration.PreventDuplicates = false;
                config.SnackbarConfiguration.NewestOnTop = false;
                config.SnackbarConfiguration.ShowCloseIcon = true;
                config.SnackbarConfiguration.VisibleStateDuration = 2000;
                config.SnackbarConfiguration.HideTransitionDuration = 300;
                config.SnackbarConfiguration.ShowTransitionDuration = 300;
                config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
            });

            services.AddHttpClient();
            services.AddSingleton<Globals>();
            services.AddSingleton<DesktopClientInfoService>();
            services.AddSingleton<Functions>(sp => new Functions(sp.GetRequiredService<Globals>(), sp.GetRequiredService<IHttpClientFactory>().CreateClient()));

#if DEBUG
            services.AddBlazorWebViewDeveloperTools();
            services.AddLogging(builder => builder.AddDebug());
#endif

            services.AddSingleton<MainForm>();
        }
    }
}
