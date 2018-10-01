using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCarLinesUrban : MonoBehaviour {

    private GameObject pathEnhanced = null;
    private GameObject centerLine = null;
    private LinesUtilsAlt linesUtilsAlt = new LinesUtilsAlt();
    public enum Lane { RIGHT, OPPOSITE }
    public enum LaneState { RED, YELLOW, GREEN }
    public LaneState laneState = LaneState.GREEN; //this is for the symbol on the dashboard

    private Transform rayCastPos;
   

    void Start () {

        LoadCarHierarchy();

        CreatePathEnhanced();
        CreateCenterLine();
        linesUtilsAlt.LineRend = linesUtilsAlt.CreateLineRenderer(ref linesUtilsAlt.lineRendererEmpty, "LineRenderer", 2.5f, new Color32(0x3A, 0xC1, 0xAA, 0xFF), ResourceHandler.instance.mats[2]);
        linesUtilsAlt.LineRend2 = linesUtilsAlt.CreateLineRenderer(ref linesUtilsAlt.centerLineRendererEmpty, "centerLineRenderer", 0.5f, new Color32(0xFF, 0xFF, 0xFF, 0xFF), ResourceHandler.instance.mats[2]);
        linesUtilsAlt.CenterLineLerper(Color.red, Color.yellow, Color.green);
        StartCoroutine(NavigationLine(0.25f));
        StartCoroutine(LaneKeeping(0.25f));
    }
	
    void LoadCarHierarchy()
    {
        foreach (Transform t in transform)
            if (t.name.Equals("rayCastPos"))
            {
                rayCastPos = t;
                break;
            }
    }

    Lane MonitorLane(Vector3 Point)
    {
        Vector3 heading = transform.position - Point; //Vector that goes from pathPoint to the center of the PlayerCar
        if (Vector3.Dot(heading, transform.right) < 0)
            return Lane.OPPOSITE;
        else
            return Lane.RIGHT;
    }

    void CreatePathEnhanced()
    {
        pathEnhanced = Instantiate(ResourceHandler.instance.prefabs[14]);
    }

    void CreateCenterLine()
    {
        centerLine = Instantiate(ResourceHandler.instance.prefabs[12]);
    }

    IEnumerator NavigationLine(float waitTime)
    {
        while (true)
        {
            yield return new WaitForSeconds(waitTime);
            linesUtilsAlt.DrawLine(transform.gameObject);
        }
    }

    IEnumerator LaneKeeping(float waitTime)
    {
        while (true)
        {
            yield return new WaitForSeconds(waitTime);

            RaycastHit hit;
            CurvySpline curvySpline = null;
            CurvySpline pathEnhancedCurvySpline = null;
            if (Physics.Raycast(rayCastPos.position, rayCastPos.TransformDirection(-Vector3.up), out hit, 10.0f, 1 << LayerMask.NameToLayer("Graphics")))
            {
                curvySpline = hit.collider.gameObject.GetComponent<CurvySpline>();
                pathEnhancedCurvySpline = pathEnhanced.GetComponent<CurvySpline>();
                if (curvySpline.IsInitialized && pathEnhancedCurvySpline.IsInitialized)
                {
                    //Debug.Log("meshcollider found is: " + hit.collider);
                    float carTf = curvySpline.GetNearestPointTF(transform.position); //determine the nearest t of the car with respect to the centerLine
                    Vector3 centerLinePoint = curvySpline.Interpolate(carTf);
                    float dist = Vector3.Distance(transform.position, centerLinePoint);
                    Lane lane = MonitorLane(centerLinePoint); //this is to understand if I am partially in the oncoming lane

                    if (dist < 2.0f && lane == Lane.RIGHT) //if dist2 < 2.7f I am in the right lane
                    {
                        linesUtilsAlt.CenterLineColor = linesUtilsAlt.ChangeMatByDistance(dist / 2.0f);
                        SetDashBoardColor(dist, 2.0f);
                    }
                        
                    else if (dist >= 2.0f && lane == Lane.RIGHT)
                    {
                        linesUtilsAlt.CenterLineColor = new Color32(0x00, 0xFF, 0x00, 0x00);
                        laneState = LaneState.GREEN;
                    }
                        
                    else if (lane == Lane.OPPOSITE) // this is to draw the redLine only when the PlayerCar is moving to the left, if it is moving to the right the line doesn't have to be red!
                    {
                        linesUtilsAlt.CenterLineColor = new Color32(0xFF, 0x00, 0x00, 0xFF);
                        laneState = LaneState.RED;
                    }
                        
                    linesUtilsAlt.DrawCenterLine(gameObject, carTf, curvySpline, ResourceHandler.instance.sprites[36].texture);
                }
            }
            else
                laneState = LaneState.GREEN;
        }
    }

    void SetDashBoardColor(float dist, float normFactor)
    {
        float value = Mathf.Clamp(dist / normFactor, 0.0f, 1.0f);
        if (value <= 0.4f)
            laneState = LaneState.RED;
        else if (value > 0.4f && value <= 0.8f)
            laneState = LaneState.YELLOW;
        else
            laneState = LaneState.GREEN;
    }
}
