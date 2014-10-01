using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TriClass: IComparer
{
	public int id;
	public Vector3[] faceVertexes;
	public TriClass[] neighbors;
	public int numOfNeighbors;
	public Vector3 centerPoint;
	public TriClass(int idNum,Vector3[] verts)
	{
		id = idNum;
		faceVertexes = verts;
        neighbors = new TriClass[3];
	}
	

	public int CompareTo(object a)
	{
		TriClass that = (TriClass)a;
		if (that.faceVertexes [0] == faceVertexes [0] && that.faceVertexes [1] == faceVertexes [1] && that.faceVertexes [2] == faceVertexes [2] ||
			that.faceVertexes [1] == faceVertexes [0] && that.faceVertexes [2] == faceVertexes [1] && that.faceVertexes [0] == faceVertexes [2] ||
			that.faceVertexes [2] == faceVertexes [0] && that.faceVertexes [0] == faceVertexes [1] && that.faceVertexes [1] == faceVertexes [2]) 
		{
			return 0;
		}
		return id.CompareTo (that.id);
	}

	public int Compare(object a, object b)
	{
		TriClass curr = (TriClass)a;
		TriClass that = (TriClass)b;

		if (that.faceVertexes [0] == curr.faceVertexes [0] && that.faceVertexes [1] == curr.faceVertexes [1] && that.faceVertexes [2] == curr.faceVertexes [2] ||
		    that.faceVertexes [1] == curr.faceVertexes [0] && that.faceVertexes [2] == curr.faceVertexes [1] && that.faceVertexes [0] == curr.faceVertexes [2] ||
		    that.faceVertexes [2] == curr.faceVertexes [0] && that.faceVertexes [0] == curr.faceVertexes [1] && that.faceVertexes [1] == curr.faceVertexes [2]) 
		{
			return 0;
		}
		return curr.id.CompareTo (that.id);
	}

}
