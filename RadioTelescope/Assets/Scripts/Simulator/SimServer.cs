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
using log4net;

// This script is what communicates with the control room, receiving commands through the
// modbus registers and updating the the MCUCommand object that the TelescopeController uses
// to determine what the telescope object should do.
public class SimServer : MonoBehaviour
{
	// log4net logger.
	private static readonly ILog Log = LogManager.GetLogger(typeof(SimServer));
	
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
	private const int OUTGOING_REGISTERS_SIZE = 13;
	
	private const float HEARTBEAT_FLIP = 0.5f;
	private float timer = 0.0f;
	
	void Update()
	{
		timer += Time.deltaTime;
	}
	
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
		
		timer = 0.0f;
		
		ushort[] current;
		ushort[] last = new ushort[INCOMING_REGISTERS_SIZE];
		while(runSimulator)
		{
			// Sleep so that we're not running as fast as the CPU allows, which is overkill.
			Thread.Sleep(50);
			
			// Get the latest registers and update the UI.
			current = CopyRegisters(1025, INCOMING_REGISTERS_SIZE);
			ui.UpdateIncoming(current);
			
			// If these registers are new, update the MCUCommand object for the telescope controller.
			if(!current.SequenceEqual(last))
				command.UpdateCommand(current);
			
			// Update the outgoing registers.
			UpdateMSWStatus();
			UpdateLSWStatus();
			UpdateHeartbeat();
			UpdatePosition();
			
			// Update the UI with the outgoing register values.
			ui.UpdateOutgoing(GenerateOutgoing());
			
			// Set the current registers to the last registers.
			last = current;
		}
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
	/// Update the status bits for azimuth and elevtion in the outgoing registers.
	/// </summary>
	private void UpdateMSWStatus()
	{
		// Set bit positions 0 or 1 depending on the direction of motor movement.
		// Bit 2 is unused/unimplemented.
		// Set bit position 3 if the motor isn't moving.
		// Set bit position 4 if the motor is in the home position.
		// Set bit positions 5 or 6 depending on the direction of accelerating, should the motors be moving.
		// Set bit posiiton 7 if a relative movement command has completed.
		if(tc.AzimuthMoving())
		{
			if(tc.AzimuthPosMotion())
				SetAzimuthStatusBit((int)MSWStatusBit.posMoving);
			else
				SetAzimuthStatusBit((int)MSWStatusBit.negMoving);
			
			if(tc.AzimuthAccelerating())
			{
				SetAzimuthStatusBit((int)MSWStatusBit.accelerating);
				ResetAzimuthStatusBit((int)MSWStatusBit.decelerating);
			}
			else if(tc.AzimuthDecelerating())
			{
				SetAzimuthStatusBit((int)MSWStatusBit.decelerating);
				ResetAzimuthStatusBit((int)MSWStatusBit.accelerating);
			}
			else
			{
				ResetAzimuthStatusBit((int)MSWStatusBit.accelerating);
				ResetAzimuthStatusBit((int)MSWStatusBit.decelerating);
			}
			
			ResetAzimuthStatusBit((int)MSWStatusBit.homed);
			ResetAzimuthStatusBit((int)MSWStatusBit.stopped);
			ResetAzimuthStatusBit((int)MSWStatusBit.complete);
		}
		else
		{
			SetAzimuthStatusBit((int)MSWStatusBit.stopped);
			if(tc.AzimuthHomed())
				SetAzimuthStatusBit((int)MSWStatusBit.homed);
			if(tc.RelativeMove())
				SetAzimuthStatusBit((int)MSWStatusBit.complete);
			
			ResetAzimuthStatusBit((int)MSWStatusBit.posMoving);
			ResetAzimuthStatusBit((int)MSWStatusBit.negMoving);
			ResetAzimuthStatusBit((int)MSWStatusBit.accelerating);
			ResetAzimuthStatusBit((int)MSWStatusBit.decelerating);
		}
		
		if(tc.ElevationMoving())
		{
			if(tc.ElevationPosMotion())
				SetElevationStatusBit((int)MSWStatusBit.posMoving);
			else
				SetElevationStatusBit((int)MSWStatusBit.negMoving);
			
			if(tc.ElevationAccelerating())
			{
				SetElevationStatusBit((int)MSWStatusBit.accelerating);
				ResetElevationStatusBit((int)MSWStatusBit.decelerating);
			}
			else if(tc.ElevationDecelerating())
			{
				SetElevationStatusBit((int)MSWStatusBit.decelerating);
				ResetElevationStatusBit((int)MSWStatusBit.accelerating);
			}
			else
			{
				ResetElevationStatusBit((int)MSWStatusBit.accelerating);
				ResetElevationStatusBit((int)MSWStatusBit.decelerating);
			}
			
			ResetElevationStatusBit((int)MSWStatusBit.homed);
			ResetElevationStatusBit((int)MSWStatusBit.stopped);
			ResetElevationStatusBit((int)MSWStatusBit.complete);
		}
		else
		{
			SetElevationStatusBit((int)MSWStatusBit.stopped);
			if(tc.ElevationHomed())
				SetElevationStatusBit((int)MSWStatusBit.homed);
			if(tc.RelativeMove())
				SetElevationStatusBit((int)MSWStatusBit.complete);
			
			ResetElevationStatusBit((int)MSWStatusBit.posMoving);
			ResetElevationStatusBit((int)MSWStatusBit.negMoving);
			ResetElevationStatusBit((int)MSWStatusBit.accelerating);
			ResetElevationStatusBit((int)MSWStatusBit.decelerating);
		}
		
		// Bit 8 and 9 are unused/unimplemented.
		
		// Set bit 10 when the simulation has first been started. Bit 10 is reset
		// after the telescope has been homed for the first time.
		if(command.invalidAzimuthPosition)
			SetAzimuthStatusBit((int)MSWStatusBit.invalidPosition);
		else
			ResetAzimuthStatusBit((int)MSWStatusBit.invalidPosition);
		
		if(command.invalidElevationPosition)
			SetElevationStatusBit((int)MSWStatusBit.invalidPosition);
		else
			ResetElevationStatusBit((int)MSWStatusBit.invalidPosition);
		
		// Set bit position 11 if a limit switch has been hit.
		// command.invalidInput gets set true in this case in TelescopeController.
		if(command.invalidInput)
			SetBothStatusBits((int)MSWStatusBit.invalidInput);
		else
			ResetBothStatusBits((int)MSWStatusBit.invalidInput);
		
		// Bit 12 and 13 are unused/unimplemented.
		
		// Set bit position 14 if the simulation has received a configure MCU command.
		if(command.configured)
			SetBothStatusBits((int)MSWStatusBit.axisEnabled);
		else
			ResetBothStatusBits((int)MSWStatusBit.axisEnabled);
	}
	
