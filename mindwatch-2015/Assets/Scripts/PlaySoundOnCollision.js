var soundFile:AudioClip;
var notPlayedYet:boolean = true;
  
 function OnTriggerEnter(trigger:Collider) {
      if(trigger.GetComponent.<Collider>().tag=="SensorMax") {
         GetComponent.<AudioSource>().clip = soundFile;
         if(notPlayedYet){
         	GetComponent.<AudioSource>().Play();
         	notPlayedYet = false;
         	}
      }
 }