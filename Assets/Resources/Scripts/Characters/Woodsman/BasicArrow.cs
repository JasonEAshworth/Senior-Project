using UnityEngine;
using System.Collections;
using Exploder;

public class BasicArrow : ProjectileTrapObj
{
	public bool basic = true;
	private Woodsman woodsman;
	private GameObject hawk;
	private HawkAI2 hawkScript;
	public bool infinitePierce = false;
	public  int numPierces = 0;
	public float heldTime = 0.0f;

	public void PostEnable(bool basicA, float time)
	{
		basic = basicA;
		heldTime = time;

		if (!basic)
		{
			if (heldTime > 5.0f)
			{
				infinitePierce = true;
			}
			else
			{
				numPierces = Mathf.FloorToInt(heldTime);
			}
		}
	}

	private void Start()
	{
		woodsman = GameObject.Find("Woodsman(Clone)").GetComponent<Woodsman>();
		hawk = GameObject.FindGameObjectWithTag("Hawk");
		hawkScript = hawk.GetComponent<HawkAI2>();
	}
	
	protected override void HitObject (Transform t)
	{
		if(t.collider.GetComponent<Explodable>() != null)
		{
			t.collider.SendMessage("Boom");
		}
		if(t.gameObject.CompareTag("Enemy"))
		{
			EnemyBase scr = t.gameObject.GetComponent<EnemyBase>();
			scr.takeDamage(damage);
			scr.damageTaken += damage;
			if(hawkScript.enemiesToAttack.Contains(t.gameObject) == false)
			{
				hawkScript.enemiesToAttack.Add (t.gameObject);
			}
				
			if(basic)
			{
				gameObject.SetActive(false);
			}
			else
			{
				woodsman.hitCount += 1;
				
				if(infinitePierce == false)
				{
					if(numPierces > 0)
					{
						numPierces -= 1;
					}
					else
					{
						gameObject.SetActive(false);
					}
				}
			}
		}
		else if(t.gameObject.CompareTag("wall"))
		{
			gameObject.SetActive(false);
		}
	}
}
