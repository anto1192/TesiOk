using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class lineSFtest : MonoBehaviour {

    
    IEnumerator Start()
    {

        while (!gameObject.GetComponent<CurvySpline>().IsInitialized)
            yield return null;
        //aa();
        ff();
        //VehicleController vc = FindObjectOfType(typeof(VehicleController)) as VehicleController;
        //Renderer[] rend = vc.gameObject.GetComponentsInChildren<Renderer>();
        //foreach (Renderer r in rend)
        //{
        //    foreach (Material mat in r.materials)
        //    {
        //        mat.renderQueue = 3010;

        //    }
        //}
       

    }
    //void OnEnable()
    //{
    //    AutoPathHelperUrban.OnPath += gg;
    //}

    //void OnDisable()
    //{
    //    AutoPathHelperUrban.OnPath -= gg;
    //}
    
    void gg()
    {
            int offset = 0;
            Transform[] kids = gameObject.GetComponentsInChildren<Transform>();
            for (int i = 1; i < kids.Length; i += offset)
            {
                if (kids[i].GetComponent<SFNodeInfo>().GetPoints().Length != 0)
            {
                LineRenderer lineR;
                GameObject empty = new GameObject("TESTLINER");
                empty.layer = LayerMask.NameToLayer("Graphics");
                empty.transform.eulerAngles = new Vector3(90, 90, 0);
                lineR = empty.AddComponent<LineRenderer>();
                lineR.useWorldSpace = true;
                lineR.alignment = LineAlignment.Local;
                AnimationCurve curve = new AnimationCurve();
                curve.AddKey(0, 2);
                curve.AddKey(1, 2);
                lineR.widthCurve = curve;
                int size = kids[i].GetComponent<SFNodeInfo>().GetPoints().Length;
                lineR.positionCount = size;
                lineR.SetPositions(kids[i].GetComponent<SFNodeInfo>().GetPoints());
                offset = size;
            }
                

            }
        
    }

    void bb()
    {
        Transform[] kids = GameObject.Find("SFIntersections").GetComponentsInChildren<Transform>();
        foreach (Transform tr in kids)
        {

            if (tr.gameObject.transform.parent != null)
            {
                if (tr.name == "Node1")
                {
                    AutoPathHelperUrban urb = tr.gameObject.GetComponent<AutoPathHelperUrban>();
                    Destroy(urb);
                }
            }
        }
    }

    void aa()
    {

        Transform[] kids = GameObject.Find("1").transform/*.GetChild(1)*/.GetComponentsInChildren<Transform>();



        //foreach (var tr in kids)
        //{
        //    if (tr.parent != null)
        //    {
        //        if (tr.name == "NodeInterp")
        //        {
        //            Destroy(tr.gameObject);

        //        }

        //    }
        //}
        //int i = 1;
        //foreach (Transform tr in kids)
        //{
        //    if (tr.parent != null)
        //    {
                //        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                //        g.transform.SetParent(tr);
                //        g.transform.localPosition = new Vector3(0f, 0f, 0f);
                //    }

                //}
                //int pathIndex = 1;
                //foreach (var tr in kids)
                //{
                //    if (tr.gameObject.transform.parent != null)
                //    {
                //        Vector3 currentSpot = tr.position;
                //        RaycastHit hit;
                //        Physics.Raycast(currentSpot + Vector3.up * 5, -Vector3.up, out hit, 100f, (1 << LayerMask.NameToLayer("Roads")));
                //        tr.position = new Vector3(currentSpot.x, hit.point.y + 0.2f, currentSpot.z);
                //    }
                //}


                //    //Transform[] kidsOdd = kids.Where((x, i) => i % 2 == 0).ToArray();
                //    //foreach (var k in kidsOdd)
                //    //    if (k.transform.parent != null)
                //    //        Destroy(k.gameObject);


                //}

                LineRenderer lineR;
                GameObject empty = new GameObject("TESTLINER");
                empty.layer = LayerMask.NameToLayer("Graphics");
                empty.transform.eulerAngles = new Vector3(90, 90, 0);
                lineR = empty.AddComponent<LineRenderer>();
                lineR.useWorldSpace = true;
                lineR.alignment = LineAlignment.Local;
                AnimationCurve curve = new AnimationCurve();
                curve.AddKey(0, 2);
                curve.AddKey(1, 2);
                lineR.widthCurve = curve;

                const int SPLINE_GIZMO_SMOOTHNESS = 10;
                var pointList = new List<Vector3>();
                //Vector3 lastPos = kids[1].transform.position;
                
                    float increaseAmount = 1f / (kids.Length * SPLINE_GIZMO_SMOOTHNESS);

                    for (float i = increaseAmount; i < 1; i += increaseAmount)
                    {
                        //Vector3 pos = GameObject.Find("1").GetComponent<CurvySpline>().Interpolate(i);
                        //pointList.Add(pos);

                        //lastPos = pos;
                    }

                    int size = pointList.Count;
                    lineR.positionCount = size;
                    lineR.SetPositions(pointList.ToArray());
                
            //}
                
        //}
    }

    void ff()
    {
        Transform[] kids = gameObject.GetComponentsInChildren<Transform>();
        List<float> dists = new List<float>();

        for (int i = 0; i < kids.Length; ++i)
        {
            if (kids[i].gameObject.transform.parent != null)
            {

                //Vector3 currentSpot = kids[i].transform.position;
                //RaycastHit hit;
                //Physics.Raycast(currentSpot + Vector3.up * 2, -Vector3.up, out hit, 100f, (1 << LayerMask.NameToLayer("Roads"))); //the layer is EnvironmentProp and not Roads since there is a hidden mesh before roads!
                //kids[i].transform.position = new Vector3(currentSpot.x, hit.point.y + 0.2f, currentSpot.z);

                //kids[i].transform.position += kids[i].transform.right * .4f;

                //kids[i].transform.SetSiblingIndex(kids[i].GetComponent<CurvySplineSegment>().ControlPointIndex);
                




            }
        }
        
        //LineRenderer lineR;
        //GameObject empty = new GameObject("TESTLINER");
        //empty.layer = LayerMask.NameToLayer("Graphics");
        //empty.transform.eulerAngles = new Vector3(90, 90, 0);
        //lineR = empty.AddComponent<LineRenderer>();
        //lineR.useWorldSpace = true;
        //lineR.alignment = LineAlignment.Local;
        //AnimationCurve curve = new AnimationCurve();
        //curve.AddKey(0, 2);
        //curve.AddKey(1, 2);
        //lineR.widthCurve = curve;

        //const int SPLINE_GIZMO_SMOOTHNESS = 10;
        //var pointList = new List<Vector3>();
        ////Vector3 lastPos = kids[1].transform.position;

        //float increaseAmount = 1f / (kids.Length * SPLINE_GIZMO_SMOOTHNESS);

        //for (float i = increaseAmount; i < 1; i += increaseAmount)
        //{
        //    Vector3 pos = gameObject.GetComponent<CurvySpline>().Interpolate(i);
        //    pointList.Add(pos);

        //    //lastPos = pos;
        //}

        //int size = pointList.Count;
        //lineR.positionCount = size;
        //lineR.SetPositions(pointList.ToArray());
    }

    void vv()
    {
        int pathIndex = 1; //offset must be one and not zero otherwise it will be included in the kids list also the parent of the hierarchy, resulting in wrong computations
        Transform[] kids = gameObject.GetComponentsInChildren<Transform>();
        foreach (var tr in kids)
        {
            if (tr.gameObject.transform.parent != null)
            {
               

                if (pathIndex < kids.Length - 1)
                {
                    tr.LookAt(kids[pathIndex + 1].position); //this is in order to orient the kids to the road direction
                }
                else
                    tr.LookAt(kids[1].position); //the last node takes the orientation of the first since it doesn't have a node in front of it


              pathIndex++;
            }
        }
    }

}


