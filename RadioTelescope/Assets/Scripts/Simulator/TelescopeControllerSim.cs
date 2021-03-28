using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental;
using UnityStandardAssets.Vehicles.Car;

// This script controls the telescope according to the inputs from the simulator.
public class TelescopeControllerSim : MonoBehaviour
{
	public GameObject elevation;
	public GameObject azimuth;
	public float speed = 1.0f;
	public Sensors sen;
	
	public TMP_Text ZPos;
	public TMP_Text YPos;
	public TMP_Text elevationText;
	public TMP_Text azimuthText;
	public TMP_Text speedText;
	public TMP_Text targetAzimuthText;
	public TMP_Text targetElevationText;
	
	private float targetElevation = 0.0f;
	private float targetAzimuth = 0.0f;
	private float elevationDegrees = 0.0f;
	private float azimuthDegrees = 0.0f;
	
	private float azimuthRemainder = 0.0f;
	
	private bool negativeAzimuthTarget = false;
	private bool movingAzimuth = false;
	private bool movingElevation = false;
	
	public void Update()
	{	
		if(Math.Round(azimuthDegrees, 1) != Math.Round(targetAzimuth, 1))
			azimuthDegrees = ChangeAzimuth(!negativeAzimuthTarget ? speed : -speed);
		else
			targetAzimuth = azimuthDegrees;
		
		if(targetAzimuth == azimuthDegrees && azimuthRemainder > 0.0f)
		{
			if(negativeAzimuthTarget)
			{
				targetAzimuth = (targetAzimuth - azimuthRemainder) - 360.0f;
				azimuthRemainder = 0.0f;
			}
			else
			{
				targetAzimuth = (targetAzimuth + azimuthRemainder) - 360.0f;
				azimuthRemainder = 0.0f;
			}
		}
		else if(targetAzimuth == azimuthDegrees && azimuthRemainder == 0.0f)
			movingAzimuth = false;
		
		if((int)elevation.transform.localEulerAngles.z != (int) targetElevation)
		{
			if(targetElevation <= 109.0f && targetElevation >= 0.0f)
			{
				if(targetElevation >= elevation.transform.localEulerAngles.z)
					elevationDegrees = ChangeElevation(-speed);
				else
					elevationDegrees = ChangeElevation(speed);
				sen.UpdateElevationSensor("Good");
			}
			else
			{
				targetElevation = (targetElevation > 109.0f ? 109.0f : 0.0f);
				sen.UpdateElevationSensor("Hit");
			}
		}
		else
			movingElevation = false;
		
		YPos.text = "Unity Az Position: " + System.Math.Round(azimuthDegrees, 0);
		ZPos.text = "Unity El Position: " + System.Math.Round(elevationDegrees, 0);
		if(Math.Round(azimuthDegrees, 0) == 359)
			azimuthText.text = "Azimuth Degrees: " + (System.Math.Round(azimuthDegrees, 0) + 1);
		else
			azimuthText.text = "Azimuth Degrees: " + System.Math.Round(azimuthDegrees, 2);
		elevationText.text = "Elevation Degrees: " + System.Math.Round((elevationDegrees - 16.0f), 0);
		speedText.text = "Speed: " + System.Math.Round(speed, 2);
	}
	
	private float ChangeElevation(float speed)
	{
		elevation.transform.Rotate(0, 0, -speed);
		return elevation.transform.eulerAngles.z;
	}
	
	private float ChangeAzimuth(float speed)
	{
		azimuth.transform.Rotate(0, speed, 0);
		azimuthDegrees += speed + (negativeAzimuthTarget ? 360.0f : 0.0f);
		return azimuthDegrees - (azimuthDegrees >= 360.0f ? 360.0f : 0.0f);
	}
	
	public void TargetElevation(float el)
	{
		if(!movingElevation)
		{
			targetElevationText.text = "Target Elevation: " + el;
			targetElevation = targetElevation + el;
			movingElevation = true;
		}
	}
	
	public void TargetAzimuth(float az)
	{
		if(!movingAzimuth)
		{
			targetAzimuthText.text = "Target Azimuth: " + az;
			negativeAzimuthTarget = false;
			if(az < 0.0f)
			{
				negativeAzimuthTarget = true;
				az = az * -1;
			}
			
			if(negativeAzimuthTarget)
			{
				if(azimuthDegrees == 0.0f)
					targetAzimuth = 360.0f - az;
				else
					targetAzimuth = azimuthDegrees - az;
			}
			else
				targetAzimuth = azimuthDegrees + az;
			
			if(negativeAzimuthTarget && targetAzimuth <= 0.0f)
			{
				azimuthRemainder = targetAzimuth + 359.0f;
				targetAzimuth = 1.0f;
			}
			else if(targetAzimuth >= 360.0f)
			{
				azimuthRemainder = targetAzimuth - 359.0f;
				targetAzimuth = 359.0f;
			}
			movingAzimuth = true;
		}
	}
	
	public float GetElevationDegrees()
	{
		return elevationDegrees;
	}
	
	public float GetAzimuthDegrees()
	{
		return azimuthDegrees;
	}
}
