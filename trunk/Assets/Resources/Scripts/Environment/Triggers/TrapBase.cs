using UnityEngine;
using System.Collections;

public abstract class TrapBase : MonoBehaviour
{
	public bool spawner = false;
	public float damage = 10.0f;	// amount of damage dealt to the player on contact
	public float lifetime = 5.0f;	// how long this trap object has to live

	protected virtual void FixedUpdate()
	{
		if(!spawner)
		{
			lifetime -= Time.deltaTime;
			if (lifetime <= 0.0f)
			{
				Destroy(gameObject);
			}
		}
	}

	protected virtual void ActivateTrigger(bool state) {}

	protected abstract void OnTriggerEnter(Collider c);

	protected abstract void HitObject(Transform t);

	// Can be overridden in a script that inherits from TrapBase in case we want
	// a trap to have an additional effect on the player
	public virtual void trapEffect(GameObject go) {}
}
