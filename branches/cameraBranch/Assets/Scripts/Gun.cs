using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour {

	public enum GunType {Semi, Burst, Auto};

	public GunType gunType;

	public Transform spawn;

	public void Shoot()
	{
		Ray ray = new Ray(spawn.position,spawn.forward);
		RaycastHit hit;

		float ShotDist = 20.2f;

		if(Physics.Raycast(ray,out hit,ShotDist)){
		   ShotDist = hit.distance;

		}

		Debug.DrawRay(ray.origin, ray.direction * ShotDist,Color.red, 1);
	}

	public void ShootContinuous(){
		if (gunType == GunType.Auto)
		{
			Shoot();
		}
	}
}
