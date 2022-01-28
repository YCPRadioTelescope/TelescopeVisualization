using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A collection of static helper methods. Some are used by multiple classes
// and defined here to reduce duplication of code.
public class Utilities
{
	private const int STEPS_PER_REVOLUTION = 20000;
	private const int ENCODER_COUNTS_PER_REVOLUTION_BEFORE_GEARING = 8000;
	
	/// <summary>
	/// Helper method to compute the distance between two angles on a circle.
	/// </summary>
	public static float AngleDistance(float a, float b)
	{
		// Mathf.Repeat is functionally similar to the modulus operator, but works with floats.
		return Mathf.Repeat((a - b + 180.0f), 360.0f) - 180.0f;
	}
	
	/// <summary>
	/// Determine the distance required to stop if moving at the given speed
	/// and slowing with the given deceleration.
	/// </summary>
	/// <param name="speed">The current rotation speed in degrees per second.</param>
	/// <param name="decel">The deceleration rate in degrees per second squared.</param>
	/// <returns>The distance in degrees required to stop.</returns>
	public static float StoppingDistance(float speed, float decel)
	{
		// Kinematics!
		//		dx = change in distance
		//		v0 = velocity original
		//		vf = velocity final
		//		a = acceleration
		//		t = time
		
		// vf = v0 + at
		// t = (vf - v0) / a
		// 		v0 = 0, vf = azSpeed, a = accel, t = time to reach 0 velocity.
		float stoppingTime = speed / decel;
		
		// dx = v0t + 0.5at^2
		//		v0 = azSpeed, t = stopping time, a = accel, dx = distance to reach 0 velocity.
		return speed * stoppingTime - 0.5f * decel * stoppingTime * stoppingTime;
	}
	
	/// <summary>
	/// Method used to convert steps to degrees.
	/// </summary>
	/// <param name="steps"> steps passed from the control room (or wherever)</param>
	/// <param name="gearingRatio"> the constant ratio associated with the type of movement. For us, this will be azimuth or elevation gearing </param>
	/// <returns> a float "degree" value from the information passed </returns>
	public static float StepsToDegrees(int steps, float gearingRatio)
	{
		return steps * 360.0f / (STEPS_PER_REVOLUTION * gearingRatio);
	}
	
	/// <summary>
	/// Helper method to convert degrees into encoder values.
	/// </summary>
	/// <param name="degrees"> degrees passed from the sim</param>
	/// <param name="gearingRatio"> the constant ratio associated with the type of movement. For us, this will be azimuth or elevation gearing </param>
	/// <returns> an int "encoder" value from the information passed </returns>
	public static int DegreesToEncoder(float degrees, int gearingRatio)
	{
		return (int)(degrees * ENCODER_COUNTS_PER_REVOLUTION_BEFORE_GEARING * gearingRatio / 360.0);
	}
	
	/// <summary>
	/// Helper method to convert degrees into steps.
	/// </summary>
	/// <param name="degrees"> degrees passed from the sim</param>
	/// <param name="gearingRatio"> the constant ratio associated with the type of movement. For us, this will be azimuth or elevation gearing </param>
	/// <returns> an int "step" value from the information passed </returns>
	public static int DegreesToSteps(float degrees, int gearingRatio)
	{
		return (int)(degrees * STEPS_PER_REVOLUTION * gearingRatio / 360.0);
	}
	
	/// <summary>
	/// Helper method to handle rotating across 0 azimuth.
	/// </summary>
	public static float BoundAzimuth(float az)
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
	/// Helper method for bounding elevation within the limit switches.
	/// </summary>
	public static float BoundElevation(float el, float min, float max)
	{
		if(el < min)
			return min;
		if(el > max)
			return max;
		return el;
	}
	
	/// <summary>
	/// Helper method to determine if the magnitidue of the given angle is within
	/// the epsilon distance.
	/// </summary>
	public static bool WithinEpsilon(float angle, float epsilon)
	{
		return Mathf.Abs(angle) < epsilon;
	}
}
