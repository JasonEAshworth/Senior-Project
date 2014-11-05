using UnityEngine;
using System.Collections;

public class FlameTrap : MonoBehaviour {

	private bool isFiring = false;
	
	void Update()
	{
		if(!isFiring)
		{
			StartCoroutine(Fireball());
		}
	}
	
	IEnumerator Fireball()
	{
		isFiring = true;
		yield return StartCoroutine(Wait());

		ParticleSystem p = gameObject.GetComponent<ParticleSystem> ();
		Debug.Log (p);
		p.Play ();

		isFiring = false;
	}
	
	IEnumerator Wait()
	{
		yield return new WaitForSeconds (3.0f);
	}
}
