using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using LibreHardwareMonitor.Hardware;

public class CpuTempGauge : Form
{
    private float currentTemp = 0, peakTemp = 0;
    private Computer computer;
    private System.Timers.Timer readTimer;
    private Label lblCpuTag, lblTemp, lblPeak;
    private NotifyIcon trayIcon;
    private ContextMenuStrip trayMenu;
    private bool dragging = false;
    private Point dragStart;

    public CpuTempGauge()
    {
        this.Text = "CPU Temperature";
        this.FormBorderStyle = FormBorderStyle.None;
        this.Width = 180; this.Height = 72;
        this.TopMost = true; this.ShowInTaskbar = false;
        this.BackColor = Color.Black;
        this.Opacity = 0.40;

        var scr = Screen.PrimaryScreen;
        this.Left = scr.WorkingArea.Right - this.Width - 10;
        this.Top = 10;

        lblCpuTag = new Label();
        lblCpuTag.Text = "CPU";
        lblCpuTag.ForeColor = Color.White;
        lblCpuTag.Font = new Font("Microsoft YaHei", 10, FontStyle.Bold);
        lblCpuTag.TextAlign = ContentAlignment.MiddleLeft;
        lblCpuTag.Location = new Point(10, 0);
        lblCpuTag.Size = new Size(40, 44);
        lblCpuTag.BackColor = Color.Transparent;
        this.Controls.Add(lblCpuTag);

        lblTemp = new Label();
        lblTemp.Text = "--\u00b0C";
        lblTemp.ForeColor = Color.Lime;
        lblTemp.Font = new Font("Consolas", 26, FontStyle.Bold);
        lblTemp.TextAlign = ContentAlignment.MiddleRight;
        lblTemp.Location = new Point(0, 0);
        lblTemp.Size = new Size(175, 44);
        lblTemp.BackColor = Color.Transparent;
        this.Controls.Add(lblTemp);

        lblPeak = new Label();
        lblPeak.Text = "\u5cf0\u503c: --\u00b0C";
        lblPeak.ForeColor = Color.FromArgb(160, 255, 255, 255);
        lblPeak.Font = new Font("Consolas", 9);
        lblPeak.TextAlign = ContentAlignment.MiddleCenter;
        lblPeak.Location = new Point(0, 42);
        lblPeak.Size = new Size(158, 22);
        lblPeak.BackColor = Color.Transparent;
        this.Controls.Add(lblPeak);

        var closeBtn = new Label();
        closeBtn.Text = "\u00d7";
        closeBtn.ForeColor = Color.FromArgb(120, 255, 255, 255);
        closeBtn.Font = new Font("Consolas", 10);
        closeBtn.Location = new Point(158, 44);
        closeBtn.Size = new Size(18, 18);
        closeBtn.BackColor = Color.Transparent;
        closeBtn.Click += (object sb, EventArgs ev) => { this.Hide(); };
        this.Controls.Add(closeBtn);

        MouseEventHandler md = (object sb, MouseEventArgs ev) => {
            if (ev.Button == MouseButtons.Left) { dragging = true; dragStart = new Point(ev.X, ev.Y); }
        };
        MouseEventHandler mm = (object sb, MouseEventArgs ev) => {
            if (dragging) { this.Left += ev.X - dragStart.X; this.Top += ev.Y - dragStart.Y; }
        };
        MouseEventHandler mu = (object sb, MouseEventArgs ev) => { dragging = false; };

        this.MouseDown += md; this.MouseMove += mm; this.MouseUp += mu;
        lblCpuTag.MouseDown += md; lblCpuTag.MouseMove += mm; lblCpuTag.MouseUp += mu;
        lblTemp.MouseDown += md; lblTemp.MouseMove += mm; lblTemp.MouseUp += mu;
        lblPeak.MouseDown += md; lblPeak.MouseMove += mm; lblPeak.MouseUp += mu;

        trayIcon = new NotifyIcon();
        trayIcon.Icon = CreateTempIcon(0);
        trayIcon.Text = "CPU: --\u00b0C | \u5cf0\u503c: --\u00b0C";
        trayIcon.Visible = true;
        trayIcon.Click += (object sb, EventArgs ev) => {
            this.Visible = !this.Visible;
            if (this.Visible) { this.Activate(); this.TopMost = true; }
        };

        trayMenu = new ContextMenuStrip();
        var exitItem = trayMenu.Items.Add("\u9000\u51fa");
        exitItem.Click += (object sb, EventArgs ev) => {
            readTimer.Stop(); readTimer.Dispose();
            computer.Close(); trayIcon.Visible = false;
            Application.Exit();
        };
        trayIcon.ContextMenuStrip = trayMenu;

        computer = new Computer { IsCpuEnabled = true };
        computer.Open();

        var initTimer = new Timer();
        initTimer.Interval = 300;
        initTimer.Tick += delegate(object s, EventArgs e) { initTimer.Stop(); initTimer.Dispose(); ReadTemperature(); };
        initTimer.Start();

        readTimer = new System.Timers.Timer(3000);
        readTimer.Elapsed += (object s, System.Timers.ElapsedEventArgs e) => ReadTemperature();
        readTimer.AutoReset = true;
        readTimer.Start();
    }

