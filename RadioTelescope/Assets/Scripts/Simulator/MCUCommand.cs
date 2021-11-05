using UnityEngine;
using UnityEngine.UI;
using System;

// master list of known register data
// https://docs.google.com/spreadsheets/d/1vBKsnV7Xso0u19ZyhtVimiCXpZjc007usVjvAsxHJNU/edit#gid=0

/// <summary> 
/// Public class <c>MCUCommand</c> is used to store input register data from the control room
/// in an easy way for us to decode the commands on the <c>TelescopeControllerSim</c>
/// </summary>
public class MCUCommand : MonoBehaviour 
{
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
	
	public bool jog = false;
	public bool posJog = false;
	public bool azJog = false;
	
	public bool ignoreCommand = false;
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
		Reset();
		currentCommand = "simulation initialization";
		azimuthSpeed = 20.0f;
		elevationSpeed = 20.0f;
		if(tc.Azimuth() < 180.0f)
			azimuthData = -tc.Azimuth();
		else
			azimuthData = 360.0f - tc.Azimuth();
		elevationData = -tc.Elevation() + 15.0f;
	}
	
	// Receive a test movement from the UI.
	public void TestMove(float azimuth, float elevation, float speed)
	{
		Reset();
		currentCommand = "simulation test movement";
		azimuthData = AngleDistance(azimuth, tc.Azimuth());
		elevationData = AngleDistance(elevation, tc.Elevation());
		azimuthSpeed = speed;
		elevationSpeed = speed;
	}
	
	/// <summary>
	/// Update the command with the register data received from the control room.
	/// </summary>
	/// <param name="registerData"> Raw register data from the modbus registers. </param>
	public void UpdateCommand(ushort[] registerData)
	{
		// Reset the state of the MCUCommand so that information doesn't carry over from the previous command.
		Reset();
		
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
		
		// NOTE FROM LUCAS: from my digging, the acceleration between the azimuth and elevation instructions is always the same
		// NOTE FROM JONATHAN: I'm creating a variable for both just in case this ever changes.
		azimuthAccelerationBits = registerData[(int)IncomingRegIndex.accelerationAzimuth];
		elevationAccelerationBits = registerData[(int)IncomingRegIndex.accelerationElevation];
		
		// NOTE FROM JONATHAN: The control room sets the same value for the acceleration and deceleration, but again, grab the
		// registers separately in case that ever change.
		azimuthDecelerationBits = registerData[(int)IncomingRegIndex.decelerationAzimuth];
		elevationDecelerationBits = registerData[(int)IncomingRegIndex.decelerationElevation];
		
		// Determine the incoming command and make any changes to the received information if necessary.
		// Relative move:
		if(firstCommandAzimuth == (ushort)CommandType.RELATIVE_MOVE)
		{
			currentCommand = "relative move";
			// The positive and negative directions on the hardware elevation motor are flipped
			// compared to what the simulation uses, so flip the recevied value.
			ConvertToDegrees();
			elevationData *= -1;
		}
		// Jogs:
		else if(firstCommandAzimuth == (ushort)CommandType.POSITIVE_JOG)
		{
			currentCommand = "positive azimuth jog";
			jog = true;
			azJog = true;
			posJog = true;
			ConvertToDegrees();
		}
		else if(firstCommandAzimuth == (ushort)CommandType.NEGATIVE_JOG)
		{
			currentCommand = "negative azimuth jog";
			jog = true;
			azJog = true;
			posJog = false;
			ConvertToDegrees();
		}
		else if(firstCommandElevation == (ushort)CommandType.POSITIVE_JOG)
		{
			currentCommand = "positive elevation jog";
			jog = true;
			azJog = false;
			posJog = true;
			ConvertToDegrees();
		}
		else if(firstCommandElevation == (ushort)CommandType.NEGATIVE_JOG)
		{
			currentCommand = "negative elevation jog";
			jog = true;
			azJog = false;
			posJog = false;
			ConvertToDegrees();
		}
		// Homing:
		// Clockwise and counterclockwise homes are currently not handled differently from one another.
		else if(firstCommandAzimuth == (ushort)CommandType.CLOCKWISE_HOME
				|| firstCommandAzimuth == (ushort)CommandType.COUNTERCLOCKWISE_HOME)
		{
			currentCommand = "home";
			ConvertToDegrees();
			if(tc.Azimuth() < 180.0f)
				azimuthData = -tc.Azimuth();
			else
				azimuthData = 360.0f - tc.Azimuth();
			elevationData = -tc.Elevation() + 15.0f;
		}
		// MCU related:
		// TODO FROM LUCAS: write back proper registers
		else if(firstCommandAzimuth == (ushort)CommandType.CONFIGURE_MCU)
		{
			currentCommand = "congifure MCU";
			ignoreCommand = true;
		}
		else if(firstCommandAzimuth == (ushort)CommandType.CLEAR_MCU_ERRORS)
		{
			currentCommand = "clear MCU errors";
			ClearMCUErrors();
		}
		// Stops:
		else if(firstCommandAzimuth == (ushort)CommandType.CONTROLLED_STOP)
		{
			currentCommand = "controlled stop";
		}
		else if(firstCommandAzimuth == (ushort)CommandType.IMMEDIATE_STOP)
		{
			currentCommand = "immediate stop";
		}
		else if(secondCommandElevation == (ushort)CommandType.CANCEL_MOVE)
		{
			currentCommand = "cancel move";
		}
		else
		{
			currentCommand = "unknown command";
			ignoreCommand = true;
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
		
		jog = false;
		posJog = false;
		azJog = false;
		
		ignoreCommand = false;
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
	
	private void ClearMCUErrors()
	{
		invalidInput = false;
	}
	
	/// <summary>
	/// Helper method used to convert raw hex register values to floats used by the controller.
	/// </summary>
	private void ConvertToDegrees() 
	{
		// Convert all step values to floats in degrees.
		// NOTE FROM LUCAS: this process will most likely change when we want to make the process interruptable, so instead of an absolute conversion
		// something like (# of steps for 1 degree) -- future work
		azimuthData = ConvertStepsToDegrees(azimuthDataBits, AZIMUTH_GEARING_RATIO);
		elevationData = ConvertStepsToDegrees(elevationDataBits, ELEVATION_GEARING_RATIO);
		azimuthSpeed = ConvertStepsToDegrees(azimuthSpeedBits, AZIMUTH_GEARING_RATIO);
		elevationSpeed = ConvertStepsToDegrees(elevationSpeedBits, ELEVATION_GEARING_RATIO);
		// TODO: Determine if the acceleration values should use the gearing ratios.
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
