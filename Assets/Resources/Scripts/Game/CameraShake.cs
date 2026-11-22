using UnityEngine;

/// Short camera shakes for hits, slams, and explosions. The dungeon camera script sets the
/// camera position every physics step, so this adds an offset after it and takes it back off
/// before the next frame's offset.
public class CameraShake : MonoBehaviour
{
	static float trauma;
	Vector3 applied;

	public static void Add(float amount)
	{
		trauma = Mathf.Clamp01(trauma + amount);
	}

	void LateUpdate()
	{
		transform.position -= applied;
		applied = Vector3.zero;
		if (trauma <= 0.0f || Time.timeScale == 0.0f)
			return;
		float power = trauma * trauma * 0.6f;
		applied = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * power;
		transform.position += applied;
		trauma = Mathf.Max(0.0f, trauma - Time.deltaTime * 1.6f);
	}
}
