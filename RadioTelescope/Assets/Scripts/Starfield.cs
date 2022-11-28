using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

// This script controls the creation and movement of the stars in the sky.
public class Starfield : MonoBehaviour
{
	public GameObject telescope;

	public int day;
	public int month;
	public int year;
	public int hours;
	public int minutes;
	public int seconds;

	public float number_hours = 3f;
	public float number_seconds = 60f;
	public int maxParticles;
	public ParticleSystem particleSystem;
	public TextAsset starCSV;
	public bool isReadCSV = false;
	
	private Vector3 rotationalAxis;
	
	// Awake is called on the initalization of all game objects.
	void Awake()
	{
		// Schedule a burst of maxParticles particles so that all
		// the stars are created at once.
		particleSystem = GetComponent<ParticleSystem>();
		var main = particleSystem.main;
		main.maxParticles = maxParticles;
		ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[1];
		bursts[0].minCount = (short)maxParticles;
		bursts[0].maxCount = (short)maxParticles;
		bursts[0].time = 0.0f;
		particleSystem.emission.SetBursts(bursts, 1);
	}
	
	// Start is called before the first frame update.
	void Start()
	{
		// Rotate the particleSystem about the global axis, simulating the
		// latitude of the observer.
		TelescopeInfo ti = telescope.GetComponent<TelescopeInfo>();
		particleSystem.transform.Rotate(90.0f - ti.latitude, 0.0f, 0.0f, Space.World);
		rotationalAxis = new Vector3(0.0f, 1.0f, 0.0f);

		//call position to set stars based on sidereal time
		set_star_position_from_date(day, month, year, hours, minutes, seconds);
	}
	
	// Update is called once per frame.
	void Update()
	{
		// Rotate the sky about the Earth's axis at the given rotational speed.
		// This rotates the particleSystem relative to itself.
		particleSystem.transform.Rotate(rotationalAxis, RotationalSpeed(number_hours, number_seconds), Space.Self);
	}
	
	// A function to compute the angle per frame necessary to cause the
	// given number of hours to elapse in the given number of seconds. *Changed 360 to 356 due to sidereal time.
	float RotationalSpeed(float hours, float seconds)
	{
		return 356.0f / (24.0f / hours) / seconds * Time.deltaTime;
	}

	//read CSV only works on late update, not sure why
    private void LateUpdate()
    {
		//checks if the stars have been read by the csv, if not, readCSV is called
		if (!isReadCSV)
		{
			isReadCSV = true;
			read_CSV();
		}
	}

    //Reads the CSV file and inputs the star information into the particle system, this should only be called once
    public void read_CSV()
    {
		// Split the starCSV file into each line.
		string[] lines = starCSV.text.Split('\n');
		// Create maxParticles particles and initalize them.
		ParticleSystem.Particle[] particleStars = new ParticleSystem.Particle[maxParticles];
		particleSystem.GetParticles(particleStars);
		for (int i = 0; i < maxParticles; i++)
		{
			// Split each line by the commas.
			string[] components = lines[i].Split(',');

			// The first component of each line is the magnitude.
			float mag = float.Parse(components[0]);

			// Change the alpha value of this star given its magnitude.
			int mode = 0;
			if (mode == 0)
			{
				// Static 5 bucket brightness with log base 2 scaling.
				if (mag < 1.5)
					particleStars[i].startColor = new Color32(255, 255, 255, 255);
				else if (mag < 2.5)
					particleStars[i].startColor = new Color32(255, 255, 255, 128);
				else if (mag < 3.5)
					particleStars[i].startColor = new Color32(255, 255, 255, 64);
				else if (mag < 4.5)
					particleStars[i].startColor = new Color32(255, 255, 255, 32);
				else
					particleStars[i].startColor = new Color32(255, 255, 255, 16);
			}
			else
			{
				// Dynamic brightness with log base 2 scaling.
				if (mag < 0)
					mag = 0;
				mag = (float)Math.Pow(2, mag);
				particleStars[i].startColor = new Color(1, 1, 1, 1 / mag);
			}
			// Create a vector using the X, Y, and Z coordinates of the star.
			// Compontents 2 and 3 are intentionally switched.
			Vector3 starPosition = new Vector3(float.Parse(components[1]), float.Parse(components[3]), float.Parse(components[2]));

			// Normalize the vector to a length of one, then move it out to
			// 900 units from the particle system. This puts the stars just
			// before the far clipping plane of the camera, which is at 1000.
			particleStars[i].position = Vector3.Normalize(starPosition) * 900;
		}
		particleSystem.SetParticles(particleStars, maxParticles);
	}

	public void set_star_position_from_date(int day, int month, int year, int hours, int minutes, int seconds)
    {
		//NOTE THAT THIS FUNCTION HAS AN ERROR OF BETWEEN 0-2 degrees, this is unnoticible in game
		DateTime equinox = new DateTime(2022, 3, 20, 15, 33, 00, DateTimeKind.Utc);
		DateTime newDate = new DateTime(year, month, day, hours, minutes, seconds, DateTimeKind.Utc);
		Quaternion rotate = Quaternion.Euler(-26.067f, 112.8f, 44.425f);
		particleSystem.transform.rotation = rotate;
		float minuteDifference = ((float)(newDate - equinox).TotalMinutes) % 262800;
		float angleDifference = (float)minuteDifference * 0.25068f; //this equals the angles of change 0.25068 degrees per minute


		//Distance between player and merak at vernal equinox of 2022 is 921 units. New rotation for starsystem is (-26.067f, 112.8f, 44.425f)
		//A degree change for the solar system is every 3.98 minutes, find minute distance, then change the angle from that. 

		particleSystem.transform.Rotate(rotationalAxis, (float)angleDifference, Space.Self);
	}
}
