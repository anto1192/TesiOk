using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AutoPathHelperUrban : MonoBehaviour {

    private TrafSystem tf;
    private GameObject ttag;
    private GameObject SFCurves;
    private GameObject SFIntersections;

    public Transform tobecopied;

    //private List<TrafEntry> entries = new List<TrafEntry>();



    //void OnDrawGizmos()
    //{
    //    Transform[] kids = GameObject.Find("SFIntersections").GetComponentsInChildren<Transform>();
    //    foreach (Transform tr in kids)
    //        if (tr.gameObject.transform.parent != null)
    //            GameObject.CreatePrimitive(PrimitiveType.Sphere).transform.SetParent(tr);
    //}

    private void Awake() {
        ttag = Resources.Load<GameObject>("Prefabs/InfoTag");
        SFCurves = Resources.Load<GameObject>("Prefabs/SFCurvesDef");
        SFIntersections = Resources.Load<GameObject>("Prefabs/SFIntersections");
    }

    void Start() {
        //InitSFIntersections();
        //InitsfCurves();
        func1();




    }



    void InitsfCurves()
    {
        //Dictionary<string, GameObject> nodes = new Dictionary<string, GameObject>();
        //List<Vector3> waypoints = new List<Vector3>();
        GameObject sfCurves = Instantiate(SFCurves);
        sfCurves.transform.SetParent(gameObject.transform);
        //Transform[] kids = GameObject.Find("SFCurvesDef").GetComponentsInChildren<Transform>();
        //foreach (Transform tr in kids)
        //    if (tr.gameObject.transform.parent != null)
        //        nodes.Add(tr.name, tr.gameObject);

        //for (int i = 3440; i <= 3483; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 3440; i <= 3483; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 3396; i <= 3439; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 1;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 3396; i <= 3439; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 3352; i <= 3395; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 2;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 3352; i <= 3395; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 3308; i <= 3351; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 3;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 3308; i <= 3351; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();







        //for (int i = 2848; i <= 2871; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 164;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 2848; i <= 2871; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 2872; i <= 2895; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 164;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 1;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 2872; i <= 2895; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 2896; i <= 2919; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 164;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 2;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 2896; i <= 2919; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 2920; i <= 2943; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 164;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 3;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 2920; i <= 2943; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();







        //for (int i = 2678; i <= 2703; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 162;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 2678; i <= 2703; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 2652; i <= 2677; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 162;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 1;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 2652; i <= 2677; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 2626; i <= 2651; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 162;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 2;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 2626; i <= 2651; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 2600; i <= 2625; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 162;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 3;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 2600; i <= 2625; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();







        //for (int i = 2572; i <= 2599; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 167;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 2572; i <= 2599; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 2544; i <= 2571; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 167;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 1;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 2544; i <= 2571; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 2516; i <= 2543; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 167;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 2;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 2516; i <= 2543; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 2488; i <= 2515; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 167;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 3;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 2488; i <= 2515; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();







        //for (int i = 2460; i <= 2487; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 155;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 2460; i <= 2487; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 2432; i <= 2459; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 155;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 1;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 2432; i <= 2459; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 2404; i <= 2431; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 155;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 2;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 2404; i <= 2431; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 2376; i <= 2403; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 155;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 3;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 2376; i <= 2403; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();







        //for (int i = 1534; i <= 1547; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 0;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 1534; i <= 1547; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 1520; i <= 1533; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 0;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 1;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 1520; i <= 1533; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 1506; i <= 1519; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 0;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 2;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 1506; i <= 1519; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 1492; i <= 1505; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 0;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 3;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 1492; i <= 1505; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();







        //for (int i = 1475; i <= 1491; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 150;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 1475; i <= 1491; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 1458; i <= 1474; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 150;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 1;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 1458; i <= 1474; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 1441; i <= 1457; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 150;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 2;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 1441; i <= 1457; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 1424; i <= 1440; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 150;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 3;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 1424; i <= 1440; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();







        //for (int i = 1385; i <= 1423; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 151;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 1385; i <= 1423; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 1346; i <= 1384; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 151;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 1;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 1346; i <= 1384; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 1307; i <= 1345; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 151;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 2;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 1307; i <= 1345; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 1268; i <= 1306; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 151;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 3;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 1268; i <= 1306; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();







        //for (int i = 1611; i <= 1631; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 165;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 1611; i <= 1631; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 1590; i <= 1610; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 165;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 1;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 1590; i <= 1610; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 1569; i <= 1589; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 165;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 2;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 1569; i <= 1589; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 1548; i <= 1568; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 165;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 3;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 1548; i <= 1568; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();







        //for (int i = 1632; i <= 1647; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 152;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 1632; i <= 1647; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 1648; i <= 1663; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 152;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 1;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 1648; i <= 1663; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 1664; i <= 1679; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 152;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 2;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 1664; i <= 1679; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 1680; i <= 1695; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 152;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 3;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 1680; i <= 1695; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();






        //for (int i = 1696; i <= 1705; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 154;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 1696; i <= 1705; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 1706; i <= 1715; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 154;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 1;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 1706; i <= 1715; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 1716; i <= 1725; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 154;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 2;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 1716; i <= 1725; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 1726; i <= 1735; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 154;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 3;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 1726; i <= 1735; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();







        //for (int i = 1787; i <= 1803; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 153;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 1787; i <= 1803; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 1770; i <= 1786; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 153;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 1;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 1770; i <= 1786; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 1753; i <= 1769; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 153;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 2;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 1753; i <= 1769; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 1736; i <= 1752; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 153;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 3;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 1736; i <= 1752; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();







        //for (int i = 1804; i <= 1839; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 130;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 1804; i <= 1839; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 1840; i <= 1875; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 130;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 1;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 1840; i <= 1875; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 1876; i <= 1911; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 130;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 2;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 1876; i <= 1911; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 1912; i <= 1947; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 130;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 3;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 1912; i <= 1947; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();







        //for (int i = 1948; i <= 1983; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 129;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 1948; i <= 1983; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 1984; i <= 2019; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 129;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 1;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 1984; i <= 2019; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 2020; i <= 2055; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 129;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 2;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 2020; i <= 2055; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 2056; i <= 2091; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 129;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 3;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 2056; i <= 2091; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();







        //for (int i = 2092; i <= 2127; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 128;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 2092; i <= 2127; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 2128; i <= 2163; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 128;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 1;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 2128; i <= 2163; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 2164; i <= 2199; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 128;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 2;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 2164; i <= 2199; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 2200; i <= 2235; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 128;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 3;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 2200; i <= 2235; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();







        //for (int i = 2236; i <= 2270; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 127;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 2236; i <= 2270; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 2271; i <= 2305; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 127;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 1;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 2271; i <= 2305; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 2306; i <= 2340; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 127;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 2;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 2306; i <= 2340; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 2341; i <= 2375; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 127;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 3;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 2341; i <= 2375; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();







        //for (int i = 2812; i <= 2847; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 163;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 2812; i <= 2847; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 2776; i <= 2811; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 163;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 1;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 2776; i <= 2811; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 2740; i <= 2775; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 163;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 2;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 2740; i <= 2775; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 2704; i <= 2739; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 163;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 3;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 2704; i <= 2739; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();







        //for (int i = 2944; i <= 2979; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 125;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 2944; i <= 2979; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 2980; i <= 3015; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 125;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 1;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 2980; i <= 3015; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 3016; i <= 3051; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 125;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 2;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 3016; i <= 3051; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 3052; i <= 3087; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 125;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 3;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 3052; i <= 3087; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();







        //for (int i = 207; i <= 275; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 124;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 207; i <= 275; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 138; i <= 206; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 124;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 1;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 138; i <= 206; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 69; i <= 137; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 124;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 2;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 69; i <= 137; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 0; i <= 68; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 124;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 3;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 0; i <= 68; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();







        //for (int i = 459; i <= 519; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 123;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 459; i <= 519; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 398; i <= 458; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 123;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 1;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 398; i <= 458; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 337; i <= 397; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 123;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 2;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 337; i <= 397; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 276; i <= 336; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 123;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 3;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 276; i <= 336; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();







        //for (int i = 520; i <= 538; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 122;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 520; i <= 538; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 539; i <= 557; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 122;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 1;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 539; i <= 557; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 558; i <= 576; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 122;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 2;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 558; i <= 576; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 577; i <= 595; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 122;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 3;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 577; i <= 595; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();







        //for (int i = 596; i <= 650; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 121;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 596; i <= 650; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 651; i <= 705; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 121;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 1;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 651; i <= 705; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 706; i <= 760; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 121;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 2;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 706; i <= 760; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 761; i <= 815; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 121;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 3;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 761; i <= 815; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();







        //for (int i = 816; i <= 928; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 120;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 816; i <= 928; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 929; i <= 1041; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 120;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 1;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 929; i <= 1041; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 1042; i <= 1154; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 120;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 2;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 1042; i <= 1154; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 1155; i <= 1267; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 120;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 3;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 1155; i <= 1267; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();







        //for (int i = 3589; i <= 3623; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 119;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 3589; i <= 3623; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 3554; i <= 3588; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 119;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 1;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 3554; i <= 3588; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 3519; i <= 3553; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 119;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 2;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 3519; i <= 3553; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 3484; i <= 3518; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 119;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 3;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 3484; i <= 3518; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();







        //for (int i = 3656; i <= 3718; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 118;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 3656; i <= 3718; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 3719; i <= 3781; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 118;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 1;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 3719; i <= 3781; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 3782; i <= 3844; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 118;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 2;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 3782; i <= 3844; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 3845; i <= 3907; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 118;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 3;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 3845; i <= 3907; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();







        //for (int i = 3908; i <= 3926; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 117;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 3908; i <= 3926; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 3927; i <= 3945; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 117;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 1;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 3927; i <= 3945; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 3946; i <= 3964; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 117;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 2;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 3946; i <= 3964; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 3965; i <= 3983; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 117;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 3;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 3965; i <= 3983; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();







        //for (int i = 3984; i <= 4078; ++i)    //nodi da correggere
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 116;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 3984; i <= 4078; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 4079; i <= 4173; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 116;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 1;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 4079; i <= 4173; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 4174; i <= 4268; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 116;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 2;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 4174; i <= 4268; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 4269; i <= 4363; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 116;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 3;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 4269; i <= 4363; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();







        //for (int i = 4364; i <= 4450; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 28;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 4364; i <= 4450; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 4451; i <= 4537; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 28;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 1;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 4451; i <= 4537; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 4538; i <= 4624; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 28;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 2;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 4538; i <= 4624; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 4625; i <= 4711; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 28;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 3;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 4625; i <= 4711; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();







        //for (int i = 4867; i <= 4925; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 174;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 4867; i <= 4925; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 4808; i <= 4866; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 174;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 1;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 4808; i <= 4866; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 4750; i <= 4807; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 174;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 2;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 4750; i <= 4807; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();







        //for (int i = 4926; i <= 6014; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 169;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 4926; i <= 6014; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 6015; i <= 7105; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 169;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 1;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 6015; i <= 7105; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 7106; i <= 8197; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 169;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 2;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 7106; i <= 8197; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();







        //for (int i = 8198; i <= 8277; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 175;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 8198; i <= 8277; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 8278; i <= 8360; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 175;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 1;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 8278; i <= 8360; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();
        //for (int i = 8361; i <= 8446; ++i)
        //{
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 175;
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 2;
        //    waypoints.Add(nodes["Node" + i].transform.position);
        //}
        //for (int i = 8361; i <= 8446; ++i)
        //    nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        //waypoints.Clear();

    }

    void InitSFIntersections()
    {
        GameObject sfIntersections = Instantiate(SFIntersections);
        sfIntersections.transform.SetParent(gameObject.transform);
    }

    List<GameObject> LinSpace(Vector3 p1, Vector3 p2, GameObject parent, TrafEntry tf)
    {
        List<GameObject> equallySpaced = new List<GameObject>();
        float maxSpacing = 5f;
        float maxDistance = 0;
        Vector3 diff = p2 - p1;
        Vector3 dir = diff.normalized;
        float totalDistance = diff.magnitude;
        for (float dist = maxDistance; dist < totalDistance; dist += maxSpacing)
        {
            Vector3 v = p1 + dir * dist;
            GameObject game = new GameObject("NodeInterp");
            Vector3 currentSpot = v;
            RaycastHit hit;
            Physics.Raycast(currentSpot + Vector3.up * 5, -Vector3.up, out hit, 100f, (1 << LayerMask.NameToLayer("EnvironmentProp"))); //the layer is EnvironmentProp and not Roads since there is a hidden mesh before roads!
            game.transform.position = new Vector3(currentSpot.x, hit.point.y + 0.2f, currentSpot.z);
            game.transform.SetParent(parent.transform);
            SphereCollider sphereCol = game.AddComponent<SphereCollider>();
            sphereCol.isTrigger = true;
            game.layer = LayerMask.NameToLayer("Graphics");
            game.tag = "Waypoint";

            tf.waypoints.Add(game.transform.position);

            SFNodeInfo nodeInfo = game.AddComponent<SFNodeInfo>();
            nodeInfo.identifier = tf.identifier;
            nodeInfo.subIdentifier = tf.subIdentifier;

            equallySpaced.Add(game);
        }
        return equallySpaced;
    }

    void func1()
    { //helper function used to visualize ids and sub-ids of the different trafEntries

        tf = FindObjectOfType(typeof(TrafSystem)) as TrafSystem;
        //foreach (var r in tf.intersections)
        //{
            int i = 0;
            var points = tf.intersections[0].GetPoints();
            //Vector3 sum = Vector3.zero;
            //if (points.Length == 2)
            //{
            foreach (var p in points)
            {
                GameObject game = new GameObject("Node" + i);
                game.transform.position = p;
                //    game.transform.SetParent(gameObject.transform);
                SphereCollider sphereCol = game.AddComponent<SphereCollider>();
                //    //sphereCol.isTrigger = true;
                //    //sphereCol.radius = 2.0f;
                //    game.layer = LayerMask.NameToLayer("Graphics");
                //    laneMarkers.Add(p.ToString(), new KeyValuePair<int, int>(r.identifier, r.subIdentifier));
                i++;
            }
            //LinSpace(points[0], points[1], gameObject);

            //} else
            //foreach (var p in points)
            //{
            //    sum += p;
            //}

            //Vector3 aver = sum / points.Length;
            //GameObject infoTag = Instantiate(ttag, aver, Quaternion.identity);
            //infoTag.transform.eulerAngles = new Vector3(90, 0, 0);
            //infoTag.transform.GetChild(0).transform.GetChild(0).GetComponent<Text>().text = "id: " + r.identifier.ToString() + "subid: " + r.subIdentifier.ToString();
            //infoTag.transform.GetChild(0).transform.GetChild(0).GetComponent<Text>().fontSize = 30;
            //foreach (var p in points)
            //{
            //    GameObject game = new GameObject("Node" + i);
            //    game.transform.position = p;
            //    game.transform.SetParent(gameObject.transform);
            //    SphereCollider sphereCol = game.AddComponent<SphereCollider>();
            //    i++;
            //}

        }


        

    void func2() //this is to increment the height of SFCurves points
    {
        GameObject go = GameObject.Find("SFCurvesDef");
        

        Transform[] kids = go.GetComponentsInChildren<Transform>();
        
        for (int i = 0; i < kids.Length; ++i)
        {
            if (kids[i].gameObject.transform.parent != null)
            {
                //kids[i].transform.position += Vector3.up * 0.4f;
                Vector3 currentSpot = kids[i].transform.position;
                RaycastHit hit;
                Physics.Raycast(currentSpot + Vector3.up * 2, -Vector3.up, out hit, 100f, (1 << LayerMask.NameToLayer("EnvironmentProp"))); //the layer is EnvironmentProp and not Roads since there is a hidden mesh before roads!
                kids[i].transform.position = new Vector3(currentSpot.x, hit.point.y + 0.2f, currentSpot.z);
                //SphereCollider sfc = tr.gameObject.AddComponent<SphereCollider>();
                //sfc.isTrigger = true;
                //tr.gameObject.layer = LayerMask.NameToLayer("Graphics");
                //tr.gameObject.tag = "Waypoint";
                //tr.name = "Node" + i;
                //i++;
            }
        }
    }

    void func3()
    {
        GameObject parent = new GameObject("SFIntersections");
        parent.transform.SetParent(gameObject.transform);
        tf = FindObjectOfType(typeof(TrafSystem)) as TrafSystem;
        List<GameObject> equallySpaced = new List<GameObject>();

        foreach (var r in tf.entries)
        {
            int i = 0;
            var points = r.GetPoints();

            if (points.Length == 2)
            {
                TrafEntry trafEntry = new TrafEntry();
                trafEntry.waypoints = new List<Vector3>();
                trafEntry.identifier = r.identifier;
                trafEntry.subIdentifier = r.subIdentifier;

                List<GameObject> gamosPerEntry = new List<GameObject>();

                foreach (var p in points)
                {
                    GameObject game = new GameObject("Node" + i);
                    Vector3 currentSpot = p;
                    RaycastHit hit;
                    Physics.Raycast(currentSpot + Vector3.up * 5, -Vector3.up, out hit, 100f, (1 << LayerMask.NameToLayer("EnvironmentProp"))); //the layer is EnvironmentProp and not Roads since there is a hidden mesh before roads!
                    game.transform.position = new Vector3(currentSpot.x, hit.point.y + 0.2f, currentSpot.z);
                    game.transform.SetParent(parent.transform);
                    SphereCollider sphereCol = game.AddComponent<SphereCollider>();
                    sphereCol.isTrigger = true;
                    game.layer = LayerMask.NameToLayer("Graphics");
                    game.tag = "Waypoint";


                    trafEntry.waypoints.Add(game.transform.position);

                    SFNodeInfo nodeInfo = game.AddComponent<SFNodeInfo>();
                    nodeInfo.identifier = trafEntry.identifier;
                    nodeInfo.subIdentifier = trafEntry.subIdentifier;

                    gamosPerEntry.Add(game);

                    i++;
                }
                equallySpaced = LinSpace(points[0], points[1], parent, trafEntry); //this is to obtain a set of equally spaced points
                Destroy(equallySpaced[0]); //since I don't want to instantiate points into p0 and p1 I delete them
                Destroy(equallySpaced[equallySpaced.Count - 1]); //since I don't want to instantiate points into p0 and p1 I delete them
                equallySpaced.RemoveAt(0); //I clean the list deleting the two extremes
                equallySpaced.RemoveAt(equallySpaced.Count - 1); //I clean the list deleting the two extremes

                trafEntry.waypoints.RemoveAt(0);
                trafEntry.waypoints.RemoveAt(trafEntry.waypoints.Count - 1);
                trafEntry.waypoints.Add(trafEntry.waypoints[0]);
                trafEntry.waypoints.RemoveAt(0);

                foreach (GameObject g in equallySpaced)
                    gamosPerEntry.Add(g)
;
                //entries.Add(trafEntry);

                foreach (GameObject g in gamosPerEntry)
                    g.GetComponent<SFNodeInfo>().waypoints = trafEntry.waypoints;

            }
        }
    }

    void func4() //this is to modify the waypoints data structure of each waypoint so that it contains only waypoints from the current index to the last and not all of them 
    {
        GameObject go = GameObject.Find("SFCurvesDef");
        Transform[] kids = go.GetComponentsInChildren<Transform>();

        for (int i = 0; i < kids.Length; ++i)
        {
            if (kids[i].gameObject.transform.parent != null && kids[i].GetComponent<SFNodeInfo>().waypoints.Count != 0)
            {
                int ii = kids[i].GetComponent<SFNodeInfo>().waypoints.IndexOf(kids[i].transform.position);
                Vector3[] w = kids[i].GetComponent<SFNodeInfo>().GetPoints();
                List<Vector3> corrected = new List<Vector3>();
                for (int y = ii; y < w.Length; ++y)
                    corrected.Add(w[y]);
                kids[i].GetComponent<SFNodeInfo>().waypoints.Clear();
                kids[i].GetComponent<SFNodeInfo>().waypoints.AddRange(corrected);
            }
        }
    }

    void func5()
    {
        GameObject parent = new GameObject("SFTurns");
        tf = FindObjectOfType(typeof(TrafSystem)) as TrafSystem;
        
        foreach (var r in tf.intersections)
        {
            int i = 0;
            var points = r.GetPoints();
            TrafEntry trafEntry = new TrafEntry();
            trafEntry.waypoints = new List<Vector3>();
            trafEntry.identifier = r.identifier;
            trafEntry.subIdentifier = r.subIdentifier;

            foreach (var p in points)
            {
                GameObject game = new GameObject("Node" + i);
                Vector3 currentSpot = p;
                RaycastHit hit;
                Physics.Raycast(currentSpot + Vector3.up * 5, -Vector3.up, out hit, 100f, (1 << LayerMask.NameToLayer("EnvironmentProp"))); //the layer is EnvironmentProp and not Roads since there is a hidden mesh before roads!
                game.transform.position = new Vector3(currentSpot.x, hit.point.y + 0.2f, currentSpot.z);
                game.transform.SetParent(parent.transform);
                SphereCollider sphereCol = game.AddComponent<SphereCollider>();
                sphereCol.isTrigger = true;
                game.layer = LayerMask.NameToLayer("Graphics");
                game.tag = "Waypoint";


                trafEntry.waypoints.Add(game.transform.position);

                SFNodeInfo nodeInfo = game.AddComponent<SFNodeInfo>();
                nodeInfo.identifier = trafEntry.identifier;
                nodeInfo.subIdentifier = trafEntry.subIdentifier;
                nodeInfo.waypoints = trafEntry.waypoints;

                i++;
            }
           

            
        }
    }

    void func6()
    {
        Transform[] kids = GameObject.Find("GuardrailUrban").GetComponentsInChildren<Transform>();
        List<Transform> nodes = new List<Transform>();

         //offset must be one and not zero otherwise it will be included in the kids list also the parent of the hierarchy, resulting in wrong computations

        for (int i = 928; i < kids.Length; ++i)
        {
            GameObject go = GameObject.Find("Node" + i);
            if (go != null)
            {
                nodes.Add(go.transform);
            }
            
        }

        int ii = 927;
        foreach (Transform tr in nodes)
        { tr.name = "Node" + ii; ii ++; }
            

    }

    void func7()
    {
        Dictionary<string, GameObject> nodes = new Dictionary<string, GameObject>();
        List<Vector3> waypoints = new List<Vector3>();
        Transform[] kids = GameObject.Find("GuardrailUrban").GetComponentsInChildren<Transform>();
        foreach (Transform tr in kids)
            if (tr.gameObject.transform.parent != null)
                nodes.Add(tr.name, tr.gameObject);

        for (int i = 0; i <= 36; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 0; i <= 36; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 37; i <= 70; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 37; i <= 70; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 71; i <= 105; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 71; i <= 105; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 106; i <= 140; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 106; i <= 140; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 141; i <= 175; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 141; i <= 175; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 176; i <= 210; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 176; i <= 210; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 211; i <= 246; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 211; i <= 246; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 247; i <= 281; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 247; i <= 281; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 282; i <= 394; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 282; i <= 394; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 395; i <= 429; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 395; i <= 429; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 430; i <= 464; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 430; i <= 464; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 465; i <= 499; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 465; i <= 499; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 500; i <= 534; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 500; i <= 534; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 535; i <= 555; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 535; i <= 555; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 556; i <= 575; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 556; i <= 575; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 576; i <= 592; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 576; i <= 592; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 593; i <= 613; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 593; i <= 613; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 614; i <= 631; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 614; i <= 631; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 632; i <= 682; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 632; i <= 682; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 683; i <= 698; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 683; i <= 698; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 699; i <= 732; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 699; i <= 732; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 733; i <= 783; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 733; i <= 783; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 784; i <= 818; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 784; i <= 818; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 819; i <= 854; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 819; i <= 854; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 855; i <= 890; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 855; i <= 890; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 891; i <= 926; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 891; i <= 926; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 927; i <= 948; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 927; i <= 948; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 949; i <= 979; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 949; i <= 979; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 980; i <= 1010; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 980; i <= 1010; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 1011; i <= 1023; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 1011; i <= 1023; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 1024; i <= 1049; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 1024; i <= 1049; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 1050; i <= 1067; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 1050; i <= 1067; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 1068; i <= 1085; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 1068; i <= 1085; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 1086; i <= 1106; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 1086; i <= 1106; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 1107; i <= 1125; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 1107; i <= 1125; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 1126; i <= 1143; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 1126; i <= 1143; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 1144; i <= 1164; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 1144; i <= 1164; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 1165; i <= 1177; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 1165; i <= 1177; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 1178; i <= 1232; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 1178; i <= 1232; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 1233; i <= 1249; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 1233; i <= 1249; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 1250; i <= 1265; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 1250; i <= 1265; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 1266; i <= 1282; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 1266; i <= 1282; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 1283; i <= 1292; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 1283; i <= 1292; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 1293; i <= 1310; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 1293; i <= 1310; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 1311; i <= 1336; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 1311; i <= 1336; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 1337; i <= 1360; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 1337; i <= 1360; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 1361; i <= 1384; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 1361; i <= 1384; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 1385; i <= 1403; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 1385; i <= 1403; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 1404; i <= 1464; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 1404; i <= 1464; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();



        for (int i = 1465; i <= 1533; ++i)
        {
            //nodes["Node" + i].GetComponent<SFNodeInfo>().identifier = 166;
            //nodes["Node" + i].GetComponent<SFNodeInfo>().subIdentifier = 0;
            waypoints.Add(nodes["Node" + i].transform.position);
        }
        for (int i = 1465; i <= 1533; ++i)
            nodes["Node" + i].GetComponent<SFNodeInfo>().waypoints.AddRange(waypoints);
        waypoints.Clear();
    }

    void func8()
    {

        Dictionary<string, GameObject> nodes = new Dictionary<string, GameObject>();
        Transform[] kids = GameObject.Find("SFCurvesDef").GetComponentsInChildren<Transform>();
        foreach (Transform tr in kids)
            if (tr.gameObject.transform.parent != null)
                nodes.Add(tr.name, tr.gameObject);

        for (int i = 0; i <= 68; ++i)
        {
            nodes["Node" + i].transform.LookAt(nodes["Node" + (i+1)].transform);
            if (i == 68)
                nodes["Node" + i].transform.rotation = nodes["Node" + (i - 1)].transform.rotation;
        }


        for (int i = 69; i <= 137; ++i)
        {
            nodes["Node" + i].transform.LookAt(nodes["Node" + (i + 1)].transform);
            if (i == 137)
                nodes["Node" + i].transform.rotation = nodes["Node" + (i - 1)].transform.rotation;
        }


        for (int i = 138; i <= 206; ++i)
        {
            nodes["Node" + i].transform.LookAt(nodes["Node" + (i + 1)].transform);
            if (i == 206)
                nodes["Node" + i].transform.rotation = nodes["Node" + (i - 1)].transform.rotation;
        }


        for (int i = 207; i <= 275; ++i)
        {
            nodes["Node" + i].transform.LookAt(nodes["Node" + (i + 1)].transform);
            if (i == 275)
                nodes["Node" + i].transform.rotation = nodes["Node" + (i - 1)].transform.rotation;
        }


        for (int i = 276; i <= 336; ++i)
        {
            nodes["Node" + i].transform.LookAt(nodes["Node" + (i + 1)].transform);
            if (i == 336)
                nodes["Node" + i].transform.rotation = nodes["Node" + (i - 1)].transform.rotation;
        }


        for (int i = 337; i <= 397; ++i)
        {
            nodes["Node" + i].transform.LookAt(nodes["Node" + (i + 1)].transform);
            if (i == 397)
                nodes["Node" + i].transform.rotation = nodes["Node" + (i - 1)].transform.rotation;
        }


        for (int i = 398; i <= 458; ++i)
        {
            nodes["Node" + i].transform.LookAt(nodes["Node" + (i + 1)].transform);
            if (i == 458)
                nodes["Node" + i].transform.rotation = nodes["Node" + (i - 1)].transform.rotation;
        }


        for (int i = 459; i <= 519; ++i)
        {
            nodes["Node" + i].transform.LookAt(nodes["Node" + (i + 1)].transform);
            if (i == 519)
                nodes["Node" + i].transform.rotation = nodes["Node" + (i - 1)].transform.rotation;
        }


        for (int i = 520; i <= 538; ++i)
        {
            nodes["Node" + i].transform.LookAt(nodes["Node" + (i + 1)].transform);
            if (i == 538)
                nodes["Node" + i].transform.rotation = nodes["Node" + (i - 1)].transform.rotation;
        }


        for (int i = 539; i <= 557; ++i)
        {
            nodes["Node" + i].transform.LookAt(nodes["Node" + (i + 1)].transform);
            if (i == 557)
                nodes["Node" + i].transform.rotation = nodes["Node" + (i - 1)].transform.rotation;
        }


        for (int i = 558; i <= 576; ++i)
        {
            nodes["Node" + i].transform.LookAt(nodes["Node" + (i + 1)].transform);
            if (i == 576)
                nodes["Node" + i].transform.rotation = nodes["Node" + (i - 1)].transform.rotation;
        }


        for (int i = 577; i <= 595; ++i)
        {
            nodes["Node" + i].transform.LookAt(nodes["Node" + (i + 1)].transform);
            if (i == 595)
                nodes["Node" + i].transform.rotation = nodes["Node" + (i - 1)].transform.rotation;
        }


        for (int i = 596; i <= 650; ++i)
        {
            nodes["Node" + i].transform.LookAt(nodes["Node" + (i + 1)].transform);
            if (i == 650)
                nodes["Node" + i].transform.rotation = nodes["Node" + (i - 1)].transform.rotation;
        }


        for (int i = 651; i <= 705; ++i)
        {
            nodes["Node" + i].transform.LookAt(nodes["Node" + (i + 1)].transform);
            if (i == 705)
                nodes["Node" + i].transform.rotation = nodes["Node" + (i - 1)].transform.rotation;
        }


        for (int i = 706; i <= 760; ++i)
        {
            nodes["Node" + i].transform.LookAt(nodes["Node" + (i + 1)].transform);
            if (i == 760)
                nodes["Node" + i].transform.rotation = nodes["Node" + (i - 1)].transform.rotation;
        }


        for (int i = 761; i <= 815; ++i)
        {
            nodes["Node" + i].transform.LookAt(nodes["Node" + (i + 1)].transform);
            if (i == 815)
                nodes["Node" + i].transform.rotation = nodes["Node" + (i - 1)].transform.rotation;
        }


        for (int i = 1736; i <= 1752; ++i)
        {
            nodes["Node" + i].transform.LookAt(nodes["Node" + (i + 1)].transform);
            if (i == 1752)
                nodes["Node" + i].transform.rotation = nodes["Node" + (i - 1)].transform.rotation;
        }


        for (int i = 1753; i <= 1769; ++i)
        {
            nodes["Node" + i].transform.LookAt(nodes["Node" + (i + 1)].transform);
            if (i == 1769)
                nodes["Node" + i].transform.rotation = nodes["Node" + (i - 1)].transform.rotation;
        }


        for (int i = 1770; i <= 1786; ++i)
        {
            nodes["Node" + i].transform.LookAt(nodes["Node" + (i + 1)].transform);
            if (i == 1786)
                nodes["Node" + i].transform.rotation = nodes["Node" + (i - 1)].transform.rotation;
        }


        for (int i = 1787; i <= 1803; ++i)
        {
            nodes["Node" + i].transform.LookAt(nodes["Node" + (i + 1)].transform);
            if (i == 1803)
                nodes["Node" + i].transform.rotation = nodes["Node" + (i - 1)].transform.rotation;
        }


        for (int i = 1696; i <= 1705; ++i)
        {
            nodes["Node" + i].transform.LookAt(nodes["Node" + (i + 1)].transform);
            if (i == 1705)
                nodes["Node" + i].transform.rotation = nodes["Node" + (i - 1)].transform.rotation;
        }


        for (int i = 1706; i <= 1715; ++i)
        {
            nodes["Node" + i].transform.LookAt(nodes["Node" + (i + 1)].transform);
            if (i == 1715)
                nodes["Node" + i].transform.rotation = nodes["Node" + (i - 1)].transform.rotation;
        }


        for (int i = 1716; i <= 1725; ++i)
        {
            nodes["Node" + i].transform.LookAt(nodes["Node" + (i + 1)].transform);
            if (i == 1725)
                nodes["Node" + i].transform.rotation = nodes["Node" + (i - 1)].transform.rotation;
        }


        for (int i = 1726; i <= 1735; ++i)
        {
            nodes["Node" + i].transform.LookAt(nodes["Node" + (i + 1)].transform);
            if (i == 1735)
                nodes["Node" + i].transform.rotation = nodes["Node" + (i - 1)].transform.rotation;
        }


        for (int i = 1804; i <= 1839; ++i)
        {
            nodes["Node" + i].transform.LookAt(nodes["Node" + (i + 1)].transform);
            if (i == 1839)
                nodes["Node" + i].transform.rotation = nodes["Node" + (i - 1)].transform.rotation;
        }


        for (int i = 1840; i <= 1875; ++i)
        {
            nodes["Node" + i].transform.LookAt(nodes["Node" + (i + 1)].transform);
            if (i == 1875)
                nodes["Node" + i].transform.rotation = nodes["Node" + (i - 1)].transform.rotation;
        }


        for (int i = 1876; i <= 1911; ++i)
        {
            nodes["Node" + i].transform.LookAt(nodes["Node" + (i + 1)].transform);
            if (i == 1911)
                nodes["Node" + i].transform.rotation = nodes["Node" + (i - 1)].transform.rotation;
        }


        for (int i = 1912; i <= 1947; ++i)
        {
            nodes["Node" + i].transform.LookAt(nodes["Node" + (i + 1)].transform);
            if (i == 1947)
                nodes["Node" + i].transform.rotation = nodes["Node" + (i - 1)].transform.rotation;
        }


        for (int i = 816; i <= 928; ++i)
        {
            nodes["Node" + i].transform.LookAt(nodes["Node" + (i + 1)].transform);
            if (i == 928)
                nodes["Node" + i].transform.rotation = nodes["Node" + (i - 1)].transform.rotation;
        }


        for (int i = 929; i <= 1041; ++i)
        {
            nodes["Node" + i].transform.LookAt(nodes["Node" + (i + 1)].transform);
            if (i == 1041)
                nodes["Node" + i].transform.rotation = nodes["Node" + (i - 1)].transform.rotation;
        }


        for (int i = 1042; i <= 1154; ++i)
        {
            nodes["Node" + i].transform.LookAt(nodes["Node" + (i + 1)].transform);
            if (i == 1154)
                nodes["Node" + i].transform.rotation = nodes["Node" + (i - 1)].transform.rotation;
        }


        for (int i = 1155; i <= 1267; ++i)
        {
            nodes["Node" + i].transform.LookAt(nodes["Node" + (i + 1)].transform);
            if (i == 1267)
                nodes["Node" + i].transform.rotation = nodes["Node" + (i - 1)].transform.rotation;
        }


        for (int i = 3484; i <= 3518; ++i)
        {
            nodes["Node" + i].transform.LookAt(nodes["Node" + (i + 1)].transform);
            if (i == 3518)
                nodes["Node" + i].transform.rotation = nodes["Node" + (i - 1)].transform.rotation;
        }


        for (int i = 3519; i <= 3553; ++i)
        {
            nodes["Node" + i].transform.LookAt(nodes["Node" + (i + 1)].transform);
            if (i == 3553)
                nodes["Node" + i].transform.rotation = nodes["Node" + (i - 1)].transform.rotation;
        }


        for (int i = 3554; i <= 3588; ++i)
        {
            nodes["Node" + i].transform.LookAt(nodes["Node" + (i + 1)].transform);
            if (i == 3588)
                nodes["Node" + i].transform.rotation = nodes["Node" + (i - 1)].transform.rotation;
        }


        for (int i = 3589; i <= 3623; ++i)
        {
            nodes["Node" + i].transform.LookAt(nodes["Node" + (i + 1)].transform);
            if (i == 3623)
                nodes["Node" + i].transform.rotation = nodes["Node" + (i - 1)].transform.rotation;
        }




    } //this is to assign the proper orientation to SFCurvesDef points

    void func9()
    {
        
        Transform[] kids = GameObject.Find("SFIntersections").GetComponentsInChildren<Transform>();
        Dictionary<int, Quaternion> nodes = new Dictionary<int, Quaternion>();
        //foreach (Transform tr in kids)
        //{
        //    if (tr.gameObject.transform.parent != null)
        //    {
        //        Vector3[] ve;
        //        if (tr.name == "Node0")
        //        {
        //            ve = tr.gameObject.GetComponent<SFNodeInfo>().GetPoints();
        //            int len = ve.Length - 2;
        //            if (!nodes.ContainsKey(tr.gameObject.GetComponent<SFNodeInfo>().identifier))
        //                nodes.Add(tr.gameObject.GetComponent<SFNodeInfo>().identifier, tr.gameObject.GetComponent<SFNodeInfo>().GetPoints()[len].);
        //        }
        //    }
                
        //}

        foreach (Transform tr in kids)
        {
            if (tr.gameObject.transform.parent != null)
            {
                if (tr.gameObject.GetComponent<SFNodeInfo>().identifier == 161)
                {
                    if (tr.gameObject.GetComponent<SFNodeInfo>().GetPoints().Length != 1)
                        tr.LookAt(tr.GetComponent<SFNodeInfo>().GetPoints()[1]);
                }


                if (tr.gameObject.GetComponent<SFNodeInfo>().identifier == 160)
                {
                    if (tr.gameObject.GetComponent<SFNodeInfo>().GetPoints().Length != 1)
                        tr.LookAt(tr.GetComponent<SFNodeInfo>().GetPoints()[1]);
                }


                if (tr.gameObject.GetComponent<SFNodeInfo>().identifier == 159)
                {
                    if (tr.gameObject.GetComponent<SFNodeInfo>().GetPoints().Length != 1)
                        tr.LookAt(tr.GetComponent<SFNodeInfo>().GetPoints()[1]);
                }


                if (tr.gameObject.GetComponent<SFNodeInfo>().identifier == 158)
                {
                    if (tr.gameObject.GetComponent<SFNodeInfo>().GetPoints().Length != 1)
                        tr.LookAt(tr.GetComponent<SFNodeInfo>().GetPoints()[1]);
                }


                if (tr.gameObject.GetComponent<SFNodeInfo>().identifier == 157)
                {
                    if (tr.gameObject.GetComponent<SFNodeInfo>().GetPoints().Length != 1)
                        tr.LookAt(tr.GetComponent<SFNodeInfo>().GetPoints()[1]);
                }


                if (tr.gameObject.GetComponent<SFNodeInfo>().identifier == 156)
                {
                    if (tr.gameObject.GetComponent<SFNodeInfo>().GetPoints().Length != 1)
                        tr.LookAt(tr.GetComponent<SFNodeInfo>().GetPoints()[1]);
                }


                if (tr.gameObject.GetComponent<SFNodeInfo>().identifier == 73)
                {
                    if (tr.gameObject.GetComponent<SFNodeInfo>().GetPoints().Length != 1)
                        tr.LookAt(tr.GetComponent<SFNodeInfo>().GetPoints()[1]);
                }


                if (tr.gameObject.GetComponent<SFNodeInfo>().identifier == 72)
                {
                    if (tr.gameObject.GetComponent<SFNodeInfo>().GetPoints().Length != 1)
                        tr.LookAt(tr.GetComponent<SFNodeInfo>().GetPoints()[1]);
                }


                if (tr.gameObject.GetComponent<SFNodeInfo>().identifier == 71)
                {
                    if (tr.gameObject.GetComponent<SFNodeInfo>().GetPoints().Length != 1)
                        tr.LookAt(tr.GetComponent<SFNodeInfo>().GetPoints()[1]);
                }


                if (tr.gameObject.GetComponent<SFNodeInfo>().identifier == 70)
                {
                    if (tr.gameObject.GetComponent<SFNodeInfo>().GetPoints().Length != 1)
                        tr.LookAt(tr.GetComponent<SFNodeInfo>().GetPoints()[1]);
                }


                if (tr.gameObject.GetComponent<SFNodeInfo>().identifier == 69)
                {
                    if (tr.gameObject.GetComponent<SFNodeInfo>().GetPoints().Length != 1)
                        tr.LookAt(tr.GetComponent<SFNodeInfo>().GetPoints()[1]);
                }


                if (tr.gameObject.GetComponent<SFNodeInfo>().identifier == 68)
                {
                    if (tr.gameObject.GetComponent<SFNodeInfo>().GetPoints().Length != 1)
                        tr.LookAt(tr.GetComponent<SFNodeInfo>().GetPoints()[1]);
                }


                if (tr.gameObject.GetComponent<SFNodeInfo>().identifier == 133)
                {
                    if (tr.gameObject.GetComponent<SFNodeInfo>().GetPoints().Length != 1)
                        tr.LookAt(tr.GetComponent<SFNodeInfo>().GetPoints()[1]);
                }


                if (tr.gameObject.GetComponent<SFNodeInfo>().identifier == 132)
                {
                    if (tr.gameObject.GetComponent<SFNodeInfo>().GetPoints().Length != 1)
                        tr.LookAt(tr.GetComponent<SFNodeInfo>().GetPoints()[1]);
                }


                if (tr.gameObject.GetComponent<SFNodeInfo>().identifier == 131)
                {
                    if (tr.gameObject.GetComponent<SFNodeInfo>().GetPoints().Length != 1)
                        tr.LookAt(tr.GetComponent<SFNodeInfo>().GetPoints()[1]);
                }


                if (tr.gameObject.GetComponent<SFNodeInfo>().identifier == 134)
                {
                    if (tr.gameObject.GetComponent<SFNodeInfo>().GetPoints().Length != 1)
                        tr.LookAt(tr.GetComponent<SFNodeInfo>().GetPoints()[1]);
                }


                if (tr.gameObject.GetComponent<SFNodeInfo>().identifier == 103)
                {
                    if (tr.gameObject.GetComponent<SFNodeInfo>().GetPoints().Length != 1)
                        tr.LookAt(tr.GetComponent<SFNodeInfo>().GetPoints()[1]);
                }


                if (tr.gameObject.GetComponent<SFNodeInfo>().identifier == 104)
                {
                    if (tr.gameObject.GetComponent<SFNodeInfo>().GetPoints().Length != 1)
                        tr.LookAt(tr.GetComponent<SFNodeInfo>().GetPoints()[1]);
                }


                if (tr.gameObject.GetComponent<SFNodeInfo>().identifier == 30)
                {
                    if (tr.gameObject.GetComponent<SFNodeInfo>().GetPoints().Length != 1)
                        tr.LookAt(tr.GetComponent<SFNodeInfo>().GetPoints()[1]);
                }


                if (tr.gameObject.GetComponent<SFNodeInfo>().identifier == 48)
                {
                    if (tr.gameObject.GetComponent<SFNodeInfo>().GetPoints().Length != 1)
                        tr.LookAt(tr.GetComponent<SFNodeInfo>().GetPoints()[1]);
                }


                if (tr.gameObject.GetComponent<SFNodeInfo>().identifier == 67)
                {
                    if (tr.gameObject.GetComponent<SFNodeInfo>().GetPoints().Length != 1)
                        tr.LookAt(tr.GetComponent<SFNodeInfo>().GetPoints()[1]);
                }


                if (tr.gameObject.GetComponent<SFNodeInfo>().identifier == 24)
                {
                    if (tr.gameObject.GetComponent<SFNodeInfo>().GetPoints().Length != 1)
                        tr.LookAt(tr.GetComponent<SFNodeInfo>().GetPoints()[1]);
                }


                if (tr.gameObject.GetComponent<SFNodeInfo>().identifier == 23)
                {
                    if (tr.gameObject.GetComponent<SFNodeInfo>().GetPoints().Length != 1)
                        tr.LookAt(tr.GetComponent<SFNodeInfo>().GetPoints()[1]);
                }


                if (tr.gameObject.GetComponent<SFNodeInfo>().identifier == 22)
                {
                    if (tr.gameObject.GetComponent<SFNodeInfo>().GetPoints().Length != 1)
                        tr.LookAt(tr.GetComponent<SFNodeInfo>().GetPoints()[1]);
                }


                if (tr.gameObject.GetComponent<SFNodeInfo>().identifier == 21)
                {
                    if (tr.gameObject.GetComponent<SFNodeInfo>().GetPoints().Length != 1)
                        tr.LookAt(tr.GetComponent<SFNodeInfo>().GetPoints()[1]);
                }


                if (tr.gameObject.GetComponent<SFNodeInfo>().identifier == 56)
                {
                    if (tr.gameObject.GetComponent<SFNodeInfo>().GetPoints().Length != 1)
                        tr.LookAt(tr.GetComponent<SFNodeInfo>().GetPoints()[1]);
                }


                if (tr.gameObject.GetComponent<SFNodeInfo>().identifier == 57)
                {
                    if (tr.gameObject.GetComponent<SFNodeInfo>().GetPoints().Length != 1)
                        tr.LookAt(tr.GetComponent<SFNodeInfo>().GetPoints()[1]);
                }


                if (tr.gameObject.GetComponent<SFNodeInfo>().identifier == 58)
                {
                    if (tr.gameObject.GetComponent<SFNodeInfo>().GetPoints().Length != 1)
                        tr.LookAt(tr.GetComponent<SFNodeInfo>().GetPoints()[1]);
                }


                if (tr.gameObject.GetComponent<SFNodeInfo>().identifier == 59)
                {
                    if (tr.gameObject.GetComponent<SFNodeInfo>().GetPoints().Length != 1)
                        tr.LookAt(tr.GetComponent<SFNodeInfo>().GetPoints()[1]);
                }


                if (tr.gameObject.GetComponent<SFNodeInfo>().identifier == 60)
                {
                    if (tr.gameObject.GetComponent<SFNodeInfo>().GetPoints().Length != 1)
                        tr.LookAt(tr.GetComponent<SFNodeInfo>().GetPoints()[1]);
                }


                if (tr.gameObject.GetComponent<SFNodeInfo>().identifier == 12)
                {
                    if (tr.gameObject.GetComponent<SFNodeInfo>().GetPoints().Length != 1)
                        tr.LookAt(tr.GetComponent<SFNodeInfo>().GetPoints()[1]);
                }


                if (tr.gameObject.GetComponent<SFNodeInfo>().identifier == 51)
                {
                    if (tr.gameObject.GetComponent<SFNodeInfo>().GetPoints().Length != 1)
                        tr.LookAt(tr.GetComponent<SFNodeInfo>().GetPoints()[1]);
                }


                if (tr.gameObject.GetComponent<SFNodeInfo>().identifier == 47)
                {
                    if (tr.gameObject.GetComponent<SFNodeInfo>().GetPoints().Length != 1)
                        tr.LookAt(tr.GetComponent<SFNodeInfo>().GetPoints()[1]);
                }


                if (tr.gameObject.GetComponent<SFNodeInfo>().identifier == 66)
                {
                    if (tr.gameObject.GetComponent<SFNodeInfo>().GetPoints().Length != 1)
                        tr.LookAt(tr.GetComponent<SFNodeInfo>().GetPoints()[1]);
                }


                if (tr.gameObject.GetComponent<SFNodeInfo>().identifier == 11)
                {
                    if (tr.gameObject.GetComponent<SFNodeInfo>().GetPoints().Length != 1)
                        tr.LookAt(tr.GetComponent<SFNodeInfo>().GetPoints()[1]);
                }


                if (tr.gameObject.GetComponent<SFNodeInfo>().identifier == 50)
                {
                    if (tr.gameObject.GetComponent<SFNodeInfo>().GetPoints().Length != 1)
                        tr.LookAt(tr.GetComponent<SFNodeInfo>().GetPoints()[1]);
                }


                if (tr.gameObject.GetComponent<SFNodeInfo>().identifier == 46)
                {
                    if (tr.gameObject.GetComponent<SFNodeInfo>().GetPoints().Length != 1)
                        tr.LookAt(tr.GetComponent<SFNodeInfo>().GetPoints()[1]);
                }


                if (tr.gameObject.GetComponent<SFNodeInfo>().identifier == 10)
                {
                    if (tr.gameObject.GetComponent<SFNodeInfo>().GetPoints().Length != 1)
                        tr.LookAt(tr.GetComponent<SFNodeInfo>().GetPoints()[1]);
                }


                if (tr.gameObject.GetComponent<SFNodeInfo>().identifier == 49)
                {
                    if (tr.gameObject.GetComponent<SFNodeInfo>().GetPoints().Length != 1)
                        tr.LookAt(tr.GetComponent<SFNodeInfo>().GetPoints()[1]);
                }


                if (tr.gameObject.GetComponent<SFNodeInfo>().identifier == 45)
                {
                    if (tr.gameObject.GetComponent<SFNodeInfo>().GetPoints().Length != 1)
                        tr.LookAt(tr.GetComponent<SFNodeInfo>().GetPoints()[1]);
                }


                if (tr.gameObject.GetComponent<SFNodeInfo>().identifier == 29)
                {
                    if (tr.gameObject.GetComponent<SFNodeInfo>().GetPoints().Length != 1)
                        tr.LookAt(tr.GetComponent<SFNodeInfo>().GetPoints()[1]);
                }

            }

        }
    }

    void func10()
    {
        Transform[] kids = GameObject.Find("SFIntersections").GetComponentsInChildren<Transform>();
        foreach (Transform tr in kids)
        {
            if (tr.gameObject.transform.parent != null)
            {
                if (tr.name == "NodeInterp")
                {
                    Destroy(tr.gameObject);
                }

            }
        }
    }


}











