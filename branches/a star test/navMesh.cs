using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class navMesh : MonoBehaviour 
{
	public List<Vector3> verts;
	public List<TriClass> tris;
	public int count =0;
	public int matches = 0;
	public List<int[]> facesIndexes;
	Vector3[] temp;

	private GameObject enemy;
	private GameObject target;
	private TriClass enemyTri;
	private TriClass targetTri;
	private Vector3 curDestination;
	private aStar aStarAI;
	private List<TriClass> enemyPath;

	// Use this for initialization
	void Start () 
	{
		verts = new List<Vector3>();
		tris = new List<TriClass>();
		facesIndexes = new List<int[]> ();
		//string[] lines = System.IO.File.ReadAllLines ("Assets/Meshes/plain.obj");

		GameObject floor = GameObject.FindGameObjectWithTag ("Floor");
		Mesh floorMesh = floor.GetComponent<MeshFilter> ().mesh;
		for (int i=0; i<floorMesh.vertices.Length; i++) 
		{
			verts.Add(floor.transform.TransformPoint(floorMesh.vertices[i]));
		}

		for (int i=0; i<floorMesh.triangles.Length/3; i++)
		{
			Vector3[] triVerts = new Vector3[3];
			for (int j=0; j<3; j++)
			{
				int indexTemp = floorMesh.triangles[3*i+j];
				triVerts[j] = verts[indexTemp];
			}
			TriClass temp = new TriClass(i,triVerts);
			tris.Add (temp);
		}

		for (int i=0; i<tris.Count; i++) 
		{
			//Debug.Log((verts[tris[i].faceVertexes[0]]  + verts[tris[i].faceVertexes[1]] +  verts[tris[i].faceVertexes[2]] )/3);

			Vector3 vert1 = tris[i].faceVertexes[0];
			Vector3 vert2 = tris[i].faceVertexes[1];
			Vector3 vert3 = tris[i].faceVertexes[2];
			//Debug.Log(vert1 + " " + vert2 + " " + vert3 + " " + i);

			//To find the Center of the Triangle.
			Vector3 triCenter = (vert1 + vert2 + vert3)  /3.0f;
			tris[i].centerPoint = triCenter;
			Debug.Log (triCenter);
			
			//Debug.Log (count);
			int index = 0;
            for(int j=0;j<tris.Count;j++)
			{
                if (i == j) { continue; }

				for(int z=0;z<3;z++)
				{
					if(tris[j].faceVertexes[z] == vert1)
					{	
						//Debug.Log (tris[j].faceVertexes[z] + " " + vert1);
						matches++;
					}
					else if(tris[j].faceVertexes[z] == vert2)
					{
						//Debug.Log (tris[j].faceVertexes[z] + " " + vert2);
						matches++;
					}
					else if(tris[j].faceVertexes[z] == vert3)
					{
						//Debug.Log("Matched3");
						matches++;
					}
				}
				//Debug.Log (matches);
				if(matches == 2)
				{
					//Debug.Log ("We made it!" + tris[i].id);
                    tris[i].neighbors.SetValue(tris[j], index);
					//Debug.Log (tris[j].id);
					index++;
				};
				matches = 0;
            }
            tris[i].numOfNeighbors = index -1;
			
		}


		target = GameObject.FindGameObjectWithTag ("Player");
		enemy = GameObject.FindGameObjectWithTag ("Enemy");
		//Debug.Log ("1");
		enemyTri = tris[findTriOfPoint (enemy.transform.position)];
		//Debug.Log ("2");
		targetTri = tris [findTriOfPoint (target.transform.position)];
		enemyPath = new List<TriClass> ();

		aStarAI = new aStar (enemyTri, tris);
		starNode tempNode = aStarAI.runSearch (targetTri);
		createTriPath (tempNode);
	
	}

	// Takes an end node of a path, and sets up the enemy path list so that we can travel
	void createTriPath(starNode endNode) 
	{
		// If there's no end node, return
		if (endNode == null) {
			return;
		}

		// We don't need the last tri, as its the one the player is in. Instead, we'll simply use a lazy
		// follow AI once the player and enemy are in the same tri. If we are already there, simply return.
		starNode backtrack = endNode.getParent ();
		if (backtrack == null) {
			return;
		} 

		// We know that the final node, which has a null parent, is the node for the enemies current tri. 
		// As such, we don't need to store the node in the path. In adition, the second to last node will
		// always be our first distination, and so we don't need to store that tri either. Instead, we just
		// put that tri directly into our destination variable. 
		while (backtrack.getParent() != null) 
		{
			enemyPath.Add(backtrack.myTri);
			backtrack = backtrack.getParent();
		}
		curDestination = backtrack.myTri.centerPoint;
	}

	// Update is called once per frame
	void Update () 
	{
		//Debug.Log ("3");
		int taIndex = findTriOfPoint (target.transform.position);
		TriClass curTargetTri = tris [taIndex];
		//Debug.Log ("4");
		int enIndex = findTriOfPoint (enemy.transform.position);
		TriClass curEnemyTri = tris [enIndex];
		if (curTargetTri != targetTri) 
		{
			targetTri = curTargetTri;
			enemyTri = curEnemyTri;

			aStarAI.restartSearch(enemyTri);
			starNode tempNode = aStarAI.runSearch (targetTri);
			createTriPath (tempNode);

		}

		float distToMove = 2 * Time.deltaTime;

		while ( Vector3.Distance(enemy.transform.position, curDestination) < distToMove)
		{
			enemy.transform.position = curDestination;
			distToMove -= Vector3.Distance(enemy.transform.position, curDestination);

			if (enemyPath.Count != 0) 
			{
				enemyPath.RemoveAt(enemyPath.Count-1);
				if (enemyPath.Count == 0) 
				{
					curDestination = target.transform.position;
				} 
				else 
				{
					curDestination = enemyPath[enemyPath.Count-1].centerPoint;
				}
			}
			else if (curDestination != target.transform.position)
			{
				curDestination = target.transform.position;
			} 
            else
            {
                break;
            }

		}
		
        if (curDestination != target.transform.position) 
        {
            enemy.transform.LookAt (curDestination);
            enemy.transform.position += enemy.transform.forward * distToMove;
        }
	}

	int findTriOfPoint(Vector3 checkPoint)
	{
        for( int i = 0; i < tris.Count; i++) 
		{
            if(inTri(checkPoint, (Vector3)tris[i].faceVertexes[0], (Vector3)tris[i].faceVertexes[1], (Vector3)tris[i].faceVertexes[2])) 
			{
				return i;
			}
		}
		Debug.Log ("YOU DUN F**KED UP SON");
		return -1;
	}

	// Slightly modified version of the solution found here:
	// http://stackoverflow.com/questions/2049582/how-to-determine-a-point-in-a-triangle
	private bool inTri(Vector3 checkPoint, Vector3 p0, Vector3 p1, Vector3 p2) 
	{
		float area = 0.5f * (-p1.z * p2.x + p0.z * (-p1.x + p2.x) + p0.x * (p1.z - p2.z) + p1.x * p2.z);
		float sign = area < 0 ? -1 : 1;
		float s = (p0.z * p2.x - p0.x * p2.z + (p2.z - p0.z) * checkPoint.x + (p0.x - p2.x) * checkPoint.z) * sign;
		float t = (p0.x * p1.z - p0.z * p1.x + (p0.z - p1.z) * checkPoint.x + (p1.x - p0.x) * checkPoint.z) * sign;
		
		return (s > 0 && t > 0 && (s + t) < 2 * area * sign);
	}
}

