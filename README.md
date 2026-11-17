# MeterRecorder

安科瑞三款智能电表的读数记录工具。通过 Modbus RTU 采集实时值，停止时落盘为 JSON，
可导出 CSV、回放历史数据。交流表与直流表共用同一套界面，表型差异只体现为通道数量。

- 解决方案：`MeterRecorder.sln`
- 程序：`MeterRecorder.exe`（.NET Framework 4.8）
- 依赖：Newtonsoft.Json 13.0.3（`packages/`）

## 支持的仪表

| 型号 | 类型 | 通道数 | 默认从站地址 | 采集内容 |
|---|---|---|---|---|
| DJSF1352 | 直流单路 | 3 | 1 | 电压、电流、功率 |
| AMC | 交流三相 | 17 | 1 | 相电压、线电压、电流、有功、无功 |
| ADL400 | 交流三相 | 17 | 177 | 同 AMC，功率为 32 位整型 |

三款表的寄存器地址与数值缩放分别写在 `Models/` 下三个类里，`Core/MeterChannels.cs`
保存每款的通道定义（名称、单位、小数位、分组）。

## 功能

- **定时采集**：默认 1 秒一帧，采集循环在后台线程，界面不被阻塞。
- **实时显示**：左侧卡片按分组显示当前值，右上曲线跟随所选分组，下方记录表保存本次采集。
- **落盘记录**：点「停止」后先等采集线程退出，再把整段数据写入 JSON，文件名即起止时间。
- **导出 CSV**：导出当前分组可见的列，GB2312 编码，Excel 直接打开不乱码。
- **历史回放**：独立模态窗体，可勾选 1~3 个通道叠加；通道定义随文件走，换表型也能打开。
- **失败可见**：端口打不开、读数中断、写盘失败都会弹提示并写 `Logs\log.txt`，不静默记 0。

## 运行环境

- Windows 7 SP1 及以上，.NET Framework 4.8
- 一条 Modbus RTU 链路（RS485 转 USB 或板载串口），默认 9600 / 8 位 / 无校验 / 1 位停止位
- 免安装：整个目录拷到现场机器即可运行

## 快速开始

