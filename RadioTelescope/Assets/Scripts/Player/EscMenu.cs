using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// This script controls the behavior of the escape menu.
public class EscMenu : MonoBehaviour
{
	private bool pause = false;
	public GameObject pauseMenu;
	public Button resumeButton;
	public Button exitButton;
	public MouseLook mouseLook;
	
	// Start is called before the first frame update.
	void Start()
	{
		// Don't show the pause menu when the game is started.
		pauseMenu.SetActive(false);
	}
	
	// Update is called once per frame.
	void Update()
	{
		// If escape is pressed, the pause menu is active.
		if(Input.GetButtonDown("Cancel"))
			pause = true;
		
		if(pause)
		{
			// If pause is active, unlock the cursor and add on click 
			// listeners to see if the resume or exit buttons are pressed.
			Cursor.lockState = CursorLockMode.None;
			// The pause menu can only be closed by clicking resume.
			resumeButton.onClick.AddListener(SetPauseFalse);
			exitButton.onClick.AddListener(Application.Quit);
		}
		else
			// If pause is inactive, clock the mouse to the player view.
			Cursor.lockState = CursorLockMode.Locked;
		
		Cursor.visible = pause;
		mouseLook.enabled = !pause;
		pauseMenu.SetActive(pause);
	}
	
	private void SetPauseFalse()
	{
		pause = false;
	}
}
