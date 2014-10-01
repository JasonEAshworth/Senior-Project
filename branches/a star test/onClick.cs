using UnityEngine;
using System.Collections;

public class onClick : MonoBehaviour {
	// 'Moving and Object towards a raycast', re-written in C4
	public float targetRadius = 1;
	public int speed = 1;
	private Vector3 target;


	// Use this for initialization
	void Start () {
		target = transform.position;
	}
	
	// Update is called once per frame
	public Vector3 Update () {
		target.y = transform.position.y;
		if(Input.GetButtonDown ("Fire1")){
			//Debug.Log("TEST");
			RaycastHit hit;
			Ray mRay = Camera.main.ScreenPointToRay(Input.mousePosition);
			if(Physics.Raycast(mRay, out hit)){
				target = hit.point;
				//Debug.Log("TEST2");
			}
		}
	// Maybe one day when im a Big Boy, ill LERP instead
		if (Vector3.Distance (transform.position, target) > targetRadius) {
			transform.LookAt (target);
			transform.position += transform.forward * speed * Time.deltaTime;
		}
		//Debug.Log (target);
		return target;
	}

}