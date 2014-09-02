using UnityEngine;
using System.Collections;

public class LightFlicker : MonoBehaviour {
	
	public float minIntensity = 0.25f;
	public float maxIntensity = 0.75f;

	private float lerpMul;
	private float random = Random.Range(0.0f, 65535.0f);


	void Start()
	{
		lerpMul = Random.Range (2.0f, 2.5f);
	}
		
	void Update()
	{
		float noise = Mathf.PerlinNoise (random, Time.time);
		light.intensity = Mathf.Lerp (minIntensity, maxIntensity, noise);
	}
}
