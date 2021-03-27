using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script controls the pinning of UI to certain parts of the screen.
// This script runs every frame so that changing the window size updates the UI positions.
public class UIPin : MonoBehaviour
{
	public int position;
	public int bufferX;
	public int bufferY;

	// Updated is called once every frame.
	void FixedUpdate()
	{
		switch(position)
		{
			// Pin the UI to the upper left.
			case 1:
			{
				transform.position = new Vector3(0 + bufferX, Screen.height - bufferY, 0);
				break;
			}
			// Pin the UI to the bottom left.
			case 2:
			{
				transform.position = new Vector3(0 + bufferX, 0 + bufferY, 0);
				break;
			}
			// Pin the UI to the upper right.
			case 3:
			{
				transform.position = new Vector3(Screen.width - bufferX, Screen.height - bufferY, 0);
				break;
			}
			// Pin the UI to the bottom right.
			case 4:
			{
				transform.position = new Vector3(Screen.width - bufferX, 0 + bufferY, 0);
				break;
			}
			// If an invalid position is given, pin the UI to the center of the screen.
			// This makes it obvious that something is wrong.
			default:
			{
				transform.position = new Vector3(Screen.width/2, Screen.height/2, 0);
				break;
			}
		}
	}
}