using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrioVR.SkeletalLib.RawApi
{

    /**
    * \brief An enum expressing the different types of errors the Skeletal API could return
    * 
    */

    public enum YEIErrorEnum //AKA YEI_Error
    {
        /// <summary>
        /// The API call successfuly executed.
        /// </summary>
        NoError = 0,

        /// <summary>
        /// The command returned a failed response
        /// </summary>
        ErrorCommandFail,

        /// <summary>
        /// The API call was made on a device 
        /// type that does not suppport the attemped command.
        /// </summary>
        InvalidCommand,

        /// <summary>
        /// The TSS_ID parameter passed 
        /// in to an API call is not associated 
        /// with a connected 3-Space device.
        /// </summary>

        InvalidId,

        /// <summary>
        /// General parameter fail
        /// </summary>
        ErrorParameter,
        
        /// <summary>
        /// The command's timeout has been reached
        /// </summary>
        ErrorTimeout,

        /// <summary>
        /// The API call executed failed to write 
        /// all the data necessary to execute the 
        /// command to the intended 3-Space device
        /// </summary>
        ErrorWriting,
        
        /// <summary>
        /// The API call executed failed to read 
        /// all the data necessary to execute the 
        /// to the intended 3-Space device
        /// </summary>
        ErrorReading,
        
        /// <summary>
        /// The 3-Space device's stream slots are full
        /// </summary>
        ErrorStreamSlotsFull,
        
        /// <summary>
        /// The 3-Space device's stream configuration 
        /// is corrupted
        ErrorStreamConfig,
        
        /// <summary>
        /// A memory error occurred in the API
        /// </summary>
        ErrorMemory,
        
        /// <summary>
        /// 3-Space device firmware does not support 
        /// that command, firmware update highly recommende
        /// </summary>
        ErrorFirmwareIncompatible,

        /// <summary>
        /// A node in the Skeleton reported error, 
        /// call yei_getNodeError to see whitch 
        /// node and its error
        /// </summary>
        /// 
        ErrorNodeFail,
        
        /// <summary>
        /// 
        /// </summary>
        ErrorNodeNotPresent,

    }
}
