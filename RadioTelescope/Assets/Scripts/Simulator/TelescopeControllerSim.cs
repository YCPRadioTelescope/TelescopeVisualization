using System;
using System.Collections;
using System.Collections.Generic;
using log4net;
using UnityEngine;

// This script controls the telescope according to the inputs from
// the control room as received by the MCUCommand updated by SimServer.
public class TelescopeControllerSim : MonoBehaviour
{
	// log4net logger.
	private static readonly ILog Log = LogManager.GetLogger(typeof(TelescopeControllerSim));
	
	// The game objects that get rotated by a movement command.
	public GameObject azimuth;
	public GameObject elevation;
	
	// The command that determines the telescope's movement.
	public MCUCommand command;
	
	// The object that updates the UI with the state of variables.
	public UIHandler ui;
	
	// The current values of the azimuth and elevation.
	private float simTelescopeAzimuthDegrees;
	private float simTelescopeElevationDegrees;
	
	// Whether the azimuth or elevation motors are moving,
	// the direction of travel (true = positive, false = negative),
	// and the acceleration direction.
	// Always check the moving bool before checking the motion or accelerating
	// bools.
	private bool azimuthMoving = false;
	private bool azimuthPosMotion = false;
	private bool azimuthAccelerating = false;
	private bool azimuthDecelerating = false;
	private bool azimuthHomed = false;
	
	private bool elevationMoving = false;
	private bool elevationPosMotion = false;
	private bool elevationAccelerating = false;
	private bool elevationDecelerating = false;
	private bool elevationHomed = false;
	
	private bool executingRelativeMove = false;
	
	// If the angle and target are within this distance, consider them equal.
	private float epsilon = 0.001f;
	
	// The max and min allowed angles for the elevation, expressed as the 
	// actual angle plus 15 degrees to convert the actual angle to the 
	// angle range that Unity uses.
	private float maxEl = 92.0f + 15.0f;
	private float minEl = -8.0f + 15.0f;
	
	/// <summary>
	/// Start is called before the first frame.
	/// </summary>
	void Start()
	{
		Log.Debug("Initializing simulator.");
		
		// Set the current azimuth and elevation degrees to the rotation
		// of the game objects.
		simTelescopeAzimuthDegrees = azimuth.transform.eulerAngles.y;
		simTelescopeElevationDegrees = elevation.transform.eulerAngles.z;
		
		// Initialize the MCUCommand.
		command.InitSim();
	}
	
	/// <summary>
	/// Update is called once per frame.
	/// </summary>
	void Update()
	{
		// Determine what the current command is.
		HandleCommand();
		
		// Update the azimuth and elevation positions.
		UpdateAzimuth();
		UpdateElevation();
		
		// Determine if any errors have occurred.
		HandleErrors();
		
		// Update any non-error output.
		HandleOutput();
	}
	
	/// <summary>
	/// Determine what the current command is and update necessary variables.
	/// </summary>
	public void HandleCommand() 
	{
		if(command.ignoreCommand == true) 
			return;
		
		if(command.jog) 
			HandleJog();
		
		// Update the UI with the input azimuth and elevation.
		ui.InputAzimuth(command.azimuthData);
		ui.InputElevation(command.elevationData);
	}
	
	/// <summary>
	/// Determine if any errors have occurred and update the necessary boolean values
	/// so that the SimServer can set the correct error bits.
	/// </summary>
	public void HandleErrors()
	{
		command.invalidInput = LimitSwitchHit();
		if(command.invalidInput)
			Log.Warn("A limit switch has been hit.");
	}
	
	/// <summary>
	/// Determine if any special output needs tracked and update the necessary boolean
	/// values so that the SimServer can set the correct error bits.
	/// </summary>
	public void HandleOutput()
	{
		// If the current command is a relative move, record that so that the
		// move complete bit can be set.
		if(command.relativeMove)
			executingRelativeMove = true;
		// If the current command is not a relative move or a stop command, then
		// the move complete bit shouldn't be set.
		else if(!command.stop)
			executingRelativeMove = false;
		
		// If the current command is a home command and the axis has stopped moving,
		// then this axis is homed.
		if(command.home && !AzimuthMoving())
		{
			command.invalidAzimuthPosition = false;
			azimuthHomed = true;
		}
		// If the current command is not a home command and it moves this axis, 
		// then this axis is not homed.
		else if(!command.home && AzimuthMoving())
			azimuthHomed = false;
		
		if(command.home && !ElevationMoving())
		{
			command.invalidElevationPosition = false;
			elevationHomed = true;
		}
		else if(!command.home && ElevationMoving())
			elevationHomed = false;
		
	}
	
