#pragma strict
var fireRate : float = 4.0;
var pos = Vector3(0,0,0);
var nextShot : float = 0.0;
var Lazer : GameObject;
var canShoot : boolean = false;

function Start () {
	
}

function Update () {
pos = transform.position;
if(Input.GetButtonDown("Fire1") && canShoot == false){
	nextShot = Time.time + fireRate;
	canShoot = true;
}

if(Input.GetButtonUp("Fire1") ){
	canShoot = false;
}
if(Time.time >= nextShot && canShoot == true){
	canShoot = false;
	shoot();
}




	
}


function shoot() : void {

	
	Instantiate(Lazer, Vector3(pos.x,pos.y,pos.z), Quaternion.Euler(transform.eulerAngles.x,transform.eulerAngles.y,transform.eulerAngles.z));

}


