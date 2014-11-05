using UnityEngine;
using System.Collections;

public class FBParticleBehavior : MonoBehaviour {

	private ParticleSystem ps;
	private ParticleSystem.Particle[] allParticles;
	private bool setpos = false;
	private float px, py, pz;

	void Start () {
		ps = gameObject.GetComponent <ParticleSystem>();
	}

	void Update () {
		allParticles = new ParticleSystem.Particle[ps.particleCount];
		ps.GetParticles (allParticles);

		if (setpos) {
			for (int i=0; i<allParticles.Length; i++) {	
				allParticles[i].position = new Vector3(px, py, pz);
			}
			ps.SetParticles (allParticles, ps.particleCount);
		}
	}

	void OnParticleCollision(GameObject go)
	{
		for (int i=0; i<allParticles.Length; i++) {
			/*float vx = allParticles[i].velocity.x;
			float vy = allParticles[i].velocity.y;
			float vz = allParticles[i].velocity.z;*/
			px = allParticles[i].position.x;
			py = allParticles[i].position.y;
			pz = allParticles[i].position.z;

			allParticles[i].position = new Vector3(px, py, pz);
			//allParticles[i].velocity = new Vector3(vx - 1.0f, vy - 1.0f, vz - 1.0f);
		}
		setpos = true;
		ps.SetParticles (allParticles, ps.particleCount);
	}
}
