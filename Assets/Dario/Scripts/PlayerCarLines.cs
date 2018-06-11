using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCarLines : MonoBehaviour {

    private Material[] mats;
    private Sprite[] sprites;
    private GameObject[] prefabs;

    private GameObject pathEnhanced = null;
    private GameObject centerLine = null;
    private LinesUtilsAlt linesUtilsAlt = new LinesUtilsAlt();
    public enum Lane { RIGHT, OPPOSITE }
    private CurvySplineSegment curSegment = null;
    private List<CurvySplineSegment> segmentsToSearch = new List<CurvySplineSegment>();
    private Transform rayCastPos;

    private void Awake()
    {
        LoadMaterials();
        LoadPrefabs();
    }
    
    void Start () {

        LoadCarHierarchy();

        CreatePathEnhanced();
        CreateCenterLine();
        linesUtilsAlt.LineRend = linesUtilsAlt.CreateLineRenderer(ref linesUtilsAlt.lineRendererEmpty, "LineRenderer", 2.5f, new Color32(0x3A, 0xC1, 0xAA, 0xFF), mats[3], () => linesUtilsAlt.InitGlowTexture());
        linesUtilsAlt.LineRend2 = linesUtilsAlt.CreateLineRenderer(ref linesUtilsAlt.centerLineRendererEmpty, "centerLineRenderer", 0.5f, new Color32(0xFF, 0xFF, 0xFF, 0xFF), mats[3], () => linesUtilsAlt.InitGlowTexture2());
        linesUtilsAlt.CenterLineLerper();
        StartCoroutine(InitNavigationLine());
        StartCoroutine(NavigationLine(0.25f));
        StartCoroutine(LaneKeeping(0.25f));
    }

    void LoadSprites()
    {

        try
        {
            Debug.Log("Loading sprites...");
            sprites = Resources.LoadAll<Sprite>("UI/Sprites");
            foreach (var sp in sprites)
            {
                Debug.Log(sp.name);
            }
        }
        catch (Exception e)
        {
            Debug.Log("Loading sprites failed with the following exception: ");
            Debug.Log(e);
        }

    }

    void LoadMaterials()
    {

        try
        {
            Debug.Log("Loading materials...");
            mats = Resources.LoadAll<Material>("Materials");
            foreach (var mat in mats)
            {
                Debug.Log(mat.name);
            }
        }
        catch (Exception e)
        {
            Debug.Log("Loading materials failed with the following exception: ");
            Debug.Log(e);
        }

    }

    void LoadPrefabs()
    {
        try
        {
            Debug.Log("Loading prefabs...");
            prefabs = Resources.LoadAll<GameObject>("Prefabs");
            foreach (var pre in prefabs)
            {
                Debug.Log(pre.name);
            }
        }
        catch (Exception e)
        {
            Debug.Log("Loading prefabs failed with the following exception: ");
            Debug.Log(e);
        }
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
            if (Physics.Raycast(rayCastPos.position, rayCastPos.TransformDirection(-Vector3.up), out hit, 10.0f, 1 << LayerMask.NameToLayer("Graphics")))
            {
                curvySpline = hit.collider.gameObject.GetComponent<CurvySpline>();
                if (curvySpline.IsInitialized)
                {
                    float carTf = curvySpline.GetNearestPointTF(transform.position); //determine the nearest t of the car with respect to the centerLine
                    float carTf2 = pathEnhanced.GetComponent<CurvySpline>().GetNearestPointTF(transform.position);
                    float dist = Vector3.Distance(transform.position, curvySpline.Interpolate(carTf));
                    Lane lane = MonitorLane(pathEnhanced.GetComponent<CurvySpline>().GetTangentFast(carTf2)); //this is to understand if I am partially in the oncoming lane
                    if (dist < 4.0f && lane == Lane.RIGHT)
                        linesUtilsAlt.CenterLineColor = linesUtilsAlt.ChangeMatByDistance(dist);
                    else if (dist >= 4.0f && lane == Lane.RIGHT)
                        linesUtilsAlt.CenterLineColor = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
                    else if (lane == Lane.OPPOSITE)
                        linesUtilsAlt.CenterLineColor = new Color32(0xFF, 0x00, 0x00, 0xFF);
                    linesUtilsAlt.DrawCenterLine(carTf, MonitorLane(curvySpline.GetTangentFast(carTf)), curvySpline);
                }
            }
        }
    }

    void CreatePathEnhanced()
    {
        pathEnhanced = Instantiate(prefabs[10]);
    }

    void CreateCenterLine()
    {
        centerLine = Instantiate(prefabs[8]);
    }


}
