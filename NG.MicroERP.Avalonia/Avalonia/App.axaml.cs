using System.Net.Http;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using NG.MicroERP.Shared.Helper;

namespace Avalonia
{
    public partial class App : Application
    {
        public static Globals Globals { get; private set; } = null!;
        public static Functions Functions { get; private set; } = null!;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            Globals = new Globals();
            var httpClient = new HttpClient();
            Functions = new Functions(Globals, httpClient);

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new Pages.LoginPage();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
