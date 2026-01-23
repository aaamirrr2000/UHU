using NG.MicroERP.WorkHub.Pages;

namespace NG.MicroERP.WorkHub;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		
		// Register routes
		Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
		Routing.RegisterRoute(nameof(DashboardPage), typeof(DashboardPage));
		Routing.RegisterRoute(nameof(AttendancePage), typeof(AttendancePage));
		Routing.RegisterRoute(nameof(RosterPage), typeof(RosterPage));
		Routing.RegisterRoute(nameof(LeaveRequestPage), typeof(LeaveRequestPage));
		Routing.RegisterRoute(nameof(MyLeavesPage), typeof(MyLeavesPage));
	}
}
