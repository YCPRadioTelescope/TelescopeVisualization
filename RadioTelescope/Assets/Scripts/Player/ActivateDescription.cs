using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

// This script controls the activation of the HightlightTarget scripts,
// which highlight the part of the radio telescope that is clicked on
// or pointed at in VR and shows that part's name and description.
public class ActivateDescription : MonoBehaviour
{
	public GameObject highlight;
	public GameObject vrHighlight;
	public SteamVR_Action_Single rightTrigger;
	
	// Start is called before the first frame update.
	void Start()
	{
		// Ensure that the highlighter is inactive at game start.
		highlight.SetActive(false);
		vrHighlight.SetActive(false);
	}
	
	// Update is called once per frame.
	void Update()
	{
		// Check if the mouse button is being held or the right trigger
		// is being pressed. If so, activate the highlight script.
		if(Input.GetMouseButtonDown(0))
			highlight.SetActive(true);
		else if(Input.GetMouseButtonUp(0))
			highlight.SetActive(false);
		
		if(rightTrigger.GetAxis(SteamVR_Input_Sources.RightHand) > 0.2f)
			vrHighlight.SetActive(true);
		else
			vrHighlight.SetActive(false);
	}
}
