

var time : float = .2;
var min : float = .5;
var max : float = 5;
var useSmooth = false;
var smoothTime : float = 10;
var mGlow : GameObject;

function Start () {
	if(useSmooth == false && GetComponent.<Light>()){
	InvokeRepeating("OneLightChange", time, time);
	}
}


function OneLightChange () {
	GetComponent.<Light>().intensity = Random.Range(min,max);
	//color.a = (((x-3) / (5-3)) * 0.5f) + 0.5f;
}

function Update () {
	if(useSmooth == true && GetComponent.<Light>()){
		GetComponent.<Light>().intensity = Mathf.Lerp(GetComponent.<Light>().intensity,Random.Range(min,max),Time.deltaTime*smoothTime);
	}
	if(GetComponent.<Light>() == false){
		print("Please add a light component for light flicker");
		mGlow.GetComponent.<Renderer>().material.color.a = (((GetComponent.<Light>().intensity-min) / (max-min)) * 0.5f) + 0.5f;
	}
}