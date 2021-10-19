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
		oRegisters = new ushort[10];
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
		
		registersText.text = "1st word".PadLeft(54) + " ";
		registersText.text += "2nd word".PadLeft(25) + " ";
		registersText.text += "1st position".PadLeft(25) + " ";
		registersText.text += "2nd position".PadLeft(24) + " ";
		registersText.text += "1st speed".PadLeft(24) + " ";
		registersText.text += "2nd speed".PadLeft(24) + " ";
		registersText.text += "1st accel".PadLeft(25) + " ";
		registersText.text += "2nd accel".PadLeft(25) + " ";
		registersText.text += "1st decel".PadLeft(25) + " ";
		registersText.text += "2nd decel".PadLeft(25) + " ";
		registersText.text += "\n";
		registersText.text += "Incoming Azimuth:  <mspace=0.5em>";
		for(int i = 0; i < iRegisters.Length / 2; i++)
		{
			string text = Convert.ToString(iRegisters[i], 2);
			registersText.text += text.PadLeft(17) + "|";
		}
		registersText.text += "</mspace>\n";
		registersText.text += "Incoming Elevation:<mspace=0.5em>";
		for(int i = iRegisters.Length / 2; i < iRegisters.Length; i++)
		{
			string text = Convert.ToString(iRegisters[i], 2);
			registersText.text += text.PadLeft(17) + "|";
		}
		registersText.text += "</mspace>\n\n";
		
		registersText.text += "motors moving".PadLeft(50) + " ";
		registersText.text += "1st steps".PadLeft(25) + " ";
		registersText.text += "2nd steps".PadLeft(25) + " ";
		registersText.text += "1st encoder".PadLeft(24) + " ";
		registersText.text += "2nd encoder".PadLeft(21) + " ";
		registersText.text += "\n";
		registersText.text += "Outgoing Azimuth:   <mspace=0.5em>";
		for(int i = 0; i < oRegisters.Length / 2; i++)
		{
			string text = Convert.ToString(oRegisters[i], 2);
			registersText.text += text.PadLeft(17) + "|";
		}
		registersText.text += "</mspace>\n";
		registersText.text += "Outgoing Elevation: <mspace=0.5em>";
		for(int i = oRegisters.Length / 2; i < oRegisters.Length; i++)
		{
			string text = Convert.ToString(oRegisters[i], 2);
			registersText.text += text.PadLeft(17) + "|";
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
