using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// This script can be accessed to update the sensors UI with
// the status of sensors.
public class Sensors : MonoBehaviour
{
	public TMP_Text elevation;
	
	// Update the elevation limit sensor UI.
	public void UpdateElevationSensor(string status)
	{
		elevation.text = "Elevation Limit: " + status;
	}
}
