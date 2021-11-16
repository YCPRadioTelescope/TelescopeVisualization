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
	
	public GameObject startPanel;
	public GameObject testPanel;
	public GameObject infoPanel;
	public GameObject failurePanel;
	
	public Button test1Button;
	public Button test2Button;
	public Button test3Button;
	public Button test4Button;
	
	public GameObject test1Panel;
	public GameObject test2Panel;
	public GameObject test3Panel;
	public GameObject test4Panel;
	
	// Start is called before the first frame update.
	void Start()
	{
		// Get the EventSystem game object. This is what finds where the next UI
		// element to move to is.
		system = EventSystem.current;
		
		test1Button.onClick.AddListener(OpenTest1);
		test2Button.onClick.AddListener(OpenTest2);
		test3Button.onClick.AddListener(OpenTest3);
		test4Button.onClick.AddListener(OpenTest4);
	}
	
	// Update is called once per frame.
	void Update()
	{
		// Pressing tab will move the currently selected text field or button downward.
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
	
	// OnGUI generates GUI elements each frame.
	void OnGUI()
	{
		// Create a GUI box over each of the UI panels, then give them tooltips.
		// When a box is hovered over, Unity sets the global GUI.tooltip string
		// to the tooltip of the box.
		Rect startPanelRect = startPanel.GetComponent<RectTransform>().rect;
		startPanelRect.x += startPanel.GetComponent<UIPin>().bufferX;
		startPanelRect.y += startPanel.GetComponent<UIPin>().bufferY;
		GUI.Box(startPanelRect, new GUIContent("", "This menu is for filling in the MCU and PLC IP and port numbers that the simulator listens on for the control room connection. Clicking auto fill for sim will fill the menu with the same numbers that the control room uses for the simulation. Click start sim to start listening. The sim should be started before the control room starts the telescope."), GUIStyle.none);
		
		Rect testPanelRect = testPanel.GetComponent<RectTransform>().rect;
		testPanelRect.x += Screen.width - testPanel.GetComponent<UIPin>().bufferX;
		testPanelRect.y += testPanel.GetComponent<UIPin>().bufferY;
		GUI.Box(testPanelRect, new GUIContent("", "This menu is for testing the radio telescope controller without the control room connection. The behavior of the inputed values matches the behavior of the custom orientation script from the control room."), GUIStyle.none);
		
		Rect infoPanelRect = infoPanel.GetComponent<RectTransform>().rect;
		infoPanelRect.x += Screen.width - infoPanel.GetComponent<UIPin>().bufferX;
		infoPanelRect.y += infoPanel.GetComponent<UIPin>().bufferY;
		GUI.Box(infoPanelRect, new GUIContent("", "This menu displays the current state of the telescope controller variables. Most of the values are updated every frame to match the current state of the telescope controller, except the input azimuth and elevation values which are only updated when the telescope controller receives a command."), GUIStyle.none);
		
		Rect failPanelRect = failurePanel.GetComponent<RectTransform>().rect;
		failPanelRect.x += Screen.width - failurePanel.GetComponent<UIPin>().bufferX;
		failPanelRect.y += failurePanel.GetComponent<UIPin>().bufferY;
		GUI.Box(failPanelRect, new GUIContent("", "WIP panel. We'll use this to test failures."), GUIStyle.none);
		
		GameObject activePanel = GetActiveTest();
		Rect activePanelRect = new Rect();
		if(activePanel)
		{
			activePanelRect = activePanel.GetComponent<RectTransform>().rect;
			activePanelRect.x += Screen.width - activePanel.GetComponent<UIPin>().bufferX;
			activePanelRect.y += activePanel.GetComponent<UIPin>().bufferY;
			// TODO: Change the tooltip depending on which game object is active.
			GUI.Box(activePanelRect, new GUIContent("", "This panel will eventually activate a failure."), GUIStyle.none);
		}
		
		// Set the location of the tooltip. The tooltip should appear under the box being hovered over, so get the position of the mouse to see which box we're hitting.
		Rect tooltip = new Rect(-10,-10,5,5);
		// The mouse is hovering over the start panel.
		float x = Input.mousePosition.x;
		if(x > startPanelRect.x && x < startPanelRect.x + startPanelRect.width)
			tooltip = startPanelRect;
		// The mouse is hovering over the active panel.
		else if(activePanel && x > activePanelRect.x && x < activePanelRect.x + activePanelRect.width)
			tooltip = activePanelRect;
		// The mouse is hovering over the failure panel.
		else if(x > failPanelRect.x && x < failPanelRect.x + failPanelRect.width)
			tooltip = failPanelRect;
		// The mouse is hovering over the test panel.
		else if(x > testPanelRect.x && x < testPanelRect.x + testPanelRect.width)
			tooltip = testPanelRect;
		// The mouse is hovering over the info panel.
		else if(x > infoPanelRect.x && x < infoPanelRect.x + infoPanelRect.width)
			tooltip = infoPanelRect;
		
		// Move the tooltip rect below the panel we're hovering over and
		// increase its height to fit any text.
		tooltip.y += tooltip.height;
		tooltip.height = 500;
		GUI.Label(tooltip, GUI.tooltip);
	}
	
	// Get the failure panel that is currently active, if any.
	private GameObject GetActiveTest()
	{
		if(test1Panel.activeSelf)
			return test1Panel;
		if(test2Panel.activeSelf)
			return test2Panel;
		if(test3Panel.activeSelf)
			return test3Panel;
		if(test4Panel.activeSelf)
			return test4Panel;
		return null;
	}
	
	// When a button is clicked, the failure panel associated with that button
	// is toggled and all other panels are deactivated.
	private void OpenTest1()
	{
		test1Panel.SetActive(!test1Panel.activeSelf);
		test2Panel.SetActive(false);
		test3Panel.SetActive(false);
		test4Panel.SetActive(false);
	}
	
	private void OpenTest2()
	{
		test1Panel.SetActive(false);
		test2Panel.SetActive(!test2Panel.activeSelf);
		test3Panel.SetActive(false);
		test4Panel.SetActive(false);
	}
	
	private void OpenTest3()
	{
		test1Panel.SetActive(false);
		test2Panel.SetActive(false);
		test3Panel.SetActive(!test3Panel.activeSelf);
		test4Panel.SetActive(false);
	}
	
	private void OpenTest4()
	{
		
		test1Panel.SetActive(false);
		test2Panel.SetActive(false);
		test3Panel.SetActive(false);
		test4Panel.SetActive(!test4Panel.activeSelf);
	}
}
