using System;
using System.Threading;
using System.Threading.Tasks;

namespace MeterRecorder
{
    /// <summary>
    /// 采样完成事件的参数。
    /// </summary>
    public sealed class MeterSampleEventArgs : EventArgs
    {
        /// <summary>
        /// 初始化采样完成事件参数。
        /// </summary>
        /// <param name="sample">本次采样结果</param>
        public MeterSampleEventArgs(MeterSample sample)
        {
            Sample = sample;
        }

        /// <summary>
        /// 本次采样结果。
        /// </summary>
        public MeterSample Sample { get; }
    }

    /// <summary>
    /// 采集中断事件的参数。
    /// </summary>
    public sealed class SamplingErrorEventArgs : EventArgs
    {
        /// <summary>
        /// 初始化采集中断事件参数。
        /// </summary>
        /// <param name="error">导致中断的异常</param>
        public SamplingErrorEventArgs(Exception error)
        {
            Error = error;
        }

        /// <summary>
        /// 导致采集中断的异常。
        /// </summary>
        public Exception Error { get; }
    }

    /// <summary>
    /// 定时采集服务：负责端口开关、采样循环与取消，通过事件把结果推给界面。
    /// 事件在后台线程触发，订阅方需自行切回界面线程。
    /// </summary>
    public sealed class MeterSampler : IDisposable
    {
        /// <summary>
        /// 被采集的电表。
        /// </summary>
        private readonly IEnergyMeters _meter;
        /// <summary>
        /// 当前采集循环的取消源，未采集时为 null。
        /// </summary>
        private CancellationTokenSource _cancellation;
        /// <summary>
        /// 采集循环所在的任务，停止时用它等待线程退出。
        /// </summary>
        private Task _loop;
        /// <summary>
        /// 两次采样的间隔。
        /// </summary>
        private TimeSpan _interval;
        /// <summary>
        /// 是否已释放。
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// 为指定电表创建采集服务。
        /// </summary>
        /// <param name="meter">要采集的电表</param>
        public MeterSampler(IEnergyMeters meter)
        {
            if (meter == null)
            {
                throw new ArgumentNullException(nameof(meter));
            }
            _meter = meter;
        }

        /// <summary>
        /// 采集到一帧数据时触发。
        /// </summary>
        public event EventHandler<MeterSampleEventArgs> SampleReady;

        /// <summary>
        /// 采集中断时触发，触发前端口已关闭。
        /// </summary>
        public event EventHandler<SamplingErrorEventArgs> SampleFailed;

        /// <summary>
        /// 是否正在采集。
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// 开始采集。端口打开失败时直接抛出异常，由调用方提示用户。
        /// </summary>
        /// <param name="interval">两次采样的间隔</param>
        public void Start(TimeSpan interval)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(MeterSampler));
            }
            if (IsRunning)
            {
                throw new InvalidOperationException("采集已经启动。");
            }

            _interval = interval;
            _meter.Open();

            _cancellation = new CancellationTokenSource();
            IsRunning = true;
            CancellationToken token = _cancellation.Token;
            _loop = Task.Run(() => Run(token));
        }

        /// <summary>
        /// 停止采集并关闭端口。可重复调用，采集异常中断后再调用也不会出错。
        /// </summary>
        public async Task StopAsync()
        {
            CancellationTokenSource cancellation = _cancellation;
            if (cancellation != null)
            {
                cancellation.Cancel();
            }

            Task loop = _loop;
            if (loop != null)
            {
                try
                {
                    await loop.ConfigureAwait(false);
                }
                catch (Exception)
                {
                    // 采集线程的异常已在 Run 内部处理并上报，这里只等它退出。
                }
            }

            CloseQuietly();
            IsRunning = false;

            if (cancellation != null)
            {
                cancellation.Dispose();
            }
            _cancellation = null;
            _loop = null;
        }

        /// <summary>
        /// 释放资源，必要时先停止采集。
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;

            CancellationTokenSource cancellation = _cancellation;
            if (cancellation != null)
            {
                cancellation.Cancel();
            }
            Task loop = _loop;
            if (loop != null)
            {
                try
                {
                    // 主窗体关闭前会先异步停止采集，这里只是兜底。等待过久会把界面卡住，
                    // 500 ms 足够让一次读操作收尾。
                    loop.Wait(TimeSpan.FromMilliseconds(500));
                }
                catch (Exception)
                {
                }
            }
            CloseQuietly();
            _meter.Dispose();
            if (cancellation != null)
            {
                cancellation.Dispose();
            }
            _cancellation = null;
            _loop = null;
            IsRunning = false;
        }

        /// <summary>
        /// 采集循环：读一帧、抛出事件、等待一个间隔，直到取消或异常。
        /// </summary>
        /// <param name="token">取消标记</param>
        private void Run(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    MeterSample sample = _meter.Read();
                    EventHandler<MeterSampleEventArgs> handler = SampleReady;
                    if (handler != null)
                    {
                        handler(this, new MeterSampleEventArgs(sample));
                    }
                    token.WaitHandle.WaitOne(_interval);
                }
            }
            catch (Exception ex)
            {
                IsRunning = false;
                CloseQuietly();
                EventHandler<SamplingErrorEventArgs> handler = SampleFailed;
                if (handler != null)
                {
                    handler(this, new SamplingErrorEventArgs(ex));
                }
            }
        }

        /// <summary>
        /// 关闭端口并忽略异常。端口已断开时关闭会抛错，此处不必上报。
        /// </summary>
        private void CloseQuietly()
        {
            try
            {
                _meter.Close();
            }
            catch (Exception)
            {
                // 端口已经断开时关闭会抛异常，这里忽略即可。
            }
        }
    }
}
