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
	public TMP_Text ZPos;
	public TMP_Text YPos;
	public TMP_Text azimuthText;
	public TMP_Text elevationText;
	public TMP_Text speedText;
	public TMP_Text targetAzimuthText;
	public TMP_Text targetElevationText;
	
	// The target and current value of the azimuth and elevation.
	private float targetAzimuth = 0.0f;
	private float targetElevation = 0.0f;
	private float azimuthDegrees = 0.0f;
	private float elevationDegrees = 0.0f;
	
	// Keeps track of whether the azimuth is moving clockwise or counter clockwise.
	private bool moveCCW = false;
	
	// Keeps track of if a current command is being executed, preventing
	// new commands from being taken.
	private bool executingCommand = false;
	
	// Update is called every frame.
	public void Update()
	{
		// Update the azimuth and elevation positions.
		UpdateAzimuth();
		UpdateElevation();
		
		// If the elevation and azimuth have reached their targets, the current command is
		// done executing.
		executingCommand = targetElevation != ElevationTransform() || targetAzimuth != azimuthDegrees;
		
		// Update the UI at the end of every frame.
		UpdateUI();
	}
	
	// Target a new azimuth.
	public void TargetAzimuth(float az)
	{
		// No new commands are taken if one is already executing.
		if(executingCommand)
			return;
		
		// If the azimuth we were given is negative, we are moving counter clockwise.
		moveCCW = (az < 0.0f);
		targetAzimuth = BoundAzimuth(targetAzimuth + az);
	}
	
	// Target a new elevation.
	public void TargetElevation(float el)
	{
		// No new commands are taken if one is already executing.
		if(executingCommand)
			return;
		
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
		if(Math.Round(ElevationTransform(), 1) != Math.Round(targetElevation, 1))
		{
			// We are currently executing a command.
			executingCommand = true;
			elevationDegrees = ChangeElevation((targetElevation >= ElevationTransform()) ? speed : -speed);
		}
		// Account for any inaccuracy caused by the rounding.
		else
			targetElevation = ElevationTransform();
	}
	
	// Rotate the telescope game object azimuth.
	private float ChangeAzimuth(float speed)
	{
		azimuth.transform.Rotate(0, speed, 0);
		return BoundAzimuth(azimuthDegrees + speed);
	}
	
	// Rotate the telescope game object elevation.
	private float ChangeElevation(float speed)
	{
		elevation.transform.Rotate(0, 0, speed);
		return ElevationTransform();
	}
	
	// Bound the given azimuth to a value between 0 and 360.
	private float BoundAzimuth(float az)
	{
		if(az < 0.0f)
			az += 360.0f;
		if(az >= 360.0f)
			az -= 360.0f;
		return az;
	}
	
	// Unlike the azimuth, the elevation object's rotation on the is not equivelant to
	// the elevation degrees. Because of this, comparisons are done against this value
	// instead of against elevationDegrees as UpdateAzimuth does with azimuthDegrees.
	private float ElevationTransform()
	{
		return elevation.transform.eulerAngles.z;
	}
	
	// Update the UI according to the current state of the variables when called.
	private void UpdateUI()
	{
		YPos.text = "Unity Az Position: " + System.Math.Round(azimuthDegrees, 1);
		ZPos.text = "Unity El Position: " + System.Math.Round(elevationDegrees, 1);
		azimuthText.text = "Azimuth Degrees: " + System.Math.Round(azimuthDegrees, 1);
		elevationText.text = "Elevation Degrees: " + System.Math.Round((elevationDegrees - 15.0f), 1);
		speedText.text = "Speed: " + System.Math.Round(speed, 2);
		targetElevationText.text = "Target Elevation: " + System.Math.Round(targetElevation, 1);
		targetAzimuthText.text = "Target Azimuth: " + System.Math.Round(targetAzimuth, 1);
	}
}
