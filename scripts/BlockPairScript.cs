using UnityEngine;
using System.Collections;
using BlockGame;

public class BlockPairScript : MonoBehaviour {


  private GameObject blockInstance;
  public int towerHeight;
  public int towerWidth;

  public BlockScript leftBlock;
  public BlockScript rightBlock;

  public GameObject previewLeftBlock;
  public GameObject previewRightBlock;
  private string previewLeftType;
  private string previewRightType;

  public ManagerScript manager;

  public bool isActive;
  public bool gameOver = false;

  public int orientation = 0;  // Left is pivot, right is oriented 90 * orientation degrees

  private float leftHoldTime = 0;
  private float rightHoldTime = 0;
  private float holdTime = 0.5f;  // Hold time until blocks slide

  private bool queueDiamond = false;


  void Awake() {
    initializeBlockInstance();
  }

  public void InitializePreviewBlocks() {

    previewLeftBlock = GameObject.CreatePrimitive(PrimitiveType.Cube);
    previewLeftBlock.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/UnlitMaterial");

    previewLeftBlock.transform.position = new Vector3(-1, 14, 0);
    previewRightBlock = (GameObject) Instantiate(previewLeftBlock, new Vector3(6, 14, 0), Quaternion.identity);

    previewLeftBlock.name = "PreviewBlock_Left";
    previewRightBlock.name = "PreviewBlock_Right";

    previewLeftType = getRandomElement();
    previewRightType = getRandomElement();

    previewLeftBlock.GetComponent<MeshRenderer>().material.color = getColorByType(previewLeftType);
    previewRightBlock.GetComponent<MeshRenderer>().material.color = getColorByType(previewRightType);
  }

  private void initializeBlockInstance() {
    blockInstance = GameObject.CreatePrimitive(PrimitiveType.Cube);
    Destroy(blockInstance.GetComponent<BoxCollider>());
    blockInstance.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/UnlitMaterial");
    blockInstance.name = "Block";
    blockInstance.AddComponent<BlockScript>();
    blockInstance.SetActive(false);
  }


  public void InitializeBlockPair() {
    if (gameOver) return;
    isActive = true;

    leftBlock = SpawnBlock(previewLeftType, 2);
    rightBlock = SpawnBlock(previewRightType, 3);

    if (previewRightType == "diamond") previewRightBlock.GetComponent<DiamondScript>().RemoveDiamondScript();

    if (previewLeftBlock == null) {
      InitializePreviewBlocks();
    } else if (queueDiamond) {
      previewLeftType = getRandomElement();
      previewRightType = "diamond";
      queueDiamond = false;

      previewLeftBlock.GetComponent<MeshRenderer>().material.color = getColorByType(previewLeftType);
      previewRightBlock.AddComponent<DiamondScript>();
    } else {
      previewLeftType = getRandomElement();
      previewRightType = getRandomElement();

      previewLeftBlock.GetComponent<MeshRenderer>().material.color = getColorByType(previewLeftType);
      previewRightBlock.GetComponent<MeshRenderer>().material.color = getColorByType(previewRightType);
    }

    leftBlock.SetFallSpeed(1f + 0.3f * manager.speed);

    leftBlock.isLeft = true;
    rightBlock.isLeft = false;

    leftBlock.blockPair = this;
    rightBlock.blockPair = this;

    orientation = 0;
  }

  private string getRandomElement() {
    // return "fire"; // DEBUG
    float randomVal = Random.value * 4;
    if (randomVal <= 1) return "earth";
    else if (randomVal <= 2) return "air";
    else if (randomVal <= 3) return "water";
    else return "fire";
  }

  public void RemoveBlockFromPair(bool isLeft) {
    if (isLeft) {
      leftBlock = null;
    } else {
      rightBlock = null;
    }
  }

  private Color getColorByType(string type) {
    if (type == "earth") {
      return new Color(0.5f, 0.25f, 0f);
    } else if (type == "air") {
      return new Color (0.5f, 1f, 0.7f);
    } else if (type == "water") {
      return Color.blue;
    } else {
      return Color.red;
    }
  }

  public BlockScript SpawnBlock(string type, int column) {
    GameObject blockObject = (GameObject) Instantiate(blockInstance, new Vector3(column, towerHeight + 1, 0), Quaternion.identity);
    blockObject.SetActive(true);
    BlockScript block = blockObject.GetComponent<BlockScript>();
    block.column = column;
    block.type = type;

    if (type == "diamond") {
      blockObject.AddComponent<DiamondScript>();
    } else {
      blockObject.GetComponent<MeshRenderer>().material.color = getColorByType(type);
    }

    return block;
  }

