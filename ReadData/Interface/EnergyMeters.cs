using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReadDataSoftware
{
    public  interface EnergyMeters
    {
        bool IsOpen { get; }
        void Open();

        void Close();

        double[] ReadVoltage();

        double[] ReadCurrent();

        double[] ReadPower();

    }
}
