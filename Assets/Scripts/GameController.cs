using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {
	// Khoi tao Singleton
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

	// Khoi tao size cua grid gom vien cac gia tri 0 - de lay duong di, ben trong la gia tri khac 0
	private int gridSizeX = 12;
	private int gridSizeY = 8;

	// Cac bien co the keo Obj vao tu Editor
	[SerializeField]	
	private PanelController panelController;
	[SerializeField]	// Vi tri ma tran tren man hinh
	private Transform startPos;
	[SerializeField]	
	private GameManager gameManager;
	[SerializeField]	// Khoang cach giua 2 block tren man hinh
	private float offsetY;
	[SerializeField]
	private float offsetX;
	[SerializeField]	// Mang cac block: blocks[i] value i
	private BlockController[] blocks;

	// Cac bien can thiet cho thuat toan
	private BlockController[,] board;	// Ma tran chua cac Obj Block
	private int numBlockActivated;	
	private GameManager gameMng;	// Bien de lay Component GameManager
	private BlockController[] blocksActivated = new BlockController[2]; // Mang chua cac block da Active de kiem tra
	private List<BlockController> path;	// Luu duong di
	private List<BlockController>[] listPath;	// Luu cac duong di thoa man de tim duong ngan nhat
	private int numListPath;	// So cac duong di thoa man
	private bool isCheckingOver;	// Dang kiem tra Over

	// Ham duoc tu dong chay khi bat dau game, gom cac khoi tao...
	public void Start() {
		numBlockActivated = 0;	
		gameMng = gameManager.GetComponent<GameManager> ();
		InstanceBlocks ();	// Tao ra cac block tren man hinh
		path = new List<BlockController> ();
		listPath = new List<BlockController>[50];
	}

	public void InstanceBlocks()
	{	
		board = new BlockController[gridSizeX, gridSizeY];
		List<int> saveHalfMaxtrix = new List<int> (); // Luu 1 nua gia tri da tao roi tao ra nua con lai, de dam bao moi block deu co cap
		for (int x = 0; x < gridSizeX; x++) {
			for (int y = 0; y < gridSizeY; y++) {
				var pos = ConvertBoardToPosition (x, y);
				int value;
				// Vien la cac block co gia tri 0
				if (x == 0 || y == 0 || x == gridSizeX - 1 || y == gridSizeY - 1) {
					value = 0;
				} else if (x <= 5) {
					if (gameMng.getLevel () == 1) {
						value = (int)Random.Range (1f, 7f);
					} else if (gameMng.getLevel () == 2) {
						value = (int)Random.Range (1f, 13f);
					} else
						value = (int)Random.Range (1f, 25f);
					saveHalfMaxtrix.Add (value);
				} else {
					int random = 0;
					random = Random.Range (0, saveHalfMaxtrix.Count - 1);
					value = saveHalfMaxtrix [random];
					saveHalfMaxtrix.RemoveAt (random);
				}
				// Tao cac khoi tren man hinh o vi tri pos
				var block = Instantiate (blocks [value], pos, blocks [value].transform.rotation);
				block.Init (x, y);
				board [x, y] = block; // Dua vao ma tran board dung cho thuat toan
				// An cac khoi block 0 tren man hinh
				if (x == 0 || y == 0 || x == gridSizeX - 1 || y == gridSizeY - 1) {
					block.gameObject.SetActive (false);
				}
			}
		}
	}

	// Ham lay vi tri cac block de dua len man hinh
	Vector3 ConvertBoardToPosition(int x, int y) {
		Vector3 offset = new Vector3(offsetX * x, offsetY * y);
		return startPos.position + offset;
	}

	// Kiem tra click vao block (duoc goi khi 1 khoi duoc click)
	public void CheckClick (BlockController blockTap) {
		// Chi xet khi click vao block chua duoc active
		if (!blockTap.isActivated) {
			blocksActivated [numBlockActivated] = blockTap;
			numBlockActivated++;
			if (numBlockActivated == 1)
				blockTap.isActivated = true;
			if (numBlockActivated == 2) { 
				blockTap.isActivated = true;
				// Neu dem duoc 2 block da active thi kiem tra
				isCheckingOver = false;
				numListPath = 0;
				// Kiem tra neu 2 gia tri bang nhau, va co duong di thoa man thi xoa di thay bang block 0 (duong di) nguoc lai thi de-active 2 block di
				if ((blocksActivated [0].value == blocksActivated [1].value)
				    && (BFSFind (blocksActivated [0], blocksActivated [1]))) {
					ChangeToZeroBlock (blocksActivated [0]);
					ChangeToZeroBlock (blocksActivated [1]);
					// Neu co duong di thi ve
					if (numListPath != 0) {
						var minPath = FindMinPath (listPath);
						DrawPath (minPath);
					}
					// Tang diem kiem tra thang thua
					gameMng.updateScore ();
					if (CheckWin ()) {
						gameMng.finishLevelScoreUpdate ();
						gameMng.setTime (121);
						gameMng.levelUp ();
					} else {
						isCheckingOver = true;
						numListPath = 0;
						if (CheckGameOver ())
							panelController.GetComponent<PanelController> ().ShowLosePanel ();
					}
				} else {
					blocksActivated [0].isActivated = false;
					blocksActivated [1].isActivated = false;
				}
				numBlockActivated = 0; // Khoi tao lai
			}
		}
	}

	// Ve duong giua 2 diem
	void DrawLine(Vector3 start, Vector3 end)
	{
		float duration = 0.2f;
		Color lineColor = Color.red;
		GameObject myLine = new GameObject ();
		myLine.transform.position = start;
		myLine.AddComponent<LineRenderer> ();
		LineRenderer lr = myLine.GetComponent<LineRenderer> ();
		lr.material = new Material (Shader.Find ("Particles/Alpha Blended Premultiply"));
		lr.startColor = lineColor; lr.endColor = lineColor;
		lr.startWidth = 0.1f; lr.endWidth = 0.1f;
		lr.SetPosition (0, start);
		lr.SetPosition (1, end);
		GameObject.Destroy (myLine, duration);
	}

	// Ve duong di
	void DrawPath(List<BlockController> path) {
		for (int i = 0; i < path.Count - 1; i++) {
			DrawLine (path [i].transform.position, path [i + 1].transform.position);
		}
	}

	// Tim duong di ngan nhat
	List<BlockController> FindMinPath(List<BlockController>[] listPath) {
		List<BlockController> minPath = new List<BlockController> ();
		minPath = listPath [0];
		if (numListPath >= 1) {
			for (int i = 0; i < numListPath; i++) {
				if (listPath [i].Count < minPath.Count) {
					minPath = listPath [i];
				}
			}
		}
		return minPath;
	}

	// Thay bang block 0
	void ChangeToZeroBlock(BlockController block) {
		var pos = ConvertBoardToPosition (block.x, block.y);
		var blockNew = Instantiate (blocks [0], pos, blocks [0].transform.rotation);
		blockNew.Init (block.x, block.y);
		board [block.x, block.y] = blockNew;
		blockNew.gameObject.SetActive (false);
		block.DestroyBlock ();
	}

	bool CheckWin()
	{
		foreach (BlockController block in board) {
			if (block.value != 0) {
				return false;
			}	
		}
		return true;
	}

	bool CheckGameOver() {
		for (int x = 1; x < gridSizeX - 1; x++)
			for (int y = 1; y < gridSizeY - 1; y++) {
				if (board [x, y].value != 0) {
					for (int i = 1; i < gridSizeX - 1; i++)
						for (int j = 1; j < gridSizeY - 1; j++) {
							if ((board [x, y].value == board [i, j].value) && (!CompareBlock (board [i, j], board [x, y]))
							    && BFSFind (board [x, y], board [i, j]))
								return false;
						}
				}
			}
		return true;
	}

	// Ham tim tat ca cac duong di thoa man va tra ve bool co duong di khong
	bool BFSFind(BlockController block, BlockController targetBlock) {
		// Kiem tra ngay ben canh thi tra ve dung
		if (CheckTarget (block, targetBlock)) {
			if (path.Count == 0) {
				// Khong ve duong khi check over
				if (!isCheckingOver) {
					DrawLine (block.transform.position, targetBlock.transform.position);
				}
			}
			return true;
		}
		// Kiem tra target xung quanh co duong di khong
		if (CheckZero (targetBlock))
			return false;
		//Neu la dang check over thi chi can co duong di thoa man tra ve true luon
		if (isCheckingOver && numListPath != 0) {
			return true;
		}
		// Neu path khong thoa man ngay lap tuc tra ve false de tranh tim het duong khong thoa man
		if (!Check2Turn (path))
			return false;
		
		path.Add (block);
		// Lay duong di xung quanh chua co trong path
		var neighbor = GetNeighbor (block);
		foreach (BlockController neighborBlock in neighbor) {
			if ((neighborBlock.value == 0) && !path.Contains (neighborBlock)) {
				// Neu xung quanh co target thi Add block va target vao path roi xet xem duong di thoa man khong sau do xoa di va check tiep
				if (CheckTarget (neighborBlock, targetBlock)) {
					path.Add (neighborBlock);
					path.Add (targetBlock);
					if (Check2Turn (path)) {
						{
							listPath [numListPath] = new List<BlockController> (path);
							numListPath++;
						}
					}
					path.Remove (targetBlock);
					path.Remove (neighborBlock);
				}
				BFSFind (neighborBlock, targetBlock);
			}
		}
		path.RemoveAt (path.Count - 1);

		if (numListPath != 0) {
			return true;
		}
		return false;
	}

	// Kiem tra ra khoi board
	bool CheckOutOfRange(int posX, int posY) {
		if (posX < 0 || posY < 0 || gridSizeX <= posX || gridSizeY <= posY)
			return true;
		return false;
	}

	// So sanh vi tri 2 block
	bool CompareBlock(BlockController block1, BlockController block2) {
		if (block1.x == block2.x && block1.y == block2.y)
			return true;
		return false;
	}

	// Tra ve list cac block 4 huong
	List<BlockController> GetNeighbor(BlockController block) {
		List<BlockController> neighbor = new List<BlockController> ();
		if (!CheckOutOfRange (block.x + 1, block.y))
			neighbor.Add (board [block.x + 1, block.y]);
		if (!CheckOutOfRange (block.x, block.y + 1))
			neighbor.Add (board [block.x, block.y + 1]);
		if (!CheckOutOfRange (block.x - 1, block.y))
			neighbor.Add (board [block.x - 1, block.y]);
		if (!CheckOutOfRange (block.x, block.y - 1))
			neighbor.Add (board [block.x, block.y - 1]);
		return neighbor;
	}

	// Kiem tra ngoat lon hon 2 lan la sai
	bool Check2Turn(List<BlockController> path) {
		int count = 0;
		int i = 0;
		BlockController current;
		while (i < path.Count) {
			current = path [i];
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

	// Kiem tra ngay canh target
	bool CheckTarget(BlockController block, BlockController targetBlock) {
		var neighbor = GetNeighbor (block);
		foreach (BlockController neighborBlock in neighbor) {
			if (CompareBlock (neighborBlock, targetBlock)) {
				return true;
			}
		}
		return false;
	}

	// Kiem tra xung quanh co duong di khong
	bool CheckZero(BlockController targetBlock) {
		var neighbor = GetNeighbor (targetBlock);
		foreach (BlockController neighborBlock in neighbor) {
			if (CompareBlock (neighborBlock, targetBlock)) {
				return true;
			}
		}
		return false;
	}

	public void ClearBoard() {
		foreach (var item in board) {
			item.DestroyBlock ();
		}
	}
}
