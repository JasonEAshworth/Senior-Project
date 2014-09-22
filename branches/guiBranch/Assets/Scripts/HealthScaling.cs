using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HealthScaling : MonoBehaviour {
	
	public RawImage healthBar;

	private Vector2 pos;

	// Use this for initialization
	void Start () {
		pos = healthBar.rectTransform.sizeDelta;
	}
	
	// Update is called once per frame
	void Update () {
		//pos = new Vector2 (pos.x - 10.0f, pos.y - 36.0f);
		healthBar.rectTransform.sizeDelta = healthBar.rectTransform.sizeDelta - (new Vector2 (2.0f, 0.0f));

	}
}
