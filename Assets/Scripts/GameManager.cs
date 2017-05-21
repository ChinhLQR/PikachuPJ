using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	[SerializeField]
	Text timeText;

	[SerializeField]
	Text scoreText;

	[SerializeField]
	Text levelText;

	[SerializeField]
	private PanelController panelController;


	private float timeLeft = 120f;
	private int score = 0;
	private int level = 1;
	void Start()
	{

		timeText.text = "Time: " + timeLeft;
		scoreText.text = "Score: " + score;
		levelText.text = "Level: " + level;
	}


	void Update () 
	{
		if(timeLeft>=0)
		updateTime ();

	}

	public void setTime(float value)
	{
		timeLeft = value;
		timeText.text = "Time: " + timeLeft;
	}

	public void setLevel(int value)
	{
		level = value;
		levelText.text = "Level: " + level;
	}
	public void setScore(int value)
	{
		score = value;
		scoreText.text = "Score: " + score;

	}
	public int getScore()
	{
		return score;
	}


	public void levelUp()
	{
		if (level < 3) {
			level++;
			levelText.text = "Level: " + level;
			GameController.Instance.ClearBoard ();
			GameController.Instance.InstanceBlocks ();
		} else 
		{
			Debug.Log ("U win son");

			Debug.Log ("Total Score: " + score);
		}
		
	}
	public void updateScore()
	{
		score += 100;
		scoreText.text = "Score: " + score;
	}

	public void finishLevelScoreUpdate()
	{
		score += (int)timeLeft * 10 + 500;
		scoreText.text = "Score: " + score;
	}

	public void updateTime()
	{
		
		timeLeft -= Time.deltaTime;
		if (timeLeft <= 0) 
		{
			panelController.GetComponent<PanelController> ().ShowLosePanel ();
			Debug.Log ("YouLose");
		}
		if(timeLeft>=0)
			timeText.text = "Time: " + (int)timeLeft;
	}
		

}
