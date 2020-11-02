using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script controls the creation and movement of the stars in the sky.
public class Starfield : MonoBehaviour
{
	public ParticleSystem particleSystem;
	public int maxParticles;
	public TextAsset starCSV;
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
		// Rotate the particleSystem about the global axis, simulating the latitude of the observer.
		// TODO: Accurately rotate the particleSystem to position the stars as they are in person.
		// TODO: Figure out the angles to rotate the particleSystem by given the time of year.
		particleSystem.transform.Rotate(0.0f, 0.0f, 0.0f, Space.World);
		rotationalAxis = new Vector3(0.0f, 1.0f, 0.0f);
	}
	
	// Update is called once per frame.
	void Update()
	{
		// Rotate the sky about the Earth's axis at the given rotational speed.
		// This rotates the particleSystem relative to itself.
		particleSystem.transform.Rotate(rotationalAxis, RotationalSpeed(24.0f, 60.0f), Space.Self);
	}
	
	// A function to compute the angle per frame necessary to cause the
	// given number of hours to elapse in the given number of seconds.
	float RotationalSpeed(float hours, float seconds)
	{
		return 360.0f / (24.0f / hours) / seconds / 60.0f;
	}
	
	// LastUpdate is called once per frame after every Update function has been called.
	void LateUpdate()
	{
		// Split the starCSV file into each line.
		string[] lines = starCSV.text.Split('\n');
		// Create maxParticles particles and initalize them.
		ParticleSystem.Particle[] particleStars = new ParticleSystem.Particle[maxParticles];
		particleSystem.GetParticles(particleStars);
		for(int i = 0; i < maxParticles; i++)
		{
			// Split each line by the commas.
			string[] components = lines[i].Split(',');
			
			// The first component of each line is the magnitude.
			float mag = float.Parse(components[0]);
			
			// 
			int mode = 0;
			if(mode == 0)
			{
				// Static 5 bucket brightness with log base 2 scaling.
				if(mag < 1.5)
					particleStars[i].startColor = new Color32(255, 255, 255, 255);
				else if(mag < 2.5)
					particleStars[i].startColor = new Color32(255, 255, 255, 128);
				else if(mag < 3.5)
					particleStars[i].startColor = new Color32(255, 255, 255, 64);
				else if(mag < 4.5)
					particleStars[i].startColor = new Color32(255, 255, 255, 32);
				else
					particleStars[i].startColor = new Color32(255, 255, 255, 16);
			}
			else
			{
				// Dynamic brightness with log base 2 scaling.
				if(mag < 0)
					mag = 0;
				mag = (float)Math.Pow(2, mag);
				particleStars[i].startColor = new Color(1, 1, 1, 1 / mag);
			}
			// Create a vector using the X, Y, and Z coordinates of the star.
			// Compontents 2 and 3 are intentionally switched.
			Vector3 starPosition = new Vector3( float.Parse(components[1]),
												float.Parse(components[3]),
												float.Parse(components[2]));
			
			// Normalize the vector to a length of one, then move it out to
			// 900 units from the particle system. This puts the stars just
			// before the far clipping plane of the camera.
			particleStars[i].position = Vector3.Normalize(starPosition) * 900;
			// Particles only exist for one frame before a new one is created.
			particleStars[i].remainingLifetime = 1;
		}
		particleSystem.SetParticles(particleStars, maxParticles);
	}
}