1. 用 Visual Studio 2022 打开 `MeterRecorder.sln`，生成解决方案。
2. 运行 `MeterRecorder.exe`。首次运行会在 exe 同级创建 `System\`、`Data\`、`Logs\` 三个目录。
3. 点工具条「设置」，选串口、波特率、校验位与电表型号，点「保存&应用」后**重启软件**生效。
4. 点「开始」采集，卡片、曲线、记录表同步刷新。
5. 点「停止」，确认后自动停线程并保存数据文件，界面随之清空。
6. 需要 Excel 分析点「导出 CSV」；要看历史数据点「打开回放」。

## 界面说明

工具条按钮：

| 按钮 | 作用 | 可用条件 |
|---|---|---|
| 开始 | 打开串口并启动采集 | 空闲时 |
| 停止 | 停采集 → 落盘 → 清空 | 采集中 |
| 导出 CSV | 导出当前分组 | 有数据且空闲 |
| 打开回放 | 选择 JSON 打开回放窗体 | 空闲时 |
| 清空 | 确认后清除本次数据 | 有数据且空闲 |
| 设置 | 打开系统设置 | 空闲时 |

界面分四块：

- **实时卡片**（左上）：按通道分组生成，通道几个就排几张，分组一换另起一段。
- **趋势曲线**（右上）：跟随下方选中的分组，最多 3 条线，每条保留 1000 点。
- **分组按钮**（表头上方）：`全部 / 相电压 / 线电压 / 电流 / 有功功率 / 无功功率`，
  切换分组只重建列，不更换控件。
- **记录表**（下方）：本次采集的逐帧记录，采集中自动滚到最新一行；表格最多保留最近
  5000 行，落盘仍是全部数据。

主窗体客户区 1280×740（含边框与标题栏约 1300×780），设置窗体 523×356。
两者边框固定，不能用鼠标拖边调整大小；主窗体保留最大化按钮，1366×768 的工控机
最大化后能完整显示，设置窗体为对话框样式、不可最大化。

## 配置文件

配置文件为 `System\system.ini`，由「设置」窗体写入：

```ini
[SystemSet]
PortName=COM3
BaudRate=9600
DataBits=8
StopBits=One
Parity=None
EnergyMetersType=ADL400
SlaveAddress=177
```

| 键 | 取值 | 说明 |
|---|---|---|
| `PortName` | `COM1`~`COM10` | 界面只列这十个，虚拟串口请直接改 ini |
| `BaudRate` | 300~115200 | 超出范围按默认 9600 处理 |
| `DataBits` | 5~8 | 默认 8 |
| `StopBits` | `None` / `One` / `Two` / `OnePointFive` | 默认 `One` |
| `Parity` | `None` / `Odd` / `Even` / `Mark` / `Space` | 默认 `None` |
| `EnergyMetersType` | `DJSF1352` / `AMC` / `ADL400` | 决定通道定义与寄存器地址 |
| `SlaveAddress` | 1~247 | 留空或填 0 时用表型默认值 |

解析全部走 `TryParse` 并带范围校验，ini 里有脏值不会导致启动崩溃，只会回落到默认值。

> `SlaveAddress` 目前没有界面入口，需要手工编辑 ini。ADL400 出厂默认 177，AMC 与
> DJSF1352 默认 1，接多块表时靠这个区分。

## 数据文件

保存目录 `Data\<表型>\`，文件名取首末样本时间：

```
Data\ADL400\20261116142001至20261116143000.json
```

格式为带表型与版本的对象（下面是实际输出结构，`Channels` 只列一项示意）：

```json
{
  "MeterType": "ADL400",
  "Version": 1,
  "Channels": [
    { "Key": "ua", "Name": "A 相电压", "Unit": "V", "Decimals": 1, "Group": "相电压" }
  ],
  "Samples": [
    { "Time": "2026-11-16T14:20:01", "Values": [220.4, 219.8, 220.1, 381.6, 9.461, 3.72] }
  ]
}
```

- `Values` 与 `Channels` **按下标一一对应**，数组长度即通道数（三相表 17 个）。
- `Version` 用于后续格式升级；读到高于程序支持的版本会提示升级，不做硬解析。
- 旧版的无包装数组格式仍可打开，程序按首条记录是否含 `phaseVoltageA` 判断三相还是直流。

## CSV 格式

只导出当前分组可见的列，逗号分隔，每个字段后跟一个制表符（便于 Excel 对齐），
编码为 GB2312。表头与首行示例：

```
时间	,A 相电压 (V)	,B 相电压 (V)	,C 相电压 (V)	,
2026-11-16 14:20:01	,220.4	,219.8	,220.1	,
```

导出失败（文件被 Excel 占用、目录只读）会提示具体原因，不会假装成功。

## 历史回放

「打开回放」选择一个 JSON，弹出只读的模态窗体：

- 顶部显示文件名、表型、记录条数与时间范围；
- 左侧列出文件里的全部通道，最多勾选 3 个，超选会被拒绝；
- 右侧曲线与下方记录表复用主窗体的两个控件，行为与采集时一致；
- 底部可导出 CSV，默认文件名与数据文件同名。

通道定义取自文件本身，所以拿 ADL400 的数据在配成 DJSF1352 的机器上也能正常打开。

## 项目结构

```
MeterRecorder/
├─ Core/         表型枚举与转换、通道定义、样本、IEnergyMeter、工厂
├─ Config/       AppPaths（目录）、DeviceConfig（ini 配置）
├─ Protocol/     ModbusRtuClient（组帧/超时/校验）、IModbusTransport 与串口实现、
│                ModbusCrc、ModbusFrame、ModbusException
├─ Models/       三款表的寄存器地址与缩放
├─ Services/     MeterSampler（采集循环、取消、端口生命周期）、RecordingSession
├─ Storage/      SampleRepository（JSON 读写、旧格式兼容）
├─ Controls/     ChannelCardPanel、SampleTable、TrendChart
├─ Forms/        MainForm、SystemSettingsForm、ReplayForm
├─ Help/         ini 读写、CSV 导出、Logger、FormIcon
├─ App/          SystemParameters（组合根：配置、路径与当前表实例）
└─ Properties/   程序集信息与资源
```

`MeterRecorder.Tests/` 是同级的测试工程，用 xUnit 覆盖协议与表计解析，见下文「测试」。

分层意图：`Core` / `Protocol` / `Models` 不依赖界面，`Services` 只负责采集，
`Controls` 与 `Forms` 只负责渲染。新增一款表只需加一份通道定义加一个 `IEnergyMeter`
实现，窗体代码不用改。

### 命名与注释约定

- 类型、方法、属性、事件用 `PascalCase`；参数与局部变量用 `camelCase`。
- 私有实例字段 `_camelCase`，私有静态字段 `s_camelCase`，常量 `PascalCase`。
- 窗体控件用 `_` 前缀加控件类型后缀（`_saveButton`、`_parityCombo`），
  与 `Designer.cs` 里的字段同名，三个窗体保持一致。
- 文件名与主类型同名，一个文件只放一个类型；扩展方法与枚举分开
  （`MeterType.cs` 与 `MeterTypeExtensions.cs`），partial 类与其 Designer 除外。
- 所有成员都写 XML 注释，说明「为什么这么做」而不是复述签名；有参数的方法写 `<param>`。
- 两处例外：事件处理器的 `sender` / `e` 不写参数说明；测试方法的参数由
  `[InlineData]` 直接给出，也不重复写。

## 构建

- Visual Studio 2022，需安装 .NET Framework 4.8 开发工具集
- NuGet 依赖：Newtonsoft.Json 13.0.3（`packages.config`，还原到解决方案同级的 `packages\`）
- 命令行：`msbuild MeterRecorder.sln /p:Configuration=Release`

产物 `MeterRecorder.exe` 依赖同目录的 `Newtonsoft.Json.dll`，发布时两个文件一起拷。

图标以 exe 资源形式发布（工程里的 `ApplicationIcon`），窗体在构造时从 exe 读取；
`.resx` 里不再存二进制对象。这样 Visual Studio 与 `dotnet build` 的构建行为完全一致，
也不需要额外的 `System.Resources.Extensions` 引用。

## 测试

`MeterRecorder.Tests` 用 xUnit，共 120 个用例，覆盖除界面与真实串口以外的全部逻辑：

- **协议**：CRC 计算（说明书示例帧比对）、03H 组帧、坏帧检测、分片到达的拼帧、应答超时；
- **应答校验**：正常帧、从站地址不匹配、功能码不符、数据长度不符、CRC 错误、异常应答码与中文说明；
- **三款表**：DJSF1352 指数缩放（含 0/4/5 档与负值）与「FFFF 表示无效」、ADL400 全部 17 个通道的 32 位拼接与符号、AMC 三种小数位与功率符号位；
- **通道与表型**：通道键唯一性、默认曲线通道解析、表型枚举互转、`MeterFactory` 装配；
- **配置与存储**：ini 读写与脏值回落、样本文件新旧格式、旧格式脏记录跳过、`RecordingSession` 命名与回读；
- **采集服务**：启动开端口、失败中断、停止可重复、释放容错。

界面（四个控件与三个窗体）与 `SerialPortTransport` 需要真实窗口或串口，不在自动化范围。

### 在 Visual Studio 里

1. 打开 `MeterRecorder.sln`，首次会还原 xUnit 相关包（需联网；也可右键解决方案 →「还原 NuGet 包」）；
2. 菜单 **测试 → 测试资源管理器**（`Ctrl+E, T`），点「全部运行」；
3. 只跑一个：在测试资源管理器里按名称筛选，或在代码里右键方法 →「运行测试」/「调试测试」，后者可打断点。

### 命令行

```
dotnet test MeterRecorder.Tests/MeterRecorder.Tests.csproj
dotnet test MeterRecorder.Tests/MeterRecorder.Tests.csproj --filter FullyQualifiedName~ModbusCrcTests
dotnet test MeterRecorder.Tests/MeterRecorder.Tests.csproj --filter FullyQualifiedName~Read_CombinesThirtyTwoBitPowerWithSign
```

命令行需要 .NET SDK；在 Visual Studio 里跑不需要。

### 写新测试

测试文件加在 `MeterRecorder.Tests` 下即可，SDK 工程会自动包含。表计类测试先用
`FakeModbusTransport` 预置应答，再构造表对象：

```csharp
FakeModbusTransport transport = new FakeModbusTransport()
    .Expect(177, 0x0061, 3, 2204, 2198, 2201)   // 从站 177、起始 0x61、读 3 个寄存器
    .Expect(177, 0x0078, 3, 3816, 3812, 3814);

