using UnityEngine;
using System.Collections;

public class BlockScript : MonoBehaviour {

  public int column;
  public bool isFalling = true;
  public static float fallSpeed = 0.5f;
  public string type;

  public float fallSpeedMultiplier = 1f;

  public bool isLeft = false;

  public BlockPairScript blockPair;
  public DestroyBlockScript destroyBlock;

  ManagerScript manager;

	// Use this for initialization
	void Awake () {
    manager = GameObject.Find("Manager").GetComponent<ManagerScript>();
	}

  public void SetFallSpeed(float newSpeed) {
    fallSpeed = newSpeed;
  }

  public void AddBlockToColumn() {
    manager.AddBlockToColumn(gameObject);
    fallSpeedMultiplier = 1;
    if (blockPair != null) {
      blockPair.RemoveBlockFromPair(isLeft);
    }
    if (destroyBlock != null) {
      destroyBlock.BlockLanded();
    }

  }

	// Update is called once per frame
	void Update () {
    if (isFalling) {
      float actualFallSpeed = Mathf.Min(30f, fallSpeed * fallSpeedMultiplier);
      if (Input.GetKey( KeyCode.S ) || Input.GetKey( KeyCode.DownArrow)) {
        actualFallSpeed = Mathf.Max(10f, actualFallSpeed);
      }
      transform.position += Vector3.down * actualFallSpeed * Time.deltaTime;
      float columnHeight = manager.currentHeights[column];
      if (transform.position.y <= columnHeight + 1) {
        AddBlockToColumn();
      }
    }
	}
}
