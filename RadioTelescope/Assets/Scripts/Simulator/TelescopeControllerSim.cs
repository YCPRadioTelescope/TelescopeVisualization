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
	public GameObject azimuth;
	public GameObject elevation;
	
	// The command that determines the telescope's movement.
	public MCUCommand command;
	
	// The object that updates the UI with the state of variables.
	public UIHandler ui;
	
	// The current values of the azimuth and elevation in degrees.
	private float simTelescopeAzimuthDegrees;
	private float simTelescopeElevationDegrees;
	
	// The current azimuth and elevation speeds in degrees per second.
	private float azSpeed = 0.0f;
	private float elSpeed = 0.0f;
	
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
		Log.Debug("NEW SIMULATION INSTANCE");
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
		if(command.ignoreCommand)
			return;
		
		// Determine what the current command is.
		HandleCommand();
		
		// Update the azimuth and elevation positions.
		// This is a mess of parameters being passed, but it cuts down on a lot of code duplications
		// given that moving the azimuth or elevation is just a difference of the variables to use.
		// Could perhaps cut down on the number of parameters being passed by creating an object
		// that contains all the axis information. Create an "Axis" object?
		UpdateAxis(MoveAzimuth, ref simTelescopeAzimuthDegrees, ref command.azimuthData,
			ref azSpeed, ref azimuthAccelerating, ref azimuthDecelerating, command.cachedAzData,
			command.azimuthSpeed, command.azimuthAcceleration, command.azimuthDeceleration,
			ref azimuthMoving, ref azimuthPosMotion, "azimuth");
		UpdateAxis(MoveElevation, ref simTelescopeElevationDegrees, ref command.elevationData,
			ref elSpeed, ref elevationAccelerating, ref elevationDecelerating, command.cachedElData,
			command.elevationSpeed, command.elevationAcceleration, command.elevationDeceleration,
			ref elevationMoving, ref elevationPosMotion, "elevation");
		
		// Determine if any errors have occurred.
		HandleErrors();
		
		// Update any non-error output.
		HandleOutput();
	}
	
	/// <summary>
	/// Update is called when the scene ends.
	/// </summary>
	void OnDestroy()
	{
		Log.Debug("END SIMULATION INSTANCE\n\n\n");
	}
	
	/// <summary>
	/// Determine what the current command is and update necessary variables.
	/// </summary>
	public void HandleCommand() 
	{
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
		return System.Math.Round(azSpeed, 2);
	}
	
	/// <summary>
	/// Return the current elevation speed truncated to a single decimal place.
	/// For use in the UI.
	/// </summary>
	public double ElevationSpeed()
	{
		return System.Math.Round(elSpeed, 2);
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
	/// Update the given telescope axis.
	/// <summary>
	/// <param name="Move">The function that determines which axis gets moved.</param>
	/// <param name="current">A reference to the current axis orientation.</param>
	/// <param name="moveBy">A reference to the current number of degrees to move by.</param>
	/// <param name="speed">A reference to the current axis speed.</param>
	/// <param name="accelerating">A reference to the azimuth or elevation accelerating bool.</param>
	/// <param name="decelerating">A reference to the azimuth or elevation decelerating bool.</param>
	/// <param name="cached">The cached axis movement distance.</param>
	/// <param name="maxSpeed">The maximum allowed speed for the current movement.</param>
	/// <param name="accel">The acceleration rate of the speed.</param>
	/// <param name="decel">The deceleration rate of the speed.</param>
	/// <param name="moving">A reference to the azimuth or elevation moving bool.</param>
	/// <param name="posMotion">A reference to the azimuth or elevation posMotion bool.</param>
	/// <param name="axis">A string of the name of the axis being moved.</param>
	private void UpdateAxis(Func<float, float, float> Move, ref float current, ref float moveBy, ref float speed,
		ref bool accelerating, ref bool decelerating, float cached, float maxSpeed, float acceleration, float deceleration,
		ref bool moving, ref bool posMotion, string axis)
	{
		// If a movement has been received then shift the current axis speed.
		// Also shift the current axis speed if we don't have a movement but
		// there is still some remaining speed.
		if(moveBy != 0.0f || speed != 0.0f)
		{
			float progress = (moveBy != 0.0f && cached != 0.0f) ? (1.0f - Mathf.Abs(moveBy) / Mathf.Abs(cached)) : 1.0f;
			ShiftSpeed(ref speed, ref accelerating, ref decelerating,
				moveBy, progress, maxSpeed, acceleration, deceleration);
		}
		
		// If the axis has momentum, move it.
		if(speed != 0.0f)
		{
			// Get the current orientation.
			float old = current;
			
			// Move the axis.
			current = Move(current, speed);
			
			// Update the MCUCommand by subtracting the angle moved from the remaining degrees to move by.
			float moved = AngleDistance(current, old);
			moveBy -= moved;
			// If this axis is the elevation and it didn't move despite the speed being non-zero,
			// that means that we've hit a limit switch and therefore should drop the speed to 0.
			if(axis == "elevation" && moved == 0.0f && (
				(WithinEpsilon(AngleDistance(current, maxEl), epsilon) && speed > 0.0f) ||
				(WithinEpsilon(AngleDistance(current, minEl), epsilon) && speed < 0.0f)))
			{
				speed = 0.0f;
				moveBy = 0.0f;
			}
			
			// If the total degrees remaining to move by is less than the epsilon, consider it on target.
			if(moveBy != 0.0f && WithinEpsilon(moveBy, epsilon))
			{
				Log.Debug("Threw out the remaining " + moveBy + " degree " + axis + " movement because it was smaller than the accepted epsilon value of " + epsilon + ".");
				moveBy = 0.0f;
			}
		}
		
		// Update whether this axis is moving and its direction
		// of movement.
		moving = (speed != 0.0f);
		posMotion = (speed > 0.0f);
	}
	
	/// <summary>
	/// Shift the given speed up or down according to acceleration and deceleration values.
	/// Changes the given accelerating and decelerating booleans.
	/// </summary>
	/// <param name="speed">A reference to the azimuth or elevation speed.</param>
	/// <param name="accelerating">A reference to the azimuth or elevation accelerating bool.</param>
	/// <param name="decelerating">A reference to the azimuth or elevation decelerating bool.</param>
	/// <param name="remaining">The remaining number of degrees to move by for the current movement.</param>
	/// <param name="progress">A value from 0 to 1 denoting the % of the current movement that has been completed.</param>
	/// <param name="maxSpeed">The maximum allowed speed for the current movement.</param>
	/// <param name="accel">The acceleration rate of the speed.</param>
	/// <param name="decel">The deceleration rate of the speed.</param>
	private void ShiftSpeed(ref float speed, ref bool accelerating, ref bool decelerating,
		float remaining, float progress, float maxSpeed, float accel, float decel)
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
			sign = (speed > 0.0f) ? 1.0f : -1.0f;
			// Stop commands don't send a deceleration value, so just use the value
			// we normally receive.
			decel = 0.9f;
		}
		
		// Accelerate if we're in the first 50% of a movement.
		if(progress <= 0.5f)
		{
			speed += sign * accel * Time.deltaTime;
			if(Mathf.Abs(speed) >= Mathf.Abs(maxSpeed))
				speed = sign * maxSpeed;
			
			accelerating = (Mathf.Abs(speed) != Mathf.Abs(maxSpeed));
			decelerating = false;
		}
		// Don't accelerate if we're in the last 50% of a movement.
		else if(progress > 0.5f)
		{
			// Decelerate if the remaining movement is smaller than the stopping distance.
			if(progress == 1.0f || StoppingDistance(speed, decel) >= remaining)
			{
				speed -= sign * decel * Time.deltaTime;
				if((sign == 1.0f && speed <= 0.0f) ||
						(sign == -1.0f && speed >= 0.0f))
					speed = 0.0f;
				
				decelerating = (speed != 0.0f);
			}
			accelerating = false;
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
		azimuth.transform.Rotate(0, speed, 0);
		
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
		float bounded = BoundElevation(elevation.transform.eulerAngles.z + speed, minEl, maxEl);
		if(bounded == minEl || bounded == maxEl)
			speed = bounded - current;
		
		// Rotate the elevation game object by the final speed.
		elevation.transform.Rotate(0, 0, speed);
		
		// Return the new elevation orientation, bounded within the range [minEl, maxEl].
		return BoundElevation(current + speed, minEl, maxEl);
	}
}
