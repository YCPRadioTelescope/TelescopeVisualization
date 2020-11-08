using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A script that simply holds a name and description for the object
// that it is associated with. This name and description is then
/// displayed to the player if the associated game object is highlighted.
public class ObjectDesc : MonoBehaviour
{
	public String Name;
	public String Description;
	public float yOffset;
	public float xOffset;
}
