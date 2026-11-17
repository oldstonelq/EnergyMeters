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
        private static RecordingSession NewSession()
        {
            return new RecordingSession(MeterType.Adl400, MeterChannels.GetChannels(MeterType.Adl400));
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
