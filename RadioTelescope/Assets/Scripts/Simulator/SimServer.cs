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
	private TcpListener MCU_TCPListener;
	private ModbusSlave MCU_Modbusserver;
	private Thread MCU_emulator_thread;
	
	//for controlling the VR telescope
	public TelescopeControllerSim tc;

	public MCUCommand currentCommand;
	public float speed = 0.01f;
	private float azDeg = -42069;
	private float elDeg = -42069;
	
	//UI Related variables
	public TMP_InputField mcuIP;
	public TMP_InputField mcuPort;
	public Button startButton;
	public Button fillButton;
	
	private bool runSimulator = true, moving = false, jogging = false, isConfigured = false, isTest = false, isJogComand = false;
	
	/// <summary>
	/// Start is called before the first frame
	/// </summary>
	public void Start()
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

		// create a base current command object
		ushort[] noCommand = {0x0420};
		currentCommand = new MCUCommand(noCommand);
	}
	
	/// <summary>
	///	method tied to fill button which puts in correct values for the sim MCU
	/// </summary>
	public void AutoFillInput()
	{
		mcuIP.text = "127.0.0.1";
		mcuPort.text = "8083";
	}
	
	/// <summary>
	/// Start the MCU server and thread
	/// </summary>
	public void StartServer()
	{
		Debug.Log("Start Button clicked");
		tc.speed = speed;
		tc.TargetAzimuth(0.0f);
		tc.TargetElevation(15.0f);
		
		try
		{
			MCU_TCPListener = new TcpListener(new IPEndPoint(IPAddress.Parse(mcuIP.text), int.Parse(mcuPort.text)));
			MCU_emulator_thread = new Thread(new ThreadStart(runMCUThread));
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
		runSimulator = true;
		MCU_emulator_thread.Start();
	}
	
	/// <summary>
	/// Update is called once per frame
	/// </summary>
	void Update () { 		
		// send current command to controller
		if (!currentCommand.errorFlag)
			tc.SetNewMCUCommand(currentCommand);

		// press escape to exit the program cleanly
		if(Input.GetKeyDown((KeyCode.Escape)))
			Application.Quit();
	}
	
	/// <summary>
	/// main "spin" loop for the sim'd MCU server. Handles reading from the datastore and writing back progress of the movement
	/// TODO: what should we be writing back? Are we doing that now? How often should we be writing back?
	/// </summary>
	private void runMCUThread()
	{
		byte slaveId = 1;
		// create and start the modbus server TCP slave
		MCU_Modbusserver = ModbusTcpSlave.CreateTcp(slaveId, MCU_TCPListener);
		// coils, inputs, holdingRegisters, inputRegisters
		MCU_Modbusserver.DataStore = DataStoreFactory.CreateDefaultDataStore(0, 0, 1054, 0);
		
		MCU_Modbusserver.Listen();
		
		ushort[] last, current;
		last = copyModbusDataStoreRegisters(1025, 20);
		while (runSimulator)
		{
			Thread.Sleep(200);
			current = copyModbusDataStoreRegisters(1025, 20);

			// jog commands frequently send the same exact register contents (is jog), so we need a special case for them
			// 0x0080 and 0x0100 tell us the direction of the jog. This is handled in buildMCUCommand.
			// these checks are basically if (are we trying to jog something)
			// 								then constantly check for new register data;
			if (current[0] == 80 || current[0] == 100 || current[10] == 80 || current[10] == 100)
			{
				isJogComand = true;

			} else { isJogComand = false; }

			if(!current.SequenceEqual(last) || isJogComand)
			{
				Debug.Log("!! New Register Data Incoming !!");
				currentCommand = buildMCUCommand(current);
			}
			if(moving)
			{
				// figure out what we are updating the MCU registers with
				// TODO: no idea how jake got to this, need to figure out how he did (or ask him later)
				if(currentCommand.azimuthDegrees != 0 || currentCommand.elevationDegrees != 0)
				{
					// if the currentCommand's azimuth degrees are less than -( azimuth speed ):
					// 		return -( azimuth speed)
					// else
					//      if the currentCommand's azimuth degrees are greater than +( azimuith speed ):
					// 			  return azimuth speed
					// 		else
					// 			  return azimuth degrees
					float travAZ = (currentCommand.azimuthDegrees < -currentCommand.azimuthSpeed) ? -currentCommand.azimuthSpeed 
									: (currentCommand.azimuthDegrees > currentCommand.azimuthSpeed) ? currentCommand.azimuthSpeed : currentCommand.azimuthDegrees;


					// if the currentCommand's elevation degrees are less than -( elevation speed ): 
					// 		return -( elevation speed )
					// else
					//      if the currentCommand's elevation degrees are greater than +( elevation speed ):
					// 			  return elevation speed
					// 		else
					// 			  return elevation degrees		
					float travEL = (currentCommand.elevationDegrees < -currentCommand.elevationSpeed) ? -currentCommand.elevationSpeed
									: (currentCommand.elevationDegrees > currentCommand.elevationSpeed) ? currentCommand.elevationSpeed : currentCommand.elevationDegrees;

					updateMCURegisters((int)travAZ, (int)travEL);
				}
				else
				{
					moving = false;
					// TODO: updating this register store again, what do these values mean?
					//       the value we are OR'ing by is the positive (clockwise) jog, is this significant?
					MCU_Modbusserver.DataStore.HoldingRegisters[1] = (ushort)(MCU_Modbusserver.DataStore.HoldingRegisters[1] | 0x0080);
					MCU_Modbusserver.DataStore.HoldingRegisters[11] = (ushort)(MCU_Modbusserver.DataStore.HoldingRegisters[11] | 0x0080);
				}
			}
			if(jogging)
			{
				// TODO: this can't be right. Right?
				updateMCURegisters((int)currentCommand.azimuthSpeed, (int)currentCommand.elevationSpeed);
			}
			last = current;
		}
	}
	
	private MCUCommand buildMCUCommand(ushort[] data)
	{
		isConfigured = true;
		string outstr = "";
		for(int v = 0; v < data.Length; v++) {
			outstr += Convert.ToString( data[v] , 16 ).PadLeft( 5 ) + ",";
		}
		Debug.Log("Spitting out registers: \n");
		Debug.Log(outstr);
		Debug.Log("All done spitting out registers\n");

		jogging = false;

		// figure out which move we are doing to decide what we write to the input register store (MCU's RESPONSE to CONTROL ROOM)
		if (data[0] == 4)
		{
			Debug.Log("Recieved immediate stop.");
		} else if ((data[0] == 0x0080 )|| data[0] == 0x0100 || data[10] == 0x0080 || data[10] == 0x0100) 
		{
			jogging = true;

		} else if(data[0] == 0x0002 || data[0] == 0x0002) // RELATIVE MOVE
		{
			moving = true;
		}

		// build mcu command based on register data
		currentCommand = new MCUCommand(data);

		// set register store as in motion 
		// TODO: What do these values mean? Do we need to set them differently based on other cases?
		MCU_Modbusserver.DataStore.HoldingRegisters[1] = (ushort)(MCU_Modbusserver.DataStore.HoldingRegisters[1] & 0xff7f);
		MCU_Modbusserver.DataStore.HoldingRegisters[11] = (ushort)(MCU_Modbusserver.DataStore.HoldingRegisters[11] & 0xff7f);

		return currentCommand;
	}
	
	/// <summary>
	/// Writes information back into the register store based on move variables
	/// TODO: right now this is definitely not right. What do we update by?
	/// </summary>
	private void updateMCURegisters(float travAZ, float travEL)
	{
		// currentCommand.azimuthDegrees += travAZ;
		// currentCommand.elevationDegrees += travEL;

		MCU_Modbusserver.DataStore.HoldingRegisters[3] = (ushort)(((int)currentCommand.azimuthDegrees & 0xffff0000) >> 16);
		MCU_Modbusserver.DataStore.HoldingRegisters[4] = (ushort)((int)currentCommand.azimuthDegrees & 0xffff);
		MCU_Modbusserver.DataStore.HoldingRegisters[13] = (ushort)(((int)currentCommand.elevationDegrees & 0xffff000) >> 16);
		MCU_Modbusserver.DataStore.HoldingRegisters[14] = (ushort)((int)currentCommand.elevationDegrees & 0xffff);
	}
	
	/// <summary>
	/// returns a copy of the current MCU server datastore (REGISTERS)
	/// </summary>
	private ushort[] copyModbusDataStoreRegisters(int start_index, int length)	{
		ushort[] data = new ushort[length];
		for(int i = 0; i < length; i++)
		{
			data[i] = MCU_Modbusserver.DataStore.HoldingRegisters[i + start_index];
		}
		return data;
	}
	
	public void Bring_down()
	{
		runSimulator = false;
		MCU_emulator_thread.Join();
		MCU_TCPListener.Stop();
		MCU_Modbusserver.Dispose();
	}
	
	void ExitServer()
	{
		tcpListener.Server.Close();
	}
}
