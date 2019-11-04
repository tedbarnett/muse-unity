/*
* UniOSC
* Copyright © 2014 Stefan Schlupek
* All rights reserved
* info@monoflow.org
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


namespace UniOSC{

	/// <summary>
	/// Uni OSC scale game object.
	/// </summary>
	[AddComponentMenu("UniOSC/UniOSCScaleRunningAverageGameObject")]
	public class UniOSCScaleRunningAverageGameObject :  UniOSCEventTarget {

		[HideInInspector]
		public Transform transformToScale;
		public float scaleMax;
		public float scaleMin;
//		public int indexCount;
		public float minTimeToData;
		public int sampleSize = 20;					// number of samples of _data to include in runningAverageValue
		public float runningAverageValue = 0.0f; 	//current running average of last sampleSize values of _data
		public float runningAverageMax = 0.0f; 	//current maximum running average of last sampleSize values of _data
		private int samplePointer = 0; 	//current location of pointer into the dataArray
		private float scaleFactor = 0; // computed scaling value (runningAverageMax scaled by scaleMax)
		public bool showMaxValueOnly = false; // if true, then scale to the highest value reached so far
		public bool scaleX = false;
		public bool scaleY = false;
		public bool scaleZ = true;
		private float scaleValueX = 1.0f;
		private float scaleValueY = 1.0f;
		private float scaleValueZ = 1.0f;

		private Vector3 _scale;
		private float _data = 0.0f; // latest sample collected
		private float[] dataArray = new float[100];


		void Awake(){
			_scale.Set(scaleValueX,scaleValueY,scaleValueZ);
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
			if (Time.realtimeSinceStartup < minTimeToData) return; // don't start reading until mintimeToData reached

			_data = ((float)args.Message.Data [0] +
								(float)args.Message.Data [1] +
								(float)args.Message.Data [2] +
								(float)args.Message.Data [3]) / 4; //average of all 4 sensors
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
								scaleFactor = runningAverageMax * (scaleMax * (1 - scaleMin)) + scaleMin;
								TextMesh textObject = GameObject.Find("MaxAlphaText").GetComponent<TextMesh>();
								textObject.text = runningAverageMax.ToString();
//								Debug.Log ("Maximum Value = " + runningAverageMax);
						} else {
								scaleFactor = runningAverageValue * (scaleMax * (1 - scaleMin)) + scaleMin;
						}
				if (scaleX) scaleValueX = scaleFactor;
				if (scaleY) scaleValueY = scaleFactor;
				if (scaleZ) scaleValueZ = scaleFactor;
			_scale.Set(scaleValueX,scaleValueY,scaleValueZ);

			transformToScale.localScale = _scale;

		}

	}
}