using UnityEngine;
using UnityEngine.UI;
using System;
using log4net;

// master list of known register data
// https://docs.google.com/spreadsheets/d/1vBKsnV7Xso0u19ZyhtVimiCXpZjc007usVjvAsxHJNU/edit#gid=0

/// <summary> 
/// Public class <c>MCUCommand</c> is used to store input register data from the control room
/// in an easy way for us to decode the commands on the <c>TelescopeControllerSim</c>
/// </summary>
public class MCUCommand : MonoBehaviour 
{
	// log4net logger.
	private static readonly ILog Log = LogManager.GetLogger(typeof(MCUCommand));
	
	// The object that controls the telescope's movement according to the current command.
	public TelescopeControllerSim tc;
	
	public string currentCommand = "";
	
	public float azimuthData = 0.0f;
	public float elevationData = 0.0f;
	public float azimuthSpeed = 0.0f;
	public float elevationSpeed = 0.0f;
	public float azimuthAcceleration = 0.0f;
	public float elevationAcceleration = 0.0f;
	public float azimuthDeceleration = 0.0f;
	public float elevationDeceleration = 0.0f;
	public float cachedAzData = 0.0f;
	public float cachedElData = 0.0f;
	
	public bool configured = false;
	public bool relativeMove = false;
	public bool jog = false;
	public bool posJog = false;
	public bool azJog = false;
	public bool home = false;
	public bool stop = false;
	
	// All commands are ignored until we can confirm that they're valid.
	public bool ignoreCommand = true;
	
	// The hardware always starts with the invalid position bits flipped.
	// The telescope must be homed to turn this off.
	public bool invalidAzimuthPosition = true;
	public bool invalidElevationPosition = true;
	public bool invalidInput = false;
	
	private int azimuthDataBits = 0;
	private int elevationDataBits = 0;
	private int azimuthSpeedBits = 0;
	private int elevationSpeedBits = 0;
	private int azimuthAccelerationBits = 0;
	private int elevationAccelerationBits = 0;
	private int azimuthDecelerationBits = 0;
	private int elevationDecelerationBits = 0;
	
	private const float STEPS_PER_REVOLUTION = 20000.0f;
	private const float AZIMUTH_GEARING_RATIO = 500.0f;
	private const float ELEVATION_GEARING_RATIO = 50.0f;
	
	// Initialize the simulation telescope to point to the home position.
	public void InitSim()
	{
		Log.Debug("Received simulation initialization.");
		Reset();
		currentCommand = "simulation initialization";
		azimuthSpeed = 20.0f;
		azimuthAcceleration = 0.9f;
		azimuthDeceleration = 0.9f;
		elevationSpeed = 20.0f;
		elevationAcceleration = 0.9f;
		elevationDeceleration = 0.9f;
		if(tc.Azimuth() < 180.0f)
			azimuthData = -tc.Azimuth();
		else
			azimuthData = 360.0f - tc.Azimuth();
		elevationData = -tc.Elevation() + 15.0f;
		cachedAzData = azimuthData;
		cachedElData = elevationData;
		LogMove();
		Log.Debug("Executing command.\n");
		ignoreCommand = false;
	}
	
	// Receive a test movement from the UI.
	public void TestMove(float azimuth, float elevation, float speed)
	{
		Log.Debug("Received test movement from simulation UI.");
		Reset();
		currentCommand = "simulation test movement";
		azimuthData = AngleDistance(azimuth, tc.Azimuth());
		elevationData = AngleDistance(elevation, tc.Elevation());
		cachedAzData = azimuthData;
		cachedElData = elevationData;
		azimuthSpeed = speed;
		azimuthAcceleration = 0.9f;
		azimuthDeceleration = 0.9f;
		elevationSpeed = speed;
		elevationAcceleration = 0.9f;
		elevationDeceleration = 0.9f;
		LogMove();
		Log.Debug("Executing command.\n");
		ignoreCommand = false;
	}
	
