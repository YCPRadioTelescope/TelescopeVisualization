using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToddStone : MonoBehaviour
{
    public GameObject Todd;
    private bool playing;

    private void OnMouseOver()
    {
        if(Input.GetMouseButtonDown(0) && !playing)
        {
            Todd.active = true;
            playing = true;
            StartCoroutine(Todd_Wait());
        }
    }

    IEnumerator Todd_Wait()
    {
        //yield on a new YieldInstruction that waits for 5 seconds.
        Debug.Log("CALLED");
        yield return new WaitForSeconds(13);
        Todd.active = false;
        playing = false;
        Debug.Log("Disabled");
    }
}
