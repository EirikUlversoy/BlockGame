using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DestroyBlockScript : MonoBehaviour {

  private ManagerScript manager;
  private int towerWidth;
  private int towerHeight;

  private BlockPairScript blockPair;
  private GameObject[,] blockGrid;
  public GameObject[] blocksToDestroy;
  private bool[,] blockCheckGrid;
  public int[] minHeightOfDestroyedBlocks;
  public int numBlocksToDestroy;
  public int scoreMultiplier;
  public int numFallingBlocks;

  private float blockDestroySpeed = 5f;

  private int techBonusPointValue = 1000;

  public void Initialize(ManagerScript setManager, BlockPairScript setBlockPair, int width, int height) {
    manager = setManager;
    towerWidth = width;
    towerHeight = height;
    blockPair = setBlockPair;
  }

  public void DoDiamondDestroy(string type, int setScoreMultiplier) {
    scoreMultiplier = setScoreMultiplier;
    blockGrid = manager.blockGrid;
    blocksToDestroy = new GameObject[towerHeight * towerWidth];

    minHeightOfDestroyedBlocks = new int[towerWidth];
    for (int i = 0 ; i < minHeightOfDestroyedBlocks.Length ; i ++) {
      minHeightOfDestroyedBlocks[i] = 10000;
    }

    for (int i = 0 ; i < towerWidth ; i++) {
      for (int j = 0 ; j < towerHeight ; j++) {
        if (blockGrid[i,j] != null && (blockGrid[i,j].GetComponent<BlockScript>().type == type || blockGrid[i,j].GetComponent<BlockScript>().type == "diamond")) {
          if (manager.currentHeights[i] >= j) manager.currentHeights[i] = j;
          minHeightOfDestroyedBlocks[i] = Mathf.Min(j, minHeightOfDestroyedBlocks[i]);
          blocksToDestroy[numBlocksToDestroy] = blockGrid[i,j];
          numBlocksToDestroy++;
          blockGrid[i,j] = null;
        }
      }
    }
  }

  // INITIALZE
  public void CheckForDestroyBlocks(int setScoreMultiplier) {
    blockGrid = manager.blockGrid;
    scoreMultiplier = setScoreMultiplier;
    numFallingBlocks = 0;
    for (int i = 0 ; i < towerWidth - 1 ; i++) {
      for (int j = 0 ; j < towerHeight - 1 ; j++) {
        GameObject blockObject = blockGrid[i,j];
        if (blockObject == null) {
          j = 100;
          continue;  // Break?
        }
        BlockScript blockScript = blockObject.GetComponent<BlockScript>();
        string type = blockScript.type;
        if (checkNode(type, new int[2]{i,j}, new int[2]{i+1,j+1}, true, true)) {
          int rectangleDimensions = getMaxRectangleArea(i, j, type);
          Debug.Log(rectangleDimensions);
          startBlockDestroy(i, j, type);
          return;
        }
      }
    }
    blockPair.InitializeBlockPair();
  }

  private int getMaxRectangleArea(int leftOfBlock, int bottomOfBlock, string type) {
    bool[,] blockVisitedGrid = new bool[towerWidth, towerHeight];
    Queue<int[]> blocksToCheck = new Queue<int[]>();
    int maxRectangleArea = 4;
    // int[] maxRectangleCorner = new int[2]{leftOfBlock + 1, bottomOfBlock + 1};
    blocksToCheck.Enqueue(new int[2]{leftOfBlock + 1, bottomOfBlock + 1});
    while (blocksToCheck.Count > 0) {
      int[] blockToCheck = blocksToCheck.Dequeue();
      if (blockVisitedGrid[blockToCheck[0], blockToCheck[1]]) {
        continue;
      } else {
        blockVisitedGrid[blockToCheck[0], blockToCheck[1]] = true;
      }

      // check vertical
      bool verticalCheck = true;
      if (blockToCheck[1] + 1 >= towerHeight) {
        verticalCheck = false;
      }
      for (int i = leftOfBlock ; verticalCheck && i <= blockToCheck[0] ; i++) {
        if (i >= towerWidth) {
          verticalCheck = false;
        }
        GameObject block = blockGrid[i, blockToCheck[1] + 1];
        if (block == null || block.GetComponent<BlockScript>().type != type) {
          verticalCheck = false;
        }
      }
      if (verticalCheck) {
        int[] blockToEnqueue = new int[2]{blockToCheck[0], blockToCheck[1] + 1};
        blocksToCheck.Enqueue(blockToEnqueue);
        int rectangleArea = (blockToEnqueue[0] - leftOfBlock + 1) * (blockToEnqueue[1] - bottomOfBlock + 1);
        if (rectangleArea > maxRectangleArea) {
          maxRectangleArea = rectangleArea;
          // maxRectangleCorner = blockToEnqueue;
        }
      }

      // Check horizontal
      bool horizontalCheck = true;
      if (blockToCheck[0] + 1 >= towerWidth) {
        horizontalCheck = false;
      }
      for (int j = bottomOfBlock ; horizontalCheck && j <= blockToCheck[1] ; j++) {
        if (j >= towerHeight) {
          horizontalCheck = false;
        }
        GameObject block = blockGrid[blockToCheck[0] + 1, j];
        if (block == null || block.GetComponent<BlockScript>().type != type) {
          horizontalCheck = false;
        }
      }
      if (horizontalCheck) {
        int[] blockToEnqueue = new int[2]{blockToCheck[0] + 1, blockToCheck[1]};
        blocksToCheck.Enqueue(blockToEnqueue);
        int rectangleArea = (blockToEnqueue[0] - leftOfBlock + 1) * (blockToEnqueue[1] - bottomOfBlock + 1);
        if (rectangleArea > maxRectangleArea) {
          maxRectangleArea = rectangleArea;
          // maxRectangleCorner = blockToEnqueue;
        }
      }
    }

    return maxRectangleArea;
  }

  private bool checkNode(string type, int[] lowerLeftCorner, int[] upperRightCorner, bool checkHorizontal, bool checkVertical) {
    if (upperRightCorner[0] >= towerWidth ||
        upperRightCorner[1] >= towerHeight ||
        blockGrid[upperRightCorner[0], upperRightCorner[1]] == null ||
        blockGrid[upperRightCorner[0], upperRightCorner[1]].GetComponent<BlockScript>().type != type) {
      return false;
    }
    if (checkHorizontal) {
      for (int i = lowerLeftCorner[0] ; i < upperRightCorner[0] ; i++) {
        GameObject obj = blockGrid[i,upperRightCorner[1]];
        if (obj == null || obj.GetComponent<BlockScript>().type != type) {
          return false;
        }
      }
    }
    if (checkVertical) {
      for (int j = lowerLeftCorner[1] ; j < upperRightCorner[1] ; j++) {
        GameObject obj = blockGrid[upperRightCorner[0], j];
        if (obj == null || obj.GetComponent<BlockScript>().type != type) {
          return false;
        }
      }
    }
    return true;
  }

  private void startBlockDestroy(int leftOfBlock, int bottomOfBlock, string type) {
    blocksToDestroy = new GameObject[(towerHeight - 1) * (towerWidth - 1)];
    blockCheckGrid = new bool[towerWidth, towerHeight];
    minHeightOfDestroyedBlocks = new int[towerWidth];

    for (int i = 0 ; i < minHeightOfDestroyedBlocks.Length ; i ++) {
      minHeightOfDestroyedBlocks[i] = 10000;
    }

    checkBlock(leftOfBlock, bottomOfBlock, type);

    for (int i = 0 ; i < towerWidth ; i++) {
      for (int j = 0 ; j < towerHeight ; j++) {
        if (blockCheckGrid[i,j]) {
          blocksToDestroy[numBlocksToDestroy] = blockGrid[i,j];
          numBlocksToDestroy++;
          blockGrid[i,j] = null;
        }
      }
    }
  }

  private void endBlockDestroy() {
    if (numBlocksToDestroy == 1) { // Diamond tech
      manager.addPoints(techBonusPointValue);
    } else {
      manager.addPoints(numBlocksToDestroy * (scoreMultiplier + 1) * manager.speed);
    }
    for (int i = 0 ; i < numBlocksToDestroy ; i++) {
      if (blocksToDestroy[i].GetComponent<DiamondScript>()) {
        blocksToDestroy[i].GetComponent<DiamondScript>().RemoveDiamondScript();
      }
      Destroy(blocksToDestroy[i]);
      blocksToDestroy[i] = null;
    }
    numBlocksToDestroy = 0;

    // Set blocks to fall
    for (int i = 0 ; i < towerWidth ; i++) {
      int minHeightOfColumn = minHeightOfDestroyedBlocks[i];
      for (int j = minHeightOfColumn + 1 ; j < towerHeight ; j++) {
        GameObject blockObject = blockGrid[i,j];
        if (blockObject == null) {
          continue;
        } else {
          blockGrid[i,j] = null;
          blockObject.GetComponent<BlockScript>().fallSpeedMultiplier = 10;
          blockObject.GetComponent<BlockScript>().isFalling = true;
          // blockObject.GetComponent<BlockScript>().destroyBlock = this;
          numFallingBlocks++;
        }
      }
    }

    if (numFallingBlocks <= 0) finishDestroy();

  }

  public void BlockLanded() {
    if (numFallingBlocks > 0) {
      numFallingBlocks--;
      if (numFallingBlocks <= 0) {
        finishDestroy();
      }
    }
  }

  private void finishDestroy() {
    CheckForDestroyBlocks(scoreMultiplier + 1);
  }

  private void checkBlock(int i, int j, string type) {
    GameObject blockObject = blockGrid[i,j];
    if (blockObject == null || blockCheckGrid[i,j] == true) {
      return;
    }
    if (blockObject.GetComponent<BlockScript>().type == type) {
      blockCheckGrid[i,j] = true;
      minHeightOfDestroyedBlocks[i] = Mathf.Min(j, minHeightOfDestroyedBlocks[i]);
      if (manager.currentHeights[i] >= j) manager.currentHeights[i] = j;
      if (i - 1 >= 0) checkBlock(i - 1, j, type);
      if (i + 1 < towerWidth) checkBlock(i + 1, j, type);
      if (j - 1 >= 0) checkBlock(i, j - 1, type);
      if (j + 1 < towerHeight) checkBlock(i, j + 1, type);
    }
  }

  void Update() {
    if (numBlocksToDestroy > 0) {
      for (int i = 0 ; i < numBlocksToDestroy ; i++) {
        blocksToDestroy[i].transform.localScale -= new Vector3(1,1,0) * Time.deltaTime * blockDestroySpeed;
      }
      if (blocksToDestroy[0].transform.localScale.x <= 0) {
        endBlockDestroy();
      }
    }

  }
}
