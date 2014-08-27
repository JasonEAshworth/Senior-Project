using UnityEngine;
using System.Collections;
using System;

[RequireComponent (typeof(CharacterController))]

public class cameraControl : MonoBehaviour
{
		private bool isOrthographic;
		public GameObject[] targets;
		public float currentDistance;
		public float largestDistance;
		public Camera theCamera;
		public float height = 5.0f;
		public float avgDistance;
		public float distance = 5.0f;                    // Default Distance 
		public float speed = 1;
		public float offset;

		public void Start ()
		{
			Console.WriteLine ("START");
			targets = GameObject.FindGameObjectsWithTag ("Player");
			if (theCamera) {
				isOrthographic = theCamera.orthographic;
			}
		}

		public void OnGUI ()
		{
				GUILayout.Label ("largest distance is = " + largestDistance.ToString ());
				GUILayout.Label ("height = " + height.ToString ());
				GUILayout.Label ("number of players = " + targets.Length.ToString ());
		}

		public void LateUpdate ()
		{
				targets = GameObject.FindGameObjectsWithTag ("Player");
				if (!GameObject.FindWithTag ("Player")) {
						return;
				}
				Vector3 sum = new Vector3 (0, 0, 0);
				for (int i = 0; i < targets.Length; i++) {
						sum += targets [i].transform.position;
				}

				Vector3 avgDistance = sum / targets.Length;

				float largestDifference = returnLargestDifference ();

				distance = Mathf.Lerp (height, largestDifference, 0.5f);

				if (largestDifference < 8) {
						distance = 8;
						Vector3 temp = new Vector3 (theCamera.transform.position.x, 10, theCamera.transform.position.z);
						theCamera.transform.position = temp;
				}

				if (largestDifference > 8) {
						distance = 8;
				}

				float tempX = avgDistance.x - 2;
				float tempZ = avgDistance.z + distance + 0.5f * (largestDifference);
				float tempY = distance;
				theCamera.transform.position = new Vector3 (tempX, tempY, tempZ);
				theCamera.transform.LookAt (avgDistance);
		}

		public float returnLargestDifference ()
		{
				currentDistance = 0.0f;
				largestDistance = 0.0f;
				for (int i = 0; i < targets.Length; i++) {
						for (int j = 0; j < targets.Length; j++) {
								currentDistance = Vector3.Distance (targets [i].transform.position, targets [j].transform.position);
								if (currentDistance > largestDistance) {
										largestDistance = currentDistance;
								}
						}
				}
				return largestDistance;
		}
}
