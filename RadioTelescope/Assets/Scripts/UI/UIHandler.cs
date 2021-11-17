﻿using System;
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
	public TMP_Text currentBase;
	
	// The latest received input azimuth and elevation.
	private double azimuth;
	private double elevation;
	
	// The number of unused register columns from the incoming registers.
	private int unused = 2;
	
	// The current incoming and outgoing registers.
	private ushort[] iRegisters;
	private ushort[] oRegisters;
	
	// Variables for keeping track of the current base to display the registers in.
	private int[] numberBase;
	private int baseIndex = 2;
	
	private float timer = 0.0f;
	private bool failedSim = false;
	private Color old;
	private int flash = 0;
	
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
		oRegisters = new ushort[13];
		
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
		
		timer += Time.deltaTime;
		if(failedSim)
		{
			if(flash > 0)
			{
				if(flash % 2 == 1)
					startButton.GetComponent<Image>().color = Color.red;
				else
					startButton.GetComponent<Image>().color = old;
				if(timer > 0.3f)
				{
					flash--;
					timer -= 0.3f;
				}
			}
			else
			{
				startButton.GetComponent<Image>().color = old;
				failedSim = false;
			}
		}
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
		
		// Update the string for the current command and display base.
		currentCommand.text = "Current command: " + command.currentCommand;
		currentBase.text = "Display base: " + numberBase[baseIndex].ToString();
		
		// Update the values shown on the modbus registers panel.
		// First print the incoming azimuth and elevation registers.
		registersText.text = "MSW command".PadLeft(45) + " ";
		registersText.text += "LSW command".PadLeft(17) + " ";
		registersText.text += "MSW data".PadLeft(22) + " ";
		registersText.text += "LSW data".PadLeft(22) + " ";
		registersText.text += "MSW speed".PadLeft(22) + " ";
		registersText.text += "LSW speed".PadLeft(22) + " ";
		registersText.text += "acceleration".PadLeft(23) + " ";
		registersText.text += "deceleration".PadLeft(23) + " ";
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
		registersText.text += "MSW status bits".PadLeft(49) + " ";
		registersText.text += "LSW status bits".PadLeft(22) + " ";
		registersText.text += "MSW steps".PadLeft(22) + " ";
		registersText.text += "LSW steps".PadLeft(22) + " ";
		registersText.text += "MSW encoder".PadLeft(21) + " ";
		registersText.text += "LSW encoder".PadLeft(20) + " ";
		registersText.text += "heartbeat".PadLeft(24) + " ";
		registersText.text += "\n";
		registersText.text += "Outgoing Azimuth:   <mspace=0.5em>";
		for(int i = 0; i < oRegisters.Length / 2 + 1; i++)
		{
			string text = Convert.ToString(oRegisters[i], numberBase[baseIndex]);
			registersText.text += text.PadLeft(17) + "|";
		}
		registersText.text += "</mspace>\n";
		registersText.text += "Outgoing Elevation: <mspace=0.5em>";
		for(int i = oRegisters.Length / 2 + 1; i < oRegisters.Length; i++)
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
	
	public void FailedSimStart()
	{
		if(failedSim)
			return;
		
		timer = 0.0f;
		failedSim = true;
		old = startButton.GetComponent<Image>().color;
		flash = 3;
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
