#region licence/info

//////project name
//Phidget Interface 888

//////description
//VVVV Plug In for the Phidget Interfaces.  http://www.phidgets.com/products.php?category=1
//you can connect an Phidget Interface to vvv an controll the digital In and Out's.

//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

//////language/ide
//C# sharpdevelop 

//////dependencies
//VVVV.PluginInterfaces.V1;
//VVVV.Utils.VColor;
//VVVV.Utils.VMath;
//the phidgets drivers which you can find on  http://www.phidgets.com/downloads_sections.php

//////initial author
//phlegma 

#endregion licence/info

//use what you need
using System;
using System.Drawing;
using VVVV.PluginInterfaces.V1;
using Phidgets;
using Phidgets.Events;

//the vvvv node namespace
namespace VVVV.Nodes
{
	//class definition
	public class PhidgetInterface: IPlugin, IDisposable
    {	          	
    	#region field declaration
    	
    	//the host (mandatory)
    	private IPluginHost FHost; 
    	// Track whether Dispose has been called.
   		private bool FDisposed = false;

    	//input pin declaration
    	private IValueIn FEnable;
        private IValueIn FSensitivity;
        private IValueIn FSerial;
        private IValueIn FDigitalOut;
        private IValueIn FRatiomatric;

    	
    	//output pin declaration
    	private IValueOut FAnalogIn;
        private IValueOut FDigitalIn;
        private IValueOut FConnected;
        private IStringOut FInfo;
        private IValueOut FDigiOutCount;

        //GetInterfaceData
        private GetInterfaceData m_IKitData;
        private Manager Anzahl = new Manager();
    	
    	#endregion field declaration
       
    	#region constructor/destructor
    	
        public PhidgetInterface()
        {
			//the nodes constructor
			//nothing to declare for this node
            
		}
        
