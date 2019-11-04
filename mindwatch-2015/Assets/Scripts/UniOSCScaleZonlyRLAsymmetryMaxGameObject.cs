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
	[AddComponentMenu("UniOSC/ScaleZonlyRLAsymmetryMaxGameObject")]
	public class UniOSCScaleZonlyRLAsymmetryMaxGameObject :  UniOSCEventTarget {

		[HideInInspector]
		public Transform transformToScale;
		public float scaleMax;
		public float scaleMin; // used here to ensure no negative results
		public float minTimeToData;
		public int sampleSize = 100;			// number of samples of _data to include in runningAverageValue
		public float runningAverageValue = 0.0f; 	//current running average of last sampleSize values of _data
		public float runningAverageMax = 0.0f; 	//current maximum running average of last sampleSize values of _data
		public int samplePointer = 0; 	//current location of pointer into the dataArray
		public float barLength = 0; // bar scale length in Z axis (runningAverageMax scaled by scaleMax)
		public float barLengthPositive = 10.0f; // ln result could be negative so add this value to bar length plot is positive
		
		private Vector3 _scale;
		private float _data = 0.0f; // latest sample collected
		private float[] dataArray = new float[200];

		private float _dataLeft = 0.00001f;
		private float _dataRight = 0.00001f;
		private float _dataResult = 0.00001f;


		void Awake(){
			_scale.Set(1.0f,1.0f,1.0f);
			barLength = scaleMin;
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


			_dataLeft = (float)args.Message.Data [1];
			_dataRight = (float)args.Message.Data [2];
			_dataResult = Mathf.Log(_dataRight / _dataLeft); // Natural log of ratio of R/L (higher is better?) -- Could be negative!
			dataArray [samplePointer] = _dataResult;
			runningAverageValue = 0;
		
			for(int i = 0; i < sampleSize; i++){
				runningAverageValue = runningAverageValue + dataArray[i];
				}
			runningAverageValue = runningAverageValue/sampleSize;
			samplePointer++;
			if (samplePointer > (sampleSize - 1)) {
				samplePointer = 0;
				}
			//			
			if (runningAverageValue > runningAverageMax) {
				runningAverageMax = runningAverageValue;
//				Debug.Log ("NEW runningAverageMax = " + runningAverageMax + ", _dataResult = " + _dataResult);
				}


			barLength = (runningAverageMax)*(scaleMax); // scale bar to display proper length on screen
			if (barLength < scaleMin) {
						barLength = scaleMin;
						}
			_scale.Set(1.0f,1.0f,barLength);

			transformToScale.localScale = _scale;

		}

	}
}