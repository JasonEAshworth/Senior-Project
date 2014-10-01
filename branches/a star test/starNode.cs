using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class starNode: System.IComparable<starNode>
{
	
	private starNode parent;
	public float costSoFar;
	public float heuristic;
	public TriClass myTri;

	public starNode (TriClass mT, float h)
	{
		parent = null;
		costSoFar = 0;
		heuristic = h;
		myTri = mT;
	}
	
	public void setParent (starNode p) 
	{
		parent = p;
		costSoFar = parent.costSoFar + Vector3.Distance (parent.myTri.centerPoint, myTri.centerPoint);
	}

	public starNode getParent() 
	{
		return parent;
	}
	
	public int CompareTo(starNode a)
	{
		starNode that = a;
		/*
		if (a.myTri.CompareTo (myTri) == 0) {
			return 0;
		} 
		else if (this.heuristic.CompareTo (that.heuristic) == 0)
		{
			return -1;
		} */
		return this.heuristic.CompareTo (that.heuristic);
	}
}

