using UnityEngine;
using System.Collections;

public class Adventurer : PlayerBase
{
	void start()
	{
		int health = 100;
		int maxHealth = health;
		moveSpeed = 4.0f;
	}
	
	public override void basicAttack(string dir)
	{
		//Debug.Log ("warrior basic attack");
	}
	
	public override void specialAttack()
	{
		//Debug.Log ("warrior special attack");
	}
	
	public override void classAbility(string dir)
	{
		//Debug.Log ("warrior class ability");
	}
}
