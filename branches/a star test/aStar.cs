using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class aStar
{
	List<TriClass> allTri;
	heapSort<starNode> open;
	List<starNode> closed;
	List<TriClass> explored;
	TriClass startTri;

	// Set up a future A* search
	public aStar (TriClass sT, List<TriClass> aT)
	{
		allTri = aT;
		startTri = sT;

		open = new heapSort<starNode>();
		closed = new List<starNode>();
		explored = new List<TriClass>();
	}

	// Restart a search with a new start
	public void restartSearch(TriClass newStartTri)
	{
		startTri = newStartTri;
		open = new heapSort<starNode>();
		closed = new List<starNode>();
		explored = new List<TriClass>();
	}

	public starNode runSearch (TriClass endTri) 
	{
		starNode startNode = new starNode (startTri, Vector3.Distance(startTri.centerPoint, endTri.centerPoint));
		open.Add (startNode);
		explored.Add (startTri);
        int counter = 0;

		while (open.Count > 0) 
		{
            if (counter == 10000) {
                Debug.Break();
                Debug.LogError("INFINITE LOOP BREAK");
                return null;
            }
            counter++;
			// Take best node off of open, and check if its the end goal
			starNode curNode = open.Pop();
			if (curNode.myTri == endTri)
			{
				return curNode;
			}
			// If not, add it to closed
			closed.Add(curNode);

			// Check all neighbors of C
			for (int i = 0; i < curNode.myTri.neighbors.Length; i++) 
			{
                if (curNode.myTri.neighbors[i] == null) {
                    continue;
                }
				//int curNeighbor = curNode.myTri.neighbors[i];
				//TriClass triNeighbor = allTri[curNeighbor];
                TriClass triNeighbor = curNode.myTri.neighbors[i];
				starNode nodeNeighbor = new starNode(triNeighbor, Vector3.Distance(triNeighbor.centerPoint, endTri.centerPoint));
				nodeNeighbor.setParent(curNode);

				if(explored.Contains(triNeighbor)) 
				{
					if (open.Contains(nodeNeighbor) && (curNode.costSoFar + Vector3.Distance(curNode.myTri.centerPoint, nodeNeighbor.myTri.centerPoint)) < nodeNeighbor.costSoFar)
					{
						open.replace(open.FindIndex(x => x.myTri.CompareTo(nodeNeighbor.myTri) == 0), nodeNeighbor);
						open.reheap();
                        //Debug.Log("OPTION 1");
					}
					else if (open.Contains(nodeNeighbor) && (curNode.costSoFar + Vector3.Distance(curNode.myTri.centerPoint, nodeNeighbor.myTri.centerPoint)) < nodeNeighbor.costSoFar)
					{
						closed.Remove(nodeNeighbor);
                        open.Add(nodeNeighbor);
                        //Debug.Log("OPTION 2");
					}
				}
				else
				{
                    open.Add(nodeNeighbor);
                    explored.Add (triNeighbor);
                    //Debug.Log("OPTION 3");
				}
			}
		}
		return null;
	}
}

