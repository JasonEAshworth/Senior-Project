using UnityEngine;
using System.Collections;

public abstract class TrapBase : MonoBehaviour
{
	public float damage = 10.0f;	// amount of damage dealt to the player on contact
	public float lifetime = 5.0f;	// how long this trap object has to live

	protected virtual void FixedUpdate()
	{
		lifetime -= Time.deltaTime;
		if (lifetime <= 0.0f)
		{
			Destroy(gameObject);
		}
	}

	protected abstract void ActivateTrigger(bool state);

	// Can be overridden in a script that inherits from durationtrapobj in case we want
	// a duration trap to have an additional effect on the player
	public virtual void trapEffect(GameObject go) {}
}
