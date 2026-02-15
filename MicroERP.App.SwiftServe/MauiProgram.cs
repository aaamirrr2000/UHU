using Microsoft.Extensions.Logging;

using MicroERP.App.SwiftServe.Services;
using MicroERP.App.SwiftServe.Helper;
using MicroERP.Shared.Services;

namespace MicroERP.App.SwiftServe
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
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
            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<INavigationService, NavigationService>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}

