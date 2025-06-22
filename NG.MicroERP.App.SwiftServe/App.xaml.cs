using NG.MicroERP.App.SwiftServe.Components.MauiPages;
using NG.MicroERP.Shared.Helper;

using System.Diagnostics;

namespace NG.MicroERP.App.SwiftServe
{
    public partial class App : Application
    {
        private Window _mainWindow;
        private bool IsPhone = true;

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
            int newWidth = 1280;
            int newHeight = 800;
            int X = 300;
            int Y = 100;

            if (IsPhone==true)
            {
                newWidth = 550;
                newHeight = 1000;
                X = 1375;
                Y = 50;
            }

            _mainWindow = new Window(new NavigationPage(new LoginPage()));

            if (Debugger.IsAttached)
            {
                _mainWindow.X = X;
                _mainWindow.Y = Y;
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
