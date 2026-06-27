# CpuTempGauge

Windows 桌面 CPU 温度监控小工具，胶囊形悬浮窗，实时显示 CPU 封装温度及峰值温度。

## 功能

- 🖥️ **实时温度**：每 3 秒读取一次 CPU Package 温度
- 📊 **峰值追踪**：自动记录运行期间的最高温度
- 🔄 **重置峰值**：双击温度区域可将峰值重置为当前温度
- 🎨 **颜色预警**：绿 → 橙 → 橙红 → 红
- 🖱️ **拖拽移动**：按住拖动，顺滑无延迟
- 🔘 **关闭**：点击右下角 ×

## 使用方法

1. 双击 `CpuTempGauge.exe` 运行
2. 双击温度数字重置峰值
3. 按住任意位置拖动

> ⚠️ **首次运行需要管理员权限**（读取 CPU 传感器需要内核驱动支持）

## 杀毒软件误报说明

`LibreHardwareMonitorLib.dll` 包含硬件访问所需的**内核级驱动**，部分杀毒软件可能将其误报为"木马"或"风险工具"。这是已知的正常误报：

- ✅ 本项目代码（`CpuTempGauge.cs`）完全开源透明，可自行审查
- ✅ `LibreHardwareMonitorLib.dll` 来自知名开源项目 [LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor)（Apache 2.0 协议）
- ✅ 该驱动仅用于读取 CPU 温度传感器，**不会修改系统、不会联网、不会收集任何数据**

**解决方法**：在杀毒软件中将 `CpuTempGauge.exe` 所在文件夹加入排除项。

## 技术细节

- 语言：C# (.NET Framework 4.7.2+)
- 硬件访问：LibreHardwareMonitorLib（开源，Apache 2.0）
- 界面：WinForms + GDI+ 自定义绘制
- 窗口：毛玻璃效果 + 圆角胶囊区域

## 致谢

- [LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor) — 提供硬件传感器读取能力（Apache 2.0）
