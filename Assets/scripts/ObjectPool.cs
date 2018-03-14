using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class SpawnPoint
{
	public Transform _SpawnT;
	public Vector3 _SpawnPos;

	public Vector3 pSpawnPoint
	{
		get
		{
			Vector3 result = _SpawnPos;
			if(_SpawnT)
			{
				result = _SpawnT.position;
			}
			return result;
		}
	}
}

public enum StrideType
{
	Horizontal,
	Vertical
}

[Serializable]
public class PoolData
{
	public Sprite _Sprite;
	public int _SpriteOrder;
	public SpawnPoint _SpawnPoint;
	public StrideType _StrideType;
	public Vector3 _ExtraOffset;
	public int _Count;
	public bool _Activate;
	public Transform _Parent;
}

public class ObjectPool : MonoBehaviour
{
	public string _Name;
	public PoolData _PoolData;
	public BoxCollider2D _Bounds;

	public class ObjectData
	{
		public GameObject mGameObject;
		public Transform mTransform;
		public SpriteRenderer mSpriteRenderer;
	}
	
	private List<ObjectData> poolList = null;
	private int firstObjectIndex = 0;
	private int lastObjectIndex = 0;

	void Start()
	{
		poolList = new List<ObjectData>();
		for(int i = 0; i < _PoolData._Count; i++)
		{
			GameObject poolObject = new GameObject();
			SpriteRenderer spriteRenderer = poolObject.AddComponent<SpriteRenderer>();
			spriteRenderer.sortingOrder = _PoolData._SpriteOrder;
			spriteRenderer.sprite = _PoolData._Sprite;
			poolObject.transform.position = _PoolData._SpawnPoint.pSpawnPoint + i * GetSpriteStrideOffset(spriteRenderer);
			poolObject.name = _Name + "_" + i.ToString();
			poolObject.SetActive(_PoolData._Activate);
			poolObject.transform.SetParent(_PoolData._Parent);

			ObjectData data = new ObjectData();
			data.mGameObject = poolObject;
			data.mTransform = poolObject.transform;
			data.mSpriteRenderer = spriteRenderer;
			poolList.Add(data);
		}
		lastObjectIndex = _PoolData._Count - 1;
	}

	public Vector3 GetSpriteStrideOffset(SpriteRenderer spriteRenderer)
	{
		Vector3 offset = _PoolData._ExtraOffset;
		switch(_PoolData._StrideType)
		{
			case StrideType.Horizontal:
			{
				offset += Vector3.right * spriteRenderer.bounds.size.x;
			}break;

			case StrideType.Vertical:
			{
				offset += Vector3.up * spriteRenderer.bounds.size.y;
			}break;
		}
		return offset;
	}


	void Update()
	{
		//TODO:Fix bound checking based on stride type
		ObjectData firstObject = poolList[firstObjectIndex];
		float maxX = firstObject.mTransform.position.x + firstObject.mSpriteRenderer.bounds.size.x * 0.5f;
		if(maxX < _Bounds.bounds.min.x)
		{
			ObjectData lastObject = poolList[lastObjectIndex];
			firstObject.mTransform.position = lastObject.mTransform.position + GetSpriteStrideOffset(firstObject.mSpriteRenderer);
			lastObjectIndex = firstObjectIndex;

			firstObjectIndex++;
			if(firstObjectIndex == poolList.Count)
			{
				firstObjectIndex = 0;
			}
		}
	}
}
