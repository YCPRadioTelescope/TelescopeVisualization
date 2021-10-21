using System;
using System.IO;
using System.Net; 
using System.Net.Sockets; 
using System.Text; 
using System.Threading; 
using UnityEngine;
using Modbus.Data;
using Modbus.Device;
using System.Linq;
using static MCUCommand;

public class SimServer : MonoBehaviour {
	
	/// <summary>
	/// Private constants
	/// </summary>
	private const int AZIMUTH_GEARING_RATIO = 500;
	private const int ELEVATION_GEARING_RATIO = 50;
	private const int ENCODER_COUNTS_PER_REVOLUTION_BEFORE_GEARING = 8000;
	private const int STEPS_PER_REVOLUTION = 20000;
	
	private const int incomingRegisterSize = 20;
	private const int outgoingRegisterSize = 10;
	
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
	
	public UIHandler ui;
	
	public MCUCommand command;
	private float azDeg = -42069;
	private float elDeg = -42069;
	
	private bool runSimulator = false;
	private bool homing = false;
	
	/// <summary>
	/// Start the MCU server and thread
	/// </summary>
	public void StartServer()
	{
		// Don't start the sim twice and maybe somehow screw something up.
		if(runSimulator)
			return;
		
		try
		{
			MCU_TCPListener = new TcpListener(new IPEndPoint(IPAddress.Parse(ui.MCUIP()), int.Parse(ui.MCUPort())));
			MCU_emulator_thread = new Thread(new ThreadStart(RunMCUThread));
		}
		catch(Exception e)
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
		catch(Exception e)
		{
			if((e is SocketException) || (e is ArgumentOutOfRangeException) || (e is InvalidOperationException))
			{
				Debug.Log(e);
				return;
			}
		}
		runSimulator = true;
		MCU_emulator_thread.Start();
		ui.StartSim();
	}
	
	/// <summary>
	/// main "spin" loop for the sim'd MCU server. Handles reading from the datastore and writing back progress of the movement
	/// TODO: what should we be writing back? Are we doing that now? How often should we be writing back?
	/// </summary>
	private void RunMCUThread()
	{
		byte slaveId = 1;
		// create and start the modbus server TCP slave
		MCU_Modbusserver = ModbusTcpSlave.CreateTcp(slaveId, MCU_TCPListener);
		// coils, inputs, holdingRegisters, inputRegisters
		MCU_Modbusserver.DataStore = DataStoreFactory.CreateDefaultDataStore(0, 0, 1054, 0);
		
		MCU_Modbusserver.Listen();
		
		ushort[] current;
		ushort[] last = new ushort[incomingRegisterSize];
		while(runSimulator)
		{
			// Sleep so that we're not running as fast as the CPU allows, which is overkill.
			Thread.Sleep(100);
			
			// Get the latest registers and update the UI.
			current = CopyRegisters(1025, incomingRegisterSize);
			ui.UpdateIncoming(current);
			
			// If these registers are new or this is a jog command, construct a new MCUCommand
			// object for the telescope controller. This object gets sent to the telescope controller
			// every frame in the Update function.
			if(!current.SequenceEqual(last))
				command.UpdateCommand(current, tc.Azimuth(), tc.Elevation());
			
			// we are still in motion
			// TODO: here we can write back more checks (like if an error happens)
			if(tc.AzimuthMoving())
				UpdateRegistersAzimuthMoving();
			else
				UpdateRegistersAzimuthStopped();
			
			if(tc.ElevationMoving())
				UpdateRegistersElevationMoving();
			else
				UpdateRegistersElevationStopped();
				
			if(tc.Homed())
				UpdateRegistersFinishedHome();
			
			// Update the telescope's current position.
			UpdateRegistersPosition();
			// Update the UI with the outgoing register values.
			ui.UpdateOutgoing(GenerateOutgoing());
			last = current;
		}
	}
	
	/// <summary>
	/// Writes to shared register store with the current position of the sim telescope
	/// This needs to convert the degrees of our azimuth and elevation to steps and encoder steps
	/// the CR looks for registers  [2 + 3 = azSteps]
	/// 							[3 + 4 = azEncoder]
	/// 							[12 + 13 = elSteps]
	/// 							[14 + 15 = elEncoder]
	/// </summary>
	private void UpdateRegistersPosition()
	{
		int azEncoder = DegreesToEncoder(tc.Azimuth(), AZIMUTH_GEARING_RATIO);
		int elEncoder = (-1) * DegreesToEncoder(tc.Elevation() - 15.0f, ELEVATION_GEARING_RATIO);
		int azSteps = DegreesToSteps(tc.Azimuth(), AZIMUTH_GEARING_RATIO);
		int elSteps = (-1) * DegreesToSteps(tc.Elevation() - 15.0f, ELEVATION_GEARING_RATIO);
		
		// write actual values using some magic bit work
		// we need to split it across 2 register because 1 reg doesn't have enough space
		MCU_Modbusserver.DataStore.HoldingRegisters[(int)WriteBackRegPos.firstWordAzimuthSteps] = (ushort)(azSteps >> 16);
		MCU_Modbusserver.DataStore.HoldingRegisters[(int)WriteBackRegPos.secondWordAzimuthSteps] = (ushort)(azSteps & 0xffff);
		MCU_Modbusserver.DataStore.HoldingRegisters[(int)WriteBackRegPos.firstWordElevationSteps] = (ushort)(elSteps >> 16);
		MCU_Modbusserver.DataStore.HoldingRegisters[(int)WriteBackRegPos.secondWordElevationSteps] = (ushort)(elSteps & 0xffff);
		
		MCU_Modbusserver.DataStore.HoldingRegisters[(int)WriteBackRegPos.firstWordAzimuthEncoder] = (ushort)(azEncoder >> 16);
		MCU_Modbusserver.DataStore.HoldingRegisters[(int)WriteBackRegPos.secondWordAzimuthEncoder] = (ushort)(azEncoder & 0xffff);
		MCU_Modbusserver.DataStore.HoldingRegisters[(int)WriteBackRegPos.firstWordElevationEncoder] = (ushort)(elEncoder >> 16);
		MCU_Modbusserver.DataStore.HoldingRegisters[(int)WriteBackRegPos.secondWordElevationEncoder] = (ushort)(elEncoder & 0xffff);
	}

