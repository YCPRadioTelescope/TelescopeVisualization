using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EscMenu : MonoBehaviour
{
	private bool pause = false;
	public GameObject pauseMenu;
	public Button resumeButton;
	public Button exitButton;
	public mouseLook mouseLook;
	
	// Start is called before the first frame update.
	void Start()
	{
		// Don't show the pause menu when the game is started.
		pauseMenu.SetActive(false);
	}
	
	// Update is called once per frame.
	void Update()
	{
		// If escape is pressed, toggle the state of the pause menu.
		if(Input.GetButtonDown("Cancel"))
			pause = !pause;
		
		if(pause)
		{
			// If pause is active, unlock the cursor and add on click 
			// listeners to see if the resume or exit buttons are pressed.
			Cursor.lockState = CursorLockMode.None;
			resumeButton.onClick.AddListener(setPauseFalse);
			exitButton.onClick.AddListener(Application.Quit);
			Cursor.visible = true;
			mouseLook.enabled = false;
		}
		else
		{
			// If pause is inactive, clock the mouse to the player view.
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			mouseLook.enabled = true;
		}
		pauseMenu.SetActive(pause);
	}
	
	private void setPauseFalse()
	{
		pause = false;
	}
}
