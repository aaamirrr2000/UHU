using NG.MicroERP.Shared.Helper;

namespace NG.MicroERP
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
