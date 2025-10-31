using Google.Protobuf.WellKnownTypes;
using HikConnect.Models;
using MySql.Data.MySqlClient;
using Mysqlx.Expr;
using Newtonsoft.Json;
using NG.MicroERP.API.Helper;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace HikConnect
{
    public partial class frmHikSync : Form
    {

        List<ScannerDevicesModel> Devices = new List<ScannerDevicesModel>();
        List<DeviceInfoModel> DevicesInfo = new List<DeviceInfoModel>();
        DapperFunctions dapper = new DapperFunctions();
        List<string> AllowedPCs = new List<string>
        {
                  "BFEBFBFF000906ED-C021096013SNK221N"
        };


        public frmHikSync()
        {
            InitializeComponent();

            Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));

            //foreach (Control ctrl in Controls)
            //{
            //    EnableDrag(ctrl);
            //}


            // Check if the current PC is allowed
            string PCId = GetPCUniqueId();
            bool isAllowed = false;

            foreach (var pc in AllowedPCs)
            {
                if (pc.Equals(PCId, StringComparison.OrdinalIgnoreCase))
                {
                    isAllowed = true;
                    break;
                }
            }

            if (isAllowed)
            {
                btnGetEvents_Click(null, null);
            }
            else
            {
                ShowMessage($"System Error: Unauthorized PC ({PCId})");
            }
        }

        private async Task DevicesListAsync()
        {
            string SQL = "SELECT * FROM ScannerDevices WHERE IsActive=1";
            Devices = (await dapper.SearchByQuery<ScannerDevicesModel>(SQL)) ?? new List<ScannerDevicesModel>();

            foreach (var dv in Devices)
            {
                ShowMessage($"Device: {dv.DeviceIpAddress} ({dv.UserName})");
                await GetDeviceInfoAsync(dv);
            }
        }


        //Get access event data
        public async Task AcsEvents(DateTime startDate, DateTime endDate)
        {
            try
            {
                ShowMessage($"Getting Data {startDate.ToString("dd-MMM-yyyy")} to {endDate.ToString("dd-MMM-yyyy")}");

                if (lblStatus.InvokeRequired)
                    lblStatus.Invoke(new Action(() => lblStatus.Text = "Getting Events..."));
                else
                    lblStatus.Text = "Getting Events...";

                var AllDevicesData = new StringBuilder();
                int i = 1;

                foreach (var dv in Devices)
                {
                    // Add cancellation token support
                    var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(2));

                    try
                    {
                        var hik = new HikvisionApi(dv.DeviceIpAddress, dv.UserName, dv.Password);

                        // Pass cancellation token to prevent hanging
                        string res = await hik.GetAccessEventsAsync(startDate, endDate);

                        Root result = HikvisionEventParser.ParseAcsEventJson(res);

                        // Thread-safe UI update
                        string message = $"Scanner: {dv.DeviceIpAddress}, Total Records: {result.AcsEvent.InfoList.Count.ToString("#,##0")}";
                        ShowMessage(message);

                        // Don't return early - continue to next device even if no data
                        if (result?.AcsEvent?.InfoList == null || result.AcsEvent.InfoList.Count == 0)
                        {
                            // Log and continue to next device
                            string noDataMessage = $"No data for device {dv.DeviceIpAddress}, moving to next device.";
                            ShowMessage(noDataMessage, "warning");
                            continue;
                        }

                        foreach (var item in result.AcsEvent.InfoList)
                        {
                            string name = item.Name?.Trim() ?? "";
                            string verify = item.CurrentVerifyMode?.Trim() ?? "";

                            Save(item, dv.DeviceIpAddress);

                        }

                        await Task.Delay(1000);
                    }
                    catch (TaskCanceledException ex)
                    {
                        ShowMessage(ex.Message);
                        continue;
                    }
                    catch (Exception ex)
                    {
                        ShowMessage(ex.Message);
                        continue;
                    }
                }

                // Final status update
                if (lblStatus.InvokeRequired)
                    lblStatus.Invoke(new Action(() => lblStatus.Text = "Completed"));
                else
                    lblStatus.Text = "Completed";


            }
            catch (Exception ex)
            {
                // Handle any unexpected errors
                ShowMessage(ex.Message);

                if (lblStatus.InvokeRequired)
                    lblStatus.Invoke(new Action(() => lblStatus.Text = "Error"));
                else
                    lblStatus.Text = "Error";
            }
        }

        public void Save(Info item, string deviceIp)
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

            dapper.Insert(sql);
        }

        DeviceInfoModel DeserializeDeviceInfo(string xml)
        {
            var serializer = new XmlSerializer(typeof(DeviceInfoModel));
            using var reader = new StringReader(xml);
            return (DeviceInfoModel)serializer.Deserialize(reader);
        }

        private async void btnGetEvents_Click(object sender, EventArgs e)
        {
            if (!EventsTimer.Enabled)
            {
                EventsTimer.Start();
                lblStatus.Text = "Timer started - waiting for first tick...";
                ShowMessage($"Timer started at {DateTime.Now:HH:mm:ss}");
            }
            else
            {
                ShowMessage($"Timer was already running at {DateTime.Now:HH:mm:ss}");
            }
        }

        private void frmHikSync_Load(object sender, EventArgs e)
        {
            DevicesListAsync();
            lblTimer.Text = $"{EventsTimer.Interval / 60000} minutes";
        }

        private async Task GetDeviceInfoAsync(ScannerDevicesModel dv)
        {
            string strUrl = $"http://{dv.DeviceIpAddress}/ISAPI/System/deviceInfo";

            try
            {
                using var handler = new HttpClientHandler
                {
                    Credentials = new NetworkCredential(dv.UserName, dv.Password)
                };

                using var httpClient = new HttpClient(handler)
                {
                    Timeout = TimeSpan.FromSeconds(10)
                };

                ShowMessage($"Getting device info from {dv.DeviceIpAddress}");

                var response = await httpClient.GetAsync(strUrl);

                if (!response.IsSuccessStatusCode)
                {
                    ShowMessage($"HTTP {response.StatusCode} - {response.ReasonPhrase}", "error");
                    return;
                }

                string strResponseData = await response.Content.ReadAsStringAsync();
                ShowMessage($"Response received from {dv.DeviceIpAddress}", "success");

                DeviceInfoModel info = DeserializeDeviceInfo(strResponseData);
                DevicesInfo.Add(info);

                // Optional: limit log size or truncate long responses
                if (strResponseData.Length > 1000)
                    strResponseData = strResponseData.Substring(0, 1000) + "...";

                ShowMessage(strResponseData, "info");
            }
            catch (TaskCanceledException)
            {
                ShowMessage($"Timeout while contacting {dv.DeviceIpAddress}", "warning");
            }
            catch (HttpRequestException ex)
            {
                ShowMessage($"Network error contacting {dv.DeviceIpAddress}: {ex.Message}", "error");
            }
            catch (Exception ex)
            {
                ShowMessage($"Unexpected error {dv.DeviceIpAddress}: {ex.Message}", "error");
            }
        }


        private async void EventsTimer_Tick(object sender, EventArgs e)
        {
            EventsTimer.Stop();

            try
            {
                DateTime end = DateTime.Now;
                DateTime start = DateTime.Today.AddDays(-31);

                lblStatus.Text = $"Auto-fetching events from {start:yyyy-MM-dd HH:mm:ss} to {end:yyyy-MM-dd HH:mm:ss}";

                // Run heavy tasks on a background thread
                await Task.Run(async () =>
                {
                    await AcsEvents(start, end);
                });

                // UI updates always on main thread
                lblStatus.Text = $"Last auto-fetch: {DateTime.Now:HH:mm:ss}";
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Auto-fetch error: {ex.Message}";
                ShowMessage($"Auto-fetch error at {DateTime.Now:HH:mm:ss}: {ex.Message}", "error");
            }
            finally
            {
                EventsTimer.Start();
            }
        }


        private void btnPause_Click(object sender, EventArgs e)
        {
            EventsTimer.Stop();
        }

        private void lblStatus_Click(object sender, EventArgs e)
        {

        }

        public static string GetPCUniqueId()
        {
            try
            {
                string cpuId = GetCpuId();
                string motherboardId = GetMotherboardId();

                // Combine them to generate a unique identifier
                string uniqueId = cpuId + "-" + motherboardId;
                return uniqueId;
            }
            catch
            {
                return "UNKNOWN-PC";
            }
        }

        private static string GetCpuId()
        {
            string cpuId = string.Empty;
            ManagementClass mc = new ManagementClass("win32_processor");
            ManagementObjectCollection moc = mc.GetInstances();

            foreach (ManagementObject mo in moc)
            {
                cpuId = mo.Properties["processorID"].Value.ToString();
                break;
            }

            return cpuId;
        }

        private static string GetMotherboardId()
        {
            string motherboardId = string.Empty;
            ManagementClass mc = new ManagementClass("Win32_BaseBoard");
            ManagementObjectCollection moc = mc.GetInstances();

            foreach (ManagementObject mo in moc)
            {
                motherboardId = mo.Properties["SerialNumber"].Value.ToString();
                break;
            }

            return motherboardId;
        }

        private void EnableDrag(Control control)
        {
            control.MouseDown += Form_MouseDown;
            control.MouseMove += Form_MouseMove;
            control.MouseUp += Form_MouseUp;
        }

        public void ShowMessage(string message, string messageType = "info")
        {
            if (txtMessages.InvokeRequired)
            {
                txtMessages.BeginInvoke(new Action(() => ShowMessage(message, messageType)));
                return;
            }

            string dateTime = DateTime.Now.ToString("dd-MMM-yyyy hh:mm:ss tt");

            Color color = messageType.ToLower() switch
            {
                "error" => Color.IndianRed,
                "success" => Color.LightGreen,
                "warning" => Color.Gold,
                "info" => Color.Cyan,
                _ => Color.Yellow
            };

            // Build formatted message
            string formattedMessage = $"{dateTime} {message}\r\n";

            // Insert at the beginning
            txtMessages.SelectionStart = 0;
            txtMessages.SelectionLength = 0;

            txtMessages.SelectionColor = Color.White;
            txtMessages.SelectedText = $"{dateTime} ";

            txtMessages.SelectionColor = color;
            txtMessages.SelectedText = $"{message}\r\n";

            txtMessages.SelectionColor = txtMessages.ForeColor;
            txtMessages.ScrollToCaret();
        }


    }
}