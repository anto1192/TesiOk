using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayTurnSignal : MonoBehaviour {

    private DashBoardControllerUrban dashBoardUrban;
    private AudioSource turnAudioSource;

    void Start ()
    {
        turnAudioSource = transform.GetComponent<AudioSource>();
    }

    void PlayLoopAudio()
    {
        turnAudioSource.PlayOneShot(ResourceHandler.instance.audioClips[5]);
    }
}
