﻿using UnityEngine;
using System.Collections;

public class LightFlicker : MonoBehaviour {
	
	public float minIntensity = 0.25f;
	public float maxIntensity = 0.75f;
	public float speed = 1.0f;
	private float random;
	void Start()
	{
		random = Random.Range(0.0f, 65535.0f);
	}
		
	void Update()
	{
		float noise = Mathf.PerlinNoise (random, Time.time * speed);
		light.intensity = Mathf.Lerp (minIntensity, maxIntensity, noise);
	}
}
