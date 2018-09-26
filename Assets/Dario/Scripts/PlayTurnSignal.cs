using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayTurnSignal : MonoBehaviour {

    private bool hasPlayedON = false;
    private bool hasPlayedOFF = false;

    private AudioSource turnAudioSource;

   
    void Start ()
    {
        turnAudioSource = transform.GetComponent<AudioSource>();
    }
	
	
	void Update ()
    {
        
    }

   void PlayONAudio()
    {
        if (!hasPlayedON)
        {
            turnAudioSource.PlayOneShot(ResourceHandler.instance.audioClips[8]);
            hasPlayedON = true;
        }
    }

    void PlayLoopAudio()
    {
        turnAudioSource.PlayOneShot(ResourceHandler.instance.audioClips[6]);
    }
}
