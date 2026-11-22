using UnityEngine;

/// Simple shapes and flashes made at run time: attack warnings on the floor, explosion
/// flashes, and glowing orbs. They use the Sprites/Default shader, which every build includes.
public static class Fx
{
	static Material baseMaterial;

	public static Material Unlit(Color color)
	{
		if (baseMaterial == null)
			baseMaterial = new Material(Shader.Find("Sprites/Default"));
		var m = new Material(baseMaterial) { color = color };
		return m;
	}

	static Material softParticle;

	/// A round, soft-edged particle (a plain quad looks like a square in the air).
	public static Material SoftParticle()
	{
		if (softParticle != null)
			return softParticle;
		const int size = 64;
		var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
		var px = new Color[size * size];
		for (int y = 0; y < size; y++)
			for (int x = 0; x < size; x++)
			{
				float dx = (x + 0.5f) / size * 2 - 1, dy = (y + 0.5f) / size * 2 - 1;
				float a = Mathf.Clamp01(1 - Mathf.Sqrt(dx * dx + dy * dy));
				px[y * size + x] = new Color(1, 1, 1, a * a);
			}
		tex.SetPixels(px);
		tex.Apply();
		softParticle = Unlit(Color.white);
		softParticle.mainTexture = tex;
		return softParticle;
	}

	static GameObject Shape(PrimitiveType type, Color color)
	{
		GameObject go = GameObject.CreatePrimitive(type);
		Object.Destroy(go.GetComponent<Collider>());
		go.GetComponent<Renderer>().sharedMaterial = Unlit(color);
		go.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		return go;
	}

	/// A flat warning disc on the floor that fills in over its lifetime.
	public static GameObject WarningDisc(Vector3 center, float radius, float seconds, Color color)
	{
		GameObject go = Shape(PrimitiveType.Cylinder, color);
		go.name = "Warning";
		go.transform.position = new Vector3(center.x, center.y + 0.05f, center.z);
		go.transform.localScale = new Vector3(radius * 2, 0.02f, radius * 2);
		go.AddComponent<FxGrow>().Init(seconds, radius * 2, true);
		return go;
	}

	/// A flat warning strip from a point along a direction.
	public static GameObject WarningLine(Vector3 from, Vector3 dir, float length, float width, float seconds, Color color)
	{
		GameObject go = Shape(PrimitiveType.Cube, color);
		go.name = "Warning";
		dir.y = 0;
		dir.Normalize();
		go.transform.position = from + dir * (length / 2) + Vector3.up * 0.05f;
		go.transform.rotation = Quaternion.LookRotation(dir);
		go.transform.localScale = new Vector3(width, 0.02f, length);
		Object.Destroy(go, seconds);
		return go;
	}

	/// A quick expanding, fading sphere with a burst of light.
	public static void Flash(Vector3 at, float radius, Color color)
	{
		GameObject go = Shape(PrimitiveType.Sphere, color);
		go.name = "Flash";
		go.transform.position = at;
		go.transform.localScale = Vector3.one * 0.3f;
		Light l = go.AddComponent<Light>();
		l.color = color;
		l.range = radius * 3;
		l.intensity = 4;
		go.AddComponent<FxGrow>().Init(0.35f, radius * 2, false);
	}

	public static GameObject Orb(Vector3 at, float size, Color color, bool light = true)
	{
		GameObject go = Shape(PrimitiveType.Sphere, color);
		go.transform.position = at;
		go.transform.localScale = Vector3.one * size;
		if (light)
		{
			Light l = go.AddComponent<Light>();
			l.color = color;
			l.range = 4;
			l.intensity = 2.5f;
		}
		return go;
	}
}

/// Grows a shape to a size over time, fading it, then removes it.
public class FxGrow : MonoBehaviour
{
	float duration, size, t;
	bool flat;
	Material mat;
	Color start;
	Light lightSource;

	public void Init(float seconds, float targetSize, bool flatDisc)
	{
		duration = seconds;
		size = targetSize;
		flat = flatDisc;
		mat = GetComponent<Renderer>().material;
		start = mat.color;
		lightSource = GetComponent<Light>();
	}

	void Update()
	{
		t += Time.deltaTime;
		float k = Mathf.Clamp01(t / duration);
		if (flat)
		{
			// A warning fills from faint to strong so players can read the timing.
			mat.color = new Color(start.r, start.g, start.b, Mathf.Lerp(start.a * 0.3f, start.a, k));
		}
		else
		{
			float s = Mathf.Lerp(0.3f, size, k);
			transform.localScale = Vector3.one * s;
			mat.color = new Color(start.r, start.g, start.b, start.a * (1 - k));
			if (lightSource != null)
				lightSource.intensity = 4 * (1 - k);
		}
		if (t >= duration)
			Destroy(gameObject);
	}
}
