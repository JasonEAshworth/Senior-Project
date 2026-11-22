using System;
using System.Collections;
using UnityEngine;

/// Lets a desktop build check itself: run with -screenshot out.png [-seconds N] and the player
/// saves one frame after N seconds (default 8), then quits. Does nothing without the argument.
public class BuildCheck : MonoBehaviour
{
	string path;
	float seconds = 8.0f;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	static void Install()
	{
		if (Application.isEditor || Application.platform == RuntimePlatform.WebGLPlayer)
			return;
		string[] args = Environment.GetCommandLineArgs();
		int i = Array.IndexOf(args, "-screenshot");
		if (i < 0 || i + 1 >= args.Length)
			return;
		var check = new GameObject("BuildCheck").AddComponent<BuildCheck>();
		DontDestroyOnLoad(check.gameObject);
		check.path = args[i + 1];
		int s = Array.IndexOf(args, "-seconds");
		if (s >= 0 && s + 1 < args.Length)
			float.TryParse(args[s + 1], System.Globalization.NumberStyles.Float,
				System.Globalization.CultureInfo.InvariantCulture, out check.seconds);
	}

	IEnumerator Start()
	{
		yield return new WaitForSecondsRealtime(seconds);
		yield return new WaitForEndOfFrame();
		ScreenCapture.CaptureScreenshot(path);
		Debug.Log("BuildCheck: saved " + path);
		// CaptureScreenshot writes the file on a later frame.
		yield return new WaitForSecondsRealtime(1.0f);
		Application.Quit();
	}
}
