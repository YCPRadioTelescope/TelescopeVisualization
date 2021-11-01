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
	
	///
	/// member fields
	///
	public string currentCommand = "";
	public float azimuthDegrees = 0.0f;
	public float elevationDegrees = 0.0f;
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
	
	///
	/// static constants (from the control room, not eyeballed for unity's sake)
	///
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
			azimuthDegrees = -tc.Azimuth();
		else
			azimuthDegrees = 360.0f - tc.Azimuth();
		elevationDegrees = -tc.Elevation() + 15.0f;
	}
	
	// Receive a test movement from the UI.
	public void TestMove(float azimuth, float elevation, float speed)
	{
		Reset();
		currentCommand = "simulation test movement";
		azimuthDegrees = AngleDistance(azimuth, tc.Azimuth());
		elevationDegrees = AngleDistance(elevation, tc.Elevation());
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
		ushort firstWordAzimuth = registerData[(int)RegPos.firstWordAzimuth];
		ushort secondWordAzimuth = registerData[(int)RegPos.secondWordAzimuth];
		ushort firstWordElevation = registerData[(int)RegPos.firstWordElevation];
		ushort secondWordElevation = registerData[(int)RegPos.secondWordElevation];
		
		// Grab the information from the other registers. All values received from the control room are in steps and will
		// later be converted to degrees as necessary.
		azimuthDegrees = (registerData[(int)RegPos.firstPosAzimuth] << 16) + registerData[(int)RegPos.secondPosAzimuth];
		elevationDegrees = (registerData[(int)RegPos.firstPosElevation] << 16) + registerData[(int)RegPos.secondPosElevation];
		
		azimuthSpeed = (registerData[(int)RegPos.firstSpeedAzimuth] << 16) + registerData[(int)RegPos.secondSpeedAzimuth];
		elevationSpeed = (registerData[(int)RegPos.firstSpeedElevation] << 16) + registerData[(int)RegPos.secondSpeedElevation];
		
		// NOTE FROM LUCAS: from my digging, the acceleration between the azimuth and elevation instructions is always the same
		azimuthAcceleration = registerData[(int)RegPos.accelerationAzimuth];
		elevationAcceleration = registerData[(int)RegPos.accelerationElevation];
		
		azimuthDeceleration = registerData[(int)RegPos.decelerationAzimuth];
		elevationDeceleration = registerData[(int)RegPos.decelerationElevation];
		
		// Determine the incoming command and make any changes to the received information if necessary.
		// Relative move:
		if(firstWordAzimuth == (ushort)MoveType.RELATIVE_MOVE)
		{
			currentCommand = "relative move";
			// The positive and negative directions on the hardware elevation motor are flipped
			// compared to what the simulation uses, so flip the recevied value.
			elevationDegrees *= -1;
			ConvertToDegrees();
		}
		// Jogs:
		else if(firstWordAzimuth == (ushort)MoveType.COUNTERCLOCKWISE_AZIMUTH_JOG)
		{
			currentCommand = "positive azimuth jog";
			jog = true;
			azJog = true;
			posJog = true;
			ConvertToDegrees();
		}
		else if(firstWordAzimuth == (ushort)MoveType.CLOCKWISE_AZIMTUH_JOG)
		{
			currentCommand = "negative azimuth jog";
			jog = true;
			azJog = true;
			posJog = false;
			ConvertToDegrees();
		}
		else if(firstWordElevation == (ushort)MoveType.POSITIVE_ELEVATION_JOG)
		{
			currentCommand = "positive elevation jog";
			jog = true;
			azJog = false;
			posJog = true;
			ConvertToDegrees();
		}
		else if(firstWordElevation == (ushort)MoveType.NEGATIVE_ELEVATION_JOG)
		{
			currentCommand = "negative elevation jog";
			jog = true;
			azJog = false;
			posJog = false;
			ConvertToDegrees();
		}
		// Homing:
		// Clockwise and counterclockwise homes are currently not handled differently from one another.
		else if(firstWordAzimuth == (ushort)MoveType.CLOCKWISE_HOME
				|| firstWordAzimuth == (ushort)MoveType.COUNTERCLOCKWISE_HOME)
		{
			currentCommand = "home";
			ConvertToDegrees();
			if(tc.Azimuth() < 180.0f)
				azimuthDegrees = -tc.Azimuth();
			else
				azimuthDegrees = 360.0f - tc.Azimuth();
			elevationDegrees = -tc.Elevation() + 15.0f;
		}
		// Stops:
		else if(secondWordAzimuth == (ushort)MoveType.CANCEL_MOVE)
		{
			currentCommand = "cancel move";
		}
		else if(firstWordAzimuth == (ushort)MoveType.CONTROLLED_STOP)
		{
			currentCommand = "controlled stop";
		}
		else if(firstWordAzimuth == (ushort)MoveType.IMMEDIATE_STOP)
		{
			currentCommand = "immediate stop";
		}
		// MCU related:
		// TODO FROM LUCAS: write back proper registers
		else if(firstWordAzimuth == (ushort)MoveType.CONFIGURE_MCU)
		{
			currentCommand = "congifure MCU";
			ignoreCommand = true;
		}
		// TODO FROM LUCAS: clear proper registers
		else if(firstWordAzimuth == (ushort)MoveType.CLEAR_MCU_ERRORS)
		{
			// NOTE FROM LUCAS: this case will get more love later, for now just set
			// errorFlag (don't do anything with this MCUCommand object)
			currentCommand = "clear MCU errors";
			ignoreCommand = true;
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
		azimuthDegrees = 0.0f;
		elevationDegrees = 0.0f;
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
	}
	
	/// <summary>
	/// Helper method used to convert raw hex register values to workable values we can use on the controller side 
	/// </summary>
	private void ConvertToDegrees() 
	{
		// get everything to floats
		Convert.ToSingle(azimuthSpeed);
		Convert.ToSingle(elevationSpeed);
		Convert.ToSingle(azimuthDegrees);
		Convert.ToSingle(elevationDegrees);
		
		// next convert azimuth and elevation steps to degrees
		// this process will most likely change when we want to make the process interruptable, so instead of an absolute conversion
		// something like (# of steps for 1 degree) -- future work
		azimuthDegrees = ConvertStepsToDegrees(azimuthDegrees, AZIMUTH_GEARING_RATIO);
		elevationDegrees = ConvertStepsToDegrees(elevationDegrees, ELEVATION_GEARING_RATIO);
		azimuthSpeed = ConvertStepsToDegrees(azimuthSpeed, AZIMUTH_GEARING_RATIO);
		elevationSpeed = ConvertStepsToDegrees(elevationSpeed, ELEVATION_GEARING_RATIO);
	}
	
	/// <summary>
	/// Helper method used to convert steps to degrees - this is taken from <c>ConversionHelper.cs</c> on the control room
	/// </summary>
	/// <param name="steps"> steps passed from the control room (or wherever)</param>
	/// <param name="gearingRatio"> the constant ratio associated with the type of movement. For us, this will be azimuth or elevation gearing </param>
	/// <returns> a float "degree" value from the information passed </returns>
	private float ConvertStepsToDegrees(float steps, float gearingRatio)
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
