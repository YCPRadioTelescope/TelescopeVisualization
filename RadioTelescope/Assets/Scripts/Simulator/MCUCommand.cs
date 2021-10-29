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
	///
	/// static constants (from the control room, not eyeballed for unity's sake)
	///
	private const float STEPS_PER_REVOLUTION = 20000.0f;
	private const float AZIMUTH_GEARING_RATIO = 500.0f;
	private const float ELEVATION_GEARING_RATIO = 50.0f;
	
	///
	/// member fields
	///
	public float azimuthDegrees = 0.0f;
	public float elevationDegrees = 0.0f;
	public float azimuthSpeed = 0.0f;
	public float elevationSpeed = 0.0f;
	public float acceleration = 0.0f;
	public float deceleration = 0.0f;
	public bool jog = false;
	public bool posJog = false;
	public bool azJog = false;
	public bool errorFlag = false;
	public bool stopMove = false;
	
	/// <summary>
	/// constructor for building mcu command objects
	/// </summary>
	/// <param name="registerData"> raw register data from the control room </param>
	/// <param name="simAzimuthDegrees"> helper param to calculate how far we need to go for a relative move </param>
	public void UpdateCommand(ushort[] registerData, float simAzimuthDegrees = 0.0f, float simElevationDegrees = 0.0f)
	{
		Reset();
		// we can determine move type by looking at the first register value
		// i chose to do the first azimuth word because the average move type (relative) starts this way
		// other edge cases are handled by switching on a 0 value (instruction not for the azimuth motor)
		switch(registerData[(int) RegPos.firstWordAzimuth])
		{ 
			case (ushort)MoveType.RELATIVE_MOVE:
				Debug.Log("RELATIVE MOVE INCOMING");
				
				// calculate speed fields
				azimuthSpeed = ((registerData[(int)RegPos.firstSpeedAzimuth] << 16) 
									+ registerData[(int)RegPos.secondSpeedAzimuth]);
				
				elevationSpeed = ((registerData[(int)RegPos.firstSpeedElevation] << 16) 
									+ registerData[(int)RegPos.secondSpeedElevation]);
				
				// grab acceleration (we set registers 6 & 7 on the control room side, but the previous team only grabbed 6 so only 6 here)
				// NOTE: the sim does not account for acceleration (I don't think we need to) but if you wanted too, you most likely
				// would need to combine registers 6 & 7 to get the actual value
				// NOTE 2: from my digging, the acceleration between the azimuth and elevation instructions is always the same
				acceleration = registerData[(int)RegPos.accelerationAzimuth];
				deceleration = registerData[(int)RegPos.decelerationAzimuth];
				
				// calculate azimuth and elevation steps (this is set on registers 3 & 4 for azimuth and 12 & 13 for elevation)
				// note the var is called *azimuthDegrees* and *elevationDegrees* but right now these are in steps. They get converted below
				azimuthDegrees = (registerData[(int)RegPos.firstPosAzimuth] << 16) + registerData[(int)RegPos.secondPosAzimuth];
				elevationDegrees = (registerData[(int)RegPos.firstPosElevation] << 16) + registerData[(int)RegPos.secondPosElevation];
				
				// convert raw register values into simulator friendly terms
				ConvertToDegrees();
				
				// the CR flips the elevation for some reason, so we will flip it back
				elevationDegrees *= -1;
				break;
			
			case (ushort)MoveType.CLOCKWISE_AZIMTUH_JOG:
				Debug.Log("AZIMTUH JOG LEFT COMMAND INCOMING");
				jog = true;
				azJog = true;
				posJog = false;
				azimuthSpeed = ((registerData[(int)RegPos.firstSpeedAzimuth] << 16) 
									+ registerData[(int)RegPos.secondSpeedAzimuth]);
				
				// convert raw register values into simulator friendly terms
				ConvertToDegrees();
				break;
			
			case (ushort)MoveType.COUNTERCLOCKWISE_AZIMUTH_JOG:
				Debug.Log("AZIMTUH JOG RIGHT COMMAND INCOMING");
				jog = true;
				azJog = true;
				posJog = true;
				azimuthSpeed = ((registerData[(int)RegPos.firstSpeedAzimuth] << 16) 
									+ registerData[(int)RegPos.secondSpeedAzimuth]);
				
				// convert raw register values into simulator friendly terms
				ConvertToDegrees();
				break;
			
			case (ushort)MoveType.CLOCKWISE_HOME:
			case (ushort)MoveType.COUNTERCLOCKWISE_HOME:
				Debug.Log("HOME COMMAND INCOMING");
				// for this move we just want to 0 the telescope, nothing fancy
				// we do want a value for the speed in case we need to move to the 0 position
				// TODO: grab the actual speed from the CR
				// it comes in on registerData[5] (az) and registerData[15] (el)
				azimuthSpeed = 5.0f;
				elevationSpeed = 5.0f;
				acceleration = 0.0f;
				if(simAzimuthDegrees < 180.0f)
					azimuthDegrees = -simAzimuthDegrees;
				else
					azimuthDegrees = 360.0f - simAzimuthDegrees;
				// to match the unity sim up with the CR we need to move to 15 instead of 0
				// we are always 15 off 
				elevationDegrees = -simElevationDegrees + 15.0f;
				break;
			
			case 0x0000: // COULD BE A BUNCH OF THINGS -- a lot of register bits start with 0 because they are for elevation only or are some sort of stop move
				// first check to see if it's an elevation jog command
				if(registerData[(int)RegPos.firstWordElevation] == (ushort)MoveType.NEGATIVE_ELEVATION_JOG) 
				{
					Debug.Log("NEGATIVE ELEVATION JOG COMMAND INCOMING");
					jog = true;
					azJog = false;
					posJog = false;
					elevationSpeed = ((registerData[(int)RegPos.firstSpeedElevation] << 16) 
										+ registerData[(int)RegPos.secondSpeedElevation]);
					
					// convert raw register values into simulator friendly terms
					ConvertToDegrees();
					break;
				}
				else if(registerData[(int)RegPos.firstWordElevation] == (ushort)MoveType.POSITIVE_ELEVATION_JOG)
				{
					Debug.Log("POSITIVE ELEVATION JOG COMMAND INCOMING");
					jog = true;
					azJog = false;
					posJog = true;
					elevationSpeed = ((registerData[(int) RegPos.firstSpeedElevation] << 16) 
										+ registerData[(int) RegPos.secondSpeedElevation]);
					
					// convert raw register values into simulator friendly terms
					ConvertToDegrees();
					break;
				}
				
				// Cancel move also starts with a 0x0000, but it is deliminated by the second register (a 3)
				if(registerData[(int)RegPos.secondWordAzimuth] == (ushort)MoveType.CANCEL_MOVE)
				{
					Debug.Log("CANCEL MOVE INCOMING");
					// set error flag so TelescopeController doesn't do anything with the currentCommand's fields
					stopMove = true;
					break;
				}
				
				Debug.Log("MCUCOMMAND: !!ERROR!! We fell through the 0x000 case and did not match any conditions.");
				errorFlag = true;
				break;
			
			case (ushort)MoveType.CONTROLLED_STOP:
			case (ushort)MoveType.IMMEDIATE_STOP:
				Debug.Log("STOP MOVE INCOMING");
				// Nothing else must be done.
				break;
			
			// TODO: clear proper registers
			case (ushort)MoveType.CLEAR_MCU_ERRORS:
				Debug.Log("CLEAR MCU ERRORS COMMAND INCOMING");
				// this case will get more love later, for now just set errorFlag (don't do anything with this MCUCommand object)
				errorFlag = true;
				break;
			
			// TODO: write back proper registers
			case (ushort)MoveType.CONFIGURE_MCU:
				Debug.Log("CONFIGURE MCU COMMAND INCOMING");
				// we don't need to do anything with this command, so we're just going to set the errorFlag so this command is ignored
				errorFlag = true;
				break;
			
			case (ushort)MoveType.SIM_TELESCOPECONTROLLER_INIT:
				Debug.Log("Building MCUCommand for telescope controller to put in start position");
				azimuthSpeed = 20.0f;
				elevationSpeed = 20.0f;
				acceleration = 50.0f;
				if(simAzimuthDegrees < 180.0f)
					azimuthDegrees = -simAzimuthDegrees;
				else
					azimuthDegrees = 360.0f - simAzimuthDegrees;
				elevationDegrees = -simElevationDegrees + 15.0f;
				break;
			
			case (ushort)MoveType.TEST_MOVE:
				// this is for the TestMove routine to arbitrarily move the telescope from within unity
				Debug.Log("Buidling MCUCommand for TestMove.cs");
				
				// we can't use ConvertToDegrees() here because the values here are already in degrees
				// NOTE: these indexes do not line up with the enum since we set these ourselves in TestMove.cs
				azimuthDegrees = AngleDistance(Convert.ToInt32(registerData[1]), simAzimuthDegrees);
				elevationDegrees = AngleDistance(Convert.ToInt32(registerData[2]), simElevationDegrees);
				azimuthSpeed = Convert.ToInt32(registerData[3]);
				elevationSpeed = Convert.ToInt32(registerData[3]);
				break;
			
			default: // catch "all" and return error command
				Debug.Log("!!! ERROR !!! MCUCommand Constructor: Cannot determine a move type from control room. Setting error flag to true and everything else to 0.0f.");
				errorFlag = true;
				break;
		}
	}
	
	private void Reset()
	{
		azimuthDegrees = 0.0f;
		elevationDegrees = 0.0f;
		azimuthSpeed = 0.0f;
		elevationSpeed = 0.0f;
		acceleration = 0.0f;
		deceleration = 0.0f;
		jog = false;
		posJog = false;
		azJog = false;
		errorFlag = false;
		stopMove = false;
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
