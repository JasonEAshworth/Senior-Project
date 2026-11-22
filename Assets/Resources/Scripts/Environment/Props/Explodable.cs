using UnityEngine;

/// Breaks a prop into tumbling pieces when an attack sends it "Boom".
/// Replaces the paid Exploder package from 2015. This script keeps the GUID of that package's
/// Explodable script, so the props that used it link to this one without any prefab edits.
public class Explodable : MonoBehaviour
{
	public float force = 1.0f;
	public float radius = 0.5f;
	public int targetFragments = 10;

	const float FragmentLifetime = 3.0f;
	const int MinFragments = 4;
	const int MaxFragments = 24;

	bool broken;

	void Boom()
	{
		if (broken)
			return;
		broken = true;

		Renderer[] renderers = GetComponentsInChildren<Renderer>();
		if (renderers.Length == 0)
		{
			gameObject.SetActive(false);
			return;
		}

		Bounds bounds = renderers[0].bounds;
		foreach (Renderer r in renderers)
			bounds.Encapsulate(r.bounds);
		Material material = renderers[0].sharedMaterial;

		int count = Mathf.Clamp(targetFragments, MinFragments, MaxFragments);
		float volume = Mathf.Max(bounds.size.x * bounds.size.y * bounds.size.z, 0.001f);
		float pieceSize = Mathf.Max(0.08f, Mathf.Pow(volume / count, 1.0f / 3.0f));
		float blastRadius = Mathf.Max(radius, bounds.extents.magnitude) * 2.0f;

		for (int i = 0; i < count; i++)
		{
			GameObject piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
			piece.name = name + " piece";
			piece.layer = LayerMask.NameToLayer("Ignore Raycast");
			piece.transform.position = bounds.center + Vector3.Scale(Random.insideUnitSphere, bounds.extents * 0.6f);
			piece.transform.rotation = Random.rotation;
			piece.transform.localScale = Vector3.one * pieceSize * Random.Range(0.6f, 1.0f);
			if (material != null)
				piece.GetComponent<Renderer>().sharedMaterial = material;

			Rigidbody body = piece.AddComponent<Rigidbody>();
			body.mass = 0.2f;
			body.AddExplosionForce(2.0f + force * 2.0f, bounds.center, blastRadius, 0.5f, ForceMode.Impulse);
			Destroy(piece, FragmentLifetime + Random.value);
		}

		gameObject.SetActive(false);
	}
}
