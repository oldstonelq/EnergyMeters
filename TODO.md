# 架构待办

记录 2026-11-11 架构评审发现的问题，2026-11-13 补充窗体改造设计稿。行号与文件位置以提交 `e3bf4e4` 为准，后续改动后可能漂移。

图例：`[ ]` 未开始　`[x]` 已完成　**P0** 结构性　**P1** 可靠性　**P2** 清理

> 窗体改造（第 12 项）放在文档末尾独立小节，因为它既有待办也含设计稿，别按编号顺序直接跳到它 —— 它依赖第 2、3 项。

---

## 完成记录

### 2026-11-16　P0 结构性改造落地（`26e6317`）

一次提交完成第 1、2、3、4 项，并顺带覆盖第 5、6、7、9、11 项。

- 新增 `Core/`：`MeterType` 枚举、`ChannelDescriptor`、`MeterSample`、`MeterChannels`（三份通道定义）、`IEnergyMeters`（改为 `Channels` + `Read()`）、`MeterFactory`。
- 新增 `Config/`：`AppPaths`（构造后只读）、`DeviceConfig`（串口 + 表型 + 从站地址，读取时带范围校验与枚举容错）。
- 新增 `Services/MeterSampler.cs`：采集循环、取消、端口开关与事件推送，事件在后台线程触发。
- 新增 `Storage/SampleRepository.cs`：JSON 带 `meterType` / `version` / `channels`，同时能读旧的数组格式。
- `SystemParas` 收敛为组合根，只剩 `Paths`、`Device`、`EnergyMeters` 三个只读属性与 `Load()`。
- 删除 `AMC_DataStructure`、`DJSF1352_DataStructure`、`SerialPortSettings`、`JsonSerializerHelper`，以及 `Interface/` 下重复的 `IEnergyMeters`。
- 表型字符串比较从 21 处降为 0；`aMainForm` 不再按型号分支，只剩一处用枚举选择表格，第 12 项会去掉。

验证：`dotnet build` 0 警告 0 错误；另建独立工程对存储层做了 14 项断言（新旧两种格式、通道顺序、时间与数值精度）全部通过。

**注意**：本次只改了数据流与代码结构，界面布局仍是「两块表格叠放」的老样子，属于第 12 项范围。

---

## P0 结构性

这几项决定后续所有改动的成本。第 1、2 项收益最大且风险最小，第 3 项是分水岭。

### [x] 1. 表型字符串改为枚举

- **现状**：`SystemParas.EnergyMetersType` 是 `string`，取值为 `"AMC"` / `"DJSF1352"` / `"ADL400"`。全项目共有 **21 处** `EnergyMetersType == "xxx"` 比较，散落在 `aMainForm.cs`、`SystemParas.cs`。
- **影响**：写成 `"ADL40O"`（字母 O）编译期毫无反应，运行时静默走错分支；新增表型要逐处补 if。
- **动作**：
  - 新增 `MeterType` 枚举（`Djsf1352` / `Amc` / `Adl400`），`SystemParas` 只保留一个 `MeterType` 属性。
  - ini 的字符串与枚举之间做一次显式映射（`Enum.TryParse` + 别名兼容）。
  - 把 21 处比较改为 `switch` 或策略查表。
- **验收**：`grep "EnergyMetersType =="` 结果为 0。
- **风险**：低。纯机械替换，编译期即可验证。

### [x] 2. `IEnergyMeters` 返回强类型而非 `double[]`

- **现状**：`ReadVoltage()` / `ReadCurrent()` / `ReadPower()` 全部返回 `double[]`。`voltage[3]` 是线电压 UBC、`power[7]` 是总无功——这些语义只存在于 `aMainForm.RecordSample` 的代码记忆里。
- **影响**：数组长度随表型变化（1 / 6 / 6、1 / 3 / 8），表型判断写错一个分支就是越界；新增表型必须同步改 UI 的映射代码，违反开闭原则。
- **动作**：
  - 新增 `MeterSample`（`Time` + 强类型字段，或 `Voltages` / `Currents` / `Powers` 三个只读集合）。
  - `IEnergyMeters` 改为 `MeterSample Read()` 或保留三个方法但返回具名类型。
  - `RecordSample` 与 `AddRow` 改为从 `MeterSample` 取字段，不再用下标。
