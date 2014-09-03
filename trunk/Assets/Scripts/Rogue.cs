using UnityEngine;
using System.Collections;

public class Rogue : PlayerBase
{
	void start()
	{
		int health = 100;
		int maxHealth = health;
		float visibility = 1.0f;
		float moveSpeed = 6.0f;
	}

	public override void basicAttack()
	{
		if(Input.GetKeyDown (basicAttackKey)){
			//Check enemy facing
			Debug.Log ("warrior basic attack");
		}
	}
	
	public override void specialAttack()
	{
		if(Input.GetKeyDown (basicAttackKey))
		{
			//Envenom
			Debug.Log ("warrior special attack");
		}
	}
	
	public override void classAbility()
	{
		if(Input.GetKeyDown (classAbilityKey))
		{
			visibility = 0.0f;
			Debug.Log ("warrior class ability");
		}
	}
}
