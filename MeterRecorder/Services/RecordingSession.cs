using System;
using System.Collections.Generic;
using System.IO;

namespace MeterRecorder
{
    /// <summary>
    /// 一次采集会话：持有本次样本，负责保存与清空。界面只读取它的集合，
    /// 不再自己维护样本状态，也不必知道文件命名规则。
    /// </summary>
    public sealed class RecordingSession
    {
        private readonly List<MeterSample> _samples = new List<MeterSample>();
        private readonly MeterType _meterType;
        private readonly IReadOnlyList<ChannelDescriptor> _channels;

        /// <summary>
        /// 为指定表型与通道定义创建会话。
        /// </summary>
        /// <param name="meterType">表型，写入数据文件的表头</param>
        /// <param name="channels">通道定义，写入数据文件的表头</param>
        public RecordingSession(MeterType meterType, IReadOnlyList<ChannelDescriptor> channels)
        {
            _meterType = meterType;
            _channels = channels ?? new ChannelDescriptor[0];
        }

        /// <summary>
        /// 本次采集的样本，按时间顺序排列。表格与曲线直接读这个集合。
        /// </summary>
        public IReadOnlyList<MeterSample> Samples
        {
            get { return _samples; }
        }

        /// <summary>
        /// 已采集的样本条数。
        /// </summary>
        public int Count
        {
            get { return _samples.Count; }
        }

        /// <summary>
        /// 追加一帧样本。
        /// </summary>
        /// <param name="sample">采样结果</param>
        public void Add(MeterSample sample)
        {
            if (sample == null)
            {
                return;
            }
            _samples.Add(sample);
        }

        /// <summary>
        /// 清空本次采集。
        /// </summary>
        public void Clear()
        {
            _samples.Clear();
        }

        /// <summary>
        /// 用首末样本时间生成文件名，如 20261116142000至20261116143000.json。
        /// </summary>
        public string BuildFileName()
        {
            if (_samples.Count == 0)
            {
                throw new InvalidOperationException("没有可保存的数据。");
            }
            string startTime = _samples[0].Time.ToString("yyyyMMddHHmmss");
            string endTime = _samples[_samples.Count - 1].Time.ToString("yyyyMMddHHmmss");
            return startTime + "至" + endTime + ".json";
        }

        /// <summary>
        /// 保存到指定目录，返回文件完整路径。写盘失败时抛出异常，由调用方提示用户。
        /// </summary>
        /// <param name="directory">数据目录，不存在时自动创建</param>
        public string Save(string directory)
        {
            if (string.IsNullOrEmpty(directory))
            {
                throw new ArgumentException("数据目录不能为空。", "directory");
            }
            string fileName = BuildFileName();
            Directory.CreateDirectory(directory);
            string filePath = Path.Combine(directory, fileName);
            SampleRepository.Save(filePath, _meterType, _channels, _samples);
            return filePath;
        }
    }
}
