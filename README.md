# MeterRecorder

安科瑞三款智能电表的读数记录工具：直流表 DJSF1352，交流表 AMC、ADL400。

通过 Modbus RTU 采集实时值，按通道分组显示卡片、曲线与记录表；停止时落盘为
JSON（带表型、格式版本与通道定义），可导出 CSV，也可用回放窗体打开历史文件。

- 解决方案：`MeterRecorder.sln`
- 程序：`MeterRecorder.exe`（.NET Framework 4.8）
- 依赖：Newtonsoft.Json 13.0.3（`packages/`）
