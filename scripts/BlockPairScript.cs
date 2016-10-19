﻿using UnityEngine;
using System.Collections;

public class BlockPairScript : MonoBehaviour {


  public GameObject blockPrefab;
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


  public void InitializePreviewBlocks(GameObject leftPreview, GameObject rightPreview) {
    previewLeftBlock = leftPreview;
    previewRightBlock = rightPreview;

    previewLeftType = getRandomElement();
    previewRightType = getRandomElement();

    previewLeftBlock.GetComponent<MeshRenderer>().material.color = getColorByType(previewLeftType);
    previewRightBlock.GetComponent<MeshRenderer>().material.color = getColorByType(previewRightType);
  }


  public void InitializeBlockPair() {
    if (gameOver) return;
    isActive = true;

    leftBlock = SpawnBlock(previewLeftType, 2);
    rightBlock = SpawnBlock(previewRightType, 3);

    InitializePreviewBlocks(previewLeftBlock, previewRightBlock);

    leftBlock.SetFallSpeed(0.2f + 0.3f * manager.speed);

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
    GameObject blockObject = (GameObject) Instantiate(blockPrefab, new Vector3(column, towerHeight + 1, 0), Quaternion.identity);
    BlockScript block = blockObject.GetComponent<BlockScript>();
    block.column = column;
    block.type = type;

    blockObject.GetComponent<MeshRenderer>().material.color = getColorByType(type);

    return block;
  }

  private void checkInputs() {

    if (Input.GetKeyDown( KeyCode.A ) || Input.GetKeyDown( KeyCode.LeftArrow)) {
      tryHorizontalMove(-1);
    }
    if (Input.GetKeyDown( KeyCode.D ) || Input.GetKeyDown( KeyCode.RightArrow)) {
      tryHorizontalMove(1);
    }
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

  // private void tryDrop (int amount) {
  //   if (leftBlock != null) leftBlock.transform.position += Vector3.down * amount;
  //   if (rightBlock != null) rightBlock.transform.position += Vector3.down * amount;
  // }

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
    if (testPosition.x < 0 || testPosition.x >= towerWidth) return;

    float newTestColumnHeight = manager.currentHeights[Mathf.FloorToInt(testPosition.x)];
    if (testPosition.y < newTestColumnHeight) return;

    orientation = newDirection;
    rightBlock.column = Mathf.FloorToInt(testPosition.x);
    rightBlock.transform.position = testPosition;

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
