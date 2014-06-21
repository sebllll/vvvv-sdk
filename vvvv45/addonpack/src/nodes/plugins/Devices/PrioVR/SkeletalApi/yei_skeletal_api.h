/********************************************//**
 * \file yei_skeletal_api.h
 * \brief  ThreeSpace API 2.0 (PrioVR API beta)
 * \author Chris George
 * \author Daniel Morrison
 * \copyright Copyright 1998-2014, YEI Corporation.
 *
 * The YEI 3-Space C API is released under the YEI 3-Space Open Source License, which allows for both
 * non-commercial use and commercial use with certain restrictions.
 *
 * For Non-Commercial Use, your use of Covered Works is governed by the GNU GPL v.3, subject to the YEI 3-Space Open
 * Source Licensing Overview and Definitions.
 *
 * For Commercial Use, a YEI Commercial/Redistribution License is required, pursuant to the YEI 3-Space Open Source
 * Licensing Overview and Definitions. Commercial Use, for the purposes of this License, means the use, reproduction
 * and/or Distribution, either directly or indirectly, of the Covered Works or any portion thereof, or a Compilation,
 * Improvement, or Modification, for Pecuniary Gain. A YEI Commercial/Redistribution License may or may not require
 * payment, depending upon the intended use.
 *
 * Full details of the YEI 3-Space Open Source License can be found in license.txt
 * License also available online at http://www.yeitechnology.com/yei-3-space-open-source-license
 ***********************************************/
#ifndef YEI_SKELETAL_API_INCLUDED
#define YEI_SKELETAL_API_INCLUDED


#if defined(YEI_STATIC_LIB)
#define TSS_EXPORT
#elif defined(_MSC_VER)
#define YEI_EXPORT __declspec(dllexport)
#elif defined(__GNUC__)
#define YEI_EXPORT __attribute__ ((dllexport))
#endif

typedef unsigned int YEI_Device_Id;

typedef enum YEI_Timestamp_Mode{
    YEI_TIMESTAMP_NONE,   /**< Disables timestamp, timestamp will return 0 */
    YEI_TIMESTAMP_SENSOR, /**< 3-Space device's timestamp, this can be set with tss_updateCurrentTimestamp */
    YEI_TIMESTAMP_SYSTEM  /**< The data is timestamped on arrival to the system using the high-resolution performance counter */
}YEI_Timestamp_Mode;

typedef enum YEI_Type
{
    YEI_3_SPACE_SENSOR,
    YEI_PRIOVR_SYSTEM
}YEI_Type;

typedef enum YEI_Error
{
    YEI_NO_ERROR,                   /**< The API call successfully executed */
    YEI_ERROR_COMMAND_FAIL,         /**< The command returned a failed response */
    YEI_INVALID_COMMAND,            /**< The API call was made on a device type that does not support the attempted command */
    YEI_INVALID_ID,                 /**< The TSS_Device_Id parameter passed in to an API call is not associated with a connected 3-Space device */
    YEI_ERROR_PARAMETER,            /**< General parameter fail */
    YEI_ERROR_TIMEOUT,              /**< The command's timeout has been reached */
    YEI_ERROR_WRITE,                /**< The API call executed failed to write all the data necessary to execute the command to the intended 3-Space device */
    YEI_ERROR_READ,                 /**< The API call executed failed to read all the data necessary to execute the command to the intended 3-Space device */
    YEI_ERROR_STREAM_SLOTS_FULL,    /**< The 3-Space device's stream slots are full */
    YEI_ERROR_STREAM_CONFIG,        /**< The 3-Space device's stream configuration is corrupted */
    YEI_ERROR_MEMORY,               /**< A memory error occurred in the API */
    YEI_ERROR_FIRMWARE_INCOMPATIBLE,/**< 3-Space device firmware does not support that command, firmware update highly recommended */
    YEI_ERROR_NODE_FAIL,            /**< A node in the Skeleton reported error, call yei_getNodeError to see whitch node and its error */
    YEI_ERROR_NODE_NOT_PRESENT
}YEI_Error;

static const char* const YEI_Error_String[] = {
	"YEI_NO_ERROR",                   
	"YEI_ERROR_COMMAND_FAIL",         
	"YEI_INVALID_COMMAND",            
	"YEI_INVALID_ID",                 
	"YEI_ERROR_PARAMETER",            
	"YEI_ERROR_TIMEOUT",              
	"YEI_ERROR_WRITE",                
	"YEI_ERROR_READ",                 
	"YEI_ERROR_STREAM_SLOTS_FULL",    
	"YEI_ERROR_STREAM_CONFIG",        
	"YEI_ERROR_MEMORY",               
	"YEI_ERROR_FIRMWARE_INCOMPATIBLE",
	"YEI_ERROR_NODE_FAIL",            
	"YEI_ERROR_NODE_NOT_PRESENT"
};

/********************************************//**
 * \brief Create ts devices by serial number
 *
 * This convenience function will take a list of serial numbers and will write a list of device ids
 * \param serial_list The list of serial devices to find
 * \param device_list The list of devices ids that will be written to, this list must be the same size as the serial_list
 * \param list_length The  length of serial_list, which should be the same length as the device_list
 * \param search_wireless If set to non-zero(true) this will create any dongles it finds automaticly and will search its
    logical table to see if it can wirelessly communicate with any matching serial numbered devices
 * \param search_unknown_devices If set to non-zero(true) this will search all available comports. Com ports that cannot
    be passively identified as ThreeSpace devices will have bytes written to them to get more information. This is for physical comports and 3rd party serial to usb adapters
 * \param mode The desired timestamp mode the 3-Space device is to be configured with.
 * if TSS_NO_DEVICE_ID is returned then sensor not found
 * \return number of found devices, -1 if error
 ***********************************************/

YEI_EXPORT YEI_Device_Id yei_setUpThreeSpaceSensors(	unsigned int * serial_list,
                                    unsigned char * axis_byte_list,
								    char * discovered_list,
								    int list_length,
								    int search_wireless,
								    int search_unknown_devices,
								    YEI_Timestamp_Mode mode);

YEI_EXPORT YEI_Device_Id yei_setUpPrioVRSensors(    unsigned int priovr_serial_number,
                                                    unsigned int * logical_id_list,
                                                    unsigned char * axis_byte_list,
								                    char * discovered_list,
								                    int list_length,
								                    YEI_Timestamp_Mode mode);

YEI_EXPORT YEI_Error yei_getNodeError(YEI_Device_Id device, YEI_Error * error_list, int list_length);

YEI_EXPORT YEI_Error yei_reDiscoverSensors(YEI_Device_Id device, char * discovered_list, int list_length);

YEI_EXPORT YEI_Error yei_tareSensors(YEI_Device_Id device);

YEI_EXPORT YEI_Error yei_startStreaming(YEI_Device_Id device);

YEI_EXPORT YEI_Error yei_stopStreaming(YEI_Device_Id device);

YEI_EXPORT YEI_Error yei_getLastStreamDataAll( YEI_Device_Id device,
                                    char * output_data,
                                    unsigned int output_data_len,
                                    unsigned int * timestamp);

YEI_EXPORT YEI_Error yei_resetSkeletalAPI();


YEI_EXPORT YEI_Error yei_testMath();

#endif //YEI_SKELETAL_API_INCLUDED