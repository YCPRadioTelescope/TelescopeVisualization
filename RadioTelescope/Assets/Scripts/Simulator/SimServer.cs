using System;
using System.IO;
using System.Linq;
using System.Net; 
using System.Net.Sockets; 
using System.Text; 
using System.Threading; 
using UnityEngine;
using Modbus.Data;
using Modbus.Device;

// This script is what communicates with the control room, receiving commands through the
// modbus registers and updating the the MCUCommand object that the TelescopeController uses
// to determine what the telescope object should do.
public class SimServer : MonoBehaviour
{
	// The object that controls the telescope's movement according to the current command.
	public TelescopeControllerSim tc;
	
	// The command that determines the telescope's movement.
	public MCUCommand command;
	
	// The object that updates the UI with the state of variables.
	public UIHandler ui;
	
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
	
	// Keep track of whether the simulation connection has been started.
	private bool runSimulator = false;
	
	/// <summary>
	/// Private constants
	/// </summary>
	private const int AZIMUTH_GEARING_RATIO = 500;
	private const int ELEVATION_GEARING_RATIO = 50;
	private const int STEPS_PER_REVOLUTION = 20000;
	private const int ENCODER_COUNTS_PER_REVOLUTION_BEFORE_GEARING = 8000;
	
	private const int INCOMING_REGISTERS_SIZE = 20;
	private const int OUTGOING_REGISTERS_SIZE = 10;
	
	/// <summary>
	/// Establish the TCP connection to the control room and start the MCU thread
	/// that monitors the modbus registers. This function is called by UIHandler
	/// when the Start Sim button on the UI is pressed.
	/// </summary>
	public void StartServer()
	{
		// Don't attempt to start the simulator twice.
		if(runSimulator)
			return;
		
		// Create the TCP listener for the control room connection and
		// the MCUThread that will monitor the modbus registers.
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
		
		// Start the TCP listener.
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
		
		// Mark the simulation as having been started and start the MCU thread.
		runSimulator = true;
		MCU_emulator_thread.Start();
		ui.StartSim();
	}
	
	/// <summary>
	/// A thread that monitors the modbus registers, updating the MCUCommand if any new
	/// registers are received and updating the outgoing registers with the current state
	/// of the simulation.
	/// </summary>
	private void RunMCUThread()
	{
		// Begin monitoring the modbus registers.
		byte slaveId = 1;
		MCU_Modbusserver = ModbusTcpSlave.CreateTcp(slaveId, MCU_TCPListener);
		MCU_Modbusserver.DataStore = DataStoreFactory.CreateDefaultDataStore(0, 0, 1054, 0);
		MCU_Modbusserver.Listen();
		
		ushort[] current;
		ushort[] last = new ushort[INCOMING_REGISTERS_SIZE];
		while(runSimulator)
		{
			// Sleep so that we're not running as fast as the CPU allows, which is overkill.
			Thread.Sleep(100);
			
			// Get the latest registers and update the UI.
			current = CopyRegisters(1025, INCOMING_REGISTERS_SIZE);
			ui.UpdateIncoming(current);
			
			// If these registers are new, update the MCUCommand object for the telescope controller.
			if(!current.SequenceEqual(last))
				command.UpdateCommand(current);
			
			// TODO FROM LUCAS: here we can write back more checks (like if an error happens)
			
			// Update the outgoing registers.
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
			
			UpdateRegistersPosition();
			
			// Update the UI with the outgoing register values.
			ui.UpdateOutgoing(GenerateOutgoing());
			
			// Set the current registers to the last registers.
			last = current;
		}
	}
	
