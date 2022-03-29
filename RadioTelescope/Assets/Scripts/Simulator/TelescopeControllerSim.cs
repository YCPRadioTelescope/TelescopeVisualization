using System;
using System.Collections;
using System.Collections.Generic;
using log4net;
using UnityEngine;
using static Utilities;

// This script controls the telescope according to the inputs from
// the control room as received by the MCUCommand updated by SimServer.
public class TelescopeControllerSim : MonoBehaviour
{
	// log4net logger.
	private static readonly ILog Log = LogManager.GetLogger(typeof(TelescopeControllerSim));
	
	// The game objects that get rotated by a movement command.
	public GameObject azimuthObject;
	public GameObject elevationObject;
	
	// The command that determines the telescope's movement.
	public MCUCommand command;
	
	// The object that updates the UI with the state of variables.
	public UIHandler ui;
	
	// Objects representing the state of each axis.
	private Axis azimuth;
	private Axis elevation;
	
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
		Log.Debug("NEW SIMULATION INSTANCE");
		Log.Debug("Initializing simulator.");
		
		// Create the azimuth and elevation Axis objects, which hold information
		// about the state of each axis.
		azimuth = new Axis(true, azimuthObject.transform.eulerAngles.y, command);
		elevation = new Axis(false, elevationObject.transform.eulerAngles.z, command);
		
