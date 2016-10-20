using System;
using UnityEngine;

namespace BlockGame {

  public class GameLoad {

    public GameLoad() {
    }

    public void LoadGame() {

      GameObject manager = new GameObject();
      manager.name = "Manager";
      manager.AddComponent<ManagerScript>();

    }


  }

}