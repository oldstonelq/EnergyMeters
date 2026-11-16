using Newtonsoft.Json;

namespace ReadDataSoftware
{
    /// <summary>
    /// 描述一次采样中的一个参量通道：名称、单位、小数位与所属分组。
    /// 界面只依赖这个描述来生成卡片、表头与曲线轴，不需要知道当前接的是哪款表。
    /// </summary>
    public sealed class ChannelDescriptor
    {
        /// <summary>
        /// 描述一个参量通道。
        /// </summary>
        /// <param name="key">通道标识</param>
        /// <param name="name">显示名称</param>
        /// <param name="unit">单位</param>
        /// <param name="decimals">显示小数位</param>
        /// <param name="group">所属分组</param>
        [JsonConstructor]
        public ChannelDescriptor(string key, string name, string unit, int decimals, string group)
        {
            Key = key;
            Name = name;
            Unit = unit;
            Decimals = decimals;
            Group = group;
        }

        /// <summary>
        /// 通道标识，用于持久化与查找，如 "ua"。
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// 显示名称，如 "A 相电压"。
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 单位，如 "V"。
        /// </summary>
        public string Unit { get; }

        /// <summary>
        /// 显示时保留的小数位。
        /// </summary>
        public int Decimals { get; }

        /// <summary>
        /// 所属分组，如 "相电压"，界面按此归拢卡片与表格列。
        /// </summary>
        public string Group { get; }

        /// <summary>
        /// 带单位的显示名称，如 "A 相电压 (V)"。
        /// </summary>
        [JsonIgnore]
        public string DisplayName
        {
            get { return string.IsNullOrEmpty(Unit) ? Name : Name + " (" + Unit + ")"; }
        }

        /// <summary>
        /// 按通道精度格式化数值。
        /// </summary>
        public string Format(double value)
        {
            return value.ToString("F" + Decimals);
        }
    }
}
