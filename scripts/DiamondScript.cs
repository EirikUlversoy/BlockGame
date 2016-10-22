using UnityEngine;
using System.Collections;

public class DiamondScript : MonoBehaviour {

  private Material defaultMaterial;
  private Material diamondMaterial;

  private MeshRenderer renderer;

  void Start() {
    renderer = gameObject.GetComponent<MeshRenderer>();
    defaultMaterial = renderer.material;
    diamondMaterial = Resources.Load<Material>("Materials/DiamondMaterial");
    renderer.material = diamondMaterial;
  }

  public void RemoveDiamondScript() {
    renderer.material = defaultMaterial;
    Destroy(this);
  }


}