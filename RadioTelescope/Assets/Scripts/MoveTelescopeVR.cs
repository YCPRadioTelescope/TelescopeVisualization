using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zinnia.Extension;

public class MoveTelescopeVR : MonoBehaviour
{
	public GameObject telescope;
	public TelescopeController tc;
	public GameObject joystick;
	private Vector3 _rotation;

	// Start is called before the first frame update
	void Start()
	{
		tc = telescope.GetComponent<TelescopeController>();
	}

	// Update is called once per frame
	void Update()
	{
		_rotation = joystick.TryGetEulerRotation(true);
		if (_rotation.x > 180)
		{
			_rotation.x -= 360;
		}

		if (_rotation.z > 190)
		{
			_rotation.z -= 360;
		}

		tc.RotateZ(_rotation.x * (float) 0.01);
		tc.RotateY(_rotation.z * (float) 0.01);
	}
}