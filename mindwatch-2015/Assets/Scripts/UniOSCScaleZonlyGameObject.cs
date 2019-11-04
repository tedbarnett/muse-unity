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
	[AddComponentMenu("UniOSC/ScaleZonlyGameObject")]
	public class UniOSCScaleZonlyGameObject :  UniOSCEventTarget {

		[HideInInspector]
		public Transform transformToScale;
		public float scaleMax;
		public float scaleMin;
		public int indexCount;
		public float minTimeToData;

		private Vector3 _scale;
		private float _data = 0.1f;


		void Awake(){
			_scale.Set(_data,_data,_data);
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

			_data = ((float)args.Message.Data [0] +
								(float)args.Message.Data [1] +
								(float)args.Message.Data [2] +
								(float)args.Message.Data [3]) / 4; //average of all 4 sensors

//			_data = 0;
//			for (int i = 0; i <=indexCount; i++) {
//				_data = _data + (float)args.Message.Data[i];
//			}
//			_data = _data / (1 + indexCount);
			_data = _data*(scaleMax*(1-scaleMin)) + scaleMin;
			_scale.Set(1.0f,1.0f,_data);
//			ParticleSystem.emissionRate = _data*cloudemissionRate;

			transformToScale.localScale = _scale;

		}

	}
}