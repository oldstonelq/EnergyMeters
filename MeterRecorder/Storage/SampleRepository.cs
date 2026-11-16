using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MeterRecorder
{
    /// <summary>
    /// 采集文件的内容：表型、格式版本、通道定义与样本。
    /// 通道定义随文件保存，回放时不必依赖当前程序里的表型配置。
    /// </summary>
    public sealed class SampleDocument
    {
        /// <summary>
        /// 表型，取值为配置文件里的写法，如 "ADL400"。
        /// </summary>
        public string MeterType { get; set; }

        /// <summary>
        /// 文件格式版本。
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// 通道定义。
        /// </summary>
        public List<ChannelDescriptor> Channels { get; set; }

        /// <summary>
        /// 样本集合。
        /// </summary>
        public List<MeterSample> Samples { get; set; }
    }

    /// <summary>
    /// 采集文件的读写。
    /// </summary>
    public static class SampleRepository
    {
        /// <summary>
        /// 当前写入的格式版本。
        /// </summary>
        public const int CurrentVersion = 1;

        private static readonly JsonSerializerSettings s_serializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            DateFormatHandling = DateFormatHandling.IsoDateFormat
        };

        /// <summary>
        /// 保存样本到文件。
        /// </summary>
        public static void Save(string filePath, MeterType meterType,
            IReadOnlyList<ChannelDescriptor> channels, IReadOnlyList<MeterSample> samples)
        {
            SampleDocument document = new SampleDocument
            {
                MeterType = meterType.ToConfigValue(),
                Version = CurrentVersion,
                Channels = new List<ChannelDescriptor>(channels),
                Samples = new List<MeterSample>(samples)
            };
            string json = JsonConvert.SerializeObject(document, s_serializerSettings);
            System.IO.File.WriteAllText(filePath, json, System.Text.Encoding.UTF8);
        }

        /// <summary>
        /// 从文件读取样本，兼容早期不带通道信息的数组格式。
        /// </summary>
        /// <returns>解析失败时返回 null</returns>
        public static SampleDocument Load(string filePath)
        {
            string json = System.IO.File.ReadAllText(filePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            string trimmed = json.TrimStart();
            if (trimmed.StartsWith("[", StringComparison.Ordinal))
            {
                return LoadLegacyArray(json);
            }

            SampleDocument document = JsonConvert.DeserializeObject<SampleDocument>(json, s_serializerSettings);
            if (document == null)
            {
                return null;
            }
            if (document.Version > CurrentVersion)
            {
                throw new NotSupportedException(
                    string.Format("文件格式版本 {0} 高于当前程序支持的 {1}，请升级软件。",
                        document.Version, CurrentVersion));
            }

            MeterType meterType;
            if (!MeterTypeExtensions.TryParseConfigValue(document.MeterType, out meterType))
            {
                meterType = MeterType.Djsf1352;
            }
            if (document.Channels == null || document.Channels.Count == 0)
            {
                document.Channels = new List<ChannelDescriptor>(MeterChannels.GetChannels(meterType));
            }
            document.Samples = document.Samples ?? new List<MeterSample>();
            return document;
        }

        /// <summary>
        /// 解析旧版本数组格式：[{ "time": ..., "voltage": ..., "phaseVoltageA": ... }]。
        /// 旧文件没有记录表型，按首个元素的字段判断是三相还是直流；AMC 与 ADL400 的
        /// 通道定义相同，统一按三相处理不影响结果。
        /// </summary>
        private static SampleDocument LoadLegacyArray(string json)
        {
            JArray array = JArray.Parse(json);
            bool isThreePhase = false;
            if (array.Count > 0)
            {
                JObject first = array[0] as JObject;
                isThreePhase = first != null && first["phaseVoltageA"] != null;
            }

            MeterType meterType = isThreePhase ? MeterType.Amc : MeterType.Djsf1352;
            List<MeterSample> samples = new List<MeterSample>(array.Count);
            foreach (JToken token in array)
            {
                JObject item = token as JObject;
                if (item == null)
                {
                    continue;
                }
                MeterSample sample = isThreePhase
                    ? BuildThreePhaseSample(item)
                    : BuildSinglePhaseSample(item);
                if (sample != null)
                {
                    samples.Add(sample);
                }
            }

            return new SampleDocument
            {
                MeterType = meterType.ToConfigValue(),
                Version = CurrentVersion,
                Channels = new List<ChannelDescriptor>(MeterChannels.GetChannels(meterType)),
                Samples = samples
            };
        }

        /// <summary>
        /// 把旧版直流数据的一条记录转成样本，缺电压字段时返回 null。
        /// </summary>
        /// <param name="item">JSON 数组里的一条记录</param>
        private static MeterSample BuildSinglePhaseSample(JObject item)
        {
            if (item["voltage"] == null)
            {
                return null;
            }
            return new MeterSample(ReadTime(item), new[]
            {
                ReadDouble(item, "voltage"),
                ReadDouble(item, "current"),
                ReadDouble(item, "power")
            });
        }

        /// <summary>
        /// 把旧版三相数据的一条记录转成样本，缺相电压字段时返回 null。
        /// </summary>
        /// <param name="item">JSON 数组里的一条记录</param>
        private static MeterSample BuildThreePhaseSample(JObject item)
        {
            if (item["phaseVoltageA"] == null)
            {
                return null;
            }
            double[] values =
            {
                ReadDouble(item, "phaseVoltageA"),
                ReadDouble(item, "phaseVoltageB"),
                ReadDouble(item, "phaseVoltageC"),
                ReadDouble(item, "lineVoltageUAB"),
                ReadDouble(item, "lineVoltageUBC"),
                ReadDouble(item, "lineVoltageUAC"),
                ReadDouble(item, "currentA"),
                ReadDouble(item, "currentB"),
                ReadDouble(item, "currentC"),
                ReadDouble(item, "activePowerOfPhaseA"),
                ReadDouble(item, "activePowerOfPhaseB"),
                ReadDouble(item, "activePowerOfPhaseC"),
                ReadDouble(item, "totalActivePower"),
                ReadDouble(item, "phaseReactivePowerA"),
                ReadDouble(item, "phaseReactivePowerB"),
                ReadDouble(item, "phaseReactivePowerC"),
                ReadDouble(item, "totalReactivePower")
            };
            return new MeterSample(ReadTime(item), values);
        }

        /// <summary>
        /// 读取时间字段，缺失时返回 DateTime.MinValue。
        /// </summary>
        /// <param name="item">JSON 对象</param>
        private static DateTime ReadTime(JObject item)
        {
            JToken token = item["time"];
            if (token == null || token.Type == JTokenType.Null)
            {
                return DateTime.MinValue;
            }
            return token.Value<DateTime>();
        }

        /// <summary>
        /// 读取数值字段，缺失或为 null 时返回 0。
        /// </summary>
        /// <param name="item">JSON 对象</param>
        /// <param name="propertyName">字段名</param>
        private static double ReadDouble(JObject item, string propertyName)
        {
            JToken token = item[propertyName];
            if (token == null || token.Type == JTokenType.Null)
            {
                return 0;
            }
            return token.Value<double?>() ?? 0;
        }
    }
}
