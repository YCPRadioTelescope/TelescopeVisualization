using UnityEngine;
using System.Collections;

// This script measures the current FPS of the program and updates
// the UI with the measured FPS.
public class FPSDisplay : MonoBehaviour
{
	private float deltaTime = 0.0f;
	
	// Update is called once per frame.
	void Update()
	{
		// Keep track of the elapsed time since the last frame.
		deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
	}
	
	// OnGUI is called when the GUI is rendered each frame.
	void OnGUI()
	{
		// Create the GUI.
		GUIStyle style = new GUIStyle();
		int w = Screen.width, h = Screen.height;
		
		// Position the GUI.
		Rect rect = new Rect(0, 0, w, h * 2 / 100);
		style.alignment = TextAnchor.UpperLeft;
		style.fontSize = h * 2 / 100;
		style.normal.textColor = new Color (1.0f, 1.0f, 1.0f, 1.0f);
		
		// Calculate the FPS.
		float msec = deltaTime * 1000.0f;
		float fps = 1.0f / deltaTime;
		string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
		
		// Draw the GUI.
		GUI.Label(rect, text, style);
	}
}