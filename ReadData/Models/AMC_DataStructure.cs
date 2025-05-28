using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReadData
{
    public class AMC_DataStructure
    {
        /// <summary>
        /// 时间
        /// </summary>
        public DateTime Time { get; set; }
        /// <summary>
        /// 相电压UA
        /// </summary>
        public double PhaseVoltageA { get; set;}   
        /// <summary>
        /// 相电压UB
        /// </summary>
        public double PhaseVoltageB { get; set;}
        /// <summary>
        /// 相电压UC
        /// </summary>
        public double PhaseVoltageC { get; set;}
        /// <summary>
        /// 线电压UAB
        /// </summary>
        public double LineVoltageUAB {  get; set;}
        /// <summary>
        /// 线电压UBC
        /// </summary>
        public double LineVoltageUBC { get; set; }
        /// <summary>
        /// 线电压UAC
        /// </summary>
        public double LineVoltageUAC { get; set; }
        /// <summary>
        /// 电流IA
        /// </summary>
        public double CurrentA { get; set; }
        /// <summary>
        /// 电流IB
        /// </summary>
        public double CurrentB { get; set; }
        /// <summary>
        /// 电流IC
        /// </summary>
        public double CurrentC { get; set; }
        /// <summary>
        /// A 相有功功率
        /// </summary>
        public double ActivePowerOfPhaseA { get; set; }
        /// <summary>
        /// B 相有功功率
        /// </summary>
        public double ActivePowerOfPhaseB { get; set; }
        /// <summary>
        /// C 相有功功率
        /// </summary>
        public double ActivePowerOfPhaseC { get; set; }
        /// <summary>
        /// 总有功功率
        /// </summary>
        public double TotalActivePower { get; set; }
        /// <summary>
        /// A 相无功功率
        /// </summary>
        public double PhaseReactivePowerA { get; set; }
        /// <summary>
        /// B 相无功功率
        /// </summary>
        public double PhaseReactivePowerB { get; set; }
        /// <summary>
        /// C 相无功功率
        /// </summary>
        public double PhaseReactivePowerC { get; set; }
        /// <summary>
        /// 总无功功率
        /// </summary>
        public double TotalReactivePower { get; set; }

    }
}