	private void UpdateLSWStatus()
	{
		// If the telescope has been homed, LSW status bit 2 gets set depending
		// on the telescope's orientation relative to the home position.
		if(!command.invalidAzimuthPosition && tc.Azimuth() > 180.0f)
			SetBit((int)OutgoingRegIndex.secondStatusAzimuth, (int)LSWStatusBit.home);
		else
			ResetBit((int)OutgoingRegIndex.secondStatusAzimuth, (int)LSWStatusBit.home);
		
		if(!command.invalidElevationPosition && tc.Elevation() > 15.0f)
			SetBit((int)OutgoingRegIndex.secondStatusElevation, (int)LSWStatusBit.home);
		else
			ResetBit((int)OutgoingRegIndex.secondStatusElevation, (int)LSWStatusBit.home);
		
		// If at any point a relative move as send, LSW status bit 7 get set.
		if(command.relativeMove && Mathf.Abs(command.cachedAzData) > 0.0f)
			SetBit((int)OutgoingRegIndex.secondStatusAzimuth, (int)LSWStatusBit.writeComplete);
		
		if(command.relativeMove && Mathf.Abs(command.cachedElData) > 0.0f)
			SetBit((int)OutgoingRegIndex.secondStatusElevation, (int)LSWStatusBit.writeComplete);
	}
	
	/// <summary>
	/// Set a specific status bit on both motor registers.
	/// </summary>
	private void SetBothStatusBits(int position)
	{
		SetAzimuthStatusBit(position);
		SetElevationStatusBit(position);
	}
	
	/// <summary>
	/// Reset a specific status bit on both motor registers.
	/// </summary>
	private void ResetBothStatusBits(int position)
	{
		ResetAzimuthStatusBit(position);
		ResetElevationStatusBit(position);
	}
	
	/// <summary>
	/// Set a specific azimuth status bit.
	/// </summary>
	private void SetAzimuthStatusBit(int position)
	{
		SetBit((int)OutgoingRegIndex.firstStatusAzimuth, position);
	}
	
	/// <summary>
	/// Reset a specific azimuth status bit.
	/// </summary>
	private void ResetAzimuthStatusBit(int position)
	{
		ResetBit((int)OutgoingRegIndex.firstStatusAzimuth, position);
	}
	
	/// <summary>
	/// Set a specific elevation status bit.
	/// </summary>
	private void SetElevationStatusBit(int position)
	{
		SetBit((int)OutgoingRegIndex.firstStatusElevation, position);
	}
	
	/// <summary>
	/// Reset a specific elevation status bit.
	/// </summary>
	private void ResetElevationStatusBit(int position)
	{
		ResetBit((int)OutgoingRegIndex.firstStatusElevation, position);
	}
	
	/// <summary>
	/// Update the heartbeat.
	/// </summary>
	private void UpdateHeartbeat()
	{
		const int HEARTBEAT_BIT = 14;
		
		if(timer >= HEARTBEAT_FLIP)
		{
			timer -= HEARTBEAT_FLIP;
			if(GetBit((int)OutgoingRegIndex.heartbeat, HEARTBEAT_BIT))
				ResetBit((int)OutgoingRegIndex.heartbeat, HEARTBEAT_BIT);
			else
				SetBit((int)OutgoingRegIndex.heartbeat, HEARTBEAT_BIT);
		}
	}
	
