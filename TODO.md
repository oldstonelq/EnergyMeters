# 架构待办

记录 2026-11-11 架构评审发现的问题，2026-11-13 补充窗体改造设计稿，2026-11-16 补二次评审（第 16 项起）。
第 1~15 项的行号以提交 `e3bf4e4` 为准，第 16 项起以提交 `e54ed89` 之后的代码为准，改动后都可能漂移，定位时优先看文件与特征描述。

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

### 2026-11-16　窗体改造落地（`55a1cc5`）

按「先控件、再骨架」两步做：

- 新增 `Controls/`：`ChannelCardPanel`（按通道分组生成卡片，自动换行）、`SampleTable`（列由当前通道组运行时生成，组按钮用同容器内互斥的 RadioButton）、`TrendChart`（系列由通道决定，只追加新增点并裁剪到 1000）。
- 主窗体换成「工具条 + 卡片/曲线 + 记录表」三段式，两个 `SplitContainer` 分层，全部 Dock 布局；删除 DGV1/DGV2、21 个列定义与 `timer1`。
- 通道多的表默认只显示第一个分组，否则十几列挤在一屏读不清；切换分组时曲线跟着换通道。
- 状态栏显示串口、表型、采集状态与本轮条数，按钮按空闲/采集中两态启用。

验证方式：用临时 net48 工程引用编译产物，把窗体渲染成 PNG 核对（直流 3 通道、三相 17 通道两种布局），再用 `Chart.SaveImage` 单独确认曲线、坐标轴与标题。临时工程已删除。

已知取舍：曲线仍把电压/电流/功率三条不同量纲的线画在同一根 Y 轴上，量级相差大时小量级的线会被压扁 —— 与改造前一致，要改进需多轴或分图。另：为绕开 SDK 版 MSBuild 的 resx 限制，截图时曾临时去掉窗体图标，已还原。

### 2026-11-16　历史回放窗体（`4cc3bc5`）

- 新增 `Forms/Form_Replay.cs`：元信息栏（文件名、表型、条数、时间范围）+ 左侧通道勾选（`CheckedListBox`，超过 3 条时拒绝勾选）+ 曲线 + 记录表 + 导出/关闭。
- 直接复用 `SampleTable` 与 `TrendChart`，回放窗体的代码只有两百来行。
- 主窗体的「读取」改为「打开回放」，模态打开，不再覆盖当前采集画面。
- 关键差异：通道定义取自文件（`SampleDocument.Channels`），所以**换了表型也能看老数据** —— 之前主窗体的读取要求文件通道数与当前配置一致。

验证方式：临时工程生成 300 条三相样本并落盘，加载后渲染窗体核对（元信息、17 项勾选、三条曲线、分组表格、按钮位置）。临时工程已删除。

### 2026-11-17　二次评审 P0 修复（`b4cf891`）

- **第 16 项**：`MeterChannels` 新增 `GetDefaultChartKeys` / `ResolveChannelIndexes` / `GetDefaultChartIndexes`，按 `ua`、`ia`、`pTotal` 这类 Key 解析下标；`MainForm` 与 `ReplayForm` 不再出现 `{ 0, 6, 12 }`。`SampleTable` 里的 `channels.Count > 8` 改为具名常量 `AllGroupColumnLimit` —— 阈值本身仍然需要，但不再是匿名魔数。
- **第 17 项**：`FormClosing` 改为 `async void`：先 `e.Cancel = true`，`await StopAsync()`，再 `Close()`；新增 `_stopBeforeClose` 处理连点两次关闭。**没有**改用 `BeginInvoke` 处理样本，因为曲线与表格的追加顺序仍要同步保证，根因是界面线程阻塞等待。
- **第 18 项**：`Program` 用静态字段 `s_instanceMutex` 持有互斥体，`Application.Run` 结束后 `ReleaseMutex` 与 `Dispose`；原来那处直接退出的调用改成 `return`。
- **第 19 项**：`SampleTable.MaxRows` 默认 5000，`Append` 后裁剪首行；`Reload` 只补最后 `MaxRows` 行，避免先塞满再逐行删。
- **第 20 项**：`IEnergyMeters : IDisposable`，三个表类实现并释放 `_client`；`MeterSampler.Dispose` 里调用，兜底等待从 3 秒降到 500 ms。
- **第 24 项**：曲线数量上限统一为 `TrendChart.MaxSeries`，两个窗体不再各定义一份。

