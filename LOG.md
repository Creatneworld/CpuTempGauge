# CpuTempGauge — 开发日志

## 2026-06-29

### 完成
- 修复 PawnIO 驱动阻塞 CPU 传感器问题，改用 LHM 原生驱动
- CPU/GPU 温度轮流显示（每 3 秒切换）
- 温度图标优化：Arial 9pt Bold 居中，去描边，全填充
- 清理 20 个调试/测试文件
- 完善 README.md
- 推送至 GitHub

### 技术决策
- **驱动选择**：放弃 PawnIO 自定义驱动，使用 LHM 自带的 WinRing0 驱动
- **传感器优先级**：CPU Package > Core Max > 任意 CPU 温度 > SuperIO 兜底 > WMI ACPI > GPU
- **交替展示**：CPU/GPU 每 3 秒切换，各自独立追踪峰值

### 踩坑记录
- PawnIO.sys 虽然能加载，但会阻塞 LHM 对 CPU 温度传感器的 MSR 访问
- `.codex\AGENTS.md` 早于本 LOG.md 存在，之前的会话记录在其中

### 待办
- [x] 全部完成

## 2026-06-30

### 完成
- 移除全部日志写入代码（Log() 方法及所有调用）
- 删除桌面日志文件 CpuTempGauge.log
- 删除旧版计划任务 \CpuTempGauge（指向 D:\CPUThermometer\CpuTempGaugeNew.exe）
- 修复「已启动」弹窗问题（旧版与新版本互斥体冲突）
- 零警告编译通过

### 技术决策
- **日志清理**：调试日志对用户无实际价值，温度已在悬浮窗和托盘图标上实时显示
- **重复启动根因**：旧版 \CpuTempGauge 计划任务与新版使用同一互斥体名，开机同时启动导致冲突
- **当前自启**：仅保留 \CpuTempGauge_StartPawnIO → startup.bat
- **编译修复**：添加 -target:winexe，消除控制台窗口（避免「大对话框」）
- **定位修复**：添加 StartPosition = FormStartPosition.Manual，确保窗口固定在右上角
- **GPU 读取修复**：CPU/GPU 传感器改为独立并行扫描，消除硬件枚举顺序依赖
- **旧目录清理**：删除 D:\CPUThermometer 残留调试文件
- **最终确认**：编译 -target:winexe 无控制台窗口，StartPosition=Manual 定位右上角
- **GPU 传感器修复**：排除 Hot Spot/VRAM/Memory 等非核心传感器，确保读取真实 GPU Core 温度
- **传感器验证**：通过 LHM dump 确认本机 CPU 温度传感器全部为 null（需走 WMI 兜底），GPU 有两个温度（Core=57°C, Hot Spot=70°C）
- **旧目录清理**：删除 D:\CPUThermometer
