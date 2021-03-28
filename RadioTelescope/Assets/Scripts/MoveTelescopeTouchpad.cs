using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK.Prefabs.CameraRig.UnityXRCameraRig.Input;

// A script to control the telescope via the left touchpad.
public class MoveTelescopeTouchpad : MonoBehaviour
{
	public TelescopeController tc;
	public UnityAxis1DAction leftTouchpadVertical;
	public UnityAxis1DAction leftTouchpadHorizontal;

	// Update is called once per frame.
	void Update()
	{
		tc.ChangeAzimuth(leftTouchpadHorizontal.Value);
		tc.ChangeElevation(leftTouchpadVertical.Value);
	}
}
