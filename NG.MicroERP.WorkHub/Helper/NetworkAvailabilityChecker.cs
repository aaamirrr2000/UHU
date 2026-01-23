namespace NG.MicroERP.WorkHub.Helper;

public class NetworkAvailabilityChecker
{
    private readonly System.Timers.Timer networkCheckTimer;

    public NetworkAvailabilityChecker()
    {
        networkCheckTimer = new System.Timers.Timer(300000); // 5 minutes
        networkCheckTimer.Elapsed += NetworkCheckTimer_Elapsed;
        networkCheckTimer.AutoReset = true; // Repeats automatically
    }

    public void StartCheckingNetworkAvailability()
    {
        networkCheckTimer.Start();
    }

    public void StopCheckingNetworkAvailability()
    {
        networkCheckTimer.Stop();
    }

    private void NetworkCheckTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        NetworkAccess networkAccess = Connectivity.Current.NetworkAccess;
        if (networkAccess == NetworkAccess.Internet)
        {
            // Network is available, execute your function here
            // For example: Show a notification
        }
    }
}
