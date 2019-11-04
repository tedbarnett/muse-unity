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
	[AddComponentMenu("UniOSC/UniOSCScaleRunningAverageMaxGameObject")]
	public class UniOSCScaleRunningAverageMaxGameObject :  UniOSCEventTarget {

		[HideInInspector]
		public Transform transformToScale;
		public float scaleMax;
		public float scaleMin;
		public int indexCount;
		public float minTimeToData;
		public int sampleSize = 100;			// number of samples of _data to include in runningAverageValue
		public float runningAverageValue = 0.0f; 	//current running average of last sampleSize values of _data
		public float runningAverageMax = 0.0f; 	//current maximum running average of last sampleSize values of _data
		public int samplePointer = 0; 	//current location of pointer into the dataArray
		public float barLength = 0; // bar scale length in Z axis (runningAverageMax scaled by scaleMax)

		private Vector3 _scale;
		private float _data = 0.0f; // latest sample collected
		private float[] dataArray = new float[200];


		void Awake(){
			_scale.Set(1.0f,1.0f,1.0f);
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

			if (runningAverageValue > runningAverageMax) {
				runningAverageMax = runningAverageValue;
//				Debug.Log ("runningAverageMax = " + runningAverageMax + ", _data = " + _data);
			}
			barLength = runningAverageMax*(scaleMax*(1-scaleMin)) + scaleMin;

			_scale.Set(1.0f,1.0f,barLength);
//			ParticleSystem.emissionRate = _data*cloudemissionRate;

			transformToScale.localScale = _scale;

		}

	}
}