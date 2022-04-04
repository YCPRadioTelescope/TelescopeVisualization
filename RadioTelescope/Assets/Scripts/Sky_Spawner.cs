using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sky_Spawner : MonoBehaviour
{
    public GameObject sky_interaction_object;
    public GameObject star_system;
	public TextAsset Sky_Data;


	private void Start()
    {
		//Run the CSV only once at the start of the program, this creates all the interactable objects from the program
		read_CSV();
    }

	public void read_CSV()
	{
		// Split the Sky_Data file into each line.
		string[] lines = Sky_Data.text.Split('\n');
		// For each Line, create the interactable objects(Triangles)
		for (int i = 0; i < lines.Length; i++)
		{
			// Split each line by the commas.
			string[] components = lines[i].Split(',');

			//Get the RA, DEC, Dist, and Label from the CSV for each line
			float RA = float.Parse(components[0]);
			float DEC = float.Parse(components[1]);
			float Dist = float.Parse(components[2]);
			//If the Distance is entered as 0, set to default of 500
			if(Dist == 0)
            {
				Dist = 500f;
            }
			string label = components[3];
			string desc = components[4];
			string image = components[5];

			Vector3 position = PolarToCartesian(RA, DEC, Dist);
			position = Vector3.Normalize(position) * 900;
			GameObject sky_interaction_clone = Instantiate(sky_interaction_object, position, Quaternion.identity);
			sky_interaction_clone.gameObject.transform.SetParent(star_system.transform);
			Fill_Data(sky_interaction_clone, RA.ToString(), DEC.ToString(), label, desc);


			string filepath = "Sky_Interaction_Data/" + image;
			Texture2D tex = Resources.Load(filepath) as Texture2D;

			Debug.Log("Data from" + label + "RA:" + RA + ", DEC:" + DEC + ", Dist:" + Dist + ", Desc:" + desc + ", ImageK:" + image);
		}
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

    public void Fill_Data(GameObject star_interaction, string RA, string DEC, string label, string desc)
    {
		star_interaction.gameObject.GetComponent<Star_Object>().RA = RA;
		star_interaction.gameObject.GetComponent<Star_Object>().DEC = DEC;
		star_interaction.gameObject.GetComponent<Star_Object>().Label = label;
		star_interaction.gameObject.GetComponent<Star_Object>().description = desc;
	}
}