	/// <summary>
	/// Return true if the telescope elevation has hit a limit switch. This is true if the
	/// current elevation is beyond a limit value or at a limit value and it has a target
	/// to go even further beyond that.
	/// </summary>
	public bool LimitSwitchHit()
	{
		float current = simTelescopeElevationDegrees;
		float target = current + command.elevationData;
		return (current > maxEl || (current == maxEl && target > maxEl))
			|| (current < minEl || (current == minEl && target < minEl));
	}
	
	/// <summary>
	/// Return true if a relative move was received.
	/// </summary>
	public bool RelativeMove()
	{
		return executingRelativeMove;
	}
	
	/// <summary>
	/// Return the current azimuth angle.
	/// </summary>
	public float Azimuth()
	{
		return simTelescopeAzimuthDegrees;
	}
	
	/// <summary>
	/// Return true if the azimuth motor is moving.
	/// </summary>
	public bool AzimuthMoving()
	{
		return azimuthMoving;
	}
	
	/// <summary>
	/// Return true if the azimuth motor is moving in the positive direction
	/// and false if it is moving in the negative direction.
	/// Always check AzimuthMoving before checking this function.
	/// </summary>
	public bool AzimuthPosMotion()
	{
		return azimuthPosMotion;
	}
	
	/// <summary>
	/// Return true if the azimuth motor has positive acceleration and
	/// false if it has negative acceleration (i.e. it is decelerating).
	/// Always check AzimuthMoving before checking this function.
	/// </summary>
	public bool AzimuthAccelerating()
	{
		return azimuthAccelerating;
	}
	
	/// <summary>
	/// Return true if the azimuth motor has negative acceleration (i.e. deceleration).
	/// Always check AzimuthMoving before checking this function.
	/// </summary>
	public bool AzimuthDecelerating()
	{
		return azimuthDecelerating;
	}
	
	/// <summary>
	/// Return true if the azimuth orientation is at the homed position.
	/// </summary>
	public bool AzimuthHomed()
	{
		return azimuthHomed;
	}
	
	/// <summary>
	/// Return the current elevation angle where negative values are below the horizon.
	/// </summary>
	public float Elevation()
	{
		return simTelescopeElevationDegrees;
	}
	
	/// <summary>
	/// Return true if the elevation motor is moving.
	/// </summary>
	public bool ElevationMoving()
	{
		return elevationMoving;
	}
	
	/// <summary>
	/// Return true if the elevation motor is moving in the positive direction
	/// and false if it is moving in the negative direction.
	/// Always check ElevationMoving before checking this function.
	/// </summary>
	public bool ElevationPosMotion()
	{
		return elevationPosMotion;
	}
	
	/// <summary>
	/// Return true if the elevation motor has positive acceleration.
	/// Always check ElevationMoving before checking this function.
	/// </summary>
	public bool ElevationAccelerating()
	{
		return elevationAccelerating;
	}
	
	/// <summary>
	/// Return true if the elevation motor has negative acceleration (i.e. deceleration).
	/// Always check ElevationMoving before checking this function.
	/// </summary>
	public bool ElevationDecelerating()
	{
		return elevationDecelerating;
	}
	
	/// <summary>
	/// Return true if the elevation orientation is at the homed position.
	/// </summary>
	public bool ElevationHomed()
	{
		return elevationHomed;
	}
	
	/// <summary>
	/// Return the angle of the azimuth object truncated to a single decimal place.
	/// For use in the UI.
	/// </summary>
	public double UnityAzimuth()
	{
		return System.Math.Round(azimuth.transform.eulerAngles.y, 1);
	}
	
	/// <summary>
	/// Return the angle of the elevation object truncated to a single decimal place.
	/// For use in the UI.
	/// </summary>
	public double UnityElevation()
	{
		return System.Math.Round(elevation.transform.eulerAngles.z, 1);
	}
	
