using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficCarNavigationLine : MonoBehaviour {

    private LinesUtilsAlt linesUtilsAlt = new LinesUtilsAlt();
    private GameObject pathEnhanced = null;
    private VehicleController vc;
    private Material mat;

    private CurvySplineSegment curSegment = null;
    private List<CurvySplineSegment> segmentsToSearch = new List<CurvySplineSegment>();
    private const int DETECTION_DIST = 22500; //150m

    private void Awake() {
        LoadMaterial();
    }

    void Start()
    {
        linesUtilsAlt.LineRend = linesUtilsAlt.CreateLineRenderer(linesUtilsAlt.LineRendererEmpty, "TrafLineRenderer", 1.0f, new Color32(0x4C, 0x8B, 0xDD, 0xFF), mat, () => linesUtilsAlt.InitGlowTexture());
        pathEnhanced = GameObject.Find("SplinePCH(Clone)");
        vc = FindObjectOfType(typeof(VehicleController)) as VehicleController;
        StartCoroutine(InitNavigationLine());
        StartCoroutine(NavigationLine(0.25f)); 
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
            Vector3 offset = vc.gameObject.transform.position - transform.position;
            float sqrLen = offset.sqrMagnitude;
            if (sqrLen <= DETECTION_DIST) 
            {
                CurvySpline curvySpline = pathEnhanced.GetComponent<CurvySpline>();
                segmentsToSearch.Add(curSegment);
                if (curSegment.SegmentIndex != curvySpline.Segments.Count - 1 && curSegment.SegmentIndex != 0)
                {
                    segmentsToSearch.Add(curvySpline[curSegment.SegmentIndex + 1].GetComponent<CurvySplineSegment>());
                    segmentsToSearch.Add(curvySpline[curSegment.SegmentIndex - 1].GetComponent<CurvySplineSegment>());
                } else if (curSegment.SegmentIndex == curvySpline.Segments.Count - 1)
                    segmentsToSearch.Add(curvySpline[0].GetComponent<CurvySplineSegment>());
                else if (curSegment.SegmentIndex == 0)
                    segmentsToSearch.Add(curvySpline[1].GetComponent<CurvySplineSegment>());

                float carTf = curvySpline.GetNearestPointTFExt(transform.position, segmentsToSearch.ToArray());
                linesUtilsAlt.DrawLine(gameObject, curvySpline, carTf);
                curSegment = curvySpline.TFToSegment(carTf);
                segmentsToSearch.Clear();
            }
            else if (linesUtilsAlt.LineRend.positionCount != 0)
                linesUtilsAlt.LineRend.positionCount = 0; //delete the LineRenderer
        }
    }

    void LoadMaterial()
    {

        try
        {
            mat = Resources.Load<Material>("Materials/LineRendererGlow");
        }
        catch (Exception e)
        {
            Debug.Log("Loading material failed with the following exception: ");
            Debug.Log(e);
        }

    }
}
