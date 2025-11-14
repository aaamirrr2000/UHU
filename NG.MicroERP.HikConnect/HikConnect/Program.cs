using Serilog;

namespace HikConnect
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {


            // Create Logs folder if it doesn't exist
            string logFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            Directory.CreateDirectory(logFolder);

            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(
                    path: Path.Combine(logFolder, "log-.txt"),  // note the dash for rolling logs
                    rollingInterval: RollingInterval.Day,       // creates one file per day
                    retainedFileCountLimit: 7,                  // keep last 7 days
                    fileSizeLimitBytes: null,                   // no size limit
                    shared: true,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .CreateLogger();


            ApplicationConfiguration.Initialize();
            Application.Run(new frmHikSync());
        }
    }
}