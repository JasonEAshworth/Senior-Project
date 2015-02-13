using UnityEngine;
using System;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour
{
	private List<GameObject> objectList;
	public GameObject model;
	public int maxSize;

	private void Awake()
	{
		objectList = new List<GameObject>(maxSize);
		objectList.Add(model);
		model.SetActive(false);
	}
	
	public GameObject New()
	{
		GameObject t = null;

		List<GameObject> unused = objectList.FindAll(i => i.activeSelf == false);

		if(unused.Count > 0)
		{
			t = unused[0];
			t.transform.position = transform.position;
			t.transform.rotation = transform.rotation;
			t.gameObject.SetActive(true);
		}
		else if(objectList.Count < objectList.Capacity)
		{
			t = Instantiate(model, transform.position, transform.rotation) as GameObject;
			t.transform.parent = model.transform.parent;

			objectList.Add(t);
		}

		return t;
	}

	public void ActivateTrigger(bool state)
	{
		New();
	}
}
