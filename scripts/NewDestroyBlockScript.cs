using UnityEngine;
using System.Collections;

public class NewDestroyBlockScript : MonoBehaviour {

  private int numFallingBlocks = 0;
  private ManagerScript manager;

  private bool[,] blockCheckGrid;
  private int[] minHeightOfDestroyedBlocks;
  private GameObject[,] blockGrid;

  private int towerWidth;
  private int towerHeight;

  private int scoreMultiplier = 0;
  private int numBlocksToDestroy = 0;

  private GameObject[] blocksToDestroy;
  private float blockDestroySpeed = 5f;

  private int techBonusPointValue = 1000;

  public void DoDestroy(int leftOfBlock, int bottomOfBlock, string type, ManagerScript setManager, int setScoreMultiplier) {
    manager = setManager;
    towerHeight = manager.towerHeight;
    towerWidth = manager.towerWidth;
    scoreMultiplier = setScoreMultiplier;

    blocksToDestroy = new GameObject[(towerHeight - 1) * (towerWidth - 1)];

    blockCheckGrid = new bool[towerWidth, towerHeight];
    minHeightOfDestroyedBlocks = new int[towerWidth];
    for (int i = 0 ; i < minHeightOfDestroyedBlocks.Length ; i ++) {
      minHeightOfDestroyedBlocks[i] = 10000;
    }
    blockGrid = manager.blockGrid;

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

  public void DoDiamondDestroy(string type, ManagerScript setManager, int setScoreMultiplier) {
    manager = setManager;
    towerHeight = manager.towerHeight;
    towerWidth = manager.towerWidth;
    scoreMultiplier = setScoreMultiplier;
    blockGrid = manager.blockGrid;
    blocksToDestroy = new GameObject[(towerHeight - 1) * (towerWidth - 1)];

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

  private void finishDestroy() {
    manager.CheckForDestroyBlocks(scoreMultiplier + 1);
    Destroy(gameObject);
  }

  public void BlockLanded() {
    numFallingBlocks--;
    if (numFallingBlocks <= 0) {
      finishDestroy();
    }
  }

  private void endDestroyBlocks() {
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

  void Update() {
    if (numBlocksToDestroy > 0) {
      for (int i = 0 ; i < numBlocksToDestroy ; i++) {
        blocksToDestroy[i].transform.localScale -= new Vector3(1,1,0) * Time.deltaTime * blockDestroySpeed;
      }
      if (blocksToDestroy[0].transform.localScale.x <= 0) {
        endDestroyBlocks();
      }
    }

  }


}