	/// <summary>
	/// Get the state of a specific bit position in a specific register index.
	/// </summary>
	private bool GetBit(int index, int position)
	{
		return ((MCU_Modbusserver.DataStore.HoldingRegisters[index] >> position) & 1) == 1;
	}
	
	/// <summary>
	/// Set a specific bit position in a specific register index.
	/// </summary>
	private void SetBit(int index, int position)
	{
		MCU_Modbusserver.DataStore.HoldingRegisters[index] |= (ushort)(1 << position);
	}
	
	/// <summary>
	/// Reset a specific bit position in a specific register index.
	/// </summary>
	private void ResetBit(int index, int position)
	{
		MCU_Modbusserver.DataStore.HoldingRegisters[index] &= (ushort)(0xffff - (1 << position));
	}
	
	/// <summary>
	/// Update the azimuth and elevation step and encoder positions for the outgoing registers.
	/// </summary>
	private void UpdatePosition()
	{
		// Convert the telescope controller's current orietnation from degrees to steps and encoder values.
		int azSteps = DegreesToSteps(tc.Azimuth(), AZIMUTH_GEARING_RATIO);
		int elSteps = (-1) * DegreesToSteps(tc.Elevation() - 15.0f, ELEVATION_GEARING_RATIO);
		int azEncoder = DegreesToEncoder(tc.Azimuth(), AZIMUTH_GEARING_RATIO);
		int elEncoder = (-1) * DegreesToEncoder(tc.Elevation() - 15.0f, ELEVATION_GEARING_RATIO);
		
		// The integers representing the step and encoder values are spread across two ushort registers.
		SetRegister((int)OutgoingRegIndex.firstWordAzimuthSteps, (ushort)(azSteps >> 16));
		SetRegister((int)OutgoingRegIndex.secondWordAzimuthSteps, (ushort)(azSteps & 0xffff));
		SetRegister((int)OutgoingRegIndex.firstWordElevationSteps, (ushort)(elSteps >> 16));
		SetRegister((int)OutgoingRegIndex.secondWordElevationSteps, (ushort)(elSteps & 0xffff));
		
		SetRegister((int)OutgoingRegIndex.firstWordAzimuthEncoder, (ushort)(azEncoder >> 16));
		SetRegister((int)OutgoingRegIndex.secondWordAzimuthEncoder, (ushort)(azEncoder & 0xffff));
		SetRegister((int)OutgoingRegIndex.firstWordElevationEncoder, (ushort)(elEncoder >> 16));
		SetRegister((int)OutgoingRegIndex.secondWordElevationEncoder, (ushort)(elEncoder & 0xffff));
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
	
	/// <summary>
	/// Set the given register to the given data.
	/// </summary>
	private void SetRegister(int register, ushort data)
	{
		MCU_Modbusserver.DataStore.HoldingRegisters[register] = data;
	}
	
	/// <summary>
	/// Generate and return an array of the outgoing modbus registers that the
	/// simulation alters. For use in the UI.
	/// </summary>
	private ushort[] GenerateOutgoing()
	{
		ushort[] data = new ushort[OUTGOING_REGISTERS_SIZE];
		int pos = 0;
		SetData(data, pos++, (int)OutgoingRegIndex.firstStatusAzimuth);
		SetData(data, pos++, (int)OutgoingRegIndex.secondStatusAzimuth);
		SetData(data, pos++, (int)OutgoingRegIndex.firstWordAzimuthSteps);
		SetData(data, pos++, (int)OutgoingRegIndex.secondWordAzimuthSteps);
		SetData(data, pos++, (int)OutgoingRegIndex.firstWordAzimuthEncoder);
		SetData(data, pos++, (int)OutgoingRegIndex.secondWordAzimuthEncoder);
		SetData(data, pos++, (int)OutgoingRegIndex.heartbeat);
		
		SetData(data, pos++, (int)OutgoingRegIndex.firstStatusElevation);
		SetData(data, pos++, (int)OutgoingRegIndex.secondStatusElevation);
		SetData(data, pos++, (int)OutgoingRegIndex.firstWordElevationSteps);
		SetData(data, pos++, (int)OutgoingRegIndex.secondWordElevationSteps);
		SetData(data, pos++, (int)OutgoingRegIndex.firstWordElevationEncoder);
		SetData(data, pos++, (int)OutgoingRegIndex.secondWordElevationEncoder);
		return data;
	}
	
	/// <summary>
	/// Set the given position of the given ushort array with the data from the given register position.
	/// </summary>
	private void SetData(ushort[] data, int position, int register)
	{
		data[position] = MCU_Modbusserver.DataStore.HoldingRegisters[register];
	}
}
