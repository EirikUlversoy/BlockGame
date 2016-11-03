using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

public class HighScoreScript : MonoBehaviour {

  string hostname = "localhost:3000";
  // string hostname = "https://www.bradyfukumoto.com";

  private JSONNode high_scores;
  private bool waitingForWWW = false;

  private string debugName = "Brady";
  private int debugScore = 100;
  private int debugSpeed = 5;

  private GameObject highScoreField;

  void Start() {
    waitingForWWW = true;
    initializeHighScoreScreen();
    ShowHighScoreScreen();
    // StartCoroutine(GetHighScores());
    // StartCoroutine(SubmitHighScore());
  }

  private void initializeHighScoreScreen() {
    GameObject newCanvas = new GameObject("Canvas");
    Canvas c = newCanvas.AddComponent<Canvas>();
    // newCanvas.AddComponent<GraphicRaycaster>();
    c.renderMode = RenderMode.ScreenSpaceOverlay;
    newCanvas.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

    EventSystem eventSystem = new GameObject().AddComponent<EventSystem>();
    eventSystem.gameObject.name = "Event System";
    eventSystem.gameObject.AddComponent<StandaloneInputModule>();

    highScoreField = new GameObject("HighScoreNameInput");
    highScoreField.AddComponent<RectTransform>();
    highScoreField.AddComponent<CanvasRenderer>();
    InputField inputField = highScoreField.AddComponent<InputField>();
    highScoreField.transform.SetParent(newCanvas.transform, false);
    GameObject highScoreFieldText = new GameObject("HighScoreFieldText");
    highScoreFieldText.transform.SetParent(highScoreField.transform, false);
    Text scoreText = highScoreFieldText.AddComponent<Text>();
    scoreText.text = "TEST";
    inputField.textComponent = scoreText;
    scoreText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
    scoreText.fontSize = 24;


    inputField.ActivateInputField();
    // eventSystem.SetSelectedGameObject(inputField.gameObject, null);
    // inputField.OnPointerClick (new PointerEventData(eventSystem));



    // scoreText.rectTransform.anchoredPosition = new Vector2(0,30);
    // scoreText.rectTransform.sizeDelta = new Vector2(200,30);
    // scoreText.transform.SetParent(newCanvas.transform, false);
    // scoreText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
    // scoreText.fontSize = 36;
    // scoreText.color = Color.white;
    // scoreText.fontStyle = FontStyle.Bold;
    // scoreText.alignment = TextAnchor.UpperLeft;
    // scoreTextObject.AddComponent<Shadow>().effectColor = Color.black;
  }

  public void ShowHighScoreScreen() {

  }

  // curl -H "Content-Type: application/json" -X POST -d '{"user":{"email":"bradyfukumoto@gmail.com","password":"[PASSWORD]"}}' https://www.bradyfukumoto.com/api/v1/sessions
  IEnumerator GetHighScores() {
    // We should only read the screen after all rendering is complete
    yield return new WaitForEndOfFrame();

    // Upload to a cgi script
    WWW w = new WWW(hostname + "/api/v1/block_game_high_scores");
    yield return w;
    waitingForWWW = false;
    if (!string.IsNullOrEmpty(w.error)) {
      Debug.Log("ERROR");
    }
    else {
      high_scores = JSON.Parse(w.text);
      Debug.Log(high_scores["high_scores"]);
    }
  }

  // curl -H "Content-Type: application/json" -X POST -d '{"high_score":{"user_id":"1","game_id":"1","score":"100"}}' https://www.bradyfukumoto.com/api/v1/high_scores
  IEnumerator SubmitHighScore() {
    // We should only read the screen after all rendering is complete
    yield return new WaitForEndOfFrame();

    // Create a Web Form
    WWWForm form = new WWWForm();
    form.AddField("high_score[name]", debugName);
    form.AddField("high_score[score]", debugScore);
    form.AddField("high_score[speed]", debugSpeed);
    form.AddField("score_hash", nameScoreHash(debugName, debugScore, debugSpeed));

    // Upload to a cgi script
    WWW w = new WWW(hostname + "/api/v1/block_game_high_scores", form);
    yield return w;
    if (!string.IsNullOrEmpty(w.error)) {
      print(w.error);
      Debug.Log("ERROR SUBMITTING SCORE");
    }
    else {
      print(w.text);
      JSONNode high_score = JSON.Parse(w.text);
      Debug.Log(w.text);
    }
  }


  // Hash name and score for validation on server
  private int nameScoreHash(string name, int score, int speed) {
    char[] nameChars = name.ToCharArray();
    int sum = 0;
    for (int i = 0 ; i < nameChars.Length ; i++) {
      sum += nameChars[i];
    }
    return sum ^ (score + speed);
  }


}