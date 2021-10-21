using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script controls the telescope according to the inputs from
// the control room as received by the MCUCommand updated by SimServer.
public class TelescopeControllerSim : MonoBehaviour
{
	// The game objects that get rotated by a movement command.
	public GameObject azimuth;
	public GameObject elevation;
	
	// The command that determines the telescope's movement.
	public MCUCommand command;
	
	// UI elements that get updated with the state of variables.
	public UIHandler ui;
	
	// The current values of the azimuth and elevation.
	private float simTelescopeAzimuthDegrees;
	private float simTelescopeElevationDegrees;
	
	// Whether the azimuth or elevation have reached their target
	// as defined by the MCUCommand.
	private bool azimuthMoving = false;
	private bool elevationMoving = false;
	
	// If the angle and target are within this distance, consider them equal.
	private float epsilon = 0.1f;
	
	// The max and min allowed angles for the elevation, expressed as the 
	// actual angle plus 15 degrees to convert the actual angle to the 
	// angle range that Unity uses.
	private float maxEl = 92.0f + 15.0f;
	private float minEl = -8.0f + 15.0f;
	
	/// <summary>
	/// Start is called before the first frame.
	/// </summary>
	public void Start()
	{
		// Set the current azimuth and elevation degrees to the rotation
		// of the game objects.
		simTelescopeAzimuthDegrees = azimuth.transform.eulerAngles.y;
		simTelescopeElevationDegrees = elevation.transform.eulerAngles.z;
		
		// Initialize the MCUCommand by targeting 0,0.
		ushort[] simStart = { (ushort)MoveType.SIM_TELESCOPECONTROLLER_INIT };
		command.UpdateCommand(simStart);
	}
	
	/// <summary>
	/// Update is called once per frame
	/// </summary>
	public void Update()
	{
		// Determine what the current command is and update the target orientation.
		HandleCommand();
		
		// Update the azimuth and elevation positions, if necessary.
		UpdateAzimuth();
		UpdateElevation();
	}
	
	/// <summary>
	/// Determine what the current command is and update the target orientation, if necessary.
	/// </summary>
	public void HandleCommand() 
	{
		// If an error has occurred, do nothing.
		if(command.errorFlag == true) 
			return;
		
		if(command.stopMove)
			HandleStop();
		else if(command.jog) 
			HandleJog();
		else
		{
			// This command was a relative move, which requires no special handling.
		}
		
		ui.InputAzimuth(command.azimuthDegrees);
		ui.InputElevation(command.elevationDegrees);
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
	/// Return true if the telescope orientation is at the homed position.
	/// For use in the UI.
	/// </summary>
	public bool Homed()
	{
		return !AzimuthMoving() && !ElevationMoving() && Azimuth() == 0.0f && Elevation() == 15.0f;
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
		return System.Math.Round(command.azimuthDegrees, 1);
	}
	
	/// <summary>
	/// Return the current elevation target angle truncated to a single decimal place.
	/// For use in the UI.
	/// </summary>
	public double TargetElevation()
	{
		return System.Math.Round(command.elevationDegrees - 15.0f, 1);
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
	/// Handle a stop command by setting the target orientation to the current orientation.
	/// </summary>
	private void HandleStop()
	{
		command.azimuthDegrees = simTelescopeAzimuthDegrees;
		command.elevationDegrees = simTelescopeElevationDegrees;
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
		
		command.azimuthDegrees = simTelescopeAzimuthDegrees + target * azJog;
		command.elevationDegrees = simTelescopeElevationDegrees + target * elJog;
	}
	
	/// <summary>
	/// Update the telescope azimuth.
	/// <summary>
	private void UpdateAzimuth()
	{
		float target = BoundAzimuth(command.azimuthDegrees);
		ref float current = ref simTelescopeAzimuthDegrees;
		// If the current azimuth does not equal the target azimuth, move toward the target.
		if(current != target)
		{
			float speed = command.azimuthSpeed;
			float distance = AngleDistance(target, current);
			current = ChangeAzimuth(current, target, distance < 0 ? -speed : speed);
			
			// If the azimuth and target are close, set the azimuth to the target.
			if(Mathf.Abs(AngleDistance(current, target)) < epsilon)
				current = target;
		}
		azimuthMoving = (current != target);
	}
	
	/// <summary>
	/// Update the telescope elevation.
	/// <summary>
	private void UpdateElevation()
	{
		float target = BoundElevation(command.elevationDegrees);
		ref float current = ref simTelescopeElevationDegrees;
		// If the current elevation does not equal the target elevation, move toward the target.
		if(current != target)
		{
			float speed = command.elevationSpeed;
			current = ChangeElevation(current, target, target < current ? -speed : speed);
			
			// If the elevation and target are close, set the elevation to the target.
			if(Mathf.Abs(AngleDistance(current, target)) < epsilon)
				current = target;
		}
		elevationMoving = (current != target);
	}

	/// <summary>
	/// Rotate the azimuth object.
	/// </summary>
	/// <param name="current">The current azimuth angle.</param>
	/// <param name="target">The target azimuth angle.</param>
	/// <param name="speed">The speed at which to rotate in degrees per second.</param>
	/// <returns>The new azimuth angle.</returns>
	private float ChangeAzimuth(float current, float target, float speed)
	{
		// Alter the movement speed by the time since the last frame. This ensures
		// a smooth movement regardless of the framerate.
		speed *= Time.deltaTime;
		
		// If we're closer to the target than the movement speed, lower the movement
		// speed so that we don't overshoot.
		// Unlike elevation, which doesn't wrap, azimuth needs to account for wrapping,
		// e.g. 350 and 10 are 20 degrees away, not 340, so use the AngleDistance.
		float distance = Mathf.Abs(AngleDistance(current, target));
		if(distance < Mathf.Abs(speed)) 
		 	speed = distance * (speed < 0 ? -1 : 1);
		
		azimuth.transform.Rotate(0, speed, 0);
		return BoundAzimuth(current + speed);
	}
	
	/// <summary>
	/// Rotate the elevation object.
	/// </summary>
	/// <param name="current">The current elevation angle.</param>
	/// <param name="target">The target elevation angle.</param>
	/// <param name="speed">The speed at which to rotate in degrees per second.</param>
	/// <returns>The new elevation angle.</returns>
	private float ChangeElevation(float current, float target, float speed)
	{
		// Alter the movement speed by the time since the last frame. This ensures
		// a smooth movement regardless of the framerate.
		speed *= Time.deltaTime;
		
		// If we're closer to the target than the movement speed, lower the movement
		// speed so that we don't overshoot.
		if(Mathf.Abs(target - current) < Mathf.Abs(speed))
			speed = target - current;
		
		elevation.transform.Rotate(0, 0, speed);
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
	/// Class helper method to compute the distance between two angles on a circle.
	/// </summary>
	private float AngleDistance(float a, float b)
	{
		// Mathf.Repeat is functionally similar to the modulus operator, but works with floats.
		return Mathf.Repeat((a - b + 180.0f), 360.0f) - 180.0f;
	}
}
