using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using MKGlowSystem;
using System.Linq;
using TMPro;


public class EnvironmentSensingAltTrigger : MonoBehaviour
{
    private bool BOUNDINGCUBE = true; //this is to select the appropriate function: BoundingCubeLerped or InfoTagWarn
    private bool dashORHud = false; //enable/disable dash

    private LinesUtils linesUtils = new LinesUtils();
    private RiskAssessment riskAssessment;
    private TrafficLightContainer trafLight; //this is to track the last trafficLight

    private Vector3 infoTagStartScale = Vector3.zero;

    [SerializeField]
    private AnimationCurve infoTagResize = new AnimationCurve();

    private Dictionary<int, float> CSVDictionary = new Dictionary<int, float>(); //this is to store the precomputed values of angle(speed)
    public Dictionary<int, CubesAndTags> IDsAndGos = new Dictionary<int, CubesAndTags>(); //this is to store gameobjects ids and their relative cubes
    private Dictionary<string, int> CSVSignDictionary = new Dictionary<string, int>(); //this is to store the precomputed values of angle(speed)

    private Quaternion rot0; //this is to store the initial rotation of the needle

    //private GameObject speedPanel; //this is used to semplify the search into the car hierarchy which is very deep
    public Vector3 obstaclePrevPos; //this is to store the initial position of the obstacle in order to compute its speed

    private LayerMask mask;

    private Transform rayCastPos;
    //private RectTransform speed;
    //private RectTransform needle;
    private Rigidbody rb;
    private VehicleController vehicleController;

    private DriverCamera driverCam;
    private GameObject leapCam;

    //public GameObject SpeedPanel { set { speedPanel = value; } }
    public DriverCamera DriverCam { set { driverCam = value; } }
    public GameObject LeapCam { set { leapCam = value; } }
    public Transform RayCastPos { get { return rayCastPos; } }
    public Rigidbody Rb { get { return rb; } }
    public VehicleController Vc { get { return vehicleController; } }
    public LayerMask Mask { get { return mask; } }

    void Awake()
    {
        LoadDictionary();
    }

    void Start() {

        LoadCarHierarchy();

        infoTagStartScale = ResourceHandler.instance.prefabs[3].transform.localScale;

        CreateInfoTagResizeCurve();

        linesUtils.CenterLineLerper(Color.red, Color.yellow, Color.green, new Color32(0x00, 0x80, 0xFF, 0x00));

        mask = (1 << LayerMask.NameToLayer("Traffic")) | (1 << LayerMask.NameToLayer("obstacle")) | (1 << LayerMask.NameToLayer("EnvironmentProp")) | (1 << LayerMask.NameToLayer("Signage")); //restrict OnTriggerEnter only to the layers of interest

        StartCoroutine(CleanObjects(0.25f));

        riskAssessment = new RiskAssessment(infoTagStartScale, driverCam, infoTagResize, rayCastPos, gameObject, mask, linesUtils);
    }

    void Update()
    {
        SetSpeed();

        EnvironmentDetect();

        if (Input.GetKeyDown(KeyCode.D))
        {
            DisableEnvironmentDetect();
        }
    }

    void LateUpdate()
    {
        RearrangeInfoTags();
        PlayNearestAudio();
    }

