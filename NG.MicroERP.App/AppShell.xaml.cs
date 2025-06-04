using NG.MicroERP.App.Pages;

namespace NG.MicroERP.App
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(ClientPage), typeof(ClientPage));
            Routing.RegisterRoute(nameof(ExecutionPage), typeof(ExecutionPage));
            Routing.RegisterRoute(nameof(OrderPage), typeof(OrderPage));
            Routing.RegisterRoute(nameof(LogoutPage), typeof(LogoutPage));
            Routing.RegisterRoute(nameof(MenuPage), typeof(MenuPage));
        }
    }
}
