#pragma strict
var collided : GameObject;
var filler : GameObject;
filler = new GameObject("blank");
collided = filler;
var speed : float = 5;

function Start () {
	
}

function Update () {

	transform.Translate(Vector3(0,0,-1 * speed * Time.deltaTime));
	
}
function OnCollisionEnter(col : Collision){

	collided = col.gameObject;
	if(collided.tag != "red"){
		Destroy (gameObject);
	}

}

function OnCollisionStay(col : Collision){

	collided = col.gameObject;

}

function OnCollisionExit(col : Collision){

	collided = filler;
	Destroy (gameObject);



}