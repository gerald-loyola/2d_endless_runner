using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class PlayerData
{
	public string _Name;
	public Sprite _Sprite;
	public Sprite _ShadowSprite;
	public Vector3 _ShadowOffset;
	public SpawnPoint _SpawnPoint;
	public SpawnPoint _StartPoint;
	public float _JumpSpeed;

	public RuntimeAnimatorController _RunAnim;
	public RuntimeAnimatorController _JumpAnim;
	public RuntimeAnimatorController _DeadAnim;

	public int HighScore;
}

public class Player
{
	enum State
	{
		None,
		Run,
		Jump,
		Dead
	}

	private Transform transform = null;
	public Transform pTransform
	{
		get { return transform;}
	}

	private Transform shadowTransform = null;

	private GameObject gameObject = null;
	private BoxCollider2D boxCollider = null;
	private SpriteRenderer renderer = null;
	private SpriteRenderer shadowRenderer = null;
	private Animator animator = null;
	private State state;
	private PlayerData playerData;
	public PlayerData pPlayerData
	{
		get { return playerData;}
	}

	private Vector3 velocity = Vector3.zero;
	private string scoreKey = "";

	public Player(PlayerData playerData, Transform parent)
	{
		gameObject = new GameObject();
		transform = gameObject.transform;
		boxCollider = gameObject.AddComponent<BoxCollider2D>();
		transform.position = playerData._SpawnPoint.pSpawnPoint;
		renderer = gameObject.AddComponent<SpriteRenderer>();
		renderer.sortingOrder = 0;
		renderer.sprite = playerData._Sprite;
	
		gameObject.name = playerData._Name;
		gameObject.SetActive(true);
		transform.SetParent(parent);

		//bob shadow
		GameObject shadowObject = new GameObject();
		shadowObject.transform.position = playerData._SpawnPoint.pSpawnPoint + playerData._ShadowOffset;
		shadowTransform  = shadowObject.transform;
		shadowRenderer = shadowObject.AddComponent<SpriteRenderer>();
		shadowRenderer.sortingOrder = -1;
		shadowRenderer.sprite = playerData._ShadowSprite;

		shadowObject.name = playerData._Name + "_shadow";
		shadowObject.SetActive(true);
		shadowObject.transform.SetParent(parent);

		animator = gameObject.AddComponent<Animator>();
		this.playerData = playerData;
		SetState(State.Run);

		scoreKey = string.Format("{0}_{1}", playerData._Name, "score");
		playerData.HighScore = PlayerPrefs.GetInt(scoreKey, 0);
	}

	void SetState(State newState)
	{
		if(newState != state)
		{
			state = newState;
			switch(newState)
			{
				case State.Run:
				{
					animator.runtimeAnimatorController = playerData._RunAnim;
				}break;

				case State.Jump:
				{
					animator.runtimeAnimatorController = playerData._JumpAnim;
					velocity = Vector3.up * playerData._JumpSpeed;
				}break;

				case State.Dead:
				{
					animator.runtimeAnimatorController = playerData._DeadAnim;
				}break;
			}
		}
	}

	const float Max_Dampner = 0.0f;
	float dampner = Max_Dampner;

	void UpdateState()
	{
		switch(state)
		{
			case State.Jump:
			{
				Vector3 acceleration = Vector3.up * -9.8f * 2.0f;
				transform.position += (0.5f * acceleration * Time.deltaTime * Time.deltaTime) + velocity * Time.deltaTime;
				velocity += acceleration * Time.deltaTime;

				Vector3 startPoint = playerData._StartPoint.pSpawnPoint;
				if (transform.position.y < startPoint.y)
				{
					transform.position = startPoint;
					//SetState(State.Run);
					velocity += Vector3.up * playerData._JumpSpeed * dampner;
					dampner -= Time.deltaTime * 5f;
				}

				if (dampner < 0.0f)
				{
					dampner = Max_Dampner;
					SetState(State.Run);
				}

				float jumpHeight = Vector3.Distance(transform.position, startPoint);
				float halfJumpH = playerData._JumpSpeed * 0.5f;
				Color shadowColor = shadowRenderer.material.color;
				shadowColor.a = Mathf.Min(Mathf.Max(1.0f - jumpHeight / halfJumpH , 0.2f), 0.75f);
				shadowRenderer.material.color = shadowColor;
				shadowTransform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.5f, jumpHeight / halfJumpH);
			}break;
		}
	}

	void UpdateInput()
	{
		if(state == State.Dead)
		{
			return;
		}

		if(Input.anyKey)
		{
			SetState(State.Jump);
		}
	}

	public void SetHighScore(int score)
	{
		if (score > playerData.HighScore)
		{
			PlayerPrefs.SetInt(scoreKey, score);
			playerData.HighScore = score;
		}
	}

	public void Update()
	{
		UpdateInput();
		UpdateState();
	}

	public bool IsTouching(Collider2D collider)
	{
		return boxCollider.bounds.Intersects(collider.bounds);
	}

	public void Dead()
	{
		SetState(State.Dead);
	}

	public void MoveTo(Vector3 newPosition)
	{
		transform.position = newPosition;
		shadowTransform.position = newPosition + playerData._ShadowOffset;
	}
}
