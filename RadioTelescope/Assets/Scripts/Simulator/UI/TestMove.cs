using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

// This script handles manually inputted movement from the simulation. The behavior of the inputted
// values matches the behavior of the custom orientation movement script from the control room. That is,
// the inputted values are the angle to be moved to, and the script calculates how much the telescope
// need to move by to reach that target.
public class TestMove : MonoBehaviour
{
	// The object that controls the telescope's movement according to the current command.
	public TelescopeControllerSim tc;
	
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
		// Get the azimuth, elevation, and speed values from the UI, clamping them to allowable values.
		int az = Mathf.Clamp(int.Parse(azimuth.text), 0, 360);
		int el = Mathf.Clamp(int.Parse(elevation.text), -15, 95);
		int sp = Mathf.Max(0, int.Parse(speed.text));
		
		// Send the clamped values back to the UI in case the user provided out of bounds input.
		elevation.text = el.ToString();
		azimuth.text = az.ToString();
		speed.text = sp.ToString();
		
		// Add 15 degrees to the elevation angle. This is because Unity does not use negative angles,
		// but we consider elevation orientations below the horizon as negative.
		el += 15;
		
		// TODO FROM LUCAS: make this a move by again instead of an absolute move
		
		// Generate a dummy data object that the MCUCommand recognizes as coming from the test UI.
		ushort[] registerData =  { (ushort)MoveType.TEST_MOVE, (ushort)az, (ushort)el, (ushort)sp };
		command.UpdateCommand(registerData);
	}
}
