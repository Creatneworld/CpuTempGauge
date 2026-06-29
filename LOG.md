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
