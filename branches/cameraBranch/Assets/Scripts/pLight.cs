using UnityEngine;
using System.Collections;

public class pLight : MonoBehaviour {
	
	private Transform target;
	private Vector3	lgt;
	
	
	void Start () {
		target = GameObject.FindGameObjectWithTag("Player").transform;
	
		
	}
	
	// Update is called once per frame
	void Update () {
		lgt = new Vector3( target.position.x , transform.position.y,  target.position.z);
		transform.position = Vector3.Lerp(transform.position,lgt, Time.deltaTime *8);
		transform.LookAt(target.transform);
	}
}
