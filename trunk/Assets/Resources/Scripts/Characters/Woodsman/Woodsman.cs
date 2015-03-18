using UnityEngine;
using System.Collections;

public class Woodsman : PlayerBase
{
	// objects init
	private Transform shootPosition;
	private GameObject hawk;
	private Transform hawkPos;
	private HawkAI2 hawkScripts;	
	public Animator anim;
	private LineRenderer lr;
	private GameObject bomb;
	public GameObject objectHit;

	// bool inits
	private bool canFire = true;
	private bool canSpecial = true;
	private bool attacking = false;
	private bool canMove = true;
	private bool bombActive = false;

	// init variables
	private float timeBNAttacks = 5.0f;
	private float basicTimer = 0.5f;
	private float specialTimer = 3.0f;
	private float firstButtonPressTime = 0.0f;
	public float zeroMoveTimer = 0.0f;
	private float canMoveTimer = 0.0f;
	private int lineSegments = 2;
	private float lineWidth = 0.09f;
	private float lineLength = 35.0f;
	private Vector3 lineEndPoint = Vector3.zero;
	public int hitCount = 0;
	private float dmg = 20.0f;

	public void init()
	{
		// initialize the linerenderer to hold only 2 positions
		lr = gameObject.GetComponent<LineRenderer> ();
		lr.SetVertexCount (lineSegments);
		lr.SetWidth(lineWidth, lineWidth);

		// get the animator object
		anim = GetComponent<Animator> ();

		health = 100;
		maxHealth = health;

		maxMana = 3.0f;
		mana = 0.0f;

		// instantiate the hawk at the hawkspawn position
		hawkPos = transform.Find("hawkSpawn");
		hawk = Instantiate(Resources.Load("Prefabs/Character/WoodsMan/Hawk"),hawkPos.position,Quaternion.identity) as GameObject;

		// acquire the position from where to shoot arrows
		shootPosition = transform.Find("shootPos");

		// set the moveTimer to 0 at the beginning
		canMoveTimer = 0.0f;

		// Get the hawk script to be able to set modes
		hawkScripts = hawk.GetComponent<HawkAI2> ();
	}

	protected override void Update()
	{
		// call update of parent class
		base.Update();

		timeBNAttacks -= Time.deltaTime;
		if(timeBNAttacks <= 0.0f)
		{
			timeBNAttacks = 5.0f;
			hawkScripts.enemiesToAttack.Clear ();
		}

		if(hitCount >= 5)
		{
			Debug.Log ("Added Mana");
			addMana(1.0f);
			hitCount = 0;
		}

		// Cast out a ray to figure out if the aim assist will hit anything
		// if so make it the contact point of the collision for the end of the line
		RaycastHit hit;
		Ray rayCast = new Ray (transform.position, transform.forward);
		if(Physics.Raycast(rayCast,out hit, lineLength,~LayerMask.GetMask ("CaptureBox")))
		{
			lineEndPoint = new Vector3(hit.point.x,shootPosition.position.y,hit.point.z);
			objectHit = hit.collider.gameObject; 
		}
		else
		{
			lineEndPoint = (shootPosition.forward*lineLength)+new Vector3(transform.position.x,shootPosition.position.y,transform.position.z);
		}

		// render line
		for(int i=0;i<lineSegments;i++)
		{
			if(i == 0)
			{
				lr.SetPosition(i,shootPosition.position);
			}
			if(i == 1)
			{
				lr.SetPosition(i,lineEndPoint);
			}
		}


		hawk.transform.position = new Vector3 (hawk.transform.position.x, hawkPos.position.y, hawk.transform.position.z);
		if (!canFire) 
		{
			basicTimer -= Time.deltaTime;
			if(basicTimer <= 0.0f)
			{
				canFire = true;
				basicTimer = 0.5f / attackSpeed;
			}
		}
		
		if (!canSpecial) 
		{
			specialTimer -= Time.deltaTime;
			if(specialTimer <= 0.0f)
			{
				canSpecial = true;
				specialTimer = 3.0f;
			}
			
		}


		if(!canMove)
		{
			canMoveTimer += Time.deltaTime;
			if(canMoveTimer >= 0.04f)
			{
				moveMulti = 0.3f;
			}
		}

		//Debug.Log (hawkCost);

	}

	public override void basicAttack(string dir)
	{
		//Debug.Log ("warrior basic attack");
		if (dir == "down" && canFire) 
		{
			firstButtonPressTime = Time.time;
			timeBNAttacks = 5.0f;
			canMove = false;
		}
		if (dir == "up")
		{
			float temp = Time.time - firstButtonPressTime;
			firstButtonPressTime = Time.time;
			canMove = true;
			moveMulti = 1.0f;
			canMoveTimer = 0.0f;
			if(temp > 0.7f / attackSpeed && canSpecial)
			{
				specialAttackWoods(temp);
			}
			else if(canFire)
			{
				anim.SetTrigger("Attack");
				if(objectHit.CompareTag("Enemy"))
				{
					objectHit.SendMessage("takeDamage",dmg *attackMultiplier);
					hawkScripts.enemiesToAttack.Add (objectHit.gameObject);
					hitCount += 1;
					EnemyBase scr = objectHit.GetComponent<EnemyBase>();
					scr.damageTaken += dmg;
				}
				else if(objectHit.CompareTag("HawkTrigger"))
				{
					Debug.Log ("added a target");
					hawk.SendMessage("setTarget",objectHit);
				}
//				GameObject bullet = Instantiate (Resources.Load ("Prefabs/Character/WoodsMan/woodsManBullet"), shootPosition.position, Quaternion.LookRotation(transform.forward)) as GameObject;
//				//bullet.transform.up = transform.forward;
//				canFire = false;
			}
		}
	}
	
	public void specialAttackWoods(float time)
	{
		anim.SetTrigger("Attack");
//		GameObject specialBullet = Instantiate (Resources.Load ("Prefabs/Character/WoodsMan/woodsManSpecial"), shootPosition.position, Quaternion.identity) as GameObject;
//		woodsSpecialBulletScript scr = specialBullet.GetComponent<woodsSpecialBulletScript>();
//		scr.heldTime = time;


		canSpecial = false;
	}
	
	public override void classAbility(string dir)
	{
		//Debug.Log ("warrior class ability");
		if (dir == "down") 
		{

			if(!bombActive && mana >= 1.0f)
			{
				useMana(1.0f);

				Vector3 bombPos = Vector3.zero;
				Vector3 bombUP = Vector3.zero;
				RaycastHit hit;
				Ray rayCast = new Ray (transform.position, -Vector3.up);
				if(Physics.Raycast(rayCast,out hit, 5.0f))
				{
					bombPos = hit.point; 
					bombUP = hit.transform.up;
				}
				bomb = Instantiate(Resources.Load ("Prefabs/Character/WoodsMan/bomb"),new Vector3(transform.position.x,bombPos.y + 0.261836f,transform.position.z),Quaternion.identity) as GameObject;
				bomb.transform.up = bombUP;
				bombActive = true;
			}
			else
			{
				if(bomb)
				{
					bombBehavior scr = bomb.GetComponent<bombBehavior>();
					scr.explode = true;
					bombActive = false;
				}

			}
//			if (hawkScripts.mode != 2 && hawkScripts.mode != 3  && mana > hawkCost) 
//			{
//				anim.SetTrigger("Hawk");
//				hawkScripts.mode = 2;
//			}
		}
	}
}
