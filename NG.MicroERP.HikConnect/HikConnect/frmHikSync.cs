using Google.Protobuf.WellKnownTypes;
using HikConnect.Helper;
using HikConnect.Models;
using MySql.Data.MySqlClient;
using Mysqlx.Expr;
using Newtonsoft.Json;
using NG.MicroERP.API.Helper;
using Serilog;
using System.Data.Common;
using System.Diagnostics;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace HikConnect
{
    public partial class frmHikSync : Form
    {
        private DateTime _lastSuccessfulRun = DateTime.Now;
        private System.Windows.Forms.Timer _watchdogTimer;
        List<ScannerDevicesModel> Devices = new List<ScannerDevicesModel>();
        List<EventConfigModel> EventConfig = new List<EventConfigModel>();
        EventConfigModel EConfig = new EventConfigModel();
        List<DeviceInfoModel> DevicesInfo = new List<DeviceInfoModel>();
        DapperFunctions dapper = new DapperFunctions();
        List<string> AllowedPCs = new List<string>
        {
                  "77272AFD750A707C24D2",
                  "905DC168C3B29C507A4C"
        };

        private static readonly HttpClientHandler _handler = new HttpClientHandler();
        private static readonly HttpClient _httpClient = new HttpClient(_handler)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        private bool _isAuthorized = false;

        public frmHikSync()
        {
            InitializeComponent();

            // === UI Initialization ===
            Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));

            // === Authorization Check ===
            string pcId = Config.GetPCUniqueId();
            _isAuthorized = AllowedPCs.Any(pc => pc.Equals(pcId, StringComparison.OrdinalIgnoreCase));

            if (!_isAuthorized)
            {
                ShowMessage($"System Error: Unauthorized PC detected.", "error", pcId);
                ShowMessage($"PC ID: {pcId}", "info");
                EventsTimer.Stop();
                return; // Exit constructor early for unauthorized systems
            }

            cmdStart.Visible = false;
            btnPause.Visible = true;
            btnClose.Visible = true;

            // === Environment Diagnostics ===
            if (Debugger.IsAttached)
                ShowMessage("Debug Mode Active.", "info");
            else
                ShowMessage("Live Mode Active.", "info");

        }

        private async void frmHikSync_Load(object sender, EventArgs e)
        {
            if (!_isAuthorized)
            {
                UpdateButtonStates(false);
                return;
            }

            // Load config and device info
            await SyncConfigAsync();
            await DevicesListAsync();

            // Perform one-time data refresh if needed
            EventsTimer_Tick(null, null);

            // Ensure all buttons reflect stopped state
            UpdateButtonStates(false);

            // Show timer interval info
            int totalSeconds = EventsTimer.Interval / 1000;
            lblTimer.Text = $"Sync Timer: {totalSeconds} sec";
        }


        private async Task DevicesListAsync()
        {
            try
            {
                string SQL = "SELECT * FROM ScannerDevices WHERE IsActive = 1";
                var freshDevices = await dapper.SearchByQuery<ScannerDevicesModel>(SQL) ?? new List<ScannerDevicesModel>();

                if (freshDevices == null || freshDevices.Count == 0)
                {
                    ShowMessage("No active devices found.", "error");
                    Devices = new List<ScannerDevicesModel>(); // Clear existing devices
                    return;
                }

                // Replace the existing devices list completely
                Devices = freshDevices;
                ShowMessage($"Found {Devices.Count} active device{(Devices.Count > 1 ? "s" : "")}...", "success");

                // Clear and refresh device info
                DevicesInfo.Clear();

                int index = 1;
                foreach (var dv in Devices)
                {
                    string ip = dv?.DeviceIpAddress?.Trim() ?? "Unknown IP";
                    string user = dv?.UserName?.Trim() ?? "Unknown User";

                    ShowMessage($"[{index}/{Devices.Count}] Device: {ip} ({user})", "info");

                    if (!string.IsNullOrWhiteSpace(ip) && ip != "Unknown IP")
                    {
                        await GetDeviceInfoAsync(dv);
                    }
                    else
                    {
                        ShowMessage($"Skipped device with missing IP ({user}).", "warning");
                    }
                    index++;
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Device list error: {ex.Message}", "error");
            }
        }

        private async Task SyncConfigAsync()
        {
            try
            {
                string SQL = "SELECT * FROM eventconfig WHERE Id = 1";
                EventConfig = await dapper.SearchByQuery<EventConfigModel>(SQL) ?? new List<EventConfigModel>();

                if (EventConfig == null || EventConfig.Count == 0)
                {
                    ShowMessage("No Sync Configuration Found.", "error");
                    return;
                }

                EConfig = EventConfig[0];
                var summary = new StringBuilder();

                if (EConfig.Timer_Seconds != EventsTimer.Interval)
                    summary.Append($"⏱ Sync time set to {EConfig.Timer_Seconds} seconds. ");

                EventsTimer.Interval = EConfig.Timer_Seconds != 0 ? EConfig.Timer_Seconds * 1000 : 60000;
                EConfig.Historical_Days = EConfig.Historical_Days == 0 ? 30 : EConfig.Historical_Days;
                lblTimer.Text = $"Sync Timer: {EventsTimer.Interval / 1000} sec";

                if (EConfig.StartStop == 1)
                {
                    EventsTimer.Start();
                    UpdateButtonStates(false);
                    await dapper.Insert("UPDATE EVENTCONFIG SET StartStop = 0");
                }

                summary.Append(EConfig.Email_Log == 1
                    ? "📧 Email Log is enabled. "
                    : "📧 Email Log is disabled. ");

                await dapper.Insert("UPDATE EVENTCONFIG SET Email_Log = 0");

                // ✅ Show only one combined message
                ShowMessage(summary.ToString().Trim(), "info");
            }
            catch (Exception ex)
            {
                ShowMessage($"⚠ Auto-fetch error: {ex.Message}", "error");
            }
        }
        public async Task AcsEvents(DateTime startDate, DateTime endDate)
        {
            ShowMessage($"Gathering Events {startDate:dd-MMM-yy} to {endDate:dd-MMM-yy}, Last {EConfig.Historical_Days} Days.", "info", StartNewMessageGroup: true);

            if (Devices == null || Devices.Count == 0)
            {
                ShowMessage("No devices available to process.", "warning");
                return;
            }

            int totalDevices = Devices.Count;
            int processedDevices = 0;
            int failedDevices = 0;

            foreach (var dv in Devices)
            {
                try
                {
                    ShowMessage($"Processing device {++processedDevices}/{totalDevices}: {dv.DeviceIpAddress}", "process");

                    using var hik = new HikvisionApi(dv.DeviceIpAddress, dv.UserName, dv.Password);

                    Log.Information("Starting event pull for device {Device}", dv.DeviceIpAddress);
                    ShowMessage($"INFO: Pulling events from device {dv.DeviceIpAddress}...", "info");

                    string res = await hik.GetAccessEventsAsync(startDate, endDate);
                    Root result = HikvisionEventParser.ParseAcsEventJson(res);
                    int recordCount = result?.AcsEvent?.InfoList?.Count ?? 0;

                    if (recordCount == 0)
                    {
                        ShowMessage($"Scanner: {dv.DeviceIpAddress}, No records found.", "warning");
                        continue;
                    }

                    ShowMessage($"✅ Scanner: {dv.DeviceIpAddress}, Total Records: {recordCount:#,##0}", "success");
                    Log.Information("Device {Device} returned {Count} events", dv.DeviceIpAddress, recordCount);

                    const int batchSize = 100;
                    int processedCount = 0;

                    // Batch processing
                    for (int i = 0; i < result.AcsEvent.InfoList.Count; i += batchSize)
                    {
                        var batch = result.AcsEvent.InfoList.Skip(i).Take(batchSize).ToList();

                        foreach (var item in batch)
                        {
                            try
                            {
                                await Save(item, dv.DeviceIpAddress);
                                processedCount++;

                                //lblCount.Invoke(new Action(() => lblCount.Text = processedCount.ToString()));
                            }
                            catch (Exception ex)
                            {
                                ShowMessage($"❌ Error saving event from {dv.DeviceIpAddress}: {ex.Message}", "error", ex.Message);
                                Log.Error(ex, "Failed to save event for device {Device}", dv.DeviceIpAddress);
                            }
                        }
                    }

                    ShowMessage($"💾 Saved: {processedCount:#,##0} records from Scanner {dv.DeviceIpAddress}.", "success");
                    Log.Information("SUCCESS: Scanner: {Device}, Total Records Saved: {Count}", dv.DeviceIpAddress, processedCount);
                }
                //catch (OperationCanceledException)
                //{
                //    ShowMessage($"❌ Cancelled while processing device {dv.DeviceIpAddress}.", "warning");
                //    Log.Warning("Operation cancelled for {Device}", dv.DeviceIpAddress);
                //    failedDevices++;
                //    break;
                //}
                catch (Exception ex)
                {
                    ShowMessage($"❌ Error processing device {dv.DeviceIpAddress}: {ex.Message}", "error", ex.Message);
                    Log.Error(ex, $"Exception while processing device {dv.DeviceIpAddress}");
                    failedDevices++;
                    continue;
                }
            }

            // Summary
            if (failedDevices > 0)
            {
                ShowMessage($"Processing completed. {processedDevices - failedDevices}/{totalDevices} devices successful, {failedDevices} failed.", "warning");
            }
            else
            {
                ShowMessage($"Processing completed. All {totalDevices} devices processed successfully.", "success");
            }
        }

        public async Task Save(Info item, string deviceIp)
        {
            // Helper: safely escape quotes
            string Escape(string? val) => string.IsNullOrEmpty(val) ? "" : val.Replace("'", "''");

            string cardNo = Escape(item.CardNo);
            string personName = Escape(item.Name);
            string empNo = Escape(item.EmployeeNoString);
            string verifyMode = Escape(item.CurrentVerifyMode);
            string maskDetected = (item.Mask?.ToLower() == "yes" || item.Mask == "1") ? "1" : "0";

            // ✅ Generate unique event ID
            string uniqueSource = $"{deviceIp}-{item.SerialNo}-{item.Time:yyyyMMddHHmmss}-{empNo}";
            string eventUid;
            using (var md5 = MD5.Create())
            {
                var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(uniqueSource));
                eventUid = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }

            //✅ Build SQL query (skips duplicates automatically)
            string sql = $@"
                        INSERT INTO acsevent (
                            event_uid,
                            event_time,
                            device_name,
                            device_id,
                            major,
                            minor,
                            card_no,
                            person_name,
                            employee_no,
                            departmentid,
                            designationid,
                            verify_mode,
                            picture_url,
                            in_out_direction,
                            mask_detected,
                            temperature,
                            reader_id,
                            door_no,
                            created_at
                        )
                        SELECT
                            '{eventUid}',
                            '{item.Time:yyyy-MM-dd HH:mm:ss}',
                            '{Escape(item.Name)}',
                            '{Escape(deviceIp)}',
                            {item.Major},
                            {item.Minor},
                            '{cardNo}',
                            '{personName}',
                            '{empNo}',
                            COALESCE(e.DepartmentId, 0),
                            COALESCE(e.DesignationId, 0),
                            '{verifyMode}',
                            '',
                            NULL,
                            {maskDetected},
                            NULL,
                            {item.CardReaderNo ?? 0},
                            {item.DoorNo},
                            NOW()
                        FROM (SELECT 1 AS dummy) d
                        LEFT JOIN Employees e ON e.Id = {empNo}
                        ON DUPLICATE KEY UPDATE
                            event_time = VALUES(event_time);";

            await dapper.Insert(sql);

            // 🔁 Run another update to fix missing department/designation IDs
            string fixSql = @"
                                UPDATE acsevent a
                                INNER JOIN employees e ON a.employee_no = e.EmpId
                                SET 
                                    a.departmentid = e.DepartmentId,
                                    a.designationid = e.DesignationId
                                WHERE 
                                    (a.departmentid = 0 OR a.designationid = 0);
                            ";


            await dapper.Update(fixSql);
        }

        DeviceInfoModel DeserializeDeviceInfo(string xml)
        {
            var serializer = new XmlSerializer(typeof(DeviceInfoModel));
            using var reader = new StringReader(xml);
            return (DeviceInfoModel)serializer.Deserialize(reader);
        }

        private void cmdStart_Click(object sender, EventArgs e)
        {
            if (!_isAuthorized)
                return;

            if (!EventsTimer.Enabled)
            {
                EventsTimer.Start();
                ShowMessage("🔄 Sync started.", "success");
                UpdateButtonStates(true);
            }
        }



        private async Task GetDeviceInfoAsync(ScannerDevicesModel dv)
        {
           
            string url = $"http://{dv.DeviceIpAddress}/ISAPI/System/deviceInfo";
            int maxRetries = 3;

            ShowMessage($"Getting device info from {dv.DeviceIpAddress}");

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    using var handler = new HttpClientHandler
                    {
                        Credentials = new NetworkCredential(dv.UserName.Trim(), dv.Password.Trim())
                    };

                    using var client = new HttpClient(handler)
                    {
                        Timeout = TimeSpan.FromSeconds(10)
                    };

                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string strResponseData = await response.Content.ReadAsStringAsync();

                        DeviceInfoModel info = DeserializeDeviceInfo(strResponseData);
                        DevicesInfo.Add(info);

                        ShowMessage(!string.IsNullOrEmpty(strResponseData)
                            ? "Device response OK."
                            : "No response received from device.",
                            !string.IsNullOrEmpty(strResponseData) ? "success" : "warning");

                        break; // Exit retry loop on success
                    }
                    else
                    {
                        ShowMessage($"HTTP {response.StatusCode} at {url}. Attempt {attempt} of {maxRetries}", "warning");

                        if (attempt == maxRetries)
                        {
                            ShowMessage($"Failed to get device info after {maxRetries} attempts.", "error");
                        }
                        else
                        {
                            await Task.Delay(1000); // wait before retry
                        }
                    }
                }
                //catch (HttpRequestException hre)
                //{
                //    ShowMessage($"Request exception at {url}: {hre.Message}. Attempt {attempt} of {maxRetries}", "error");
                //    if (attempt < maxRetries) await Task.Delay(1000);
                //}
                catch (Exception ex)
                {
                    ShowMessage($"Unexpected error at {dv.DeviceIpAddress}: {ex.Message}. Attempt {attempt} of {maxRetries}", "error");
                    if (attempt < maxRetries) await Task.Delay(1000);
                }
            }
        }

        private async void EventsTimer_Tick(object sender, EventArgs e)
        {
            // Stop timer immediately to prevent overlapping executions
            EventsTimer.Stop();

            try
            {
                DateTime end = DateTime.Now;
                DateTime start = DateTime.Today.AddDays(EConfig.Historical_Days * -1);

                // Process all devices
                await AcsEvents(start, end);

                _lastSuccessfulRun = DateTime.Now;
            }
            catch (Exception ex)
            {
                ShowMessage($"Auto-fetch error at {DateTime.Now:HH:mm:ss}: {ex.Message}", "error");
            }
            finally
            {
                // Always restart timer after a fixed delay to ensure consistent timing
                if (!this.IsDisposed && this.IsHandleCreated)
                {
                    // Use a delay to prevent immediate retry on errors
                    await Task.Delay(5000); // 5 second delay before restart
                    this.BeginInvoke(new Action(() =>
                    {
                        if (!this.IsDisposed && _isAuthorized)
                        {
                            EventsTimer.Start();
                            ShowMessage("Timer restarted. Next sync scheduled.", "debug");
                        }
                    }));
                }
            }
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            if (EventsTimer.Enabled)
            {
                EventsTimer.Stop();
                ShowMessage("⏸ Sync paused.", "warning");
                UpdateButtonStates(false);
            }
        }


        public void ShowMessage(string message, string messageType = "info", string EmailMessage="", bool StartNewMessageGroup=false)
        {
            if (txtMessages.InvokeRequired)
            {
                txtMessages.BeginInvoke(new Action(() => ShowMessage(message, messageType)));
                return;
            }

            string dateTime = DateTime.Now.ToString("dd-MMM hh:mm tt");

            // Pick a color and icon for each message type
            Color color;
            string icon;

            switch (messageType.ToLower())
            {
                case "error":
                    color = Color.IndianRed;
                    icon = "🚫 ";   // clearer error icon
                    break;
                case "success":
                    color = Color.LightGreen;
                    icon = "🎯 ";   // success/target achieved
                    break;
                case "warning":
                    color = Color.Gold;
                    icon = "⚠️ ";   // standard warning
                    break;
                case "info":
                    color = Color.Cyan;
                    icon = "💡 ";   // helpful info icon
                    break;
                case "debug":
                    color = Color.LightGray;
                    icon = "🧩 ";   // debug or detail
                    break;
                case "process":
                    color = Color.White;
                    icon = "⏳ ";   // processing/loading
                    break;
                default:
                    color = Color.Yellow;
                    icon = "📢 ";   // general notification
                    break;
            }


            if (StartNewMessageGroup == true)
            {
                txtMessages.SelectionColor = Color.White;
                txtMessages.SelectedText = $"";
            }
            
            // ✅ Prevent duplicate icon: check if message already starts with any known icon
            string[] knownIcons = { "❌", "✅", "⚠️", "ℹ️", "🔍", "📢", "📧", "▶", "⏱" };
            if (knownIcons.Any(i => message.TrimStart().StartsWith(i)))
                icon = ""; // Don’t prepend another icon

            string formattedMessage = $"{dateTime} {icon}{message}\r\n";

            // Insert at the beginning
            txtMessages.SelectionStart = 0;
            txtMessages.SelectionLength = 0;

            txtMessages.SelectionColor = Color.White;
            txtMessages.SelectedText = $"{dateTime} ";

            txtMessages.SelectionColor = color;
            txtMessages.SelectedText = $"{icon}{message}\r\n";

            txtMessages.SelectionColor = txtMessages.ForeColor;
            txtMessages.ScrollToCaret();
            Log.Information($"{messageType.ToUpper()}: {message}");

            if (EConfig.Email_Log == 1)
            {
                if (messageType.Equals("error", StringComparison.OrdinalIgnoreCase))
                {
                    _ = Config.sendEmail(
                        "aaamirrr2000@gmail.com",
                        $"MoITT HikConnect {messageType}",
                        $"❌ Error: {formattedMessage}" +
                        (!string.IsNullOrEmpty(EmailMessage) ? $"\r\n❌ System Error: {EmailMessage}" : "")
                    );
                }
            }
        }


        private void UpdateButtonStates(bool isRunning)
        {
            if (!_isAuthorized)
            {
                cmdStart.Visible = false;
                btnPause.Visible = false;
                btnClose.Visible = true;
                return;
            }

            cmdStart.Visible = !isRunning;  // show when stopped
            btnPause.Visible = isRunning;   // show when running
            btnClose.Visible = true;        // always visible
        }


        private void btnClose_Click(object sender, EventArgs e)
        {
            EventsTimer.Stop();
            ShowMessage("Application closing...", "info");
            Close();
        }


        private void frmHikSync_FormClosing(object sender, FormClosingEventArgs e)
        {
            EventsTimer?.Stop();
            EventsTimer?.Dispose();
            _watchdogTimer?.Stop();
            _watchdogTimer?.Dispose();
            _httpClient?.Dispose();

            base.OnFormClosing(e);
        }
    }


}