验证：`dotnet build` 0 警告 0 错误；临时工程跑了 12 项断言 —— 三款表默认曲线下标（`0,1,2` / `0,6,12` / `0,6,12`）、缺失 Key 跳过、表格绑定 6000 条后只剩 5000 行且首行正确、再追加后首行继续前移、采集器停止只关端口、释放时才调用表对象的 `Dispose`。窗体本身没能在临时宿主里启动（SDK 版 MSBuild 产出的资源需要特定资源程序集，宿主版本对不上），关窗行为要到 VS 里点一次确认。

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
- **完成情况**：JSON 落盘改为抛异常 + 弹窗；后续复查又补了两条同类静默失败 —— `FileHelp.WriteIniKeys` 改为返回 `bool`（原先忽略 Win32 返回值），`DeviceConfig.Save` 任一项写失败即抛 `IOException`；`FileHelp.ExportCsv` 增加 `out string error`，导出失败不再提示"是否打开文件"。

### [x] 7. 配置读取加校验

- **现状**：`SystemParas.cs:110`、`:116` 直接 `int.Parse(baudRate)` / `int.Parse(dataBits)`，且发生在 `Program.Main` 中，外层没有 try。
- **影响**：ini 被手工改坏或写入脏值，程序启动即崩，用户看不到任何有意义的提示。
- **动作**：改为 `int.TryParse` + 范围校验（波特率 > 0、数据位 5~8），非法值回落默认并记录日志。
- **备注**：停止位与校验位已改为 `Enum.TryParse`，本条只剩整型两项。

---

## P2 清理

### [x] 8. 移除 Excel COM 依赖

- **现状**：`Help/FileHelp.cs:83` 的 `ReadExcelFile` 与 `:61` 的 `ReadCsvFile` 无人调用，但 `ReadExcelFile` 拖着一整个 `Microsoft.Office.Interop.Excel` 引用。
- **影响**：部署包必须要求目标机器装 Office；`packages/` 里多一个几十 MB 的依赖。
- **动作**：删除这两个方法 → 移除 csproj 中的 Excel 引用与 `packages.config` 条目 → 确认 `ExportCsv` 不受影响。
- **完成情况**：两个方法与 `using Excel = ...` 一并删除，顺带清掉 `FileHelp` 里另外四个无用的 using；csproj 引用与 `packages.config` 条目移除，`packages/` 下的 Excel 包缓存也删了。删完后全量编译通过，产物里不再有 interop 程序集 —— 目标机器不再需要装 Office。

### [x] 9. 数据模型命名与复用

- **现状**：`ADL400` 复用 `AMC_DataStructure`（类型名与内容不符）；`SystemParas.Data1` / `Data2` 无语义；`DJSF1352_DataStructure` 与 `AMC_DataStructure` 都是"一次采样"，却各写一套。
- **动作**：合并为统一的 `MeterSample`（与第 2 项合并实施），集合命名改为 `samples` 之类。
- **备注**：改名涉及 JSON 序列化字段，需与第 5 项一并处理，否则旧文件失效。

### [x] 10. 视图列定义从 Designer 移出

- **现状**：`aMainForm.Designer.cs` 中硬编码 21 个 `DataGridViewTextBoxColumn`，两套表格对应两种表型。
- **动作**：改为运行时按表型生成列（配合第 2 项之后才划算）。
- **完成情况**：随第 12 项一并完成 —— 21 个列定义与两块表格一起删除，`SampleTable` 的列由当前通道组运行时生成。

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

### [x] 12. 按本设计稿改造窗体

- **依赖**：第 2、3 项已于 `26e6317` 完成，前置已就绪。剩下的工作是把控件改成按通道定义渲染 —— 数据模型已经能直接喂给 `SampleTable` / `TrendChart`。
- **现状问题**：
  - 两块 `DataGridView` 叠在同一个 `panel4` 里，都 `Dock = Fill`，靠 `Visible` 切换 —— 等于一个 Form 装了两套界面；
  - 三相表时把 `ColumnStyles` 改成 `99.46% / 0.54%`，曲线实际被压没了；
  - 21 个列名硬编码在 Designer；主表尺寸 1734×635，比 1366 宽的工控机屏幕还宽；
  - 三种表的专属参数（分流器类型、PT/CT、从站地址）在界面上完全没有入口，全是代码里的常量。
- **验收**：新增一款表时不需要改动窗体代码，只需提供一份通道定义 + 一个 `IEnergyMeters` 实现。
- **完成情况**（`55a1cc5`）：主窗体与三个控件已按设计落地，见「完成记录」。历史回放窗体拆到第 13 项。

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

### [x] 13. 历史回放窗体