- **验收**：`aMainForm` 中不再出现 `voltage[` / `current[` / `power[` 这类下标访问。
- **风险**：中。需要同时改 3 个表实现 + UI 两处映射，建议与第 1 项一起做。

### [x] 3. 把采集循环从窗体搬到 `MeterSampler`

- **现状**：`aMainForm.Thread_AutoWork` 是窗体私有方法，内部直接读 `SystemParas.EnergyMeters`、直接把对象塞进 `SystemParas.Data1/Data2`、直接 `Invoke` 更新控件。并发模型是「后台 Task 写共享集合 + UI Timer 每秒轮询」，靠 `_sampleLock` 与 `_chartedSampleCount` 维持一致。
- **影响**：采集逻辑无法脱离窗体存在，无界面/无串口时无法验证；无法做多表并联；加表型必须改 UI。
- **动作**：
  - 新增 `MeterSampler`：`Start(IEnergyMeters, TimeSpan)` / `Stop()`，内部持有 `CancellationTokenSource`，采样结果通过 `event EventHandler<SampleEventArgs>` 推送。
  - 窗体只订阅事件：收到样本时更新 DataGridView 与图表。
  - 删除 `timer1`、`_sampleLock`、`_chartedSampleCount`。
- **验收**：`aMainForm` 中不再出现 `while` 循环与 `Task.Run`；可以在控制台宿主里跑通一次采集。
- **风险**：中。行为等价性需要真机验证一次。

### [x] 4. 拆分 `SystemParas`

- **现状**：一个类里 13 个 `public static` 成员，同时是配置中心、对象注册表、内存数据库；路径在静态构造时依赖 `Application.StartupPath`。
- **影响**：任何代码都能改任何全局状态；配置与业务数据混在一起；无法注入替身做测试。
- **动作**：
  - `AppPaths`：只读，路径计算集中在此。
  - `DeviceConfig`：串口参数 + 表型 + 从站地址，构造时注入，不再全局可变。
  - `SystemParas` 收敛为组合根，只负责装配。
- **验收**：`grep "public static" Public/SystemParas.cs` 只剩 1~2 处（如 `Current` 单例）。
- **风险**：中低。可以分两步走：先只读化（`readonly`），再抽类型。

---

## P1 可靠性

### [x] 5. JSON 加类型与版本包装头

- **现状**：`JsonSerializerHelper.LoadFromJsonFile`（`Help/JsonSerializerHelper.cs:58`）用反射取属性名 + 判断 JSON 文本里是否包含某个驼峰子串来猜文件属于哪种表。
- **影响**：格式一改，旧数据文件全部读不进来；换个字段名就可能误判；错误信息只有"不是该类型仪表数据"。
- **动作**：落盘改为 `{ "meterType": "ADL400", "version": 1, "samples": [...] }`；读取时按 `meterType` 分派，按 `version` 做兼容。
- **验收**：`JsonSerializerHelper` 中不再出现 `PropertyInfo` 与 `jsonString.Contains`。
- **注意**：这是**破坏性格式变更**，需要同时保留旧文件的读取分支，或提供一次性迁移。
- **前置依赖**：第 12 项的历史回放窗体必须靠这个标记才能确定用哪套通道定义，所以格式改造要排在回放之前。

### [x] 6. 写盘失败要可见

- **现状**：`JsonSerializerHelper.cs:34` 与 `:74` 的 `catch` 里只有 `Console.WriteLine`。本项目是 `WinExe`，没有控制台，输出直接丢弃。
- **影响**：采集几小时的数据可能一条没存下来，用户却看到"数据保存成功"以外的沉默。
- **动作**：失败时写 `LogError` 并向调用方返回明确结果；`SaveCollectedData` 里对失败要弹提示。

### [x] 7. 配置读取加校验

- **现状**：`SystemParas.cs:110`、`:116` 直接 `int.Parse(baudRate)` / `int.Parse(dataBits)`，且发生在 `Program.Main` 中，外层没有 try。
- **影响**：ini 被手工改坏或写入脏值，程序启动即崩，用户看不到任何有意义的提示。
- **动作**：改为 `int.TryParse` + 范围校验（波特率 > 0、数据位 5~8），非法值回落默认并记录日志。
- **备注**：停止位与校验位已改为 `Enum.TryParse`，本条只剩整型两项。

---

## P2 清理

### [ ] 8. 移除 Excel COM 依赖

