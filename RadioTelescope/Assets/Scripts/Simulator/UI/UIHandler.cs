using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// This script controls the updating of various UI elements
// according to the current state of the SimServer and 
// TelescopeControllerSim scripts.
public class UIHandler : MonoBehaviour
{
	// The object that communicates with the control room and updates the current command.
	public SimServer server;
	
	// The object that controls the telescope's movement according to the current command.
	public TelescopeControllerSim tc;
	
	// The command that determines the telescope's movement.
	public MCUCommand command;
	
	// The UI objects for user input for the MCU IP and port to listen on.
	public Button startButton;
	public Button fillButton;
	public TMP_InputField mcuIP;
	public TMP_InputField mcuPort;
	
	// UI objects that display the current state of various values from the telescope controller.
	public TMP_Text unityAzimuthText;
	public TMP_Text unityElevationText;
	public TMP_Text azimuthText;
	public TMP_Text elevationText;
	public TMP_Text inputAzimuthText;
	public TMP_Text inputElevationText;
	public TMP_Text targetAzimuthText;
	public TMP_Text targetElevationText;
	public TMP_Text azimuthSpeedText;
	public TMP_Text elevationSpeedText;
	
	// UI object that displays the current state of the incoming and outgoing modbus registers.
	public TMP_Text registersText;
	public TMP_Text currentCommand;
	
	// The latest received input azimuth and elevation.
	private double azimuth;
	private double elevation;
	
	// The number of unused register columns from the incoming registers.
	private int unused = 2;
	
	// The current incoming and outgoing registers.
	private ushort[] iRegisters;
	private ushort[] oRegisters;
	
	private int[] numberBase;
	private int baseIndex = 0;
	
	// Start is called before the first frame update.
	void Start()
	{
		// Open the program in windowed mode.
		Screen.fullScreen = false;
		Screen.SetResolution(1340, 720, FullScreenMode.Windowed);
		
		// Create a click listener on the start and fill buttons. If clicked, call StartServer or AutoFillInput
		startButton.onClick.AddListener(server.StartServer);
		fillButton.onClick.AddListener(AutoFillInput);
		
		// Initialize the incoming and outgoing register values to all 0s.
		iRegisters = new ushort[20];
		oRegisters = new ushort[10];
		
		numberBase = new int[3];
		numberBase[0] = 2;
		numberBase[1] = 10;
		numberBase[2] = 16;
	}
	
	// Update is called once per frame.
	void Update()
	{
		// Press escape to exit the program cleanly.
		if(Input.GetKeyDown((KeyCode.Escape)))
			Application.Quit();
		if(Input.GetKeyDown((KeyCode.B)))
			baseIndex = (baseIndex + 1) % 3;
	}
	
	// OnGUI generates GUI elements each frame.
	void OnGUI()
	{
		// Update the values shown on the telescope info panel.
		unityAzimuthText.text = "Unity Azimuth: " + tc.UnityAzimuth();
		unityElevationText.text = "Unity Elevation: " + tc.UnityElevation();
		
		azimuthText.text = "Sim Azimuth: " + tc.SimAzimuth();
		elevationText.text = "Sim Elevation: " + tc.SimElevation();
		
		inputAzimuthText.text = "Input Azimuth: " + azimuth;
		inputElevationText.text = "Input Elevation: " + elevation;
		
		targetAzimuthText.text = "Target Azimuth: " + tc.TargetAzimuth();
		targetElevationText.text = "Target Elevation: " + tc.TargetElevation();
		
		azimuthSpeedText.text = "Azimuth Speed: " + tc.AzimuthSpeed();
		elevationSpeedText.text = "Elevation Speed: " + tc.ElevationSpeed();
		
		// Update the string for the current command.
		currentCommand.text = "Current command: " + command.currentCommand;
		
		// Update the values shown on the modbus registers panel.
		// First print the incoming azimuth and elevation registers.
		registersText.text = "1st word".PadLeft(54) + " ";
		registersText.text += "2nd word".PadLeft(25) + " ";
		registersText.text += "1st position".PadLeft(25) + " ";
		registersText.text += "2nd position".PadLeft(24) + " ";
		registersText.text += "1st speed".PadLeft(24) + " ";
		registersText.text += "2nd speed".PadLeft(24) + " ";
		registersText.text += "acceleration".PadLeft(24) + " ";
		registersText.text += "deceleration".PadLeft(24) + " ";
		registersText.text += "\n";
		registersText.text += "Incoming Azimuth:  <mspace=0.5em>";
		for(int i = 0; i < iRegisters.Length / 2 - unused; i++)
		{
			string text = Convert.ToString(iRegisters[i], numberBase[baseIndex]);
			registersText.text += text.PadLeft(17) + "|";
		}
		registersText.text += "</mspace>\n";
		registersText.text += "Incoming Elevation:<mspace=0.5em>";
		for(int i = iRegisters.Length / 2; i < iRegisters.Length - unused; i++)
		{
			string text = Convert.ToString(iRegisters[i], numberBase[baseIndex]);
			registersText.text += text.PadLeft(17) + "|";
		}
		registersText.text += "</mspace>\n\n";
		
		// Print the outgoing azimuth and elevation registers.
		registersText.text += "motors moving".PadLeft(50) + " ";
		registersText.text += "1st steps".PadLeft(25) + " ";
		registersText.text += "2nd steps".PadLeft(25) + " ";
		registersText.text += "1st encoder".PadLeft(24) + " ";
		registersText.text += "2nd encoder".PadLeft(21) + " ";
		registersText.text += "\n";
		registersText.text += "Outgoing Azimuth:   <mspace=0.5em>";
		for(int i = 0; i < oRegisters.Length / 2; i++)
		{
			string text = Convert.ToString(oRegisters[i], numberBase[baseIndex]);
			registersText.text += text.PadLeft(17) + "|";
		}
		registersText.text += "</mspace>\n";
		registersText.text += "Outgoing Elevation: <mspace=0.5em>";
		for(int i = oRegisters.Length / 2; i < oRegisters.Length; i++)
		{
			string text = Convert.ToString(oRegisters[i], numberBase[baseIndex]);
			registersText.text += text.PadLeft(17) + "|";
		}
		registersText.text += "</mspace>";
	}
	
	// Get the current MCU IP input.
	public string MCUIP()
	{
		return mcuIP.text;
	}
	
	// Get the current MCU port input.
	public string MCUPort()
	{
		return mcuPort.text;
	}
	
	// Turn the start sim button green to shows that the simulation
	// has started.
	public void StartSim()
	{
		startButton.GetComponent<Image>().color = Color.green;
	}
	
	// Updating the input azimuth target.
	public void InputAzimuth(float input)
	{
		azimuth = System.Math.Round(input, 1);
	}
	
	// Updating the input elevation target.
	public void InputElevation(float input)
	{
		elevation = System.Math.Round(input, 1);
	}
	
	// Updating the incoming register values.
	public void UpdateIncoming(ushort[] newRegisters)
	{
		iRegisters = newRegisters;
	}
	
	// Updating the outgoing register values.
	public void UpdateOutgoing(ushort[] newRegisters)
	{
		oRegisters = newRegisters;
	}
	
	// Autofill the MCU IP and port to listen on to the default used by
	// the control room for the simulation.
	private void AutoFillInput()
	{
		mcuIP.text = "127.0.0.1";
		mcuPort.text = "8083";
	}
}
