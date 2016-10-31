using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioScript : MonoBehaviour {

  private AudioSource[] audioSources;

  private int layersActive = 1;

  private float playTime = 0;
  private float clipLength = 24;
  private bool isPlaying = false;

  public bool shouldPlayAudio = true;

  private Dictionary<string, AudioClip> blockClips;
  private AudioSource blockSoundSource;
  private AudioSource blockFallSoundSource;

  void Awake() {
    gameObject.name = "AudioManager";
    AudioClip audio0 = Resources.Load<AudioClip>("Audio/KeyboardHarmony");
    AudioClip audio1 = Resources.Load<AudioClip>("Audio/KeyboardMelody");

    blockClips = new Dictionary<string, AudioClip>();
    blockClips["DROP"] = Resources.Load<AudioClip>("Audio/snow2");
    blockClips["FALL"] = Resources.Load<AudioClip>("Audio/Swoosh");
    blockClips["BREAK"] = Resources.Load<AudioClip>("Audio/Break1");
    blockClips["BREAK2"] = Resources.Load<AudioClip>("Audio/Break2");
    blockClips["DIAMOND"] = Resources.Load<AudioClip>("Audio/DiamondShatter");

    blockSoundSource = createAudioObject(blockClips["DROP"]);
    blockFallSoundSource = createAudioObject(blockClips["FALL"]);

    audioSources = new AudioSource[2];
    audioSources[0] = createAudioObject(audio0);
    audioSources[1] = createAudioObject(audio1);

    transform.position = Camera.main.transform.position;
    transform.parent = Camera.main.transform;
  }

  public void PlayBlockSound(string soundKey) {
    if (!shouldPlayAudio) return;

    if (soundKey == "FALL") {
      blockFallSoundSource.Play();
      return;
    } else if (soundKey == "BREAK" && Random.value > 0.5) {
      soundKey = "BREAK2";
    }
    blockSoundSource.clip = blockClips[soundKey];
    blockSoundSource.Play();
  }

  public void PlayAudio() {
    if (!shouldPlayAudio) return;
    // for (int i = 0 ; i < audioSources.Length ; i++) {
    //   if (layersActive - 1 >= i) {
    //     audioSources[i].Play();
    //     playTime = 0;
    //     isPlaying = true;
    //   }
    // }
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