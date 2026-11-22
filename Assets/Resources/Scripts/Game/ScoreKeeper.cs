using UnityEngine;

/// Arcade scoring. Attacks in the 2015 code do not record who hit whom, so a kill is credited
/// to the closest hero who is up, which for melee and most shots is the one who landed it.
public static class ScoreKeeper
{
	const float CreditRange = 16.0f;

	public static void CreditKill(EnemyBase enemy)
	{
		PlayerManager pm = PlayerManager.Instance;
		if (pm == null)
			return;
		PlayerBase best = null;
		float bestSqr = CreditRange * CreditRange;
		foreach (GameObject p in pm.players)
		{
			if (p == null || !p.activeInHierarchy)
				continue;
			PlayerBase pb = p.GetComponent<PlayerBase>();
			if (pb.slot == null || pb.dead)
				continue;
			float sqr = (p.transform.position - enemy.transform.position).sqrMagnitude;
			if (sqr < bestSqr)
			{
				bestSqr = sqr;
				best = pb;
			}
		}
		if (best == null)
			return;

		PlayerSlot slot = best.slot;
		slot.combo = Time.time - slot.lastKillTime <= GameSession.ComboWindow ? slot.combo + 1 : 1;
		slot.lastKillTime = Time.time;
		slot.kills++;
		int points = Mathf.RoundToInt(enemy.PointValue * slot.ComboMultiplier);
		slot.points += points;
		string text = "+" + points + (slot.combo > 1 ? "  x" + slot.ComboMultiplier.ToString("0.0") : "");
		ArcadeHud.Popup(enemy.transform.position, text, slot.Color);
	}
}
