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
	public SimServer server;
	
	public Button startButton;
	public Button fillButton;
	public TMP_InputField mcuIP;
	public TMP_InputField mcuPort;
	
	public TelescopeControllerSim tc;
	
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
	
	private double azimuth;
	private double elevation;
	
	public TMP_Text registersText;
	
	private ushort[] iRegisters;
	private ushort[] oRegisters;
	
	// Start is called before the first frame update.
	void Start()
	{
		startButton.onClick.AddListener(server.StartServer);
		fillButton.onClick.AddListener(AutoFillInput);
		
		Screen.fullScreen = false;
		Screen.SetResolution(1024, 768, FullScreenMode.Windowed);
		
		iRegisters = new ushort[20];
		oRegisters = new ushort[20];
	}
	
	// OnGUI generates GUI elements each frame.
	void OnGUI()
	{
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
		
		// X0 = first word
		// X1 = second word
		// X2 = first position
		// X3 = second position
		// X4 = first speed
		// X5 = second speed
		// X6 = first accel
		// X7 = second accel
		// X8 = first decel
		// X9 = second decel
		registersText.text = "Incoming Azimuth:   <mspace=0.5em>";
		for(int i = 0; i < 10; i++)
		{
			string text = Convert.ToString(iRegisters[i], 2);
			registersText.text += text.PadLeft(16) + "|";
		}
		registersText.text += "</mspace>\n";
		registersText.text += "Incoming Elevation: <mspace=0.5em>";
		for(int i = 10; i < 20; i++)
		{
			string text = Convert.ToString(iRegisters[i], 2);
			registersText.text += text.PadLeft(16) + "|";
		}
		
		registersText.text += "</mspace>\n\n\n";
		
		// X0 = 
		// X1 = motors moving bit
		// X2 = 
		// X3 = first steps
		// X4 = second steps
		// X5 = first encoder
		// X6 = second encoder
		// X7 = 
		// X8 = 
		// X9 = 
		registersText.text += "Outgoing Azimuth:   <mspace=0.5em>";
		for(int i = 0; i < 10; i++)
		{
			string text = Convert.ToString(oRegisters[i], 2);
			registersText.text += text.PadLeft(16) + "|";
		}
		registersText.text += "</mspace>\n";
		registersText.text += "Outgoing Elevation: <mspace=0.5em>";
		for(int i = 10; i < 20; i++)
		{
			string text = Convert.ToString(oRegisters[i], 2);
			registersText.text += text.PadLeft(16) + "|";
		}
		registersText.text += "</mspace>";
	}
	
	public string MCUIP()
	{
		return mcuIP.text;
	}
	
	public string MCUPort()
	{
		return mcuPort.text;
	}
	
	public void StartSim()
	{
		startButton.GetComponent<Image>().color = Color.green;
	}
	
	public void InputAzimuth(float input)
	{
		azimuth = System.Math.Round(input, 1);
	}
	
	public void InputElevation(float input)
	{
		elevation = System.Math.Round(input, 1);
	}
	
	public void UpdateIncoming(ushort[] newRegisters)
	{
		iRegisters = newRegisters;
	}
	
	public void UpdateOutgoing(ushort[] newRegisters)
	{
		oRegisters = newRegisters;
	}
	
	/// <summary>
	///	method tied to fill button which puts in correct values for the sim MCU
	/// </summary>
	private void AutoFillInput()
	{
		mcuIP.text = "127.0.0.1";
		mcuPort.text = "8083";
	}
}
