using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script controls the activation of the HightlightTarget object,
// which highlights the part of the radio telescope that is clicked on
// and shows its name and description.
public class ActivateDescription : MonoBehaviour
{
	public GameObject highlight;
	
	// Start is called before the first frame update.
	void Start()
	{
		// Ensure that the highlighter is inactive at game start.
		highlight.SetActive(false);
	}
	
	// Update is called once per frame.
	void Update()
	{
		// Check if the mouse button is being held. If so, activate
		// the hightlighted game object, which causes its name and
		// description to be shown..
		if(Input.GetMouseButtonDown(0))
			highlight.SetActive(true);
		else if(Input.GetMouseButtonUp(0))
			highlight.SetActive(false);
	}
}
