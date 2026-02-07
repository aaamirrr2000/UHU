namespace NG.MicroERP.Desktop.Services
{
    public class DesktopClientInfoService
    {
        public string GetClientInfoString()
        {
            // For desktop app, return simplified client info
            string username = Environment.UserName ?? "Unknown";
            string ip = "127.0.0.1";
            string browser = "Desktop App";
            string os = Environment.OSVersion.ToString();
            var time = DateTime.UtcNow.ToString("MMddHHmm");

            return $"{username}|{ip}|{browser}|{os}|{time}";
        }
    }
}
