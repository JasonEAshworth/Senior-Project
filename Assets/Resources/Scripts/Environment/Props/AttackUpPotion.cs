using UnityEngine;
using System.Collections;

public class AttackUpPotion : MonoBehaviour
{
	private float attackIncrease = 2.0f;
	private float effectDuration = 10.0f;

	public void usePotion(PlayerBase p)
	{
		StartCoroutine(attackCoroutine(p));	
	}
	
	private IEnumerator attackCoroutine(PlayerBase p)
	{
		//GetComponent<Animator>().speed = attackIncrease;
		// Multiply and divide, not set: shop upgrades and power-ups also scale attackMultiplier.
		p.attackMultiplier *= attackIncrease;
		yield return new WaitForSeconds(effectDuration);
		//GetComponent<Animator>().speed = 1.0f;
		p.attackMultiplier /= attackIncrease;
		Destroy(this);
		yield return null;
	}


}