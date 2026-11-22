using System;
using System.Collections.Generic;
using UnityEngine;

/// Arcade high-score entry: each qualifying player picks three letters with up and down, and
/// confirms each letter with jump or attack. Calls onDone when every player has finished.
public class InitialsEntry : MonoBehaviour
{
	const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ";

	readonly Queue<PlayerSlot> queue = new Queue<PlayerSlot>();
	PlayerSlot current;
	int[] letters = new int[3];
	int position;
	string reached;
	Action onDone;

	public static InitialsEntry Begin(IEnumerable<PlayerSlot> players, string reachedText, Action done)
	{
		var go = new GameObject("InitialsEntry");
		InitialsEntry e = go.AddComponent<InitialsEntry>();
		e.reached = reachedText;
		e.onDone = done;
		foreach (PlayerSlot s in players)
			if (GameSession.Qualifies(s.points))
				e.queue.Enqueue(s);
		e.Next();
		return e;
	}

	void Next()
	{
		if (queue.Count == 0)
		{
			current = null;
			Action d = onDone;
			Destroy(gameObject);
			d?.Invoke();
			return;
		}
		current = queue.Dequeue();
		letters = new int[] { 0, 0, 0 };
		position = 0;
	}

	void Update()
	{
		if (current == null)
			return;
		int dy = PlayerControls.NavY(current.binding);
		if (dy != 0)
			Step(-dy);
		int dx = PlayerControls.NavX(current.binding);
		if (dx != 0)
			position = Mathf.Clamp(position + dx, 0, 2);
		if (PlayerControls.Pressed(current.binding, PlayerAction.Jump) || PlayerControls.Pressed(current.binding, PlayerAction.Attack))
			Confirm();
	}

	void Step(int d)
	{
		letters[position] = (letters[position] + d + Alphabet.Length) % Alphabet.Length;
	}

	void Confirm()
	{
		if (position < 2)
		{
			position++;
			return;
		}
		string initials = "" + Alphabet[letters[0]] + Alphabet[letters[1]] + Alphabet[letters[2]];
		GameSession.AddHighScore(initials.Trim().Length == 0 ? "???" : initials, current.points, reached);
		Next();
	}

	void OnGUI()
	{
		if (current == null)
			return;
		GUI.depth = -500;
		ArcadeUI.Begin();
		float w = ArcadeUI.Width;
		var r = new Rect(w / 2 - 300, 170, 600, 360);
		ArcadeUI.Panel(r, new Color(0, 0, 0, 0.9f));
		ArcadeUI.Frame(r, current.Color);
		ArcadeUI.Text(new Rect(r.x, r.y + 16, r.width, 50), "NEW HIGH SCORE!", 40, ArcadeUI.Gold, TextAnchor.MiddleCenter, true);
		ArcadeUI.Text(new Rect(r.x, r.y + 70, r.width, 36), current.Label + "  " + current.points.ToString("N0"), 28, current.Color, TextAnchor.MiddleCenter, true);
		for (int i = 0; i < 3; i++)
		{
			var cell = new Rect(r.x + 165 + i * 95, r.y + 130, 80, 100);
			ArcadeUI.Panel(cell, i == position ? new Color(0.3f, 0.25f, 0.1f, 1) : new Color(0.15f, 0.15f, 0.15f, 1));
			ArcadeUI.Text(cell, Alphabet[letters[i]].ToString(), 64, Color.white, TextAnchor.MiddleCenter, true);
			if (GUI.Button(new Rect(cell.x, cell.y - 26, 80, 24), "^")) { position = i; Step(1); }
			if (GUI.Button(new Rect(cell.x, cell.yMax + 2, 80, 24), "v")) { position = i; Step(-1); }
		}
		ArcadeUI.Text(new Rect(r.x, r.y + 270, r.width, 30), "Up/down: letter    Jump or attack: next", 16, ArcadeUI.Dim, TextAnchor.MiddleCenter);
		if (ArcadeUI.Button(new Rect(r.x + 220, r.y + 302, 160, 40), position < 2 ? "Next" : "Done", 20))
			Confirm();
	}
}
