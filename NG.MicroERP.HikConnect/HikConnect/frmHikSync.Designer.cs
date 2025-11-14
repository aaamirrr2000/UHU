namespace HikConnect
{
    partial class frmHikSync
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            txtMessages = new RichTextBox();
            cmdStart = new Button();
            btnPause = new Button();
            lblTimer = new Label();
            lblTitle = new Label();
            EventsTimer = new System.Windows.Forms.Timer(components);
            panel1 = new Panel();
            flowLayoutPanel1 = new FlowLayoutPanel();
            btnClose = new Button();
            panel1.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // txtMessages
            // 
            txtMessages.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtMessages.BackColor = Color.Black;
            txtMessages.BorderStyle = BorderStyle.None;
            txtMessages.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtMessages.ForeColor = Color.Yellow;
            txtMessages.Location = new Point(7, 7);
            txtMessages.Name = "txtMessages";
            txtMessages.ReadOnly = true;
            txtMessages.Size = new Size(567, 623);
            txtMessages.TabIndex = 1;
            txtMessages.Text = "";
            txtMessages.WordWrap = false;
            // 
            // cmdStart
            // 
            cmdStart.BackColor = Color.FromArgb(64, 64, 64);
            cmdStart.FlatAppearance.BorderSize = 0;
            cmdStart.FlatStyle = FlatStyle.Flat;
            cmdStart.Font = new Font("Segoe UI", 9.75F);
            cmdStart.ForeColor = Color.White;
            cmdStart.Location = new Point(3, 3);
            cmdStart.Name = "cmdStart";
            cmdStart.Size = new Size(99, 28);
            cmdStart.TabIndex = 2;
            cmdStart.Text = "▶ Start";
            cmdStart.UseVisualStyleBackColor = false;
            cmdStart.Click += cmdStart_Click;
            // 
            // btnPause
            // 
            btnPause.BackColor = Color.FromArgb(64, 64, 64);
            btnPause.FlatAppearance.BorderSize = 0;
            btnPause.FlatStyle = FlatStyle.Flat;
            btnPause.Font = new Font("Segoe UI", 9.75F);
            btnPause.ForeColor = Color.White;
            btnPause.Location = new Point(108, 3);
            btnPause.Name = "btnPause";
            btnPause.Size = new Size(99, 28);
            btnPause.TabIndex = 3;
            btnPause.Text = "⏸ Pause";
            btnPause.UseVisualStyleBackColor = false;
            btnPause.Click += btnPause_Click;
            // 
            // lblTimer
            // 
            lblTimer.BackColor = Color.Black;
            lblTimer.Font = new Font("Segoe UI", 9.75F);
            lblTimer.ForeColor = Color.FromArgb(255, 255, 128);
            lblTimer.Location = new Point(14, 669);
            lblTimer.Name = "lblTimer";
            lblTimer.Size = new Size(298, 21);
            lblTimer.TabIndex = 5;
            lblTimer.Text = "Next Sync: --:--";
            lblTimer.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblTitle
            // 
            lblTitle.BackColor = Color.FromArgb(10, 132, 255);
            lblTitle.Font = new Font("Microsoft Sans Serif", 18F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(0, -2);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(581, 80);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "📡 Scanner Sync";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            lblTitle.MouseDown += Form_MouseDown;
            lblTitle.MouseMove += Form_MouseMove;
            lblTitle.MouseUp += Form_MouseUp;
            // 
            // EventsTimer
            // 
            EventsTimer.Enabled = true;
            EventsTimer.Interval = 60000;
            EventsTimer.Tick += EventsTimer_Tick;
            // 
            // panel1
            // 
            panel1.BackColor = Color.Black;
            panel1.Controls.Add(lblTimer);
            panel1.Controls.Add(flowLayoutPanel1);
            panel1.Controls.Add(txtMessages);
            panel1.Location = new Point(0, 78);
            panel1.Name = "panel1";
            panel1.Size = new Size(579, 695);
            panel1.TabIndex = 7;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(cmdStart);
            flowLayoutPanel1.Controls.Add(btnPause);
            flowLayoutPanel1.Controls.Add(btnClose);
            flowLayoutPanel1.Location = new Point(12, 633);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(324, 40);
            flowLayoutPanel1.TabIndex = 8;
            // 
            // btnClose
            // 
            btnClose.BackColor = Color.FromArgb(64, 64, 64);
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Font = new Font("Segoe UI", 9.75F);
            btnClose.ForeColor = Color.White;
            btnClose.Location = new Point(213, 3);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(99, 28);
            btnClose.TabIndex = 7;
            btnClose.Text = "🚪 Close";
            btnClose.UseVisualStyleBackColor = false;
            btnClose.Click += btnClose_Click;
            // 
            // frmHikSync
            // 
            BackColor = Color.Black;
            ClientSize = new Size(582, 775);
            Controls.Add(panel1);
            Controls.Add(lblTitle);
            Font = new Font("Microsoft Sans Serif", 10F);
            FormBorderStyle = FormBorderStyle.None;
            Name = "frmHikSync";
            StartPosition = FormStartPosition.CenterScreen;
            Load += frmHikSync_Load;
            MouseDown += Form_MouseDown;
            MouseMove += Form_MouseMove;
            MouseUp += Form_MouseUp;
            panel1.ResumeLayout(false);
            flowLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        // Rounded corners P/Invoke
        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, int nTopRect, int nRightRect, int nBottomRect,
            int nWidthEllipse, int nHeightEllipse);

        // Drag form anywhere
        private bool dragging = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;

        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            dragCursorPoint = Cursor.Position;
            dragFormPoint = this.Location;
        }

        private void Form_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point diff = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                this.Location = Point.Add(dragFormPoint, new Size(diff));
            }
        }

        private void Form_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private RichTextBox txtMessages;
        private Button cmdStart;
        private Button btnPause;
        private Label lblTitle;
        private Label lblTimer;
        private System.Windows.Forms.Timer EventsTimer;
        private Panel panel1;
        private Button btnClose;
        private FlowLayoutPanel flowLayoutPanel1;
    }
}
