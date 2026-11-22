using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// Batch-mode helpers. Run with: Unity -batchmode -quit -projectPath . -executeMethod ProjectTools.BuildWebGL
public static class ProjectTools
{
	static string[] Scenes { get { return GameContent.Scenes; } }

	/// Rewrites every asset as YAML text, so git can diff it and asset references can be searched.
	[MenuItem("Tools/Rise of the Restless/Convert Assets To Text")]
	public static void ConvertToText()
	{
		EditorSettings.serializationMode = SerializationMode.ForceText;
		AssetDatabase.ForceReserializeAssets();
		AssetDatabase.SaveAssets();
	}

	/// Strips components whose script no longer exists (the purged Rewired and Exploder
	/// packages). Unity refuses to save a prefab that still has one.
	[MenuItem("Tools/Rise of the Restless/Remove Missing Scripts")]
	public static void RemoveMissingScripts()
	{
		int total = 0;
		foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" }))
		{
			string path = AssetDatabase.GUIDToAssetPath(guid);
			GameObject root = PrefabUtility.LoadPrefabContents(path);
			int removed = RemoveMissing(root.transform, path);
			if (removed > 0)
				PrefabUtility.SaveAsPrefabAsset(root, path);
			PrefabUtility.UnloadPrefabContents(root);
			total += removed;
		}

		foreach (string guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets" }))
		{
			string path = AssetDatabase.GUIDToAssetPath(guid);
			Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
			int removed = 0;
			foreach (GameObject go in scene.GetRootGameObjects())
				removed += RemoveMissing(go.transform, path);
			if (removed > 0)
				EditorSceneManager.SaveScene(scene);
			total += removed;
		}
		Debug.Log($"[missing] removed {total} components with missing scripts");
	}

	static int RemoveMissing(Transform root, string assetPath)
	{
		int removed = 0;
		foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
		{
			int n = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
			if (n > 0)
				Debug.Log($"[missing] {assetPath} : {HierarchyPath(t)} : {n}");
			removed += n;
		}
		return removed;
	}

	static string HierarchyPath(Transform t)
	{
		return t.parent == null ? t.name : HierarchyPath(t.parent) + "/" + t.name;
	}

	/// GitHub rejects files over 100 MB, and the WebGL data file is one file. Plain DXT textures
	/// barely gzip, so the data file passed 128 MB. Crunched DXT at up to 1024 px keeps the detail
	/// the top-down camera shows and cuts the download several times. Desktop builds are unchanged.
	[MenuItem("Tools/Rise of the Restless/Crunch WebGL Textures")]
	public static void CrunchWebGLTextures()
	{
		int changed = 0;
		AssetDatabase.StartAssetEditing();
		try
		{
			foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets" }))
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);
				if (!(AssetImporter.GetAtPath(path) is TextureImporter importer))
					continue;
				if (importer.textureCompression == TextureImporterCompression.Uncompressed)
					continue;
				TextureImporterPlatformSettings web = importer.GetPlatformTextureSettings("WebGL");
				web.overridden = true;
				web.maxTextureSize = Mathf.Min(importer.maxTextureSize, 1024);
				web.format = TextureImporterFormat.Automatic;
				web.textureCompression = TextureImporterCompression.Compressed;
				web.crunchedCompression = true;
				web.compressionQuality = 50;
				importer.SetPlatformTextureSettings(web);
				importer.SaveAndReimport();
				changed++;
			}
		}
		finally
		{
			AssetDatabase.StopAssetEditing();
		}
		Debug.Log($"[crunch] set WebGL crunch on {changed} textures");
	}

	[MenuItem("Tools/Rise of the Restless/Build WebGL")]
	public static void BuildWebGL()
	{
		PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
		// GitHub Pages cannot send a Content-Encoding header for .gz files, so the loader
		// decompresses them in JavaScript instead.
		PlayerSettings.WebGL.decompressionFallback = true;
		Build(BuildTarget.WebGL, "Build/WebGL");
	}

	[MenuItem("Tools/Rise of the Restless/Build Windows")]
	public static void BuildWindows()
	{
		Build(BuildTarget.StandaloneWindows64, "Build/Windows/RiseOfTheRestless.exe");
	}

	static void Build(BuildTarget target, string path)
	{
		GameContent.Prepare();
		BuildReport report = BuildPipeline.BuildPlayer(Scenes, path, target, BuildOptions.None);
		foreach (BuildStep step in report.steps)
			foreach (BuildStepMessage message in step.messages)
				if (message.type == LogType.Error || message.type == LogType.Exception)
					Debug.Log($"[build error] {step.name}: {message.content}");
		Debug.Log($"Build {target}: {report.summary.result}, {report.summary.totalSize / (1024 * 1024)} MB, {report.summary.totalErrors} errors");
		if (Application.isBatchMode && report.summary.result != BuildResult.Succeeded)
			EditorApplication.Exit(1);
	}
}
