using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Sky_Spawner : MonoBehaviour
{
    public GameObject sky_interaction_object;
    public GameObject star_system;
	private List<System.Tuple<Star_collection, GameObject>> created_star_interactions;
	string FileContent = File.ReadAllText(Application.streamingAssetsPath + "/Sky_Interaction_Data/Sky_Data.csv");
	string imageFilepath = "";
	TextAsset Sky_Data;

	private void Start()
    {
		created_star_interactions = new List<System.Tuple<Star_collection, GameObject>>();
		Sky_Data = new TextAsset(FileContent);
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
			if (lines[i] != "")
			{
				// Split each line by the commas.
				string[] components = lines[i].Split(',');

				//Get the RA, DEC, Dist, and Label from the CSV for each line
				float RA_Hours = float.Parse(components[0]);
				float RA_Minutes = float.Parse(components[1]);
				float RA = RATimeToDegrees(RA_Hours, RA_Minutes);
				float DEC = float.Parse(components[2]);
				float Dist = float.Parse(components[3]);
				//If the Distance is entered as 0, set to default of 500
				if (Dist == 0)
				{
					Dist = 500f;
				}
				string label = components[4];
				string desc = components[5];
				string image_name = components[6];
				string date = components[7];
				image_name = image_name.Replace("\n", "").Replace("\r", "");
				Vector3 position = PolarToCartesian(RA, DEC, Dist);
				position = Vector3.Normalize(position) * 900;

				try { 
					imageFilepath = Application.streamingAssetsPath + "/Sky_Interaction_Data/" + image_name + ".jpg";
					byte[] pngBytes = System.IO.File.ReadAllBytes(imageFilepath);
				}
				catch {
					Debug.Log("Could not find " + image_name + " as a .jpg, trying png");
					try { 
						imageFilepath = Application.streamingAssetsPath + "/Sky_Interaction_Data/" + image_name + ".png";
						byte[] pngBytes = System.IO.File.ReadAllBytes(imageFilepath);
					}
					catch { 
						Debug.Log("Could not find " + image_name + " as a .png either" );
						imageFilepath = null;
					}
				}

				Texture2D new_tex = new Texture2D(128, 128);
				if (imageFilepath != null)
                {
					byte[] pngBytes = System.IO.File.ReadAllBytes(imageFilepath);
					new_tex.LoadImage(pngBytes);
				}

				GameObject source = isin_created_interactions(RA, DEC);
				if (source == null)
				{
					//Create new point
					GameObject sky_interaction_clone = Instantiate(sky_interaction_object, position, Quaternion.identity);
					sky_interaction_clone.gameObject.transform.SetParent(star_system.transform);

					Star_collection star_add = gameObject.AddComponent<Star_collection>();
					star_add.constructor(RA.ToString(), DEC.ToString(), label, desc, new_tex, System.DateTime.Parse(date));
					created_star_interactions.Add(new System.Tuple<Star_collection, GameObject>(star_add, sky_interaction_clone));

					Fill_Data(sky_interaction_clone, RA.ToString(), DEC.ToString(), label, desc, new_tex, date);
					//
				}
				else
				{
					Fill_Data(source, RA.ToString(), DEC.ToString(), label, desc, new_tex, date);
				}
			}
		}

		foreach (System.Tuple<Star_collection, GameObject> x in created_star_interactions)
		{
			x.Item2.GetComponent<Star_Object>().SortCollectionByDate();
		}
	}

	private GameObject isin_created_interactions(float RA, float DEC)
    {
		foreach(System.Tuple<Star_collection, GameObject> x in created_star_interactions)
        {
			if(Mathf.Abs(RA - float.Parse(x.Item1.RA)) < 1 && Mathf.Abs(DEC - float.Parse(x.Item1.DEC)) < 1)
            {
				return x.Item2;
            }
        }
		//Did not find another match
		return null;
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

	float RATimeToDegrees(float RA_Hours, float RA_Minutes)
    {
		float RA;
		RA = (RA_Hours + (RA_Minutes / 60)) * 15;
		return RA;
    }

    public void Fill_Data(GameObject star_interaction, string RA, string DEC, string label, string desc, Texture2D tex, string date)
    {
		star_interaction.GetComponent<Star_Object>().AddtoCollections(RA, DEC, label, desc, tex, date);
	}
}
