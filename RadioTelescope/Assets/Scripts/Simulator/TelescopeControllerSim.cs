using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script controls the telescope according to the inputs from
// the control room as received by the MCUCommand created by SimServer.
public class TelescopeControllerSim : MonoBehaviour
{
	// The game objects that get rotated by a movement command.
	public GameObject azimuth;
	public GameObject elevation;
	
	// UI elements that get updated with the state of variables.
	public UIHandler ui;
	
	// The MCUCommand object that determines the target orientation.
	public MCUCommand command;
	
	// The current values of the azimuth and elevation.
	private float simTelescopeAzimuthDegrees;
	private float simTelescopeElevationDegrees;
	
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
		
		// Create a dummy MCU command and target 0,0.
		ushort[] simStart = { (ushort)MoveType.SIM_TELESCOPECONTROLLER_INIT };
		command.Update(simStart);
	}
	
	/// <summary>
	/// Update is called once per frame
	/// </summary>
	public void Update()
	{
		HandleCommand();
		// Update the azimuth and elevation positions.
		UpdateAzimuth();
		UpdateElevation();
	}
	
	/// <summary>
	/// When we first start the sim we will be sending an MCUCommand object over. This will have dummy data until we get a real command from the control room
	/// We know what fake data we are initially building the command with so we can ignore that until we get real data
	/// --- This gets updated every frame
	/// </summary>
	/// <param name="incoming"> the incoming MCUCommand object from <c>SimServer.cs</c></param>
	public void HandleCommand() 
	{
		if(command.errorFlag == true || command.acceleration == (float)Dummy.THICC) 
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
	
	public float Azimuth()
	{
		return simTelescopeAzimuthDegrees;
	}
	
	public bool AzimuthMoving()
	{
		return azimuthMoving;
	}
	
	public float Elevation()
	{
		return simTelescopeElevationDegrees;
	}
	
	public bool ElevationMoving()
	{
		return elevationMoving;
	}
	
	public bool Homed()
	{
		return !AzimuthMoving() && !ElevationMoving() && Azimuth() == 0.0f && Elevation() == 15.0f;
	}
	
	public double UnityAzimuth()
	{
		return System.Math.Round(azimuth.transform.eulerAngles.y, 1);
	}
	
	public double UnityElevation()
	{
		return System.Math.Round(elevation.transform.eulerAngles.z, 1);
	}
	
	public double SimAzimuth()
	{
		return System.Math.Round(simTelescopeAzimuthDegrees, 1);
	}
	
	public double SimElevation()
	{
		return System.Math.Round((simTelescopeElevationDegrees - 15.0f), 1);
	}
	
	public double TargetAzimuth()
	{
		return System.Math.Round(command.azimuthDegrees, 1);
	}
	
	public double TargetElevation()
	{
		return System.Math.Round(command.elevationDegrees - 15.0f, 1);
	}
	
	public double AzimuthSpeed()
	{
		return System.Math.Round(command.azimuthSpeed, 2);
	}
	
	public double ElevationSpeed()
	{
		return System.Math.Round(command.elevationSpeed, 2);
	}
	
	private void HandleStop()
	{
		// if we receive a stop, set the moveTo's to the current position of the simulator
		command.azimuthDegrees = simTelescopeAzimuthDegrees;
		command.elevationDegrees = simTelescopeElevationDegrees;
	}
	
	private void HandleJog()
	{
		// if it's a jog, we want to move 1 degree in the jog direction
		// as of right now, the control room cannot jog on both motors (az & el) at the same time
		// each jog command will be one or the other
		
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
	/// rotates the game object 
	/// </summary>
	/// <param name="azSpeed"> the speed at which we rotate </param>
	/// <returns></returns>
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
	/// updates the elevation of the sim telescope 
	/// </summary>
	/// <param name="elSpeed"> speed which we are moving by </param>
	/// <returns></returns>
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
	/// Class helper method to help with calculating rotations over 0,0.
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
