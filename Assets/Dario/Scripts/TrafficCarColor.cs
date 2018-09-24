using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficCarColor : MonoBehaviour {

    private Color32[] colorArray = { new Color32(0, 8, 156, 255), new Color32(37, 37, 37, 255), new Color32(34, 180, 92, 255), new Color32(255, 255, 255, 255), new Color32(135, 28, 28, 255) };

    private void Awake()
    {
        Color32 carColor = colorArray[Random.Range(0, colorArray.Length)];
        Material mat = transform.GetComponent<Renderer>().material;
        mat.color = carColor;
    }
}
