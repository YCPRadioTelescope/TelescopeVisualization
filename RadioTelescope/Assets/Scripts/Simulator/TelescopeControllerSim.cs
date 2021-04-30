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
	public TMP_Text azimuthSpeedText;
	public TMP_Text elevationSpeedText;
	
	// The current and target values of the azimuth and elevation.
	public float simTelescopeAzimuthDegrees;
	public float simTelescopeElevationDegrees;

	private float targetAzimuth;
	private float targetElevation;

	// our MCUCommand object
	private MCUCommand currentMCUCommand;
	
	// If the angle and target are within this distance, consider them equal.
	private float epsilon = 0.1f;
	
	// Keeps track of whether the azimuth is moving clockwise or counter clockwise.
	private bool moveCCW = false;
	
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
		ushort[] simStart = {(ushort) MoveType.SIM_TELESCOPECONTROLLER_INIT};
		currentMCUCommand = new MCUCommand(simStart);
	}
	
	/// <summary>
	/// Update is called once per frame
	/// </summary>
	public void Update()
	{
		// Update the azimuth and elevation positions.
		UpdateAzimuth();
		UpdateElevation();
		
		// Update the UI at the end of every frame.
		UpdateUI();
	}

	/// <summary>
	/// When we first start the sim we will be sending an MCUCommand object over. This will have dummy data until we get a real command from the control room
	/// We know what fake data we are initially building the command with so we can ignore that until we get real data
	/// --- This gets updated every frame
	/// </summary>
	/// <param name="incoming"> the incoming MCUCommand object from <c>SimServer.cs</c></param>
	public void SetNewMCUCommand(MCUCommand incoming) 
	{
		if (incoming.errorFlag == false && incoming.acceleration != (float) Dummy.THICC) 
		{
			currentMCUCommand = incoming;

			if (currentMCUCommand.stopMove)
			{
				// if we receive a stop, set the moveTo's to the current position of the simulator
				currentMCUCommand.azimuthDegrees = simTelescopeAzimuthDegrees;
				currentMCUCommand.elevationDegrees = simTelescopeElevationDegrees;
			}

			// if it's a jog, we want to move 1 degree in the jog direction
			// as of right now, the control room cannot jog on both motors (az & el) at the same time
			// each jog command will be one or the other
			else if (currentMCUCommand.jog) 
			{
				// figure out azimuth direction
				if (currentMCUCommand.azimuthSpeed > 0)
				{
					Debug.Log("TELESCOPECONTROLLER: Positive Azimuth jog");
					currentMCUCommand.azimuthDegrees = simTelescopeAzimuthDegrees + 1.0f;

					// need to set the elevation to the current elevation so we can identify when we stopped moving
					// be default we check if the mcuCommand.el == simTelescope.el to see if the movement is "done"
					currentMCUCommand.elevationDegrees = simTelescopeElevationDegrees;

					moveCCW = false;
					currentMCUCommand.azimuthDegrees = BoundAzimuth(currentMCUCommand.azimuthDegrees);
				} else if (currentMCUCommand.azimuthSpeed < 0)
				{
					Debug.Log("TELESCOPECONTROLLER: Negative Azimuth jog");
					currentMCUCommand.azimuthDegrees = simTelescopeAzimuthDegrees - 1.0f;

					// we can't target 0 degrees, so incase of 0 just subtract 1 more
					if (currentMCUCommand.azimuthDegrees == 0)
						currentMCUCommand.azimuthDegrees = -1.0f;

					// need to set the elevation to the current elevation so we can identify when we stopped moving
					// be default we check if the mcuCommand.el == simTelescope.el to see if the movement is "done"
					currentMCUCommand.elevationDegrees = simTelescopeElevationDegrees;

					// we always want to send a negative value here, no matter the degree we always want to move *left*
					// the *true* passed in is for the leftJog flag, which tells TargetAzimuth to always set the moveCWW flag to true
					moveCCW = true;
					currentMCUCommand.azimuthDegrees = BoundAzimuth(currentMCUCommand.azimuthDegrees);
				}

				// figure out elevation direction
				// have to update the mcuCommand here (not in MCUCommand.cs) because that class doesn't have access to the current telescope
				// orientation. The TargetElevation method makes a check to change elevation based on the currentMCUCommand's data,
				// se we have to update that member here
				else if (currentMCUCommand.elevationSpeed > 0)
				{
					currentMCUCommand.elevationDegrees = simTelescopeElevationDegrees + 1.0f;

					// same thing here as with the azimuth stuff, need to make sure the azimuth for this incoming command is the same 
					// as the simTelescope position so we can register the move complete
					currentMCUCommand.azimuthDegrees = simTelescopeAzimuthDegrees;

					UpdateElevationUI(currentMCUCommand.elevationDegrees);
				} else if (currentMCUCommand.elevationSpeed < 0)
				{
					currentMCUCommand.elevationDegrees = simTelescopeElevationDegrees - 1.0f;

					currentMCUCommand.azimuthDegrees = simTelescopeAzimuthDegrees;
					UpdateElevationUI(currentMCUCommand.elevationDegrees);
				}
			} else // RELATIVE MOVE, just pass in the targeted degrees
			{
				// AngleDistance returns a value relative to the current position, if it is negative we know we need to go left
				float distance = AngleDistance(currentMCUCommand.azimuthDegrees, simTelescopeAzimuthDegrees);
				moveCCW = distance < 0;

				currentMCUCommand.azimuthDegrees = BoundAzimuth(currentMCUCommand.azimuthDegrees);
				UpdateAzimuthUI(currentMCUCommand.azimuthDegrees);
				UpdateElevationUI(currentMCUCommand.elevationDegrees);
			}
		} 
	}
	
	/// <summary>
	/// update the UI
	/// </summary>
	/// <param name="az"></param>
	/// <param name="leftJog"></param>
	public void UpdateAzimuthUI(float az, bool leftJog = false)
	{
		inputAzimuthText.text = "Input Azimuth: " + System.Math.Round(az, 1);
	}
	
	/// <summary>
	/// Target a new elevation.
	/// </summary>
	public void UpdateElevationUI(float el)
	{
		inputElevationText.text = "Input Elevation: " + System.Math.Round(el, 1);
	}
	
	/// <summary>
	/// Update the telescope azimuth.
	/// <summary>
	private void UpdateAzimuth()
	{
		// If the current azimuth does not equal the target azimuth, move toward the target.
		if(simTelescopeAzimuthDegrees != currentMCUCommand.azimuthDegrees)
		{
			simTelescopeAzimuthDegrees = ChangeAzimuth(moveCCW ? -currentMCUCommand.azimuthSpeed : currentMCUCommand.azimuthSpeed);
			
			// If the azimuth and target are close, set the azimuth to the target.
			if(Mathf.Abs(AngleDistance(simTelescopeAzimuthDegrees, currentMCUCommand.azimuthDegrees)) < epsilon)
			{
				Debug.Log("Distance between simTelescopeAzimuthDegrees and currentMCUCommand azimuth degrees close enough, finishing movement");
				simTelescopeAzimuthDegrees = currentMCUCommand.azimuthDegrees;
			}
		}
	}
	
	/// <summary>
	/// Update the telescope elevation, called every frame a move is not happening.
	/// <summary>
	private void UpdateElevation()
	{
		// If the current elevation does not equal the target elevation, move toward the target.
		if(simTelescopeElevationDegrees != currentMCUCommand.elevationDegrees)
		{
			simTelescopeElevationDegrees = ChangeElevation((currentMCUCommand.elevationDegrees > simTelescopeElevationDegrees) 
															? currentMCUCommand.elevationSpeed : -currentMCUCommand.elevationSpeed);
			// If the elevation and target are close, set the elevation to the target.
			if(Mathf.Abs(AngleDistance(simTelescopeElevationDegrees, currentMCUCommand.elevationDegrees)) < epsilon)
				simTelescopeElevationDegrees = currentMCUCommand.elevationDegrees;
		}
	}

	/// <summary>
	/// rotates the game object 
	/// </summary>
	/// <param name="azSpeed"> the speed at which we rotate </param>
	/// <returns></returns>
	private float ChangeAzimuth(float azSpeed)
	{
		// FOR PRESENTATION PURPOSES ONLY:
		// Hard set the speed to 2 while the speed from the MCUCommand still isn't calibrated.
		azSpeed = 2.0f;
		// Alter the movement speed by the time since the last frame. This ensures
		// a smooth movement regardless of the framerate.
		azSpeed *= 60.0f * Time.deltaTime;

		// If we're closer to the target than the movement speed, lower the movement
		// speed so that we don't overshoot.
		// Unlike elevation, which doesn't wrap, azimuth needs to account for wrapping.
		// e.g. 350 and 10 are 20 degrees away, not 340.
		float distance = Mathf.Abs(AngleDistance(simTelescopeAzimuthDegrees, currentMCUCommand.azimuthDegrees));
		if (distance < Mathf.Abs(azSpeed)) 
		{
		 	azSpeed = distance * (moveCCW ? -1 : 1);
		}
		azimuth.transform.Rotate(0, azSpeed, 0);
		// add the speed to the simTelescopeAz because the speed is how much we move in a single frame
		// speed is the relative 
		return BoundAzimuth(simTelescopeAzimuthDegrees + azSpeed);
	}
	
	/// <summary>
	/// updates the elevation of the sim telescope 
	/// </summary>
	/// <param name="elSpeed"> speed which we are moving by </param>
	/// <returns></returns>
	private float ChangeElevation(float elSpeed)
	{
		// FOR PRESENTATION PURPOSES ONLY:
		// Hard set the speed to 2 while the speed from the MCUCommand still isn't calibrated.
		elSpeed = 2.0f;
		// Alter the movement speed by the time since the last frame. This ensures
		// a smooth movement regardless of the framerate.
		elSpeed *= 60.0f * Time.deltaTime;
		// If we're closer to the target than the movement speed, lower the movement
		// speed so that we don't overshoot.
		if (Mathf.Abs(currentMCUCommand.elevationDegrees - simTelescopeElevationDegrees) < Mathf.Abs(elSpeed))
			elSpeed = currentMCUCommand.elevationDegrees - simTelescopeElevationDegrees;
		elevation.transform.Rotate(0, 0, elSpeed);
		return simTelescopeElevationDegrees + elSpeed;
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
		return Mathf.Repeat((a - b + 180.0f), 360.0f) - 180.0f;
	}
	
	/// <summary>
	/// Update the UI according to the current state of the variables when called.
	/// TODO: the elevation needs to be offset somehow to actually be correct, can never show -15 degrees
	/// </summary>
	private void UpdateUI()
	{
		unityAzimuthText.text = "Unity Az Position: " + System.Math.Round(azimuth.transform.eulerAngles.y, 1);
		unityElevationText.text = "Unity El Position: " + (System.Math.Round(elevation.transform.eulerAngles.z, 1));
		azimuthText.text = "Azimuth Degrees: " + System.Math.Round(simTelescopeAzimuthDegrees, 1);
		elevationText.text = "Elevation Degrees: " + (System.Math.Round((simTelescopeElevationDegrees - 15.0f), 1));
		targetElevationText.text = "Target Elevation: " + System.Math.Round(currentMCUCommand.elevationDegrees - 15.0f, 1);
		targetAzimuthText.text = "Target Azimuth: " + System.Math.Round(currentMCUCommand.azimuthDegrees, 1);
		azimuthSpeedText.text = "Azimtuh Speed: " + System.Math.Round(currentMCUCommand.azimuthSpeed, 2);
		elevationSpeedText.text = "Elevation Speed: " + System.Math.Round(currentMCUCommand.elevationSpeed, 2);
	}
}
