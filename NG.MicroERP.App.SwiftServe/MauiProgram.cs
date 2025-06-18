using Microsoft.Extensions.Logging;

using MudBlazor;
using MudBlazor.Services;

using NG.MicroERP.App.SwiftServe.Services;
using NG.MicroERP.Shared.Helper;
using NG.MicroERP.Shared.Services;

namespace NG.MicroERP.App.SwiftServe
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            //Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NNaF5cXmBCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXlfeHVXRWNcWU12XktWYUA=");

            var builder = MauiApp.CreateBuilder();
            builder
                 .UseMauiApp<App>()
                 
                 .ConfigureFonts(fonts =>
                 {
                     fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                     fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                     fonts.AddFont("Orbitron.ttf", "Orbitron");
                     fonts.AddFont("RubikScribble.ttf", "RubikScribble");
                     fonts.AddFont("FiraSansExtraCondensed.ttf", "FiraSansExtraCondensed");
                 });

            builder.Services.AddSingleton<CartStateService>();
            builder.Services.AddScoped<Globals>();
            builder.Services.AddSingleton<INavigationService, NavigationService>();
            builder.Services.AddMudServices();
            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddMudServices(config =>
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


#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		    builder.Logging.AddDebug();
            #endif

            return builder.Build();
        }
    }
}
