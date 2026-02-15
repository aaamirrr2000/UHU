
using MicroERP.Shared.Pages;
using MicroERP.Shared;

namespace MicroERP
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
            return new Window(new MainPage()) { Title = "MicroERP" };
        }
    }
}

