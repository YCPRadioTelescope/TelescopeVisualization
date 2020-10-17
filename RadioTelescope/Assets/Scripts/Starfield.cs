using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Starfield : MonoBehaviour
{
	public ParticleSystem particleSystem;
	public int maxParticles = 1000;
	public TextAsset starCSV;
	
	void Awake()
	{
		particleSystem = GetComponent<ParticleSystem>();
		var main = particleSystem.main;
		main.maxParticles = maxParticles;
		ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[1];
		bursts[0].minCount = (short)maxParticles;
		bursts[0].maxCount = (short)maxParticles;
		bursts[0].time = 0.0f;
		particleSystem.emission.SetBursts(bursts, 1);
	}
	
	// Start is called before the first frame update
	void Start()
	{
	}
	
	// Update is called once per frame
	void Update()
	{
	}
	
	void LateUpdate()
	{	
		string[] lines = starCSV.text.Split('\n');
		ParticleSystem.Particle[] particleStars = new ParticleSystem.Particle[maxParticles];
		particleSystem.GetParticles(particleStars);
		for(int i = 0; i < maxParticles; i++)
		{
			string[] components = lines[i].Split(',');
			// Color.white * (1.0f – ((float.Parse(components[0]) + 1.44f) / 8));
			// 0 magnitude = 100x a 5 magnitude
			// 0 vs 1 = 2.5
			// Logarithmic
			// 0 = 255
			// 1 = 112
			// 2 = 42
			// 3 = 16
			// 4 = 7
			// Do a grid in the sky for right ascension and declination
			// Pie wedges going to the north star and south pole to give you a sense of direction
			float mag = float.Parse(components[0]);
			int mode = -3;
			// Make the stars twinkle (random +/- brightness)
			// Requirement: must be logarithmic
			if(mode == -3)
			{
				// Dynamic, 2 scaling
				if(mag < 0)
					mag = 0;
				mag = (float)Math.Pow(2, mag);
				particleStars[i].startColor = new Color(1, 1, 1, 1 / mag);
			}
			else if(mode == -2)
			{
				// 5 buckets, sqrt(2) scaling
				if(mag < 1.5)
					particleStars[i].startColor = new Color32(255, 255, 255, 255);
				else if(mag < 2.5)
					particleStars[i].startColor = new Color32(255, 255, 255, 128);
				else if(mag < 3.5)
					particleStars[i].startColor = new Color32(255, 255, 255, 90);
				else if(mag < 4.5)
					particleStars[i].startColor = new Color32(255, 255, 255, 64);
				else
					particleStars[i].startColor = new Color32(255, 255, 255, 45);
			}
			else if(mode == -1)
			{
				// Dynamic, sqrt(2) scaling
				if(mag < 0)
					mag = 0;
				mag = (float)Math.Pow(Math.Sqrt(2), mag);
				particleStars[i].startColor = new Color(1, 1, 1, 1 / mag);
			}
			else if(mode == 0)
			{
				// Dynamic, 1/mag scaling
				if(mag < 1)
					mag = 1;
				if(mag > 5)
					mag = 5;
				particleStars[i].startColor = new Color(1, 1, 1, 1 / mag);
			}
			else if(mode == 1)
			{
				// Darkest stars half max
				if(mag < 0.5)
					particleStars[i].startColor = new Color32(255, 255, 255, 255);
				else if(mag < 1.5)
					particleStars[i].startColor = new Color32(255, 255, 255, 212);
				else if(mag < 2.5)
					particleStars[i].startColor = new Color32(255, 255, 255, 177);
				else if(mag < 3.5)
					particleStars[i].startColor = new Color32(255, 255, 255, 146);
				else if(mag >= 3.5)
					particleStars[i].startColor = new Color32(255, 255, 255, 123);
			}
			else if(mode == 2)
			{
				// Original suggestion; 100^(1/5) scaling(?)
				if(mag < 0.5)
					particleStars[i].startColor = new Color32(255, 255, 255, 255);
				else if(mag < 1.5)
					particleStars[i].startColor = new Color32(255, 255, 255, 112);
				else if(mag < 2.5)
					particleStars[i].startColor = new Color32(255, 255, 255, 42);
				else if(mag < 3.5)
					particleStars[i].startColor = new Color32(255, 255, 255, 16);
				else if(mag >= 3.5)
					particleStars[i].startColor = new Color32(255, 255, 255, 7);
			}
			else
				// No scaling
				particleStars[i].startColor = new Color32(255, 255, 255, 255);
			Vector3 starPosition = new Vector3( float.Parse(components[1]),
												float.Parse(components[3]),
												float.Parse(components[2]));
			particleStars[i].position = Vector3.Normalize(starPosition) * 900;
			particleStars[i].remainingLifetime = Mathf.Infinity;
		}
		particleSystem.SetParticles(particleStars, maxParticles);
	}
}
