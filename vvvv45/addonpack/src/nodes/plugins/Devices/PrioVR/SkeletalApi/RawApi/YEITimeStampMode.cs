using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrioVR.SkeletalLib.RawApi
{
    public enum YEITimeStampModeEnum : uint //AKA YEI_Timestamp_Mode
    {
        /// <summary>
        /// Disables timestamp, timestamp will return 0
        /// </summary>
        None,

        /// <summary>
        /// 3-Space device's timestamp, this can be set 
        /// with tss_updateCurrentTimestamp
        /// </summary>
        Sensor,

        /// <summary>
        /// The data is timestamped on arrival to the system 
        /// using the high-resolution performance counter
        /// </summary>
        System
    }
}
