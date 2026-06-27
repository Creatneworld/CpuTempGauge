CpuTempGauge
这是 Windows 桌面上一个轻量级 CPU 温度监控小工具，以胶囊形状悬浮在屏幕右上角，实时显示 CPU 封装温度及峰值温度。

## 功能

- 🖥️ **实时温度**：每 3 秒读取一次 CPU Package 温度
- 📊 **峰值追踪**：自动记录运行期间的最高温度
- 🔄 **重置峰值**：双击温度区域可将峰值重置为当前温度
- 🎨 **颜色预警**：绿 (<50°C) → 橙 (<65°C) → 橙红 (<80°C) → 红 (≥80°C)
- 🖱️ **拖拽移动**：按住温度区域任意拖动
- ✖️ **关闭**：点击右上角 × 按钮
- 🔄 **开机自启**：可设置计划任务实现开机自动运行

## 截图

*（运行后截图）*

## 使用方法

1. 直接运行 `CpuTempGauge.exe`（需要管理员权限读取 CPU 传感器）
2. 双击温度数字可重置峰值记录
3. 拖拽窗口到任意位置

## 技术细节

- 语言：C# (.NET Framework 4.7.2+)
- 硬件访问：LibreHardwareMonitorLib（开源，Apache 2.0 协议）
- 界面：WinForms，GDI+ 圆角区域
- 文件大小：~300KB（含依赖库）

## 致谢

本项目基于以下开源项目：

- **[LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor)** (Apache 2.0)
  用于读取 CPU 温度传感器数据。`LibreHardwareMonitorLib.dll` 直接引用自此项目的发布包。
  感谢 LibreHardwareMonitor 社区提供的硬件监控能力。
