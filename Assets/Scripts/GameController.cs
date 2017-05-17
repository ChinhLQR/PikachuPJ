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
	//Tim duong di 
	bool BFSFindPath(BlockController block, BlockController targetBlock)
	{
		Queue<BlockController> queue = new Queue<BlockController>();
		BlockController currentBlock;
		BlockController[,] parentBlock = new BlockController[gridSizeX, gridSizeY];

		int[,] visit = new int[gridSizeX, gridSizeY];
		//Khoi tao mang visit dnah dau da tham va hoan thanh tham
		for (int i = 0; i < gridSizeX; i++)
		{
			for (int j = 0; j < gridSizeY; j++)
			{
				visit[i, j] = 0; // = 0 la chua tham
			}
		}

		queue.Enqueue(block);
		while (queue.Count > 0)
		{
			currentBlock = queue.Dequeue();
			//Debug.Log ("?");
			visit[currentBlock.x, currentBlock.y] = 1; // = 1 la da tham
			// Check 4 huong xem co target khong
			if (CompareBlock (currentBlock, targetBlock)) {
				break;
			}

			//Kiem tra tim 4 huong neu khong ra khoi grid && (hoac la target hoac la gia tri grid == 0) && chua duoc visit. thi Enqueue va gan parent
			if (!CheckOutOfRange (currentBlock.x + 1, currentBlock.y) 
				&& (CompareBlock (board [currentBlock.x + 1, currentBlock.y], targetBlock) ||(board [currentBlock.x + 1, currentBlock.y].value == 0))
				&& (visit [currentBlock.x + 1, currentBlock.y] == 0)) 
			{
				queue.Enqueue (board [currentBlock.x + 1, currentBlock.y]);
				parentBlock [currentBlock.x + 1, currentBlock.y] = currentBlock;
			}
			if (!CheckOutOfRange (currentBlock.x, currentBlock.y + 1)
				&& (CompareBlock (board [currentBlock.x, currentBlock.y + 1], targetBlock) || (board [currentBlock.x, currentBlock.y + 1].value == 0))
				&& (visit [currentBlock.x, currentBlock.y + 1] == 0)) {
				queue.Enqueue (board [currentBlock.x, currentBlock.y + 1]);
				parentBlock [currentBlock.x, currentBlock.y + 1] = currentBlock;
			}
			if (!CheckOutOfRange (currentBlock.x - 1, currentBlock.y)
				&& (CompareBlock (board [currentBlock.x - 1, currentBlock.y], targetBlock) || (board [currentBlock.x - 1, currentBlock.y].value == 0))
				&& (visit [currentBlock.x - 1, currentBlock.y] == 0)) {
				queue.Enqueue (board [currentBlock.x - 1, currentBlock.y]);
				parentBlock [currentBlock.x - 1, currentBlock.y] = currentBlock;
			}
			if (!CheckOutOfRange (currentBlock.x, currentBlock.y - 1)
				&& (CompareBlock (board [currentBlock.x, currentBlock.y - 1], targetBlock) || (board [currentBlock.x, currentBlock.y - 1].value == 0))
				&& (visit [currentBlock.x, currentBlock.y - 1] == 0)) {
				queue.Enqueue (board [currentBlock.x, currentBlock.y - 1]);
				parentBlock [currentBlock.x, currentBlock.y - 1] = currentBlock;
			}
		}

		currentBlock = targetBlock;
		BlockController parent = parentBlock[currentBlock.x, currentBlock.y];
		bool isColing = false;
		//, isRowing = false;
		if (parent == null)
			return false;
		if (currentBlock.x == parent.x) {
			isColing = true;
			//isRowing = false;

		}
		currentBlock = parent;
		int count = 0;
		while (!CompareBlock(currentBlock,block))
		{
			parent = parentBlock[currentBlock.x, currentBlock.y];
			if (isColing && (currentBlock.x != parent.x)) {
				count++;
				isColing = false;
			} 
			if (!isColing && (currentBlock.y != parent.y)) {
				count++;
				isColing = true;
			}

			currentBlock = parent;
		}
		if (count <= 2)
			return true;
		return false;
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
