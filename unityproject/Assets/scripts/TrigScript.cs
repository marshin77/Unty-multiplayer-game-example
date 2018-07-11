using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TrigScript : MonoBehaviour 
{
	[SerializeField] GameObject collided;
	[SerializeField] GameObject mainplayer;


	void OnTriggerEnter(Collider col)
	{
		

		PlayerHealth health = mainplayer.GetComponent<PlayerHealth> ();
		collided = col.gameObject;
		if (collided.tag == "blueLazer") 
		{
			health.TakeDamage (10,collided);
		}
	}

}
