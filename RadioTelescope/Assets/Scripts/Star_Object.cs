using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRTK.Prefabs.CameraRig.UnityXRCameraRig.Input;

public class Star_Object : MonoBehaviour
{

    public GameObject start;
    public GameObject end;

    public GameObject Canvus_Object;
    public GameObject Canvus_Object_VR;

    public GameObject playerMK;
    public GameObject playerVR;
    private GameObject player;
    public Animator animator;
    public bool is_hovered;

    private RaycastHit hitInfo;
    private LineRenderer lr;
    public bool vrActive;

    //Variables for UI 
    public List<Star_collection> star_Collections;

    public UnityAxis1DAction rightTrigger;

    private void Start()
    {
        if (playerMK.activeSelf)
        {
            player = playerMK;
        }
        else
        {
            player = playerVR;
        }
        animator = transform.GetComponent<Animator>();

        if (vrActive)
            lr = this.transform.GetComponent<LineRenderer>();
    }
    // Update is called once per frame
    void Update()
    {
        //Make the object face the telescope
        Vector3 target = new Vector3(player.transform.position.x, player.transform.position.y, player.transform.position.z);
        transform.LookAt(target, Vector3.up);

        //Make the object move when hovered
        if (is_hovered)
        {
            animator.SetBool("is_selected", true);
            if(Input.GetMouseButtonDown(0) || rightTrigger.IsActivated)
            {
                animator.SetTrigger("clicked");
                SetTextandImage(Canvus_Object, star_Collections[0]);
                SetTextandImage(Canvus_Object_VR, star_Collections[0]);
            }
        }
        else
        {
                animator.SetBool("is_selected", false);
        }
    }

    public void SetTextandImage(GameObject Canvus_Object, Star_collection star_collection)
    {
        Transform label = Canvus_Object.transform.Find("Label");
        Transform desc = Canvus_Object.transform.Find("Description");
        Transform tex = Canvus_Object.transform.Find("Image");
        Transform ra = Canvus_Object.transform.Find("RA");
        Transform dec = Canvus_Object.transform.Find("DEC");

        label.GetComponent<Text>().text = star_collection.Label;
        desc.GetComponent<Text>().text = star_collection.description;
        tex.GetComponent<RawImage>().texture = star_collection.image;
        ra.GetComponent<Text>().text = "RA: " + star_collection.RA;
        dec.GetComponent<Text>().text= "DEC: " + star_collection.DEC;
    }
    public void AddtoCollections(string RA, string DEC, string label, string desc, Texture2D tex)
    {
        Star_collection star_add = gameObject.AddComponent<Star_collection>();
        star_add.constructor(RA, DEC, label, desc, tex);
        star_Collections.Add(star_add);
    }
}


