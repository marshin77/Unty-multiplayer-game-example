using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerHealth : NetworkBehaviour  {

	public int health = 40;




	public void TakeDamage (int amount,GameObject lazer)
	{
		if(!isServer)
		{
			return;
		}
		NetworkServer.Destroy (lazer);
		health -= amount;
		if (health <= 0) 
		{
			CmdKill();
		}
	}



	[Command]
	void CmdKill()
	{
		GetComponent<CapsuleCollider>().enabled = false;
		GetComponent<MeshRenderer>().enabled = false;
	}
}
