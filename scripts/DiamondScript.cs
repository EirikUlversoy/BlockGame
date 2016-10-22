using UnityEngine;
using System.Collections;

public class DiamondScript : MonoBehaviour {

  private Material defaultMaterial;
  private Material diamondMaterial;

  private MeshRenderer renderer;

  private GameObject lightTransformParent;
  private Light[] lights;

  private float lightDistance = 3;
  private float rotateSpeed = 360;

  void Start() {
    gameObject.name = "Diamond";
    renderer = gameObject.GetComponent<MeshRenderer>();
    defaultMaterial = renderer.material;
    diamondMaterial = Resources.Load<Material>("Materials/DiamondMaterial");
    renderer.material = diamondMaterial;

    lightTransformParent = new GameObject();
    lightTransformParent.name = "DiamondLightTransformParent";
    lightTransformParent.transform.parent = transform;
    lightTransformParent.transform.localPosition = Vector3.zero;
    lightTransformParent.transform.localEulerAngles = new Vector3(0,0,45);

    lights = new Light[4];

    for (int i = 0 ; i < lights.Length ; i++) {
      Light light = new GameObject().AddComponent<Light>();
      light.transform.parent = lightTransformParent.transform;
      light.gameObject.name = "Light_" + i;

      if (i == 0) {
        light.transform.localPosition = new Vector3(lightDistance,0,0);
        light.color = new Color(0.5f, 0.25f, 0f); // earth
      } else if (i == 1) {
        light.transform.localPosition = new Vector3(0,lightDistance,0);
        light.color = new Color(0.5f, 1f, 0.7f); // air
      } else if (i == 2) {
        light.transform.localPosition = new Vector3(-lightDistance,0,0);
        light.color = Color.blue; // water
      } else {
        light.transform.localPosition = new Vector3(0,-lightDistance,0);
        light.color = Color.red; // fire
      }

      lights[i] = light;
    }
  }

  public void RemoveDiamondScript() {
    for (int i = 0 ; i < lights.Length ; i++) {
      Destroy(lights[i]);
    }
    Destroy(lightTransformParent);
    renderer.material = defaultMaterial;
    Destroy(this);
  }

  void Update() {
    if (lightTransformParent != null) {
      lightTransformParent.transform.localEulerAngles += new Vector3(0,1,0) * (rotateSpeed * Time.deltaTime);
    }
  }


}