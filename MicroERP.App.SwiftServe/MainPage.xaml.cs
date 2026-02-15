using MicroERP.App.SwiftServe.Helper;
using Microsoft.Maui.Controls;

namespace MicroERP.App.SwiftServe
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            // MyGlobals is now static, no need to get from DI
            Serilog.Log.Information($"MainPage: MyGlobals static class ready - User: {MyGlobals.User?.Username ?? "NULL"}, Token length: {MyGlobals.Token?.Length ?? 0}");
            
            // Ensure BlazorWebView is visible immediately to prevent flicker
            if (blazorWebView != null)
            {
                blazorWebView.IsVisible = true;
            }
        }
    }
}

