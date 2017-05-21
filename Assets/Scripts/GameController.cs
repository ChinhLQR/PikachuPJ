using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
	// Singleton instance
	static public GameController Instance { get; private set; }

	// This method is called when the script is loaded
	void Awake () {
		// Check for other instance
		if (Instance == null) {
			// No? Let me be one
			Instance = this;
		} else {
			// Already has one? I better destroy myself
			Destroy (this);
		}
	}

	// This method is called when the script is destroy
	void OnDestroy () {
		// I am the singleton ? If yes then I should release the singleton as well
		if ( Instance == this ) {
			Instance = null;
		}
	}

	private int gridSizeX = 12;
	private int gridSizeY = 8;

	private BlockController[,] board;
	private int numBlockActivated;

	[SerializeField]
	private Transform startPos;
	[SerializeField]
	private GameManager gameManager;
	[SerializeField]
	private float offsetY;
	[SerializeField]
	private float offsetX;
	[SerializeField]
	private BlockController[] blocks;

	private Color lineColor;
	private bool founded = false;
	private bool isDrawed = false;
	private GameManager gameMng;
	private BlockController[] blocksActivated = new BlockController[2];
	private List<BlockController> path;
	private List<BlockController>[] listPath;
	private int listPathLength;
	private bool isCheckingOver;
	public void Start()
	{
		numBlockActivated = 0;
		gameMng = gameManager.GetComponent<GameManager> ();
		InstanceBlocks();
		path = new List<BlockController> ();
		listPath = new List<BlockController>[100];
	}
	public void InstanceBlocks()
	{	

		List<int> save2 = new List<int> ();

		Random rnd = new Random ();
		board = new BlockController[gridSizeX, gridSizeY];
		for (int x = 0; x < gridSizeX; x++) {
			for (int y = 0; y < gridSizeY; y++) {

				var pos = ConvertBoardToPosition (x, y);
				int value;
				if (x == 0 || y == 0 || x == gridSizeX - 1 || y == gridSizeY - 1) {
					value = 0;
				} else if (x <= 5) {
					if (gameMng.getLevel () == 1) {
						value = (int)Random.Range (1f, 7f);
					} else if (gameMng.getLevel () == 2) {
						value = (int)Random.Range (1f, 13f);
					}
					else value = (int)Random.Range (1f, 25f);
					save2.Add (value);
//					Debug.Log (save2.Count);

				} else {
					int ran = 0;
					ran = Random.Range (0, save2.Count - 1);
					value = save2 [ran];
					save2.RemoveAt (ran);
				}
				var block = Instantiate (blocks [value], pos, blocks [value].transform.rotation);
				block.Init (x, y);
				board [x, y] = block;
				if (x == 0 || y == 0 || x == gridSizeX - 1 || y == gridSizeY - 1) {
					block.gameObject.SetActive (false);
				}

			}
		}

	}

	public Vector3 ConvertBoardToPosition(int x, int y)
	{
		Vector3 offset = new Vector3(offsetX * x, offsetY * y);
		return startPos.position + offset;
	}


	public void CheckClick (BlockController blockTap)
	{
		if (!blockTap.isActivated) {
			blocksActivated [numBlockActivated] = blockTap;
			numBlockActivated++;
			if (numBlockActivated == 1)
				blockTap.isActivated = true;
			if (numBlockActivated == 2) { 

				// Check Path neu co thi xoa khong thi de-active
				founded = false;
				isDrawed = false;
				isCheckingOver = false;
				lineColor = Color.red;

				listPathLength = 0;
				if ((blocksActivated [0].value == blocksActivated [1].value)
					&& (BFSFind(blocksActivated [0], blocksActivated [1]))) {
					ChangeToZeroBlock (blocksActivated [0]);
					ChangeToZeroBlock (blocksActivated [1]);
					if (listPathLength != 0) {
						var minPath = FindMinPath (listPath);
//						DebugPath (minPath);
						for (int i = 0; i < minPath.Count - 1; i++) {
							DrawLine (minPath [i].transform.position, minPath [i + 1].transform.position);
						}
					}
					founded = false;
					gameMng.updateScore ();
					if (CheckWin ()) 
					{
						gameMng.finishLevelScoreUpdate ();
						gameMng.setTime (121);
						gameMng.levelUp ();
					} 
					isCheckingOver = true;
					Debug.Log (CheckGameOver());
					Debug.Log (listPathLength);

				} else {
					blocksActivated [0].isActivated = false;
					blocksActivated [1].isActivated = false;
				}
				numBlockActivated = 0;
			}
		}
	}

	List<BlockController> FindMinPath(List<BlockController>[] listPath) {
		List<BlockController> minPath = new List<BlockController>();
		minPath = listPath [0];
		if (listPathLength >= 1) {
			for (int i = 0; i < listPathLength; i++) {
				if (listPath [i].Count < minPath.Count) {
					minPath = listPath [i];
				}
			}
		}
		return minPath;
	}


	public void ChangeToZeroBlock(BlockController block) {
		var pos = ConvertBoardToPosition (block.x, block.y);

		var blockNew =Instantiate (blocks [0], pos, blocks [0].transform.rotation);
		blockNew.Init (block.x, block.y);
		board [block.x, block.y] = blockNew;
		blockNew.gameObject.SetActive (false);
		block.DestroyBlock ();
	}
	// Kiem tra ra khoi board
	bool CheckOutOfRange(int posX, int posY) {
		if (posX < 0 || posY < 0 || gridSizeX <= posX || gridSizeY <= posY)
			return true;
		return false;
	}
	bool CompareBlock(BlockController block1, BlockController block2) {
		if (block1.x == block2.x && block1.y == block2.y)
			return true;
		return false;
	}

	List<BlockController> GetNeighbor(BlockController block) {
		List<BlockController> neighbor = new List<BlockController>();
		if (!CheckOutOfRange (block.x + 1, block.y))
			neighbor.Add (board [block.x + 1, block.y]);
		if (!CheckOutOfRange (block.x, block.y + 1))
			neighbor.Add (board [block.x, block.y + 1]);
		if (!CheckOutOfRange (block.x - 1, block.y))
			neighbor.Add (board [block.x - 1, block.y]);
		if (!CheckOutOfRange (block.x, block.y-1))
			neighbor.Add (board [block.x, block.y - 1]);
		return neighbor;
	}
	//Kiem tra ngoat
	bool Check2Turn(List<BlockController> path) {
		int count = 0;
		int i = 0;
		BlockController current;
		while (i < path.Count) {
			current = path[i];
			if (i - 1 >= 0) {
				if (i - 2 >= 0) {
					if ((current.x != path [i - 1].x) && (path [i - 2].x == path [i - 1].x))
						count++;
					if ((current.y != path [i - 1].y) && (path [i - 2].y == path [i - 1].y))
						count++;
				}
			}
			i++;
			if (count > 2)
				return false;
		}
		return true;
	}

	void DebugPath(List<BlockController> path){
		Debug.Log ("+++++");
		int i = 0;
		if (path != null)
			while (i < path.Count) {
				Debug.Log (path [i].x + "-" + path [i].y);
				i++;
			}
		Debug.Log ("+++++");
	}

	bool CheckTarget(BlockController block, BlockController targetBlock)
	{
		var neighbor = GetNeighbor (block);
		foreach (BlockController neighborBlock in neighbor) {

			if (CompareBlock (neighborBlock, targetBlock)) {
				return true;
			}
		}
		return false;
	}

	bool CheckZero(BlockController targetBlock)
	{
		var neighbor = GetNeighbor (targetBlock);
		foreach (BlockController neighborBlock in neighbor) {

			if (CompareBlock(neighborBlock,targetBlock)) {
				return true;
			}
		}
		return false;
	}
	bool BFSFind(BlockController block, BlockController targetBlock)
	{

		if (CheckTarget (block, targetBlock)) {
			if (path.Count == 0) {
				if (!isCheckingOver) {
					DrawLine (block.transform.position, targetBlock.transform.position);
				}
			}
			return true;
		}
		if (CheckZero (targetBlock))
			return false;
		if (isCheckingOver)
			if (founded) return founded;
		if (!Check2Turn (path))
			return false;
		path.Add (block);
//		Debug.Log (block.x + "--" + block.y);

		var neighbor = GetNeighbor (block);
		foreach (BlockController neighborBlock in neighbor) {
			if ((neighborBlock.value == 0) && !path.Contains (neighborBlock)) {
				if (CheckTarget (neighborBlock, targetBlock)) {
					path.Add (neighborBlock);
					path.Add (targetBlock);
					if (Check2Turn (path)) {
//						DebugPath (path);
						{
							if (!isCheckingOver) {
								listPath [listPathLength] = new List<BlockController> (path);
								
								listPathLength++;
							} else
								founded = true;

//							if (!isDrawed) {
//								DrawPath (path);
//								isDrawed = true;
//							}
						}
					}
					path.Remove (targetBlock);
					path.Remove (neighborBlock);

				}
				BFSFind (neighborBlock, targetBlock);
			}
		}

		path.RemoveAt (path.Count - 1);
		if (!isCheckingOver && listPathLength != 0)
			founded = true;
		return founded;
	}

	public void ClearBoard()
	{
		foreach (var item in board)
		{
			item.DestroyBlock ();
		}
	}

	bool CheckGameOver()
	{

		for (int x = 1; x < gridSizeX - 1; x++)
			for (int y = 1; y < gridSizeY - 1; y++) {
				if (board [x, y].value != 0) {
					for (int i = 1; i < gridSizeX - 1; i++)
						for (int j = 1; j < gridSizeY - 1; j++) {
							if ((board [x, y].value == board [i, j].value) && (!CompareBlock(board[i,j],board[x,y]))
								&& BFSFind (board [x, y], board [i, j])) {
								return false;
							}
						}
				}
			}

		return true;
	}

	void DrawLine(Vector3 start, Vector3 end, float duration = 0.2f)
	{
		GameObject myLine = new GameObject();
		myLine.transform.position = start;
		myLine.AddComponent<LineRenderer>();
		LineRenderer lr = myLine.GetComponent<LineRenderer>();
		lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
		lr.SetColors(lineColor, lineColor);
		lr.SetWidth(0.1f, 0.1f);
		lr.SetPosition(0, start);
		lr.SetPosition(1, end);
		GameObject.Destroy(myLine, duration);
	}

	private bool CheckWin()
	{

		foreach(BlockController block in board)
		{
			if (block.value != 0) 
			{

				return false;
			}	
		}


		return true;
	}
}
