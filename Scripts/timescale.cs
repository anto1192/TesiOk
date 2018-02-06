using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class timescale : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
            Time.timeScale += 0.1f;

        if (Input.GetKeyDown(KeyCode.KeypadMinus))
            Time.timeScale -= 0.1f;
        if (Time.timeScale < 0)
            Time.timeScale = 0;
    }
}
