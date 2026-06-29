# CpuTempGauge

一款轻量级的 Windows 桌面 CPU/GPU 温度监控小工具。系统托盘常驻 + 半透明浮动窗，CPU 和 GPU 温度每 3 秒轮流显示，带颜色预警和峰值追踪。

## 功能

- 🔟 **CPU/GPU 轮流显示** — 浮动窗每 3 秒切换显示 CPU 和 GPU 温度
- 🎯 **智能传感器选择** — 优先读取 CPU Package，其次 Core Max/CPU Core，支持多兜底路径
- 🔥 **峰值追踪** — CPU 和 GPU 各自独立记录最高温度
- 🎨 **颜色预警** — 温度变化时数字颜色随之变化：绿(<50°C) → 橙(<65°C) → 橙红(<80°C) → 红(≥80°C)
- 🖥️ **系统托盘** — 左键单击切换浮动窗显示/隐藏，悬停查看 CPU+GPU 双温度
- 🪟 **半透明浮窗** — 右上角半透明显示，可拖拽移动
- 🔌 **LibreHardwareMonitor 原生驱动** — 不再依赖自定义内核驱动

## 使用方法

1. **以管理员身份运行** `CpuTempGauge.exe`（LibreHardwareMonitor 需要 Ring 0 驱动权限）
2. 浮动窗显示在屏幕右上角
3. 点击通知区域图标可隐藏/显示浮动窗
4. 如须退出，右键托盘图标 → 退出

> ⚠️ **需要管理员权限**（读取 CPU/GPU 温度传感器需要内核驱动）

## 文件说明

```
CpuTempGauge/
├── CpuTempGauge.exe                   # 主程序
├── CpuTempGauge.cs                    # 完整源代码（C#）
├── LibreHardwareMonitorLib.dll        # 硬件读取库
├── HidSharp.dll                       # 依赖项
├── System.*.dll                       # .NET 依赖项
├── startup.bat                        # 启动脚本
└── README.md
```

## 技术细节

- 语言：C#（.NET Framework 4.7.2+）
- 硬件访问：LibreHardwareMonitorLib（开源，Apache 2.0）
- 界面：WinForms
- 窗口：半透明（Opacity = 0.40），无边框浮动
- 传感器读取：LHM 原生驱动 → WMI ACPI → GPU（三级兜底）
- 读取间隔：3 秒

## 杀毒软件误报说明

`LibreHardwareMonitorLib.dll` 包含硬件访问所需的内核级驱动，部分杀毒软件可能将其误报为"木马"或"风险工具"。这是已知的正常误报。

- ✅ 本项目代码（`CpuTempGauge.cs`）完全开源透明，可自行审查
- ✅ `LibreHardwareMonitorLib.dll` 来自知名开源项目 LibreHardwareMonitor（Apache 2.0）
- ✅ 该驱动仅用于读取温度传感器，**不会修改系统、不会联网、不会收集任何数据**

**解决方法**：在杀毒软件中将 `CpuTempGauge.exe` 所在文件夹加入排除项。

## 致谢

- [LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor) — 提供硬件传感器读取能力（Apache 2.0）
