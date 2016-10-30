using UnityEngine;
using System.Collections;

public class AudioScript : MonoBehaviour {

  private AudioSource[] audioSources;

  private int layersActive = 1;

  private float playTime = 0;
  private float clipLength = 24;
  private bool isPlaying = false;

  public bool shouldPlayAudio = true;

  void Awake() {
    gameObject.name = "AudioManager";
    AudioClip audio0 = Resources.Load<AudioClip>("Audio/KeyboardHarmony");
    AudioClip audio1 = Resources.Load<AudioClip>("Audio/KeyboardMelody");

    audioSources = new AudioSource[2];
    audioSources[0] = createAudioObject(audio0);
    audioSources[1] = createAudioObject(audio1);

    transform.position = Camera.main.transform.position;
    transform.parent = Camera.main.transform;
  }

  public void PlayAudio() {
    if (!shouldPlayAudio) return;
    for (int i = 0 ; i < audioSources.Length ; i++) {
      if (layersActive - 1 >= i) {
        audioSources[i].Play();
        playTime = 0;
        isPlaying = true;
      }
    }
  }

  public void StopAudio() {
    for (int i = 0 ; i < audioSources.Length ; i++) {
      audioSources[i].Stop();
      isPlaying = false;
    }
  }

  private AudioSource createAudioObject(AudioClip clipToPlay) {
    GameObject audioSourceObject = new GameObject();
    audioSourceObject.transform.position = transform.position;
    audioSourceObject.transform.parent = transform;

    AudioSource audioSource = audioSourceObject.AddComponent<AudioSource>();
    audioSource.clip = clipToPlay;

    return audioSource;
  }

  void Update() {
    if (isPlaying) {
      playTime += Time.deltaTime;
      if (playTime >= clipLength) {
        layersActive++;
        StopAudio();
        PlayAudio();
      }
    }
  }
}