using UnityEngine;
using System;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour
{
	private List<GameObject> objectList;
	public GameObject model;
	public int maxSize;
	private Vector3 position;
	private Quaternion rotation;

	private void Awake()
	{
		objectList = new List<GameObject>(maxSize);
		position = model.transform.position;
		rotation = model.transform.rotation;
		objectList.Add(model);
		model.SetActive(false);
	}
	
	public void New()
	{
		GameObject t;

		List<GameObject> unused = objectList.FindAll(i => i.activeSelf == false);

		if(unused.Count > 0)
		{
			t = unused[0];
			t.transform.position = position;
			t.transform.rotation = rotation;
			t.gameObject.SetActive(true);
		}
		else if(objectList.Count < objectList.Capacity)
		{
			t = Instantiate(model, position, rotation) as GameObject;
			t.transform.parent = model.transform.parent;

			objectList.Add(t);
		}
	}

	public void ActivateTrigger(bool state)
	{
		New();
	}
}
