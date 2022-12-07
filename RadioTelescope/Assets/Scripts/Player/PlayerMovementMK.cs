using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script controls the players movement.
public class PlayerMovementMK : MonoBehaviour
{
	public CharacterController controller;

	public Vector3 velocity;
	public float speed = 12f;
	public float gravity = -9.81f;
	public float jumpHeight = 3f;

	public bool fly = false;
	public float flySpeedVertical = 5f;

	public Transform groundCheck;
	public LayerMask groundMask;
	public float groundDistance = 0.4f;


	// Update is called once per frame
	void Update()
	{
		// Toggle flight if Q is pressed.
		if (Input.GetButtonDown("Toggle Fly"))
			fly = !fly;

		// Get the status of the WASD keys and move the player's X and Z location according to that.
		float x = Input.GetAxis("Horizontal");
		float z = Input.GetAxis("Vertical");
		Vector3 move = transform.right * x + transform.forward * z;
		controller.Move(move * speed * Time.deltaTime);

		if (fly)
		{
			// If the player is in flight mode, Space and E simply move the player up and down.
			// When neither is pressed, no gravity is applied.
			if (Input.GetButton("Jump"))
				velocity.y = flySpeedVertical;
			else if (Input.GetButton("Fly Down"))
				velocity.y = -flySpeedVertical;
			else
				velocity.y = 0;
		}
		else
		{
			// If the player is not in flight mode, they can only jump while grounded.
			if (Input.GetButtonDown("Jump") && Physics.CheckSphere(groundCheck.position, groundDistance, groundMask))
				velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
			else
				// Gravity is only applied if the player isn't grounded.
				// Gravity is acceleration, so it must be multiplied by Time.deltaTime twice.
				// (Once here and once when velocity is finally applied.)
				velocity.y += gravity * Time.deltaTime;
		}
		// Move the player's Y location according to their flight velocity or their jump and gravity.
		controller.Move(velocity * Time.deltaTime);
	}
}
