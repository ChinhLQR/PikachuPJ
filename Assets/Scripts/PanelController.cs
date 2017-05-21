using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelController : MonoBehaviour {

	[SerializeField]
	GameObject LosePanel;
	[SerializeField]
	GameObject WinPanel;

	[SerializeField]
	private GameManager gameManager;

	public Text scoreText;

	void Start()
	{
		gameManager = gameManager.GetComponent<GameManager> ();
	}


	public void ShowLosePanel()
	{
		LosePanel.gameObject.SetActive (true);
	}

	public void ShowWinPanel()
	{
		WinPanel.gameObject.SetActive (true);
		scoreText.text = "Score: " + gameManager.getScore();
	}
	public void Restart()
	{
		GameController.Instance.ClearBoard ();
		GameController.Instance.InstanceBlocks ();
		gameManager.setTime(120f);
		gameManager.setLevel(1);
		gameManager.setScore(0);
		LosePanel.gameObject.SetActive (false);
		WinPanel.gameObject.SetActive (false);
	}
}
