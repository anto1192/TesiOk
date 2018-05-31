using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoPathHelper : MonoBehaviour
{

    private CarAutoPath path;


    //private void OnDrawGizmos()
    //{
    //    bool ptset = false;
    //    Vector3 lastpt = Vector3.zero;
    //    for (int i = 0; i < gameObject.transform.childCount; i++)
    //    {
    //        Vector3 wayPoint = gameObject.transform.GetChild(i).transform.position + new Vector3(0, 0.2f, 0);
    //        if (ptset)
    //        {
    //            Gizmos.color = Color.blue;
    //            Gizmos.DrawLine(lastpt, wayPoint);
    //        }
    //        lastpt = wayPoint;
    //        ptset = true;
    //    }

    //}



    void Start()
    {
        CreateAutoPathEnhanced();
    }

    void CreateAutoPathEnhanced()
    {
        path = FindObjectOfType(typeof(CarAutoPath)) as CarAutoPath;
        int pathIndex = 0;
        foreach (var p in path.pathNodes)
        {
            GameObject go = new GameObject("Node" + pathIndex);

            Vector3 currentSpot = p.position;
            RaycastHit hit;
            Physics.Raycast(currentSpot + Vector3.up * 5, -Vector3.up, out hit, 100f, (1 << LayerMask.NameToLayer("Roads")));
            go.transform.position = new Vector3(currentSpot.x, hit.point.y + 0.2f, currentSpot.z);

            if (pathIndex < path.pathNodes.Count - 1)
                go.transform.LookAt(path.pathNodes[pathIndex + 1].position); //this is in order to orient the pathNodes to the road direction
            else
                go.transform.LookAt(path.pathNodes[0].position); //the last node takes the orientation of the first since it doesn't have a node in front of it

            go.transform.SetParent(gameObject.transform);
            go.AddComponent<SphereCollider>();
            //go.GetComponent<SphereCollider>().isTrigger = true;
            go.layer = LayerMask.NameToLayer("Graphics");
            go.tag = "Waypoint";
            pathIndex++;
        }
    }
}
	
	

