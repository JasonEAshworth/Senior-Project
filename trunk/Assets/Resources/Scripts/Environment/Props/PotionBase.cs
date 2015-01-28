using UnityEngine;
using System.Collections;

public class PotionBase : MonoBehaviour 
{
	// Health potion
	private static float healValue = 20.0f;

	// Haste potion
	private static float speedIncrease = 1.5f;
	private static float effectDuration = 10.0f;

	void Update()
	{
		transform.Rotate (new Vector3 (0, 30, 0) * Time.deltaTime);
	}

	void OnTriggerEnter(Collider player)
	{
		if (player.gameObject.CompareTag("Player")) 
		{
			player.gameObject.SendMessage("addItem", gameObject.tag);
			Destroy(gameObject);
		}
	}

	public static void useHealthPotion(PlayerBase p)
	{
		float amount = 20.0f;
		p.potionImg.enabled = false;
		if (p.health + amount > p.maxHealth)
		{
			amount -= (p.health + amount) - p.maxHealth;
		}
		float amt4Health = amount / p.maxHealth;
		p.healthBar.rectTransform.sizeDelta = p.healthBar.rectTransform.sizeDelta + (new Vector2 (p.healthBarWidth*amt4Health, 0.0f));
	}

	public static void useHastePotion(PlayerBase p)
	{
		//StartCoroutine(hasteCoroutine(p));	
	}

	private IEnumerator hasteCoroutine(PlayerBase p)
	{
		p.GetComponent<Animator>().speed = speedIncrease;
		yield return new WaitForSeconds(effectDuration);
		p.GetComponent<Animator>().speed = 1.0f;
		yield return null;
	}
}
