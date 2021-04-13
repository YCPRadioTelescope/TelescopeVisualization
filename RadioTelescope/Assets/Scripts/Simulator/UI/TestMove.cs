using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// This script handles manually inputted movement from the simulation. The behavior of the inputted
// values matches the behavior of the custom orientation movement script from the control room. That is,
// the inputted values are the angle to be moved to, and the script calculates how much the telescope
// need to move by to reach that target.
public class TestMove : MonoBehaviour
{
	public TelescopeControllerSim tc;
	public TMP_InputField azimuth;
	public TMP_InputField elevation;
	public TMP_InputField speed;
	public Button testButton;
	
	// Start is called before the first frame update.
	void Start()
	{
		// Create a click listener on the test button.
		testButton.onClick.AddListener(TestMovement);
	}
	
	// If the test button is clicked, send the values from the UI to the
	// telescope controller. 
	private void TestMovement()
	{
		// Get the azimuth and elevation values from the UI, clamping them to allowable values.
		float az = Mathf.Clamp(float.Parse(azimuth.text), 0.0f, 360.0f);
		float el = Mathf.Clamp(float.Parse(elevation.text), -15.0f, 94.0f);
		// Send the clamped values back to the UI in case the user provided out of bounds input.
		elevation.text = el.ToString();
		azimuth.text = az.ToString();
		
		// Calculate how far we want to move from the current location.
		az = az - tc.simTelescopeAzimuthDegrees;
		el = el - tc.simTelescopeElevationDegrees + 15.0f;
		// Account for if it's quicker to spin counter clockwise or across 0.
		if(az > 180.0f)
			az -= 360.0f;
		if(az < -180.0f)
			az += 360.0f;
		
		// Send the command.
		tc.TargetElevation(el);
		tc.TargetAzimuth(az);
		tc.speed = float.Parse(speed.text);
	}
}
