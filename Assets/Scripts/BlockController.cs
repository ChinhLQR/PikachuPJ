using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BlockController : MonoBehaviour
{
	private int _x;
	public int x {
		get {
			return _x;
		}
		set {
			_x = value;
		}
	}

	private int _y;
	public int y {
		get {
			return _y;
		}
		set {
			_y = value;
		}
	}

	[SerializeField]
	private int _value;
	public int value {
		get {
			return _value;
		}
	}

	private bool _isActivated;
	public bool isActivated {
		get {
			return _isActivated;
		}
		set {
			_isActivated = value;
			ActiveBlock (value);
		}
	}

	[SerializeField]
	private GameObject selectedObject;

	void OnEnable()
	{
		isActivated = false;
	}

	public void Init(int x, int y)
	{
		this.x = x;
		this.y = y;
		//this.value = value;

		gameObject.name = x + "-" + y;
	}

	void ActiveBlock(bool isActivated)
	{
		if (selectedObject != null)
			selectedObject.SetActive (isActivated);
	}

	public void DestroyBlock()
	{
		Destroy (gameObject);
	}

	public void OnMouseDown()
	{
		GameController.Instance.CheckTap(this);
	}

}

