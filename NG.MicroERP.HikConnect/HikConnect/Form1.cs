using Google.Protobuf.WellKnownTypes;
using HikConnect.Models;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using NG.MicroERP.API.Helper;
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

        List<ScannerDeviceModel> Devices = new List<ScannerDeviceModel>();
        List<DeviceInfoModel> DevicesInfo = new List<DeviceInfoModel>();
        DapperFunctions dapper = new DapperFunctions();

        public frmHikSync()
        {
            InitializeComponent();
            btnGetEvents_Click(null, null);

        }

        private async Task DevicesListAsync()
        {
            string SQL = $@"SELECT * FROM ScannerDevices Where IsActive=1";
            Devices = (await dapper.SearchByQuery<ScannerDeviceModel>(SQL)) ?? new List<ScannerDeviceModel>();

            lstDevices.Items.Clear();
            foreach (var dv in Devices)
            {
                lstDevices.Items.Add($"{dv.DeviceIpAddress} ({dv.UserName})");
            }
        }

        //Get access event data

        public async Task AcsEvents(DateTime startDate, DateTime endDate) // Change from async void to async Task
        {
            try
            {
                // Use Invoke for thread-safe UI updates


                txtMessages.Text += $"\r\n{DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss tt")} - Getting Data between {startDate.ToString("dd-MMM-yyyy")} and {endDate.ToString("dd-MMM-yyyy")}\r\n";

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
                        string message = $"Scanner: {dv.DeviceIpAddress}, Total Records: {result.AcsEvent.InfoList.Count.ToString("#,##0")}\r\n";
                        if (txtMessages.InvokeRequired)
                            txtMessages.Invoke(new Action(() => txtMessages.Text += message));
                        else
                            txtMessages.Text += message;

                        // Don't return early - continue to next device even if no data
                        if (result?.AcsEvent?.InfoList == null || result.AcsEvent.InfoList.Count == 0)
                        {
                            // Log and continue to next device
                            string noDataMessage = $"No data for device {dv.DeviceIpAddress}, moving to next device.\r\n";
                            if (txtMessages.InvokeRequired)
                                txtMessages.Invoke(new Action(() => txtMessages.Text += noDataMessage));
                            else
                                txtMessages.Text += noDataMessage;

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
                    catch (TaskCanceledException)
                    {
                        string timeoutMessage = $"Timeout processing device {dv.DeviceIpAddress}, moving to next device.\r\n";
                        if (txtMessages.InvokeRequired)
                            txtMessages.Invoke(new Action(() => txtMessages.Text += timeoutMessage));
                        else
                            txtMessages.Text += timeoutMessage;
                        continue;
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = $"Error processing device {dv.DeviceIpAddress}: {ex.Message}\r\n";
                        if (txtMessages.InvokeRequired)
                            txtMessages.Invoke(new Action(() => txtMessages.Text += errorMessage));
                        else
                            txtMessages.Text += errorMessage;
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
                string errorMessage = $"Unexpected error: {ex.Message}\r\n";
                if (txtMessages.InvokeRequired)
                    txtMessages.Invoke(new Action(() => txtMessages.Text += errorMessage));
                else
                    txtMessages.Text += errorMessage;

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

            // ✅ Build SQL query (skips duplicates automatically)
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
                            verify_mode,
                            picture_url,
                            in_out_direction,
                            mask_detected,
                            temperature,
                            reader_id,
                            door_no,
                            created_at
                        ) VALUES (
                            '{eventUid}',
                            '{item.Time:yyyy-MM-dd HH:mm:ss}',
                            '{Escape(item.Name)}',
                            '{Escape(deviceIp)}',
                            {item.Major},
                            {item.Minor},
                            '{cardNo}',
                            '{personName}',
                            '{empNo}',
                            '{verifyMode}',
                            '',     -- picture_url not available
                            NULL,   -- in_out_direction unknown
                            {maskDetected},
                            NULL,   -- temperature not available
                            {item.CardReaderNo ?? 0},
                            {item.DoorNo},
                            NOW()
                        )
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
                txtMessages.Text += $"Timer started at {DateTime.Now:HH:mm:ss}\r\n";
            }
            else
            {
                txtMessages.Text += $"Timer was already running at {DateTime.Now:HH:mm:ss}\r\n";
            }
        }

        private void frmHikSync_Load(object sender, EventArgs e)
        {
            DevicesListAsync();
            lblTimer.Text = $"{EventsTimer.Interval / 60000} minutes";
        }

        private void btnDevicesInfo_Click(object sender, EventArgs e)
        {
            txtMessages.Text = "";
            foreach (var dv in Devices)
            {
                txtMessages.Text += $":: Getting Device Information from {dv.DeviceIpAddress}\r\n";
                string strUrl = $"http://{dv.DeviceIpAddress}/ISAPI/System/deviceInfo";
                using (WebClient client = new WebClient())
                {
                    client.Credentials = new NetworkCredential(dv.UserName, dv.Password);
                    client.Headers.Add("Accept", "application/json");
                    client.Headers.Add("Content-Type", "application/json; charset=UTF-8");

                    byte[] responseData = client.DownloadData(strUrl);
                    string strResponseData = Encoding.UTF8.GetString(responseData);

                    txtMessages.Text += strResponseData + "\r\n";

                    DeviceInfoModel info = DeserializeDeviceInfo(strResponseData);
                    DevicesInfo.Add(info);

                }
            }
        }

        private async void EventsTimer_Tick(object sender, EventArgs e)
        {
            EventsTimer.Stop();

            try
            {
                DateTime end = DateTime.Now;
                DateTime start = DateTime.Today.AddDays(-2);

                lblStatus.Text = $"Auto-fetching events from {start:yyyy-MM-dd HH:mm:ss} to {end:yyyy-MM-dd HH:mm:ss}";
                await AcsEvents(start, end);

                lblStatus.Text = $"Last auto-fetch: {DateTime.Now:HH:mm:ss}";
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Auto-fetch error: {ex.Message}";
                txtMessages.Text += $"Auto-fetch error at {DateTime.Now:HH:mm:ss}: {ex.Message}\r\n";
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
    }
}