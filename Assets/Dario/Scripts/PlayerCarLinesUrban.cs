using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCarLinesUrban : MonoBehaviour {

    private Material[] mats;
    private GameObject[] prefabs;

    private GameObject pathEnhanced = null;
    private GameObject centerLine = null;
    private LinesUtilsAlt linesUtilsAlt = new LinesUtilsAlt();
    public enum Lane { RIGHT, OPPOSITE }

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

    Lane MonitorLane(Vector3 Point)
    {
        if (Vector3.Dot(gameObject.transform.TransformDirection(Vector3.forward), Point) < 0)
            return Lane.OPPOSITE;
        else
            return Lane.RIGHT;
    }

    void CreatePathEnhanced()
    {
        pathEnhanced = Instantiate(prefabs[11]);
    }

    void CreateCenterLine()
    {
        centerLine = Instantiate(prefabs[9]);
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
                    float carTf2 = pathEnhancedCurvySpline.GetNearestPointTF(transform.position);
                    float dist = Vector3.Distance(transform.position, curvySpline.Interpolate(carTf));
                    float dist2 = Vector3.Distance(transform.position, pathEnhancedCurvySpline.Interpolate(carTf2));
                    if (dist < 4.0f && dist2 < 2.7f)
                        linesUtilsAlt.CenterLineColor = linesUtilsAlt.ChangeMatByDistance(dist);
                    else if (dist >= 4.0f && dist2 < 2.7f)
                        linesUtilsAlt.CenterLineColor = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
                    else if (dist2 >= 2.7f)
                        linesUtilsAlt.CenterLineColor = new Color32(0xFF, 0x00, 0x00, 0xFF);
                    linesUtilsAlt.DrawCenterLine(gameObject, carTf, curvySpline);
                }
            }
        }
    }
}
