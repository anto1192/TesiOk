using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SFNodeInfo : MonoBehaviour {

    public int identifier;
    public int subIdentifier;
    public List<Vector3> waypoints = new List<Vector3>();

    public Vector3[] GetPoints()
    {
        return waypoints.ToArray();
        //return road.waypoints.Select(wp => wp.position).ToArray();
    }
}