	/// <summary>
	/// Update the command with the register data received from the control room.
	/// </summary>
	/// <param name="registerData"> Raw register data from the modbus registers. </param>
	public void UpdateCommand(ushort[] registerData)
	{
		Log.Debug("Received new register data from SimServer.");
		// Reset the state of the MCUCommand so that information doesn't carry over from the previous command.
		Reset();
		
		Log.Debug("	Parsing register data:");
		// Grab the words that determine the incoming command.
		ushort firstCommandAzimuth = registerData[(int)IncomingRegIndex.firstCommandAzimuth];
		ushort secondCommandAzimuth = registerData[(int)IncomingRegIndex.secondCommandAzimuth];
		ushort firstCommandElevation = registerData[(int)IncomingRegIndex.firstCommandElevation];
		ushort secondCommandElevation = registerData[(int)IncomingRegIndex.secondCommandElevation];
		
		// Grab the information from the other registers. Values received from the control room are in steps and will
		// later be converted to degrees as necessary.
		azimuthDataBits = (registerData[(int)IncomingRegIndex.firstDataAzimuth] << 16) + registerData[(int)IncomingRegIndex.secondDataAzimuth];
		elevationDataBits = (registerData[(int)IncomingRegIndex.firstDataElevation] << 16) + registerData[(int)IncomingRegIndex.secondDataElevation];
		
		azimuthSpeedBits = (registerData[(int)IncomingRegIndex.firstSpeedAzimuth] << 16) + registerData[(int)IncomingRegIndex.secondSpeedAzimuth];
		elevationSpeedBits = (registerData[(int)IncomingRegIndex.firstSpeedElevation] << 16) + registerData[(int)IncomingRegIndex.secondSpeedElevation];
		
		azimuthAccelerationBits = registerData[(int)IncomingRegIndex.accelerationAzimuth];
		elevationAccelerationBits = registerData[(int)IncomingRegIndex.accelerationElevation];
		
		azimuthDecelerationBits = registerData[(int)IncomingRegIndex.decelerationAzimuth];
		elevationDecelerationBits = registerData[(int)IncomingRegIndex.decelerationElevation];
		
		Log.Debug("		MSW Command Azimuth:   0x" + Convert.ToString(firstCommandAzimuth, 16));
		Log.Debug("		LSW Command Azimuth:   0x" + Convert.ToString(secondCommandAzimuth, 16));
		Log.Debug("		Azimuth Data:          0x" + Convert.ToString(azimuthDataBits, 16));
		Log.Debug("		Azimuth Speed:         0x" + Convert.ToString(azimuthSpeedBits, 16) + "\n");
		
		Log.Debug("		MSW Command Elevation: 0x" + Convert.ToString(firstCommandElevation, 16));
		Log.Debug("		LSW Command Elevation: 0x" + Convert.ToString(secondCommandElevation, 16));
		Log.Debug("		Elevation Data:        0x" + Convert.ToString(elevationDataBits, 16));
		Log.Debug("		Elevation Speed:       0x" + Convert.ToString(elevationSpeedBits, 16) + "\n");
		
		Log.Debug("		Acceleration:          0x" + Convert.ToString(azimuthAccelerationBits, 16));
		
		// Determine the incoming command and make any changes to the received information if necessary.
		bool ignore = false;
		// Relative move:
		if(firstCommandAzimuth == (ushort)CommandType.RELATIVE_MOVE)
		{
			currentCommand = "relative move";
			relativeMove = true;
			// The positive and negative directions on the hardware elevation motor are flipped
			// compared to what the simulation uses, so flip the recevied value.
			ConvertToDegrees();
			elevationData *= -1;
			Log.Debug("	Relative move.");
			LogMove(2);
		}
		// Jogs:
		else if(firstCommandAzimuth == (ushort)CommandType.POSITIVE_JOG)
		{
			currentCommand = "positive azimuth jog";
			jog = true;
			azJog = true;
			posJog = true;
			ConvertToDegrees();
			Log.Debug("	Positive (CCW) azimuth jog.");
		}
		else if(firstCommandAzimuth == (ushort)CommandType.NEGATIVE_JOG)
		{
			currentCommand = "negative azimuth jog";
			jog = true;
			azJog = true;
			posJog = false;
			ConvertToDegrees();
			Log.Debug("	Negative (CW) azimuth jog.");
		}
		else if(firstCommandElevation == (ushort)CommandType.POSITIVE_JOG)
		{
			currentCommand = "positive elevation jog";
			jog = true;
			azJog = false;
			posJog = true;
			ConvertToDegrees();
			Log.Debug("	Positive elevation jog.");
		}
		else if(firstCommandElevation == (ushort)CommandType.NEGATIVE_JOG)
		{
			currentCommand = "negative elevation jog";
			jog = true;
			azJog = false;
			posJog = false;
			ConvertToDegrees();
			Log.Debug("	Negative elevation jog.");
		}
		// Homing:
		// Clockwise and counterclockwise homes are currently not handled differently from one another.
		else if(firstCommandAzimuth == (ushort)CommandType.CLOCKWISE_HOME
				|| firstCommandAzimuth == (ushort)CommandType.COUNTERCLOCKWISE_HOME)
		{
			currentCommand = "home";
			home = true;
			ConvertToDegrees();
			if(tc.Azimuth() < 180.0f)
				azimuthData = -tc.Azimuth();
			else
				azimuthData = 360.0f - tc.Azimuth();
			elevationData = -tc.Elevation() + 15.0f;
			Log.Debug("	Home.");
			LogMove(2);
		}
		// MCU related:
		// TODO FROM LUCAS: write back proper registers
		else if(firstCommandAzimuth == (ushort)CommandType.CONFIGURE_MCU)
		{
			currentCommand = "configure MCU";
			ignore = true;
			configured = true;
			Log.Debug("	Configure MCU.\n");
		}
		else if(firstCommandAzimuth == (ushort)CommandType.CLEAR_MCU_ERRORS)
		{
			currentCommand = "clear MCU errors";
			ClearMCUErrors();
			Log.Debug("	Clear MCU errors.");
		}
		// Stops:
		else if(firstCommandAzimuth == (ushort)CommandType.CONTROLLED_STOP)
		{
			currentCommand = "controlled stop";
			stop = true;
			Log.Debug("	Controlled stop.");
		}
		else if(firstCommandAzimuth == (ushort)CommandType.IMMEDIATE_STOP)
		{
			currentCommand = "immediate stop";
			stop = true;
			Log.Debug("	Immediate stop.");
		}
		else if(secondCommandElevation == (ushort)CommandType.CANCEL_MOVE)
		{
			currentCommand = "cancel move";
			stop = true;
			Log.Debug("	Cancel move.");
		}
		else
		{
			currentCommand = "unknown command";
			ignore = true;
			Log.Warn("	Unknown command received. Command will be ignored.\n");
		}
		
		if(!ignore)
		{
			Log.Debug("Executing command.\n");
			ignoreCommand = false;
		}
	}
	
