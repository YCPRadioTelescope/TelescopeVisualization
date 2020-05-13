//Coded by Abishek J Reuben


using UnityEngine;

using System.Collections.Generic;

using System;


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


    #endregion


    #region UnityFunctions


    private void Awake()

    {

        childMeshRenderers = new List<SubMeshes>();


        foreach (var item in GetComponentsInChildren<MeshRenderer>())

        {

            SubMeshes mesh = new SubMeshes();

            mesh.meshRenderer = item;

            mesh.originalPosition = item.transform.localPosition;

            mesh.explodedPosition = ((item.bounds.center - this.transform.position) * 1.5f) + this.transform.position;

            childMeshRenderers.Add(mesh);

        }

    }


    private void Update()

    {
        if (Input.GetButtonDown("Toggle Exploded View"))
        {
            ToggleExplodedView();
            
        }

        if (isMoving)

        {

            if (isInExplodedView)

            {
                Debug.Log("Exploding");
                foreach (var item in childMeshRenderers)

                {

                    item.meshRenderer.transform.position = Vector3.Lerp(item.meshRenderer.transform.position, item.explodedPosition, explosionSpeed);


                    if (Vector3.Distance(item.meshRenderer.transform.position, item.explodedPosition) < 0.001f)

                    {

                        isMoving = false;

                    }

                }

            }

            else

            {

                foreach (var item in childMeshRenderers)

                {

                    item.meshRenderer.transform.localPosition = Vector3.Lerp(item.meshRenderer.transform.localPosition, item.originalPosition, explosionSpeed);


                    if (Vector3.Distance(item.meshRenderer.transform.localPosition, item.originalPosition) < 0.001f)

                    {

                        isMoving = false;

                    }

                }

            }

        }

    }


    #endregion


    #region CustomFunctions


    public void ToggleExplodedView()

    {

        if (isInExplodedView)

        {
            isInExplodedView = false;

            isMoving = true;

        }

        else

        {

            isInExplodedView = true;

            isMoving = true;

        }

    }


    #endregion

}