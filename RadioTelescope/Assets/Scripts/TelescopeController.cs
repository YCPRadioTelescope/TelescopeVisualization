﻿using System.Collections;
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
			// Elevation must be clamped to a value between 0 and 109 degrees.
			// This allows -15 degrees of movement below the horizon and 94 degrees above.
			float elevationChange = elevation.transform.eulerAngles.z;
			elevationChange += speed;
			elevationChange = Mathf.Clamp(elevationChange, 0, 109);
			elevation.transform.localRotation = Quaternion.Euler(0, 0, elevationChange);
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
