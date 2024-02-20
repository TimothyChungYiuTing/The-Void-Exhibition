using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
	public AudioSource audioSource1;
	public AudioSource audioSource2;
    //public AudioManager2 otherAudioManager;
	public AudioClip[] songClips;
    public bool isPlay1;
	public float volume;
	[SerializeField] public float multipliedVolume1 = 1f;
	[SerializeField] public float multipliedVolume2 = 0f;
	public int currentSongIndex = 0;
    private float trackTimer1 = 0f;
    private float trackTimer2 = 0f;

    new void Awake() {
        base.Awake();
        multipliedVolume1 = 1f;
        multipliedVolume2 = 0f;
		//audioSource1 = GetComponent<AudioSource>();
		//audioSource2 = GetComponent<AudioSource>();
        /*
        if (songClips.Length != 0) {
		    audioSource1.clip = songClips[0];
            audioSource1.loop = true;
		    audioSource1.Play();
        }
        */
    }

    void Update() {
        audioSource1.volume = volume * multipliedVolume1;
        audioSource2.volume = volume * multipliedVolume2;

        if (audioSource1.isPlaying) {
            trackTimer1 += Time.deltaTime;
        }
        if (audioSource2.isPlaying) {
            trackTimer2 += Time.deltaTime;
        }
    }

    /*
	public void NextSong() {
        trackTimer1 = 0;
        trackTimer2 = 0;
		currentSongIndex++;
		if (currentSongIndex == songClips.Length) {
			currentSongIndex = 0;
		}

		audioSource1.clip = songClips[currentSongIndex];
		audioSource1.Play();
	}
    */

	public void ChangeSong(int songIndex) {
        if (currentSongIndex != songIndex) {
            currentSongIndex = songIndex;
            if (isPlay1) {
                FadeOut(1);
                isPlay1 = false;
                
                trackTimer2 = 0;

                audioSource2.clip = songClips[currentSongIndex];
                FadeIn(2);
                audioSource2.Play();
            }
            else {
                FadeOut(2);
                isPlay1 = true;

                trackTimer1 = 0;

                audioSource1.clip = songClips[currentSongIndex];
                FadeIn(1);
                audioSource1.Play();
            }
        }
	}

	public void Stop() {
		audioSource1.Stop();
	}

    public void FadeIn(int sourceID) {
        StartCoroutine(FadeEffect(true, sourceID));
    }

    public void FadeOut(int sourceID) {
        StartCoroutine(FadeEffect(false, sourceID));
    }

    public IEnumerator FadeEffect(bool fadeIn, int sourceID) {
        //Fade out then Fade in again
        float startTime = Time.time;
        while (Time.time-startTime < 5f) {      //Done in 5 seconds
            if (fadeIn) {
                if (sourceID == 1)
                    multipliedVolume1 = 1 - (1f + Mathf.Cos(Mathf.PI * (Time.time-startTime) / 5f)) / 2f;
                else
                    multipliedVolume2 = 1 - (1f + Mathf.Cos(Mathf.PI * (Time.time-startTime) / 5f)) / 2f;
            }
            else {
                if (sourceID == 1)
                    multipliedVolume1 = (1f + Mathf.Cos(Mathf.PI * (Time.time-startTime) / 5f)) / 2f;
                else
                    multipliedVolume2 = (1f + Mathf.Cos(Mathf.PI * (Time.time-startTime) / 5f)) / 2f;
            }
            yield return null;
        }
        if (fadeIn) {
            if (sourceID == 1)
                multipliedVolume1 = 1f;
            else
                multipliedVolume2 = 1f;
        }
        else {
            if (sourceID == 1)
                multipliedVolume1 = 0f;
            else
                multipliedVolume2 = 0f;
        }
    }
}