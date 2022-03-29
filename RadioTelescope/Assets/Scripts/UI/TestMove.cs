﻿using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using log4net;

// This script handles manually inputted movement from the simulation. The behavior of the inputted
// values matches the behavior of the custom orientation movement script from the control room. That is,
// the inputted values are the angle to be moved to, and the script calculates how much the telescope
// need to move by to reach that target.
public class TestMove : MonoBehaviour
{
	// log4net logger.
	private static readonly ILog Log = LogManager.GetLogger(typeof(TestMove));
	
	// The command that determines the telescope's movement.
	public MCUCommand command;
	
	// The UI objects for user test movement input.
	public TMP_InputField azimuth;
	public TMP_InputField elevation;
	public TMP_InputField speed;
	public Button testButton;
	
	// Start is called before the first frame update.
	void Start()
	{
		// Create a click listener on the test button. If clicked, call TestMovement.
		testButton.onClick.AddListener(TestMovement);
	}
	
	// If the test button is clicked, send the values from the UI to the
	// telescope controller. 
	private void TestMovement()
	{
		Log.Debug("Test movement button pressed.");
		
		// Get the azimuth, elevation, and speed values from the UI, clamping them to allowable values.
		float az;
		float el;
		float sp;
		try
		{
			az = Mathf.Clamp(float.Parse(azimuth.text), 0, 360);
			el = Mathf.Clamp(float.Parse(elevation.text), -15, 95);
			sp = Mathf.Max(0, float.Parse(speed.text));
		}
		catch(Exception e)
		{
			Log.Error("	Failed test movement: " + e);
			return;
		}
		
		// Send the clamped values back to the UI in case the user provided out of bounds input.
		azimuth.text = az.ToString();
		elevation.text = el.ToString();
		speed.text = sp.ToString();
		
		// Add 15 degrees to the elevation angle. This is because Unity does not use negative angles,
		// but we consider elevation orientations below the horizon as negative.
		el += 15;
		
		Log.Debug("	Sending test movement to MCUCommand.\n");
		command.TestMove(az, el, sp);
	}
}