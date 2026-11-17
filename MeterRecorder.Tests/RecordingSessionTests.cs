using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using MeterRecorder;
using Xunit;

namespace MeterRecorder.Tests
{
    public class RecordingSessionTests
    {
        [Fact]
        public void Add_CountsSamples()
        {
            RecordingSession session = NewSession();

            session.Add(NewSample(new DateTime(2026, 11, 17, 9, 0, 0)));
            session.Add(NewSample(new DateTime(2026, 11, 17, 9, 0, 1)));

            Assert.Equal(2, session.Count);
            Assert.Equal(2, session.Samples.Count);
        }

        [Fact]
        public void BuildFileName_UsesFirstAndLastSampleTime()
        {
            RecordingSession session = NewSession();
            session.Add(NewSample(new DateTime(2026, 11, 17, 9, 0, 0)));
            session.Add(NewSample(new DateTime(2026, 11, 17, 9, 30, 5)));

            Assert.Equal("20261117090000至20261117093005.json", session.BuildFileName());
        }

        [Fact]
        public void BuildFileName_WithoutSamples_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => NewSession().BuildFileName());
        }

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

        [Fact]
        public void Clear_RemovesSamples()
        {
            RecordingSession session = NewSession();
            session.Add(NewSample(DateTime.Now));

            session.Clear();

            Assert.Equal(0, session.Count);
        }

        private static RecordingSession NewSession()
        {
            return new RecordingSession(MeterType.Adl400, MeterChannels.GetChannels(MeterType.Adl400));
        }

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
