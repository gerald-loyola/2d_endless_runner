using UnityEngine;
using System.Collections;
using System;

public class ParallaxMove : MonoBehaviour 
{
	[Serializable]
	public class ParallaxData
	{
		public ObjectPool _Object;
		public float _SpeedRatio01;
	}

	public ParallaxData[] _ParallaxObjects;
	public float _Speed = 0.2f;
	public Vector3 _Direction = Vector3.left;

	public void Update()
	{
		Vector3 velocity = _Direction * _Speed * Time.deltaTime;
		for(int i = 0; i < _ParallaxObjects.Length; i++)
		{
			ParallaxData data = _ParallaxObjects[i];
			ObjectPool pool = data._Object;
			pool.transform.position += velocity * data._SpeedRatio01;
		}
	}
}
