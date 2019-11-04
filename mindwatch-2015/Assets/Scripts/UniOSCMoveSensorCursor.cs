using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace UniOSC{
	
	/// <summary>
	/// Uni OSC scale game object.
	/// </summary>
	[AddComponentMenu("UniOSC/MoveSensorCursor")]
	public class UniOSCMoveSensorCursor:  UniOSCEventTarget {
		
		[HideInInspector]
		public Transform transformToScale;
		public float xScaleMax;
		public float xScaleMin;
		public float yScaleMax;
		public float yScaleMin;
		public float zScaleMax;
		public float zScaleMin;
		public bool allFourSensors = true;
		public float minTimeToData; // delay before starting processing
		
		public int sampleSize = 20;					// number of samples of _data to include in runningAverageValue
		public float runningAverageValue = 0.0f; 	//current running average of last sampleSize values of _data
		public float runningAverageMax = 0.0f; 	//current maximum running average of last sampleSize values of _data
		private int samplePointer = 0; 	//current location of pointer into the dataArray
		private float scaleFactor = 0; // computed scaling value (runningAverageMax scaled by scaleMax)
		public bool showMaxValueOnly = false; // if true, then scale to the highest value reached so far

		public bool scaleX = false;
		public bool scaleY = false;
		public bool scaleZ = false;
		
		public float startX = 0.0f;
		public float startY = 0.0f;
		public float startZ = 0.0f;
		private float scaleValueX = 1.0f;
		private float scaleValueY = 1.0f;
		private float scaleValueZ = 1.0f;


		private float xPos; 
		private float yPos; 
		private float zPos; 
		public bool xPosUsed; 
		public bool yPosUsed; 
		public bool zPosUsed; 
		
		private Vector3 _scale;
		private float _data = 0.1f;
		private float[] dataArray = new float[100];
		
		
		void Awake(){
//			_scale.Set(scaleValueX,scaleValueY,scaleValueZ);
		}
		
		private void _Init(){
			if(transformToScale == null){
				Transform hostTransform = GetComponent<Transform>();
				if(hostTransform != null) transformToScale = hostTransform;
			}
		}
		
		public override void OnEnable(){
			_Init();
			base.OnEnable();
		}
		
		public override void OnOSCMessageReceived(UniOSCEventArgs args){
			
			if(transformToScale == null) return;
			if(args.Message.Data.Count <1)return;
			if (Time.realtimeSinceStartup < minTimeToData) return;

			if (allFourSensors) {
			
								_data = ((float)args.Message.Data [0] +
										(float)args.Message.Data [1] +
										(float)args.Message.Data [2] +
										(float)args.Message.Data [3]) / 4; //average of all 4 sensors
						} else {
				_data = ((float)args.Message.Data [0]);
				}

			dataArray [samplePointer] = _data;
			runningAverageValue = 0;
			for(int i = 0; i < sampleSize; i++){
				runningAverageValue = runningAverageValue + dataArray[i];
			}
			runningAverageValue = runningAverageValue/sampleSize;
			samplePointer++;
			if (samplePointer > (sampleSize - 1)) {
				samplePointer = 0;
			}
			if (runningAverageValue > runningAverageMax) runningAverageMax = runningAverageValue;
			
			if (showMaxValueOnly) {
				scaleFactor = runningAverageMax * (yScaleMax * (1 - yScaleMin)) + yScaleMin;
				TextMesh textObject = GameObject.Find("MaxAlphaText").GetComponent<TextMesh>();
				textObject.text = runningAverageMax.ToString();
				Debug.Log ("Maximum Value = " + runningAverageMax);
			} else {
				scaleFactor = runningAverageValue * (zScaleMax * (1 - zScaleMin)) + zScaleMin;
			}
//			if (scaleX) scaleValueX = scaleFactor;
//			if (scaleY) scaleValueY = scaleFactor;
//			if (scaleZ) scaleValueZ = scaleFactor;
//			_scale.Set(scaleValueX,scaleValueY,scaleValueZ);
			
//			transformToScale.localScale = _scale;



			if(xPosUsed) xPos = xScaleMax * Time.realtimeSinceStartup;
			if(yPosUsed) yPos = runningAverageValue * (yScaleMax * (1 - yScaleMin)) + yScaleMin;
			if(zPosUsed) zPos = runningAverageValue * (zScaleMax * (1 - zScaleMin)) + zScaleMin;
			transform.position = new Vector3(xPos+startX, yPos+startY, zPos+startZ);	
		}
		
	}
}