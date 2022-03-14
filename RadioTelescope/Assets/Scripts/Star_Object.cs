﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Star_Object : MonoBehaviour
{

    public GameObject start;
    public GameObject end;

    public GameObject playerMK;
    public GameObject playerVR;
    private GameObject player;
    public Animator animator;
    public bool is_hovered;

    private RaycastHit hitInfo;
    private LineRenderer lr;
    public bool vrActive;
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
            if(Input.GetMouseButtonDown(0))
            {
                animator.SetTrigger("clicked");
            }
        }
        else
        {
            animator.SetBool("is_selected", false);
        }
    }
}