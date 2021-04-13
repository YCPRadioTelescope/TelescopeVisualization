using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental;
using UnityStandardAssets.Vehicles.Car;
using static MCUCommand;

// This script controls the telescope according to the inputs from the simulator.
public class TelescopeControllerSim : MonoBehaviour
{
	// The game objects that get rotated by a movement command.
	public GameObject azimuth;
	public GameObject elevation;
	
	// The speed that the telescope moves at.
	public float speed = 1.0f;
	
	// UI elements that get updated with the state of variables.
	public TMP_Text unityAzimuthText;
	public TMP_Text unityElevationText;
	public TMP_Text azimuthText;
	public TMP_Text elevationText;
	public TMP_Text targetAzimuthText;
	public TMP_Text targetElevationText;
	public TMP_Text inputAzimuthText;
	public TMP_Text inputElevationText;
	public TMP_Text speedText;
	
	// The current and target values of the azimuth and elevation.
	public float simTelescopeAzimuthDegrees { get; set; }
	public float simTelescopeElevationDegrees { get; set; }

	private float targetAzimuth { get; set; }
	private float targetElevation { get; set; }

	// our MCUCommand object
	private MCUCommand currentMCUCommand;
	
	// If the angle and target are within this distance, consider them equal.
	private float epsilon = 0.001f;
	
	// Keeps track of whether the azimuth is moving clockwise or counter clockwise.
	private bool moveCCW = false;
	// Keeps track of if a current command is being executed, preventing
	// new commands from being taken. This is mainly for testing purposes
	// and may be removed from the final simulation.
	private bool executingCommand = false;
	
	/// <summary>
	/// Start is called before the first frame
	/// </summary>
	public void Start()
	{
		// Set the current azimuth and elevation degrees to the rotation
		// of the game objects, then target 0,15.
		simTelescopeAzimuthDegrees = azimuth.transform.eulerAngles.y;
		simTelescopeElevationDegrees = elevation.transform.eulerAngles.z;
		// we need to create a dummy MCU command to start
		// this is a custom command to point at 0, 15
		ushort[] simStart = {0x0069};
		currentMCUCommand = new MCUCommand(simStart);
	}
	
	/// <summary>
	/// Update is called once per frame
	/// </summary>
	public void Update()
	{
		// TODO: we need to be updating by the variables set in the incoming MCU command, not just the target azimuth and elevation
		// Update the azimuth and elevation positions.
		UpdateAzimuth();
		UpdateElevation();
		
		// If the elevation and azimuth have reached their targets, the current command is
		// done executing. If not, then a command is being executed.
		executingCommand = targetAzimuth != simTelescopeElevationDegrees || targetElevation != simTelescopeAzimuthDegrees;
		
		// Update the UI at the end of every frame.
		UpdateUI();
	}

	/// <summary>
	/// When we first start the sim we will be sending an MCUCommand object over. This will have dummy data until we get a real command from the control room
	/// We know what fake data we are initially building the command with so we can ignore that until we get real data
	/// </summary>
	public void SetNewMCUCommand(MCUCommand incoming) 
	{
		if (incoming.acceleration != 420.69f) 
		{
			currentMCUCommand = incoming;
			TargetAzimuth(currentMCUCommand.azimuthDegrees);
			TargetElevation(currentMCUCommand.elevationDegrees);
		} 
	}
	
	/// <summary>
	/// Target a new azimuth.
	/// </summary>
	public void TargetAzimuth(float az)
	{
		// No new commands are taken if one is already executing.
		if(executingCommand)
		{
			inputAzimuthText.text = "Input Azimuth: IGNORED";
			return;
		}
		inputAzimuthText.text = "Input Azimuth: " + System.Math.Round(az, 1);
		
		// If the azimuth we were given is negative, we are moving counter clockwise.
		moveCCW = (az < 0.0f);
		targetAzimuth = BoundAzimuth(targetAzimuth + az);
	}
	
	/// <summary>
	/// Target a new elevation.
	/// </summary>
	public void TargetElevation(float el)
	{
		// No new commands are taken if one is already executing.
		if(executingCommand)
		{
			inputElevationText.text = "Input Elevation: IGNORED";
			return;
		}
		inputElevationText.text = "Input Elevation: " + System.Math.Round(el, 1);
		
		targetElevation += el;
	}
	
