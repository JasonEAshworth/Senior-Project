using UnityEngine;
using System.Collections;

public class DropGold : MonoBehaviour {


	int numGold = Random.Range(0,5);
	int numCoins = Random.Range(0,10);
	public int goldDropRate;
	public int potionDropRate;
	public GameObject gld;
	public GameObject Coin;
	public GameObject Ptn;
	
	/*void Update ()
	{
		if(Input.GetKeyDown(KeyCode.Space))
		{
			for(int g = 0; g < numGold; g++)
				GameObject.Instantiate(gld, new Vector3(transform.position.x + Random.Range(-1.0F,1.0F),transform.position.y,transform.position.z - Random.Range(-1.0F,1.0F)), transform.rotation);
			Destroy(gameObject);
		}
	}*/
	
	void OnTriggerEnter(Collider player)
	{
		if (player.gameObject.CompareTag ("Player")) 
		{
			int item = Random.Range(0, 100);
			if(item < goldDropRate)
			{
				for(int g = 0; g < numCoins; g++)
					GameObject.Instantiate(Coin, new Vector3(transform.position.x + Random.Range(-1.0F,1.0F),transform.position.y,transform.position.z - Random.Range(-1.0F,1.0F)), transform.rotation);
				for(int g = 0; g < numGold; g++)
					GameObject.Instantiate(gld, new Vector3(transform.position.x + Random.Range(-1.0F,1.0F),transform.position.y,transform.position.z - Random.Range(-1.0F,1.0F)), transform.rotation);
			}
			if(item >= 100 - potionDropRate)
			{
				GameObject.Instantiate(Ptn, new Vector3(transform.position.x + Random.Range(-1.0F,1.0F),transform.position.y,transform.position.z - Random.Range(-1.0F,1.0F)), transform.rotation);
			}
			Destroy(gameObject);
		}
		
	}
}
