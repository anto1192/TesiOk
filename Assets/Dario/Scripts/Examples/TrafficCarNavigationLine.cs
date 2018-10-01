using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficCarNavigationLine : MonoBehaviour {

    private LinesUtilsAlt linesUtilsAlt = new LinesUtilsAlt();
    private GameObject pathEnhanced = null;
    
    private CurvySpline curvySpline = null;
    private CurvySplineSegment curSegment = null;
    private List<CurvySplineSegment> segmentsToSearch = new List<CurvySplineSegment>();

    void Start()
    {
        linesUtilsAlt.LineRend = linesUtilsAlt.CreateLineRenderer(ref linesUtilsAlt.lineRendererEmpty, "TrafLineRenderer", 1.0f, new Color32(0x4C, 0x8B, 0xDD, 0xFF), ResourceHandler.instance.mats[2]);
        linesUtilsAlt.lineRendererEmpty.transform.SetParent(transform);
        linesUtilsAlt.lineRendererEmpty.transform.localPosition = Vector3.zero;
        pathEnhanced = GameObject.Find("SplinePCH(Clone)");
        
        StartCoroutine(InitNavigationLine());
        StartCoroutine(NavigationLine(0.25f)); 
    }

    IEnumerator InitNavigationLine()
    {
        curvySpline = pathEnhanced.GetComponent<CurvySpline>();
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
            
                //CurvySpline curvySpline = pathEnhanced.GetComponent<CurvySpline>();
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
            
            //else if (linesUtilsAlt.LineRend.positionCount != 0)
            //    linesUtilsAlt.LineRend.positionCount = 0; //delete the LineRenderer
        }
    }

    

   
}
