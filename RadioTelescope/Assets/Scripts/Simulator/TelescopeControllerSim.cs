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
	private float azimuthDegrees;
	private float elevationDegrees;
	private float targetAzimuth;
	private float targetElevation;
	
	// If the angle and target are within this distance, consider them equal.
	private float epsilon = 0.001f;
	
	// Keeps track of whether the azimuth is moving clockwise or counter clockwise.
	private bool moveCCW = false;
	// Keeps track of if a current command is being executed, preventing
	// new commands from being taken. This is mainly for testing purposes
	// and may be removed from the final simulation.
	private bool executingCommand = false;
	
	// Start is called before the first frame.
	public void Start()
	{
		// Set the current azimuth and elevation degrees to the rotation
		// of the game objects, then target 0,15.
		azimuthDegrees = azimuth.transform.eulerAngles.y;
		elevationDegrees = elevation.transform.eulerAngles.z;
		targetAzimuth = 0.0f;
		targetElevation = 15.0f;
	}
	
	// Update is called every frame.
	public void Update()
	{
		// Update the azimuth and elevation positions.
		UpdateAzimuth();
		UpdateElevation();
		
		// If the elevation and azimuth have reached their targets, the current command is
		// done executing. If not, then a command is being executed.
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
		if(azimuthDegrees != targetAzimuth)
		{
			azimuthDegrees = ChangeAzimuth(!moveCCW ? speed : -speed);
			// If the azimuth and target are close, set the azimuth to the target.
			if(AngleDistance(azimuthDegrees, targetAzimuth) < epsilon)
				azimuthDegrees = targetAzimuth;
		}
	}
	
	// Update the telescope elevation.
	private void UpdateElevation()
	{
		// If the current elevation does not equal the target elevation, move toward the target.
		if(elevationDegrees != targetElevation)
		{
			elevationDegrees = ChangeElevation((targetElevation > elevationDegrees) ? speed : -speed);
			// If the elevation and target are close, set the elevation to the target.
			if(AngleDistance(elevationDegrees, targetElevation) < epsilon)
				elevationDegrees = targetElevation;
		}
	}
	
	// Rotate the telescope game object azimuth.
	private float ChangeAzimuth(float moveBy)
	{
		// Alter the movement speed by the time since the last frame. This ensures
		// a smooth movement regardless of the framerate.
		moveBy *= 60.0f * Time.deltaTime;
		// If we're closer to the target than the movement speed, lower the movement
		// speed so that we don't overshoot.
		// Unlike elevation, which doesn't wrap, azimuth needs to account for wrapping.
		// e.g. 350 and 10 are 20 degrees away, not 340.
		float distance = AngleDistance(azimuthDegrees, targetAzimuth);
		if(distance < Mathf.Abs(moveBy))
			moveBy = distance * (moveCCW ? -1 : 1);
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
	
	// Compute the distance between two angles on a circle.
	private float AngleDistance(float a, float b)
	{
		// Mathf.Repeat is functionally similar to the modulus operator, but works with floats.
		return Mathf.Abs(Mathf.Repeat((a - b + 180.0f), 360.0f) - 180.0f);
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
