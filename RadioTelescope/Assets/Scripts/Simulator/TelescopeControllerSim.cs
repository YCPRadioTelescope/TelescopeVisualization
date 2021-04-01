using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental;
using UnityStandardAssets.Vehicles.Car;

// This script controls the telescope according to the inputs from the simulator.
public class TelescopeControllerSim : MonoBehaviour
{
	// The game objects that get rotated by a movement command.
	public GameObject azimuth;
	public GameObject elevation;
	// The speed that the telescope moves at.
	public float speed = 1.0f;
	
	public Sensors sen;
	
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
	
	// The target and current values of the azimuth and elevation.
	private float azimuthDegrees;
	private float elevationDegrees;
	private float targetAzimuth;
	private float targetElevation;
	
	// Keeps track of whether the azimuth is moving clockwise or counter clockwise.
	private bool moveCCW = false;
	
	// Keeps track of if a current command is being executed, preventing
	// new commands from being taken.
	private bool executingCommand = false;
	
	// Start is called before the first frame.
	public void Start()
	{
		azimuthDegrees = azimuth.transform.eulerAngles.y;
		elevationDegrees = elevation.transform.eulerAngles.z;
		targetAzimuth = azimuthDegrees;
		targetElevation = elevationDegrees;
	}
	
	// Update is called every frame.
	public void Update()
	{
		// Update the azimuth and elevation positions.
		UpdateAzimuth();
		UpdateElevation();
		
		// If the elevation and azimuth have reached their targets, the current command is
		// done executing.
		executingCommand = targetElevation != elevationDegrees || targetAzimuth != azimuthDegrees;
		
		// Update the UI at the end of every frame.
		UpdateUI();
	}
	
	// Target a new azimuth.
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
	
	// Target a new elevation.
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
	
	// Get the current azimuth.
	public float GetAzimuthDegrees()
	{
		return azimuthDegrees;
	}
	
	// Get the current elevation.
	public float GetElevationDegrees()
	{
		return elevationDegrees;
	}
	
	// Update the telescope azimuth.
	private void UpdateAzimuth()
	{
		// If the current azimuth does not equal the target azimuth, move toward the target.
		// Round the values to stop when we are close enough, as the slight inaccuracy in floats
		// would cause the telesocpe to never stop moving if we didn't round.
		if(Math.Round(azimuthDegrees, 1) != Math.Round(targetAzimuth, 1))
		{
			// We are currently executing a command.
			executingCommand = true;
			azimuthDegrees = ChangeAzimuth(!moveCCW ? speed : -speed);
		}
		// Account for any inaccuracy caused by the rounding.
		else
			targetAzimuth = azimuthDegrees;
	}
	
	// Update the telescope elevation.
	private void UpdateElevation()
	{
		// If the current elevation does not equal the target elevation, move toward the target.
		// Round the values to stop when we are close enough, as the slight inaccuracy in floats
		// would cause the telesocpe to never stop moving if we didn't round.
		if(Math.Round(elevationDegrees, 1) != Math.Round(targetElevation, 1))
		{
			// We are currently executing a command.
			executingCommand = true;
			elevationDegrees = ChangeElevation((targetElevation >= elevationDegrees) ? speed : -speed);
		}
		// Account for any inaccuracy caused by the rounding.
		else
			targetElevation = elevationDegrees;
	}
	
	// Rotate the telescope game object azimuth.
	private float ChangeAzimuth(float moveBy)
	{
		// Alter the movement speed by the time since the last frame. This ensures
		// a smooth movement regardless of the framerate.
		moveBy *= 60.0f * Time.deltaTime;
		// If we're closer to the target than the movement speed, lower the movement
		// speed so that we don't overshoot.
		if(Mathf.Abs(targetAzimuth - azimuthDegrees) < Mathf.Abs(moveBy))
			moveBy = Mathf.Abs(targetAzimuth - azimuthDegrees) * (moveCCW ? -1 : 1);
		azimuth.transform.Rotate(0, moveBy, 0);
		return BoundAzimuth(azimuthDegrees + moveBy);
	}
	
	// Rotate the telescope game object elevation.
	private float ChangeElevation(float moveBy)
	{
		// Alter the movement speed by the time since the last frame. This ensures
		// a smooth movement regardless of the framerate.
		moveBy *= 60.0f * Time.deltaTime;
		// If we're closer to the target than the movement speed, lower the movement
		// speed so that we don't overshoot.
		if(Mathf.Abs(targetElevation - elevationDegrees) < Mathf.Abs(moveBy))
			moveBy = targetElevation - elevationDegrees;
		elevation.transform.Rotate(0, 0, moveBy);
		return elevationDegrees + moveBy;
	}
	
	// Bound the given azimuth to a value between 0 and 360.
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
	
	// Update the UI according to the current state of the variables when called.
	private void UpdateUI()
	{
		unityAzimuthText.text = "Unity Az Position: " + System.Math.Round(azimuth.transform.eulerAngles.y, 1);
		unityElevationText.text = "Unity El Position: " + System.Math.Round(elevation.transform.eulerAngles.z, 1);
		azimuthText.text = "Azimuth Degrees: " + System.Math.Round(azimuthDegrees, 1);
		elevationText.text = "Elevation Degrees: " + System.Math.Round((elevationDegrees - 15.0f), 1);
		targetElevationText.text = "Target Elevation: " + System.Math.Round(targetElevation, 1);
		targetAzimuthText.text = "Target Azimuth: " + System.Math.Round(targetAzimuth, 1);
		speedText.text = "Speed: " + System.Math.Round(speed, 2);
	}
}
