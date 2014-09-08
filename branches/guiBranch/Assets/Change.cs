using UnityEngine;
using System.Collections;

public class Change : MonoBehaviour {


	// Update is called once per frame
	public void ChangeToScene(int scene) {
		Application.LoadLevel (scene);
	}
}
