#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;

using System.Net;
using System.Net.Sockets;

using VVVV.Nodes.structs;

using System.Text;

//using System.Text;
using System.Text.RegularExpressions;

using System.Threading;

#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "IO",
	Category = "Devices",
    Version = "Wiesemann+Theis Web-IO",
	Help = "Alternative Node for the Wiesemann & Theis 12x Web-IO Digital E/A",
	Tags = "",
    Author = "sebl",
    Credits = " Wiesemann & Theis (http://www.wut.de/e-5763w-13-inde-000.php)")]
	#endregion PluginInfo
	public class IO : IPluginEvaluate/*, IDisposable*/
	{
		#pragma warning disable 649
		#region fields & pins
		[Input("Output")]
		public ISpread<bool> FPinsIn;
		
		[Input("Enabled", IsSingle = true, DefaultBoolean = false)]
		public IDiffSpread<bool> FEnable;

        [Input("Do Send", IsSingle = true, IsBang = true)]
        public ISpread<bool> FDoSend;

		[Input("Get Pin States", IsSingle = true, IsBang = true)]
		public ISpread<bool> FGetState;
		
		[Input("Remote Host", IsSingle = true, StringType = StringType.IP, DefaultString = "192.168.1.1")]
		public IDiffSpread<string> FIp;
		
		[Input("Remote Port", IsSingle = true, DefaultValue = 49153)]
		public IDiffSpread<int> FPort;
		
		[Output("Actual Output")]
		public ISpread<bool> FOutput;
		
		[Output("Input")]
		public ISpread<bool> FInputs;
		
		[Output("Status")]
		public ISpread<string> FStatus;
		
		[Output("Connected", DefaultBoolean = false)]
		public ISpread<bool> FConnected;

        [Output("CallbackCounter")]
        public ISpread<int> FCallbackCounter;
		
		[Import()]
		public ILogger FLogger;
		
		
		//-------------------------------------

        private  Socket TCP_Client;

        private byte[] receiveBuffer = new byte[512];
        private byte[] lastReceive = new byte[512];

        private bool[] lastOutputPinState = new bool[12] { false, false, false, false, false, false, false, false, false, false, false, false };
        private bool[] lastInputPinState = new bool[12] { false, false, false, false, false, false, false, false, false, false, false, false };

        //private bool disposed = false;
        private bool firstConnect = true;

        private bool reconnect = false;

        private int callbackcounter = 0;

		#endregion fields & pins
		#pragma warning restore

        #region Evaluate
        public void Evaluate(int SpreadMax)
		{

            FOutput.SliceCount = 12;
            FInputs.SliceCount = 12;

            FCallbackCounter.SliceCount = 1;


            // reconnect
            if (reconnect)
            {
                 if (FEnable[0]) 
			     {
                     reconnect = false; // avoid multiple connects
                     closeConnection();
                     //StopClient();
                     connect();
                 }
                 else if (!FEnable[0])
                 {
                     reconnect = false;
                     closeConnection();
                     //StopClient();
                 }
            }

			// Enabled changed
            if (FEnable.IsChanged || FIp.IsChanged || FPort.IsChanged)
            {
                FConnected[0] = false;

				if(FEnable[0])
				{
                    if (FConnected[0])
                    {
                        closeConnection();
                        //StopClient();
                        firstConnect = true;
                    }

					connect();
				}
				else
				{
                    if (this.TCP_Client != null)
					    closeConnection();
                    //StopClient();
					firstConnect = true;
				}
			}

			// on first connect
			if (firstConnect && FEnable[0] && FConnected[0])
			{
				getPins();
				//FOutput.AssignFrom(lastOutputPinState);
				//FInputs.AssignFrom(lastInputPinState);
				firstConnect = false;
			}
			
			// getpinState manually
			if (FGetState[0] && FEnable[0] && FConnected[0])
			{
                try
                {
                    getPins();
                    //FOutput.AssignFrom(lastOutputPinState);
                    //FInputs.AssignFrom(lastInputPinState);
                }
				catch(Exception e)
                {
                    FLogger.Log(LogType.Debug, "exception during Pin-request " + e);
                    FStatus[0] = "Connection error. See [Renderer (TTY)]";
                    FLogger.Log(LogType.Debug, "reconnect ...");

                    lastOutputPinState = new bool[12] { false, false, false, false, false, false, false, false, false, false, false, false };
                    lastInputPinState = new bool[12] { false, false, false, false, false, false, false, false, false, false, false, false };
                    FConnected[0] = false;
                    firstConnect = false;
                    reconnect = true;
                }
				
			}
			
            // set Output Pins
            if (FDoSend[0] && FConnected[0])
			{
                //if (IsConnected( this.TCP_Client))
                //{
                    for (int i = 0; i < 12; i++)
                    {
                        //if (FPinsIn[i] != lastOutputPinState[i]) // dangerous if another computer has altered the pins in the meantime. rare case anyway
                        //{
                            if (FPinsIn[i])
                            {
                                setPins(i, (UInt16)1);
                            }
                            else
                            {
                                setPins(i, (UInt16)0);
                            }
                        //}
                        lastOutputPinState[i] = FPinsIn[i];
                    }
                    getPins();
                //}
			}

            FOutput.AssignFrom(lastOutputPinState);
            FInputs.AssignFrom(lastInputPinState);

            FCallbackCounter[0] = callbackcounter;
		}

        

        #endregion Evaluate


        //--------------------------------------------------------------------------------------------
		// METHODS
		//--------------------------------------------------------------------------------------------
		
		
		private void setPins(int outputPin, UInt16 state)
		{
			int CurOutputNo = outputPin;
			
			structs.setBit cmd = new structs.setBit();
			cmd.Start_1 = 0;
			cmd.Start_2 = 0;
			cmd.StructType = 0x9;
			cmd.StructLength = 12;
			cmd.Mask =  (UInt16)Math.Pow(2, CurOutputNo);
			
			if (state == 1)
			{
				cmd.Value = (UInt16)Math.Pow(2, CurOutputNo);
			}
			else
			{
				cmd.Value = 0;
			}
			
			byte[] sendCmd = ByteConvert.ToBytes(cmd, typeof(structs.setBit));
            try
            {
                this.TCP_Client.Send(sendCmd, sendCmd.Length, SocketFlags.None);
            }
            catch (Exception e)
            {
                FLogger.Log(LogType.Debug, "exception during SetPins " + e);
                FStatus[0] = "Connection error. See [Renderer (TTY)]";

                FLogger.Log(LogType.Debug, "reconnect ...");

                lastOutputPinState = new bool[12] { false, false, false, false, false, false, false, false, false, false, false, false };
                lastInputPinState = new bool[12] { false, false, false, false, false, false, false, false, false, false, false, false };
                FConnected[0] = false;
                firstConnect = false;
                reconnect = true;
            }
		}
		
		
		private void getPins()
		{
			structs.RegisterRequest readCmd = new structs.RegisterRequest();
			readCmd.Start_1 = 0;
			readCmd.Start_2 = 0;
			readCmd.StructType = 0x21;
			readCmd.StructLength = 8;
			
			byte[] sendCmd = ByteConvert.ToBytes(readCmd, typeof(structs.RegisterRequest));

            if (FConnected[0] &&  this.TCP_Client.Connected)
            {
                 this.TCP_Client.Send(sendCmd, sendCmd.Length, SocketFlags.None);
                 this.TCP_Client.BeginReceive(receiveBuffer, 0, 512, SocketFlags.None, new AsyncCallback(callback_receive),  this.TCP_Client);
            }
		}
		
		
		private void connect()
		{
            if (IsValidIP(FIp[0]) && IsValidPort(FPort[0]) )
			{
                if (callbackcounter == 0)
                {
                    callbackcounter++;
				    try
				    {
					    IPEndPoint ClientEP = new IPEndPoint(IPAddress.Parse(FIp[0]), FPort[0]);                    
                         this.TCP_Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                         this.TCP_Client.Blocking = false;
                        //int ttl =  this.TCP_Client.Ttl;

                         this.TCP_Client.BeginConnect(ClientEP, new AsyncCallback(callback_connect),  this.TCP_Client);
                        
				    }
				    catch (Exception e)
				    {
					    FLogger.Log(LogType.Debug, "exception in connect(): " + e);
                        reconnect = true;
				    }
		   	    }
                else
                {
                    // still a callback in progress..
                    reconnect = true;
                }
            }
			else
			{
				FStatus[0] = "IP and Port needed or in wrong format";
			}
			
		}
		
		
		private void callback_connect(IAsyncResult ar)
		{
			try
			{
                callbackcounter--;
                Socket socket = (Socket)ar.AsyncState;

                if (socket.Connected)
                {
                    socket.EndConnect(ar);
                    FStatus[0] = "connected";
                    FConnected[0] = true;
                }
                else
                {
                    FStatus[0] = "connection could not be established";
                    FConnected[0] = false;
                    reconnect = true;
                }

			}
			catch (Exception e)
			{
                FLogger.Log(LogType.Debug, "exception in callback_connect " + e);
                FStatus[0] = "Connection error. See [Renderer (TTY)]" ;

                FLogger.Log(LogType.Debug, "reconnect in 2 seconds...");

                lastOutputPinState = new bool[12] { false, false, false, false, false, false, false, false, false, false, false, false };
                lastInputPinState = new bool[12] { false, false, false, false, false, false, false, false, false, false, false, false };
                FConnected[0] = false;
                firstConnect = false;
                Thread.Sleep(2000);

                reconnect = true;
			}		
		}
		
		
		private void callback_receive(IAsyncResult ar)
		{
			int receiveCount = 0;
			
			try
			{
                if ( this.TCP_Client != null)
				{
                    if ( this.TCP_Client.Connected)
					{
                        receiveCount =  this.TCP_Client.EndReceive(ar);
						Array.Copy(receiveBuffer, lastReceive, 511);
						Array.Clear(receiveBuffer, 0, 512);
						
						evaluateResponse();
					}
				}
			}
			catch (Exception e)
			{
				FLogger.Log(LogType.Debug, "Error while receiving!" + e);
                FStatus[0] = "Connection error. See [Renderer (TTY)]";

                FLogger.Log(LogType.Debug, "reconnect in 2 seconds...");

                lastOutputPinState = new bool[12] { false, false, false, false, false, false, false, false, false, false, false, false };
                lastInputPinState = new bool[12] { false, false, false, false, false, false, false, false, false, false, false, false };
                FConnected[0] = false;
                firstConnect = false;
                Thread.Sleep(2000);
                
                reconnect = true;
			}
		}

        /*
        public void StopClient()
        {
            // this.TCP_Client.canRun = false;
            FConnected[0] = false;
            FStatus[0] = "disconnected";

             this.TCP_Client.Shutdown(SocketShutdown.Both);
             this.TCP_Client.BeginDisconnect(true, new AsyncCallback(DisconnectCallback),  this.TCP_Client);
        }


        private void DisconnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete the disconnection.
                client.EndDisconnect(ar);

                 this.TCP_Client.Close();
                 this.TCP_Client = null;
            }
            catch (Exception e)
            {
                FLogger.Log(LogType.Debug, "exception in callback_connect " + e);
                FStatus[0] = "Connection error. See [Renderer (TTY)]";
            }
        }
        */

        /*
        public bool IsConnected(Socket sock)
        {
            try
            {
                ASCIIEncoding encoder = new ASCIIEncoding();
                byte[] buffer = encoder.GetBytes("test");
                sock.Send(buffer, 0, buffer.Length, SocketFlags.None);
            }
            catch (Exception e)
            {
                //throw new LostConnection();
                FLogger.Log(LogType.Debug, "exception in callback_connect " + e);
                FStatus[0] = "Connection error. See [Renderer (TTY)]";
            }

            return sock.Connected;
        }
        */

		private void evaluateResponse()
		{
			
			# region test response
			/*
			switch(lastReceive[4])
			{
				case 8:
				FLogger.Log(LogType.Debug, "case 8");
				break;
				
				case 0x31:
				FLogger.Log(LogType.Debug, "case 0x31");
				break;
				
				case 0xB4:
				FLogger.Log(LogType.Debug, "case 0xB4");
				break;
				
				case 0xB5:
				FLogger.Log(LogType.Debug, "case 0xB5");
				break;
				
			}
			*/
			#endregion
			
			try
			{
				structs.registerState RegisterState = (structs.registerState)ByteConvert.ToStruct(lastReceive, typeof(structs.registerState));
				
				for(int port = 0; port<12; port++)
				{
					bool outputPinstatus = (RegisterState.OutputValue & (UInt16)Math.Pow(2, port)) == Math.Pow(2, port);
					//FLogger.Log(LogType.Debug, "Output Pinstatus " + port + " is: " + outputPinstatus);
					
					if (outputPinstatus)
					{
						lastOutputPinState[port] = true;
					}
					else
					{
						lastOutputPinState[port] = false;
					}
					
					
					bool inputPinstatus = (RegisterState.InputValue & (UInt16)Math.Pow(2, port)) == Math.Pow(2, port);
					//FLogger.Log(LogType.Debug, "Input  Pinstatus " + port + " is: " + inputPinstatus);
					
					if (inputPinstatus)
					{
						lastInputPinState[port] = true;
					}
					else
					{
						lastInputPinState[port] = false;
					}
				}

				FOutput.AssignFrom(lastOutputPinState);
				FInputs.AssignFrom(lastInputPinState);

			}
			catch (Exception e)
			{
				FLogger.Log(LogType.Debug, "Error while parsing return data: " + e);
			}
			
		}


		
		
		private void closeConnection()
		{
			try
			{
                if ( this.TCP_Client != null)
				{
                    if ( this.TCP_Client.Connected)
					{
                             this.TCP_Client.Disconnect(false);
                             this.TCP_Client.Shutdown(SocketShutdown.Both);
					}
                    
                     this.TCP_Client.Close();
                    this.TCP_Client = null;
				}

				FConnected[0] = false;
                FStatus[0] = "disconnected";

			}
			catch (Exception e)
			{
				FStatus[0] = "Error while disconnecting: " + e;
			}
		}
        
        //--------------------------------------------------------------------------------------------
        // Helpers
        //--------------------------------------------------------------------------------------------


        private bool IsValidIP(string addr)
        {
            string pattern = "^([01]?\\d\\d?|2[0-4]\\d|25[0-5])\\.([01]?\\d\\d?|2[0-4]\\d|25[0-5])\\.([01]?\\d\\d?|2[0-4]\\d|25[0-5])\\.([01]?\\d\\d?|2[0-4]\\d|25[0-5])$";
            Regex check = new Regex(pattern);

            bool valid = false;

            if (addr == "")
            {
                valid = false;
            }
            else
            {
                valid = check.IsMatch(addr, 0);
            }
            return valid;
        }

        private bool IsValidPort(int port)
        {
            if (port > 0 && port < 65553)
                return true;
            else
                return false;
        }


		
		//--------------------------------------------------------------------------------------------
		// DISPOSE
		//--------------------------------------------------------------------------------------------
			/*
		public void Dispose()
		{
            GC.SuppressFinalize(this);
            GC.SuppressFinalize( this.TCP_Client);
			Dispose(true);
            GC.Collect();			
		}
		
		
		protected virtual void Dispose(bool disposing)
		{
			// Check to see if Dispose has already been called.
			if(!this.disposed)
			{
                if(callbackcounter == 0)
                { 
                    if (disposing)
                    {
                        if ( this.TCP_Client != null)
                        {
                            closeConnection(false);
                            //StopClient();
                        }
                    }
				    // Note disposing has been done.
				    disposed = true;
                }
                else
                {
                     this.TCP_Client.Dispose();
                    Dispose(true);
                }
			}
		}*/



		
	}
}
