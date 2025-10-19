namespace HikConnect
{
    partial class frmHikSync
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            txtMessages = new RichTextBox();
            cmdStart = new Button();
            lstDevices = new ListBox();
            btnDevicesInfo = new Button();
            lblStatus = new Label();
            EventsTimer = new System.Windows.Forms.Timer(components);
            btnPause = new Button();
            lblTimer = new Label();
            SuspendLayout();
            // 
            // txtMessages
            // 
            txtMessages.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtMessages.BackColor = Color.Black;
            txtMessages.BorderStyle = BorderStyle.None;
            txtMessages.Font = new Font("Courier New", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtMessages.ForeColor = Color.FromArgb(255, 255, 128);
            txtMessages.Location = new Point(143, 3);
            txtMessages.Name = "txtMessages";
            txtMessages.Size = new Size(783, 364);
            txtMessages.TabIndex = 0;
            txtMessages.Text = "";
            txtMessages.WordWrap = false;
            // 
            // cmdStart
            // 
            cmdStart.Location = new Point(5, 315);
            cmdStart.Name = "cmdStart";
            cmdStart.Size = new Size(130, 23);
            cmdStart.TabIndex = 1;
            cmdStart.Text = "Events";
            cmdStart.UseVisualStyleBackColor = true;
            cmdStart.Click += btnGetEvents_Click;
            // 
            // lstDevices
            // 
            lstDevices.BackColor = Color.Black;
            lstDevices.BorderStyle = BorderStyle.None;
            lstDevices.ForeColor = Color.FromArgb(255, 128, 128);
            lstDevices.FormattingEnabled = true;
            lstDevices.ItemHeight = 15;
            lstDevices.Location = new Point(5, 3);
            lstDevices.Name = "lstDevices";
            lstDevices.Size = new Size(132, 270);
            lstDevices.TabIndex = 3;
            // 
            // btnDevicesInfo
            // 
            btnDevicesInfo.Location = new Point(5, 286);
            btnDevicesInfo.Name = "btnDevicesInfo";
            btnDevicesInfo.Size = new Size(130, 23);
            btnDevicesInfo.TabIndex = 6;
            btnDevicesInfo.Text = "Devices Info";
            btnDevicesInfo.UseVisualStyleBackColor = true;
            btnDevicesInfo.Click += btnDevicesInfo_Click;
            // 
            // lblStatus
            // 
            lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblStatus.ForeColor = Color.Red;
            lblStatus.Location = new Point(143, 373);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(22, 21);
            lblStatus.TabIndex = 7;
            lblStatus.Text = "...";
            // 
            // EventsTimer
            // 
            EventsTimer.Interval = 300000;
            EventsTimer.Tick += EventsTimer_Tick;
            // 
            // btnPause
            // 
            btnPause.Location = new Point(5, 344);
            btnPause.Name = "btnPause";
            btnPause.Size = new Size(130, 23);
            btnPause.TabIndex = 8;
            btnPause.Text = "Pause";
            btnPause.UseVisualStyleBackColor = true;
            btnPause.Click += btnPause_Click;
            // 
            // lblTimer
            // 
            lblTimer.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblTimer.AutoSize = true;
            lblTimer.Font = new Font("Segoe UI Light", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblTimer.ForeColor = Color.Green;
            lblTimer.Location = new Point(5, 375);
            lblTimer.Name = "lblTimer";
            lblTimer.Size = new Size(22, 21);
            lblTimer.TabIndex = 9;
            lblTimer.Text = "...";
            // 
            // frmHikSync
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(929, 403);
            Controls.Add(lblTimer);
            Controls.Add(btnPause);
            Controls.Add(lblStatus);
            Controls.Add(btnDevicesInfo);
            Controls.Add(lstDevices);
            Controls.Add(cmdStart);
            Controls.Add(txtMessages);
            Name = "frmHikSync";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "HikConnect";
            Load += frmHikSync_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private RichTextBox txtMessages;
        private Button cmdStart;
        private ListBox lstDevices;
        private Button btnDevicesInfo;
        private Label lblStatus;
        private System.Windows.Forms.Timer EventsTimer;
        private Button btnPause;
        private Label lblTimer;
    }
}