	/// <summary>
	/// Writes to shared register store with the current position of the sim telescope.
	/// This needs to convert the degrees of our azimuth and elevation to steps and encoder steps.
	/// The CR looks for registers  [2 + 3 = azSteps]
	/// 							[3 + 4 = azEncoder]
	/// 							[12 + 13 = elSteps]
	/// 							[14 + 15 = elEncoder]
	/// </summary>
	private void UpdateRegistersPosition()
	{
		// Convert the telescope controller's current orietnation from degrees to steps and encoder values.
		int azSteps = DegreesToSteps(tc.Azimuth(), AZIMUTH_GEARING_RATIO);
		int elSteps = (-1) * DegreesToSteps(tc.Elevation() - 15.0f, ELEVATION_GEARING_RATIO);
		int azEncoder = DegreesToEncoder(tc.Azimuth(), AZIMUTH_GEARING_RATIO);
		int elEncoder = (-1) * DegreesToEncoder(tc.Elevation() - 15.0f, ELEVATION_GEARING_RATIO);
		
		// The integers representing the step and encoder values are spread across two ushort registers.
		SetRegister((int)WriteBackRegPos.firstWordAzimuthSteps, (ushort)(azSteps >> 16));
		SetRegister((int)WriteBackRegPos.secondWordAzimuthSteps, (ushort)(azSteps & 0xffff));
		SetRegister((int)WriteBackRegPos.firstWordElevationSteps, (ushort)(elSteps >> 16));
		SetRegister((int)WriteBackRegPos.secondWordElevationSteps, (ushort)(elSteps & 0xffff));
		
		SetRegister((int)WriteBackRegPos.firstWordAzimuthEncoder, (ushort)(azEncoder >> 16));
		SetRegister((int)WriteBackRegPos.secondWordAzimuthEncoder, (ushort)(azEncoder & 0xffff));
		SetRegister((int)WriteBackRegPos.firstWordElevationEncoder, (ushort)(elEncoder >> 16));
		SetRegister((int)WriteBackRegPos.secondWordElevationEncoder, (ushort)(elEncoder & 0xffff));
	}

	/// <summary>
	/// Mark the azimuth and elevation motors as having stopped, and mark the telescope as being homed.
	/// </summary>
	private void UpdateRegistersFinishedHome()
	{
		SetRegister((int)WriteBackRegPos.finishedMovingAzimuth, (ushort)(MCUWriteBack.finishedHome) + (ushort)(MCUWriteBack.finishedMove));
		SetRegister((int)WriteBackRegPos.finishedMovingElevation, (ushort)MCUWriteBack.finishedMove);
	}
	
	/// <summary>
	/// Mark the azimuth motors as having stopped.
	/// </summary>
	private void UpdateRegistersAzimuthStopped()
	{
		SetRegister((int)WriteBackRegPos.finishedMovingAzimuth, (ushort)MCUWriteBack.finishedMove);
	}
	
	/// <summary>
	/// Mark the elevation motors as having stopped.
	/// </summary>
	private void UpdateRegistersElevationStopped()
	{
		SetRegister((int)WriteBackRegPos.finishedMovingElevation, (ushort)MCUWriteBack.finishedMove);
	}
	
	/// <summary>
	/// Mark the azimuth motors as currently moving.
	/// </summary>
	private void UpdateRegistersAzimuthMoving() 
	{
		SetRegister((int)WriteBackRegPos.stillMovingAzimuth, (ushort)MCUWriteBack.stillMoving);
	}
	
	/// <summary>
	/// Mark the elevation motors as currently moving.
	/// </summary>
	private void UpdateRegistersElevationMoving() 
	{
		SetRegister((int)WriteBackRegPos.stillMovingElevation, (ushort)MCUWriteBack.stillMoving);
	}
	
	/// <summary>
	/// Return a copy of the current MCU server datastore, i.e. the modbus registers.
	/// This only returns the subset of the data that we actually need.
	/// </summary>
	private ushort[] CopyRegisters(int start, int length)
	{
		ushort[] data = new ushort[length];
		for(int i = 0; i < length; i++)
			SetData(data, i, start + i);
		
		return data;
	}
	
	/// <summary>
	/// Generate and return an array of the outgoing modbus registers that the
	/// simulation alters.
	/// </summary>
	private ushort[] GenerateOutgoing()
	{
		ushort[] data = new ushort[OUTGOING_REGISTERS_SIZE];
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
	
	/// <summary>
	/// Set the given register to the given data.
	/// </summary>
	private void SetRegister(int register, ushort data)
	{
		MCU_Modbusserver.DataStore.HoldingRegisters[register] = data;
	}
	
	/// <summary>
	/// Set the given position of the given ushort array with the data from the given register position.
	/// </summary>
	private void SetData(ushort[] data, int position, int register)
	{
		data[position] = MCU_Modbusserver.DataStore.HoldingRegisters[register];
	}
	
	/// <summary>
	/// Helper method to convert degrees into encoder values.
	/// </summary>
	private int DegreesToEncoder(float degrees, int gearingRatio)
	{
		return (int)(degrees * ENCODER_COUNTS_PER_REVOLUTION_BEFORE_GEARING * gearingRatio / 360.0);
	}
	
	/// <summary>
	/// Helper method to convert degrees into steps.
	/// </summary>
	private int DegreesToSteps(float degrees, int gearingRatio)
	{
		return (int)(degrees * STEPS_PER_REVOLUTION * gearingRatio / 360.0);
	}
}
