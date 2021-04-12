using System;
using System.IO;
using System.Net; 
using System.Net.Sockets; 
using System.Text; 
using System.Threading; 
using UnityEngine;
using Valve.Newtonsoft.Json;
using Modbus.Data;
using Modbus.Device;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;
using static MCUCommand;

public class SimServer : MonoBehaviour {
	/// <summary> 	
	/// TCPListener to listen for incomming TCP connection 	
	/// requests. 	
	/// </summary> 	
	private TcpListener tcpListener; 
	/// <summary> 
	/// Background thread for TcpServer workload. 	
	/// </summary> 	
	private Thread tcpListenerThread;  	
	/// <summary> 	
	/// Create handle to connected tcp client. 	
	/// </summary> 	
	private TcpClient connectedTcpClient;
	private TcpClient PLCTCPClient;
	private TcpListener MCU_TCPListener;
	private ModbusSlave MCU_Modbusserver;
	public ModbusIpMaster PLCModbusMaster;
	private Thread MCU_emulator_thread;
	private Thread PLC_emulator_thread;
	
	//for controlling the VR telescope
	public TelescopeControllerSim tc;
	public float speed = 0.01f;
	private float azDeg = -42069;
	private float elDeg = -42069;
	
	//UI Related variables
	public string mcuIP;
	public TMP_InputField plcIP;
	public TMP_InputField mcuIP;
	public TMP_InputField plcPort;
	public TMP_InputField mcuPort;
	public Button startButton;
	public Button fillButton;
	
	private bool runsimulator = true, moving = false, jogging = false, isconfigured = false, isTest = false;
	private int acc, distAZ, distEL, currentAZ, currentEL, AZ_speed, EL_speed, PLC_port, MCU_port;
	
	//START BUTTON
	void Start()
	{
		tc.speed = speed;
		tc.TargetAzimuth(0.0f);
		tc.TargetElevation(15.0f);
		//startButton = GetComponent<Button>();
		startButton.onClick.AddListener(StartServer);
		fillButton.onClick.AddListener(AutoFillInput);
		
		//fix the fullscreen stuff
		Screen.fullScreen = false;
		Screen.SetResolution(1024, 768, FullScreenMode.Windowed);
	}
	
	public void AutoFillInput()
	{
		plcIP.text = "127.0.0.1";
		mcuIP.text = "127.0.0.1";
		plcPort.text = "8082";
		mcuPort.text = "8083";
	}
	
	//START THE SERVER THREADS
	public void StartServer()
	{
		Debug.Log("Start Button clicked");
		tc.speed = speed;
		tc.TargetAzimuth(0.0f);
		tc.TargetElevation(15.0f);
		
		try
		{
			//MCU_TCPListener = new TcpListener(new IPEndPoint(IPAddress.Parse("127.0.0.2"), 8080));
			//Debug.Log(MCU_ip,)
			MCU_TCPListener = new TcpListener(new IPEndPoint(IPAddress.Parse(mcuIP.text), int.Parse(mcuPort.text)));
			MCU_emulator_thread = new Thread(new ThreadStart(Run_MCU_server_thread));
		}
		catch (Exception e)
		{
			if((e is ArgumentNullException) || (e is ArgumentOutOfRangeException))
			{
				Debug.Log(e);
				return;
			}
			else
			{
				throw e;
			}
		}
		
		try
		{
			MCU_TCPListener.Start(1);
		}
		catch (Exception e)
		{
			if((e is SocketException) || (e is ArgumentOutOfRangeException) || (e is InvalidOperationException))
			{
				Debug.Log(e);
				return;
			}
		}
		runsimulator = true;
		MCU_emulator_thread.Start();
	}
	
	// Update is called once per frame
	void Update () { 		
		//Check for an impossible or incorrect value and display a number so you can tell it errord
		float errorCondition = -42069;
		if(azDeg != errorCondition)
		{
			//Debug.Log("Y Move");
			tc.TargetAzimuth(azDeg);
			azDeg = errorCondition;

		}
		
		if(elDeg != errorCondition)
		{
			//Debug.Log("Z Move");
			tc.TargetElevation(elDeg);
			elDeg = errorCondition;
		}
		
		//press escape to exit the program cleanly
		if(Input.GetKeyDown((KeyCode.Escape)))
			Application.Quit();
	}
	
