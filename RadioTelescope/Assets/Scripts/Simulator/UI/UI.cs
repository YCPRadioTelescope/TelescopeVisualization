using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// This script handles UI navigation via the tab key. Pressing tab will move your
// current UI selection to the next UI element (be that a text box or a button).
public class UI : MonoBehaviour
{
	private EventSystem system;
	
	// Start is called before the first frame update.
	void Start()
	{
		// Get the EventSystem game object. This is what finds where the next UI
		// element to move to is.
		system = EventSystem.current;
	}
	
	// Update is called once per frame.
	void Update()
	{
		if(Input.GetKeyDown(KeyCode.Tab))
		{
			// Get the next selectable UI element, looking below the current one.
			Selectable next = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();
			
			if(next != null)
			{
				// Select the next UI element. If this UI element is a text field,
				// allow user input.
				InputField inputfield = next.GetComponent<InputField>();
				if(inputfield != null)
					inputfield.OnPointerClick(new PointerEventData(system));
				system.SetSelectedGameObject(next.gameObject, new BaseEventData(system));
			}
		}
	}
}
