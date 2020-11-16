using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A script that holds information about the part it is
// attached to, including its name and description that
// are displayed when the part is highlighted and the x
// and y offsets of the part when exploding.
public class TelescopePartInfo : MonoBehaviour
{
	public String Name;
	public String Description;
	public float yOffset;
	public float xOffset;
}
