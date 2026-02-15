using MicroERP.Shared.Helper;

namespace MicroERP
{
    public partial class MainPage : TabbedPage
    {
        
        public MainPage()
        {
            Globals Globals = new Globals();

            InitializeComponent();
            if (!Globals._tabsInitialized)
            {
                Globals._tabsInitialized = true;
            }
        }
    }
}

