using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MeterRecorder;
using Xunit;

namespace MeterRecorder.Tests
{
    /// <summary>
    /// 采样服务：端口开关、循环取数、失败中断与释放。用假表代替串口，
    /// 断言的是「循环有没有按预期把结果推出来、异常时有没有关端口」。
    /// </summary>
    public class MeterSamplerTests
    {
        /// <summary>
        /// 未提供电表属于装配错误，应在构造时报出来。
        /// </summary>
        [Fact]
        public void Constructor_NullMeter_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new MeterSampler(null));
        }

        /// <summary>
        /// 启动后应先打开端口，再按间隔推送样本；停止后端口关闭、状态归零。
        /// </summary>
        [Fact]
        public async Task Start_OpensPortAndRaisesSamples()
        {
            FakeMeter meter = new FakeMeter();
            using (MeterSampler sampler = new MeterSampler(meter))
            using (ManualResetEventSlim received = new ManualResetEventSlim(false))
            {
                int count = 0;
                MeterSample lastSample = null;
                sampler.SampleReady += delegate(object sender, MeterSampleEventArgs e)
                {
                    lastSample = e.Sample;
                    Interlocked.Increment(ref count);
                    received.Set();
                };

                sampler.Start(TimeSpan.FromMilliseconds(10));

                Assert.True(sampler.IsRunning);
                Assert.True(received.Wait(TimeSpan.FromSeconds(2)), "两秒内没有收到样本");
                Assert.True(meter.Opened >= 1);

                await sampler.StopAsync();

                Assert.False(sampler.IsRunning);
                Assert.True(meter.Closed >= 1);
                Assert.True(count >= 1);
                Assert.NotNull(lastSample);
                Assert.Equal(3, lastSample.Values.Length);
            }
        }

        /// <summary>
        /// 停止可以重复调用，也允许停下后再启动一次：
        /// 界面上「停止 → 开始」是很常见的操作序列。
        /// </summary>
        [Fact]
        public async Task StopAsync_IsRepeatable_AndAllowsRestart()
        {
            FakeMeter meter = new FakeMeter();
            using (MeterSampler sampler = new MeterSampler(meter))
            {
                sampler.Start(TimeSpan.FromMilliseconds(10));
                await sampler.StopAsync();
                await sampler.StopAsync();

                sampler.Start(TimeSpan.FromMilliseconds(10));
                await sampler.StopAsync();

                Assert.False(sampler.IsRunning);
                Assert.True(meter.Opened >= 2);
                Assert.True(meter.Closed >= 2);
            }
        }

        /// <summary>
        /// 已经启动时再次启动应报错，避免两个循环同时读同一个端口。
        /// </summary>
        [Fact]
        public async Task Start_WhenAlreadyRunning_Throws()
        {
            FakeMeter meter = new FakeMeter();
            using (MeterSampler sampler = new MeterSampler(meter))
            {
                sampler.Start(TimeSpan.FromMilliseconds(10));
                try
                {
                    Assert.Throws<InvalidOperationException>(
                        () => sampler.Start(TimeSpan.FromMilliseconds(10)));
                }
                finally
                {
                    await sampler.StopAsync();
                }
            }
        }

        /// <summary>
        /// 读失败时应先关端口、清掉运行标记，再把异常抛给界面；
        /// 否则界面会显示「正在采集」却再也收不到数据。
        /// </summary>
        [Fact]
        public void ReadFailure_RaisesSampleFailedAndStops()
        {
            FakeMeter meter = new FakeMeter { ReadError = new InvalidOperationException("从站无应答") };
            using (MeterSampler sampler = new MeterSampler(meter))
            using (ManualResetEventSlim failed = new ManualResetEventSlim(false))
            {
                Exception captured = null;
                sampler.SampleFailed += delegate(object sender, SamplingErrorEventArgs e)
                {
                    captured = e.Error;
                    failed.Set();
                };

                sampler.Start(TimeSpan.FromMilliseconds(10));

                Assert.True(failed.Wait(TimeSpan.FromSeconds(2)), "两秒内没有收到失败事件");
                Assert.IsType<InvalidOperationException>(captured);
                Assert.Equal("从站无应答", captured.Message);
                Assert.False(sampler.IsRunning);
                Assert.True(meter.Closed >= 1);
            }
        }

        /// <summary>
        /// 释放时应停止循环并释放底层电表（串口资源随之释放）。
        /// </summary>
        [Fact]
        public void Dispose_StopsLoopAndReleasesMeter()
        {
            FakeMeter meter = new FakeMeter();

            using (MeterSampler sampler = new MeterSampler(meter))
            {
                sampler.Start(TimeSpan.FromMilliseconds(10));
            }

            Assert.Equal(1, meter.DisposedCount);
            Assert.True(meter.Closed >= 1);
        }

        /// <summary>
        /// 重复释放应直接返回，不应重复关闭端口或重复释放电表。
        /// </summary>
        [Fact]
        public void Dispose_Twice_IsSafe()
        {
            FakeMeter meter = new FakeMeter();
            MeterSampler sampler = new MeterSampler(meter);

            sampler.Dispose();
            sampler.Dispose();

            Assert.Equal(1, meter.DisposedCount);
        }

        /// <summary>
        /// 端口已经断开时关闭会抛异常，释放流程必须吞掉它，
        /// 否则关窗口时会弹出一个用户无法处理的错误。
        /// </summary>
        [Fact]
        public void Dispose_WhenCloseThrows_DoesNotThrow()
        {
            FakeMeter meter = new FakeMeter { CloseError = new InvalidOperationException("端口已断开") };
            MeterSampler sampler = new MeterSampler(meter);

            sampler.Dispose();

            Assert.Equal(1, meter.DisposedCount);
        }

        /// <summary>
        /// 释放后再启动应报 ObjectDisposedException，而不是悄悄再开一个循环。
        /// </summary>
        [Fact]
        public void Start_AfterDispose_Throws()
        {
            FakeMeter meter = new FakeMeter();
            MeterSampler sampler = new MeterSampler(meter);
            sampler.Dispose();

            Assert.Throws<ObjectDisposedException>(
                () => sampler.Start(TimeSpan.FromMilliseconds(10)));
        }

        /// <summary>
        /// 内存版电表：可以指定 Read 抛异常，便于验证失败中断路径。
        /// </summary>
        private sealed class FakeMeter : IEnergyMeters
        {
                    /// <summary>
        /// Open 被调用的次数。
        /// </summary>
        private int _opened;
                    /// <summary>
        /// Close 被调用的次数。
        /// </summary>
        private int _closed;
                    /// <summary>
        /// Dispose 被调用的次数。
        /// </summary>
        private int _disposed;

            /// <summary>
            /// 打开端口的次数。
            /// </summary>
            public int Opened
            {
                get { return Volatile.Read(ref _opened); }
            }

            /// <summary>
            /// 关闭端口的次数。
            /// </summary>
            public int Closed
            {
                get { return Volatile.Read(ref _closed); }
            }

            /// <summary>
            /// 释放次数。
            /// </summary>
            public int DisposedCount
            {
                get { return Volatile.Read(ref _disposed); }
            }

            /// <summary>
            /// 非 null 时 Read 抛出该异常。
            /// </summary>
            public Exception ReadError { get; set; }

            /// <summary>
            /// 非 null 时 Close 抛出该异常，用于验证释放流程的容错。
            /// </summary>
            public Exception CloseError { get; set; }

            /// <summary>
            /// 固定返回直流表的三通道定义。
            /// </summary>
            public IReadOnlyList<ChannelDescriptor> Channels
            {
                get { return MeterChannels.SinglePhase; }
            }

            /// <summary>
            /// 是否已打开。
            /// </summary>
            public bool IsOpen { get; private set; }

            /// <summary>
            /// 打开端口。
            /// </summary>
            public void Open()
            {
                IsOpen = true;
                Interlocked.Increment(ref _opened);
            }

            /// <summary>
            /// 关闭端口。
            /// </summary>
            public void Close()
            {
                IsOpen = false;
                Interlocked.Increment(ref _closed);
                if (CloseError != null)
                {
                    throw CloseError;
                }
            }

            /// <summary>
            /// 返回一帧固定数据；配置了 ReadError 时改为抛出。
            /// </summary>
            public MeterSample Read()
            {
                if (ReadError != null)
                {
                    throw ReadError;
                }
                return new MeterSample(DateTime.Now, new[] { 48.2, 12.5, 6.0 });
            }

            /// <summary>
            /// 记录释放次数。
            /// </summary>
            public void Dispose()
            {
                Interlocked.Increment(ref _disposed);
            }
        }
    }
}
