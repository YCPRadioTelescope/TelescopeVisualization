using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A script to control the telescope via the arrow keys.
public class MoveTelescope : MonoBehaviour
{
	public TelescopeController tc;

	// Update is called once per frame
	void Update()
	{
		if(Input.GetKey("up"))
			tc.ChangeElevation(-0.25f);
		
		if(Input.GetKey("down"))
			tc.ChangeElevation(0.25f);
		
		if(Input.GetKey("right"))
			tc.ChangeAzimuth(0.25f);
		
		if(Input.GetKey("left"))
			tc.ChangeAzimuth(-0.25f);
	}
}
