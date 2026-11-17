using System;
using System.Collections.Generic;
using System.IO;
using MeterRecorder;
using Xunit;

namespace MeterRecorder.Tests
{
    /// <summary>
    /// 数据文件读取：区分新版包装格式与旧版裸数组格式，
    /// 后者没有表型字段，需要标出来让界面提示。
    /// </summary>
    public class SampleRepositoryTests
    {
        /// <summary>
        /// 旧版是不带包装的数组、没有表型字段，加载后按通道推断为三相，
        /// 并置上旧格式标记供回放窗体提示。
        /// </summary>
        [Fact]
        public void Load_LegacyArray_MarksDocumentAndInfersChannels()
        {
            string directory = Path.Combine(Path.GetTempPath(), "MeterRecorderTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, "legacy.json");
            try
            {
                File.WriteAllText(path,
                    "[{\"time\":\"2026-11-17T09:00:00\",\"phaseVoltageA\":220.4,\"phaseVoltageB\":219.8," +
                    "\"phaseVoltageC\":220.1,\"lineVoltageUAB\":381.6,\"lineVoltageUBC\":381.2," +
                    "\"lineVoltageUAC\":381.4,\"currentA\":9.46,\"currentB\":9.51,\"currentC\":9.38," +
                    "\"activePowerOfPhaseA\":1.24,\"activePowerOfPhaseB\":1.26,\"activePowerOfPhaseC\":1.22," +
                    "\"totalActivePower\":3.72,\"phaseReactivePowerA\":0.1,\"phaseReactivePowerB\":0.2," +
                    "\"phaseReactivePowerC\":0.3,\"totalReactivePower\":0.6}]");

                SampleDocument document = SampleRepository.Load(path);

                Assert.True(document.IsLegacyFormat);
                Assert.Equal("AMC", document.MeterType);
                Assert.Equal(17, document.Channels.Count);
                Assert.Equal(220.4, document.Samples[0].GetValue(0), 3);
                Assert.Equal(3.72, document.Samples[0].GetValue(12), 3);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        /// <summary>
        /// 新版文件自带表型与通道定义，加载后不应被误标成旧格式。
        /// </summary>
        [Fact]
        public void Load_WrappedDocument_IsNotMarkedAsLegacy()
        {
            string directory = Path.Combine(Path.GetTempPath(), "MeterRecorderTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, "current.json");
            try
            {
                List<MeterSample> samples = new List<MeterSample>
                {
                    new MeterSample(new DateTime(2026, 11, 17, 9, 0, 0), new double[17])
                };
                SampleRepository.Save(path, MeterType.Adl400,
                    MeterChannels.GetChannels(MeterType.Adl400), samples);

                SampleDocument document = SampleRepository.Load(path);

                Assert.False(document.IsLegacyFormat);
                Assert.Equal("ADL400", document.MeterType);
                Assert.Equal(17, document.Channels.Count);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        /// <summary>
        /// 空文件（或只写了空白）没有可解析的内容，应返回 null 让界面提示，
        /// 而不是抛异常或返回一个空文档。
        /// </summary>
        [Fact]
        public void Load_WhitespaceFile_ReturnsNull()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                string path = temp.File("empty.json");
                File.WriteAllText(path, "   \r\n\t ");

                Assert.Null(SampleRepository.Load(path));
            }
        }

        /// <summary>
        /// 文件版本高于当前程序时明确拒绝：新版本可能加了列或改了含义，
        /// 强行按旧格式解析会读出错误的数。
        /// </summary>
        [Fact]
        public void Load_FutureVersion_Throws()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                string path = temp.File("future.json");
                File.WriteAllText(path,
                    "{\"MeterType\":\"ADL400\",\"Version\":" + (SampleRepository.CurrentVersion + 1) + "}");

                Assert.Throws<NotSupportedException>(() => SampleRepository.Load(path));
            }
        }

        /// <summary>
        /// 文件里没有通道定义时，按表型补一份，回放界面才能画出表头与卡片。
        /// </summary>
        [Fact]
        public void Load_MissingChannels_FillsFromMeterType()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                string path = temp.File("no-channels.json");
                File.WriteAllText(path, "{\"MeterType\":\"ADL400\",\"Version\":1}");

                SampleDocument document = SampleRepository.Load(path);

                Assert.Equal(17, document.Channels.Count);
                Assert.NotNull(document.Samples);
                Assert.Empty(document.Samples);
            }
        }

        /// <summary>
        /// 型号字段缺失或写错时回落到直流表，与配置文件的行为保持一致。
        /// </summary>
        [Fact]
        public void Load_UnknownMeterType_FallsBackToDcMeter()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                string path = temp.File("unknown.json");
                File.WriteAllText(path, "{\"MeterType\":\"ADL999\",\"Version\":1}");