	/// <summary>
	/// Update the telescope azimuth.
	/// <summary>
	private void UpdateAzimuth()
	{
		// If the current azimuth does not equal the target azimuth, move toward the target.
		if(simTelescopeAzimuthDegrees != targetAzimuth)
		{
			// TODO: How does the speed come over, do we need to flip it or are we always going to get a properly signed speed for direction?
			simTelescopeAzimuthDegrees = ChangeAzimuth(!moveCCW ? currentMCUCommand.azimuthSpeed : -currentMCUCommand.azimuthSpeed);
			// If the azimuth and target are close, set the azimuth to the target.
			if(AngleDistance(simTelescopeAzimuthDegrees, simTelescopeAzimuthDegrees) < epsilon)
				simTelescopeAzimuthDegrees = targetAzimuth;
		}
	}
	
	/// <summary>
	/// Update the telescope elevation.
	/// <summary>
	private void UpdateElevation()
	{
		// If the current elevation does not equal the target elevation, move toward the target.
		if(simTelescopeElevationDegrees != targetElevation)
		{
			// TODO: How does the speed come over, do we need to flip it or are we always going to get a properly signed speed for direction?
			simTelescopeElevationDegrees = ChangeElevation((targetElevation > simTelescopeElevationDegrees) ? currentMCUCommand.elevationSpeed : -currentMCUCommand.elevationSpeed);
			// If the elevation and target are close, set the elevation to the target.
			if(AngleDistance(simTelescopeElevationDegrees, targetElevation) < epsilon)
				simTelescopeElevationDegrees = targetElevation;
		}
	}
	
	/// <summary>
	/// Rotate the telescope game object azimuth.
	/// </summary>
	private float ChangeAzimuth(float moveBy)
	{
		// Alter the movement speed by the time since the last frame. This ensures
		// a smooth movement regardless of the framerate.
		moveBy *= 60.0f * Time.deltaTime;
		// If we're closer to the target than the movement speed, lower the movement
		// speed so that we don't overshoot.
		// Unlike elevation, which doesn't wrap, azimuth needs to account for wrapping.
		// e.g. 350 and 10 are 20 degrees away, not 340.
		float distance = AngleDistance(simTelescopeAzimuthDegrees, targetAzimuth);
		if(distance < Mathf.Abs(moveBy))
			moveBy = distance * (moveCCW ? -1 : 1);
		azimuth.transform.Rotate(0, moveBy, 0);
		return BoundAzimuth(simTelescopeAzimuthDegrees + moveBy);
	}
	
	/// <summary>
	/// Rotate the telescope game object elevation.
	/// </summary>
	private float ChangeElevation(float moveBy)
	{
		// Alter the movement speed by the time since the last frame. This ensures
		// a smooth movement regardless of the framerate.
		moveBy *= 60.0f * Time.deltaTime;
		// If we're closer to the target than the movement speed, lower the movement
		// speed so that we don't overshoot.
		if(Mathf.Abs(targetElevation - simTelescopeElevationDegrees) < Mathf.Abs(moveBy))
			moveBy = targetElevation - simTelescopeElevationDegrees;
		elevation.transform.Rotate(0, 0, moveBy);
		return simTelescopeElevationDegrees + moveBy;
	}
	
	/// <summary>
	/// Class helper method to help with calculating rotations over 0,0
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
	/// Class helper method to compute the distance between two angles on a circle.
	/// </summary>
	private float AngleDistance(float a, float b)
	{
		// Mathf.Repeat is functionally similar to the modulus operator, but works with floats.
		return Mathf.Abs(Mathf.Repeat((a - b + 180.0f), 360.0f) - 180.0f);
	}
	
	/// <summary>
	/// Update the UI according to the current state of the variables when called.
	/// </summary>
	private void UpdateUI()
	{
		unityAzimuthText.text = "Unity Az Position: " + System.Math.Round(azimuth.transform.eulerAngles.y, 1);
		unityElevationText.text = "Unity El Position: " + System.Math.Round(elevation.transform.eulerAngles.z, 1);
		azimuthText.text = "Azimuth Degrees: " + System.Math.Round(simTelescopeAzimuthDegrees, 1);
		elevationText.text = "Elevation Degrees: " + System.Math.Round((simTelescopeElevationDegrees - 15.0f), 1);
		targetElevationText.text = "Target Elevation: " + System.Math.Round(targetElevation, 1);
		targetAzimuthText.text = "Target Azimuth: " + System.Math.Round(targetAzimuth, 1);
		speedText.text = "Speed: " + System.Math.Round(speed, 2);
	}
}
