using UnityEngine;
using System.Collections;

public class Warrior : PlayerBase
{
	public override void basicAttack()
	{
		Debug.Log ("warrior basic attack");
	}

	public override void specialAttack()
	{
		Debug.Log ("warrior special attack");
	}

	public override void classAbility()
	{
		Debug.Log ("warrior class ability");
	}
}
