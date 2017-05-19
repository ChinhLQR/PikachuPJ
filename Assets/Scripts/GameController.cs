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
	private float offsetY;
	[SerializeField]
	private float offsetX;
	[SerializeField]
	private BlockController[] blocks;

	public void Start()
	{
		numBlockActivated = 0;
		InstanceBlocks();

		score = 0;
	}

	void InstanceBlocks()
	{	
		board = new BlockController[gridSizeX, gridSizeY];
		for (int x = 0; x < gridSizeX; x++) {
			for (int y = 0; y < gridSizeY; y++) {
				
				var pos = ConvertBoardToPosition (x, y);
				int value;
				if (x == 0 || y == 0 || x == gridSizeX - 1 || y == gridSizeY - 1) {
					value = 0;
				} else {
					value = (int)Random.Range (1f, 12f);
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

	public void CheckTap (BlockController blockTap)
	{
		if (!blockTap.isActivated) {
			blocksActivated [numBlockActivated] = blockTap;
			numBlockActivated++;
			if (numBlockActivated == 1)
				blockTap.isActivated = true;
			if (numBlockActivated == 2) { 

				// Check Path neu co thi xoa khong thi de-active

				if ((blocksActivated [0].value == blocksActivated [1].value)
					&& (BFSFindPath (blocksActivated [0], blocksActivated [1]))) {
					ChangeToZeroBlock (blocksActivated [0]);
					ChangeToZeroBlock (blocksActivated [1]);
					Debug.Log (CheckGameOver ());
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
	//Tim duong di 


	bool BFSFindPath(BlockController block, BlockController targetBlock)
	{
		Debug.Log ("=====");
		Stack<BlockController> list = new Stack<BlockController>();
		BlockController currentBlock;
		BlockController lastBlock;
		BlockController[,] parentBlock = new BlockController[gridSizeX, gridSizeY];
		bool isColing = false;

		int[,] visit = new int[gridSizeX, gridSizeY];
		//Khoi tao mang visit dnah dau da tham va hoan thanh tham
		for (int i = 0; i < gridSizeX; i++)
		{
			for (int j = 0; j < gridSizeY; j++)
			{
				visit[i, j] = 0; // = 0 la chua tham
			}
		}
		int count2 = 0;
		list.Push(block);
		while (list.Count > 0)
		{
//			Debug.Log ("A");
			currentBlock = list.Pop();

			var parent1 = parentBlock[currentBlock.x,currentBlock.y];
			if (parent1 != null) {
				var parent2 = parentBlock [parent1.x, parent1.y];
				if (parent2 != null) {
					if ((currentBlock.x != parent1.x) && (parent2.x == parent1.x))
						count2++;
					if ((currentBlock.y != parent1.y) && (parent2.y == parent1.y))
						count2++;
				}
				if (count2 > 2) {
					count2 = 0;
					continue;
				}
			}
			visit[currentBlock.x, currentBlock.y] = 1; // = 1 la da tham
			// Check 4 huong xem co target khong
			//Debug.Log(currentBlock.x + "--" + currentBlock.y);
			if (CompareBlock (currentBlock, targetBlock)) {
				break;
			}
			var neighbor = GetNeighbor (currentBlock);
			foreach (BlockController neighborBlock in neighbor) {
				if ((visit [neighborBlock.x, neighborBlock.y] == 1) || (list.Contains (neighborBlock))) {
					continue;
				}
				if ((CompareBlock (neighborBlock, targetBlock) || (neighborBlock.value == 0))) {
					list.Push (neighborBlock);
					parentBlock [neighborBlock.x, neighborBlock.y] = currentBlock;
				}
			}

		}


		if (parentBlock [targetBlock.x, targetBlock.y] == null)
			return false;
		return true;

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
								&& (BFSFindPath (board [x, y], board [i, j]))) {
								return true;
							}
						}
				}
			}

		return false;
	}
}
