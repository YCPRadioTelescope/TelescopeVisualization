// Originally coded by Abishek J Reuben
// https://abishekjreuben.wixsite.com/portfolio/post/creating-a-exploded-view-for-3d-models-in-unity

using UnityEngine;
using System;
using System.Collections.Generic;
using VRTK.Prefabs.CameraRig.UnityXRCameraRig.Input;

[Serializable]

public class SubMeshes
{
	public MeshRenderer meshRenderer;
	public Vector3 originalPosition;
	public Vector3 explodedPosition;
}

public class ExplodedView : MonoBehaviour
{
	#region Variables
	public List<SubMeshes> childMeshRenderers;
	public float explosionSpeed = 1.0f;
	public UnityAxis1DAction leftTrigger;
	
	private bool isMoving = false;
	private bool isInExplodedView = false;
	private float delay;

	private TelescopeController tc;
	#endregion

	#region UnityFunctions
	// Awake is called on the initalization of all game objects.
	private void Start()
	{
		// The exploded script has a telescope controller so that the telescope can be reset when exploding.
		tc = GetComponent<TelescopeController>();

		// Create a list of all objects to explode and generate an exploded position for each.
		childMeshRenderers = new List<SubMeshes>();
		foreach(var item in GetComponentsInChildren<MeshRenderer>())
		{
			SubMeshes mesh = new SubMeshes();
			mesh.meshRenderer = item;
			mesh.originalPosition = item.transform.localPosition;
			// Generate an exploded position that is 50% further away from the center of the object than the original position.
			mesh.explodedPosition = ((item.bounds.center - this.transform.position) * 1.5f) + this.transform.position;
			if(item.transform.GetComponent<ObjectDesc>())
			{
				// The exploded position of each piece is tweaked slightly to result in better positions for viewing.
				mesh.explodedPosition.y += item.transform.GetComponent<ObjectDesc>().yOffset;
				mesh.explodedPosition.z += item.transform.GetComponent<ObjectDesc>().xOffset;
			}
			childMeshRenderers.Add(mesh);
		}
	}

	// Update is called once per frame.
	private void Update()
	{
		// Both R and the left trigger can activate the exploded view.
		if(delay <= 0 && (Input.GetButtonDown("Toggle Exploded View") || leftTrigger.Value > 0.2f))
		{
			ToggleExplodedView();
			// Causing the telescope to explode resets its rotation to the default position.
			if(isInExplodedView)
				tc.ResetRotation();
			// The exploded view can only be toggled once per second.
			// Otherwise, holding the left trigger would cause the
			// exploded view to rapidly toggle.
			delay = 1;
		}
		if(delay > 0)
			delay -= Time.deltaTime;

		if(isMoving)
		{
			if(isInExplodedView)
			{
				// Move all objects to their exploded position.
				foreach(var item in childMeshRenderers)
				{
					item.meshRenderer.transform.position = Vector3.Lerp(item.meshRenderer.transform.position, item.explodedPosition, explosionSpeed);
					// Once any one part of the telescope comes within .0001 units distance of its exploded position, stop everything.
					// All parts should reach their exploded positions at roughly the same time, so we don't need everything perfectly where it should be.
					if(Vector3.Distance(item.meshRenderer.transform.position, item.explodedPosition) < 0.0001f)
						isMoving = false;
				}
			}
			else
			{
				// Move all objects back to their original position.
				foreach(var item in childMeshRenderers)
				{
					item.meshRenderer.transform.localPosition = Vector3.Lerp(item.meshRenderer.transform.localPosition, item.originalPosition, explosionSpeed);
					// Once any one part of the telescope comes within .001 units distance of its original position, stop everything.
					// The tolerance is less for returning to the original position on purpose, as the higher tolerance for the exploded position results in
					// a smoother animation, while only a lower tolerance is necessary for everything appearing correct when being put back together.
					if(Vector3.Distance(item.meshRenderer.transform.localPosition, item.originalPosition) < 0.001f)
						isMoving = false;
				}
			}
		}
	}
	#endregion

	#region CustomFunctions
	public bool IsMoving()
	{
		return isMoving;
	}

	private void ToggleExplodedView()
	{
		isInExplodedView = !isInExplodedView;
		isMoving = true;
	}
	#endregion
}
