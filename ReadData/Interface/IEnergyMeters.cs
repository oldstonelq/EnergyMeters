using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReadDataSoftware
{
    /// <summary>
    /// 电能表统一读取接口
    /// </summary>
    public interface IEnergyMeters
    {
        bool IsOpen { get; }
        void Open();

        void Close();

        double[] ReadVoltage();

        double[] ReadCurrent();

        double[] ReadPower();

    }
}
