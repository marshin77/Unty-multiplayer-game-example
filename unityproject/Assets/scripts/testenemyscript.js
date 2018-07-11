#pragma strict
var collided : GameObject;
var filler : GameObject;
filler = new GameObject("blank");
collided = filler;
var health : int = 40;
function Start () {
	
}

function Update () {

if(health<=0){
	Destroy (gameObject);
}
	
}
function OnCollisionEnter(col : Collision){

	collided = col.gameObject;
	if(collided.tag == "blueLazer"){
		Destroy (collided);
		health = health - 10;
	}

}

function OnCollisionStay(col : Collision){

	collided = col.gameObject;

}

function OnCollisionExit(col : Collision){

	collided = filler;




}