using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sky_Spawner : MonoBehaviour
{
    public GameObject sky_interaction_object;
    public GameObject star_system;

    public float RA = 38;
    public float DEC = 89;

    private void Start()
    {
        //PolarToCartesian(37.95f, 89.26f, 132)
        //Vector3 position = new Vector3(1.3431f, 132, 1);
        Vector3 position = PolarToCartesian(37.95f, 89.26f, 132);
        position = Vector3.Normalize(position) * 900;
        GameObject sky_interaction_clone = Instantiate(sky_interaction_object, position, Quaternion.identity);
        sky_interaction_clone.gameObject.transform.SetParent(star_system.transform);
    }

    
    Vector3 PolarToCartesian(float RA, float DEC, float D)
    {
        RA = RA * (Mathf.PI / 180);
        DEC = DEC * (Mathf.PI / 180);
        //y and z are flipped

        var x = D * Mathf.Cos(DEC) * Mathf.Cos(RA);
        var y = D * Mathf.Sin(DEC);
        var z = D * Mathf.Cos(DEC) * Mathf.Sin(RA);

        return new Vector3(x, y, z);
    }
}
