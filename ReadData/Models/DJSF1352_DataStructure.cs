using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReadDataSoftware
{
    /// <summary>
    /// DJSF1352电表数据类
    /// </summary>
    public  class DJSF1352_DataStructure
    {
        /// <summary>
        /// 时间
        /// </summary>
        public DateTime Time { get; set; }
        /// <summary>
        /// 电压
        /// </summary>
       public double Voltage { get; set; }
        /// <summary>
        /// 电流
        /// </summary>
        public double Current { get; set; }
        /// <summary>
        /// 功率
        /// </summary>
        public double Power { get; set; } 
    }
}
