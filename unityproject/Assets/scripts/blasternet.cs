using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class blasternet :NetworkBehaviour 
{
	[SerializeField] float fireRate = 4.0f;
	[SerializeField] Vector3 pos;
	[SerializeField] Transform blaster;
	[SerializeField] float nextShot = 0.0f;
	[SerializeField] GameObject Lazer;
	[SerializeField] bool canShoot = false;
	void Rest()
	{
		blaster = transform.Find ("testblaster");
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(!isLocalPlayer)
		{
			return;
		}
		pos = transform.position;
		if(Input.GetButtonDown("Fire1") && canShoot == false)
		{
			nextShot = Time.time + fireRate;
			canShoot = true;
		}

		if(Input.GetButtonUp("Fire1") )
		{
			canShoot = false;
		}
		if(Time.time >= nextShot && canShoot == true)
		{
			canShoot = false;
			CmdSpawnLazer();
		}
	}
	[Command]
	void CmdSpawnLazer()
	{
		GameObject instance = Instantiate (Lazer, blaster.position, blaster.rotation) as GameObject;
		NetworkServer.Spawn (instance);
	}
}
