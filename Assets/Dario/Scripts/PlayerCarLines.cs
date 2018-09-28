using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCarLines : MonoBehaviour {

    private GameObject pathEnhanced = null;
    private GameObject centerLine = null;
    private LinesUtilsAlt linesUtilsAlt = new LinesUtilsAlt();
    public enum Lane { RIGHT, OPPOSITE }
    public enum LaneState { RED, YELLOW, GREEN } 
    public LaneState laneState = LaneState.GREEN; //this is for the symbol on the dashboard
    private CurvySplineSegment curSegment = null;
    private List<CurvySplineSegment> segmentsToSearch = new List<CurvySplineSegment>();
    private Transform rayCastPos;

    void Start () {

        LoadCarHierarchy();

        CreatePathEnhanced();
        CreateCenterLine();
        linesUtilsAlt.LineRend = linesUtilsAlt.CreateLineRenderer(ref linesUtilsAlt.lineRendererEmpty, "LineRenderer", 2.5f, new Color32(0x3A, 0xC1, 0xAA, 0xFF), ResourceHandler.instance.mats[2]);
        linesUtilsAlt.LineRend2 = linesUtilsAlt.CreateLineRenderer(ref linesUtilsAlt.centerLineRendererEmpty, "centerLineRenderer", 0.5f, new Color32(0xFF, 0xFF, 0xFF, 0xFF), ResourceHandler.instance.mats[2]);
        linesUtilsAlt.CenterLineLerper(Color.red, Color.yellow, Color.green);
        StartCoroutine(InitNavigationLine());
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
        if (Vector3.Dot(gameObject.transform.TransformDirection(Vector3.forward), Point) < 0)
            return Lane.OPPOSITE;
        else
            return Lane.RIGHT;
    }

    IEnumerator InitNavigationLine()
    {
        CurvySpline curvySpline = pathEnhanced.GetComponent<CurvySpline>();
        while (!curvySpline.IsInitialized)
            yield return null;
        float carTf = curvySpline.GetNearestPointTF(transform.position);
        curSegment = curvySpline.TFToSegment(carTf);
    }

    IEnumerator NavigationLine(float waitTime)
    {
        while (true)
        {
            yield return new WaitForSeconds(waitTime);
            CurvySpline curvySpline = pathEnhanced.GetComponent<CurvySpline>();
            segmentsToSearch.Add(curSegment);
            segmentsToSearch.Add(curvySpline[curSegment.SegmentIndex + 1].GetComponent<CurvySplineSegment>());
            segmentsToSearch.Add(curvySpline[curSegment.SegmentIndex - 1].GetComponent<CurvySplineSegment>());

            //float carTf = curvySpline.GetNearestPointTFExt(transform.position, segmentsToSearch.ToArray());
            float carTf = curvySpline.GetNearestPointTF(transform.position);

            Lane lane = MonitorLane(curvySpline.GetTangentFast(carTf));
            if (lane == Lane.RIGHT)
                linesUtilsAlt.DrawLine(gameObject, curvySpline, carTf);
            else
                linesUtilsAlt.LineRend.positionCount = 0; //delete the LineRenderer

            curSegment = curvySpline.TFToSegment(carTf);
            segmentsToSearch.Clear();

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
                    float carTf = curvySpline.GetNearestPointTF(transform.position); //determine the nearest t of the car with respect to the centerLine
                    float carTf2 = pathEnhancedCurvySpline.GetNearestPointTF(transform.position);
                    float dist = Vector3.Distance(transform.position, curvySpline.Interpolate(carTf));
                    Lane lane = MonitorLane(pathEnhancedCurvySpline.GetTangentFast(carTf2)); //this is to understand if I am partially in the oncoming lane
                    if (dist < 4.0f && lane == Lane.RIGHT)
                    {
                        linesUtilsAlt.CenterLineColor = linesUtilsAlt.ChangeMatByDistance(dist / 4.0f);
                        SetDashBoardColor(dist, 4.0f);
                    }
                        
                    else if (dist >= 4.0f && lane == Lane.RIGHT)
                    {
                        linesUtilsAlt.CenterLineColor = new Color32(0x00, 0xFF, 0x00, 0x00);
                        laneState = LaneState.GREEN;
                    }
                        
                    else if (lane == Lane.OPPOSITE)
                    {
                        linesUtilsAlt.CenterLineColor = new Color32(0xFF, 0x00, 0x00, 0xFF);
                        laneState = LaneState.RED;
                    }
                        
                    linesUtilsAlt.DrawCenterLine(carTf, MonitorLane(curvySpline.GetTangentFast(carTf)), curvySpline, ResourceHandler.instance.sprites[33].texture);
                }
            }
            else
                laneState = LaneState.GREEN;
        }
    }

    void CreatePathEnhanced()
    {
        pathEnhanced = Instantiate(ResourceHandler.instance.prefabs[13]);
    }

    void CreateCenterLine()
    {
        centerLine = Instantiate(ResourceHandler.instance.prefabs[11]);
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