		// Initialize the MCUCommand.
		command.InitSim();
	}
	
	/// <summary>
	/// Update is called once per frame.
	/// </summary>
	void Update()
	{
		// If this command should be ignored, do nothing. This can be
		// because the current command isn't handled by the simulation,
		// or because the MCUCommand is busy parsing a command and
		// we want to avoid executing partially parsed data.
		if(command.ignoreCommand)
			return;
		
		// Determine if the current command requires any special handling
		// and pass information about this command to the UI.
		HandleCommand();
		
		// Update the azimuth and elevation positions.
		UpdateAxis(MoveAzimuth, azimuth, ref command.azimuthData);
		UpdateAxis(MoveElevation, elevation, ref command.elevationData);
		
		// Update various booleans so that the SimServer knows which status
		// or error bits to set.
		HandleOutput();
	}
	
	/// <summary>
	/// Destroy is called when the scene ends (i.e. when the application is closed).
	/// </summary>
	void OnDestroy()
	{
		Log.Debug("END SIMULATION INSTANCE\n\n\n");
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
		return azimuth.degrees;
	}
	
	/// <summary>
	/// Return true if the azimuth motor is moving.
	/// </summary>
	public bool AzimuthMoving()
	{
		return azimuth.moving;
	}
	
	/// <summary>
	/// Return true if the azimuth motor is moving in the positive direction
	/// and false if it is moving in the negative direction.
	/// Always check AzimuthMoving before checking this function.
	/// </summary>
	public bool AzimuthPosMotion()
	{
		return azimuth.posMotion;
	}
	
	/// <summary>
	/// Return true if the azimuth motor has positive acceleration and
	/// false if it has negative acceleration (i.e. it is decelerating).
	/// Always check AzimuthMoving before checking this function.
	/// </summary>
	public bool AzimuthAccelerating()
	{
		return azimuth.accelerating;
	}
	
	/// <summary>
	/// Return true if the azimuth motor has negative acceleration (i.e. deceleration).
	/// Always check AzimuthMoving before checking this function.
	/// </summary>
	public bool AzimuthDecelerating()
	{
		return azimuth.decelerating;
	}
	
	/// <summary>
	/// Return true if the azimuth orientation is at the homed position.
	/// </summary>
	public bool AzimuthHomed()
	{
		return azimuth.homed;
	}
	
	/// <summary>
	/// Return the current elevation angle where negative values are below the horizon.
	/// </summary>
	public float Elevation()
	{
		return elevation.degrees;
	}
	
	/// <summary>
	/// Return true if the elevation motor is moving.
	/// </summary>
	public bool ElevationMoving()
	{
		return elevation.moving;
	}
	
	/// <summary>
	/// Return true if the elevation motor is moving in the positive direction
	/// and false if it is moving in the negative direction.
	/// Always check ElevationMoving before checking this function.
	/// </summary>
	public bool ElevationPosMotion()
	{
		return elevation.posMotion;
	}
	
	/// <summary>
	/// Return true if the elevation motor has positive acceleration.
	/// Always check ElevationMoving before checking this function.
	/// </summary>
	public bool ElevationAccelerating()
	{
		return elevation.accelerating;
	}
	
	/// <summary>
	/// Return true if the elevation motor has negative acceleration (i.e. deceleration).
	/// Always check ElevationMoving before checking this function.
	/// </summary>
	public bool ElevationDecelerating()
	{
		return elevation.decelerating;
	}
	
	/// <summary>
	/// Return true if the elevation orientation is at the homed position.
	/// </summary>
	public bool ElevationHomed()
	{
		return elevation.homed;
	}
	
	/// <summary>
	/// Return the angle of the azimuth object truncated to a single decimal place.
	/// For use in the UI.
	/// </summary>
	public double UnityAzimuth()
	{
		return System.Math.Round(azimuthObject.transform.eulerAngles.y, 1);
	}
	
	/// <summary>
	/// Return the angle of the elevation object truncated to a single decimal place.
	/// For use in the UI.
	/// </summary>
	public double UnityElevation()
	{
		return System.Math.Round(elevationObject.transform.eulerAngles.z, 1);
	}
	
	/// <summary>
	/// Return the current azimuth angle truncated to a single decimal place.
	/// For use in the UI.
	/// </summary>
	public double SimAzimuth()
	{
		return System.Math.Round(azimuth.degrees, 1);
	}
	
	/// <summary>
	/// Return the current elevation angle truncated to a single decimal place.
	/// For use in the UI.
	/// </summary>
	public double SimElevation()
	{
		return System.Math.Round(elevation.degrees - 15.0f, 1);
	}
	
	/// <summary>
	/// Return the current azimuth target angle truncated to a single decimal place.
	/// For use in the UI.
	/// </summary>
	public double TargetAzimuth()
	{
		return System.Math.Round(azimuth.degrees + command.azimuthData, 1);
	}
	
	/// <summary>
	/// Return the current elevation target angle truncated to a single decimal place.
	/// For use in the UI.
	/// </summary>
	public double TargetElevation()
	{
		return System.Math.Round(elevation.degrees + command.elevationData - 15.0f, 1);
	}
	
	/// <summary>
	/// Return the current azimuth speed truncated to a single decimal place.
	/// For use in the UI.
	/// </summary>
	public double AzimuthSpeed()
	{
		return System.Math.Round(azimuth.speed, 2);
	}
	
	/// <summary>
	/// Return the current elevation speed truncated to a single decimal place.
	/// For use in the UI.
	/// </summary>
	public double ElevationSpeed()
	{
		return System.Math.Round(elevation.speed, 2);
	}
	
	/// <summary>
	/// Determine if the current command requires any special handling
	/// and pass information about this command to the UI.
	/// </summary>
	private void HandleCommand() 
	{
		// Handle an immediate stop command by setting the axis speeds to 0.
		if(command.immediateStop)
		{
			azimuth.speed = 0.0f;
			elevation.speed = 0.0f;
		}
		// Handle a jog command by setting the target orientation 1 degree ahead
		// of the current orientation relative to the direction of the jog.
		// This causes the telescope to continually move in the direction
		// of the jog for as long as a jog command is received since
		// HandleCommand is called every frame.
		if(command.jog)
		{
			float azJog = command.azJog ? 1.0f : 0.0f;
			float elJog = command.azJog ? 0.0f : 1.0f;
			float target = command.posJog ? 1.0f : -1.0f;
			
			command.azimuthData = target * azJog;
			command.elevationData = target * elJog;
		}
		
		// Update the UI with the input azimuth and elevation.
		ui.InputAzimuth(command.azimuthData);
		ui.InputElevation(command.elevationData);
	}
	
	/// <summary>
	/// Determine if any special output needs tracked and update the necessary boolean
	/// values so that the SimServer can set the correct status or error bits.
	/// </summary>
	private void HandleOutput()
	{
		// Check if a limit switch has been hit. Log a warning if so.
		command.invalidInput = LimitSwitchHit();
		if(command.invalidInput)
			Log.Warn("A limit switch has been hit.");
		
		// If the current command is a relative move, record that so that the
		// move complete bit can be set.
		if(command.relativeMove)
			executingRelativeMove = true;
		// If the current command is not a relative move or a stop command, then
		// the move complete bit shouldn't be set.
		else if(!command.stop)
			executingRelativeMove = false;
		
		// If the current command is a home command and the axis has completed
		// its movement, then this axis is homed.
		if(command.home && command.azimuthData == 0.0f)
		{
			command.invalidAzimuthPosition = false;
			azimuth.homed = true;
		}
		// If the current command is not a home command and it moves this axis, 
		// then this axis is not homed.
		else if(!command.home && command.azimuthData != 0.0f)
			azimuth.homed = false;
		
		if(command.home && command.elevationData == 0.0f)
		{
			command.invalidElevationPosition = false;
			elevation.homed = true;
		}
		else if(!command.home && command.elevationData != 0.0f)
			elevation.homed = false;
		
	}
	
	/// <summary>
	/// Return true if the telescope elevation has hit a limit switch. This is true if the
	/// current elevation is beyond a limit value or at a limit value and it has a target
	/// to go even further beyond that.
	/// </summary>
	private bool LimitSwitchHit()
	{
		float current = elevation.degrees;
		float target = current + command.elevationData;
		return (current > maxEl || (current == maxEl && target > maxEl))
			|| (current < minEl || (current == minEl && target < minEl));
	}
	
	/// <summary>
	/// Update the given telescope axis.
	/// <summary>
	/// <param name="Move">The function that determines which axis GameObject moves.</param>
	/// <param name="axis">The current axis being acted upon.</param>
	/// <param name="moveBy">A reference to the current number of degrees to move by.</param>
	private void UpdateAxis(Func<float, float, float> Move, Axis axis, ref float moveBy)
	{
		// If a movement has been received then shift the current axis speed.
		// Also shift the current axis speed if we don't have a movement but
		// there is still some remaining speed.
		if(moveBy != 0.0f || axis.speed != 0.0f)
		{
			float progress = (moveBy != 0.0f && axis.Cached() != 0.0f) ? (1.0f - Mathf.Abs(moveBy) / Mathf.Abs(axis.Cached())) : 1.0f;
			ShiftSpeed(axis, moveBy, progress, axis.MaxSpeed(), axis.Acceleration(), axis.Deceleration());
		}
		
		// If the axis has momentum, move it.
		if(axis.speed != 0.0f)
		{
			// Get the current orientation.
			float old = axis.degrees;
			
			// Move the axis.
			axis.degrees = Move(axis.degrees, axis.speed);
			
			// Update the MCUCommand by subtracting the angle moved from the remaining degrees to move by.
			float moved = AngleDistance(axis.degrees, old);
			moveBy -= moved;
			// If this axis is the elevation and it didn't move despite the speed being non-zero,
			// that means that we've hit a limit switch and therefore should drop the speed to 0.
			if(axis.name == "elevation" && moved == 0.0f && (
				(WithinEpsilon(AngleDistance(axis.degrees, maxEl), epsilon) && axis.speed > 0.0f) ||
				(WithinEpsilon(AngleDistance(axis.degrees, minEl), epsilon) && axis.speed < 0.0f)))
			{
				axis.speed = 0.0f;
				moveBy = 0.0f;
			}
			
			// If the total degrees remaining to move by is less than the epsilon, consider it on target.
			if(moveBy != 0.0f && WithinEpsilon(moveBy, epsilon))
			{
				Log.Debug("Threw out the remaining " + moveBy + " degree " + axis.name + " movement because it was smaller than the accepted epsilon value of " + epsilon + ".");
				moveBy = 0.0f;
			}
		}
		
		// Update whether this axis is moving and its direction
		// of movement.
		axis.moving = (axis.speed != 0.0f);
		axis.posMotion = (axis.speed > 0.0f);
	}
	
	/// <summary>
	/// Shift the given speed up or down according to acceleration and deceleration values.
	/// Changes the given accelerating and decelerating booleans.
	/// </summary>
	/// <param name="axis">The current axis being acted upon.</param>
	/// <param name="remaining">The remaining number of degrees to move by for the current movement.</param>
	/// <param name="progress">A value from 0 to 1 denoting the % of the current movement that has been completed.</param>
	/// <param name="maxSpeed">The maximum allowed speed for the current movement.</param>
	/// <param name="accel">The acceleration rate of the speed.</param>
	/// <param name="decel">The deceleration rate of the speed.</param>
	private void ShiftSpeed(Axis axis, float remaining, float progress, float maxSpeed, float accel, float decel)
	{
		// Get the sign of the remaining movement then change the remaining movement to positive.
		float sign = (remaining > 0.0f) ? 1.0f : -1.0f;
		remaining = Mathf.Abs(remaining);
		
		// If we are jogging, always accelerate if able to. This is done by setting the progress to 0.
		if(command.jog)
			progress = 0.0f;
		// If we are stopping, always decelerate to zero. This is done by setting the progress to 1.
		if(command.stop)
		{
			progress = 1.0f;
			// Stop commands have no remaining movement distance, so get the sign from the
			// current speed.
			sign = (axis.speed > 0.0f) ? 1.0f : -1.0f;
			// Stop commands don't send a deceleration value, so just use the value
			// we normally receive.
			decel = 0.9f;
		}
		
		// Accelerate if we're in the first 50% of a movement.
		if(progress <= 0.5f)
		{
			axis.speed += sign * accel * Time.deltaTime;
			if(Mathf.Abs(axis.speed) >= Mathf.Abs(maxSpeed))
				axis.speed = sign * maxSpeed;
			
			axis.accelerating = (Mathf.Abs(axis.speed) != Mathf.Abs(maxSpeed));
			axis.decelerating = false;
		}
		// Don't accelerate if we're in the last 50% of a movement.
		else if(progress > 0.5f)
		{
			// Decelerate if the remaining movement is smaller than the stopping distance.
			if(progress == 1.0f || StoppingDistance(axis.speed, decel) >= remaining)
			{
				axis.speed -= sign * decel * Time.deltaTime;
				if((sign == 1.0f && axis.speed <= 0.0f) ||
						(sign == -1.0f && axis.speed >= 0.0f))
					axis.speed = 0.0f;
				
				axis.decelerating = (axis.speed != 0.0f);
			}
			axis.accelerating = false;
		}
	}
	
	/// <summary>
	/// Rotate the azimuth object.
	/// </summary>
	/// <param name="current">The current azimuth angle.</param>
	/// <param name="moveBy">The total degrees to move by before reaching the target azimuth.</param>
	/// <param name="speed">The speed at which to rotate in degrees per second.</param>
	/// <returns>The new azimuth angle.</returns>
	private float MoveAzimuth(float current, float speed)
	{
		// Alter the movement speed by the time since the last frame. This ensures
		// a smooth movement regardless of the framerate.
		speed *= Time.deltaTime;
		
		// Rotate the azimuth game object by the final speed.
		azimuthObject.transform.Rotate(0, speed, 0);
		
		// Return the new azimuth orientation, bounded within the range [0, 360).
		return BoundAzimuth(current + speed);
	}
	
	/// <summary>
	/// Rotate the elevation object.
	/// </summary>
	/// <param name="current">The current elevation angle.</param>
	/// <param name="moveBy">The total degrees to move by before reaching the target elevation.</param>
	/// <param name="speed">The speed at which to rotate in degrees per second.</param>
	/// <returns>The new elevation angle.</returns>
	private float MoveElevation(float current, float speed)
	{
		// Alter the movement speed by the time since the last frame. This ensures
		// a smooth movement regardless of the framerate.
		speed *= Time.deltaTime;
		
		// If we're closer to the target than the allowed bounds, lower the movement
		// speed so that we don't go out of bounds.
		float bounded = BoundElevation(elevationObject.transform.eulerAngles.z + speed, minEl, maxEl);
		if(bounded == minEl || bounded == maxEl)
			speed = bounded - current;
		
		// Rotate the elevation game object by the final speed.
		elevationObject.transform.Rotate(0, 0, speed);
		
		// Return the new elevation orientation, bounded within the range [minEl, maxEl].
		return BoundElevation(current + speed, minEl, maxEl);
	}
}
