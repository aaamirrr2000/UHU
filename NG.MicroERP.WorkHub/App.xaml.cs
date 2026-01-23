using Microsoft.Extensions.DependencyInjection;
using NG.MicroERP.WorkHub.Pages;

namespace NG.MicroERP.WorkHub;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
		MainPage = new AppShell();
	}
}