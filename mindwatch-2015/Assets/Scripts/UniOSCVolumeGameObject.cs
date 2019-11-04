/*
* UniOSC
* Copyright Â© 2014 Stefan Schlupek
* All rights reserved
* info@monoflow.org
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


namespace UniOSC{

	/// <summary>
	/// Uni OSC volume game object.
	/// </summary>
	[RequireComponent(typeof(AudioSource))]
	[AddComponentMenu("UniOSC/VolumeGameObject")]

	public class UniOSCVolumeGameObject :  UniOSCEventTarget {

		[HideInInspector]
		public Transform transformToScale;
		public float scaleMax;
		public float scaleMin;
		public int indexCount = 0;
		public float minTimeToData;

		private Vector3 _scale;
		private float _data;


		void Awake(){
			GetComponent<AudioSource>().volume = 0;
			GetComponent<AudioSource>().Play();
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
			_data = 0;
			for (int i = 0; i <=indexCount; i++) {
				_data += (float)args.Message.Data[i];
						}
			_data = _data / (1 + indexCount);
			_data = _data*(scaleMax*(1-scaleMin)) + scaleMin;
//			_scale.Set(_data,_data,_data);
			GetComponent<AudioSource>().volume = _data;

//			transformToScale.localScale = _scale;

		}

	}
}