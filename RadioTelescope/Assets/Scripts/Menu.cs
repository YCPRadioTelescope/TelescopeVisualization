using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{

    public Animator Fade;
    public void CloseProgram()
    {
        Application.Quit();
    }

    public void StartProgram()
    {
        Fade.SetTrigger("Start");
        StartCoroutine(StartFade());

        IEnumerator StartFade()
        {
            yield return new WaitForSeconds(3);
            SceneManager.LoadScene(1);
        }
    }
}
