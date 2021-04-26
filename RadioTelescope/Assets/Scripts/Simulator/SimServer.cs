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
	
	// for controlling the VR telescope
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
	
	private bool runSimulator = false;
	private bool moving = false;
	private bool jogging = false;
	private bool isConfigured = false;
	private bool isTest = false;
	private bool isJogComand = false;
	
	/// <summary>
	/// Start is called before the first frame
	/// </summary>
	public void Start()
	{
		tc.speed = speed;
		tc.UpdateAzimuthUI(0.0f);
		tc.UpdateElevationUI(15.0f);
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
		// Don't start the sim twice and maybe somehow screw something up.
		if(runSimulator)
			return;
		Debug.Log("Start Button clicked");
		
		tc.speed = speed;
		
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
		startButton.GetComponent<Image>().color = Color.green;
	}
	
	/// <summary>
	/// Update is called once per frame
	/// </summary>
	void Update () 
	{ 		
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
			Thread.Sleep(100);
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
				Debug.Log("--------------------------------------------- !! New Register Data Incoming !!");
				currentCommand = buildMCUCommand(current);
			}
			if(moving)
			{
				// we are still in motion
				// TODO: here we can write back more checks (like if an error happens)
				if(currentCommand.azimuthDegrees != tc.simTelescopeAzimuthDegrees || currentCommand.elevationDegrees != tc.simTelescopeElevationDegrees)
				{
					Debug.Log("SIMSERVER: Move not yet completed");
					updateMCURegistersStillMoving();
				}
				else
				{
					Debug.Log("SIMSERVER: MOVE COMPLETED");
					moving = false;
					
					updateMCURegistersFinishedMove();
				}
			}
			if(jogging)
			{
				// TODO: this can't be right. Right?
				// updateMCURegisters((int)currentCommand.azimuthSpeed, (int)currentCommand.elevationSpeed);
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
		} else if ((data[0] == 0x0080 )|| data[0] == 0x0100 || data[10] == 0x0080 || data[10] == 0x0100) // jog pos and neg, for az and el (az = 0, el = 10)
		{
			jogging = true;

		} else if(data[0] == 0x0002) // RELATIVE MOVE
		{
			moving = true;
		}

		// build mcu command based on register data
		currentCommand = new MCUCommand(data, tc.simTelescopeAzimuthDegrees);

		updateMCURegistersStillMoving();

		return currentCommand;
	}
	
	/// <summary>
	/// For now we will finish both axes at the same time - in the future this could be split out into seperate calls
	/// the control room looks at again the MSW (bit 0 for AZ, bit 10 for EL) and shifts it with the move complete constant (7), then & with 0b1
	/// If that comes out to 1, the move on that axis is done.
	/// </summary>
	private void updateMCURegistersFinishedMove()
	{
		// Azimuth
		MCU_Modbusserver.DataStore.HoldingRegisters[(int) RegPos.secondWordAzimuth] =  0xffff;
		
		// Elevation
		MCU_Modbusserver.DataStore.HoldingRegisters[(int) RegPos.secondWordElevation] =  0xffff;

	}

	/// <summary>
	/// For now we will update both axes (axis plural, i googled it)
	/// the control room looks for the most significant bit (AZ or EL) and then shifts it with the CCW_Motion constant (1) 
	/// or the CW_Motion constant (0). To show that this is still moving 
	/// </summary>
	private void updateMCURegistersStillMoving() 
	{
		// first zero our our finished moving registers
		// Azimuth
		MCU_Modbusserver.DataStore.HoldingRegisters[(int) RegPos.secondWordAzimuth] =  0x0000;
		
		// Elevation
		MCU_Modbusserver.DataStore.HoldingRegisters[(int) RegPos.firstWordElevation] =  0x0000;

		// last update the reg the CR looks for to see if we are in motion
		MCU_Modbusserver.DataStore.HoldingRegisters[(int) RegPos.firstWordAzimuth] =  0xffff;
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
