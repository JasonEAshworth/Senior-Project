using UnityEngine;
using System.Collections;
using System;

[RequireComponent (typeof(CharacterController))]

public class cameraControl : MonoBehaviour
{
    private bool isOrthographic;
    public GameObject[] targets;
    public GameObject avgBox;
    public float currentDistance;
    public float largestDistance;
    public Camera theCamera;
    public float height = 5.0f;
    public Vector3 avgDistance;
    public float distance = 5.0f;                    // Default Distance 
    public float speed = 1;
    public float offset;
    public float roomLength = 48f;
    private float roomDiagonal;
    public float xRatio = 0f;
    public float yzRatio = 0f;
    public float offZRatio = 0f;
    public float offXRatio = 0f;
    public float[] xClamp = {4,-4};
    public float[] yClamp = {2,37};
    public float[] zClamp = {0,26};
    public float[] offsetClamp = {0, 7, 0, 0};
	public bool dubugText = false;

    public void Start ()
    {
        Console.WriteLine ("START");
        targets = GameObject.FindGameObjectsWithTag ("Player");
        // Square diagonal = Sqrt(2) * Length 
        roomDiagonal = 1.414f * roomLength;
        if (theCamera) {
            isOrthographic = theCamera.orthographic;
        }
    }

    public void OnGUI ()
    {
		if (dubugText) 
		{
	        GUILayout.Label ("largest distance is = " + largestDistance.ToString ());
	        GUILayout.Label ("height = " + height.ToString ());
	        GUILayout.Label ("number of players = " + targets.Length.ToString ());
	        GUILayout.Label ("xRatio = " + xRatio.ToString ());
	        GUILayout.Label ("yzRatio = " + yzRatio.ToString ());
	        GUILayout.Label ("offRatio = " + offZRatio.ToString ());
	        GUILayout.Label ("offRatio = " + offXRatio.ToString ());
			GUILayout.Label ("Average Distance = " + avgDistance.x.ToString () + ", " + avgDistance.y.ToString () + ", " + avgDistance.z.ToString ());
		}
	}

    public void LateUpdate ()
    {
        if (!GameObject.FindWithTag ("Player")) {
            return;
        }
        Vector3 sum = new Vector3 (0, 0, 0);
        for (int i = 0; i < targets.Length; i++) {
            sum += targets [i].transform.position;
        }
        
        avgDistance = sum / targets.Length;
        
        float largestDifference = returnLargestDifference ();
        // New code work area
                
        xRatio = (avgDistance.x / roomLength) + .5f;
        float tempX = avgDistance.x + Mathf.Lerp (xClamp [0], xClamp [1], xRatio);

        //float yzRatio = largestDistance / roomDiagonal;
        yzRatio = largestDistance / roomLength;
        float tempY = Mathf.Lerp (yClamp [0], yClamp [1], yzRatio);
        float tempZ = avgDistance.z + 5 + Mathf.Lerp (zClamp [0], zClamp [1], yzRatio);

        offZRatio = (avgDistance.z / roomLength) + .5f;
        float lookZOffset = Mathf.Lerp (offsetClamp [0], offsetClamp [1], offZRatio);
        float lookXOffset = Mathf.Lerp (offsetClamp [2], offsetClamp [3], xRatio);
                
        // Old code
        
        theCamera.transform.position = new Vector3 (tempX, tempY, tempZ);
        theCamera.transform.LookAt (new Vector3 ((avgDistance.x + lookXOffset), avgDistance.y, (avgDistance.z + lookZOffset)));        
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


/* OLD CODE
 * targets = GameObject.FindGameObjectsWithTag ("Player");
        if (!GameObject.FindWithTag ("Player")) {
            return;
        }
        Vector3 sum = new Vector3 (0, 0, 0);
        for (int i = 0; i < targets.Length; i++) {
            sum += targets [i].transform.position;
        }

        avgDistance = sum / targets.Length;

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
        */