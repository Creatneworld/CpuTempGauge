using System;
using System.Drawing;
using System.Windows.Forms;
using System.Timers;
using System.Runtime.InteropServices;
using LibreHardwareMonitor.Hardware;

public class CpuTempGauge : Form
{
    [DllImport("gdi32.dll")] public static extern IntPtr CreateRoundRectRgn(int x1, int y1, int x2, int y2, int w, int h);
    [DllImport("user32.dll")] public static extern bool ReleaseCapture();
    [DllImport("user32.dll")] public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
    const int WM_NCLBUTTONDOWN = 0xA1, HTCAPTION = 2;
    
    private float curr = 0, peak = 0;
    private Computer computer;
    private System.Timers.Timer timer;
    private Label lblTemp, lblPeak, lblCpuTag;
    
    public CpuTempGauge()
    {
        this.Text = "CPU温度";
        this.FormBorderStyle = FormBorderStyle.None;
        this.Width = 180; this.Height = 72;
        this.TopMost = true; this.ShowInTaskbar = false;
        this.BackColor = Color.FromArgb(28, 28, 28);
        this.StartPosition = FormStartPosition.Manual;
        
        var rgn = CreateRoundRectRgn(0, 0, 180, 72, 36, 36);
        this.Region = System.Drawing.Region.FromHrgn(rgn);
        
        var screen = Screen.PrimaryScreen;
        this.Left = screen.WorkingArea.Right - this.Width - 10;
        this.Top = 10;
        
        lblCpuTag = new Label();
        lblCpuTag.Text = "CPU";
        lblCpuTag.ForeColor = Color.FromArgb(140, 200, 200, 200);
        lblCpuTag.Font = new Font("Microsoft YaHei", 10, FontStyle.Bold);
        lblCpuTag.TextAlign = ContentAlignment.MiddleLeft;
        lblCpuTag.Location = new Point(10, 0);
        lblCpuTag.Size = new Size(40, 44);
        lblCpuTag.BackColor = Color.Transparent;
        this.Controls.Add(lblCpuTag);
        
        lblTemp = new Label();
        lblTemp.Text = "--°C";
        lblTemp.ForeColor = Color.Lime;
        lblTemp.Font = new Font("Consolas", 26, FontStyle.Bold);
        lblTemp.TextAlign = ContentAlignment.MiddleRight;
        lblTemp.Location = new Point(0, 0);
        lblTemp.Size = new Size(175, 44);
        lblTemp.BackColor = Color.Transparent;
        this.Controls.Add(lblTemp);
        
        lblPeak = new Label();
        lblPeak.Text = "峰值: --°C";
        lblPeak.ForeColor = Color.FromArgb(130, 200, 200, 200);
        lblPeak.Font = new Font("Consolas", 9);
        lblPeak.TextAlign = ContentAlignment.MiddleCenter;
        lblPeak.Location = new Point(0, 42);
        lblPeak.Size = new Size(158, 22);
        lblPeak.BackColor = Color.Transparent;
        this.Controls.Add(lblPeak);
        
        var closeBtn = new Label();
        closeBtn.Text = "×";
        closeBtn.ForeColor = Color.FromArgb(110, 200, 200, 200);
        closeBtn.Font = new Font("Consolas", 10);
        closeBtn.Location = new Point(158, 44);
        closeBtn.Size = new Size(18, 18);
        closeBtn.BackColor = Color.Transparent;
        closeBtn.Click += (s, e) => { timer.Stop(); computer.Close(); Application.Exit(); };
        this.Controls.Add(closeBtn);
        
        // 双击重置峰值 - MouseDown + Clicks=2 比 DoubleClick 事件更可靠
        MouseEventHandler click = null;
        click = (s, e) => {
            if (e.Button == MouseButtons.Left && e.Clicks == 2) { peak = curr; UpdatePeakLabel(); }
            if (e.Button == MouseButtons.Left && e.Clicks == 1) { ReleaseCapture(); SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0); }
        };
        this.MouseDown += click;
        lblTemp.MouseDown += click;
        lblCpuTag.MouseDown += click;
        lblPeak.MouseDown += click;
        
        computer = new Computer(); computer.IsCpuEnabled = true; computer.Open();
        timer = new System.Timers.Timer(3000);
        timer.Elapsed += (s, e) => UpdateTemp();
        timer.AutoReset = true; timer.Start();
    }
    
    private void UpdateTemp()
    {
        try {
            foreach (var hw in computer.Hardware) {
                hw.Update();
                foreach (var se in hw.Sensors) {
                    if (se.SensorType == SensorType.Temperature && se.Value.HasValue && se.Name == "CPU Package") {
                        float t = se.Value.Value; curr = t;
                        if (t > peak) peak = t;
                        this.Invoke((MethodInvoker)delegate {
                            lblTemp.Text = Math.Round(curr) + "°C";
                            if (curr < 50) lblTemp.ForeColor = Color.Lime;
                            else if (curr < 65) lblTemp.ForeColor = Color.Orange;
                            else if (curr < 80) lblTemp.ForeColor = Color.OrangeRed;
                            else lblTemp.ForeColor = Color.Red;
                            UpdatePeakLabel();
                        });
                        return;
                    }
                }
            }
        } catch {}
    }
    
    private void UpdatePeakLabel()
    {
        try {
            this.Invoke((MethodInvoker)delegate {
                lblPeak.Text = peak > 0.5f ? "峰值: " + Math.Round(peak) + "°C" : "峰值: --°C";
                if (peak < 60) lblPeak.ForeColor = Color.FromArgb(130, 200, 200, 200);
                else if (peak < 75) lblPeak.ForeColor = Color.Orange;
                else lblPeak.ForeColor = Color.Red;
            });
        } catch {}
    }
    
    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        timer.Stop(); timer.Dispose(); computer.Close();
        base.OnFormClosed(e);
    }
    
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new CpuTempGauge());
    }
}