- **依赖**：第 5 项已完成，文件里带表型与通道定义，可以直接开工。
- **现状**：第 12 项为保留查看历史数据的能力，在主窗体上留了一个临时的「读取」按钮，把文件读进当前的表格与曲线。它是过渡方案 —— 会冲掉正在显示的采集结果，也不适合长时间翻看。
- **动作**：按上文「历史回放窗体」一节新建模态窗体，复用 `SampleTable` 与 `TrendChart`，补上元信息栏与通道勾选（最多同时画 3 条）；完成后删掉主窗体的「读取」按钮。
- **验收**：主窗体只负责本次采集，历史浏览全部由回放窗体承担。
- **完成情况**：新增 `Forms/Form_Replay.cs`，主窗体的「读取」改为「打开回放」。通道定义取自文件，因此回放不再要求文件表型与当前配置一致 —— 换了表也能看老数据。

---

## 建议的实施顺序

第 1~13 项、第 16~20 项与第 24 项已完成。剩下的按下表推进：

| 顺序 | 项 | 说明 |
|---|---|---|
| 1 | 23 | 抽传输接口 + 测试工程，之后再动协议才有底 |
| 2 | 21 → 22 | `RecordingSession` 与 `Logger`，建议合并成一次改 `MainForm` |
| 3 | 25 → 26 → 27 | 随手清 |
| 4 | 14 → 15 | 需要真表或重做曲线布局，单独排期 |

## 明确不做

- **不引入 DI 容器与 MVVM 框架**。三千行的内部工具，容器带来的间接层成本大于收益，构造函数传参足够。
- **不重写 Designer 布局**。手写 WinForms 布局是纯亏，保留设计时支持。
- **不迁移 .NET 8**。4.8 是这类现场工具的合理终点；分层完成后再评估跨平台需求。

## 后续待办（2026-11-16 追加）

架构改造留下的两项收尾工作，都需要接上真实表计或再动一次界面才能确认。

### [ ] 14. 真机联调确认

本次改造动过读数解析与落盘格式，接上真表后逐条核对：