- **现状**：`Help/FileHelp.cs:83` 的 `ReadExcelFile` 与 `:61` 的 `ReadCsvFile` 无人调用，但 `ReadExcelFile` 拖着一整个 `Microsoft.Office.Interop.Excel` 引用。
- **影响**：部署包必须要求目标机器装 Office；`packages/` 里多一个几十 MB 的依赖。
- **动作**：删除这两个方法 → 移除 csproj 中的 Excel 引用与 `packages.config` 条目 → 确认 `ExportCsv` 不受影响。

### [x] 9. 数据模型命名与复用

- **现状**：`ADL400` 复用 `AMC_DataStructure`（类型名与内容不符）；`SystemParas.Data1` / `Data2` 无语义；`DJSF1352_DataStructure` 与 `AMC_DataStructure` 都是"一次采样"，却各写一套。
- **动作**：合并为统一的 `MeterSample`（与第 2 项合并实施），集合命名改为 `samples` 之类。
- **备注**：改名涉及 JSON 序列化字段，需与第 5 项一并处理，否则旧文件失效。

### [ ] 10. 视图列定义从 Designer 移出

- **现状**：`aMainForm.Designer.cs` 中硬编码 21 个 `DataGridViewTextBoxColumn`，两套表格对应两种表型。
- **动作**：改为运行时按表型生成列（配合第 2 项之后才划算）。
- **备注**：本项已并入第 12 项（窗体改造），不再单独排期；新版布局里表格的列本来就由通道定义运行时生成。

### [x] 11. 命名空间残留

- **现状**：`aMainForm.cs:1` 与 `SystemParas.cs:8` 有 `using ReadData;`，但项目中不存在 `namespace ReadData`（能编过是因为存在 `ReadData.Properties`）。
- **动作**：删除这两行。
- **附带**：`Help/JsonSerializerHelper.cs` 位于全局命名空间，与其余文件不一致，建议移入 `ReadDataSoftware`。

---

## 窗体改造（设计稿）

### 设计前提

2026-11-13 确认，以下三条是本设计的前提，改动前别再动摇：

1. 主窗体的记录区**只显示本次采集**，不做历史文件浏览；
2. 历史回放独立成窗，模态打开，只读；
3. 现场**没有多表并联**，因此不设计设备标签页、不做多串口并行。

### [ ] 12. 按本设计稿改造窗体

- **依赖**：第 2、3 项已于 `26e6317` 完成，前置已就绪。剩下的工作是把控件改成按通道定义渲染 —— 数据模型已经能直接喂给 `SampleTable` / `TrendChart`。
- **现状问题**：
  - 两块 `DataGridView` 叠在同一个 `panel4` 里，都 `Dock = Fill`，靠 `Visible` 切换 —— 等于一个 Form 装了两套界面；
  - 三相表时把 `ColumnStyles` 改成 `99.46% / 0.54%`，曲线实际被压没了；
  - 21 个列名硬编码在 Designer；主表尺寸 1734×635，比 1366 宽的工控机屏幕还宽；
  - 三种表的专属参数（分流器类型、PT/CT、从站地址）在界面上完全没有入口，全是代码里的常量。
- **验收**：新增一款表时不需要改动窗体代码，只需提供一份通道定义 + 一个 `IEnergyMeters` 实现。

#### 主窗体

默认 1280 × 800，最小 1100 × 700，四个区域之间用 Splitter 隔开（现场小屏可以把记录表拉小）。

| 区域 | 尺寸 | 内容 |
|---|---|---|
| 工具条 | 48 px | 开始 / 停止、导出 CSV、打开回放、清空、设置；右侧状态灯 + 串口 + 表型 + 已采集条数 + 运行时长 |
| 实时卡片区 | 约 55% | 按通道分组渲染卡片，自动换行 |
| 曲线区 | 约 45% | 跟随选定通道，单条线（后续可扩到多条叠加） |
| 本次采集记录 | 约 300 px | 一张 `DataGridView`，列 = 时间 + 当前通道组的列，用「电压 / 电流 / 功率」切换 |

按钮状态机：

| 状态 | 开始 | 停止 | 导出 | 清空 | 设置 |
|---|---|---|---|---|---|
| 空闲 | 可用 | 禁用 | 有数据才可用 | 有数据才可用 | 可用 |
| 采集中 | 禁用 | 可用 | 禁用 | 禁用 | 禁用 |
| 异常中断 | 可用 | 禁用 | 可用 | 可用 | 可用 |

