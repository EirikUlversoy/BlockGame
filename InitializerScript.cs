using UnityEngine;
using System.Collections;
using BlockGame;

public class InitializerScript : MonoBehaviour {

  void Start () {
    GameLoad loader = new GameLoad();
    loader.LoadGame();
  }

}