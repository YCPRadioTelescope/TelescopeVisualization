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
	
	// Start is called before the first frame update.
	void Start()
	{
		startButton.onClick.AddListener(server.StartServer);
		fillButton.onClick.AddListener(AutoFillInput);
		
		Screen.fullScreen = false;
		Screen.SetResolution(1024, 768, FullScreenMode.Windowed);
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
	
	/// <summary>
	///	method tied to fill button which puts in correct values for the sim MCU
	/// </summary>
	private void AutoFillInput()
	{
		mcuIP.text = "127.0.0.1";
		mcuPort.text = "8083";
	}
}
