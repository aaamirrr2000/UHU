using MicroERP.App.Pages;

namespace MicroERP.App
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(OrderPage), typeof(OrderPage));
            Routing.RegisterRoute(nameof(OrdersPage), typeof(OrdersPage));
            Routing.RegisterRoute(nameof(BillPrintPage), typeof(BillPrintPage));
            Routing.RegisterRoute(nameof(CartPage), typeof(CartPage));
            Routing.RegisterRoute(nameof(DineinOrdersPage), typeof(DineinOrdersPage));
            Routing.RegisterRoute(nameof(KitchenOrderPage), typeof(KitchenOrderPage));
            Routing.RegisterRoute(nameof(TablesPage), typeof(TablesPage));
        }
    }
}

