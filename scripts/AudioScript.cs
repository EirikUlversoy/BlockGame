using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioScript : MonoBehaviour {

  private AudioSource[] audioSources;

  private int layersActive = 1;

  private float playTime = 0;
  private float clipLength = 8;
  private int clipsPlayed = 0;
  private bool isPlaying = false;

  private int numDrops = 0;

  public bool shouldPlayAudio = true;

  public int gameSpeed;

  private Dictionary<string, AudioClip> blockClips;
  private AudioSource blockSoundSource;
  private AudioSource blockFallSoundSource;

  private AudioClip bass1;
  private AudioClip bass2;
  private AudioClip bass3;
  private AudioClip bass4;
  private AudioClip bass5;
  private AudioClip bass6;
  private AudioClip melody1;
  private AudioClip melody2;
  private AudioClip bassp;
  private AudioClip basspp;

  void Awake() {
    gameObject.name = "AudioManager";
    bass1 = Resources.Load<AudioClip>("Audio/Bass1");
    bass2 = Resources.Load<AudioClip>("Audio/Bass2");
    bass3 = Resources.Load<AudioClip>("Audio/Bass3");
    bass4 = Resources.Load<AudioClip>("Audio/Bass4");
    bass5 = Resources.Load<AudioClip>("Audio/Bass5");
    bass6 = Resources.Load<AudioClip>("Audio/Bass6");
    melody1 = Resources.Load<AudioClip>("Audio/Melody1");
    melody2 = Resources.Load<AudioClip>("Audio/Melody2");
    bassp = Resources.Load<AudioClip>("Audio/BassUrgency");
    basspp = Resources.Load<AudioClip>("Audio/BassUrgencyPlus");

    blockClips = new Dictionary<string, AudioClip>();
    blockClips["DROP"] = Resources.Load<AudioClip>("Audio/snow2");
    blockClips["FALL"] = Resources.Load<AudioClip>("Audio/Swoosh");
    blockClips["BREAK"] = Resources.Load<AudioClip>("Audio/Break1");
    blockClips["BREAK2"] = Resources.Load<AudioClip>("Audio/Break2");
    blockClips["DIAMOND"] = Resources.Load<AudioClip>("Audio/DiamondShatter");

    blockSoundSource = createAudioObject(blockClips["DROP"]);
    blockFallSoundSource = createAudioObject(blockClips["FALL"]);

    audioSources = new AudioSource[3];
    audioSources[0] = createAudioObject(bass1);
    audioSources[1] = createAudioObject(bassp);
    audioSources[2] = createAudioObject(melody1);

    transform.position = Camera.main.transform.position;
    transform.parent = Camera.main.transform;
  }

  public void RestartAudio() {
    isPlaying = false;
    clipsPlayed = 0;
    playTime = 0;
    StopAudio();
    gameSpeed = 1;
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
    isPlaying = true;
    if (layersActive <= 1) {
      audioSources[0].clip = bass1;
      audioSources[0].Play();
    } else if (layersActive == 2) {
      audioSources[0].clip = bass2;
      audioSources[0].Play();
    } else if (layersActive == 3) {
      audioSources[0].clip = bass3;
      audioSources[0].Play();
    } else if (layersActive == 4) {
      audioSources[0].clip = bass4;
      audioSources[0].Play();
    } else if (layersActive == 5) {
      audioSources[0].clip = bass5;
      audioSources[0].Play();
    } else if (layersActive == 6) {
      audioSources[0].clip = bass5;
      if (clipsPlayed % 4 == 0) {
        audioSources[2].clip = melody1;
        audioSources[2].Play();
      }
      audioSources[0].Play();
    } else if (layersActive == 7) {
      audioSources[0].clip = bass6;
      if (clipsPlayed % 4 == 0) {
        audioSources[2].clip = melody1;
        audioSources[2].Play();
      }
      audioSources[0].Play();
    } else if (layersActive >= 8) {
      audioSources[0].clip = bass6;
      if (clipsPlayed % 4 == 0) {
        audioSources[2].clip = melody2;
        audioSources[2].Play();
      }
      audioSources[0].Play();
    }
    if (gameSpeed >= 30) {
      audioSources[1].clip = basspp;
      audioSources[1].Play();
    } else if (gameSpeed >= 15) {
      audioSources[1].clip = bassp;
      audioSources[1].Play();
    }
    playTime = 0;
  }

  public void StopAudio() {
    for (int i = 0 ; i < audioSources.Length ; i++) {
      audioSources[i].Stop();
      isPlaying = false;
    }
  }

  public void RecordDrop() {
    numDrops++;
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
        clipsPlayed++;
        if (clipsPlayed % 4 == 0) {
          layersActive++;
          Debug.Log("DROPS: " + numDrops);
          numDrops = 0;
        }
        // StopAudio();
        PlayAudio();
      }
    }
  }
}