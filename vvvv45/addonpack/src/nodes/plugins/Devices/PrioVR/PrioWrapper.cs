using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;

using PrioVR.SkeletalLib.RawApi;

namespace PrioVR
{
    public class PrioWrapper
    {

        static uint[] serial_list = {   0x00000001, 0x00000000, 0x00000008, 
                                        0x00000009, 0x0000000A, 0x0000000C, 
                                        0x0000000D, 0x0000000E, 0x00000004, 
                                        0x00000005, 0x00000010, 0x00000011};

        static byte[] axis_list  = {	(byte)9,	(byte)43,	(byte)37,
                                        (byte)37,	(byte)37,	(byte)13,			
                                        (byte)13,	(byte)13,	(byte)52,			
                                        (byte)52,	(byte)28,	(byte)28};
	
        //int list_length = sizeof(serial_list)/sizeof(uint);
        static int list_length = serial_list.Length;
        static int serial_listBytelength = list_length * sizeof(uint);
        
        //char discovered_list[sizeof(serial_list)/sizeof(unsigned int)];
        static int discovered_listLength = serial_listBytelength / sizeof(uint);
        static char[] discovered_list = new char[discovered_listLength];

	    //YEI_Quat stream_data[sizeof(serial_list)/sizeof(unsigned int)];
	    uint skeletal_device;
        YEIErrorEnum error;
        uint serial = (uint)0x00000000;
        
        void setupPrioVR()
        {
            skeletal_device = SkeletalApiInterop.yei_setUpPrioVRSensors(serial, serial_list, axis_list,discovered_list, list_length, YEITimeStampModeEnum.Sensor);
        }

    }
}
