using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenterLineHelper : MonoBehaviour {


    //private void OnDrawGizmos() {
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
        CreateCenterLine();
    }

    void CreateCenterLine()
    {
        Transform[] kids = gameObject.GetComponentsInChildren<Transform>();
       
        int pathIndex = 1; //offset must be one and not zero otherwise it will be included in the kids list also the parent of the hierarchy, resulting in wrong computations

        foreach (var tr in kids) {
            if (tr.gameObject.transform.parent != null)
            {
                Vector3 currentSpot = tr.position;
                RaycastHit hit;
                Physics.Raycast(currentSpot + Vector3.up * 5, -Vector3.up, out hit, 100f, (1 << LayerMask.NameToLayer("Roads")));
                tr.position = new Vector3(currentSpot.x, hit.point.y + 0.2f, currentSpot.z);

                if (pathIndex < kids.Length - 1)
                {
                    tr.LookAt(kids[pathIndex + 1].position); //this is in order to orient the kids to the road direction
                }
                else
                    tr.LookAt(kids[1].position); //the last node takes the orientation of the first since it doesn't have a node in front of it


                tr.gameObject.AddComponent<SphereCollider>();
                //tr.gameObject.GetComponent<SphereCollider>().isTrigger = true;
                tr.gameObject.layer = LayerMask.NameToLayer("Graphics");
                tr.gameObject.tag = "Centerpoint";
                pathIndex++;
            }
        }
    }
}