/* OLD CODE
        for (int i=0; i<lines.Length; i++) 
        {
            if(lines[i][0] == 'v')
            {
                string[] temp = lines[i].Split(' ');
                Vector3 vert = new Vector3(float.Parse(temp[1]),float.Parse(temp[2]),float.Parse(temp[3]));
                verts.Add(vert);
            }
            else if(lines[i][0] == 'f')
            {
                string[] temp = lines[i].Split(' ');
                int[] tempFace = new int[3];
                tempFace[0] = int.Parse(temp[1]);
                tempFace[1] = int.Parse(temp[2]);
                tempFace[2] = int.Parse(temp[3]);
                facesIndexes.Add (tempFace);
                //TriClass tri = new TriClass(count,tempFace);
                //tris.SetValue(tri,count);
                count++;
            }
        }

        for (int i = 0; i < count; i++) 
        {
            temp = new Vector3[3];
            for(int y=0;y<facesIndexes[i].Length;y++)
            {

                int ind = facesIndexes[i][y]-1;
                //Debug.Log (ind + " " + verts.Count);
                Vector3 v = verts[ind];
                //Debug.Log (v);
                temp.SetValue(v,y);
            }
            TriClass temp2 = new TriClass(i,temp);
            tris.Add (temp2);
        }

        for (int i=0; i<facesIndexes.Count; i++) 
        {
        
            
            //Debug.Log((verts[tris[i].faceVertexes[0]]  + verts[tris[i].faceVertexes[1]] +  verts[tris[i].faceVertexes[2]] )/3);

        
            
            Vector3 vert1 = tris[i].faceVertexes[0];
            Vector3 vert2 = tris[i].faceVertexes[1];
            Vector3 vert3 = tris[i].faceVertexes[2];
            //Debug.Log(vert1 + " " + vert2 + " " + vert3 + " " + i);
            //To find the Center of the Triangle.
            Vector3 triCenter = (vert1 + vert2 + vert3)  /3;
            tris[i].centerPoint = triCenter;
            //Debug.Log (triCenter);
            

            //Debug.Log (count);
            int index = 0;
            for(int j=0;j<count-1;j++)
            {

                for(int z=0;z<3;z++)
                {
                    if(tris[j].faceVertexes[z] == vert1)
                    {   
                        //Debug.Log (tris[j].faceVertexes[z] + " " + vert1);
                        matches++;
                    }
                    else if(tris[j].faceVertexes[z] == vert2)
                    {
                        //Debug.Log (tris[j].faceVertexes[z] + " " + vert2);
                        matches++;
                    }
                    else if(tris[j].faceVertexes[z] == vert3)
                    {
                        //Debug.Log("Matched3");
                        matches++;
                    }
                }
                //Debug.Log (matches);
                if(matches == 2)
                {
                    //Debug.Log ("We made it!" + tris[i].id);
                    tris[i].neighbors.SetValue(tris[j].id,index);
                    //Debug.Log (tris[j].id);
                    index++;
                }
                tris[i].numOfNeighbors = index -1;
                matches = 0;
            }

        }
        */