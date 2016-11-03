using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

public class HighScoreScript : MonoBehaviour {

  // private string hostname = "localhost:3000";
  private string hostname = "https://www.bradyfukumoto.com";

  private ManagerScript manager;

  private JSONNode high_scores;

  private string playerName = "";
  private int endScore = 0;
  private int endSpeed = 5;

  private GameObject highScoreField;
  private Text highScoreFieldText;
  private GameObject canvasObject;
  private GameObject highScoreTextObject;
  private Text highScoreText;
  private InputField inputField;

  private string version = "v0.1";

  private bool inputFieldShouldBeActive = false;


  void Start() {
    StartCoroutine(GetHighScores());
    initializeHighScoreScreen();

    manager = GameObject.Find("Manager").GetComponent<ManagerScript>();
  }

  private void initializeHighScoreScreen() {

    canvasObject = new GameObject("Canvas");
    Canvas c = canvasObject.AddComponent<Canvas>();
    c.renderMode = RenderMode.ScreenSpaceOverlay;
    canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

    EventSystem eventSystem = new GameObject().AddComponent<EventSystem>();
    eventSystem.gameObject.name = "Event System";
    eventSystem.gameObject.AddComponent<StandaloneInputModule>();

    highScoreField = new GameObject("HighScoreNameInput");
    highScoreField.AddComponent<RectTransform>();
    highScoreField.AddComponent<CanvasRenderer>();
    inputField = highScoreField.AddComponent<InputField>();
    highScoreField.transform.SetParent(canvasObject.transform, false);
    GameObject highScoreFieldTextObject = new GameObject("HighScoreFieldText");
    highScoreFieldTextObject.transform.SetParent(highScoreField.transform, false);
    highScoreFieldText = highScoreFieldTextObject.AddComponent<Text>();
    highScoreFieldText.rectTransform.anchoredPosition = new Vector2(0,0);
    highScoreFieldText.rectTransform.sizeDelta = new Vector2(300,100);
    inputField.textComponent = highScoreFieldText;
    highScoreFieldText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
    highScoreFieldText.fontSize = 24;
    highScoreFieldText.color = Color.white;
    highScoreFieldText.fontStyle = FontStyle.Bold;
    highScoreFieldText.alignment = TextAnchor.UpperCenter;
    highScoreFieldTextObject.AddComponent<Shadow>().effectColor = Color.black;


    highScoreTextObject = new GameObject("highScoreText");
    highScoreTextObject.AddComponent<CanvasRenderer>();
    highScoreText = highScoreTextObject.AddComponent<Text>();
    highScoreText.rectTransform.anchoredPosition = new Vector2(0,0);
    highScoreText.rectTransform.sizeDelta = new Vector2(225,500);
    highScoreText.transform.SetParent(canvasObject.transform, false);
    highScoreText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
    highScoreText.fontSize = 30;
    highScoreText.color = Color.white;
    highScoreText.fontStyle = FontStyle.Bold;
    highScoreText.alignment = TextAnchor.UpperLeft;
    highScoreTextObject.AddComponent<Shadow>().effectColor = Color.black;
    highScoreTextObject.SetActive(false);

    inputFieldShouldBeActive = false;
    canvasObject.SetActive(true);

  }

  public void ShowHighScoreScreen(int score, int speed, bool allowScoreSubmission) {
    canvasObject.SetActive(true);
    if (allowScoreSubmission) {
      endScore = score;
      endSpeed = speed;
      highScoreField.SetActive(true);
      highScoreFieldText.gameObject.SetActive(true);
      highScoreTextObject.SetActive(false);
      inputFieldShouldBeActive = true;
    } else {
      highScoreField.SetActive(false);
      highScoreFieldText.gameObject.SetActive(false);
      highScoreTextObject.SetActive(true);
    }
  }

  public void HideHighScoreScreen() {
    canvasObject.SetActive(false);
  }

  // curl -H "Content-Type: application/json" -X POST -d '{"user":{"email":"bradyfukumoto@gmail.com","password":"[PASSWORD]"}}' https://www.bradyfukumoto.com/api/v1/sessions
  IEnumerator GetHighScores() {
    // We should only read the screen after all rendering is complete
    yield return new WaitForEndOfFrame();

    // Upload to a cgi script
    WWW w = new WWW(hostname + "/api/v1/block_game_high_scores?version=" + version);
    yield return w;
    if (!string.IsNullOrEmpty(w.error)) {
      Debug.Log("ERROR IN GET HIGH SCORES");
    }
    else {
      high_scores = JSON.Parse(w.text);
      updateHighScoreText(high_scores["high_scores"].AsArray);
    }
  }

  // curl -H "Content-Type: application/json" -X POST -d '{"high_score":{"user_id":"1","game_id":"1","score":"100"}}' https://www.bradyfukumoto.com/api/v1/high_scores
  IEnumerator SubmitHighScore() {
    // We should only read the screen after all rendering is complete
    yield return new WaitForEndOfFrame();

    // Create a Web Form
    WWWForm form = new WWWForm();
    form.AddField("high_score[name]", playerName);
    form.AddField("high_score[score]", endScore);
    form.AddField("high_score[speed]", endSpeed);
    form.AddField("high_score[version]", version);
    form.AddField("score_hash", nameScoreHash(playerName, endScore, endSpeed));

    // Upload to a cgi script
    WWW w = new WWW(hostname + "/api/v1/block_game_high_scores", form);
    yield return w;
    if (!string.IsNullOrEmpty(w.error)) {
      Debug.Log("ERROR SUBMITTING SCORE");
    }
    else {
      JSONNode high_scores = JSON.Parse(w.text);
      updateHighScoreText(high_scores["high_scores"].AsArray);
    }
  }

  private void updateHighScoreText(JSONArray highScoreJSONArray) {
    string scoreString = "HIGH SCORES:\n\n";
    for (int i = 0 ; i < highScoreJSONArray.Count && i <= 5 ; i++) {
      scoreString += (i + 1) + ". " + highScoreJSONArray[i]["name"] + "\n    ";
      scoreString += highScoreJSONArray[i]["score"] + " (" + highScoreJSONArray[i]["speed"] + ")\n\n";
    }
    highScoreText.text = scoreString;
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

  void Update() {
    if (inputFieldShouldBeActive && (Input.GetKeyDown( KeyCode.Return ) || Input.GetKeyDown( KeyCode.KeypadEnter )) ) {
      if (highScoreFieldText.text.Trim().Length > 0) {
        playerName = highScoreFieldText.text.Trim();
        StartCoroutine(SubmitHighScore());
      }
      inputFieldShouldBeActive = false;
      highScoreField.SetActive(false);
      highScoreFieldText.gameObject.SetActive(false);
      highScoreTextObject.SetActive(true);

      manager.DoneWithHighScores();
      manager.flashPermanentAlert("Press ENTER to begin");

    }
    if (inputFieldShouldBeActive && !inputField.isFocused) {
      inputField.ActivateInputField();
    }
  }


}