using (Adl400EnergyMeters meter = new Adl400EnergyMeters(new DeviceConfig(), transport))
{
    Assert.Equal(220.4, meter.Read().GetValue(0), 3);
}
```

`Read()` 会发的每个请求都要预置，缺一个会抛「测试没有为该请求准备应答」。
浮点断言用 `Assert.Equal(期望, 实际, 3)`；需要固定时间戳时，给表类的 internal 构造函数
第三个参数传 `() => 固定时间`（测试程序集有 `InternalsVisibleTo`）。

测试通过 `IModbusTransport` 注入内存通道，不需要串口，也不碰真实表计。

## 日志与常见提示

日志追加写入 `Logs\log.txt`。常见弹窗：

| 提示 | 原因 | 处理 |
|---|---|---|
| 设备无法连接 | 串口被占用、线没接、波特率或从站地址不对 | 换串口、确认接线与参数，先关掉其他串口工具 |
| 读数中断：… | 采集中途 CRC 校验失败或应答超时 | 检查干扰与接地；程序不自动重试，需重新点「开始」 |
| 数据保存失败：… | 目录只读、磁盘已满 | 换可写目录或清出空间 |
| 文件格式版本 N 高于… | 用旧程序打开新文件 | 升级软件 |
| 程序已在运行中 | 单实例互斥 | 关掉已运行的实例 |

## 已知限制

- **单实例**：互斥名取 exe 名，改了程序名等于换了互斥体，两个版本可同时运行并抢串口。
- **一帧失败即中断**：串口读失败直接结束本次采集并提示，不做自动重试，避免错误数据入库。
- **曲线共用一根 Y 轴**：电压、电流、功率量级差别大时，小量级的线会被压扁；
  改多轴或分图记为待办，见 `TODO.md` 第 15 项。
- **数据目录跟随 exe**：换目录运行看不到旧数据，需要把 `Data\` 与 `System\system.ini` 一起搬。
- **表型切换需重启**：配置在启动时装配，改表型后要点「保存&应用」再重启。
- **从站地址只能改 ini**：界面暂未提供该项输入。

## 相关文档

- `TODO.md`：架构评审留下的待办、窗体设计稿，以及上真机前必须核对的读数项。
