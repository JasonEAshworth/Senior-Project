using UnityEngine;
using System.Collections;
using System;

[RequireComponent (typeof(CharacterController))]

public class cameraControl2 : MonoBehaviour
{	
	private bool isOrthographic;

    public GameObject[] targets;
	//public GameObject captureBox;
	public float[] angleClamp = {5, 15};
	public float maxDistanceAway = 40f;
	public Vector3 avgDistance;
	public GameObject cube;

    // Starts the camera, gets the player targest, and gets the diagonal distance of the room
    public void Start ()
    {
        Console.WriteLine ("START");
        targets = GameObject.FindGameObjectsWithTag ("Player");
		//captureBox = GameObject.FindGameObjectWithTag ("CaptureBox");
        //avgBox = GameObject.FindGameObjectWithTag ("avgBox");
        // Square diagonal = Sqrt(2) * Length 
        //roomDiagonal = 1.414f * roomLength;
        if (Camera.main) {
            isOrthographic = Camera.main.orthographic;
        }
    }

	public void LateUpdate() 
	{
		Console.WriteLine ("START");
		// If we can't find a player, we return without altering the camera's position
		if (!GameObject.FindWithTag ("Player")) {
			return;
		}

		// We get information on the two players that are the farthest apart
		float[] seperationInfo = LargestDifferenceInfo ();
		float largestDifference = seperationInfo [0];
		float xMax = seperationInfo[1];
		float zMax = seperationInfo[2];
		float xMid = seperationInfo[3];
		float zMid = seperationInfo[4];

		/* We need to set the camera to contain all players. We use the information gained from the last call
		 * First, we use some trig nonsense to get the horizontal FoV, and also get the offset angle for the camera.
		 * Then, we check if its the x or z distance that is determining how much we need on camera. 
		 * If its the x length, we take what we alerady have found and simply finish by finding the hight. However
		 * if its the z, we redo the calculations using equations based on z being the bounding axis. 
		 * 
		 * The work and proofs for the calculations can be found in the SVN.
		 */

		float distanceRatio = largestDifference / maxDistanceAway;
		float shiftAngle = (float)(Mathf.LerpAngle (angleClamp [1], angleClamp [0], distanceRatio) * Math.PI / 180.0f); 
		float horFoVAngle = Mathf.Atan ((16 / 9) * Mathf.Tan ((float)(Camera.main.fieldOfView * Math.PI / 180.0f)));
		float backHypot = (xMax / (2 * Mathf.Tan (.5f * horFoVAngle)));
		float zOffset =  backHypot * Mathf.Sin (shiftAngle);
		float yOffset = 0.0f;

		if (zOffset > zMax) {
			yOffset = backHypot * Mathf.Cos (shiftAngle);
		} else {
			yOffset = zMax / (Mathf.Tan (shiftAngle + (float)(Camera.main.fieldOfView * Math.PI / 360.0f)) - Mathf.Tan(shiftAngle));
			zOffset = (yOffset * Mathf.Tan (shiftAngle)) + (zMax / 2);
		}
	
		Camera.main.transform.position = new Vector3 (xMid, yOffset, zMid + zOffset);
		Camera.main.transform.LookAt(new Vector3 (xMid, 0, zMid));
		Debug.DrawLine (Camera.main.transform.position, new Vector3 (xMid, 0, zMid));
		Debug.Log ("DRAW CALL RESULTS");
		for (int i = 0; i < 4; i++) {
			Debug.Log (seperationInfo[i]);
		}
		Debug.Log (zOffset);
		Debug.Log (yOffset);
		cube.transform.position = new Vector3 (xMid, 0, zMid);
		cube.transform.localScale = new Vector3 (xMax, 2, zMax);
		Debug.Break();
	}

    // Gets the largest distance between any two players, and returns it
    public float[] LargestDifferenceInfo ()
    {
        float currentDistance = 0.0f;
        float largestDistance = 0.0f;
		float xMax = 0.0f;
		float zMax = 0.0f;
		float xMid = 0.0f;  
		float zMid = 0.0f;

		int besti = -1;
		int bestj = -1;

        for (int i = 0; i < targets.Length - 3; i++) {
            for (int j = i + 1; j < targets.Length; j++) {
                currentDistance = Vector3.Distance (targets [i].transform.position, targets [j].transform.position);
                if (currentDistance > largestDistance) {
					largestDistance = currentDistance;
					besti = i;
					bestj = j;
                }
            }
        }

		xMax = Mathf.Abs(targets[besti].transform.position.x - targets[bestj].transform.position.x);
		zMax = Mathf.Abs(targets[besti].transform.position.z - targets[bestj].transform.position.z);
		xMid = (targets[besti].transform.position.x + targets[bestj].transform.position.x) / 2;
		zMid = (targets[besti].transform.position.z + targets[bestj].transform.position.z) / 2;
		
		float[] toReturn = {largestDistance, xMax, zMax, xMid, zMid};
		return toReturn;
    }
}