	/// <summary>
	/// The CR looks has a special check to see if homing finishes (the mcu writes back a special bit) so we should too
	/// </summary>
	private void UpdateRegistersFinishedHome()
	{
		// the enum doesn't full line up here, but to check for homing the CR looks for registerData[0] so the value of the enum lines up
		// a lot of commands are like this, they take the first word (registerData[0]) which lines up for the azimuth side of the command
		MCU_Modbusserver.DataStore.HoldingRegisters[(int)WriteBackRegPos.finishedMovingAzimuth] = (ushort)MCUWriteBack.finishedHome + (ushort)MCUWriteBack.finishedMove;
		
		// this is not needed for the homing check, but it is still grabbed to see if we are done moving we need to update the elevation first word as well
		// otherwise homing never ends on the CR side
		MCU_Modbusserver.DataStore.HoldingRegisters[(int)WriteBackRegPos.finishedMovingElevation] = (ushort)MCUWriteBack.finishedMove;
	}
	
	/// <summary>
	/// For now we will finish both axes at the same time - in the future this could be split out into seperate calls
	/// the control room looks at again the MSW (bit 0 for AZ, bit 10 for EL) and shifts it with the move complete constant (7 bits to the right), then & with 0b1
	/// </summary>
	private void UpdateRegistersAzimuthStopped()
	{
		// Azimuth
		MCU_Modbusserver.DataStore.HoldingRegisters[(int)WriteBackRegPos.finishedMovingAzimuth] = (ushort)MCUWriteBack.finishedMove;
	}
	
	private void UpdateRegistersElevationStopped()
	{
		// Elevation
		MCU_Modbusserver.DataStore.HoldingRegisters[(int)WriteBackRegPos.finishedMovingElevation] = (ushort)MCUWriteBack.finishedMove;
	}
	
	/// <summary>
	/// For now we will update both axes (axis plural, i googled it)
	/// the control room looks for the most significant bit (AZ or EL) and then shifts it with the CCW_Motion constant (1) 
	/// or the CW_Motion constant (0). To show that this is still moving. The 0's are for the shift 1 right 
	/// </summary>
	private void UpdateRegistersAzimuthMoving() 
	{
		// Azimuth
		MCU_Modbusserver.DataStore.HoldingRegisters[(int)WriteBackRegPos.stillMovingAzimuth] = (ushort)MCUWriteBack.stillMoving; 
	}
	
	private void UpdateRegistersElevationMoving() 
	{
		// Elevation
		MCU_Modbusserver.DataStore.HoldingRegisters[(int)WriteBackRegPos.stillMovingElevation] = (ushort)MCUWriteBack.stillMoving;
	}
	
	/// <summary>
	/// returns a copy of the current MCU server datastore (REGISTERS)
	/// </summary>
	private ushort[] CopyRegisters(int start_index, int length)
	{
		ushort[] data = new ushort[length];
		for(int i = 0; i < length; i++)
			data[i] = MCU_Modbusserver.DataStore.HoldingRegisters[start_index + i];
		
		return data;
	}
	
	private ushort[] GenerateOutgoing()
	{
		ushort[] data = new ushort[outgoingRegisterSize];
		int pos = 0;
		SetData(data, pos++, (int)WriteBackRegPos.stillMovingAzimuth);
		SetData(data, pos++, (int)WriteBackRegPos.firstWordAzimuthSteps);
		SetData(data, pos++, (int)WriteBackRegPos.secondWordAzimuthSteps);
		SetData(data, pos++, (int)WriteBackRegPos.firstWordAzimuthEncoder);
		SetData(data, pos++, (int)WriteBackRegPos.secondWordAzimuthEncoder);
		
		SetData(data, pos++, (int)WriteBackRegPos.stillMovingElevation);
		SetData(data, pos++, (int)WriteBackRegPos.firstWordElevationSteps);
		SetData(data, pos++, (int)WriteBackRegPos.secondWordElevationSteps);
		SetData(data, pos++, (int)WriteBackRegPos.firstWordElevationEncoder);
		SetData(data, pos++, (int)WriteBackRegPos.secondWordElevationEncoder);
		return data;
	}
	
	private void SetData(ushort[] data, int position, int register)
	{
		data[position] = MCU_Modbusserver.DataStore.HoldingRegisters[register];
	}
	
	/// <summary>
	/// Helper method to convert degrees to the encoder values expected by the control room
	/// </summary>
	/// <param name="degrees"> the actual degrees of the sim telescope (per axis) </param>
	/// <param name="gearingRatio"> corresponds to the axis we are converting </param>
	/// <returns></returns>
	private int DegreesToEncoder(float degrees, int gearingRatio)
	{
		return (int)(degrees * ENCODER_COUNTS_PER_REVOLUTION_BEFORE_GEARING * gearingRatio / 360.0);
	}
	
	/// <summary>
	/// Helper method to convert degrees back to steps
	/// </summary>
	/// <param name="degrees"> actual degrees of the sim telescope (per axis) </param>
	/// <param name="gearingRatio">  </param>
	/// <returns></returns>
	private int DegreesToSteps(float degrees, int gearingRatio)
	{
		return (int)(degrees * STEPS_PER_REVOLUTION * gearingRatio / 360.0);
	}
}