- **ADL400 功率字序**：按「高字在前」实现。若三相功率与表盘差 65536 倍或出现异常大的数，对调 `Adl400EnergyMeters.ToInt32` 的两个参数。
- **AMC 电量参数**：小数点寄存器地址从代码里的十进制 `23`（= 0x0017H，手册里没有这个地址）改到手册上的 `0023H`，读数会变化，需与表盘对照 DPT / DCT / DPQ 是否正确。
- **AMC 校验位**：修正前保存过的 ini 里可能残留 `Parity=One`（那是停止位的取值），现在会被忽略并退回 `None`，需在设置里重选一次。
- **旧数据文件**：新格式带 `meterType` / `version` / `channels`，旧数组格式仍可读；但新存的文件旧版本程序打不开，升级前先备份 `Data\` 目录。
- **通讯中断行为**：现在一帧读失败就会中断本次采集并弹窗（改造前是静默记 0），按现场习惯确认这个行为是否需要改成「重试 2 次再放弃」。

### [ ] 15. 曲线多轴或分图

- **现状**：三条曲线共用一根 Y 轴。默认组合「电压 / 电流 / 功率」量纲差两个数量级，小量级的线被压成贴底的平线，只能看趋势读不出数值。
- **影响范围**：直流表默认就是这三项混画；三相表默认组是相电压（同量纲）问题不明显，但切到「全部」也会混。
- **可选做法**：
  - 按单位分轴 —— `Chart` 只支持主/次两根 Y 轴，最多分两组，表达能力有限；
  - 只画同组通道 —— 与现在的「曲线跟随分组」一致，改动最小，但直流表三项仍混；
  - 上下分图 —— 每个通道一根小图共享 X 轴，最直观，需要重做 `TrendChart` 的布局。
- **建议**：先做「同组才上图、跨组自动分图」，直流表拆成上下三格；不改数据层，只动 `TrendChart`。

---

## 二次评审的缺陷（P0）

2026-11-16 改名与文档收尾之后重新通读代码。分层已经立住：`Core` / `Protocol` / `Models` 不引用
控件，表型差异只剩 `MeterChannels` 一处，采集循环也搬出了窗体。以下 12 项是这轮发现的
问题，按「会咬人的缺陷 → 结构债 → 小问题」排列，编号接在第 15 项之后。
其中第 16~20 项与第 24 项已于 `b4cf891` 修复，剩下的见「建议的实施顺序」。

### [x] 16. 界面仍按通道下标选曲线

- **现状**：`MainForm.GetDefaultChartIndexes`（`MainForm.cs:315`）用 `_channels.Count == 3` 与硬编码下标 `{ 0, 6, 12 }`；`ReplayForm.GetDefaultChannelIndexes`（`ReplayForm.cs:107`）另写了一份 `<= 3` / `> 12` 的判断；`SampleTable.Bind`（`SampleTable.cs:66`）还有一处 `channels.Count > 8`。
- **影响**：通道模型只做了一半 —— 数据层不认表型了，界面层还在认下标。新增第四款表时（通道数不是 3 或 17），`BindSeries` 里 `channels[6]` 直接抛 `IndexOutOfRangeException`；通道顺序一调整就画错线。
- **动作**：`MeterChannels` 为每款表补一份 `DefaultChartKeys`（如 `{ "ua", "ia", "pTotal" }`），界面按 `Key` 找下标；分组默认值改为「第一个非『全部』的分组」，去掉 `Count > 8`。
- **验收**：窗体与控件里不再出现 `new[] { 0, 6, 12 }` 与 `Count == 3`；构造一个 5 通道的假表能正常出图。
- **风险**：低，纯查询替换。

**完成情况**：`MeterChannels` 提供默认曲线 Key 与解析方法，两个窗体都改为按 Key 取下标；进度顺带完成第 24 项。

### [x] 17. 采集期间关闭窗口会卡 3 秒

- **现状**：`MainForm.FormClosing` 同步调用 `_sampler.Dispose()`（`MainForm.cs:77`），`Dispose` 里 `loop.Wait(3 秒)`（`MeterSampler.cs:182`）；而采集线程的 `OnSampleReady` 用的是同步 `Invoke`（`MainForm.cs:250`）。
- **影响**：UI 线程等采集线程退出、采集线程等 UI 处理 Invoke，两边互等到超时，采集期间关窗口最长卡 3 秒；等待时间越长越像卡死。
- **动作**：`OnSampleReady` 改 `BeginInvoke`（只是在表格末尾追加，不需要同步语义）；`Dispose` 里先 `Cancel` 再 `Wait(500ms)`，超时记日志而不当场阻塞；或把 `FormClosing` 改成 `async` + `await StopAsync()`。
- **验收**：采集时点窗口 X，1 秒内关闭，日志里没有超时记录。
- **风险**：低。改成异步后，关闭途中到达的最后一两帧会被丢弃，需确认可接受。

**完成情况**：关窗改为「取消本次关闭 → 异步停止采集 → 再关闭」，连点两次只取消不打断。样本追加仍用同步 `Invoke`，因为顺序需要保证。

### [x] 18. 单实例互斥可能被 GC 回收

- **现状**：`Program.cs:24` 的 `Mutex` 是局部变量，之后没有引用，也没有 `GC.KeepAlive`。
- **影响**：Release 下 JIT 可能把它判定为已死，GC 回收时命名互斥体句柄关闭，第二个实例就能启动 —— 两个进程抢同一个串口。
- **动作**：改用静态字段持有，或在 `Application.Run` 之后加 `GC.KeepAlive(mutex)`。
- **验收**：Release 编译后连开两次，第二个仍提示「程序已在运行中」。
- **风险**：低。

**完成情况**：互斥体改由静态字段持有，退出时释放并 Dispose。

### [x] 19. 记录表行数无上限

- **现状**：`SampleTable.Append` 只 `Rows.Add`（`SampleTable.cs:117`），没有任何裁剪；曲线有 `MaxPoints = 1000`。
- **影响**：1 秒一帧，8 小时约 2.9 万行 × 17 列，滚动、切分组、重填都会明显变慢，内存持续增长；与曲线 1000 点的上限也不一致。
- **动作**：给表格设行数上限（建议 5000），超出时删最早一行；不要动 `_samples`，落盘仍要完整数据。
- **验收**：连续采集 1 小时以上，切分组与滚动无明显卡顿。
- **风险**：低。

**完成情况**：`MaxRows` 默认 5000，`Append` 后裁首行；`Reload` 只补最后 5000 行。

### [x] 20. 串口对象从不 Dispose

- **现状**：`ModbusRtuClient` 实现了 `Dispose`（`ModbusRtuClient.cs:168`），但全项目没有调用点；`IEnergyMeters` 没有继承 `IDisposable`，`MeterSampler.Dispose` 只调 `Close()`。
- **影响**：`Close` 只关端口，`Dispose` 才释放 SerialPort 的内核对象与内部事件循环；反复开始/停止可能累积句柄。
- **动作**：`IEnergyMeters : IDisposable`，三个表类实现为释放 `_client`；采样器释放时一并调用。
- **验收**：反复开始/停止 20 次，任务管理器里句柄数不持续增长。
- **风险**：低。

**完成情况**：`IEnergyMeters` 继承 `IDisposable`，采样器释放时调用；兜底等待降到 500 ms。

## 二次评审的结构债（P1）

### [ ] 21. 抽出 RecordingSession

- **现状**：样本集合与落盘逻辑都在 `MainForm`：`_samples`、`SaveCollectedData`（`:327`）、`ResetView`（`:368`）、`BuildDataFileName`；回放窗体则用 `SampleDocument`。
- **影响**：采集编排离不开窗体，无法在无界面宿主里验证；两边没有共同的「一次记录」概念，将来加统计、加导出还得继续往窗体里塞。
- **动作**：抽 `RecordingSession`（Add / Count / Save / Clear，外加样本到达事件），主窗体只做转发与刷新；`SampleTable` 的数据源改为从 session 取。
- **验收**：`MainForm.cs` 降到 250 行以内；session 能在控制台宿主里跑通 Add + Save。
- **风险**：中。会同时碰界面与存储两侧，建议放在第 16~20 项之后再动。

### [ ] 22. 日志从窗体剥离

- **现状**：`MainForm.LogError`（`MainForm.cs:407`）是 `public static`；`ReplayForm` 与 `SystemSettingsForm` 出错只弹窗，不写日志。
- **影响**：回放与设置的问题无法回溯；日志格式与路径逻辑长在窗体里。
- **动作**：新增静态 `Logger`（依赖 `AppPaths.LogFile`，内部吞异常保证不影响主流程），三个窗体统一调用。
- **验收**：`grep "LogError"` 不再指向 `MainForm`；回放窗体导出失败也留下日志。
- **风险**：低。

### [ ] 23. 抽 IModbusTransport 并补测试工程

- **现状**：`ModbusRtuClient` 内部直接 `new SerialPort`；CRC、组帧、32 位拼接、符号位、旧格式 JSON 都是纯逻辑，却没有自动验证手段。
- **影响**：改协议或新增表型只能靠真机回归，出错时靠现场现象倒推。
- **动作**：抽 `IModbusTransport`（Read / Write / DiscardInBuffer），串口实现保留；CRC 与帧解析提成可测的静态方法；新建 net48 测试工程，覆盖 CRC 示例帧、异常应答、ADL400 高字在前与符号位、AMC 小数点缩放、旧数组 JSON。
- **验收**：测试不需要串口即可全绿。
- **风险**：中。先加接口与测试、再迁移调用方，可分两次提交。

## 二次评审的小问题（P2）

### [x] 24. 合并重复的曲线策略

- **现状**：`MaxChartSeries = 3`（`MainForm.cs:20`）与 `MaxSeries = 3`（`ReplayForm.cs:17`）各定义一次；默认通道选择有两套实现（`== 3` 与 `<= 3` / `> 12`）。
- **动作**：抽成共享策略（放进 `MeterChannels` 或独立的 `ChartPolicy`），随第 16 项一起改。
- **验收**：`3` 这个上限只出现一次。
- **风险**：低。

**完成情况**：上限统一到 `TrendChart.MaxSeries`，随第 16 项一起改完。

### [ ] 25. 启动装配加兜底

- **现状**：`Program.Main` 没有 try/catch，`SystemParameters.Load()` 或 `MeterFactory.Create()` 抛异常就是无提示崩溃；文件顶部还有三个无用 using。
- **影响**：配置文件被改坏时用户看不到任何解释，只会觉得「双击没反应」。
- **动作**：装配段包 try/catch，失败时写日志 + 弹窗 + 退出；顺手删掉无用 using。
- **验收**：把 ini 里的 `PortName` 改成非法值，启动有明确提示。
- **风险**：低。

### [ ] 26. 旧格式文件的表型标注

- **现状**：`SampleRepository.LoadLegacyArray` 把三相旧文件统一记为 AMC（`SampleRepository.cs:127`）。
- **影响**：ADL400 的老文件在回放窗体里标题显示 AMC，数据本身正确，只是标签不对。
- **动作**：不去猜表型，改为在回放窗体对旧格式标注一句「表型按三相推断」。
- **验收**：打开旧文件时元信息栏有该提示。
- **风险**：低。

### [ ] 27. 表对象内的时间戳

- **现状**：三个表类在 `Read()` 里用 `DateTime.Now` 打时间（如 `Djsf1352EnergyMeters.cs:91`）。
- **影响**：测试无法固定时间；系统时间跳变时样本时间不连续。
- **动作**：做第 23 项时顺手把时钟改为可注入（`Func<DateTime>`），默认仍是 `DateTime.Now`。
- **验收**：测试里能固定时间戳。
- **风险**：低。
