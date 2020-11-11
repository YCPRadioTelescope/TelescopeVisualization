using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A script that moves the radio telescope. Must be called to from a separate script.
public class TelescopeController : MonoBehaviour
{
	public GameObject xRotation;
	public GameObject yRotation;
	private float rotateX;
	private ExplodedView exploding;
	private Quaternion xOriginal;
	private Quaternion yOriginal;

	private void Start()
	{
		exploding = GetComponent<ExplodedView>();
		// Save the original orientation of the telescope so that we can reset to it later.
		xOriginal = xRotation.transform.localRotation;
		yOriginal = yRotation.transform.localRotation;
	}

	/**
	 * Takes in a speed and moves the telescope
	 * Movement is not allowed while the telescope is exploding
	 * Returns the current rotation
	 * RotateX returns X rotation
	 * RotateY returns Y roataion
	 */
	public float RotateX(float speed)
	{
		if(!exploding.IsMoving())
		{
			rotateX = xRotation.transform.eulerAngles.z;
			rotateX += speed;
			rotateX = Mathf.Clamp(rotateX, 0, 100);
			xRotation.transform.localRotation = Quaternion.Euler(0, 0, rotateX);
		}
		return xRotation.transform.eulerAngles.z;
	}
	
	public float RotateY(float speed)
	{
		if(!exploding.IsMoving())
			yRotation.transform.Rotate(0, speed, 0);
		return yRotation.transform.eulerAngles.y;
	}

	public void ResetRotation()
	{
		xRotation.transform.localRotation = xOriginal;
		yRotation.transform.localRotation = yOriginal;
	}
}