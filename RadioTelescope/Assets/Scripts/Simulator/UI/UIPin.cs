using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Pins stuff to the corners
 * Set Position to the corner
 * 1 = upper left
 * 2 = bottom left
 * 3 = upper right
 * 4 = bottom right
 */
public class UIPin : MonoBehaviour
{
    
    public int bufferX;
    public int bufferY;
    public int position;
    
    // Start is called before the first frame update
    void FixedUpdate()
    {
        if (position == 1)
        {
            transform.position = new Vector3(0 + bufferX, Screen.height - bufferY, 0);
        }
        if (position == 2)
        {
            transform.position = new Vector3(0 + bufferX, 0 + bufferY, 0);
        }
        if (position == 3)
        {
            transform.position = new Vector3(Screen.width - bufferX, Screen.height - bufferY, 0);
        }
        if (position == 4)
        {
            transform.position = new Vector3(Screen.width - bufferX, 0 + bufferY, 0);
        }
    }

}