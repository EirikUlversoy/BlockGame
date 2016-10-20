using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ManagerScript : MonoBehaviour {

  public int towerWidth = 6;
  public int towerHeight = 13;

  public GameObject[,] blockGrid;
  public int[] currentHeights;

  private GameObject wall_left;
  private GameObject wall_right;
  private GameObject ground;
  private GameObject background;

  public GameObject destroyBlockPrefab;

  private BlockPairScript blockPair;

  public int score = 0;
  public int speed = 1;
  public GameObject scoreTextObject;
  public Text scoreText;

  public GameObject alertTextObject;
  public Text alertText;
  private float alertFlashTime = 0;

  private int numBlocksDropped = 0;

	// Use this for initialization
	void Awake () {
    blockGrid = new GameObject[towerWidth,towerHeight];
    currentHeights = new int[6]{0,0,0,0,0,0};

    initializeBlockWell();

    initializeBlockPair();

    initializeDestroyBlock();

    initializeScoreText();

    initializeAlertText();

    // Initialize level with some blocks
    AddBlockToColumn(blockPair.SpawnBlock("earth", 1).gameObject);
    AddBlockToColumn(blockPair.SpawnBlock("air", 2).gameObject);
    AddBlockToColumn(blockPair.SpawnBlock("water", 3).gameObject);
    AddBlockToColumn(blockPair.SpawnBlock("fire", 4).gameObject);
	}


  private void initializeBlockWell () {
    wall_left = GameObject.CreatePrimitive(PrimitiveType.Cube);
    wall_left.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/UnlitMaterial");

    wall_left.transform.position = new Vector3(-1, 7, 0);
    wall_right = (GameObject) Instantiate(wall_left, new Vector3(6, 7, 0), Quaternion.identity);
    ground = (GameObject) Instantiate(wall_left, new Vector3(2.5f, 0, 0), Quaternion.identity);
    background = (GameObject) Instantiate(wall_left, new Vector3(2.5f, 7, 1), Quaternion.identity);

    wall_left.transform.localScale = new Vector3(1,13,1);
    wall_right.transform.localScale = new Vector3(1,13,1);
    ground.transform.localScale = new Vector3(8,1,1);
    background.transform.localScale = new Vector3(20,15,1);

    wall_left.name = "Wall_Left";
    wall_right.name = "Wall_Right";
    ground.name = "Ground";
    background.name = "Background";

    wall_left.GetComponent<MeshRenderer>().material.color = Color.black;
    wall_right.GetComponent<MeshRenderer>().material.color = Color.black;
    ground.GetComponent<MeshRenderer>().material.color = Color.black;
    background.GetComponent<MeshRenderer>().material.color = Color.grey;
  }

  private void initializeDestroyBlock() {
    destroyBlockPrefab = new GameObject();
    destroyBlockPrefab.AddComponent<DestroyBlockScript>();
  }

  private void initializeBlockPair() {
    blockPair = new GameObject().AddComponent<BlockPairScript>();
    blockPair.towerHeight = towerHeight;
    blockPair.towerWidth = towerWidth;
    blockPair.manager = this;
    blockPair.InitializePreviewBlocks();
    blockPair.InitializeBlockPair();
  }

  void initializeScoreText () {
    GameObject newCanvas = new GameObject("Canvas");
    Canvas c = newCanvas.AddComponent<Canvas>();
    c.renderMode = RenderMode.ScreenSpaceOverlay;
    newCanvas.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

    scoreTextObject = new GameObject("scoreText");
    scoreTextObject.AddComponent<CanvasRenderer>();
    scoreText = scoreTextObject.AddComponent<Text>();
    scoreText.rectTransform.anchoredPosition = new Vector2(-80,200);
    scoreText.rectTransform.sizeDelta = new Vector2(600,140);
    updateText();
    scoreText.transform.SetParent(newCanvas.transform, false);
    scoreText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
    scoreText.fontSize = 36;
    scoreText.color = Color.white;
    scoreText.fontStyle = FontStyle.Bold;
    scoreText.alignment = TextAnchor.UpperLeft;
    scoreTextObject.AddComponent<Shadow>().effectColor = Color.black;
  }

  void initializeAlertText () {
    GameObject newCanvas = new GameObject("Canvas");
    Canvas c = newCanvas.AddComponent<Canvas>();
    c.renderMode = RenderMode.ScreenSpaceOverlay;
    newCanvas.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

    alertTextObject = new GameObject("alertText");
    alertTextObject.AddComponent<CanvasRenderer>();
    alertText = alertTextObject.AddComponent<Text>();
    alertText.rectTransform.anchoredPosition = new Vector2(280,0);
    alertText.rectTransform.sizeDelta = new Vector2(200,140);
    alertText.text = "ALERT";
    alertText.transform.SetParent(newCanvas.transform, false);
    alertText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
    alertText.fontSize = 36;
    alertText.color = new Color(1,1,1,0);
    alertText.fontStyle = FontStyle.Bold;
    alertText.alignment = TextAnchor.UpperLeft;
    alertTextObject.AddComponent<Shadow>().effectColor = Color.black;
  }

  public void flashAlert(string textToFlash) {
    alertText.text = textToFlash;
    alertFlashTime = 1f;
  }

  public void addPoints(int amount) {
    flashAlert("+" + amount + " points");
    score += amount;
    updateText();
  }
  public void increaseSpeed () {
    flashAlert("Speed Up!");
    speed++;
    updateText();
  }

  public void updateText() {
    scoreText.text = "Score: " + score + "\nSpeed: " + speed;
  }

  private void createDestroyBlock(int i, int j, string type, int scoreMultiplier) {
    GameObject destroyBlockObject = (GameObject) Instantiate(destroyBlockPrefab, new Vector3( (2f * i + 1)/2, (2f * j + 1)/2 + 1, 0 ), Quaternion.identity);
    DestroyBlockScript destroyBlock = destroyBlockObject.GetComponent<DestroyBlockScript>();
    destroyBlock.DoDestroy(i, j, type, this, scoreMultiplier);

  }

  public void BlockDropped() {
    numBlocksDropped++;
    if (numBlocksDropped % 10 == 0) increaseSpeed();
  }

  public void CheckForDestroyBlocks(int scoreMultiplier) {
    for (int i = 0 ; i < towerWidth - 1 ; i++) {
      for (int j = 0 ; j < towerHeight - 1 ; j++) {
        GameObject blockObject = blockGrid[i,j];
        if (blockObject == null) {
          j = 100;
          continue;
        }
        BlockScript blockScript = blockObject.GetComponent<BlockScript>();
        string type = blockScript.type;
        if (blockGrid[i+1,j] != null && blockGrid[i+1,j].GetComponent<BlockScript>().type == type &&
            blockGrid[i,j+1] != null && blockGrid[i,j+1].GetComponent<BlockScript>().type == type &&
            blockGrid[i+1,j+1] != null && blockGrid[i+1,j+1].GetComponent<BlockScript>().type == type) {
          createDestroyBlock(i, j, type, scoreMultiplier);
          return;
        }
      }
    }
    blockPair.InitializeBlockPair();
  }


  public void AddBlockToColumn(GameObject blockObject) {
    BlockScript block = blockObject.GetComponent<BlockScript>();
    block.isFalling = false;
    int column = block.column;
    currentHeights[column]++;
    if (currentHeights[column] > towerHeight) {
      doGameOver();
    } else {
      blockObject.transform.position = new Vector3(column, currentHeights[column], 0);
      if (blockGrid[column, currentHeights[column] - 1] != null) Debug.Log("WARNING: GRID SPOT OCCUPIED: " + column + ", " + currentHeights[column]);
      blockGrid[column, currentHeights[column] - 1] = blockObject;
    }
  }

  private void doGameOver() {
    blockPair.isActive = false;
    blockPair.gameOver = true;
    alertText.text = "GAME OVER";
    alertFlashTime = 100000f;
  }

  void Update() {
    if (alertFlashTime > 0) {
      alertText.color = new Color(1,1,1,alertFlashTime);
      alertFlashTime -= Time.deltaTime;
    }

  }

}
