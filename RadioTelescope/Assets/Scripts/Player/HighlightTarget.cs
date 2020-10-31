using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

// This script casts a ray when activated. Should this ray touch a part
// of the radio telescope, that object is highlights, and the associated
// ObjectDesc script's name and description is saved to be displayed to
// the player.
public class HighlightTarget : MonoBehaviour
{
	// Start and end are two invisible cubes, one on top of the player
	// and one off in the distance, between which the ray is cast.
	public GameObject start;
	public GameObject end;
	public Color highlightColor;
	public Shader shader;
	public Text text;
	
	private RaycastHit hitInfo;
	private Material origMat, tempMat;
	private Renderer rend = null;
	private Renderer currRend;

	// Update is called once per frame
	void Update()
	{
		var dir = start.transform.forward * 10000;
		if(Physics.Raycast(start.transform.position, dir, out hitInfo, Vector3.Distance(start.transform.position, end.transform.position)))
		{
			text.text = hitInfo.transform.GetComponent<ObjectDesc>().Name + ": " + hitInfo.transform.GetComponent<ObjectDesc>().Description;
			Debug.DrawRay(start.transform.position, dir);

			currRend = hitInfo.collider.gameObject.GetComponent<Renderer>();

			if(currRend == rend)
				return;

			if(currRend && rend && currRend != rend)
				rend.sharedMaterial = origMat;

			if(currRend)
				rend = currRend;
			else
				return;

			origMat = rend.sharedMaterial;
			tempMat = new Material(origMat);
			rend.material = tempMat;
			rend.material.shader = shader;
		}
		else if(rend)
		{
			rend.sharedMaterial = origMat;
			rend = null;
			text.text = "";
		}
	}
	
	private void OnDisable()
	{
		if(rend)
			rend.sharedMaterial = origMat;
		rend = null;
		text.text = "";
	}
}
