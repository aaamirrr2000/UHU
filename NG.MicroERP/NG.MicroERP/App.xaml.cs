
using NG.MicroERP.Shared.Pages;
using NG.MicroERP.Shared;

namespace NG.MicroERP
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            //MainPage = new NavigationPage(new LoginPage());
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new MainPage()) { Title = "NG.MicroERP" };
        }
    }
}