    private void ReadTemperature()
    {
        try {
            foreach (var hw in computer.Hardware) {
                hw.Update();
                foreach (var se in hw.Sensors) {
                    if (se.SensorType == SensorType.Temperature && se.Value.HasValue && se.Name == "CPU Package") {
                        float t = se.Value.Value;
                        currentTemp = t;
                        if (t > peakTemp) { peakTemp = t; }
                        this.Invoke((MethodInvoker)delegate {
                            lblTemp.Text = Math.Round(currentTemp) + "\u00b0C";
                            Color c;
                            if (currentTemp < 50) { c = Color.Lime; }
                            else if (currentTemp < 65) { c = Color.Orange; }
                            else if (currentTemp < 80) { c = Color.OrangeRed; }
                            else { c = Color.Red; }
                            lblTemp.ForeColor = c;
                            lblPeak.Text = peakTemp > 0.5f ? "\u5cf0\u503c: " + Math.Round(peakTemp) + "\u00b0C" : "\u5cf0\u503c: --\u00b0C";
                            trayIcon.Icon = CreateTempIcon(currentTemp);
                            trayIcon.Text = "CPU: " + Math.Round(currentTemp) + "\u00b0C | \u5cf0\u503c: " + Math.Round(peakTemp) + "\u00b0C";
                        });
                        return;
                    }
                }
            }
        } catch { }
    }

    private Icon CreateTempIcon(float temp)
    {
        var bmp = new Bitmap(16, 16);
        var g = Graphics.FromImage(bmp);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        Color color;
        if (temp < 0.5f) { color = Color.Gray; }
        else if (temp < 50) { color = Color.Lime; }
        else if (temp < 65) { color = Color.Orange; }
        else if (temp < 80) { color = Color.OrangeRed; }
        else { color = Color.Red; }
        var brush = new SolidBrush(color);
        g.FillEllipse(brush, 1, 1, 14, 14);
        brush.Dispose();
        var pen = new Pen(Color.FromArgb(180, 255, 255, 255), 1);
        g.DrawEllipse(pen, 1, 1, 14, 14);
        pen.Dispose();
        var font = new Font("Consolas", 7, FontStyle.Bold);
        var tb = new SolidBrush(Color.White);
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
        g.DrawString("C", font, tb, 4, 3);
        font.Dispose(); tb.Dispose();
        var icon = Icon.FromHandle(bmp.GetHicon());
        g.Dispose(); bmp.Dispose();
        return icon;
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        if (readTimer != null) { readTimer.Stop(); readTimer.Dispose(); }
        if (computer != null) { computer.Close(); }
        if (trayIcon != null) { trayIcon.Dispose(); }
        base.OnFormClosed(e);
    }

    protected override void OnFormClosing(FormClosingEventArgs e) { trayIcon.Visible = false; base.OnFormClosing(e); }

    [STAThread]
    static void Main() { Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false); Application.Run(new CpuTempGauge()); }
}
