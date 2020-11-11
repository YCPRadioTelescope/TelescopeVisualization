using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK.Prefabs.CameraRig.UnityXRCameraRig.Input;

// A script to control the telescope via the left touchpad.
public class MoveTelescopeTouchpad : MonoBehaviour
{
	public GameObject telescope;
	public TelescopeController tc;
	public UnityAxis1DAction leftTouchpadVertical;
	public UnityAxis1DAction leftTouchpadHorizontal;

	// Start is called before the first frame update.
	void Start()
	{
		tc = telescope.GetComponent<TelescopeController>();
	}

	// Update is called once per frame.
	void Update()
	{
		tc.RotateY(leftTouchpadHorizontal.Value);
		tc.RotateX(leftTouchpadVertical.Value);
	}
}
