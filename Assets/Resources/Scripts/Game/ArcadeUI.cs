using UnityEngine;

/// Shared immediate-mode GUI look for the menus and the HUD. Everything is laid out on a
/// virtual 1280x720 canvas that scales with the window height.
public static class ArcadeUI
{
	public const float VirtualHeight = 720.0f;
	public static readonly Color Gold = new Color(1.0f, 0.82f, 0.4f);
	public static readonly Color Dim = new Color(0.65f, 0.65f, 0.65f);
	public static readonly Color PanelColor = new Color(0.05f, 0.04f, 0.04f, 0.82f);

	static readonly System.Collections.Generic.Dictionary<int, GUIStyle> styles = new System.Collections.Generic.Dictionary<int, GUIStyle>();
	static GUIStyle button;

	public static float Scale { get { return Screen.height / VirtualHeight; } }
	public static float Width { get { return Screen.width / Scale; } }

	/// Call at the top of OnGUI: scales the virtual canvas to the window.
	public static void Begin()
	{
		float s = Scale;
		GUI.matrix = Matrix4x4.Scale(new Vector3(s, s, 1.0f));
	}

	public static GUIStyle Label(int size, TextAnchor anchor = TextAnchor.MiddleLeft, bool bold = false)
	{
		size = Mathf.Clamp(size, 8, 159);
		int key = size * 32 + (int)anchor * 2 + (bold ? 1 : 0);
		if (!styles.TryGetValue(key, out GUIStyle st))
		{
			st = new GUIStyle(GUI.skin.label)
			{
				fontSize = size, alignment = anchor, fontStyle = bold ? FontStyle.Bold : FontStyle.Normal,
				wordWrap = true, richText = true, clipping = TextClipping.Overflow,
			};
			styles[key] = st;
		}
		return st;
	}

	public static void Text(Rect r, string text, int size, Color color, TextAnchor anchor = TextAnchor.MiddleLeft, bool bold = false)
	{
		GUIStyle st = Label(size, anchor, bold);
		Color old = GUI.color;
		// A one-pixel shadow keeps text readable over the bright dungeon floor.
		GUI.color = new Color(0, 0, 0, color.a * 0.8f);
		GUI.Label(new Rect(r.x + 2, r.y + 2, r.width, r.height), text, st);
		GUI.color = color;
		GUI.Label(r, text, st);
		GUI.color = old;
	}

	public static void Panel(Rect r, Color? tint = null)
	{
		Color old = GUI.color;
		GUI.color = tint ?? PanelColor;
		GUI.DrawTexture(r, Texture2D.whiteTexture);
		GUI.color = old;
	}

	public static void Frame(Rect r, Color color, float thickness = 3.0f)
	{
		Color old = GUI.color;
		GUI.color = color;
		GUI.DrawTexture(new Rect(r.x, r.y, r.width, thickness), Texture2D.whiteTexture);
		GUI.DrawTexture(new Rect(r.x, r.yMax - thickness, r.width, thickness), Texture2D.whiteTexture);
		GUI.DrawTexture(new Rect(r.x, r.y, thickness, r.height), Texture2D.whiteTexture);
		GUI.DrawTexture(new Rect(r.xMax - thickness, r.y, thickness, r.height), Texture2D.whiteTexture);
		GUI.color = old;
	}

	public static void Bar(Rect r, float fraction, Color fill, Color back)
	{
		Panel(r, back);
		Panel(new Rect(r.x, r.y, r.width * Mathf.Clamp01(fraction), r.height), fill);
	}

	public static bool Button(Rect r, string text, int size = 22, bool highlighted = false)
	{
		if (button == null)
			button = new GUIStyle(GUI.skin.button);
		button.fontSize = size;
		Color old = GUI.color;
		GUI.color = highlighted ? Gold : Color.white;
		bool clicked = GUI.Button(r, text, button);
		GUI.color = old;
		return clicked;
	}

	/// True on alternate half-seconds, for blinking "PRESS START" text.
	public static bool Blink { get { return (Time.unscaledTime % 1.0f) < 0.6f; } }

	public static string Hex(Color c) { return ColorUtility.ToHtmlStringRGB(c); }
}
