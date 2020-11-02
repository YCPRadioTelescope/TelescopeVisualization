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
	bool isInExplodedView = false;
	public float explosionSpeed = 1.0f;
	bool isMoving = false;
	int delay;
	public UnityAxis1DAction leftTrigger;
	#endregion

	#region UnityFunctions
	// Awake is called on the initalization of all game objects.
	private void Awake()
	{
		// Create a list of all objects to explode and generate an exploded position for each.
		childMeshRenderers = new List<SubMeshes>();
		foreach(var item in GetComponentsInChildren<MeshRenderer>())
		{
			SubMeshes mesh = new SubMeshes();
			mesh.meshRenderer = item;
			mesh.originalPosition = item.transform.localPosition;
			mesh.explodedPosition = ((item.bounds.center - this.transform.position) * 1.5f) + this.transform.position;
			childMeshRenderers.Add(mesh);
		}
	}

	// Update is called once per frame.
	private void Update()
	{
		// Both R and the left trigger can activate the exploded view.
		if(delay == 0 && (Input.GetButtonDown("Toggle Exploded View") || leftTrigger.Value > 0.2f))
		{
			ToggleExplodedView();
			// The exploded view can only be toggled every 60 frames.
			// Otherwise, holding the left trigger would cause the
			// exploded view to rapidly toggle.
			delay = 60;
		}
		if(delay > 0)
			delay--;

		if(isMoving)
		{
			if(isInExplodedView)
			{
				Debug.Log("Exploding");
				// Move all objects to their exploded position.
				foreach(var item in childMeshRenderers)
				{
					item.meshRenderer.transform.position = Vector3.Lerp(item.meshRenderer.transform.position, item.explodedPosition, explosionSpeed);
					if(Vector3.Distance(item.meshRenderer.transform.position, item.explodedPosition) < 0.001f)
						isMoving = false;
				}
			}
			else
			{
				// Move all objects back to their original position.
				foreach(var item in childMeshRenderers)
				{
					item.meshRenderer.transform.localPosition = Vector3.Lerp(item.meshRenderer.transform.localPosition, item.originalPosition, explosionSpeed);
					if(Vector3.Distance(item.meshRenderer.transform.localPosition, item.originalPosition) < 0.001f)
						isMoving = false;
				}
			}
		}
	}
	#endregion

	#region CustomFunctions
	public void ToggleExplodedView()
	{
		isInExplodedView = !isInExplodedView;
		isMoving = true;
	}
	#endregion
}
