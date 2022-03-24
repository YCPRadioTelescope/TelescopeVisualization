using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A script that moves the radio telescope. Must be called to from a separate script.
public class TelescopeController : MonoBehaviour
{
	public GameObject elevation;
	public GameObject azimuth;
	private ExplodedView exploding;
	private Quaternion originalElevation;
	private Quaternion originalAzimuth;
	
	private void Start()
	{
		exploding = GetComponent<ExplodedView>();
		// Save the original orientation of the telescope so that we can reset to it later.
		originalElevation = elevation.transform.localRotation;
		originalAzimuth = azimuth.transform.localRotation;
	}
	
	public float ChangeElevation(float speed)
	{
		if(!exploding.IsMoving())
		{
			// Elevation must be clamped to a value between -8 and 92 degrees 
			// This allows -8 degrees of movement below the horizon and 92 degrees above.
			// Angle of elevation is converted to negative values when greater than 180
			float elevationChange = elevation.transform.eulerAngles.z;
			float angle = elevationChange;
			angle = (angle > 180) ? angle - 360 : angle;
			angle = (angle * -1) + 8;  //the telescope is offset by 8 degrees and must be converted from negatives
			float next_angle = Mathf.Round((angle - speed) * 10.0f) * 0.1f;

			if (next_angle >= -8f && next_angle <= 92)  //these are the bounds ie: -8 degrees below horizon and 92 degrees above
			{
				elevationChange += speed;
				elevation.transform.localRotation = Quaternion.Euler(0, 0, elevationChange);
			}
		}
		return elevation.transform.eulerAngles.z;
	}
	
	public float ChangeAzimuth(float speed)
	{
		if(!exploding.IsMoving())
			azimuth.transform.Rotate(0, speed, 0);
		return azimuth.transform.eulerAngles.y;
	}
	
	public void ResetOrientation()
	{
		elevation.transform.localRotation = originalElevation;
		azimuth.transform.localRotation = originalAzimuth;
	}
}
