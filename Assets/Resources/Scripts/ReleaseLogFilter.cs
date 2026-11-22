using UnityEngine;

/// The 2015 scripts log a lot of debug text, some of it every frame. In a browser each line goes
/// to the JavaScript console and costs frame time, so release builds keep only warnings and errors.
public static class ReleaseLogFilter
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	static void Apply()
	{
		if (!Debug.isDebugBuild)
			Debug.unityLogger.filterLogType = LogType.Warning;
	}
}
