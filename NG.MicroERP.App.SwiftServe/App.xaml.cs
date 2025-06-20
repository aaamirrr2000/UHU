using NG.MicroERP.App.SwiftServe.Components.MauiPages;
using NG.MicroERP.Shared.Helper;

using System.Diagnostics;

namespace NG.MicroERP.App.SwiftServe
{
    public partial class App : Application
    {
        private Window _mainWindow;

        public App()
        {
            InitializeComponent();

            string savedUrl = Preferences.Get("BaseURI", "").Trim();
            if (!string.IsNullOrEmpty(savedUrl))
            {
                Globals.BaseURI = savedUrl;
            }

        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            const int newWidth = 550;
            const int newHeight = 1000;

            _mainWindow = new Window(new NavigationPage(new LoginPage()));

            if (Debugger.IsAttached)
            {
                // Production (no debugger): Fixed size window
                _mainWindow.X = 1375;
                _mainWindow.Y = 5;
                _mainWindow.Width = newWidth;
                _mainWindow.Height = newHeight;
                _mainWindow.MinimumWidth = newWidth;
                _mainWindow.MinimumHeight = newHeight;
                _mainWindow.MaximumWidth = newWidth;
                _mainWindow.MaximumHeight = newHeight;
            }

            return _mainWindow;
        }
    }
}
