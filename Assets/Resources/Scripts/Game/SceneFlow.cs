using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// Scene changes behind a full-screen card: fade to black, show the chapter title and a tip
/// while the next scene loads, then fade back in. Lives across scenes.
public class SceneFlow : MonoBehaviour
{
	static SceneFlow instance;

	static readonly string[] tips = {
		"Stand next to a downed friend to revive them.",
		"Kill quickly to build a combo. Getting hit resets it.",
		"Gold buys upgrades in the hub between chapters.",
		"Every chapter you clear makes the others harder.",
		"Glowing orbs are power-ups. Grab them fast.",
		"Out of lives? Press jump to spend a continue.",
		"Clear a room without a scratch for a bonus.",
		"A second player can join at any time: press jump or start.",
	};

	float alpha;
	string title, subtitle, tip;
	GUIStyle titleStyle, subStyle, tipStyle;

	public static bool Busy { get; private set; }

	static void Ensure()
	{
		if (instance != null)
			return;
		var go = new GameObject("SceneFlow");
		DontDestroyOnLoad(go);
		instance = go.AddComponent<SceneFlow>();
	}

	public static void Load(string scene, string cardTitle = null, string cardSubtitle = null)
	{
		Ensure();
		if (Busy)
			return;
		instance.StartCoroutine(instance.LoadRoutine(scene, cardTitle, cardSubtitle));
	}

	IEnumerator LoadRoutine(string scene, string cardTitle, string cardSubtitle)
	{
		Busy = true;
		title = cardTitle;
		subtitle = cardSubtitle;
		tip = tips[Random.Range(0, tips.Length)];

		for (float t = 0; t < 0.35f; t += Time.unscaledDeltaTime)
		{
			alpha = t / 0.35f;
			yield return null;
		}
		alpha = 1.0f;
		Time.timeScale = 1.0f;

		float shownAt = Time.unscaledTime;
		AsyncOperation op = SceneManager.LoadSceneAsync(scene);
		while (!op.isDone)
			yield return null;
		float minimum = string.IsNullOrEmpty(title) ? 0.2f : 1.8f;
		while (Time.unscaledTime - shownAt < minimum)
			yield return null;

		for (float t = 0; t < 0.5f; t += Time.unscaledDeltaTime)
		{
			alpha = 1.0f - t / 0.5f;
			yield return null;
		}
		alpha = 0.0f;
		Busy = false;
	}

	void OnGUI()
	{
		if (alpha <= 0.0f)
			return;
		GUI.depth = -1000;
		if (titleStyle == null)
		{
			titleStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
			subStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Italic };
			tipStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
		}
		float s = Screen.height / 720.0f;
		titleStyle.fontSize = Mathf.RoundToInt(54 * s);
		subStyle.fontSize = Mathf.RoundToInt(22 * s);
		tipStyle.fontSize = Mathf.RoundToInt(17 * s);

		Color old = GUI.color;
		GUI.color = new Color(0, 0, 0, alpha);
		GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
		if (!string.IsNullOrEmpty(title))
		{
			GUI.color = new Color(1.0f, 0.85f, 0.45f, alpha);
			GUI.Label(new Rect(0, Screen.height * 0.36f, Screen.width, 80 * s), title, titleStyle);
			GUI.color = new Color(0.85f, 0.85f, 0.85f, alpha);
			GUI.Label(new Rect(0, Screen.height * 0.36f + 80 * s, Screen.width, 40 * s), subtitle ?? "", subStyle);
			GUI.color = new Color(0.6f, 0.6f, 0.6f, alpha);
			GUI.Label(new Rect(0, Screen.height * 0.82f, Screen.width, 40 * s), "Tip: " + tip, tipStyle);
		}
		GUI.color = old;
	}
}
