# CpuTempGauge

一款轻量级的 Windows 桌面 CPU 温度监控小工具。系统托盘常驻 + 半透明浮动窗，实时显示 CPU Package 温度及峰值。

## 功能

- 🔥 **实时测温**：每 3 秒读取 CPU Package 温度（LibreHardwareMonitor）
- 📈 **峰值追踪**：自动记录启动以来的最高温度
- 🎨 **颜色预警**：温度变化时数字颜色随之变化
  - 绿（< 50°C）→ 橙（< 65°C）→ 橙红（< 80°C）→ 红（≥ 80°C）
- 🔔 **系统托盘**：
  - **左键单击** → 切换浮动窗显示/隐藏
  - **悬停** → 查看 Tooltip 温度详情
  - **右键** → 退出程序
- 🪟 **浮动窗**：右上角半透明显示，可拖拽移动
- 🖱️ **拖拽**：按住浮动窗任意位置拖动

## 使用方法

1. 运行 `CpuTempGauge.exe`（建议以管理员身份运行）
2. 浮动窗显示在屏幕右上角
3. 单击通知区域图标可隐藏/显示浮动窗
4. 如需退出，右键托盘图标 → 退出

> ⚠️ **需要管理员权限**（读取 CPU 温度传感器需要内核驱动）

## 项目结构

```
CpuTempGauge/
├── CpuTempGauge.exe          # 主程序
├── CpuTempGauge.cs           # 完整源代码（C#）
├── LibreHardwareMonitorLib.dll  # 硬件读取库
├── HidSharp.dll              # 依赖项
└── README.md
```

## 杀毒软件误报说明

`LibreHardwareMonitorLib.dll` 包含硬件访问所需的**内核级驱动**，部分杀毒软件可能将其误报为"木马"或"风险工具"。这是已知的正常误报：

- ✅ 本项目代码（`CpuTempGauge.cs`）完全开源透明，可自行审查
- ✅ `LibreHardwareMonitorLib.dll` 来自知名开源项目 [LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor)（Apache 2.0）
- ✅ 该驱动仅用于读取 CPU 温度传感器，**不会修改系统、不会联网、不会收集任何数据**

**解决方法**：在杀毒软件中将 `CpuTempGauge.exe` 所在文件夹加入排除项。

## 技术细节

- 语言：C# (.NET Framework 4.7.2+)
- 硬件访问：LibreHardwareMonitorLib（开源，Apache 2.0）
- 界面：WinForms
- 窗口：半透明（Opacity = 0.40），无需管理员提权运行（但建议管理员以读取传感器）
- 测温：LHM 读取 i7-4790K CPU Package 传感器已验证

## 致谢

- [LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor) — 提供硬件传感器读取能力（Apache 2.0）
