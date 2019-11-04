

var time : float = .2;
var min : float = .5;
var max : float = 5;
var useSmooth = false;
var smoothTime : float = 10;
var mGlow : GameObject;
var mColor : Color;




function Start () {
	if(useSmooth == false && GetComponent.<Light>()){
	InvokeRepeating("OneLightChange", time, time);
	mColor = mGlow.GetComponent.<Renderer>().material.GetColor( "_TintColor" );
	
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
	
		mColor.a = (((GetComponent.<Light>().intensity-min) / (max-min)) * 0.5f) + 0.5f;
		mGlow.GetComponent.<Renderer>().material.SetColor( "_TintColor", mColor );
	}
