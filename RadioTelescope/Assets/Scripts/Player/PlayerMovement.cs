using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public float speed = 12f;
    Vector3 velocity;
    public float gravity = -9.81f;
    public float jumpHeight = 3f;

    public float FlySpeedVertical = 5f;

    public Transform GroundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    public bool fly;

    bool isGrounded;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Toggle Fly"))
        {
            if (fly == true)
            {
                fly = false;
            } else
            {
                fly = true;
            }
        }
        
        isGrounded = Physics.CheckSphere(GroundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        controller.Move(move * speed * Time.deltaTime);

        if (fly)
        {
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
            velocity.y += gravity * Time.deltaTime;

            
        }
        else
        {
            if (Input.GetButton("Jump"))
            {
                velocity.y = FlySpeedVertical;
            } else if (Input.GetButton("Fly Down")) {
                velocity.y = -FlySpeedVertical;
            }
            else
            {
                velocity.y = 0;
            }
        }
        controller.Move(velocity * Time.deltaTime);
    }
}
