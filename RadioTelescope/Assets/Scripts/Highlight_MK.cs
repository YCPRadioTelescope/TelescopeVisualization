using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using VRTK.Prefabs.CameraRig.UnityXRCameraRig.Input;
using UnityEngine.UI;

// This script casts a ray when activated. Should this ray touch a part
// of the radio telescope, that object is highlighted, and the associated
// ObjectDesc script's name and description is saved to be displayed to
// the player.
public class Highlight_MK : MonoBehaviour
{
	// Start and end are two invisible cubes, one on top of the player
	// and one off in the distance, between which the ray is cast.
	public GameObject start;
	public GameObject end;
	// HighlightColor and shader are applied to the object being highlighted.
	public Color highlightColor;
	public Shader shader;
	// Text saves the name and description of the highlighted part.
	public Text text;
	public GameObject background;

	// The player game object to be teleported when shift+clicking.
	public GameObject player;
	private int delayTimer;

	// The object that the raycast hit.
	private RaycastHit hitInfo;
	// The original material of the object that was hit, and the material that
	// it becomes when highlighted.
	private Material origMat, tempMat;
	// The renderer that was just hit by the raycast.
	private Renderer currRend;
	// The renderer that was hit last frame by the raycast.
	private Renderer rend = null;

	GateMovement gateMovment;

	private void Update()
	{
		// The direction of the ray cast is forward the direction of the start object.
		var dir = start.transform.forward * 10000;

		// Cast a ray between the start object and end object. If a part of the telescope
		// is hit, hitInfo is changed.
		if (Physics.Raycast(start.transform.position, dir, out hitInfo, Vector3.Distance(start.transform.position, end.transform.position)))
		{

			Debug.Log(hitInfo.transform.name);
			// If shift is held down, then the player is attempting to teleport. Move the player to the point that was hit.
			if (player && Input.GetKey(KeyCode.LeftShift) && delayTimer <= 0)
			{
				delayTimer = 10;
				player.transform.position = hitInfo.point;
				return;
			}
			delayTimer--;
			// If shift wasn't held down and we hit something that isn't a telescope part,
			// reset any previously highlighted part and return.
			
			gateMovment = hitInfo.transform.GetComponent<GateMovement>();

			if (hitInfo.transform.GetComponent<GateMovement>())
			{ 
				hitInfo.transform.parent.GetComponent<GateMovement>().activateGate();
			}
			
			if (!hitInfo.transform.GetComponent<TelescopePartInfo>())
			{
				Reset();
				return;
			}
			
			


			// Get the renderer object of the impacted game object.
			currRend = hitInfo.collider.gameObject.GetComponent<Renderer>();

			// If we already hit this object last frame, don't change anything.
			if (currRend == rend)
				return;

			// Get the name and description from the part that was hit.
			text.text = hitInfo.transform.GetComponent<TelescopePartInfo>().Name + ": " + hitInfo.transform.GetComponent<TelescopePartInfo>().Description;
			if (background)
				background.SetActive(true);

			// If we've hit a different object, reset the old object to its original material.
			if (rend)
				rend.sharedMaterial = origMat;

			// Save the original material of the impacted object and change its material to the
			// highlighted texture.
			rend = currRend;
			origMat = rend.sharedMaterial;
			tempMat = new Material(origMat);
			rend.material = tempMat;
			rend.material.shader = shader;
		}
		// If nothing was hit but something had previously been hit, reset the material of that object.
		else if (rend)
			Reset();
	}

	// Resets the state of the last highlighted part, if any, and the description text GUI.
	void Reset()
	{
		if (rend)
			rend.sharedMaterial = origMat;
		rend = null;
		text.text = "";
		if (background)
			background.SetActive(false);
	}
}
