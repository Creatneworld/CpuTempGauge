using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Management;
using LibreHardwareMonitor.Hardware;

public class CpuTempGauge : Form
{
    private float cpuTemp = 0, gpuTemp = 0, cpuPeak = 0, gpuPeak = 0;
    private bool hasCpu = false, hasGpu = false;
    private int displayToggle = 0; // 0=CPU, 1=GPU
    private Computer computer;
    private System.Windows.Forms.Timer timer;
    private Label lblTag, lblTemp, lblPeak;
    private NotifyIcon trayIcon;
    private ContextMenuStrip trayMenu;
    private bool dragging = false;
    private Point dragStart;
    private string logPath;
    
    private void Log(string m) {
        try { System.IO.File.AppendAllText(logPath, DateTime.Now.ToString("HH:mm:ss ") + m + "\r\n"); } catch { }
    }

    public CpuTempGauge()
    {
        logPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\CpuTempGauge.log";
        Log("=== CpuTempGauge START ===");
        
        this.Text = "CPU Temp";
        this.FormBorderStyle = FormBorderStyle.None;
        this.Width = 180; this.Height = 72;
        this.TopMost = true; this.ShowInTaskbar = false;
        this.BackColor = Color.Black;
        this.Opacity = 0.40;
        
        var scr = Screen.PrimaryScreen;
        this.Left = scr.WorkingArea.Right - this.Width - 10;
        this.Top = 10;

        lblTag = new Label();
        lblTag.Text = "CPU";
        lblTag.ForeColor = Color.White;
        lblTag.Font = new Font("Microsoft YaHei", 10, FontStyle.Bold);
        lblTag.TextAlign = ContentAlignment.MiddleLeft;
        lblTag.Location = new Point(10, 0);
        lblTag.Size = new Size(40, 44);
        lblTag.BackColor = Color.Transparent;
        this.Controls.Add(lblTag);

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
        closeBtn.Click += (object sb, EventArgs ev) => { this.Close(); };
        this.Controls.Add(closeBtn);

        MouseEventHandler md = (object sb, MouseEventArgs ev) => {
            if (ev.Button == MouseButtons.Left) { dragging = true; dragStart = new Point(ev.X, ev.Y); }
        };
        MouseEventHandler mm = (object sb, MouseEventArgs ev) => {
            if (dragging) { this.Left += ev.X - dragStart.X; this.Top += ev.Y - dragStart.Y; }
        };
        MouseEventHandler mu = (object sb, MouseEventArgs ev) => { dragging = false; };
        this.MouseDown += md; this.MouseMove += mm; this.MouseUp += mu;
        lblTag.MouseDown += md; lblTag.MouseMove += mm; lblTag.MouseUp += mu;
        lblTemp.MouseDown += md; lblTemp.MouseMove += mm; lblTemp.MouseUp += mu;
        lblPeak.MouseDown += md; lblPeak.MouseMove += mm; lblPeak.MouseUp += mu;

        trayIcon = new NotifyIcon();
        trayIcon.Icon = CreateTempIcon(0, false);
        trayIcon.Text = "--\u00b0C";
        trayIcon.Visible = true;
        trayIcon.Click += (object sb, EventArgs ev) => {
            this.Visible = !this.Visible;
            if (this.Visible) { this.Activate(); this.TopMost = true; }
        };

        trayMenu = new ContextMenuStrip();
        var exitItem = trayMenu.Items.Add("\u9000\u51fa");
        exitItem.Click += (object sb, EventArgs ev) => {
            timer.Stop(); if (computer != null) computer.Close(); trayIcon.Visible = false;
            Application.Exit();
        };
        trayIcon.ContextMenuStrip = trayMenu;

        try {
            computer = new Computer();
            computer.IsCpuEnabled = true;
            computer.IsGpuEnabled = true;
            computer.Open();
            Log("LHM Opened");
        } catch (Exception ex) {
            Log("LHM error: " + ex.Message);
        }

        timer = new System.Windows.Forms.Timer();
        timer.Interval = 3000;
        timer.Tick += (object s, EventArgs e) => { Tick(); };
        timer.Start();
        Log("Timer started");
    }

