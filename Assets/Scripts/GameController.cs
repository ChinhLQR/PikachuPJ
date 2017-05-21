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

	private int score;

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
	bool founded = false;
	public void Start()
	{
		numBlockActivated = 0;
		InstanceBlocks();
		path = new List<BlockController> ();
		score = 0;
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
					value = (int)Random.Range (1f, 20f);
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

	private BlockController[] blocksActivated = new BlockController[2];

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
				if ((blocksActivated [0].value == blocksActivated [1].value)
					&& (BFSFind(blocksActivated [0], blocksActivated [1]))) {
					ChangeToZeroBlock (blocksActivated [0]);
					ChangeToZeroBlock (blocksActivated [1]);
//					Debug.Log (CheckGameOver ());
					gameManager.GetComponent<GameManager> ().updateScore ();
					if (CheckWin ()) 
					{
						gameManager.GetComponent<GameManager> ().finishLevelScoreUpdate ();
						gameManager.GetComponent<GameManager> ().setTime (121);
						gameManager.GetComponent<GameManager> ().levelUp ();
					} 
					Debug.Log (CheckGameOver());
				} else {
					blocksActivated [0].isActivated = false;
					blocksActivated [1].isActivated = false;
				}

				numBlockActivated = 0;
			}
		}
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
	private BlockController[,] parentBlock;
	private List<BlockController> path;
	//Kiem tra ngoat
	bool Check2Turn(List<BlockController> path) {
		int count = 0;
		int i = 0;
		BlockController current;
		while (i < path.Count) {
			current = path[i];
			if (i-1 >= 0) {
				if (i-2 >= 0) {
					if ((current.x != path[i-1].x) && (path[i-2].x == path[i-1].x))
						count++;
					if ((current.y != path[i-1].y) && (path[i-2].y == path[i-1].y))
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
		Debug.Log ("=====");
		int i = 0;
		while (i < path.Count) {
			Debug.Log (path [i].x + "-" + path [i].y);
			i++;
		}
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

			if (neighborBlock.value == 0) {
				return false;
			}
		}
		return true;
	}
	bool BFSFind(BlockController block, BlockController targetBlock)
	{
//		Debug.Log ("=====");

		if (CheckTarget (block, targetBlock)) {
			if (path.Count == 0)
				DrawLine (block.transform.position, targetBlock.transform.position, Color.red);
			return true;
		}
		if (CheckZero (targetBlock))
			return false;
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
						DebugPath (path);
						{
							founded = true;
							for (int i = 0; i < path.Count - 1; i++) {
								DrawLine (path [i].transform.position, path [i + 1].transform.position, Color.red);
							}
						}
					}
					path.Remove (targetBlock);
					path.Remove (neighborBlock);

				}
				BFSFind (neighborBlock, targetBlock);
			}
		}

		path.RemoveAt (path.Count - 1);
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
								return true;
							}
						}
				}
			}

		return false;
	}

	void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0.0f)
	{
		GameObject myLine = new GameObject();
		myLine.transform.position = start;
		myLine.AddComponent<LineRenderer>();
		LineRenderer lr = myLine.GetComponent<LineRenderer>();
		lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
		lr.SetColors(color, color);
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
