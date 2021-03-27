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
	public GameObject xRotation;
	public GameObject yRotation;
	public float targetZ = 0.0f;
	public float targetY = 0.0f;
	public float currentZ = 0.0f;
	public float currentY = 0.0f;
	public float speed = 1.0f;
	public Sensors sen;
	
	public TMP_Text ZPos;
	public TMP_Text YPos;
	public TMP_Text ElPos;
	public TMP_Text AzPos;
	public TMP_Text SpeedTxt;
	public TMP_Text TargetYY;
	public TMP_Text TargetXX;
	
	public float yRemainder = 0;
	
	public bool isNeg = false;
	public bool isMovingY = false;
	public bool isMovingZ = false;
	
	public void Update()
	{	
		if(targetZ < 0)
			targetZ += 360.0f;
		
		if(Math.Round(currentY, 1) != Math.Round(targetY, 1))
			currentY = RotateY(!isNeg ? speed : -speed);
		else
			targetY = currentY;
		
		if(targetY == currentY && yRemainder > 0)
		{
			if(isNeg)
			{
				targetY = (targetY + yRemainder) - 360;
				yRemainder = 0;
			}
			else
			{
				targetY = (targetY + yRemainder) - 360;
				yRemainder = 0;
			}
		}
		else if(targetY == currentY && yRemainder == 0)
			isMovingY = false;
		
		if((int)xRotation.transform.localEulerAngles.z != (int) targetZ)
		{
			if(targetZ <= 105.0f && targetZ >= 0)
			{
				if(targetZ >= xRotation.transform.localEulerAngles.z)
					currentZ = RotateZ(-speed);
				else
					currentZ = RotateZ(speed);
				sen.UpdateElevationSensor("Good");
			}
			else
			{
				if(targetZ <= 105.0f)
					targetZ = 1;
				else
					targetZ = 0;
				sen.UpdateElevationSensor("Hit");
			}
		}
		else
			isMovingZ = false;
		
		YPos.text = "Unity Y Position: " + System.Math.Round(currentY, 0);
		ZPos.text = "Unity Z Position: " + System.Math.Round(currentZ, 0);
		if(Math.Round(currentY, 0) == 359)
			AzPos.text = "Y Degrees: " + (System.Math.Round(currentY, 0) + 1);
		else
			AzPos.text = "Y Degrees: " + System.Math.Round(currentY, 2);
		ElPos.text = "X Degrees: " + System.Math.Round((currentZ - 16.0), 0);
		SpeedTxt.text = "Speed: " + System.Math.Round(speed, 2);
	}
	
	public float RotateZ(float speed)
	{
		xRotation.transform.Rotate(0, 0, -speed);
		return xRotation.transform.eulerAngles.z;
	}
	
	public float RotateY(float speed)
	{
		yRotation.transform.Rotate(0, speed, 0);
		currentY += speed + (isNeg ? 360 : 0);
		return currentY - (currentY >= 360 ? 360 : 0);
	}
	
	public void SetZ(float z)
	{
		if(!isMovingZ)
		{
			TargetXX.text = "Target X: " + z;
			targetZ = targetZ + z;
			isMovingZ = true;
		}
	}
	
	public void SetY(float y)
	{
		if(!isMovingY)
		{
			TargetYY.text = "Target Y: " + y;
			isNeg = false;
			if(y < 0)
			{
				isNeg = true;
				y = y * -1;
			}
			
			if(isNeg)
			{
				if(currentY == 0)
					targetY = 360 - y;
				else
					targetY = currentY - y;
			}
			else
				targetY = currentY + y;
			
			if(isNeg && targetY <= 0)
			{
				yRemainder = targetY + 359;
				targetY = 1;
			}
			else if(targetY >= 360)
			{
				yRemainder = targetY - 359;
				targetY = 359;
			}
			isMovingY = true;
		}
	}
	
	public float getCurrentZ()
	{
		return currentZ;
	}
	
	public float getCurrentY()
	{
		return currentY;
	}
}
