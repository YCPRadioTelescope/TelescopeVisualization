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

    private void Start()
    {
        pauseMenu.SetActive(true);
        resumeButton.onClick.AddListener(setPauseFalse);
        exitButton.onClick.AddListener(Application.Quit);
        pauseMenu.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            pause = !pause;
        }
        if (pause == true)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            mouseLook.enabled = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            mouseLook.enabled = true;
        }
        pauseMenu.SetActive(pause);
    }
    private void setPauseFalse()
    {
        pause = false;
        Debug.Log("set pause false");
    }
}
