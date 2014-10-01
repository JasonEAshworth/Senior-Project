using UnityEngine;
using System.Collections;

public class onStar : navMesh {

	public int movementSpeed = 2;
	public Transform dumbTarget;
	private Transform mTransform;

	// Use this for initialization
	void Awake() {
		mTransform = transform;
	}

	// Update is called once per frame
	void Update () {
		transform.LookAt (dumbTarget);
		mTransform.position += mTransform.forward * movementSpeed * Time.deltaTime;
	
	}
}