三种表的差异只体现在通道数量上，布局本身不变：

| | DJSF1352 | AMC | ADL400 |
|---|---|---|---|
| 通道数 | 3 | 17 | 17 |
| 卡片分组 | 电压 / 电流 / 功率 | 相电压、线电压、电流、有功、无功 | 同 AMC |
| 曲线默认通道 | 功率 | 总有功 | 总有功 |
| 独有参数 | 分流器类型、量程 | 接线方式、PT/CT | 接线方式、PT/CT、从站地址 |

另外：卡片与表格只在收到样本时刷新，不要再用定时器轮询；现有 `timer1` 的 1000 ms 恰好和采样周期重合，容易抖动。

#### 历史回放窗体

模态、只读。分区自上而下：元信息栏（文件名 + 表型 + 条数 + 时长）→ 左侧通道勾选列表（最多同时画 3 条）→ 右侧曲线（带游标提示）→ 下方只读表 → 底部「导出 CSV / 关闭」。

**前置依赖**：第 5 项。当前文件里没有表型标记，读文件时无法确定该套用哪一份通道定义。

#### 可复用控件

| 控件 | 主窗体 | 回放窗体 |
|---|---|---|
| `SampleTable` | 本次采集记录 | 文件记录，只读 |
| `TrendChart` | 跟随选中通道 | 勾选 1~3 条叠加 |
| `ChannelCardPanel` | 实时值 | 不使用 |

先把 `SampleTable` 与 `TrendChart` 做出来，回放窗体的工作量就只剩「加载文件 + 通道勾选」。

#### 设置窗体

改为 `TabControl` 三页：

- **通讯**：串口、波特率、数据位、停止位、校验位、**从站地址**（新增，替掉硬编码的 177 / 1）
- **表型参数**：页内容随表型切换，表型下拉放在本页顶部
  - DJSF1352：分流器输入类型、量程、双路开关
  - AMC：接线方式、PT、CT
  - ADL400：接线方式、PT、CT、协议选择
- **存储**：数据目录、日志目录

#### 通道定义草案

```csharp
class ChannelDescriptor
{
    string Key;       // "ua"
    string Name;      // "A 相电压"
    string Unit;      // "V"
    int Decimals;     // 1
    string Group;     // "相电压"
}
```

`MeterSample.Values` 按 descriptor 顺序存放。UI 只用 descriptor 生成卡片标题、单位、列头和坐标轴名，不知道当前连的是哪款表 —— 这是「新增表型零改动 UI」的关键。

#### 实现要点

- 卡片区用 `FlowLayoutPanel`（`WrapContents = true`），或运行时增删行列的 `TableLayoutPanel`；
- 历史表始终一个 `DataGridView`，切组时重建 `Columns`，不换控件；
- 全部用 `Dock` / `Anchor`，不再写死 `Size` 与 `Location`；
- 杜绝「两个控件叠在同一容器、靠 `Visible` 切换」这种手法。

---

## 建议的实施顺序

前五步已于 `26e6317` 完成（第 1~5、7、9、11 项）。剩余：

1. 第 12 项阶段一（主窗体骨架 + `SampleTable` / `TrendChart`）—— 前置已就绪。
2. 第 12 项阶段二（历史回放窗体）—— 第 5 项已完成，不再被阻塞。
3. 第 8 项（移除 Excel COM 依赖）—— 随手可做。

第 10 项已并入第 12 项，不再单独排期。

## 明确不做

- **不引入 DI 容器与 MVVM 框架**。三千行的内部工具，容器带来的间接层成本大于收益，构造函数传参足够。
- **不重写 Designer 布局**。手写 WinForms 布局是纯亏，保留设计时支持。
- **不迁移 .NET 8**。4.8 是这类现场工具的合理终点；分层完成后再评估跨平台需求。

## 真机确认项（与架构无关，但上线前必查）

- ADL400 功率的 32 位字序按"高字在前"实现，若读数差 65536 倍需对调 `ADL400_EnergyMeters.ToInt32` 参数。
- AMC 小数点寄存器地址已从 `0x0017` 改为手册上的 `0x0023`，实际读数会变化，需与表盘对照。
- AMC 校验位修正后，旧 ini 中的 `Parity=One` 会被忽略并退回 `None`，需重新设置一次。
