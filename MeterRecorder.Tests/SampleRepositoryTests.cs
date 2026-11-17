using System;
using System.Collections.Generic;
using System.IO;
using MeterRecorder;
using Xunit;

namespace MeterRecorder.Tests
{
    public class SampleRepositoryTests
    {
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
    }
}
