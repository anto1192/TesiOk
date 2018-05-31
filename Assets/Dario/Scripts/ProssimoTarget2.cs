using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProssimoTarget2 : MonoBehaviour {


    public Vector3 target;
    public TrafEntry trafEntry;
    public bool intersection; //if true I have reached the intersection

    public ProssimoTarget2(Vector3 target, bool intersection, TrafEntry trafEntry)
    {
        this.target = target;
        this.intersection = intersection;
        this.trafEntry = trafEntry;
    }
}


