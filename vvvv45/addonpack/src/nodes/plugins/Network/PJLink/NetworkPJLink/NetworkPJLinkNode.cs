#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;

using System.Text.RegularExpressions;
using System.Threading;
using System.Collections.Generic;

using PJLink;

#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "PJLink",
	Category = "Network",
	Help = "Control a Projector through the PJLink protocol",
	Tags = "projector, remote, control",
	Author = "sebl",
    Credits = "RV realtime visions GmbH")]
	#endregion PluginInfo
	
	public class NetworkPJLinkNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("IP", DefaultString = "192.168.100.1")]
		IDiffSpread<string> FIp;
		
		[Input("Password", DefaultString = "")]
		IDiffSpread<string> FPassword;
		
		[Input("Port", DefaultValue = 4352)] // = default port
		IDiffSpread<int> FPort;
		
		[Input("PowerOn", IsBang=true)]
		IDiffSpread<bool> FPowerOn;
		
		[Input("PowerOff", IsBang=true)]
		IDiffSpread<bool> FPowerOff;
		
		[Input("Query", IsBang=true)]
		IDiffSpread<bool> FQuery;
		
		[Output("Projector Info")]
		ISpread<string> FProjectorInfo;
		
		[Output("Status")]
		ISpread<string> FStatus;
				
		[Output("Power Status")]
		ISpread<string> FPowerState;
		
		[Output("Input Select")]
		ISpread<string> FInputSelect;
		
		[Output("Lamp Status")]
		ISpread<string> FLampState;
		
		[Output("Fan Status")]
		ISpread<string> FFanState;
		
		[Output("Filter Status")]
		ISpread<string> FFilterState;
		
		[Output("Cover Status")]
		ISpread<string> FCoverState;
		
		[Output("Num Of Lamps")]
		ISpread<string> FNumOfLamps;
		
		[Output("Multi Lamp Hours")]
		ISpread<string> FMultiLampHours;
		
		[Output("Multi Lamp Status")]
		ISpread<ISpread<string>> FMultiLampStatus;
		
		[Output("Host Name")]
		ISpread<string> FProjectorHostName;
		
		[Output("Manufacturer Name")]
		ISpread<string> FProjectorManufacturerName;
		
		[Output("Name")]
		ISpread<string> FProjectorName;
		
		[Output("Product Name")]
		ISpread<string> FProjectorProductName;
		
		[Output("Is Connected")]
		ISpread<bool> FConnected;
		
		[Import()]
		public ILogger FLogger;
		
		
		List<PJLinkConnection> projectors = new List<PJLinkConnection>();
		List<ProjectorInfo> projectorInfo = new List<ProjectorInfo>();
		
		#endregion fields & pins
		
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FStatus.SliceCount = SpreadMax;
			FProjectorInfo.SliceCount = SpreadMax;
			FConnected.SliceCount = SpreadMax;
			FPowerState.SliceCount = SpreadMax;
			FInputSelect.SliceCount = SpreadMax;
			FLampState.SliceCount = SpreadMax;
			FFanState.SliceCount = SpreadMax;
			FFilterState.SliceCount = SpreadMax;
			FCoverState.SliceCount = SpreadMax;
			FNumOfLamps.SliceCount = SpreadMax;
			FMultiLampHours.SliceCount = SpreadMax;
			
			FMultiLampStatus.SliceCount = SpreadMax;
			
			FProjectorHostName.SliceCount = SpreadMax;
			FProjectorManufacturerName.SliceCount = SpreadMax;
			FProjectorName.SliceCount = SpreadMax;
			FProjectorProductName.SliceCount = SpreadMax;
			
			if (FIp.IsChanged || FPassword.IsChanged ||	FPort.IsChanged)
			{
				
				FLogger.Log(LogType.Debug, "create new arrays...");
				
				if (projectors.Count != 0)
				{
					projectors.Clear();
					projectorInfo.Clear();
				}
				FLogger.Log(LogType.Debug, "ok");
				
				
				for( int i = 0; i < SpreadMax; i++)
				{
					FLogger.Log(LogType.Debug, "---> projector " + i);
					
					bool isValid = IsValidIP(FIp[i]);
					
					if (isValid == true)
					{
						FLogger.Log(LogType.Debug, "establishing connection");
						
						if(FPassword[i].Length > 0)
						{
							projectors.Add(new PJLinkConnection(FIp[i], FPort[i], FPassword[i]));
						}else
						{
							projectors.Add(new PJLinkConnection(FIp[i], FPort[i]) );
						}
						
						
						
						PowerCommand powerQuery = new PowerCommand(PowerCommand.Power.QUERY);
						
						// THIS IS SLOW !! >> need to do it async in its own thread
						if (projectors[i].sendCommand(powerQuery) == Command.Response.SUCCESS)
						{
							FStatus[i] = "established connection";
							FConnected[i] = true;
						}else
						{
							FStatus[i] = "error during connection";
							FConnected[i] = false;
						}
						
						// the "faster" method
						// still need to enhance the async re3sponce thingy to identify the callbacks
						
						//FLogger.Log(LogType.Debug, "get powerQuery...");
						projectors[i].sendCommandAsync(powerQuery, QueryResponse);
						//FLogger.Log(LogType.Debug, "ok");
						
					}else
					{
						FStatus[i] = "invalid IP adress";
					}
					
				}
			}
			
			if (FQuery.IsChanged)
			{
				for( int i = 0; i < SpreadMax; i++)
				{
					if (FConnected[i])
					{
						
						if (FQuery[i])
						{
							FLogger.Log(LogType.Debug, "get projectorInfo...");
							projectorInfo.Add(ProjectorInfo.create(projectors[i]) );
							
							try
							{
								// XML serialization not working
								//FProjectorInfo[i] = projectorInfo[i].toXmlString();
								FProjectorInfo[i] = projectorInfo[i].ToString();
							}
							catch (Exception e)
							{
								FLogger.Log(LogType.Debug, "error reading ProjectorInfo: " + e);
							}
							
							FPowerState[i] 	= projectorInfo[i].PowerStatus.ToString();
							FInputSelect[i] = projectorInfo[i].Input.ToString();
							FLampState[i] 	= projectorInfo[i].LampStatus.ToString();
							FFanState[i] 	= projectorInfo[i].FanStatus.ToString();
							FFilterState[i] = projectorInfo[i].FilterStatus.ToString();
							FNumOfLamps[i] 	= projectorInfo[i].NumOfLamps.ToString();
							FMultiLampHours[i] = projectorInfo[i].NumOfLamps.ToString();
							
							int LampCount = projectorInfo[i].MultiLampStatus.Count;
							for (int j = 0; j < LampCount; j++)
							{
								FLogger.Log(LogType.Debug, "multilampCount " + LampCount + " projector: " + i);
								FMultiLampStatus[i].SliceCount = LampCount;
								FMultiLampStatus[i][j] = projectorInfo[i].MultiLampStatus[j].ToString();
							}
							
							FCoverState[i] = projectorInfo[i].CoverStatus.ToString();
							FProjectorHostName[i] = projectorInfo[i].ProjectorHostName.ToString();
							FProjectorManufacturerName[i] = projectorInfo[i].ProjectorManufacturerName.ToString();
							FProjectorName[i] = projectorInfo[i].ProjectorName.ToString();
							FProjectorProductName[i] = projectorInfo[i].ProjectorProductName.ToString();
							
							FLogger.Log(LogType.Debug, "ok");
							
							#region available Infos
							// available Infos:
							//projectorInfo[i].CoverStatus
							//projectorInfo[i].FanStatus
							//projectorInfo[i].FilterStatus
							//projectorInfo[i].Input
							//projectorInfo[i].InputPort
							//projectorInfo[i].LampStatus
							//projectorInfo[i].MultiLampHours
							//projectorInfo[i].MultiLampStatus
							//projectorInfo[i].NumOfLamps
							//projectorInfo[i].PowerStatus
							//projectorInfo[i].ProjectorHostName
							//projectorInfo[i].ProjectorManufacturerName
							//projectorInfo[i].ProjectorName
							//projectorInfo[i].ProjectorProductName
							# endregion available Infos
						}
					}
				}
			}
			
			if (FPowerOn.IsChanged || FPowerOff.IsChanged)
			{
				for( int i = 0; i < SpreadMax; i++)
				{
					if (FConnected[i])
					{
						if(FPowerOn[i])
						{
							PowerCommand pcOn = new PowerCommand(PowerCommand.Power.ON);
							if (projectors[i].sendCommand(pcOn) == Command.Response.SUCCESS)
							FStatus[i] = "Switching on successful";
							else
							FStatus[i] = "Communication Error";
						}
						if(FPowerOff[i])
						{
							FLogger.Log(LogType.Debug, "poweroff...");
							PowerCommand pcOff = new PowerCommand(PowerCommand.Power.OFF);
							if (projectors[i].sendCommand(pcOff) == Command.Response.SUCCESS)
							FStatus[i] = "Switching off successful";
							else
							FStatus[i] = "Communication Error";
						}
					}
				}
			}
		}
		
		#region methods
		private  void QueryResponse(Command sender, Command.Response response)
		{
			sender.GetType();
			string hm = sender.CmdResponse.ToString();
			FLogger.Log(LogType.Debug, "sender: " + hm + " response: " + response);
		}
		
		
		public bool IsValidIP(string addr)
		{
			string pattern = "^([01]?\\d\\d?|2[0-4]\\d|25[0-5])\\.([01]?\\d\\d?|2[0-4]\\d|25[0-5])\\.([01]?\\d\\d?|2[0-4]\\d|25[0-5])\\.([01]?\\d\\d?|2[0-4]\\d|25[0-5])$";
			Regex check = new Regex(pattern);
			
			bool valid = false;
			
			if (addr == "")
			{
				valid = false;
				FLogger.Log(LogType.Debug, "no address provided");
			}
			else
			{
				valid = check.IsMatch(addr, 0);
			}
			return valid;
		}
		#endregion methods

	}
}
