using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script controls whether or not the window is fullscreen.
// Always start in windowed mode, but allow the user to press F11
// to enter fullscreen mode.
public class ScreenHandler : MonoBehaviour
{
	public GameObject VR;
	
	// Awake is called on the initalization of all game objects.
	void Awake()
	{
		// The screen starts in windowed mode if this isn't a VR program.
		Screen.fullScreen = VR.activeInHierarchy;
	}
	
	// Update is called once per frame
	void Update()
	{
		// Toggle fullscreen is F11 is pressed.
		if(Input.GetKeyDown(KeyCode.F11))
			Screen.fullScreen = !Screen.fullScreen;
	}
}
