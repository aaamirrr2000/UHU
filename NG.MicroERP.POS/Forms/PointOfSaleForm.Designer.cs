namespace NG.MicroERP.POS.Forms
{
    partial class PointOfSaleForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Text = "PointOfSaleForm";

            this.Size = new Size(1200, 850);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            InitializePOSLayout();
        }

        private void InitializePOSLayout()
        {
            this.Text = "POS Terminal";
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.BackColor = Color.FromArgb(30, 30, 30);

            // Main container
            var mainContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
            };
            mainContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            mainContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            mainContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 90));
            mainContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));
            this.Controls.Add(mainContainer);

            // Tile Panel
            var tilePanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(45, 45, 48),
                Padding = new Padding(10)
            };

            string[] foodItems = { "Burger", "Steak", "Pizza", "Salad", "Tuna" };
            string[] prices = { "$9.50", "$18.00", "$12.00", "$8.00", "$15.00" };
            string[] icons = { "Images/New.png", "Images/New.png", "Images/New.png", "Images/New.png", "Images/New.png" };
            Color[] colors = { Color.DodgerBlue, Color.Crimson, Color.MediumSeaGreen, Color.Orange, Color.MediumVioletRed };

            for (int i = 0; i < foodItems.Length; i++)
            {
                var btn = new Button
                {
                    Text = $"{foodItems[i]}\n{prices[i]}",
                    Size = new Size(150, 120),
                    BackColor = colors[i % colors.Length],
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    TextAlign = ContentAlignment.BottomCenter,
                    FlatStyle = FlatStyle.Flat,
                    Image = Image.FromFile(icons[i]),
                    ImageAlign = ContentAlignment.TopCenter
                };
                tilePanel.Controls.Add(btn);
            }

            mainContainer.Controls.Add(tilePanel, 0, 0);

            // Right Panel with order list and totals
            var rightPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(50, 50, 55)
            };

            var dgvOrders = new DataGridView
            {
                Dock = DockStyle.Top,
                Height = 300,
                BackgroundColor = Color.White,
                GridColor = Color.LightGray,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 10),
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { Font = new Font("Segoe UI", 10, FontStyle.Bold) },
                DefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.White, ForeColor = Color.Black }
            };

            dgvOrders.Columns.Add("Item", "Item");
            dgvOrders.Columns.Add("Qty", "Qty");
            dgvOrders.Columns.Add("Price", "Price");
            dgvOrders.Columns.Add("Amount", "Amount");

            dgvOrders.Rows.Add("Burger", "1", "9.50", "9.50");
            dgvOrders.Rows.Add("Pizza", "2", "6.00", "12.00");

            rightPanel.Controls.Add(dgvOrders);

            // Totals Panel
            var totalPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 150,
                ColumnCount = 2,
                RowCount = 4,
                Padding = new Padding(10)
            };

            string[] labels = { "Subtotal", "Tax (10%)", "Discount", "Total" };
            string[] values = { "21.50", "2.15", "0.00", "23.65" };

            for (int i = 0; i < labels.Length; i++)
            {
                totalPanel.Controls.Add(new Label
                {
                    Text = labels[i],
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Dock = DockStyle.Fill
                }, 0, i);

                var txt = new TextBox
                {
                    Text = values[i],
                    ReadOnly = labels[i] != "Discount",
                    TextAlign = HorizontalAlignment.Right,
                    Font = new Font("Segoe UI", 12),
                    Dock = DockStyle.Fill
                };

                totalPanel.Controls.Add(txt, 1, i);
            }

            rightPanel.Controls.Add(totalPanel);
            mainContainer.Controls.Add(rightPanel, 1, 0);

            // Bottom shortcut bar
            var shortcutBar = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(35, 35, 38),
                Padding = new Padding(10),
                FlowDirection = FlowDirection.LeftToRight
            };

            string[] categories = { "Favorites", "Drinks", "Alcohol", "Starters", "Mains" };
            string[] iconsBar = { "Images/New.png", "Images/New.png", "Images/New.png", "Images/New.png", "Images/New.png" };

            for (int i = 0; i < categories.Length; i++)
            {
                var btn = new Button
                {
                    Text = categories[i],
                    Size = new Size(100, 70),
                    Font = new Font("Segoe UI", 8),
                    ForeColor = Color.White,
                    BackColor = Color.Gray,
                    FlatStyle = FlatStyle.Flat,
                    TextAlign = ContentAlignment.BottomCenter,
                    Image = Image.FromFile(iconsBar[i]),
                    ImageAlign = ContentAlignment.TopCenter
                };
                shortcutBar.Controls.Add(btn);
            }

            mainContainer.Controls.Add(shortcutBar, 0, 1);
            mainContainer.SetColumnSpan(shortcutBar, 2);
        }
    }
}