// This is the old code used in the previous version of the camera. Its kept in case we need to refer back on our
// previous methods for new ideas, or alterations to the current code. 

/* EVEN LESS OLD CODE
 * 
 * public void LateUpdateOLD ()
    {
        // If we can't find a player, we return without altering the camera's position
        if (!GameObject.FindWithTag ("Player")) {
            return;
        }

		// We sum the positions of all of the players, and from there we find the mid point between all of them
		Vector3 sum = new Vector3 (0, 0, 0);
		for (int i = 0; i < targets.Length; i++) {
			sum += targets [i].transform.position;
		}
		avgDistance = sum / targets.Length;
		captureBox.transform.position = avgDistance;
		
		// Next, we find what the biggest difference in distance between any two characters is
		float[] largestDifferenceInfo = LargestDifferenceInfo ();
		float largestDifference = largestDifferenceInfo [0];
		float maxX = largestDifferenceInfo [1];
		float maxZ = largestDifferenceInfo [2];

		float distanceOffset = largestDifference / maxDistanceAway;
		float yOffset = Mathf.Lerp (yClamp [0], yClamp [1], distanceOffset);
		float zOffset = Mathf.Lerp (zClamp [0], zClamp [1], distanceOffset);

		// The camera is set to its new position, pointed to the point to look at. 
		Camera.main.transform.position = new Vector3 (avgDistance.x, yOffset, avgDistance.z + zOffset);
		Camera.main.transform.LookAt(avgDistance);

                
    }
 *
 *
 */

/* LESS OLD CODE
 * 
 * private bool isOrthographic;
    public GameObject[] targets;
    //public GameObject avgBox;
    public float currentDistance;
    public float largestDistance;
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
    public float[] xClamp = {-8,8};
    public float[] yClamp = {10,30};
    public float[] zClamp = {7,27};
    public float[] offsetClamp = {10, -10, 10, -10};
	public bool dubugText = true;
 * 
        // We sum the positions of all of the players, and from there we find the mid point between all of them
        Vector3 sum = new Vector3 (0, 0, 0);
        for (int i = 0; i < targets.Length; i++) {
            sum += targets [i].transform.position;
        }
        avgDistance = sum / targets.Length;

        // Next, we find what the biggest difference in distance between any two characters is
        float largestDifference = returnLargestDifference ();
                
        // The camera is done via clamping. The positions of the player weights how big of an offset
        // is used. This allows us to zoom in and out, and shift the camera to the left and right
        // to keep the room mostly in room. 

        // The xRatio of the room is what decides how far to the left or right the camera will shift
        // It is the current position of the players center divided by the rooms length
        xRatio = (avgDistance.x / roomLength) + .5f;
        float tempX = avgDistance.x + Mathf.Lerp (xClamp [0], xClamp [1], xRatio);

        // The yzRatio is the largest difference between two characters divided by the length. This is
        // used to decide the position of the Y, and Z of the camera depending on exactly where the players are
        yzRatio = largestDistance / roomLength;
        float tempY = Mathf.Lerp (yClamp [0], yClamp [1], yzRatio);
        float tempZ = avgDistance.z + 5 + Mathf.Lerp (zClamp [0], zClamp [1], yzRatio);

        // The offZRatio decides the exact position where the camera looks at. This lets the camera not always look
        // dead center, which is sometimes needed to finetune the exacts of the camera
        offZRatio = (avgDistance.z / roomLength) + .5f;
        float lookZOffset = Mathf.Lerp (offsetClamp [0], offsetClamp [1], offZRatio);
        float lookXOffset = Mathf.Lerp (offsetClamp [2], offsetClamp [3], xRatio);

        // The camera is set to its new position, pointed to the point to look at. 
        Camera.main.transform.position = new Vector3 (tempX, tempY, tempZ);
        Camera.main.transform.LookAt(new Vector3 ((avgDistance.x + lookXOffset), avgDistance.y, (avgDistance.z + lookZOffset)));


        // 'AVG BOX' code is simply debug code that's useful when trying to figure out how the camera is working. It 
        // is set to be where ever the camera is looking at. 
        //avgBox.transform.position = new Vector3 ((avgDistance.x + lookXOffset), avgDistance.y, (avgDistance.z + lookZOffset));

// All the GUI debug info
    public void OnGUI ()
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

 */






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