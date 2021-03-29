using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// This script handles manually inputted movement from the simulation.
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
		// TODO: Match behavior of control room custom orientation script. You provide a value
		// equal to the angle that you want to end at, not the angle that you want to move by.
		// This would also bound test elevation movements to allowable values.
		float el = float.Parse(elevation.text);
		float az = float.Parse(azimuth.text);
		tc.TargetElevation(el);
		tc.TargetAzimuth(az);
		tc.speed = float.Parse(speed.text);
	}
}
