using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

// This script casts a ray when activated. Should this ray touch a part
// of the radio telescope, that object is highlighted, and the associated
// ObjectDesc script's name and description is saved to be displayed to
// the player.
public class HighlightTarget : MonoBehaviour
{
	// Start and end are two invisible cubes, one on top of the player
	// and one off in the distance, between which the ray is cast.
	public GameObject start;
	public GameObject end;
	// HighlightColor and shader are applied to the object being highlighted.
	public Color highlightColor;
	public Shader shader;
	// Text saves the name and description of the 
	public Text text;
	
	// The object that the raycast hit.
	private RaycastHit hitInfo;
	// The original material of the object that was hit, and the material that
	// it becomes when highlighted.
	private Material origMat, tempMat;
	// The renderer that was just hit by the raycast.
	private Renderer currRend;
	// The renderer that was hit last frame by the raycast.
	private Renderer rend = null;

	// Update is called once per frame
	void Update()
	{
		// The direction of the ray cast is forward the direction of the start object.
		var dir = start.transform.forward * 10000;
		// Cast a ray between the start object and end object. If a part of the telescope
		// is hit, hitInfo is changed.
		if(Physics.Raycast(start.transform.position, dir, out hitInfo, Vector3.Distance(start.transform.position, end.transform.position)))
		{
			// Get the renderer object of the impacted game object.
			currRend = hitInfo.collider.gameObject.GetComponent<Renderer>();
			
			// If we already hit this object last frame, don't change anything.
			if(currRend == rend)
				return;
			
			// Get the name and description from the part that was hit.
			text.text = hitInfo.transform.GetComponent<ObjectDesc>().Name + ": " + hitInfo.transform.GetComponent<ObjectDesc>().Description;
			
			// If we've hit a different object, reset the old object to its original material.
			if(rend)
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
		else if(rend)
		{
			rend.sharedMaterial = origMat;
			rend = null;
			text.text = "";
		}
	}
	
	// When this highlighter is disabled, reset any renderer that might have been highlighted.
	void OnDisable()
	{
		if(rend)
			rend.sharedMaterial = origMat;
		rend = null;
		text.text = "";
	}
}
