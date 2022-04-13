using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// This script controls the behavior of the escape menu.
public class EscMenu : MonoBehaviour
{
	public bool pause;
	public GameObject pauseMenu;
	public Button resumeButton;
	public Button exitButton;
	public mouseLook mouseLook;

    private void Start()
    {
		//start the game in a unpaused state
		pause = false;
		Cursor.lockState = CursorLockMode.Locked;
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