	/// <summary>
	/// Return the current azimuth angle truncated to a single decimal place.
	/// For use in the UI.
	/// </summary>
	public double SimAzimuth()
	{
		return System.Math.Round(simTelescopeAzimuthDegrees, 1);
	}
	
	/// <summary>
	/// Return the current elevation angle truncated to a single decimal place.
	/// For use in the UI.
	/// </summary>
	public double SimElevation()
	{
		return System.Math.Round((simTelescopeElevationDegrees - 15.0f), 1);
	}
	
	/// <summary>
	/// Return the current azimuth target angle truncated to a single decimal place.
	/// For use in the UI.
	/// </summary>
	public double TargetAzimuth()
	{
		return System.Math.Round(simTelescopeAzimuthDegrees + command.azimuthData, 1);
	}
	
	/// <summary>
	/// Return the current elevation target angle truncated to a single decimal place.
	/// For use in the UI.
	/// </summary>
	public double TargetElevation()
	{
		return System.Math.Round(simTelescopeElevationDegrees + command.elevationData - 15.0f, 1);
	}
	
	/// <summary>
	/// Return the current azimuth speed truncated to a single decimal place.
	/// For use in the UI.
	/// </summary>
	public double AzimuthSpeed()
	{
		return System.Math.Round(command.azimuthSpeed, 2);
	}
	
	/// <summary>
	/// Return the current elevation speed truncated to a single decimal place.
	/// For use in the UI.
	/// </summary>
	public double ElevationSpeed()
	{
		return System.Math.Round(command.elevationSpeed, 2);
	}
	
	/// <summary>
	/// Handle a jog command by setting the target orientation 1 degree ahead of the current orientation,
	/// relative to the direction of the jog. This causes the telescope to continually move in the direction
	/// of the jog, since HandleJog is called every frame during a jog.
	/// </summary>
	private void HandleJog()
	{
		float azJog = command.azJog ? 1.0f : 0.0f;
		float elJog = command.azJog ? 0.0f : 1.0f;
		float target = command.posJog ? 1.0f : -1.0f;
		
		command.azimuthData = target * azJog;
		command.elevationData = target * elJog;
	}
	
	/// <summary>
	/// Update the telescope azimuth.
	/// <summary>
	private void UpdateAzimuth()
	{
		// If the amount of azimuth degrees to move by is non-zero, the azimuth must move.
		ref float moveBy = ref command.azimuthData;
		if(moveBy != 0.0f)
		{
			// Get the current orientation and movement speed
			ref float current = ref simTelescopeAzimuthDegrees;
			float old = current;
			float speed = command.azimuthSpeed;
			
			// Move the azimuth.
			current = ChangeAzimuth(current, moveBy, moveBy < 0.0f ? -speed : speed);
			
			// Update the MCUCommand by subtracting the angle moved from the remaining degrees to move by.
			moveBy -= AngleDistance(current, old);
			
			// If the total degrees remaining to move by is less than the epsilon, consider it on target.
			if(moveBy != 0.0f && WithinEpsilon(moveBy))
			{
				Log.Debug("Threw out the remaining " + moveBy + " degree azimuth movement because it was smaller than the accepted epsilon value of " + epsilon + ".");
				moveBy = 0.0f;
			}
		}
		azimuthMoving = (moveBy != 0.0f);
		azimuthPosMotion = (moveBy > 0.0f);
		azimuthAccelerating = (Mathf.Abs(moveBy) > (Mathf.Abs(command.cachedAzData) * 2.0f / 3.0f));
		azimuthDecelerating = (Mathf.Abs(moveBy) < (Mathf.Abs(command.cachedAzData) / 3.0f));
	}
	