                SampleDocument document = SampleRepository.Load(path);

                Assert.Equal("DJSF1352", document.MeterType);
                Assert.Equal(3, document.Channels.Count);
            }
        }

        /// <summary>
        /// 旧版直流文件只有电压、电流、功率三个字段，加载后应按顺序映射成三个通道。
        /// </summary>
        [Fact]
        public void Load_LegacySinglePhaseArray_MapsThreeValues()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                string path = temp.File("legacy-dc.json");
                File.WriteAllText(path,
                    "[{\"time\":\"2026-11-17T09:00:00\",\"voltage\":48.2,\"current\":12.5,\"power\":6.0}]");

                SampleDocument document = SampleRepository.Load(path);

                Assert.True(document.IsLegacyFormat);
                Assert.Equal("DJSF1352", document.MeterType);
                Assert.Equal(3, document.Channels.Count);
                Assert.Equal(48.2, document.Samples[0].GetValue(0), 3);
                Assert.Equal(12.5, document.Samples[0].GetValue(1), 3);
                Assert.Equal(6.0, document.Samples[0].GetValue(2), 3);
            }
        }

        /// <summary>
        /// 旧文件里可能有采集失败留下的空记录；缺电压字段的记录应被跳过，
        /// 其余正常记录仍要保留，不能因为一条坏数据丢掉整份文件。
        /// </summary>
        [Fact]
        public void Load_LegacyRecordWithoutVoltage_IsSkipped()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                string path = temp.File("legacy-partial.json");
                File.WriteAllText(path,
                    "[{\"time\":\"2026-11-17T09:00:00\",\"current\":1.0}," +
                    "{\"time\":\"2026-11-17T09:00:01\",\"voltage\":48.2,\"current\":12.5,\"power\":6.0}]");

                SampleDocument document = SampleRepository.Load(path);

                Assert.Single(document.Samples);
                Assert.Equal(48.2, document.Samples[0].GetValue(0), 3);
            }
        }

        /// <summary>
        /// 旧记录缺时间字段时记为 DateTime.MinValue，排序与显示交给界面处理。
        /// </summary>
        [Fact]
        public void Load_LegacyRecordWithoutTime_UsesMinValue()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                string path = temp.File("legacy-no-time.json");
                File.WriteAllText(path, "[{\"voltage\":48.2}]");

                Assert.Equal(DateTime.MinValue, SampleRepository.Load(path).Samples[0].Time);
            }
        }

        /// <summary>
        /// 文件内容就是字面量 null（例如被别的程序截断）时应返回 null，
        /// 让回放界面走「文件无效」的提示分支。
        /// </summary>
        [Fact]
        public void Load_NullLiteral_ReturnsNull()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                string path = temp.File("null.json");
                File.WriteAllText(path, "null");

                Assert.Null(SampleRepository.Load(path));
            }
        }

        /// <summary>
        /// 旧数组里混进非对象元素（数字、字符串）时应跳过，
        /// 不能因为一条脏数据让整份文件打不开。
        /// </summary>
        [Fact]
        public void Load_LegacyNonObjectElement_IsSkipped()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                string path = temp.File("legacy-mixed.json");
                File.WriteAllText(path, "[1,\"坏数据\",{\"voltage\":48.2}]");

                SampleDocument document = SampleRepository.Load(path);

                Assert.Single(document.Samples);
                Assert.Equal(48.2, document.Samples[0].GetValue(0), 3);
            }
        }

        /// <summary>
        /// 三相旧文件里缺相电压的记录同样跳过；判定依据是首条记录有该字段，
        /// 后续记录不会改成按直流解析。
        /// </summary>
        [Fact]
        public void Load_LegacyThreePhaseRecordWithoutPhaseVoltage_IsSkipped()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                string path = temp.File("legacy-3p-partial.json");
                File.WriteAllText(path,
                    "[{\"phaseVoltageA\":220.4},{\"time\":\"2026-11-17T09:00:00\"}]");

                SampleDocument document = SampleRepository.Load(path);

                Assert.Equal("AMC", document.MeterType);
                Assert.Single(document.Samples);
                Assert.Equal(220.4, document.Samples[0].GetValue(0), 3);
            }
        }

        /// <summary>
        /// 空数组是一份合法但没有数据的旧文件，应得到 0 条样本而不是抛异常。
        /// </summary>
        [Fact]
        public void Load_LegacyEmptyArray_ReturnsNoSamples()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                string path = temp.File("legacy-empty.json");
                File.WriteAllText(path, "[]");

                SampleDocument document = SampleRepository.Load(path);

                Assert.True(document.IsLegacyFormat);
                Assert.Empty(document.Samples);
                Assert.Equal(3, document.Channels.Count);
            }
        }
    }
}
