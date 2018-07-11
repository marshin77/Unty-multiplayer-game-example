using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class netlazer : NetworkBehaviour 
{


	[SerializeField] float speed = 4f;
	
	// Update is called once per frame
	void Update () 
	{
		transform.Translate(0,0,-1 * speed * Time.deltaTime);
	}

}
