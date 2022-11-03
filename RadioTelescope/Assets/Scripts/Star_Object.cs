using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public bool is_focused;

    private RaycastHit hitInfo;
    private LineRenderer lr;
    public bool vrActive;
    private int StarObjectIterator = 0;

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
                setArrows();
                SetTextandImage(Canvus_Object, star_Collections[StarObjectIterator]);
                SetTextandImage(Canvus_Object_VR, star_Collections[StarObjectIterator]);
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
        Transform date = Canvus_Object.transform.Find("Date");

        if (label != null) { label.GetComponent<Text>().text = star_collection.Label; }
        if (desc != null) { desc.GetComponent<Text>().text = star_collection.description; }
        if (tex != null) { tex.GetComponent<RawImage>().texture = star_collection.image; }
        if (ra != null) { ra.GetComponent<Text>().text = "RA: " + star_collection.RA; }
        if (dec != null) { dec.GetComponent<Text>().text = "DEC: " + star_collection.DEC; }
        if(date != null) { date.GetComponent<Text>().text = star_collection.Date_text; }

    }
    public void AddtoCollections(string RA, string DEC, string label, string desc, Texture2D tex, string date)
    {
        DateTime newdate;
        Star_collection star_add = gameObject.AddComponent<Star_collection>();
        try
        {
            newdate = System.DateTime.Parse(date);
        }
        catch
        {
            //Default time if time is incorrectly entered
            newdate = System.DateTime.Parse("Jan 01, 2000");
        }
        star_add.constructor(RA, DEC, label, desc, tex, newdate);
        star_Collections.Add(star_add);
    }

    public void SortCollectionByDate()
    {
        star_Collections.Sort((x, y) => DateTime.Compare(x.Date, y.Date));
    }

    public void AddToIterator(int amount)
    {
        if(StarObjectIterator + amount < star_Collections.Count)
        {
            StarObjectIterator = StarObjectIterator + amount;
            SetTextandImage(Canvus_Object, star_Collections[StarObjectIterator]);
            SetTextandImage(Canvus_Object_VR, star_Collections[StarObjectIterator]);
        }
        else if(StarObjectIterator + amount >= star_Collections.Count - 1)
        {
            StarObjectIterator = star_Collections.Count - 1;
            SetTextandImage(Canvus_Object, star_Collections[StarObjectIterator]);
            SetTextandImage(Canvus_Object_VR, star_Collections[StarObjectIterator]);
        }
        setArrows();
    }

    public void SubtractfromIterator(int amount)
    {
        if(StarObjectIterator - amount > 0 )
        {
            StarObjectIterator = StarObjectIterator - amount;
            SetTextandImage(Canvus_Object, star_Collections[StarObjectIterator]);
            SetTextandImage(Canvus_Object_VR, star_Collections[StarObjectIterator]);
        }
        else if (StarObjectIterator - amount <= 0)
        {
            StarObjectIterator = 0;
            SetTextandImage(Canvus_Object, star_Collections[StarObjectIterator]);
            SetTextandImage(Canvus_Object_VR, star_Collections[StarObjectIterator]);
        }
        setArrows();
    }

    public void setArrows()
    {
        Transform leftarrow = Canvus_Object.transform.Find("LeftArrow");
        Transform rightarrow = Canvus_Object.transform.Find("RightArrow");
        if (StarObjectIterator == star_Collections.Count - 1 || star_Collections.Count == 1)
        {
            rightarrow.GetComponent<CanvasGroup>().alpha = 0.3f;
        }
        else
        {
            rightarrow.GetComponent<CanvasGroup>().alpha = 1f;
        }
        if (StarObjectIterator == 0)
        {
            leftarrow.GetComponent<CanvasGroup>().alpha = 0.3f;
        }
        else
        {
            leftarrow.GetComponent<CanvasGroup>().alpha = 1.0f;
        }
    }
}


