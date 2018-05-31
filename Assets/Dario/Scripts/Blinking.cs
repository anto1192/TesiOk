using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Blinking : MonoBehaviour {

    private Canvas canvas;
    

    private void Start()
    {
        canvas = GetComponent<Canvas>();
    }

    IEnumerator Blink()
    {
        while(true)
        {
            switch(canvas.enabled)
            {
                case false:
                    canvas.enabled = true;
                    yield return new WaitForSeconds(0.5f);
                    break;
                case true:
                    canvas.enabled = false;
                    yield return new WaitForSeconds(0.5f);
                    break;
            }
        }
    }

    public void StartBlinking()
    {
        //StopCoroutine("Blink");
        StartCoroutine("Blink");
    }

    public void StopBlinking()
    {
        StopCoroutine("Blink");
    }

}
