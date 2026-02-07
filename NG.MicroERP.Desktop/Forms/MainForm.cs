using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;
using NG.MicroERP.Desktop.Components;
using System.Windows.Forms;

namespace NG.MicroERP.Desktop.Forms
{
    public partial class MainForm : Form
    {
        private BlazorWebView? blazorWebView;

        public MainForm(IServiceProvider serviceProvider)
        {
            InitializeComponent(serviceProvider);
        }

        private void InitializeComponent(IServiceProvider serviceProvider)
        {
            this.Text = "MicroERP Desktop";
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;

            blazorWebView = new BlazorWebView()
            {
                Dock = DockStyle.Fill,
                HostPage = "wwwroot/index.html"
            };

            blazorWebView.Services = serviceProvider;
            blazorWebView.RootComponents.Add(new RootComponent("#app", typeof(Routes), null));

            this.Controls.Add(blazorWebView);
        }
    }
}
