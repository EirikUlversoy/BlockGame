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
  private GameObject backgroundPanel;

  private BlockPairScript blockPair;

  public int score = 0;
  public int speed = 1;
  public Text scoreText;

  public Text alertText;
  private float alertFlashTime = 0;

  private int numBlocksDropped = 0;

  private bool diamondTouch = false;
  private string diamondTouchColor = "";

  private bool gameOver = true;
  private bool doneWithHighScores = false;
  private bool readyToStart = true;

  private DestroyBlockScript destroyBlockScript;

  private AudioScript audioManager;

  private HighScoreScript highScoreManager;

  // REMOVE THESE
  public int debugDropPoints = 0;
  public int[] debugPointsPerLevel;


	// Use this for initialization
	void Awake () {
    blockGrid = new GameObject[towerWidth,towerHeight];
    currentHeights = new int[6]{0,0,0,0,0,0};
    audioManager = new GameObject().AddComponent<AudioScript>();

    Camera.main.transform.position = new Vector3(2.5f, 7f, -10f);
    Camera.main.orthographic = true;
    Camera.main.orthographicSize = 7.5f;

    highScoreManager = gameObject.AddComponent<HighScoreScript>();

    initializeBlockWell();

    initializeBlockPair();

    initializeDestroyBlock();

    initializeScoreText();

    initializeAlertText();

    flashPermanentAlert("Press ENTER to begin.");

    debugPointsPerLevel = new int[100];

	}

  private void startNewGame() {
    // Restart score and level
    gameOver = false;
    speed = 1;
    score = 0;
    backgroundPanel.SetActive(false);
    highScoreManager.HideHighScoreScreen();
    audioManager.SetGameOver(false);
    updateText();

    // Destroy all blocks
    for (int i = 0 ; i < blockGrid.GetLength(0) ; i++) {
      for (int j = 0 ; j < blockGrid.GetLength(1) ; j++) {
        GameObject blockObject = blockGrid[i,j];
        if (blockObject != null) {
          Destroy(blockObject);
          blockGrid[i,j] = null;
        }
      }
    }
    if (blockPair.leftBlock != null) Destroy(blockPair.leftBlock.gameObject);
    if (blockPair.rightBlock != null) Destroy(blockPair.rightBlock.gameObject);
    currentHeights = new int[6]{0,0,0,0,0,0};

    // Add start blocks to column
    // AddBlockToColumn(blockPair.SpawnBlock("earth", 1).gameObject);
    // AddBlockToColumn(blockPair.SpawnBlock("air", 2).gameObject);
    // AddBlockToColumn(blockPair.SpawnBlock("water", 3).gameObject);
    // AddBlockToColumn(blockPair.SpawnBlock("fire", 4).gameObject);

    // // initialize block pair
    blockPair.isActive = true;
    blockPair.gameOver = false;
    blockPair.InitializePreviewBlocks();
    blockPair.InitializeBlockPair();

    flashPermanentAlert("");

    audioManager.PlayAudio();

  }


  private void initializeBlockWell () {
    wall_left = GameObject.CreatePrimitive(PrimitiveType.Cube);
    wall_left.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/UnlitMaterial");

    wall_left.transform.position = new Vector3(-1, 7, 0);
    wall_right = (GameObject) Instantiate(wall_left, new Vector3(6, 7, 0), Quaternion.identity);
    ground = (GameObject) Instantiate(wall_left, new Vector3(2.5f, 0, 0), Quaternion.identity);
    background = (GameObject) Instantiate(wall_left, new Vector3(2.5f, 7, 1), Quaternion.identity);
    backgroundPanel = (GameObject) Instantiate(wall_left, new Vector3(2.5f, 7, -5), Quaternion.identity);

    wall_left.transform.localScale = new Vector3(1,13,1);
    wall_right.transform.localScale = new Vector3(1,13,1);
    ground.transform.localScale = new Vector3(8,1,1);
    background.transform.localScale = new Vector3(20,15,1);
    backgroundPanel.transform.localScale = new Vector3(20,15,1);

    wall_left.name = "Wall_Left";
    wall_right.name = "Wall_Right";
    ground.name = "Ground";
    background.name = "Background";
    backgroundPanel.name = "Background Panel";

    wall_left.GetComponent<MeshRenderer>().material.color = Color.black;
    wall_right.GetComponent<MeshRenderer>().material.color = Color.black;
    ground.GetComponent<MeshRenderer>().material.color = Color.black;
    background.GetComponent<MeshRenderer>().material.color = Color.grey;
    backgroundPanel.GetComponent<MeshRenderer>().material.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    backgroundPanel.SetActive(false);
  }

  private void initializeDestroyBlock() {
    destroyBlockScript = gameObject.AddComponent<DestroyBlockScript>();
    destroyBlockScript.Initialize(this, blockPair, towerWidth, towerHeight);
    destroyBlockScript.audioManager = audioManager;
  }

  private void initializeBlockPair() {
    blockPair = new GameObject().AddComponent<BlockPairScript>();
    blockPair.name = "BlockPair";
    blockPair.towerHeight = towerHeight;
    blockPair.towerWidth = towerWidth;
    blockPair.manager = this;
    blockPair.audioManager = audioManager;
  }

  void initializeScoreText () {
    GameObject newCanvas = new GameObject("Canvas");
    Canvas c = newCanvas.AddComponent<Canvas>();
    c.renderMode = RenderMode.ScreenSpaceOverlay;
    newCanvas.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

    GameObject scoreTextObject = new GameObject("scoreText");
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

    GameObject alertTextObject = new GameObject("alertText");
    alertTextObject.AddComponent<CanvasRenderer>();
    alertText = alertTextObject.AddComponent<Text>();
    alertText.rectTransform.anchoredPosition = new Vector2(280,0);
    alertText.rectTransform.sizeDelta = new Vector2(200,600);
    alertText.text = "ALERT";
    alertText.transform.SetParent(newCanvas.transform, false);
    alertText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
    alertText.fontSize = 30;
    alertText.color = new Color(1,1,1,0);
    alertText.fontStyle = FontStyle.Bold;
    alertText.alignment = TextAnchor.MiddleCenter;
    alertTextObject.AddComponent<Shadow>().effectColor = Color.black;
  }

  public void flashAlert(string textToFlash) {
    alertText.text = textToFlash;
    alertFlashTime = 1f;
  }

  public void flashPermanentAlert(string textToFlash) {
    alertText.text = textToFlash;
    alertFlashTime = 0;
    alertText.color = new Color(1,1,1,1);

  }

  public void addPoints(int amount) {
    score += amount;
    updateText();
    debugPointsPerLevel[speed] += amount;
  }
  public void increaseSpeed () {
    flashAlert("Speed Up!");
    speed++;
    audioManager.gameSpeed = speed;
    updateText();
    if (speed % 10 == 0) {  // Spawn diamond every 10 levels
      blockPair.QueueDiamond();
    }
  }

  public void updateText() {
    scoreText.text = "Score: " + score + "\nSpeed: " + speed;
  }

  public void BlockDropped() {
    numBlocksDropped++;
    if (numBlocksDropped % 10 == 0) increaseSpeed();
  }

  public void CheckForDestroyBlocks(int scoreMultiplier) {
    if (diamondTouch) {
      destroyBlockScript.DoDiamondDestroy(diamondTouchColor, scoreMultiplier);
      diamondTouch = false;
    } else {
      destroyBlockScript.CheckForDestroyBlocks(scoreMultiplier);
    }
  }

  public void AddBlockToColumn(GameObject blockObject) {
    BlockScript block = blockObject.GetComponent<BlockScript>();
    if (block.type == "diamond") {
      diamondTouch = true;
      if (currentHeights[block.column] > 0) {
        diamondTouchColor = blockGrid[block.column, currentHeights[block.column] - 1].GetComponent<BlockScript>().type;
      } else {
        flashAlert("Tech Bonus");
        diamondTouchColor = "";
      }
    }
    block.isFalling = false;
    int column = block.column;
    currentHeights[column]++;
    if (currentHeights[column] > towerHeight) {  // TODO: ADJUST THIS SOMEHOW
      doGameOver();
      Destroy(blockObject);
    } else {
      blockObject.transform.position = new Vector3(column, currentHeights[column], 0);
      if (blockGrid[column, currentHeights[column] - 1] != null) Debug.Log("WARNING: GRID SPOT OCCUPIED: " + column + ", " + currentHeights[column]);
      blockGrid[column, currentHeights[column] - 1] = blockObject;
    }
  }

  private void doGameOver() {
    gameOver = true;
    blockPair.isActive = false;
    blockPair.gameOver = true;
    backgroundPanel.SetActive(true);

    if (score > 10) {
      highScoreManager.ShowHighScoreScreen(score, speed, true);
      doneWithHighScores = false;
      readyToStart = false;
      flashPermanentAlert("GAME OVER\n~\nGreat score!\n\nType your name and press ENTER to submit");
    } else {
      highScoreManager.ShowHighScoreScreen(score, speed, false);
      readyToStart = true;
      flashPermanentAlert("GAME OVER\n\nPress ENTER to begin");
    }

    audioManager.SetGameOver(true);
  }

  public void DoneWithHighScores() {
    doneWithHighScores = true;
  }

  void Update() {
    if (alertFlashTime > 0) {
      alertText.color = new Color(1,1,1,alertFlashTime);
      alertFlashTime -= Time.deltaTime;
    }
  }

  void LateUpdate() {
    if (gameOver) {
      if (readyToStart) {
        if (Input.GetKeyDown( KeyCode.Return ) || Input.GetKeyDown( KeyCode.KeypadEnter )) {
          audioManager.RestartAudio();
          audioManager.shouldPlayAudio = true;
          blockPair.controls_standard = true;
          startNewGame();
        } else if (Input.GetKeyDown( KeyCode.N )) {
          audioManager.RestartAudio();
          audioManager.shouldPlayAudio = true;
          blockPair.controls_standard = false;
          startNewGame();
        } else if (Input.GetKeyDown( KeyCode.M )) {
          audioManager.RestartAudio();
          audioManager.shouldPlayAudio = false;
          blockPair.controls_standard = true;
          startNewGame();
        }
      } else if (doneWithHighScores) {
        readyToStart = true;
      }
    }
  }

}
