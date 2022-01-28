using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A class representing an axis of rotation of the telescope.
// Contains information about the state of the axis, such as
// whether it is currently in motion, its current speed, its
// current orientation, and more.
public class Axis
{
	public string name;
	public float degrees;
	
	public float speed = 0.0f;
	
	// Whether the axis is moving, the direction of travel (true = positive,
	// false = negative), and the acceleration direction. Always check the
	// moving bool before checking the motion or accelerating bools.
	public bool moving = false;
	public bool posMotion = false;
	public bool accelerating = false;
	public bool decelerating = false;
	public bool homed = false;
	
	private MCUCommand command;
	
	public Axis(bool azimuth, float axisDegrees, MCUCommand com)
	{
		degrees = axisDegrees;
		command = com;
		
		if(azimuth)
			name = "azimuth";
		else
			name = "elevation";
	}
	
	public float Cached()
	{
		return (name == "azimuth") ? command.cachedAzData : command.cachedElData;
	}
	
	public float MaxSpeed()
	{
		return (name == "azimuth") ? command.azimuthSpeed : command.elevationSpeed;
	}
	
	public float Acceleration()
	{
		return (name == "azimuth") ? command.azimuthAcceleration : command.elevationAcceleration;
	}
	
	public float Deceleration()
	{
		return (name == "azimuth") ? command.azimuthDeceleration : command.elevationDeceleration;
	}
}
