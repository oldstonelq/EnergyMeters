using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using MeterRecorder;
using Xunit;

namespace MeterRecorder.Tests
{
    /// <summary>
    /// 一次采集会话：样本累积、文件命名、落盘与清空。
    /// 落盘的用例会真的写临时目录，结束时删除。
    /// </summary>
    public class RecordingSessionTests
    {
        /// <summary>
        /// 追加样本后计数与集合长度同步，界面显示与落盘读的是同一份数据。
        /// </summary>
        [Fact]
        public void Add_CountsSamples()
        {
            RecordingSession session = NewSession();

            session.Add(NewSample(new DateTime(2026, 11, 17, 9, 0, 0)));
            session.Add(NewSample(new DateTime(2026, 11, 17, 9, 0, 1)));

            Assert.Equal(2, session.Count);
            Assert.Equal(2, session.Samples.Count);
        }

        /// <summary>
        /// 文件名取首末样本时间、精确到秒，便于在数据目录里按时间段辨认。
        /// </summary>
        [Fact]
        public void BuildFileName_UsesFirstAndLastSampleTime()
        {
            RecordingSession session = NewSession();
            session.Add(NewSample(new DateTime(2026, 11, 17, 9, 0, 0)));
            session.Add(NewSample(new DateTime(2026, 11, 17, 9, 30, 5)));

            Assert.Equal("20261117090000至20261117093005.json", session.BuildFileName());
        }

        /// <summary>
        /// 没有样本时确定不了时间段，应抛异常而不是生成一个非法文件名。
        /// </summary>
        [Fact]
        public void BuildFileName_WithoutSamples_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => NewSession().BuildFileName());
        }

        /// <summary>
        /// 落盘后重新加载，表型、通道数与数值都应一致，
        /// 用来守住写入与读取两端的格式没有错位。
        /// </summary>
        [Fact]
        public void Save_WritesFileThatCanBeLoadedBack()
        {
            string directory = Path.Combine(Path.GetTempPath(), "MeterRecorderTests", Guid.NewGuid().ToString("N"));
            try
            {
                RecordingSession session = NewSession();
                session.Add(NewSample(new DateTime(2026, 11, 17, 9, 0, 0)));
                session.Add(NewSample(new DateTime(2026, 11, 17, 9, 0, 1)));

                string filePath = session.Save(directory);

                Assert.True(File.Exists(filePath));
                Assert.Matches(new Regex(@"^\d{14}至\d{14}\.json$"), Path.GetFileName(filePath));

                SampleDocument document = SampleRepository.Load(filePath);
                Assert.Equal("ADL400", document.MeterType);
                Assert.Equal(2, document.Samples.Count);
                Assert.Equal(17, document.Channels.Count);
                Assert.Equal(220.4, document.Samples[0].GetValue(0), 3);
            }
            finally
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
        }

        /// <summary>
        /// 清空后计数归零，下一轮采集从空集合重新开始。
        /// </summary>
        [Fact]
        public void Clear_RemovesSamples()
        {
            RecordingSession session = NewSession();
            session.Add(NewSample(DateTime.Now));

            session.Clear();

            Assert.Equal(0, session.Count);
        }

        /// <summary>
        /// 构造一份三相会话，通道定义取自 ADL400，样本值域与真实读数一致。
        /// </summary>
        /// <summary>
        /// 采集回调偶尔会给出 null（例如上一帧失败后继续），会话应忽略它，
        /// 不能让它进入集合、把后面的保存与表格都带坏。
        /// </summary>
        [Fact]
        public void Add_NullSample_IsIgnored()
        {
            RecordingSession session = NewSession();

            session.Add(null);
            session.Add(NewSample(new DateTime(2026, 11, 17, 9, 0, 0)));

            Assert.Equal(1, session.Count);
        }

        /// <summary>
        /// 数据目录为空时属于调用方错误，应在写盘前报出来。
        /// </summary>
        [Fact]
        public void Save_EmptyDirectory_Throws()
        {
            RecordingSession session = NewSession();
            session.Add(NewSample(DateTime.Now));

            Assert.Throws<ArgumentException>(() => session.Save(string.Empty));
        }

        /// <summary>
        /// 首次运行时数据目录还不存在，保存应自行创建而不是失败。
        /// </summary>
        [Fact]
        public void Save_CreatesMissingDirectory()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                RecordingSession session = NewSession();
                session.Add(NewSample(new DateTime(2026, 11, 17, 9, 0, 0)));

                string filePath = session.Save(temp.File(@"nested\data"));

                Assert.True(File.Exists(filePath));
            }
        }

        /// <summary>
        /// 直流表只有三个通道，落盘后通道键与数值都应完整保留：
        /// 回放时按 Key 找曲线，键丢了就画不出线。
        /// </summary>
        [Fact]
        public void Save_DcSession_RoundTripsThreeChannels()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                RecordingSession session = NewSession(MeterType.Djsf1352);
                session.Add(NewDcSample(new DateTime(2026, 11, 17, 9, 0, 0)));
                session.Add(NewDcSample(new DateTime(2026, 11, 17, 9, 0, 1)));

                SampleDocument document = SampleRepository.Load(session.Save(temp.Root));

                Assert.Equal("DJSF1352", document.MeterType);
                Assert.Equal(3, document.Channels.Count);
                Assert.Equal("voltage", document.Channels[0].Key);
                Assert.Equal("current", document.Channels[1].Key);
                Assert.Equal("power", document.Channels[2].Key);
                Assert.Equal(2, document.Samples.Count);
                Assert.Equal(48.2, document.Samples[0].GetValue(0), 3);
                Assert.Equal(6.0, document.Samples[1].GetValue(2), 3);
            }
        }

        private static RecordingSession NewSession()
        {
            return NewSession(MeterType.Adl400);
        }

        private static RecordingSession NewSession(MeterType meterType)
        {
            return new RecordingSession(meterType, MeterChannels.GetChannels(meterType));
        }

        private static MeterSample NewDcSample(DateTime time)
        {
            return new MeterSample(time, new[] { 48.2, 12.5, 6.0 });
        }

        /// <summary>
        /// 造一帧 17 通道样本：首通道给固定电压，其余按下标递增，
        /// 串位时能直接从数值看出是第几项错位。
        /// </summary>
        /// <param name="time">样本时间</param>
        /// <returns>样本对象</returns>
        private static MeterSample NewSample(DateTime time)
        {
            double[] values = new double[17];
            values[0] = 220.4;
            for (int i = 1; i < values.Length; i++)
            {
                values[i] = i;
            }
            return new MeterSample(time, values);
        }
    }
}
