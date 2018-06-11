using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawLine : MonoBehaviour {

    private CarAutoPath path;
    private LineRenderer linerenderer;
    private int NextIndex;
    private GameObject car;
    private List<Vector3> LinePoints = new List<Vector3>();
  
    



    private void OnEnable()
    {
        //CarExternalInputAutoPathAdvanced.OnIndexUpdated += HandleOnOnIndexUpdated;
        
    }

    private void OnDisable()
    {
        //CarExternalInputAutoPathAdvanced.OnIndexUpdated -= HandleOnOnIndexUpdated;
        
    }

    void HandleOnOnIndexUpdated(int index) {

        NextIndex = index;
    }



    void Start () {

        
        path = GameObject.Find("AutoPath").GetComponent<CarAutoPath>();

        //Points = new Vector3[path.pathNodes.Count];
        //int i = 0;
        //foreach (var p in path.pathNodes)
        //{
        //    Vector3 currentSpot = p.position;// HermiteMath.HermiteVal(prevWaypoint, currentWaypoint, prevTangent, currentTangent, currentPc);
        //    RaycastHit hit;
        //    Physics.Raycast(currentSpot + Vector3.up * 5, -Vector3.up, out hit, 100f, ~(1 << LayerMask.NameToLayer("Traffic")));
        //    Vector3 hTarget = new Vector3(currentSpot.x, hit.point.y + 0.2f, currentSpot.z);
        //    Points[i] = hTarget;
        //    ++i;
        //}

        

        linerenderer = gameObject.GetComponent<LineRenderer>();
        linerenderer.alignment = LineAlignment.Local;
        //linerenderer.positionCount = Points.Length;

        //Gradient gradient = new Gradient();
        //gradient.SetKeys(
        //    new GradientColorKey[] { new GradientColorKey(Color.green, 1.0f), new GradientColorKey(Color.green, 1.0f) },
        //    new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 1.0f), new GradientAlphaKey(1.0f, 1.0f) }
        //    );
        //linerenderer.colorGradient = gradient;

        int ii = 0;
        //foreach (var p in Points)
        //{
        //    linerenderer.SetPosition(ii, p);
        //    ++ii;

        //}



    }

    private void Update()
    {

        //Vector3 pos = car.transform.position + new Vector3(0, 0.2f, 0);
        //LinePoints.Add(normalPoint + new Vector3(0, 0.2f, 0));
        LinePoints.Add(car.transform.position + new Vector3(0, 0.2f, 0));
        for (int i = 0; i < 4; i++) {
            LinePoints.Add(path.pathNodes[NextIndex + i].position + new Vector3(0, 0.2f, 0)); 
        }
        linerenderer.positionCount = LinePoints.Count;
        linerenderer.SetPositions(LinePoints.ToArray());
        LinePoints.Clear();


    }



}

