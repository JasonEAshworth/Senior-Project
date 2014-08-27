using UnityEngine;
using System.Collections;

public class Rogue : PlayerBase
{
	private float timeStarted = Time.time - 10.0f;
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
		if(Input.GetKeyDown(specialAttackKey))
		{
			//Envenom
			Debug.Log ("warrior special attack");
		}
	}
	
	public override void classAbility()
	{
		if(Input.GetKeyDown(classAbilityKey))
		{
			timeStarted = Time.time;
		}
		//Remove GetKeyUp if you want the ability to last 10 seconds even if the user releases the key
		if(Input.GetKeyUp(classAbilityKey))
		{
			visibility = 1.0f;
			timeStarted -= 10.0f;
		}
		float currentTime = Time.time;
		float timeSinceStart = currentTime - timeStarted;
		if(timeSinceStart < 10.0f)
		{
			visibility = 0.0f;
			Debug.Log ("warrior class ability");
		}
		else
		{
			visibility = 1.0f;
		}
	}
}