    void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & mask) != 0) //matched one!
        {
            switch (other.gameObject.layer)
            {
                case 16:
                    {
                        Bounds bounds = new Bounds();
                        if (other.transform.CompareTag("Tree"))
                        {
                            CapsuleCollider capsCol = other as CapsuleCollider;
                            bounds.center = Quaternion.Euler(other.transform.localEulerAngles) * new Vector3(capsCol.center.x * other.transform.localScale.x, capsCol.center.y * other.transform.localScale.y, capsCol.center.z * other.transform.localScale.z);
                            bounds.size = new Vector3(capsCol.radius * 2.0f * other.transform.localScale.x, capsCol.radius * 2.0f * other.transform.localScale.y, capsCol.height * other.transform.localScale.z);
                            GameObject boundingCube = CreateBoundingCube(other.transform, other.transform.position + bounds.center, bounds.size);
                            GameObject infoTag = null;
                            if (BOUNDINGCUBE)
                                infoTag = CreateInfoTag(other.transform.position + bounds.center);
                            else
                                infoTag = CreateInfoTagAlt(other.transform.position + bounds.center);

                            UpdateIDsAndGos(boundingCube, infoTag, other, bounds);
                        }
                        else if (other.transform.CompareTag("Rock"))
                        {
                            SphereCollider sphereCol = other as SphereCollider;
                            bounds.center = Quaternion.Euler(other.transform.localEulerAngles) * new Vector3(sphereCol.center.x * other.transform.localScale.x, sphereCol.center.y * other.transform.localScale.y, sphereCol.center.z * other.transform.localScale.z);
                            bounds.size = new Vector3(sphereCol.radius * 2.0f * other.transform.localScale.x, sphereCol.radius * 2.0f * other.transform.localScale.y, sphereCol.radius * 2.0f * other.transform.localScale.z);
                            GameObject boundingCube = CreateBoundingCube(other.transform, other.transform.position + bounds.center, bounds.size);
                            GameObject infoTag = null;
                            if (BOUNDINGCUBE)
                                infoTag = CreateInfoTag(other.transform.position + bounds.center);
                            else
                                infoTag = CreateInfoTagAlt(other.transform.position + bounds.center);

                            UpdateIDsAndGos(boundingCube, infoTag, other, bounds);
                        } else if (other.transform.name.Equals("Post"))
                        {
                            BoxCollider boxCol = other as BoxCollider;
                            bounds.center = Quaternion.Euler(other.transform.localEulerAngles) * new Vector3(boxCol.center.x * other.transform.localScale.x, boxCol.center.y * other.transform.localScale.y, boxCol.center.z * other.transform.localScale.z);
                            bounds.size = new Vector3(boxCol.size.x * other.transform.localScale.x * 3.0f, boxCol.size.y * other.transform.localScale.y, boxCol.size.z * other.transform.localScale.z * 3.0f);
                            GameObject boundingCube = CreateBoundingCube(other.transform, other.transform.position + bounds.center, bounds.size);
                            GameObject infoTag = null;
                            if (BOUNDINGCUBE)
                                infoTag = CreateInfoTag(other.transform.position + bounds.center);
                            else
                                infoTag = CreateInfoTagAlt(other.transform.position + bounds.center);

                            UpdateIDsAndGos(boundingCube, infoTag, other, bounds);
                        }
                    }
                    break;
                case 17:
                    { //Signage
                        Bounds bounds = new Bounds();
                        BoxCollider boxCol = other as BoxCollider;
                        bounds.center = boxCol.center;
                        bounds.size = boxCol.size * 1.25f;
                        GameObject boundingCube = CreateBoundingCube(other.transform, other.transform.position + bounds.center, bounds.size);
                        GameObject infoTagSign = CreateInfoTagSign(other.transform.position + bounds.center);
                        UpdateIDsAndGos(boundingCube, infoTagSign, other, bounds);
                    }
                    break;
                case 12:
                    {  //obstacle
                        obstaclePrevPos = other.transform.position; //init position of obstacle spawned

                        Bounds bounds = new Bounds();
                        GameObject boundingCube = null;
                        GameObject infoTag = null;
                        if (other.transform.root.CompareTag("TrafficCar"))
                        {
                            if (other.transform.name.Equals("Body1"))
                            {
                                bounds = ComputeBounds(other.transform.root);
                                boundingCube = other.transform.Find("Cube").gameObject;
                                if (BOUNDINGCUBE)
                                    infoTag = CreateInfoTag(other.transform.position + bounds.center);
                                else
                                    infoTag = CreateInfoTagAlt(other.transform.position + bounds.center);
                                UpdateIDsAndGos(boundingCube, infoTag, other, bounds);
                                other.transform.root.gameObject.AddComponent<TrafficCarNavigationLine>();
                            }
                        }
                        else
                        {
                            bounds = ComputeBounds(other.transform);
                            boundingCube = other.transform.Find("Cube").gameObject;
                            if (BOUNDINGCUBE)
                                infoTag = CreateInfoTag(other.transform.position + bounds.center);
                            else
                                infoTag = CreateInfoTagAlt(other.transform.position + bounds.center);

                            UpdateIDsAndGos(boundingCube, infoTag, other, bounds);
                        }
                    }
                    break;

                case 8:
                    {
                        if (other.transform.name.Equals("Body1") && (other.transform.root.CompareTag("TrafficCar") || other.transform.root.CompareTag("DangerousCar")) || other.CompareTag("Rock")) 
                        {
                            Bounds bounds = ComputeBounds(other.transform.root);
                            GameObject boundingCube = other.transform.Find("Cube").gameObject;
                            GameObject infoTag = null;
                            if (BOUNDINGCUBE)
                                infoTag = CreateInfoTag(other.transform.position + bounds.center);
                            else
                                infoTag = CreateInfoTagAlt(other.transform.position + bounds.center);

                            UpdateIDsAndGos(boundingCube, infoTag, other, bounds);
                            other.transform.root.gameObject.AddComponent<TrafficCarNavigationLine>();
                        }  
                    }
                    break;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        CleanObjects(other);
    }

    GameObject CreateBoundingCube(Transform tr, Vector3 center, Vector3 size)
    {
        GameObject go = Instantiate(ResourceHandler.instance.prefabs[0]);
        go.transform.position = center;
        go.transform.rotation = tr.rotation;
        go.transform.localScale = size;
        return go;
    }

    GameObject CreateInfoTag(Vector3 center)
    {
        GameObject infoTag = Instantiate(ResourceHandler.instance.prefabs[4], center, transform.rotation); //InfoTag
        return infoTag;
    }

    GameObject CreateInfoTagAlt(Vector3 center)
    {
        GameObject infoTag = Instantiate(ResourceHandler.instance.prefabs[3], center, transform.rotation); //InfoTagAlt
        return infoTag;
    }

    GameObject CreateInfoTagSign(Vector3 center)
    {
        GameObject infoTagSign = Instantiate(ResourceHandler.instance.prefabs[5], center, transform.rotation); //InfoTag
        return infoTagSign;
    }

    void UpdateIDsAndGos(GameObject boundingCube, GameObject infoTag, Collider other, Bounds bounds)
    {
        boundingCube.GetComponent<Renderer>().enabled = false;
        infoTag.GetComponent<Canvas>().enabled = false;
        CubesAndTags cubesAndTags = new CubesAndTags();
        cubesAndTags.AddElements(boundingCube, infoTag, bounds);
        cubesAndTags.other = other; //DARIO
        IDsAndGos.Add(other.gameObject.GetInstanceID(), cubesAndTags);
    }

    Vector3 ReturnTrasl(Bounds bounds, Collider other)
    {
        Vector3 trasl = bounds.size; //infoTag trasl

        if (other.transform.CompareTag("Tree"))
        {
            trasl = new Vector3(0, -Mathf.Abs(trasl.z) * 0.25f, Mathf.Abs(trasl.x));
        }

        else if (other.transform.CompareTag("Rock")) //TrafficLight Post
        {
            trasl = new Vector3(0, Mathf.Abs(trasl.y), Mathf.Abs(trasl.x));
        }
        else if (other.transform.name.Equals("Post"))
        {
            trasl = new Vector3(0, 0, Mathf.Abs(trasl.x) * 2.0f);
        }

        return trasl;
    }

    void LoadCarHierarchy()
    {
        //Dictionary<string, RectTransform> speedPanelChildren = new Dictionary<string, RectTransform>();
        //foreach (RectTransform rt in speedPanel.transform)
        //    if (rt.name.Equals("Speed") || rt.name.Equals("Needle"))
        //        speedPanelChildren.Add(rt.name, rt);

        //speed = speedPanelChildren["Speed"];
        //needle = speedPanelChildren["Needle"];

        foreach (Transform t in transform.parent)
            if (t.name.Equals("rayCastPos"))
            {
                rayCastPos = t;
                break;
            }

        rb = transform.parent.gameObject.GetComponent<Rigidbody>();
        vehicleController = transform.parent.gameObject.GetComponent<VehicleController>();

        //rot0 = speed.localRotation; //init rotation of speed needle
    }

    void LoadDictionary()
    {
        bool dic = false; //this is a fast way to understand which type  of dictionary I have to consider in the ReadCSVFile

        ReadCSVFile(Application.streamingAssetsPath + "/angle(speedKph).csv", dic);
        ReadCSVFile(Application.streamingAssetsPath + "/sign.csv", !dic);
    }

    static Bounds ComputeBounds(Transform tr)
    {
        Quaternion currentRotation = tr.rotation;
        tr.rotation = Quaternion.Euler(0f, 0f, 0f);

        Bounds b = new Bounds();

        Renderer[] r = tr.GetComponentsInChildren<Renderer>();

        for (int ir = 0; ir < r.Length; ir++)
        {
            Renderer renderer = r[ir];
            if (ir == 0)
                b = renderer.bounds;
            else
                b.Encapsulate(renderer.bounds);
        }

        tr.rotation = currentRotation;

        return b;
    }

    float CalculateDistance(Vector3 src, Vector3 dst)
    {

        //this way distance in not completely accurate since it does not take into account the distance from the center to the maximum extension of the object
        float dist = Vector3.Distance(src, dst);
        //Debug.DrawLine(src, dst, Color.green);
        return dist;
    }

    float RiskAssessmentFormula(float subjectVehicleSpeed, float obstacleSpeed, float obstacleAcc, float dist)
    {
        float freeRunningTime = 0.4f;
        if (!(Mathf.Abs(obstacleAcc) <= 0.1f))
        {
            float obstacleComponent = Mathf.Pow(obstacleSpeed, 2) / (2 * obstacleAcc);
            return Mathf.Pow(subjectVehicleSpeed, 2) / ((2 * (dist - freeRunningTime * subjectVehicleSpeed + obstacleComponent)));
        } else
            return Mathf.Pow((subjectVehicleSpeed-obstacleSpeed), 2) / ((2 * (dist - freeRunningTime * (subjectVehicleSpeed - obstacleSpeed))));
    }
 
    void UpdateInfoTag(CubesAndTags cubesAndTags, Bounds bounds, string text, Sprite sprite, float dstToTarget, Vector3 trasl, int i)
    {
        Transform Panel = cubesAndTags.infoTag[i].transform.GetChild(0);

        if (cubesAndTags.other.gameObject.layer.Equals(16))
        {
            cubesAndTags.infoTag[i].transform.position = cubesAndTags.other.transform.position + bounds.center + cubesAndTags.infoTag[i].transform.TransformDirection(0, Vector3.up.y * trasl.y, -Vector3.forward.z * trasl.z);
            Panel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text + " KPH"; //KPH
            Panel.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = Mathf.RoundToInt(dstToTarget).ToString() + " M"; //DIST
            Panel.transform.GetChild(3).GetComponent<Image>().sprite = sprite; //sprite
        }
        else if (cubesAndTags.other.gameObject.layer.Equals(17))
        {
            cubesAndTags.infoTag[i].transform.position = cubesAndTags.other.transform.position + bounds.center + cubesAndTags.infoTag[i].transform.TransformDirection(-Vector3.forward);
            Panel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = Mathf.RoundToInt(dstToTarget).ToString() + " M"; //DIST
            Panel.transform.GetChild(1).GetComponent<Image>().sprite = sprite; //sprite
        }
        else if (cubesAndTags.other.gameObject.layer.Equals(8))
        {
            cubesAndTags.infoTag[i].transform.position = cubesAndTags.other.transform.position + cubesAndTags.infoTag[i].transform.TransformDirection(0, Vector3.up.y * bounds.size.y, 0);
            Panel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text + " KPH"; //KPH
            Panel.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = Mathf.RoundToInt(dstToTarget).ToString() + " M"; //DIST
            Panel.transform.GetChild(3).GetComponent<Image>().sprite = sprite; //sprite
        }
        else if (cubesAndTags.other.gameObject.layer.Equals(12))
        {
            cubesAndTags.infoTag[i].transform.position = cubesAndTags.other.transform.position + cubesAndTags.infoTag[i].transform.TransformDirection(0, Vector3.up.y * (bounds.size.y + cubesAndTags.infoTag[0].transform.localScale.y * cubesAndTags.infoTag[0].GetComponent<RectTransform>().rect.height * 0.5f), 0);
            Panel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text + " KPH"; //KPH
            Panel.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = Mathf.RoundToInt(dstToTarget).ToString() + " M"; //DIST
            Panel.transform.GetChild(3).GetComponent<Image>().sprite = sprite; //sprite
        }

        float distMax = 150f;
        float distMin = 5.5f;
        float scaleMax = (infoTagStartScale.x * distMax) / distMin;
        float offset = Vector3.Distance(cubesAndTags.infoTag[i].transform.position, driverCam.transform.position);
        float newScale = infoTagResize.Evaluate(offset / distMax) * scaleMax;
        cubesAndTags.infoTag[i].transform.localScale = new Vector3(newScale, newScale, newScale);
        cubesAndTags.infoTag[i].transform.LookAt((2 * cubesAndTags.infoTag[i].transform.position - driverCam.transform.position));
    }

    float CalculateObstacleSpeed(Transform tr)
    {

        //if (Application.isPlaying)
        //{
        Vector3 obstacleNextPos = tr.position;
        float speed = (obstacleNextPos - obstaclePrevPos).magnitude / Time.deltaTime; //speed works in play mode, in editor mode since you stop playing, Time.deltatime gives erratic results
        obstaclePrevPos = obstacleNextPos;
        return speed;
        //}


    }

    void SetSpeed()
    {
        //float targetAngle;
        //speed.GetComponent<Text>().text = Mathf.RoundToInt(rb.velocity.magnitude * 3.6f).ToString(); //speed in kph
        //if (CSVDictionary.TryGetValue(Mathf.RoundToInt(rb.velocity.magnitude * 3.6f), out targetAngle))
        //    needle.localRotation = Quaternion.Euler(0, 0, targetAngle) * rot0;
    }

    void ReadCSVFile(string path, bool dic)
    {
        StreamReader inputStream = new StreamReader(path);
        while (!inputStream.EndOfStream)
        {
            string line = inputStream.ReadLine();
            string[] values = line.Split(',');
            if (dic == false)
                CSVDictionary.Add(int.Parse(values[0]), float.Parse(values[1]));
            else
                CSVSignDictionary.Add(values[0], int.Parse(values[1]));
        }
        inputStream.Close();
    }

    void CleanObjects(Collider other)
    {
        if (IDsAndGos.ContainsKey(other.gameObject.GetInstanceID()))
        {
            if (other.gameObject.layer.Equals(LayerMask.NameToLayer("Traffic")))
            {
                Destroy(other.transform.root.GetComponent<TrafficCarNavigationLineUrban>()); //I destroy the script that draws the navigation Line
                try
                {
                    LineRenderer lin = other.transform.root.GetComponentInChildren<LineRenderer>();
                    GameObject go = lin.gameObject;
                    Destroy(go); //I destroy the Empty that contains the LineRenderer
                }
                catch (NullReferenceException e)
                {
                    Debug.Log("incriminated car is: " + other.transform.position);
                }
                IDsAndGos[other.gameObject.GetInstanceID()].DisableCubesAndTags();
                IDsAndGos.Remove(other.gameObject.GetInstanceID());
            }
            else if (other.gameObject.layer.Equals(LayerMask.NameToLayer("obstacle")))
            {
                IDsAndGos[other.gameObject.GetInstanceID()].DisableCubesAndTags();
                IDsAndGos.Remove(other.gameObject.GetInstanceID());
            }
            else
            {
                IDsAndGos[other.gameObject.GetInstanceID()].DestroyCubesAndTags();
                IDsAndGos.Remove(other.gameObject.GetInstanceID());
            }
        }
    }

    IEnumerator CleanObjects(float waitTime) //this is to clean those cars which are deleted when are still tracked
    {
        while (true)
        {
            yield return new WaitForSeconds(waitTime);
            List<int> keys = new List<int>(IDsAndGos.Keys);
            foreach (int key in keys)
            {
                if (IDsAndGos[key].other == null) //TrafficCar no more exists, so delete it, its LineRenderer and its infoTag
                {
                    Destroy(IDsAndGos[key].infoTag[0]); //I don't use DestroyCubesAndTags since the boundingCube is already destroyed
                    IDsAndGos.Remove(key);
                }
            }
        }
    }

    void DisableEnvironmentDetect()
    {
        GameObject instrumentCluster = transform.parent.Find("InstrumentCluster").gameObject;
        if (!dashORHud)
        {
            if (instrumentCluster.GetComponent<DashBoardController>() == null)
                instrumentCluster.AddComponent<DashBoardController>();
            else
                instrumentCluster.GetComponent<DashBoardController>().enabled = true;
        }
        else
            instrumentCluster.GetComponent<DashBoardController>().enabled = false;

        driverCam.transform.GetChild(1).GetComponent<Camera>().cullingMask ^= 1 << LayerMask.NameToLayer("Graphics"); //use leapCam in VR
        dashORHud = !dashORHud;
    }

    void CreateInfoTagResizeCurve()
    {
        float distMin = 5.5f;
        float distInter = 30f;
        float distMax = 150f;
        float scaleMin = infoTagStartScale.x;
        float scaleMax = (scaleMin * distMax) / distMin;
        float scaleInter = /*(scaleMin * distInter) / distMin*/ 0.10f * scaleMax;



        infoTagResize.AddKey(new Keyframe(distMin / distMax, scaleMin / scaleMax, 0, 0));
        infoTagResize.AddKey(new Keyframe(distInter / distMax, scaleInter / scaleMax, 0, 0));
        infoTagResize.AddKey(new Keyframe(0, scaleMin / scaleMax, 0, 0));
        infoTagResize.AddKey(new Keyframe(1, scaleMin / scaleMax, 0, 0));
    }

    bool LineSegmentsIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector3 p4, out Vector2 intersection)
    {
        intersection = Vector2.zero;

        var d = (p2.x - p1.x) * (p4.y - p3.y) - (p2.y - p1.y) * (p4.x - p3.x);

        if (d == 0.0f)
        {
            return false;
        }

        var u = ((p3.x - p1.x) * (p4.y - p3.y) - (p3.y - p1.y) * (p4.x - p3.x)) / d;
        var v = ((p3.x - p1.x) * (p2.y - p1.y) - (p3.y - p1.y) * (p2.x - p1.x)) / d;

        if (u < 0.0f || u > 1.0f || v < 0.0f || v > 1.0f)
        {
            return false;
        }

        intersection.x = p1.x + u * (p2.x - p1.x);
        intersection.y = p1.y + u * (p2.y - p1.y);

        return true;
    }

    void RearrangeInfoTags()
    {
        List<CubesAndTags> orderedIDsAndGos = IDsAndGos.Values.Where(x => x.infoTag[0].GetComponent<Canvas>().enabled == true).OrderByDescending(x => x.infoTag[0].transform.position.y).ToList();

        foreach (var item1 in orderedIDsAndGos)
        {
            foreach (var item2 in orderedIDsAndGos)
            {
                if (item1 != item2)
                {
                    var offset = item2.infoTag[0].transform.position - item1.infoTag[0].transform.position;
                    float mag = offset.sqrMagnitude;
                    if (mag <= 4)
                    {
                        var RectTr1 = item1.infoTag[0].GetComponent<RectTransform>();
                        var RectTr2 = item2.infoTag[0].GetComponent<RectTransform>();
                        var rect1 = GetWorldRect2(RectTr1);
                        var rect2 = GetWorldRect2(RectTr2);
                        if (rect1.OverlapsAlt(rect2, true)/* || rect2.OverlapsAlt(rect1, true)*/)
                        {
                            float item1Height = rect1.height; /*item1.infoTag[i].GetComponent<RectTransform>().rect.height * item1.infoTag[i].transform.localScale.y*/;
                            float item2Height = rect2.height; /*item2.infoTag[i].GetComponent<RectTransform>().rect.height * item2.infoTag[i].transform.localScale.y*/;
                            //var p = item1.infoTag[i].transform.position;
                            //p.y += 3.0f;
                            item1.infoTag[0].transform.position += item1.infoTag[0].transform.up * (item2Height + item1Height) * 0.5f;
                        }
                    }
                }
            }
        }
    }

    public Rect GetWorldRect2(RectTransform rt)
    {
        Vector3[] _fourCorners = new Vector3[4];
        rt.GetWorldCorners(_fourCorners);
        //Debug.DrawLine(_fourCorners[0], _fourCorners[1], Color.green);
        //Debug.DrawLine(_fourCorners[1], _fourCorners[2], Color.green);
        //Debug.DrawLine(_fourCorners[2], _fourCorners[3], Color.green);
        //Debug.DrawLine(_fourCorners[3], _fourCorners[0], Color.green);
        Vector2 topLeft = _fourCorners[0];
        float width = rt.rect.width * rt.localScale.x;
        float height = rt.rect.height * rt.localScale.y;
        Rect rect_ = new Rect(topLeft.x, topLeft.y, width, height);
        return rect_;
    }

    void PlayAudio(AudioSource audio, bool blink)
    {
        if (blink == true)
            audio.Play();
        else
            audio.Stop();
    }

    void EnvironmentDetect()
    {
        List<CubesAndTags> objectsList = new List<CubesAndTags>(IDsAndGos.Values);
        for (int i = 0; i < objectsList.Count; i++)
        {
            if (objectsList[i].other != null)
            {
                float dist = CalculateDistance(rayCastPos.position, objectsList[i].other.transform.position); //this distance does not include bounds since it cannot always be precomputed, for example, this is the case of moving objects

                switch (objectsList[i].other.gameObject.layer)
                {
                    case 16:
                        {
                            Bounds bounds = objectsList[i].bounds[0];
                            Sprite sprite = objectsList[i].other.GetComponent<Image>().sprite;    
                            if (BOUNDINGCUBE)
                                riskAssessment.BoundingCubeLerperPCH(objectsList[i], bounds, sprite, ReturnTrasl(bounds, objectsList[i].other), 0);
                            //else
                            //    riskAssessment.InfoTagWarn(objectsList[i], bounds, sprite, ReturnTrasl(bounds, objectsList[i].other), 0);
                        }
                        break;
                    case 17: //Signage
                        { 
                            float dotProduct = Vector3.Dot(gameObject.transform.TransformDirection(Vector3.forward), objectsList[i].other.transform.TransformDirection(Vector3.up));
                            Animator anim = objectsList[i].infoTag[0].GetComponent<Animator>();
                            if (dotProduct >= -1 && dotProduct <= -0.707f)
                            {
                                Bounds bounds = objectsList[i].bounds[0];
                                int spriteIndex = CSVSignDictionary[objectsList[i].other.name];
                                objectsList[i].boundingCube[0].GetComponent<Renderer>().enabled = true;
                                objectsList[i].infoTag[0].GetComponent<Canvas>().enabled = true;
                                UpdateInfoTag(objectsList[i], bounds, "", ResourceHandler.instance.sprites[spriteIndex], dist, Vector3.zero, 0);

                                if (objectsList[i].other.transform.name.Equals("StopSign") || objectsList[i].other.transform.parent.name.Equals("SpeedLimit") || objectsList[i].other.transform.parent.name.Equals("FallenRocksSign"))
                                {
                                    if (dist <= 40)
                                        anim.SetBool("Blink", true);
                                    else
                                        anim.SetBool("Blink", false);
                                }    
                            }
                            else
                            {
                                objectsList[i].boundingCube[0].GetComponent<Renderer>().enabled = false;
                                objectsList[i].infoTag[0].GetComponent<Canvas>().enabled = false;

                                if(objectsList[i].other.transform.name.Equals("StopSign") || objectsList[i].other.transform.parent.name.Equals("SpeedLimit") || objectsList[i].other.transform.parent.name.Equals("FallenRocksSign"))
                                    anim.SetBool("Blink", false);
                            }
                        }
                        break;
                    case 12:
                        {
                            Bounds bounds = objectsList[i].bounds[0];
                            Sprite sprite = objectsList[i].other.GetComponent<Image>().sprite;
                            if (/*BOUNDINGCUBE*/!objectsList[i].other.CompareTag("Rock"))
                                riskAssessment.BoundingCubeLerperObstaclePCH(objectsList[i], bounds, CalculateObstacleSpeed(objectsList[i].other.transform), 0, sprite, Vector3.zero, 0);
                            else
                                riskAssessment.BoundingCubeLerperDangerousCarPCH(objectsList[i], bounds, 0, ResourceHandler.instance.visualisationVars.obstacleDistToWarn, 25f, sprite, Vector3.zero, 0);
                        }
                        break;
                    case 8:
                        {
                            Debug.DrawLine(objectsList[i].other.transform.position, rayCastPos.position, Color.green);
                            Bounds bounds = objectsList[i].bounds[0];
                            TrafPCH trafPCH = objectsList[i].other.transform.root.GetComponent<TrafPCH>();
                            if (trafPCH != null)
                            {
                                float speed = trafPCH.currentSpeed;
                                float acceleration = trafPCH.accelerazione;
                                Sprite sprite = objectsList[i].other.GetComponent<Image>().sprite;
                                if (/*BOUNDINGCUBE*/objectsList[i].other.transform.root.CompareTag("DangerousCar") ) 
                                    if (trafPCH.currentWaypointIndex < 1970) //overtaking Car
                                        riskAssessment.BoundingCubeLerperDangerousCarPCH(objectsList[i], bounds, speed, ResourceHandler.instance.visualisationVars.obstacleDistToWarn, 25f, sprite, Vector3.zero, 0);
                                    else
                                        riskAssessment.BoundingCubeLerperDangerousCarPCH(objectsList[i], bounds, speed, ResourceHandler.instance.visualisationVars.DangerousCarDistToWarn, 25f, sprite, Vector3.zero, 0);
                                //riskAssessment.BoundingCubeLerperPCH(objectsList[i], bounds, speed, acceleration, sprite, Vector3.zero, 0);
                                else
                                    riskAssessment.BoundingCubeLerperPCH(objectsList[i], bounds, speed, acceleration, sprite, Vector3.zero, 0);

                            } 
                        }
                        break;
                }

            }
        }
    }

    void PlayNearestAudio()
    {
        List<CubesAndTags> audioIDsAndGos = IDsAndGos.Values.Where(x => x.boundingCube[0].GetComponent<AudioSource>().isPlaying == true).ToList();

        CubesAndTags nearest = null; //nearest object whose AudioSource is playing
        float nearDist = 9999;
        foreach (var item in audioIDsAndGos)
        {
            float thisDist = (transform.position - item.other.transform.position).sqrMagnitude; //this is squaredMagnitude i.e. magnitude without square root
            if (thisDist < nearDist)
            {
                nearDist = thisDist;
                nearest = item;
            }
        }

        foreach (var item in audioIDsAndGos)
            if (item != nearest)
                item.boundingCube[0].GetComponent<AudioSource>().Stop();
    }
}
