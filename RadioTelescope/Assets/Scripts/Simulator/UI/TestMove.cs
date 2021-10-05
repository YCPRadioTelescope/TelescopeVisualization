using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using static MCUCommand;

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
		int az = Mathf.Clamp(int.Parse(azimuth.text), 0, 360);
		int el = Mathf.Clamp(int.Parse(elevation.text), -15, 95);
		// Send the clamped values back to the UI in case the user provided out of bounds input.
		elevation.text = el.ToString();
		azimuth.text = az.ToString();
		
		// we need to add 15 
		el += 15;
		
		// TODO: make this a move by again instead of an absolute move
		
		// build register data and send command
		Debug.Log("TEST_MOVE BEFORE CONVERT -- az: " + az);
		Debug.Log("TEST_MOVE BEFORE CONVERT -- el: " + el);
		
		// we don't really care about the other fields, they will be set by a special case in MCUCommand
		// I grabbed these hardcoded bit calculation values from the control room
		// the challenge with this is to convert a float to a short, which needs to be split over 2 elements in order to recombine them 
		// into 
		ushort[] registerData =  { 0x0096, (ushort)az, (ushort)el };
		
		MCUCommand testMoveCommand = new MCUCommand(registerData);
		tc.SetNewMCUCommand(testMoveCommand);
	}
}
