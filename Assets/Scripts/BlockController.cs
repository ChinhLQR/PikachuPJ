using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using EventManager;
using System;

public class BlockController : MonoBehaviour
{
	public int value;
	public int x;
	public int y;

	public float offset;

	[SerializeField]
	GameObject selectedObject;


	// Use this for initialization
	void OnEnable()
	{
		isActivated = false;
		offset = 0.07f;
	}

	public void Init(int x, int y, int value)
	{
		this.x = x;
		this.y = y;
		this.value = value;

		gameObject.name = x + "" + y;
	}

	private bool _isActived;

	public bool isActivated
	{
		get { return _isActived; }
		set
		{
			_isActived = value;
			ActiveBlock(value);
		}
	}


	void ActiveBlock(bool isActivated)
	{
		selectedObject.SetActive(isActivated);
	}






	public void DestroyBlock()
	{
		//ContentMgr.Instance.Despaw(gameObject);
				Destroy (gameObject);
	}

	public void OnMouseDown()
	{

		Debug.Log("tap");

		//if (GameController.Instance.isMerging || GameController.Instance.currentState == GameController.GameState.Waiting) return;
		//this.PostEvent(EventID.BlockTap, this);
		//GameController.Instance.CheckTap(this);
		Debug.Log("tap success");

	}

}

