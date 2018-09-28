using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class CubesAndTags
{
    public List<GameObject> boundingCube;
    public List<GameObject> infoTag;
    public List<Bounds> bounds;
    public bool alreadyEvaluated; //bool to establish if the object has been evaluated or not
    public bool alreadyPlayed; //emergency brake sound
    public Collider other; 
    public float prevState; //previousState of danger
    //public float timer;
    public Vector3 obstaclePrevPos; //this is to store the initial position of the obstacle in order to compute its speed
    public float obstaclePrevSpeed; //this is to return a speed value when the timer has not passed, but I have to return a value 
    public float obstaclePrevTime; //this is to compute the speed with some delay, otherwise the value printed on screen flickers if computed based on Update time
    public float obstacleNextTime; //this is to compute the speed with some delay, otherwise the value printed on screen flickers if computed based on Update time

    public enum Gradient { OFF, ON }
    public Gradient gradient; //this is to establish dashboard alert color 

    public enum DangerState { NONE, YELLOW, RED } 
    public DangerState dangerState; //this is to establish dashboard alert color

    public CubesAndTags()
    {
        this.boundingCube = new List<GameObject>();
        this.infoTag = new List<GameObject>();
        this.bounds = new List<Bounds>();
        this.dangerState = DangerState.NONE;
        this.gradient = Gradient.OFF;
        this.alreadyEvaluated = false;
        this.alreadyPlayed = false;
        this.prevState = 1.0f;
        this.obstaclePrevPos = Vector3.zero;
    }

    public void AddElements(GameObject boundingCube, GameObject infoTag, Bounds bounds)
    {
        this.boundingCube.Add(boundingCube);
        this.infoTag.Add(infoTag);
        this.bounds.Add(bounds);
    }

    public void DestroyCubesAndTags()
    {
        foreach (GameObject go in this.boundingCube)
            UnityEngine.MonoBehaviour.Destroy(go);
        foreach (GameObject go in this.infoTag)
            UnityEngine.MonoBehaviour.Destroy(go);
    }

    public void DisableCubesAndTags()
    {
        foreach (GameObject go in this.boundingCube)
            go.GetComponent<Renderer>().enabled = false;
        foreach (GameObject go in this.infoTag)
            UnityEngine.MonoBehaviour.Destroy(go);
    }
}
