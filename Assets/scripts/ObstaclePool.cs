using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class ObstacleData
{
	public string _Name;
	public Sprite _Sprite;
	public Vector3 _SpawnOffset;
	public Sprite _ShadowSprite;
	public Vector3 _ShadowOffset;
	public int _Count;
	public int _SpriteOrder;
}

public class ObstaclePool : MonoBehaviour 
{
	public ObstacleData[] _ObstacleInfos;
	public BoxCollider2D _WorldBounds;

	public class ObstacleCache
	{
		public int mDataIndex;
		public GameObject mGameObject;
		public Transform mTransform;
		public Collider2D mCollider;
	}

	private List<ObstacleCache> poolList = new List<ObstacleCache>();
	private List<ObstacleCache> activeList = new List<ObstacleCache>();
	public List<ObstacleCache> pActiveList
	{
		get{ return activeList; }
	}

	private float speed = 1.0f;

	void Start()
	{
		for(int i = 0; i < _ObstacleInfos.Length; i++)
		{
			ObstacleData data = _ObstacleInfos[i];
			for(int j = 0; j < data._Count; j++)
			{
				GameObject poolObject = new GameObject();
				SpriteRenderer spriteRenderer = poolObject.AddComponent<SpriteRenderer>();
				spriteRenderer.sortingOrder = data._SpriteOrder;
				spriteRenderer.sprite = data._Sprite;
				poolObject.transform.position = Vector3.up * 10000;
				BoxCollider2D collider = poolObject.AddComponent<BoxCollider2D>();
				collider.size *= 0.5f;
				poolObject.name = data._Name + "_" + j.ToString();
				poolObject.SetActive(false);
				poolObject.transform.SetParent(transform);

				if(data._ShadowSprite)
				{
					GameObject shadowObject = new GameObject();
					SpriteRenderer shadowSpriteRenderer = shadowObject.AddComponent<SpriteRenderer>();
					shadowSpriteRenderer.sortingOrder = data._SpriteOrder;
					shadowSpriteRenderer.sprite = data._ShadowSprite;
					shadowObject.name = "shadow";
					shadowObject.transform.position = poolObject.transform.position + data._ShadowOffset;
					shadowObject.transform.SetParent(poolObject.transform);
				}

				ObstacleCache cache = new ObstacleCache();
				cache.mGameObject = poolObject;
				cache.mCollider = collider;
				cache.mTransform = poolObject.transform;
				cache.mDataIndex = i;
				poolList.Add(cache);
			}
		}
	}

	public void Spawn(SpawnPoint spawnPoint)
	{
		if(poolList.Count > 0)
		{
			int randIndex = UnityEngine.Random.Range(0, poolList.Count);
			ObstacleCache obstacle = poolList[randIndex];
			obstacle.mCollider.isTrigger = false;
			obstacle.mTransform.position = spawnPoint.pSpawnPoint + _ObstacleInfos[obstacle.mDataIndex]._SpawnOffset; 
			obstacle.mGameObject.SetActive(true);
			poolList.Remove(obstacle);
			activeList.Add(obstacle);
		}
	}

	public void SetSpeed(float speed)
	{
		this.speed = speed;
	}

	public void Update()
	{
		if(activeList.Count > 0)
		{
			for(int i = 0; i < activeList.Count; i++)
			{
				ObstacleCache obstacle = activeList[i];
				obstacle.mTransform.position += Vector3.left * speed * Time.deltaTime;
			}

			//TODO:Fix bound checking based on movement type
			ObstacleCache firstObject = activeList[0];
			float maxX = firstObject.mTransform.position.x + firstObject.mCollider.bounds.size.x * 0.5f;
			if(maxX < _WorldBounds.bounds.min.x)
			{
				activeList.Remove(firstObject);
				firstObject.mGameObject.SetActive(false);
				poolList.Add(firstObject);
			}
		}
	}
}
