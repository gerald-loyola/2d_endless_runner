using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class GameData
{
	public string _Name;
	public float _DifficultyFactor;
	public float _InitSpeed;
	public float _MaxMoveSpeed;
}

[Serializable]
public class GameOverScreen
{
	public GameObject _Object;
	public Text _HighScore;
	public Text _CurrentScore;
}

public class World : MonoBehaviour 
{
	public GameData[] _GameDatas;
	public PlayerData _PlayerData;
	public ParallaxMove _ParallaxMover;
	public ObstaclePool _ObstaclePool;
	public SpawnPoint _ObstacleSpawnPoint;

	public GameOverScreen _GameOverScreen;
	public Text _Counter;
	public Text _StartText;

	public AudioClip _HitSfx;
	public AudioClip _JumpSfx;
	public AudioSource _AudioSource;

	private GameData gameData = new GameData();
	private Player player = null;
	private float nextObstacleSpawnTimer = 5.0f;
	private float spawnTimer = 0.0f;
	private float gameSpeed = 10.0f;
	private float moveTimer = 0.0f;
	private int objectDodged = 0;


	enum GameState
	{
		None,
		Initialized,
		InGame,
		GameOver
	}

	private GameState gameState = GameState.None;

	public void SetGameDifficulty(int difficultyLevel)
	{
		if (difficultyLevel > 0 && difficultyLevel <= _GameDatas.Length)
		{
			SetGameData(_GameDatas[difficultyLevel]);
		}

		SetState(GameState.Initialized);
	}

	void SetGameData(GameData gameData)
	{
		this.gameData = gameData;
		gameSpeed = gameData._InitSpeed;
	}

	void SetState(GameState newState)
	{
		gameState = newState;
		switch (newState)
		{
			case GameState.Initialized: 
			{
				_StartText.gameObject.SetActive(false);
				_Counter.gameObject.SetActive(true);
			}break;
		}
	}

	void Start()
	{
		player = new Player(_PlayerData, transform);
	}

	void SetSpeed(float speed)
	{
		_ParallaxMover._Speed = speed;
		_ObstaclePool.SetSpeed(speed);
	}

	public void Restart()
	{
		Application.LoadLevel(Application.loadedLevelName);
	}

	void UpdateCounter()
	{
		_Counter.text = objectDodged.ToString();	
	}

	void GameOver()
	{ 
		_AudioSource.PlayOneShot(_HitSfx);
		player.Dead();
		SetSpeed(0.0f);
		SetState(GameState.GameOver);

		int score = objectDodged * 100;
		int highScore = Mathf.Max(player.pPlayerData.HighScore, score);
		player.SetHighScore(score);
		_GameOverScreen._HighScore.text = highScore.ToString();
		_GameOverScreen._CurrentScore.text = score.ToString();
		Invoke("ShowHighScore", 1.0f);
	}

	void ShowHighScore()
	{
		_GameOverScreen._Object.SetActive(true);
	}

	void Update()
	{
		switch(gameState)
		{
			case GameState.Initialized:
			{
				moveTimer += Time.deltaTime;
				player.MoveTo(Vector3.Lerp(_PlayerData._SpawnPoint.pSpawnPoint, 	_PlayerData._StartPoint.pSpawnPoint, moveTimer * 1.0f));
				if(moveTimer >= 1.0f)
				{
					moveTimer = 0.0f;
					SetState(GameState.InGame);
				}
			}break;

			case GameState.InGame:
			{
				gameSpeed += Time.deltaTime * gameData._DifficultyFactor;
				SetSpeed(gameSpeed);
				player.Update();
				
				for(int i = 0; i < _ObstaclePool.pActiveList.Count; i++)
				{
					ObstaclePool.ObstacleCache obstacle = _ObstaclePool.pActiveList[i];
					if(player.IsTouching(obstacle.mCollider))
					{
						GameOver();
					    return;
					}

					float angle = Vector3.Dot(Vector3.right, (player.pTransform.position - obstacle.mTransform.position).normalized);
					//if (angle > 0.5f && angle < 0.8f)
					//{
					//	Debug.Log("Angle : " + angle);
					//	Debug.Break();
					//}

					if(obstacle.mCollider.isTrigger == false && 
						angle > 0.8f)
					{	
						objectDodged++;
						obstacle.mCollider.isTrigger = true;
						UpdateCounter();
					}
				}
				
				spawnTimer += Time.deltaTime;
				if(spawnTimer >= nextObstacleSpawnTimer)
				{
					spawnTimer = 0.0f;
					nextObstacleSpawnTimer = 2.0f + UnityEngine.Random.Range(0, 2.0f);
					_ObstaclePool.Spawn(_ObstacleSpawnPoint);
				}
			}break;
		}
	}
}