	private void Run_MCU_server_thread()
	{
		byte slaveId = 1;
		// create and start the TCP slave
		MCU_Modbusserver = ModbusTcpSlave.CreateTcp(slaveId, MCU_TCPListener);
		//coils, inputs, holdingRegisters, inputRegisters
		MCU_Modbusserver.DataStore = DataStoreFactory.CreateDefaultDataStore(0, 0, 1054, 0);
		// PLC_Modbusserver.DataStore.SyncRoot.ToString();
		
		//MCU_Modbusserver.ModbusSlaveRequestReceived += new EventHandler<ModbusSlaveRequestEventArgs>(Server_Read_handler);
		
		MCU_Modbusserver.Listen();
		
		//did something connect?
		
		// prevent the main thread from exiting
		ushort[] last, current;
		last = Copy_modbus_registers(1025, 20);
		while (runsimulator)
		{
			if(isTest)
			{
				Thread.Sleep(5);
				continue;
			}
			Thread.Sleep(50);
			current = Copy_modbus_registers(1025, 20);
			if(!current.SequenceEqual(last))
			{
				MCUCommand currentCommand = buildMCUCommand(current_out);
			}
			if(moving)
			{
				if(distAZ != 0 || distEL != 0)
				{
					int travAZ = (distAZ < -AZ_speed) ? -AZ_speed : (distAZ > AZ_speed) ? AZ_speed : distAZ;
					int travEL = (distEL < -EL_speed) ? -EL_speed : (distEL > EL_speed) ? EL_speed : distEL;
					updateMCURegisters(travAZ, travEL);
				}
				else
				{
					moving = false;
					// TODO: updating this register store again, what do these values mean?
					MCU_Modbusserver.DataStore.HoldingRegisters[1] = (ushort)(MCU_Modbusserver.DataStore.HoldingRegisters[1] | 0x0080);
					MCU_Modbusserver.DataStore.HoldingRegisters[11] = (ushort)(MCU_Modbusserver.DataStore.HoldingRegisters[11] | 0x0080);
				}
			}
			if(jogging)
			{
				updateMCURegisters(AZ_speed, EL_speed);
			}
			last = current;
		}
	}
	
	private MCUCommand buildMCUCommand(ushort[] data)
	{
		isconfigured = true;
		string outstr = "";
		for(int v = 0; v < data.Length; v++) {
			outstr += Convert.ToString( data[v] , 16 ).PadLeft( 5 ) + ",";
		}
		Debug.Log("Spitting out registers: \n");
		Debug.Log(outstr);
		Debug.Log("All done spitting out registers\n");

		jogging = false;

		if(data[0] == 4)
		{
			Debug.Log("Recieved immediate stop.");
		}

		} if(data[0] == 0x0080 || data[0] == 0x0100 || data[10] == 0x0080 || data[10] == 0x0100) 
		{
			Debug.Log("JOG COMMAND INCOMING");
			jogging = true;

		} else if(data[0] == 0x0002 || data[0] == 0x0002) // RELATIVE MOVE
		{
			Debug.Log("RELATIVE MOVE INCOMING");
			moving = true;
		}

		// build mcu command based on register data
		MCUCommand mcuCommand = new MCUCommand(data);

		// set register store as in motion 
		// TODO: What do these values mean? Do we need to set them differently based on other cases?
		MCU_Modbusserver.DataStore.HoldingRegisters[1] = (ushort)(MCU_Modbusserver.DataStore.HoldingRegisters[1] & 0xff7f);
		MCU_Modbusserver.DataStore.HoldingRegisters[11] = (ushort)(MCU_Modbusserver.DataStore.HoldingRegisters[11] & 0xff7f);

		return mcuCommand;
	}
	
	private bool updateMCURegisters(int travAZ, int travEL)
	{
		distAZ -= travAZ;
		distEL -= travEL;
		currentAZ += travAZ;
		currentEL += travEL;
		//   Debug.Log("offset: az" + currentAZ + " el " + currentEL);
		MCU_Modbusserver.DataStore.HoldingRegisters[3] = (ushort)((currentAZ & 0xffff0000) >> 16);
		MCU_Modbusserver.DataStore.HoldingRegisters[4] = (ushort)(currentAZ & 0xffff);
		MCU_Modbusserver.DataStore.HoldingRegisters[13] = (ushort)((currentEL & 0xffff0000) >> 16);
		MCU_Modbusserver.DataStore.HoldingRegisters[14] = (ushort)(currentEL & 0xffff);
		return true;
	}
	
	private ushort[] Copy_modbus_registers(int start_index, int length)
	{
		ushort[] data = new ushort[length];
		for(int i = 0; i < length; i++)
		{
			data[i] = MCU_Modbusserver.DataStore.HoldingRegisters[i + start_index];
		}
		return data;
	}
	
	public void Bring_down()
	{
		runsimulator = false;
		PLC_emulator_thread.Join();
		PLCTCPClient.Dispose();
		PLCModbusMaster.Dispose();
		MCU_emulator_thread.Join();
		MCU_TCPListener.Stop();
		MCU_Modbusserver.Dispose();
	}
	
	void ExitServer()
	{
		tcpListener.Server.Close();
	}
}
