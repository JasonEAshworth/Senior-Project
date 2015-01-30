using UnityEngine;
using System.Collections;

public class HealthPotion : MonoBehaviour 
{
	private float healValue = 20.0f;

	public void usePotion(PlayerBase p)
	{
		float amount = 20.0f;
		p.potionImg.enabled = false;
		if (p.health + amount > p.maxHealth)
		{
			amount -= (p.health + amount) - p.maxHealth;
		}
		float amt4Health = amount / p.maxHealth;
		p.healthBar.rectTransform.sizeDelta = p.healthBar.rectTransform.sizeDelta + (new Vector2 (p.healthBarWidth*amt4Health, 0.0f));
		Destroy(this);
	}
}
