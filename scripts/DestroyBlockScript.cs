using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DestroyBlockScript : MonoBehaviour {

  private ManagerScript manager;
  private int towerWidth;
  private int towerHeight;

  private BlockPairScript blockPair;
  private GameObject[,] blockGrid;
  private GameObject[] blocksToDestroy;
  private int[] minHeightOfDestroyedBlocks;
  private int numBlocksToDestroy;
  private int scoreMultiplier;
  private int numFallingBlocks;

  private int[,] pointsGrid;


  private float blockDestroySpeed = 5f;

  private int techBonusPointValue = 10;
  private int allClearPointBonus = 20;

  private int pointsToAdd = 0;

  public int debugPointsFromBlocks = 0;
  public int debugPointsFromDestroyedBlocks = 0;

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
    numBlocksToDestroy = 0;
    pointsGrid = new int[towerWidth, towerHeight];  // All values should initialize to zero

    minHeightOfDestroyedBlocks = new int[towerWidth];
    for (int i = 0 ; i < minHeightOfDestroyedBlocks.Length ; i ++) {
      minHeightOfDestroyedBlocks[i] = 10000;
    }

    if (type == "") { // Tech bonus
      for (int i = 0 ; i < towerWidth ; i++) {
        if (blockGrid[i,0] != null && blockGrid[i,0].GetComponent<BlockScript>().type == "diamond") {
          manager.currentHeights[i] = 0;
          minHeightOfDestroyedBlocks[i] = 0;
          blocksToDestroy[numBlocksToDestroy] = blockGrid[i,0];
          numBlocksToDestroy++;
          blockGrid[i,0] = null;
          pointsGrid[i,0] = techBonusPointValue * manager.speed;
          pointsToAdd += techBonusPointValue * manager.speed;
          return;
        }
      }
    }

    // else
    for (int i = 0 ; i < towerWidth ; i++) {
      for (int j = 0 ; j < towerHeight ; j++) {
        if (blockGrid[i,j] != null && (blockGrid[i,j].GetComponent<BlockScript>().type == type || blockGrid[i,j].GetComponent<BlockScript>().type == "diamond")) {
          if (manager.currentHeights[i] >= j) manager.currentHeights[i] = j;
          minHeightOfDestroyedBlocks[i] = Mathf.Min(j, minHeightOfDestroyedBlocks[i]);
          blocksToDestroy[numBlocksToDestroy] = blockGrid[i,j];
          numBlocksToDestroy++;
          pointsGrid[i,j] += numBlocksToDestroy;
          pointsToAdd += numBlocksToDestroy;
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

    Queue<int[]> destroySquares = new Queue<int[]>();

    int numDestroySquares = 0;
    pointsGrid = new int[towerWidth, towerHeight];  // All values should initialize to zero
    for (int i = 0 ; i < towerWidth - 1 ; i++) {
      for (int j = 0 ; j < towerHeight - 1 ; j++) {
        GameObject blockObject = blockGrid[i,j];
        if (blockObject == null) {
          break;  // blocks in a vertical stack must be contiguous
        }
        BlockScript blockScript = blockObject.GetComponent<BlockScript>();
        string type = blockScript.type;
        if (i + 1 >= towerWidth || j + 1 >= towerHeight ||
            blockGrid[i+1,j] == null || blockGrid[i+1,j].GetComponent<BlockScript>().type != type ||
            blockGrid[i,j+1] == null || blockGrid[i,j+1].GetComponent<BlockScript>().type != type ||
            blockGrid[i+1,j+1] == null || blockGrid[i+1,j+1].GetComponent<BlockScript>().type != type)
        {
          continue;
        } else {
          // Add block to pointsGrid
          pointsGrid[i,j] += 3; // CheckBlock will add +1, for a total of 4 points per DestroySquare block
          pointsGrid[i+1,j] += 3;
          pointsGrid[i,j+1] += 3;
          pointsGrid[i+1,j+1] += 3;
          destroySquares.Enqueue(new int[2]{i,j});
          numDestroySquares++;
        }
      }
    }
    if (numDestroySquares == 0) {
      blockPair.InitializeBlockPair();
    } else {
      Debug.Log("BLOCK BONUS: " + numDestroySquares * numDestroySquares * numDestroySquares * scoreMultiplier * manager.speed);
      manager.addPoints(numDestroySquares * numDestroySquares * numDestroySquares * scoreMultiplier * manager.speed);
      debugPointsFromBlocks += numDestroySquares * numDestroySquares * numDestroySquares * scoreMultiplier * manager.speed;
      startBlockDestroy(destroySquares);
    }
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

  private void startBlockDestroy(Queue<int[]> destroySquares) {
    blocksToDestroy = new GameObject[(towerHeight - 1) * (towerWidth - 1)];
    numBlocksToDestroy = 0;
    minHeightOfDestroyedBlocks = new int[towerWidth];

    for (int i = 0 ; i < minHeightOfDestroyedBlocks.Length ; i ++) {
      minHeightOfDestroyedBlocks[i] = 10000;
    }

    while (destroySquares.Count > 0) {
      int[] destroySquareCorner = destroySquares.Dequeue();
      string destroyType = blockGrid[destroySquareCorner[0], destroySquareCorner[1]].GetComponent<BlockScript>().type; // Dangerous but should never be null
      checkBlock(destroySquareCorner[0], destroySquareCorner[1], destroyType, new bool[towerWidth, towerHeight]);
    }

    for (int i = 0 ; i < towerWidth ; i++) {
      for (int j = 0 ; j < towerHeight ; j++) {
        if (pointsGrid[i,j] > 0 && blockGrid[i,j] != null) {
          pointsToAdd += pointsGrid[i,j];
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
      manager.addPoints(pointsToAdd * scoreMultiplier);
      Debug.Log("Destroy rectangle gives: " + (pointsToAdd * scoreMultiplier));
      debugPointsFromDestroyedBlocks += pointsToAdd * scoreMultiplier;
      pointsToAdd = 0;
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
    bool fieldEmpty = true;
    for (int i = 0 ; i < towerWidth ; i++) {
      if (blockGrid[i, 0] != null) {
        fieldEmpty = false;
        break;
      }
    }
    if (fieldEmpty) {
        manager.addPoints(manager.speed * allClearPointBonus);
      manager.flashAlert("All Clear");
    } else {
      CheckForDestroyBlocks(scoreMultiplier + 1);
    }
  }

  private void checkBlock(int i, int j, string type, bool[,] blockCheckGrid) {
    GameObject blockObject = blockGrid[i,j];
    if (blockObject == null || blockCheckGrid[i,j] == true) {
      return;
    }
    if (blockObject.GetComponent<BlockScript>().type == type) {
      pointsGrid[i,j] += 1;
      blockCheckGrid[i,j] = true;
      minHeightOfDestroyedBlocks[i] = Mathf.Min(j, minHeightOfDestroyedBlocks[i]);
      if (manager.currentHeights[i] >= j) manager.currentHeights[i] = j;
      if (i - 1 >= 0) checkBlock(i - 1, j, type, blockCheckGrid);
      if (i + 1 < towerWidth) checkBlock(i + 1, j, type, blockCheckGrid);
      if (j - 1 >= 0) checkBlock(i, j - 1, type, blockCheckGrid);
      if (j + 1 < towerHeight) checkBlock(i, j + 1, type, blockCheckGrid);
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