    private void Tick()
    {
        try {
            if (computer == null) { Log("No computer"); return; }
            
            // Update all hardware
            foreach (var hw in computer.Hardware) {
                hw.Update();
            }
            
            // Collect CPU temperature (prefer Package)
            hasCpu = false;
            float bestCpu = 0;
            foreach (var hw in computer.Hardware) {
                foreach (var se in hw.Sensors) {
                    if (se.SensorType == SensorType.Temperature && se.Value.HasValue) {
                        if (hw.HardwareType == HardwareType.Cpu) {
                            string n = se.Name ?? "";
                            float t = se.Value.Value;
                            if (n.Contains("Package")) { bestCpu = t; hasCpu = true; break; }
                            if ((n.Contains("Core Max") || n.Contains("Core Average")) && !hasCpu)
                                { bestCpu = t; hasCpu = true; }
                            if (!hasCpu) { bestCpu = t; hasCpu = true; }
                        }
                    }
                }
                if (hasCpu) break;
            }
            if (hasCpu) {
                cpuTemp = bestCpu;
                if (cpuTemp > cpuPeak) cpuPeak = cpuTemp;
            }
            
            // Collect GPU temperature
            hasGpu = false;
            foreach (var hw in computer.Hardware) {
                foreach (var se in hw.Sensors) {
                    if (se.SensorType == SensorType.Temperature && se.Value.HasValue) {
                        if (hw.HardwareType == HardwareType.GpuNvidia || hw.HardwareType == HardwareType.GpuAmd) {
                            string n = se.Name ?? "";
                            float t = se.Value.Value;
                            if (n.Contains("Core") || n.Contains("GPU")) { gpuTemp = t; hasGpu = true; break; }
                            if (!hasGpu) { gpuTemp = t; hasGpu = true; }
                        }
                    }
                }
                if (hasGpu) break;
            }
            if (hasGpu) {
                if (gpuTemp > gpuPeak) gpuPeak = gpuTemp;
            }
            
            // WMI ACPI fallback for CPU
            if (!hasCpu) {
                try {
                    using (var mo = new ManagementObjectSearcher(
                        "root\\cimv2",
                        "SELECT * FROM Win32_PerfFormattedData_Counters_ThermalZoneInformation WHERE Name='\\\\_TZ.TZ00'"))
                    {
                        foreach (var o in mo.Get()) {
                            float t = (Convert.ToUInt32(o["HighPrecisionTemperature"]) / 10f) - 273.15f;
                            if (t > 0 && t < 120) { cpuTemp = t; hasCpu = true; if (t > cpuPeak) cpuPeak = t; break; }
                        }
                    }
                } catch { }
            }
            
            // Alternate display
            bool showCpu = true;
            if (hasCpu && hasGpu) {
                displayToggle = (displayToggle + 1) % 2;
                showCpu = (displayToggle == 0);
            } else if (hasGpu) {
                showCpu = false;
            }
            
            float displayTemp;
            float displayPeak;
            if (showCpu) {
                displayTemp = cpuTemp;
                displayPeak = cpuPeak;
                lblTag.Text = "CPU";
                Log(string.Format("CPU:{0}C (Pk:{1})", Math.Round(cpuTemp), Math.Round(cpuPeak)));
            } else {
                displayTemp = gpuTemp;
                displayPeak = gpuPeak;
                lblTag.Text = "GPU";
                Log(string.Format("GPU:{0}C (Pk:{1})", Math.Round(gpuTemp), Math.Round(gpuPeak)));
            }
            
            lblTemp.Text = Math.Round(displayTemp) + "\u00b0C";
            
            Color c;
            if (displayTemp < 50) c = Color.Lime;
            else if (displayTemp < 65) c = Color.Orange;
            else if (displayTemp < 80) c = Color.OrangeRed;
            else c = Color.Red;
            lblTemp.ForeColor = c;
            
            lblPeak.Text = displayPeak > 0.5f
                ? "\u5cf0\u503c: " + Math.Round(displayPeak) + "\u00b0C"
                : "\u5cf0\u503c: --\u00b0C";
            
            trayIcon.Icon = CreateTempIcon(displayTemp, !showCpu);
            trayIcon.Text = string.Format("CPU:{0}°„C  GPU:{1}°„C", Math.Round(cpuTemp), Math.Round(gpuTemp));
            
        } catch (Exception ex) {
            Log("Error: " + ex.Message);
        }
    }

    private Icon CreateTempIcon(float temp, bool isGpu)
    {
        var bmp = new Bitmap(16, 16);
        var g = Graphics.FromImage(bmp);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        Color color;
        if (temp < 0.5f) color = Color.Gray;
        else if (temp < 50) color = Color.Lime;
        else if (temp < 65) color = Color.Orange;
        else if (temp < 80) color = Color.OrangeRed;
        else color = Color.Red;
        using (var brush = new SolidBrush(color)) {
            g.FillEllipse(brush, 0, 0, 16, 16);
        }
        using (var font = new Font("Arial", 9, FontStyle.Bold)) {
            using (var tb = new SolidBrush(Color.White)) {
                var sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                g.DrawString(isGpu ? "G" : "C", font, tb, new RectangleF(0, 0, 16, 16), sf);
            }
        }
        var icon = Icon.FromHandle(bmp.GetHicon());
        g.Dispose(); bmp.Dispose();
        return icon;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) {
            if (timer != null) { timer.Stop(); timer.Dispose(); }
            if (computer != null) { computer.Close(); }
            if (trayIcon != null) { trayIcon.Dispose(); }
        }
        base.Dispose(disposing);
    }

    [STAThread]
    static void Main()
    {
        bool firstInstance;
        var mutex = new Mutex(true, "Global\\CpuTempGaugeMutex", out firstInstance);
        if (!firstInstance) { return; }
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new CpuTempGauge());
        mutex.Close();
    }
}
