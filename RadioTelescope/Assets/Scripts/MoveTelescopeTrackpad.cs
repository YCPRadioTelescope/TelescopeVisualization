using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

// Thank you random internet stranger: https://youtu.be/bn8eMxBcI70

public class MoveTelescopeTrackpad : MonoBehaviour
{
	public GameObject telescope;
	public TelescopeController tc;
	public SteamVR_Action_Vector2 trackpad;
	
	// Start is called before the first frame update.
	void Start()
	{
		tc = telescope.GetComponent<TelescopeController>();
	}
	
	// Update is called once per frame.
	void Update()
	{
		Vector2 axis = trackpad.GetAxis(SteamVR_Input_Sources.Any);
		tc.RotateY(axis.x);
		tc.RotateZ(axis.y);
	}
}
