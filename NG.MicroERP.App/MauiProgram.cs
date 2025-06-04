using CommunityToolkit.Maui;

using Controls.UserDialogs.Maui;

using Microsoft.Extensions.Logging;

using NG.MicroERP.Shared.Services;

namespace NG.MicroERP.App
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseUserDialogs(() =>
                {
                    AlertConfig.DefaultBackgroundColor = Colors.Purple;
                    #if ANDROID
                            AlertConfig.DefaultMessageFontFamily = "OpenSans-Regular.ttf";
                    #else
                            AlertConfig.DefaultMessageFontFamily = "OpenSans-Regular";
                    #endif
                    ToastConfig.DefaultCornerRadius = 15;
                })
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Orbitron.ttf", "Orbitron");
                    fonts.AddFont("RubikScribble.ttf", "RubikScribble");
                    fonts.AddFont("FiraSansExtraCondensed.ttf", "FiraSansExtraCondensed");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif
            //builder.Services.AddServices();

            HttpClientHandler handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            HttpClient client = new HttpClient(handler);

            return builder.Build();
        }
    }
}