        // Implementing IDisposable's Dispose method.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
        	Dispose(true);
        	// Take yourself off the Finalization queue
        	// to prevent finalization code for this object
        	// from executing a second time.
        	GC.SuppressFinalize(this);
        }
        
        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        protected virtual void Dispose(bool disposing)
        {
        	// Check to see if Dispose has already been called.
        	if(!FDisposed)
        	{
        		if(disposing)
        		{
        			// Dispose managed resources.
        		}
        		// Release unmanaged resources. If disposing is false,
        		// only the following code is executed.
	        	
        		FHost.Log(TLogType.Debug, "PluginTemplate is being deleted");
                m_IKitData.Dispose();
                m_IKitData = null;
         		
        		// Note that this is not thread safe.
        		// Another thread could start disposing the object
        		// after the managed resources are disposed,
        		// but before the disposed flag is set to true.
        		// If thread safety is necessary, it must be
        		// implemented by the client.
        	}
        	FDisposed = true;
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~PhidgetInterface()
        {
        	// Do not re-create Dispose clean-up code here.
        	// Calling Dispose(false) is optimal in terms of
        	// readability and maintainability.
        	Dispose(false);
        }
        #endregion constructor/destructor
        
        #region node name and infos
       
        //provide node infos 
        public static IPluginInfo PluginInfo
	    {
	        get 
	        {
	        	//fill out nodes info
	        	//see: http://www.vvvv.org/tiki-index.php?page=vvvv+naming+conventions
	        	IPluginInfo Info = new PluginInfo();
	        	Info.Name = "IO";							//use CamelCaps and no spaces
	        	Info.Category = "Phidgets";						//try to use an existing one
	        	Info.Version = "InterfaceKit";						//versions are optional. leave blank if not needed
	        	Info.Help = "";
	        	Info.Bugs = "";
                Info.Credits = "http://www.phidgets.com/";								//give credits to thirdparty code used
	        	Info.Warnings = "";
	        	
	        	//leave below as is
	        	System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
   				System.Diagnostics.StackFrame sf = st.GetFrame(0);
   				System.Reflection.MethodBase method = sf.GetMethod();
   				Info.Namespace = method.DeclaringType.Namespace;
   				Info.Class = method.DeclaringType.Name;
   				return Info;
   				//leave above as is
	        }
		}

        public bool AutoEvaluate
        {
        	//return true if this node needs to calculate every frame even if nobody asks for its output
        	get {return true;}
        }
        
        #endregion node name and infos
        
      	#region pin creation
        
        //this method is called by vvvv when the node is created
        public void SetPluginHost(IPluginHost Host)
	    {
        	//assign host
	    	FHost = Host;

	    	//create inputs
	    	FHost.CreateValueInput("Enable", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FEnable);
            FEnable.SetSubType(0, 1, 1, 0, false, true, true);

            FHost.CreateValueInput("Digital Out", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FDigitalOut);
            FDigitalOut.SetSubType(0, 1, 1, 0, false, true, true);

            FHost.CreateValueInput("Sensitivity", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FSensitivity);
            FSensitivity.SetSubType(0, 1, 0.01, 0.1, false, false, false);

            FHost.CreateValueInput("Ratiometric", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FRatiomatric);
            FRatiomatric.SetSubType(0, 1, 1, 1, false, true, true);

            FHost.CreateValueInput("Serial", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FSerial);
            FSerial.SetSubType(0, double.MaxValue, 1, 0, false, false, true);

            //create outputs	    	
	    	FHost.CreateValueOutput("Analog In", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FAnalogIn);
            FAnalogIn.SetSubType(double.MinValue, double.MaxValue, 0.0001, 0, false, false, false);

            FHost.CreateValueOutput("Digital In", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FDigitalIn);
            FDigitalIn.SetSubType(0, 1, 1, 0, false, true, true);

            FHost.CreateValueOutput("Number of Digital Outputs", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FDigiOutCount);
            FDigiOutCount.SetSubType(0, 1, 1, 0, false, true, true);
            
            FHost.CreateValueOutput("Connected", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FConnected);
            FConnected.SetSubType(0, 1, 1, 0, false, true, true);
	    	
            FHost.CreateStringOutput("Info", TSliceMode.Dynamic, TPinVisibility.True, out FInfo);
            FInfo.SetSubType("Disabled", false);

        }

        #endregion pin creation
        
        #region mainloop
        
        public void Configurate(IPluginConfig Input)
        {
        	//nothing to configure in this plugin
        	//only used in conjunction with inputs of type cmpdConfigurate
        }
        
        //here we go, thats the method called by vvvv each frame
        //all data handling should be in here
        public void Evaluate(int SpreadMax)
        {
            
            double Enable = 0;
            double Serial;
            double Ratiomatric;

            FConnected.SliceCount = 1;
            
            FSerial.GetValue(0, out Serial);
            FEnable.GetValue(0, out Enable);
            FRatiomatric.GetValue(0, out Ratiomatric);

            if (FSerial.PinIsChanged || FEnable.PinIsChanged )
            {

                if (FSerial.PinIsChanged)
                {
                    if (m_IKitData != null)
                    {
                        m_IKitData.Close();
                        m_IKitData = null;
                    }
                }

                if (Enable == 1)
                {
                    if (m_IKitData == null)
                    {
                        m_IKitData = new GetInterfaceData();
                        m_IKitData.Open(Serial);
                    }
                }
                else
                {
                    if (m_IKitData != null)
                    {
                        FInfo.SliceCount = 1;
                        FInfo.SetString(0, "Disabled");
                        m_IKitData.Close();
                        m_IKitData = null;
                    }
                }
            }

            FConnected.SetValue(0, m_IKitData.Status);
            m_IKitData.SetRatiometric(Ratiomatric);

            if (Enable == 1)
            {
                //
                int SliceCountAnalogIn = m_IKitData.InfoDevice.ToArray()[0].AnalogOutputs;
                int SliceCountDigitalIn = m_IKitData.InfoDevice.ToArray()[0].DigitalInputs;
                int SliceCountDigitalOut = m_IKitData.InfoDevice.ToArray()[0].DigitalOutputs;

                FAnalogIn.SliceCount = SliceCountAnalogIn;
                FDigitalIn.SliceCount = SliceCountDigitalIn;
                FDigiOutCount.SliceCount = 1;
                FDigiOutCount.SetValue(1, SliceCountDigitalOut);
                
                //
                for (int i = 0; i < SliceCountAnalogIn; i++)
                {
                    FAnalogIn.SetValue(i, m_IKitData.AnalogInputs[i]);
                }


                //
                for (int i = 0; i < SliceCountDigitalIn; i++)
                {
                    FDigitalIn.SetValue(i, m_IKitData.DigitalInputs[i]);
                }


                //
                double[] digiOut = new double[m_IKitData.InfoDevice.ToArray()[0].DigitalInputs];

                for (int i = 0; i < SliceCountDigitalOut; i++)
                {
                    FDigitalOut.GetValue(i, out digiOut[i]);
                }
                m_IKitData.SetDigitalOutput(digiOut);

                //
                if (FSensitivity.PinIsChanged)
                {
                    double SliceCountSense = FSensitivity.SliceCount;
                    double[] SenseValue = new double[SliceCountAnalogIn];
                    for (int i = 0; i < SliceCountAnalogIn; i++)
                    {
                        double sense;
                        FSensitivity.GetValue(i, out sense);
                        SenseValue[i] = sense;

                    }
                    m_IKitData.SetSense(SenseValue);
                }

                //
                int SpreadSizeInfo = 3;
                for (int i = 0; i < SpreadSizeInfo; i++)
                {
                    FInfo.SliceCount = 3;
                    switch (i)
                    {
                        case 0:
                            FInfo.SetString(i, "Name: " + m_IKitData.InfoDevice.ToArray()[0].Name);
                            break;
                        case 1:
                            FInfo.SetString(i, "Serial: " + m_IKitData.InfoDevice.ToArray()[0].SerialNumber.ToString());
                            break;
                        case 2:
                            FInfo.SetString(i, "Version: " + m_IKitData.InfoDevice.ToArray()[0].Version.ToString());
                            break;
                    }
                } 	

            }
            
        }
             
        #endregion mainloop  
	}
}
