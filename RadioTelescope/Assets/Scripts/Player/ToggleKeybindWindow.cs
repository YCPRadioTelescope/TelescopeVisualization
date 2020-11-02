using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script controls whether the keybinds UI is collapsed or expanded.
public class ToggleKeybindWindow : MonoBehaviour
{
	public GameObject KeybindCollapse;
	public GameObject KeybindExpand;
	private bool expanded;

	// Start is called before the first frame update
	void Start()
	{
		// The keybinds menu starts collapsed.
		expanded = false;
	}

	// Update is called once per frame
	void Update()
	{
		if(Input.GetKeyDown(KeyCode.F1))
			expanded = !expanded;
		
		KeybindCollapse.SetActive(!expanded);
		KeybindExpand.SetActive(expanded);
	}
}