	/// <summary>
	/// Update the telescope elevation.
	/// <summary>
	private void UpdateElevation()
	{
		ref float moveBy = ref command.elevationData;
		// If the current elevation does not equal the target elevation, move toward the target.
		if(moveBy != 0.0f)
		{
			// Get the current orientation and movement speed
			ref float current = ref simTelescopeElevationDegrees;
			float old = current;
			float speed = command.elevationSpeed;
			
			// Move the elevation.
			current = ChangeElevation(current, moveBy, moveBy < 0.0f ? -speed : speed);
			
			// Update the MCUCommand by subtracting the angle moved from the remaining degrees to move by.
			moveBy -= AngleDistance(current, old);
			
			// If the total degrees remaining to move by is less than the epsilon, consider it zero.
			if(moveBy != 0.0f && WithinEpsilon(moveBy))
			{
				Log.Debug("Threw out the remaining " + moveBy + " degree elevation movement because it was smaller than the accepted epsilon value of " + epsilon + ".");
				moveBy = 0.0f;
			}
		}
		elevationMoving = (moveBy != 0.0f);
		elevationPosMotion = (moveBy > 0.0f);
		elevationAccelerating = (Mathf.Abs(moveBy) > (Mathf.Abs(command.cachedAzData) * 2.0f / 3.0f));
		elevationDecelerating = (Mathf.Abs(moveBy) < (Mathf.Abs(command.cachedElData) / 3.0f));
	}

	/// <summary>
	/// Rotate the azimuth object.
	/// </summary>
	/// <param name="current">The current azimuth angle.</param>
	/// <param name="moveBy">The total degrees to move by before reaching the target azimuth.</param>
	/// <param name="speed">The max speed speed at which to rotate in degrees per second.</param>
	/// <returns>The new azimuth angle.</returns>
	private float ChangeAzimuth(float current, float moveBy, float speed)
	{
		// Alter the movement speed by the time since the last frame. This ensures
		// a smooth movement regardless of the framerate.
		speed *= Time.deltaTime;
		
		// If we're closer to the target than the movement speed, lower the movement
		// speed so that we don't overshoot it.
		if(Mathf.Abs(moveBy) < Mathf.Abs(speed)) 
		 	speed = moveBy;
		
		// Rotate the azimuth game object by the final speed.
		azimuth.transform.Rotate(0, speed, 0);
		
		// Return the new azimuth orientation, bounded within the range [0, 360).
		return BoundAzimuth(current + speed);
	}
	
	/// <summary>
	/// Rotate the elevation object.
	/// </summary>
	/// <param name="current">The current elevation angle.</param>
	/// <param name="moveBy">The total degrees to move by before reaching the target elevation.</param>
	/// <param name="speed">The max speed speed at which to rotate in degrees per second.</param>
	/// <returns>The new elevation angle.</returns>
	private float ChangeElevation(float current, float moveBy, float speed)
	{
		// Alter the movement speed by the time since the last frame. This ensures
		// a smooth movement regardless of the framerate.
		speed *= Time.deltaTime;
		
		// If we're closer to the target than the movement speed, lower the movement
		// speed so that we don't overshoot it.
		if(Mathf.Abs(moveBy) < Mathf.Abs(speed))
			speed = moveBy;
		
		// If we're closer to the target than the allowed bounds, lower the movement
		// speed so that we don't go out of bounds.
		float bounded = BoundElevation(elevation.transform.eulerAngles.z + speed);
		if(bounded == minEl || bounded == maxEl)
			speed = bounded - current;
		
		// Rotate the elevation game object by the final speed.
		elevation.transform.Rotate(0, 0, speed);
		
		// Return the new elevation orientation, bounded within the range [minEl, maxEl].
		return BoundElevation(current + speed);
	}
	
	/// <summary>
	/// Class helper method to handle rotating across 0 azimuth.
	/// </summary>
	private float BoundAzimuth(float az)
	{
		// All values that this function might encounter should be within the range
		// [-360,720). If it's outside this range then we could use while loops instead
		// of if statements to catch them, but if anything is outside that range then
		// something bad has happened and we want to know about that.
		if(az < 0.0f)
			az += 360.0f;
		if(az >= 360.0f)
			az -= 360.0f;
		return az;
	}
	
	/// <summary>
	/// Class helper method for bounding elevation within the limit switches.
	/// </summary>
	private float BoundElevation(float el)
	{
		if(el < minEl)
			return minEl;
		if(el > maxEl)
			return maxEl;
		return el;
	}
	
	
	/// <summary>
	/// Class helper method to determine if the magnitidue of the given angle is within
	/// the epsilon distance.
	/// </summary>
	private bool WithinEpsilon(float angle)
	{
		return Mathf.Abs(angle) < epsilon;
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