  private void checkInputs() {

    if (Input.GetKeyDown( KeyCode.A ) || Input.GetKeyDown( KeyCode.LeftArrow)) {
      tryHorizontalMove(-1);
      leftHoldTime = 0;
    }
    if (Input.GetKeyDown( KeyCode.D ) || Input.GetKeyDown( KeyCode.RightArrow)) {
      tryHorizontalMove(1);
      rightHoldTime = 0;
    }
    if (Input.GetKey( KeyCode.A ) || Input.GetKey( KeyCode.LeftArrow)) {
      leftHoldTime += Time.deltaTime;
      if (leftHoldTime > holdTime) {
        tryHorizontalMove(-1);
      }
    }
    if (Input.GetKey( KeyCode.D ) || Input.GetKey( KeyCode.RightArrow)) {
      rightHoldTime += Time.deltaTime;
      if (rightHoldTime > holdTime) {
        tryHorizontalMove(1);
      }
    }
    // Drop is handled via blockpair speed
    // if (Input.GetKeyDown( KeyCode.S ) || Input.GetKeyDown( KeyCode.DownArrow)) {
    //   tryDrop(1);
    // }
    if (Input.GetKeyDown( KeyCode.W ) || Input.GetKeyDown( KeyCode.UpArrow)) {
      tryFullDrop();
    }
    if (Input.GetKeyDown( KeyCode.Q ) || Input.GetKeyDown( KeyCode.Space)) {
      tryRotate(1);
    }
    if (Input.GetKeyDown( KeyCode.E ) || Input.GetKeyDown( KeyCode.LeftShift) || Input.GetKeyDown( KeyCode.RightShift)) {
      tryRotate(-1);
    }

  }

  private void tryHorizontalMove(int direction) {

    if (leftBlock != null) {
      int newLeft = leftBlock.column + direction;
      if (newLeft < 0 || newLeft >= towerWidth) return;
      float newLeftColumnHeight = manager.currentHeights[newLeft];
      if (leftBlock.transform.position.y < newLeftColumnHeight + 1) return;
    }

    if (rightBlock != null) {
      int newRight = rightBlock.column + direction;
      if (newRight < 0 || newRight >= towerWidth) return;
      float newRightColumnHeight = manager.currentHeights[newRight];
      if (rightBlock.transform.position.y < newRightColumnHeight) return;
    }

    if (leftBlock != null) {
      leftBlock.column += direction;
      leftBlock.transform.position = new Vector3(leftBlock.column, leftBlock.transform.position.y, 0);
    }
    if (rightBlock != null) {
      rightBlock.column += direction;
      rightBlock.transform.position = new Vector3(rightBlock.column, rightBlock.transform.position.y, 0);
    }
  }

  public void tryFullDrop() {
    if (leftBlock == null || rightBlock == null) {
      if (leftBlock != null) leftBlock.AddBlockToColumn();
      if (rightBlock != null) rightBlock.AddBlockToColumn();
    } else if (leftBlock.transform.position.y < rightBlock.transform.position.y) {
      leftBlock.AddBlockToColumn();
      rightBlock.AddBlockToColumn();
    } else {
      rightBlock.AddBlockToColumn();
      leftBlock.AddBlockToColumn();
    }
  }

  // Positive direction is CCW, negative is CW
  private void tryRotate(int direction) {
    if (leftBlock == null || rightBlock == null) return;

    int newDirection = (direction + 4 + orientation) % 4;
    Vector3 testPositionOffset = Vector3.zero;
    if (newDirection == 0) testPositionOffset = new Vector3(1,0,0);
    else if (newDirection == 1) testPositionOffset = new Vector3(0,1,0);
    else if (newDirection == 2) testPositionOffset = new Vector3(-1,0,0);
    else if (newDirection == 3) testPositionOffset = new Vector3(0,-1,0);

    Vector3 testPosition = leftBlock.transform.position + testPositionOffset;
    if (testPosition.x < 0 || testPosition.x >= towerWidth) {
      trySwapRotate(direction);
      return;
    }

    float newTestColumnHeight = manager.currentHeights[Mathf.FloorToInt(testPosition.x)];
    if (testPosition.y < newTestColumnHeight) {
      trySwapRotate(direction);
      return;
    }

    orientation = newDirection;
    rightBlock.column = Mathf.FloorToInt(testPosition.x);
    rightBlock.transform.position = testPosition;

  }

  private void trySwapRotate(int direction) {
    if (leftBlock == null || rightBlock == null) return;

    int newDirection = (-direction + 4 + orientation) % 4;
    Vector3 testPositionOffset = Vector3.zero;
    if (newDirection == 0) testPositionOffset = new Vector3(1,0,0);
    else if (newDirection == 1) testPositionOffset = new Vector3(0,1,0);
    else if (newDirection == 2) testPositionOffset = new Vector3(-1,0,0);
    else if (newDirection == 3) testPositionOffset = new Vector3(0,-1,0);

    Vector3 testPosition = leftBlock.transform.position + testPositionOffset;
    if (testPosition.x < 0 || testPosition.x >= towerWidth) {
      doSwap();
      return;
    }

    float newTestColumnHeight = manager.currentHeights[Mathf.FloorToInt(testPosition.x)];
    if (testPosition.y < newTestColumnHeight) {
      doSwap();
      return;
    }

    orientation = (newDirection + 2) % 4;
    rightBlock.column = leftBlock.column;
    rightBlock.transform.position = leftBlock.transform.position;
    leftBlock.column = Mathf.FloorToInt(testPosition.x);
    leftBlock.transform.position = testPosition;

  }

  void doSwap() {
    orientation = (orientation + 2) % 4;
    int tempColumn = rightBlock.column;
    Vector3 tempPosition = rightBlock.transform.position;
    rightBlock.column = leftBlock.column;
    rightBlock.transform.position = leftBlock.transform.position;
    leftBlock.column = tempColumn;
    leftBlock.transform.position = tempPosition;
  }

  public void QueueDiamond() {
    queueDiamond = true;
  }

  void Update() {
    if (isActive) {
      if (leftBlock == null && rightBlock == null) {
        isActive = false;
        manager.BlockDropped();
        manager.CheckForDestroyBlocks(0);
      }
      checkInputs();
    }
  }

}
