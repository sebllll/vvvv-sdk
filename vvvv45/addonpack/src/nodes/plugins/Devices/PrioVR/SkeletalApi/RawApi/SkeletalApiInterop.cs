using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace PrioVR.SkeletalLib.RawApi
{

    /// <summary>
    /// 
    /// </summary>
    public class SkeletalApiInterop
    {

        [DllImport("Skeletal_API.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "yei_setUpThreeSpaceSensors")]
        public static extern uint /*YEI_Device_Id*/setUpThreeSpaceSensors(   uint[] serial_list,
                                                                                byte[] axis_list, //unsigned char[]
                                                                                int list_length,
                                                                                int search_wireless,
                                                                                int search_unknown_devices,
                                                                                YEITimeStampModeEnum mode); 

        [DllImport("Skeletal_API.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "yei_setUpPrioVRSensors")]
        public static extern uint /*YEI_Device_Id*/ yei_setUpPrioVRSensors( uint priovr_serial_number,
                                                                            uint[] logical_id_list,
                                                                            byte[] axis_byte_list, //unsigned char *
								                                            char[] discovered_list,
								                                            int list_length,
                                                                            YEITimeStampModeEnum mode);

        [DllImport("Skeletal_API.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "yei_getNodeError")]
        protected static extern YEIErrorEnum yei_getNodeError(  uint device, // YEI_Device_Id
                                                                out YEIErrorEnum[], //YEI_Error * error_list
                                                                out int list_length);

        [DllImport("Skeletal_API.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "yei_reDiscoverSensors")]
        protected static extern YEIErrorEnum yei_reDiscoverSensors(  uint device, // YEI_Device_Id 
                                                                        out char[] discovered_list, //char * discovered_list
                                                                        out int list_length);

        [DllImport("Skeletal_API.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "yei_tareSensors")]
        protected static extern YEIErrorEnum yei_tareSensors(uint device /*YEI_Device_Id */);

        [DllImport("Skeletal_API.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "yei_startStreaming")]
        protected static extern YEIErrorEnum yei_startStreaming(uint device /*YEI_Device_Id */);

        [DllImport("Skeletal_API.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "yei_stopStreaming")]
        protected static extern YEIErrorEnum yei_stopStreaming(uint device /*YEI_Device_Id */);

        [DllImport("Skeletal_API.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "yei_tareSensors")]
        protected static extern YEIErrorEnum yei_tareSensors(uint device /*YEI_Device_Id */);

        [DllImport("Skeletal_API.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "yei_getLastStreamDataAll")]
        protected static extern YEIErrorEnum yei_getLastStreamDataAll(  uint device, // YEI_Device_Id
                                                                        out char * output_data,
                                                                        out uint output_data_len,
                                                                        out uint * timestamp);

        [DllImport("Skeletal_API.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "yei_resetSkeletalAPI")]
        protected static extern YEIErrorEnum yei_resetSkeletalAPI(uint device /*YEI_Device_Id */);

        [DllImport("Skeletal_API.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "yei_testMath")]
        protected static extern YEIErrorEnum yei_testMath();


    }

}