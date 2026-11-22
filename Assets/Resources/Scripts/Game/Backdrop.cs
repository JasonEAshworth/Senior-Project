using UnityEngine;

/// A dungeon room behind a menu, with the camera slowly circling it.
public class Backdrop : MonoBehaviour
{
	public float degreesPerSecond = 6.0f;

	public static Backdrop Build(string roomName, Color ambient)
	{
		GameObject prefab = Resources.Load<GameObject>(roomName);
		if (prefab != null)
			Instantiate(prefab, Vector3.zero, Quaternion.Euler(0.0f, 180.0f, 0.0f));
		RenderSettings.ambientLight = ambient;

		var pivot = new GameObject("BackdropPivot");
		pivot.transform.position = new Vector3(10.0f, 0.0f, -14.0f);
		Backdrop b = pivot.AddComponent<Backdrop>();
		Camera cam = Camera.main;
		if (cam != null)
		{
			cam.clearFlags = CameraClearFlags.SolidColor;
			cam.backgroundColor = Color.black;
			cam.transform.SetParent(pivot.transform, false);
			cam.transform.localPosition = new Vector3(0.0f, 16.0f, -22.0f);
			cam.transform.localRotation = Quaternion.Euler(35.0f, 0.0f, 0.0f);
			cam.fieldOfView = 50.0f;
		}
		return b;
	}

	void Update()
	{
		transform.Rotate(0.0f, degreesPerSecond * Time.unscaledDeltaTime, 0.0f);
	}
}
