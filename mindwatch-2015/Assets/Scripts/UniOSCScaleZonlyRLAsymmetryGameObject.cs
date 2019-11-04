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
	[AddComponentMenu("UniOSC/ScaleZonlyRLAsymmetryGameObject")]
	public class UniOSCScaleZonlyRLAsymmetryGameObject :  UniOSCEventTarget {

		[HideInInspector]
		public Transform transformToScale;
		public float scaleMax;
		public float scaleMin;
//		public int indexCount; // Not needed here: Always uses sensor1 (Left front) and sensor2 (Right front)
		public float minTimeToData;

		private Vector3 _scale;
		private float _dataLeft = 0.00001f;
		private float _dataRight = 0.00001f;
		private float _dataResult = 0.00001f;
		public float barLength = 0; // bar scale length in Z axis (runningAverageMax scaled by scaleMa


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
			if (Time.realtimeSinceStartup < minTimeToData) return;

			_dataLeft = (float)args.Message.Data [1];
			_dataRight = (float)args.Message.Data [2];
			_dataResult = _dataRight / _dataLeft; // ratio of R/L
			barLength = Mathf.Log(_dataResult) + 10;
			barLength = barLength*(scaleMax*(1-scaleMin)) + scaleMin; // scale bar to display proper length on screen
			_scale.Set(1.0f,1.0f,barLength);

			transformToScale.localScale = _scale;

		}

	}
}