	// Reset the state of the MCUCommand.
	private void Reset()
	{
		currentCommand = "";
		
		azimuthData = 0.0f;
		elevationData = 0.0f;
		azimuthSpeed = 0.0f;
		elevationSpeed = 0.0f;
		azimuthAcceleration = 0.0f;
		elevationAcceleration = 0.0f;
		azimuthDeceleration = 0.0f;
		elevationDeceleration = 0.0f;
		cachedAzData = 0.0f;
		cachedElData = 0.0f;
		
		// Configured is always true after connecting.
		relativeMove = false;
		jog = false;
		posJog = false;
		azJog = false;
		home = false;
		stop = false;
		
		ignoreCommand = true;
		
		// Error bools are only reset by a ClearMCUErrors command.
		
		azimuthDataBits = 0;
		elevationDataBits = 0;
		azimuthSpeedBits = 0;
		elevationSpeedBits = 0;
		azimuthAccelerationBits = 0;
		elevationAccelerationBits = 0;
		azimuthDecelerationBits = 0;
		elevationDecelerationBits = 0;
	}
	
	private void LogMove(int tabs = 1)
	{
		string whitespace = "";
		for(int i = 0; i < tabs; ++i)
			whitespace += "\t";
		Log.Debug(whitespace + "Moving by (" + azimuthData + "," + elevationData + ") at a speed of (" + azimuthSpeed + "," + elevationSpeed + ") degrees/second.");
		Log.Debug(whitespace + "Estimated completion time: " + Mathf.Max(Mathf.Abs(azimuthData)/azimuthSpeed, Mathf.Abs(elevationData)/elevationSpeed) + "s");
	}
	
	private void ClearMCUErrors()
	{
		// invalidPosition is not cleared by ClearMCUErrors.
		invalidInput = false;
	}
	
	/// <summary>
	/// Helper method used to convert raw hex register values to floats used by the controller.
	/// </summary>
	private void ConvertToDegrees() 
	{
		// Convert all step values to floats in degrees.
		azimuthData = ConvertStepsToDegrees(azimuthDataBits, AZIMUTH_GEARING_RATIO);
		elevationData = ConvertStepsToDegrees(elevationDataBits, ELEVATION_GEARING_RATIO);
		azimuthSpeed = ConvertStepsToDegrees(azimuthSpeedBits, AZIMUTH_GEARING_RATIO);
		elevationSpeed = ConvertStepsToDegrees(elevationSpeedBits, ELEVATION_GEARING_RATIO);
		
		// The azimuth and elevation data fields get modified as the simulation telescope moves.
		// Cache the number of steps to determine the initial input values of the command.
		// Don't cache the data for a stop command as to save the input for the command prior
		// to the stop.
		// This is only used to determine whether the acceleration or deceleration bits should be
		// set.
		if(!stop)
		{
			cachedAzData = azimuthData;
			cachedElData = elevationData;
		}
		
		// The acceleration that is received already has the gearing ratios applied, hence the gearing ratio of 1.
		azimuthAcceleration = ConvertStepsToDegrees(azimuthAccelerationBits, 1);
		elevationAcceleration = ConvertStepsToDegrees(elevationAccelerationBits, 1);
		azimuthDeceleration = ConvertStepsToDegrees(azimuthDecelerationBits, 1);
		elevationDeceleration = ConvertStepsToDegrees(elevationDecelerationBits, 1);
	}
	
	/// <summary>
	/// Helper method used to convert steps to degrees - this is taken from <c>ConversionHelper.cs</c> on the control room
	/// </summary>
	/// <param name="steps"> steps passed from the control room (or wherever)</param>
	/// <param name="gearingRatio"> the constant ratio associated with the type of movement. For us, this will be azimuth or elevation gearing </param>
	/// <returns> a float "degree" value from the information passed </returns>
	private float ConvertStepsToDegrees(int steps, float gearingRatio)
	{
		return steps * 360.0f / (STEPS_PER_REVOLUTION * gearingRatio);
	}
	
	/// <summary>
	/// Class helper method to compute the distance between two angles on a circle.
	/// </summary>
	private float AngleDistance(float a, float b)
	{
		// Mathf.Repeat is functionally similar to the modulus operator, but works with floats.
		return Mathf.Repeat((a - b + 180.0f), 360.0f) - 180.0f;
	}
}
