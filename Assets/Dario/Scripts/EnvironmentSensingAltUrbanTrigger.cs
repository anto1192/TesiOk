using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using MKGlowSystem;
using System.Linq;
using TMPro;


public class EnvironmentSensingAltUrbanTrigger : MonoBehaviour
{
    private bool BOUNDINGCUBE = true; //this is to select the appropriate function in OnTriggerStay(): BoundingCubeLerped or InfoTagEnhanced
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
    
    private LayerMask mask;

    private Transform rayCastPos;
    private Transform speed;
    //private RectTransform needle;
    private Rigidbody rb;
    private VehicleController vehicleController;
    private DriverCamera driverCam;
    private GameObject leapCam;

    //public GameObject SpeedPanel { set { speedPanel = value; } }
    public DriverCamera DriverCam { set { driverCam = value; } }
    public GameObject LeapCam { set { leapCam = value; } }
    public Transform RayCastPos { get { return rayCastPos; }}
    public Rigidbody Rb { get { return rb; } }
    public VehicleController Vc { get { return vehicleController; } }
    public LayerMask Mask { get { return mask; } }

    void Awake()
    {
        LoadDictionary();
    }

    void Start()
    {
        LoadCarHierarchy();

        infoTagStartScale = ResourceHandler.instance.prefabs[3].transform.localScale;

        CreateInfoTagResizeCurve();

        linesUtils.CenterLineLerper(Color.red, Color.yellow, Color.green, new Color32(0x00, 0x80, 0xFF, 0x00));

        mask = (1 << LayerMask.NameToLayer("Traffic")) | (1 << LayerMask.NameToLayer("obstacle")) | (1 << LayerMask.NameToLayer("EnvironmentProp")) | (1 << LayerMask.NameToLayer("Signage") | 1 << LayerMask.NameToLayer("Roads")); //restrict OnTriggerEnter only to the layers of interest

        StartCoroutine(CleanObjects(0.25f));

        riskAssessment = new RiskAssessment(infoTagStartScale, driverCam, infoTagResize, rayCastPos, gameObject, mask, linesUtils);
    }

    void Update()
    {
        SetSpeed();
        
        EnvironmentDetect();

        if (Input.GetKeyDown(KeyCode.D))
            DisableEnvironmentDetect();
    }

    void LateUpdate()
    {
        RearrangeInfoTags();
        PlayNearestAudio();
    }

    //void OnDrawGizmos()
    //{
    //    //DebugExtension.DrawCone(rayCastPos.position, rayCastPos.forward * 150f, Color.magenta, 15);
    //}

    void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & mask) != 0) //matched one!
        {
            switch (other.gameObject.layer)
            {
                case 16:
                    {
                        Bounds bounds = new Bounds();
                        if (other.transform.CompareTag("ParkedCar"))
                        {
                            BoxCollider boxCol = other as BoxCollider;
                            bounds.center = Quaternion.Euler(other.transform.localEulerAngles) * boxCol.center;
                            bounds.size = boxCol.size;
                            GameObject boundingCube = CreateBoundingCube(other.transform, other.transform.position + bounds.center, bounds.size);
                            GameObject infoTag = null;
                            if (BOUNDINGCUBE)
                                infoTag = CreateInfoTag(other.transform.position + bounds.center);
                            else                            
                                infoTag = CreateInfoTagAlt(other.transform.position + bounds.center);
                            
                            UpdateIDsAndGos(boundingCube, infoTag, other, bounds);
                        }
                        else if (other.transform.CompareTag("Sign"))
                        {
                            BoxCollider boxCol = other as BoxCollider;
                            bounds.center = boxCol.center;
                            bounds.size = boxCol.size * 1.25f;
                            GameObject boundingCube = CreateBoundingCube(other.transform, other.transform.position + bounds.center, bounds.size);
                            GameObject infoTagSign = CreateInfoTagSign(other.transform.position + bounds.center);
                            UpdateIDsAndGos(boundingCube, infoTagSign, other, bounds);
                        }
                        switch (other.transform.name)
                        {
                            case "streetlight":
                                {
                                    CapsuleCollider capsCol = other as CapsuleCollider;
                                    bounds.center = Quaternion.Euler(other.transform.localEulerAngles) * capsCol.center;
                                    bounds.size = new Vector3(capsCol.radius * 4.0f, capsCol.radius * 4.0f, capsCol.height);
                                    GameObject boundingCube = CreateBoundingCube(other.transform, other.transform.position + bounds.center, bounds.size);
                                    GameObject infoTag = null;
                                    if (BOUNDINGCUBE)
                                        infoTag = CreateInfoTag(other.transform.position + bounds.center);
                                    else
                                        infoTag = CreateInfoTagAlt(other.transform.position + bounds.center);
                                    
                                    UpdateIDsAndGos(boundingCube, infoTag, other, bounds);
                                }
                                break;
                            case "tree_dec01":
                                {
                                    Transform oldParent = other.transform.parent;
                                    other.transform.parent = null; //I am forced to unparent since I can't take into account the parent scale in the bounding box computation
                                    CapsuleCollider capsCol = other as CapsuleCollider;
                                    bounds.center = Quaternion.Euler(other.transform.localEulerAngles) * new Vector3(capsCol.center.x * other.transform.localScale.x, capsCol.center.y * other.transform.localScale.y, capsCol.center.z * other.transform.localScale.z);
                                    bounds.size = new Vector3(capsCol.radius * 2.0f * other.transform.localScale.x, capsCol.radius * 2.0f * other.transform.localScale.y, capsCol.height * other.transform.localScale.z);
                                    other.transform.parent = oldParent;
                                    GameObject boundingCube = CreateBoundingCube(other.transform, other.transform.position + bounds.center, bounds.size);
                                    GameObject infoTag = null;
                                    if (BOUNDINGCUBE)
                                        infoTag = CreateInfoTag(other.transform.position + bounds.center);
                                    else
                                        infoTag = CreateInfoTagAlt(other.transform.position + bounds.center);
                                    
                                    UpdateIDsAndGos(boundingCube, infoTag, other, bounds);
                                }
                                break;

                            case "Post":
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
                                break;

                            case "Dumpster":

                            case "busstop":

                            case "barrier_concrete":

                            case "barrier_metal":

                            case "Lamppost":

                            case "Table_For2":

                            case "Table_For4":
                                {
                                    BoxCollider boxCol = other as BoxCollider;
                                    bounds.center = Quaternion.Euler(other.transform.localEulerAngles) * new Vector3(boxCol.center.x * other.transform.localScale.x, boxCol.center.y * other.transform.localScale.y, boxCol.center.z * other.transform.localScale.z);
                                    bounds.size = new Vector3(boxCol.size.x * other.transform.localScale.x, boxCol.size.y * other.transform.localScale.y, boxCol.size.z * other.transform.localScale.z);
                                    GameObject boundingCube = CreateBoundingCube(other.transform, other.transform.position + bounds.center, bounds.size);
                                    GameObject infoTag = null;
                                    if (BOUNDINGCUBE)
                                        infoTag = CreateInfoTag(other.transform.position + bounds.center);
                                    else
                                        infoTag = CreateInfoTagAlt(other.transform.position + bounds.center);
                                    
                                    UpdateIDsAndGos(boundingCube, infoTag, other, bounds);
                                }
                                break;
                        }
                    }
                    break;
                case 17:
                    { //Signage
                        Bounds bounds = new Bounds();
                        if (other.transform.parent.parent.name.Equals("StreetSigns"))
                        {
                            break;
                        }

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
                        
                        Bounds bounds = new Bounds();
                        GameObject boundingCube = null;
                        GameObject infoTag = null;
                        if  (other.transform.root.CompareTag("TrafficCar"))
                        {
                            if (other.transform.name.Equals("Body1")) {
                                bounds = ComputeBounds(other.transform.root);
                                try
                                {
                                    boundingCube = other.transform.Find("Cube").gameObject;
                                }
                                catch (NullReferenceException)
                                {
                                    Debug.Log("object incriminated is: " + other.transform.position);
                                }
                                if (BOUNDINGCUBE)
                                    infoTag = CreateInfoTag(other.transform.position + bounds.center);
                                else
                                    infoTag = CreateInfoTagAlt(other.transform.position + bounds.center);
                                UpdateIDsAndGos(boundingCube, infoTag, other, bounds);
                                other.transform.root.gameObject.AddComponent<TrafficCarNavigationLineUrban>();
                            }
                        }   
                        else
                        {
                            bounds = ComputeBounds(other.transform);
                            try
                            {
                                boundingCube = other.transform.Find("Cube").gameObject;
                            }
                            catch (NullReferenceException)
                            {
                                Debug.Log("object incriminated is: " + other.transform.position);
                            }
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
                        if (other.transform.name.Equals("Body1") && (other.transform.root.CompareTag("TrafficCar") || other.transform.root.CompareTag("TrafficScooter")))
                        {
                            Bounds bounds = ComputeBounds(other.transform.root);
                            GameObject boundingCube = other.transform.Find("Cube").gameObject;
                            GameObject infoTag = null;
                            if (BOUNDINGCUBE)
                                infoTag = CreateInfoTag(other.transform.position + bounds.center);
                            else
                                infoTag = CreateInfoTagAlt(other.transform.position + bounds.center);

                            UpdateIDsAndGos(boundingCube, infoTag, other, bounds);
                            other.transform.root.gameObject.AddComponent<TrafficCarNavigationLineUrban>();
                        }
                    }
                    break;
                case 18: //Roads
                    {
                        if (other.transform.name.StartsWith("TrafficLight"))
                        {
                            CubesAndTags cubesAndTags = new CubesAndTags();

                            foreach (Transform t in other.transform)
                                if (t.name.StartsWith("Light") && t.gameObject.activeInHierarchy) //there are some trafficLights not active so I have to exclude them
                                {
                                    Bounds bounds = ComputeBounds(t);
                                    GameObject boundingCube = CreateBoundingCube(t, bounds.center, bounds.size);
                                    boundingCube.transform.SetParent(t.GetComponentInChildren<MeshFilter>().gameObject.transform);
                                    boundingCube.transform.localPosition = Vector3.zero;
                                    boundingCube.transform.localScale *= 1.25f;
                                    GameObject infoTag = CreateInfoTag(bounds.center);
                                    boundingCube.GetComponent<Renderer>().enabled = false;
                                    infoTag.GetComponent<Canvas>().enabled = false;
                                    cubesAndTags.other = other; //DARIO
                                    cubesAndTags.AddElements(boundingCube, infoTag, bounds);
                                }

                            Bounds boundsPost = new Bounds();
                            BoxCollider boxCol = other as BoxCollider;
                            boundsPost.center = Quaternion.Euler(other.transform.localEulerAngles) * new Vector3(boxCol.center.x * other.transform.localScale.x, boxCol.center.y * other.transform.localScale.y, boxCol.center.z * other.transform.localScale.z);
                            boundsPost.size = new Vector3(boxCol.size.x * other.transform.localScale.x * 1.25f, boxCol.size.y * other.transform.localScale.y * 1.25f, boxCol.size.z * other.transform.localScale.z);
                            GameObject boundingCubePost = CreateBoundingCube(other.transform, other.transform.position + boundsPost.center, boundsPost.size);
                            GameObject infoTagPost = null;
                            if (BOUNDINGCUBE)
                                infoTagPost = CreateInfoTag(other.transform.position + boundsPost.center);
                            else
                                infoTagPost = CreateInfoTagAlt(other.transform.position + boundsPost.center);
                            
                            boundingCubePost.GetComponent<Renderer>().enabled = false;
                            infoTagPost.GetComponent<Canvas>().enabled = false;
                            cubesAndTags.AddElements(boundingCubePost, infoTagPost, boundsPost);
                            if (cubesAndTags.boundingCube.Count != 0) //maybe this is no more requested since at least the Post is added, the trafficLight depends
                                IDsAndGos.Add(other.gameObject.GetInstanceID(), cubesAndTags);
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

        if (other.transform.CompareTag("ParkedCar"))
        {
            trasl = new Vector3(0, Mathf.Abs(trasl.y), 0);
        }

        if (other.transform.gameObject.layer.Equals(18)) //TrafficLight Post
        {
            trasl = new Vector3(0, -Mathf.Abs(trasl.z) * 0.25f, Mathf.Abs(trasl.x));
        }

        switch (other.transform.name)
        {
            case "streetlight":

            case "Lamppost":

            case "tree_dec01":
                {
                    
                    trasl = new Vector3(0, -Mathf.Abs(trasl.z) * 0.25f, Mathf.Abs(trasl.x));
                }
                break;

            case "Post":
                {
                    trasl = new Vector3(0, 0, Mathf.Abs(trasl.x) * 2.0f);
                }
                break;

            case "Dumpster":

            case "barrier_concrete":
                {
                    trasl = new Vector3(0, Mathf.Abs(trasl.y), Mathf.Abs(trasl.x));
                }
                break;

            case "busstop":
                {
                    trasl = new Vector3(0, 0, Mathf.Sqrt(Mathf.Pow(trasl.x, 2) + Mathf.Pow(trasl.y, 2)));
                }
                break;

            case "barrier_metal":
                {
                    trasl = new Vector3(0, Mathf.Abs(trasl.y), Mathf.Abs(trasl.x));
                }
                break;

            case "Table_For2":

            case "Table_For4":
                {
                    trasl = new Vector3(0, Mathf.Abs(trasl.y), Mathf.Abs(trasl.x));
                }
                break;
        }
        return trasl;
    }

    void LoadCarHierarchy()
    {
        Transform InstrumentCluster = null;

        Dictionary<string, Transform> speedPanelChildren = new Dictionary<string, Transform>();
        foreach (Transform t in transform.parent)
            if (t.name.Equals("rayCastPos") || t.name.Equals("InstrumentCluster"))
                speedPanelChildren.Add(t.name, t);

        rayCastPos = speedPanelChildren["rayCastPos"];
        InstrumentCluster = speedPanelChildren["InstrumentCluster"];

        speed = InstrumentCluster.transform.GetChild(0).GetChild(0).GetChild(0);
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
        float freeRunningTime = 1.5f;
        float obstacleComponent = 0;
        if (obstacleAcc != 0)
            obstacleComponent = Mathf.Pow(obstacleSpeed, 2) / (2 * obstacleAcc);
        return Mathf.Pow(subjectVehicleSpeed, 2) / ((2 * (dist - freeRunningTime * subjectVehicleSpeed + obstacleComponent)));
    }

    float CalculateObstacleSpeed(CubesAndTags cubesAndTags)
    {
        //if (Application.isPlaying)
        //{
        var t = Time.time;
        if (t >= cubesAndTags.obstacleNextTime)
        {
            Vector3 obstacleNextPos = cubesAndTags.other.transform.position;
            cubesAndTags.obstaclePrevSpeed = (obstacleNextPos - cubesAndTags.obstaclePrevPos).magnitude / (t - cubesAndTags.obstaclePrevTime); //speed works in play mode, in editor mode since you stop playing, Time.deltatime gives erratic results
            cubesAndTags.obstaclePrevPos = obstacleNextPos; 
            cubesAndTags.obstaclePrevTime = t;
            cubesAndTags.obstacleNextTime = t + 0.5f;
            return cubesAndTags.obstaclePrevSpeed;
        }
        else
            return cubesAndTags.obstaclePrevSpeed;
        //}
    }

    void SetSpeed()
    {
        float speedToShow = Mathf.RoundToInt(rb.velocity.magnitude * 3.6f);
        TextMeshProUGUI textMeshProUGUI = speed.GetComponent<TextMeshProUGUI>();
        if ((speedToShow > 50f && vehicleController.accellInput != 0))
            //speedToShow = 50f;
            textMeshProUGUI.color = Color.red;
        else
            textMeshProUGUI.color = Color.white;
        speed.GetComponent<TextMeshProUGUI>().text = speedToShow.ToString(); //speed in kph
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
            } else if (other.gameObject.layer.Equals(LayerMask.NameToLayer("obstacle")))
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
            if (instrumentCluster.GetComponent<DashBoardControllerUrban>() == null)
                instrumentCluster.AddComponent<DashBoardControllerUrban>();
            else
                instrumentCluster.GetComponent<DashBoardControllerUrban>().enabled = true;
        }
        else
            instrumentCluster.GetComponent<DashBoardControllerUrban>().enabled = false;

        Camera camActive = driverCam.transform.GetChild(1).GetComponent<Camera>();
        if (camActive.enabled)
            camActive.cullingMask ^= 1 << LayerMask.NameToLayer("Graphics"); //use leapCam in VR
        else
            leapCam.GetComponent<Camera>().cullingMask ^= 1 << LayerMask.NameToLayer("Graphics"); //use leapCam in VR
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

    void PlayNearestAudio()
    {
        List<CubesAndTags> audioIDsAndGos = IDsAndGos.Values.Where(x => x.boundingCube[0] != null).Where(x => x.boundingCube[0].GetComponent<AudioSource>().isPlaying == true).ToList();
        
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
                            if (!objectsList[i].other.transform.CompareTag("Sign"))
                            {
                                Sprite sprite;
                                if (objectsList[i].other.transform.name.Equals("tree_dec01"))
                                    sprite = ResourceHandler.instance.sprites[17];
                                else
                                    sprite = objectsList[i].other.GetComponent<Image>().sprite;
                                if (BOUNDINGCUBE)
                                    riskAssessment.BoundingCubeLerperSF(objectsList[i], bounds, sprite, ReturnTrasl(bounds, objectsList[i].other), 0);
                                //else
                                //    riskAssessment.InfoTagWarn(objectsList[i], bounds, sprite, ReturnTrasl(bounds, objectsList[i].other), 0);
                            }
                            else
                            {
                                float dotProduct = Vector3.Dot(gameObject.transform.TransformDirection(Vector3.forward), objectsList[i].other.transform.TransformDirection(Vector3.up));
                                if (dotProduct >= -1 && dotProduct <= -0.707f/* && kvp.Key != null*/)
                                {
                                    objectsList[i].boundingCube[0].GetComponent<Renderer>().enabled = true;
                                    objectsList[i].infoTag[0].GetComponent<Canvas>().enabled = true;
                                    int spriteIndex = CSVSignDictionary[objectsList[i].other.name];
                                    riskAssessment.UpdateInfoTag(objectsList[i], bounds, "", ResourceHandler.instance.sprites[spriteIndex], dist, Vector3.zero, 0); 
                                }
                                else
                                {
                                    objectsList[i].boundingCube[0].GetComponent<Renderer>().enabled = false;
                                    objectsList[i].infoTag[0].GetComponent<Canvas>().enabled = false;
                                }
                            }
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
                                riskAssessment.UpdateInfoTag(objectsList[i], bounds, "", ResourceHandler.instance.sprites[spriteIndex], dist, Vector3.zero, 0);

                                if (objectsList[i].other.transform.name.Equals("StopSign"))
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

                                if (objectsList[i].other.transform.name.Equals("StopSign"))
                                    anim.SetBool("Blink", false);
                            }
                        }
                        break;
                    case 12:
                        {
                            Bounds bounds = objectsList[i].bounds[0];
                            Sprite sprite = objectsList[i].other.GetComponent<Image>().sprite;
                            if (/*BOUNDINGCUBE*/objectsList[i].other.CompareTag("Obstacle"))
                                riskAssessment.BoundingCubeLerperObstacleDefSF(objectsList[i], bounds, CalculateObstacleSpeed(objectsList[i]), 0, sprite, Vector3.zero, 0);
                            else
                            {
                                Vector3 targetPoint = new Vector3(objectsList[i].other.transform.position.x, rayCastPos.transform.position.y, objectsList[i].other.transform.position.z);
                                float dstToTarget = Vector3.Distance(rayCastPos.position, targetPoint);
                                objectsList[i].boundingCube[0].GetComponent<Renderer>().enabled = true;
                                objectsList[i].infoTag[0].GetComponent<Canvas>().enabled = true;
                                riskAssessment.UpdateInfoTag(objectsList[i], bounds, Mathf.RoundToInt(CalculateObstacleSpeed(objectsList[i]) * 3.6f).ToString(), sprite, dstToTarget, Vector3.zero, 0);
                            }
                                //riskAssessment.InfoTagWarnObstacle2(objectsList[i], bounds, CalculateObstacleSpeed(objectsList[i].other.transform), 0, sprite, Vector3.zero, 0);

                        }
                        break;
                    case 8:
                        {
                            Debug.DrawLine(objectsList[i].other.transform.position, rayCastPos.position, Color.green);
                            Bounds bounds = objectsList[i].bounds[0];
                            TrafAIMotor trafAIMotor = objectsList[i].other.transform.root.GetComponent<TrafAIMotor>();
                            if (trafAIMotor != null)
                            {
                                float speed = trafAIMotor.currentSpeed;
                                float acceleration = trafAIMotor.accelerazione;
                                Sprite sprite = objectsList[i].other.GetComponent<Image>().sprite;
                                if (/*BOUNDINGCUBE*/objectsList[i].other.transform.root.CompareTag("TrafficCar"))
                                    riskAssessment.BoundingCubeLerperSF(objectsList[i], bounds, speed, acceleration, sprite, Vector3.zero, 0);
                                else
                                    riskAssessment.BoundingCubeLerperScooterSF(objectsList[i], bounds, speed, sprite, Vector3.zero, 0);
                                //    riskAssessment.InfoTagWarn(objectsList[i], bounds, speed, acceleration, sprite, Vector3.zero, 0);
                            }
                        }
                        break;
                    case 18: //Roads
                        {
                            if (objectsList[i].bounds.Count == 3)
                            {
                                float dotProduct = Vector3.Dot(gameObject.transform.TransformDirection(Vector3.forward), objectsList[i].other.transform.TransformDirection(Vector3.right));
                                if (dotProduct >= -1 && dotProduct <= -0.707f)
                                {
                                    TrafAIMotor trafAIMotor = transform.root.GetComponent<TrafAIMotor>();
                                    if (trafAIMotor != null) //Among the various potential trafficLights which can be traced by OnTriggerEnter I am sure by this test that I Am considering the next one and not the others
                                    {
                                        if (trafAIMotor.nextEntry50 != null)
                                        {
                                            trafLight = trafAIMotor.nextEntry50.light;
                                        }
                                        if (trafLight != null && trafLight.gameObject.GetInstanceID().Equals(objectsList[i].other.gameObject.GetInstanceID()))
                                        {
                                            Transform Panel = objectsList[i].infoTag[0].transform.GetChild(0);
                                            Panel.transform.GetChild(2).GetComponent<TextMeshProUGUI>().fontSize = 110;

                                            TrafLightState currentState = trafLight.State;
                                            Animator anim = objectsList[i].infoTag[0].GetComponent<Animator>();
                                            if (currentState.Equals(TrafLightState.GREEN))
                                            {
                                                Panel.transform.GetChild(3).GetComponent<Image>().sprite = ResourceHandler.instance.sprites[25];
                                                Panel.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "GO";
                                                Panel.transform.GetChild(2).GetComponent<TextMeshProUGUI>().color = new Color32(0x3B, 0xAA, 0x34, 0xFF); 

                                                anim.SetInteger("trafLightState", (int)TrafLightState.GREEN);

                                            }
                                            else if (currentState.Equals(TrafLightState.YELLOW))
                                            {
                                                Panel.transform.GetChild(3).GetComponent<Image>().sprite = ResourceHandler.instance.sprites[26];
                                                Vector3 offset = trafLight.transform.position - rayCastPos.position;
                                                float mag = offset.sqrMagnitude;
                                                if (mag >= 1600 || trafAIMotor.hasStopTarget) //40m 
                                                {
                                                    Panel.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "STOP";
                                                }
                                                //else if ()
                                                //{
                                                //    Panel.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "STOP";
                                                //}
                                                else
                                                {
                                                    Panel.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "GO";
                                                }
                                                Panel.transform.GetChild(2).GetComponent<TextMeshProUGUI>().color = new Color32(0xFE, 0xED, 0x01, 0xFF); 
                                                anim.SetInteger("trafLightState", (int)TrafLightState.YELLOW);

                                            }
                                            else
                                            {
                                                Panel.transform.GetChild(3).GetComponent<Image>().sprite = ResourceHandler.instance.sprites[27];
                                                Panel.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "STOP";
                                                Panel.transform.GetChild(2).GetComponent<TextMeshProUGUI>().color = new Color32(0xE3, 0x07, 0x13, 0xFF);

                                                anim.SetInteger("trafLightState", (int)TrafLightState.RED);
                                                
                                            }


                                            //IDsAndGos[other.gameObject.GetInstanceID()].infoTag[i].transform.rotation = Quaternion.Euler(0f, 90f, 0f) * IDsAndGos[other.gameObject.GetInstanceID()].boundingCube[0].transform.rotation;
                                            float infoTagOffset = Vector3.Distance(objectsList[i].boundingCube[0].transform.position, objectsList[i].boundingCube[1].transform.position);
                                            objectsList[i].infoTag[0].transform.position = objectsList[i].boundingCube[0].transform.position + objectsList[i].infoTag[0].transform.TransformDirection(-Vector3.right.x * (infoTagOffset / 2), -Vector3.up.y * 1.5f, -Vector3.forward.z);

                                            float distMax = 150f;
                                            float distMin = 5.5f;
                                            float scaleMax = (infoTagStartScale.x * distMax) / distMin;
                                            float offset2 = Vector3.Distance(objectsList[i].infoTag[0].transform.position, driverCam.transform.position);
                                            float newScale = infoTagResize.Evaluate(offset2 / distMax) * scaleMax;
                                            objectsList[i].infoTag[0].transform.localScale = new Vector3(newScale, newScale, newScale);
                                            objectsList[i].infoTag[0].transform.LookAt((2 * objectsList[i].infoTag[0].transform.position - driverCam.transform.position));

                                            objectsList[i].boundingCube[0].GetComponent<Renderer>().enabled = objectsList[i].boundingCube[1].GetComponent<Renderer>().enabled = true;
                                            objectsList[i].infoTag[0].GetComponent<Canvas>().enabled = true;
                                        }
                                        else
                                        {
                                            objectsList[i].boundingCube[0].GetComponent<Renderer>().enabled = false;
                                            objectsList[i].infoTag[0].GetComponent<Canvas>().enabled = false;
                                            objectsList[i].boundingCube[1].GetComponent<Renderer>().enabled = false;
                                            objectsList[i].infoTag[1].GetComponent<Canvas>().enabled = false;
                                        }
                                    }
                                    else
                                    {
                                        objectsList[i].boundingCube[0].GetComponent<Renderer>().enabled = false;
                                        objectsList[i].infoTag[0].GetComponent<Canvas>().enabled = false;
                                        objectsList[i].boundingCube[1].GetComponent<Renderer>().enabled = false;
                                        objectsList[i].infoTag[1].GetComponent<Canvas>().enabled = false;
                                    }

                                }
                                else
                                {
                                    objectsList[i].boundingCube[0].GetComponent<Renderer>().enabled = false;
                                    objectsList[i].infoTag[0].GetComponent<Canvas>().enabled = false;
                                    objectsList[i].boundingCube[1].GetComponent<Renderer>().enabled = false;
                                    objectsList[i].infoTag[1].GetComponent<Canvas>().enabled = false;
                                }
                                Bounds boundsPost = objectsList[i].bounds[2];
                                Sprite sprite = objectsList[i].other.GetComponent<Image>().sprite;
                                riskAssessment.BoundingCubeLerperSF(objectsList[i], boundsPost, sprite, ReturnTrasl(boundsPost, objectsList[i].other), 2);
                            }
                            else
                            {
                                Bounds boundsPost = objectsList[i].bounds[0];
                                Sprite sprite = objectsList[i].other.GetComponent<Image>().sprite;
                                riskAssessment.BoundingCubeLerperSF(objectsList[i], boundsPost, sprite, ReturnTrasl(boundsPost, objectsList[i].other), 0);
                            }
                        }
                        break;
                }       
            }
        }
    }

    //Transform CalculatePointDistance(Collider[] colls, string pointType)
    //{
    //    Transform nearest = null; //in URBAN I considr the nearest point otherwise the furthest would be a point located in the near lanes
    //    float nearDist = 9999;
    //    for (int i = 0; i < colls.Length; ++i)
    //    {
    //        if (colls[i].tag.Equals(pointType))
    //        { //consider only Centerpoints and ignore other Colliders that belong to Graphics 
    //            float thisDist = (transform.position - cacheColls[i].transform.position).sqrMagnitude; //this is squaredMagnitude i.e. magnitude without square root
    //            if (thisDist < nearDist)
    //            {
    //                nearDist = thisDist;
    //                nearest = cacheColls[i].transform;
    //            }
    //        }
    //    }
    //    return nearest;
    //}

    //void CreateAnimationCurve()
    //{
    //    float distMin = 5.5f;
    //    float distInter = 50f;
    //    float distMax = 150f;
    //    float scaleMin = infoTagStartScale.x;
    //    float scaleInter = (scaleMin * distInter) / distMin;
    //    float scaleMax = (scaleMin * distMax) / distMin;

    //    infoTagResize.AddKey(new Keyframe(distMin/distMax, scaleMin/scaleMax, 0, 0)); //this is to compute an ease-in-ease-out curve
    //    infoTagResize.AddKey(new Keyframe(distInter/distMax, scaleInter/scaleMax , 0, 0));

    //    infoTagResize.AddKey(new Keyframe(0, scaleMin / scaleMax, 0, 0)); 
    //    infoTagResize.AddKey(new Keyframe(1, scaleInter / scaleMax, 0, 0));
    //}

    //KeyValuePair<Sprite, string> ReturnSignPic(string signName, Sprite[] sprites)
    //{
    //    KeyValuePair<Sprite, string> returnPair = new KeyValuePair<Sprite, string>(null, "");

    //    switch (signName)
    //    {
    //        case "One_Way_Right":
    //        case "OneWayRight":
    //            {
    //                returnPair = new KeyValuePair<Sprite, string>(sprites[41], "ONE WAY RIGHT");
    //            }
    //            break;

    //        case "One_Way_Left":
    //        case "OneWayLeft":
    //            {
    //                returnPair = new KeyValuePair<Sprite, string>(sprites[40], "ONE WAY LEFT");
    //            }
    //            break;


    //        case "NotNotEnter_NoPole":
    //        case "DoNotEnter":
    //            {
    //                returnPair = new KeyValuePair<Sprite, string>(sprites[57], "DO NOT ENTER");
    //            }
    //            break;

    //        case "2_Hr_Parking":
    //            {
    //                returnPair = new KeyValuePair<Sprite, string>(sprites[43], "2 HR PARKING");
    //            }
    //            break;

    //        case "15_Minute_Parking":
    //            {
    //                returnPair = new KeyValuePair<Sprite, string>(sprites[56], "15 MIN PARKING");
    //            }
    //            break;

    //        case "30_Minute_Parking":
    //            {
    //                returnPair = new KeyValuePair<Sprite, string>(sprites[42], "30 MIN PARKING");
    //            }
    //            break;


    //        case "Bicycle":
    //            {
    //                returnPair = new KeyValuePair<Sprite, string>(sprites[34], "WATCH FOR CYCLISTS");
    //            }
    //            break;

    //        case "Disabled_Parking":
    //            {
    //                returnPair = new KeyValuePair<Sprite, string>(sprites[55], "DISABLED PARKING");
    //            }
    //            break;

    //        case "Do_Not_Block_Intersection":
    //            {
    //                returnPair = new KeyValuePair<Sprite, string>(sprites[35], "DO NOT BLOCK INTERSECTION");
    //            }
    //            break;


    //        case "Left_Lane_Must_Turn_Left":
    //            {
    //                returnPair = new KeyValuePair<Sprite, string>(sprites[39], "LEFT LANE MUST TURN LEFT");
    //            }
    //            break;

    //        case "Left_Turn_Signal_Yield_on_Green":
    //            {
    //                returnPair = new KeyValuePair<Sprite, string>(sprites[49], "ONE WAY");
    //            }
    //            break;

    //        case "NoTurnLeft_NoPole":
    //        case "No_Left_Turn":
    //            {
    //                returnPair = new KeyValuePair<Sprite, string>(sprites[36], "NO LEFT TURN");
    //            }
    //            break;

    //        case "NoTurnRight_NoPole":
    //        case "No_Right_Turn":
    //            {
    //                returnPair = new KeyValuePair<Sprite, string>(sprites[37], "NO RIGHT TURN");
    //            }
    //            break;

    //        case "No_Parking":
    //            {
    //                returnPair = new KeyValuePair<Sprite, string>(sprites[27], "NO PARKING");
    //            }
    //            break;

    //        case "No_Parking_Bus_Stop":
    //            {
    //                returnPair = new KeyValuePair<Sprite, string>(sprites[47], "NO PARKING BUS STOP");
    //            }
    //            break;

    //        case "No_Truck_Parking":
    //            {
    //                returnPair = new KeyValuePair<Sprite, string>(sprites[26], "NO TRUCK PARKING");
    //            }
    //            break;

    //        case "NoParkingAnyTime":
    //            {
    //                returnPair = new KeyValuePair<Sprite, string>(sprites[23], "NO PARKING");
    //            }
    //            break;

    //        case "Parking":
    //            {
    //                returnPair = new KeyValuePair<Sprite, string>(sprites[45], "PARKING");
    //            }
    //            break;

    //        case "Parking_2":
    //            {
    //                returnPair = new KeyValuePair<Sprite, string>(sprites[46], "PARKING");
    //            }
    //            break;

    //        case "PickUp_DropOff":
    //            {
    //                returnPair = new KeyValuePair<Sprite, string>(sprites[24], "PICK-UP DROPOFF");

    //            }
    //            break;

    //        case "Reserved_Disabled_Parking":
    //            {
    //                returnPair = new KeyValuePair<Sprite, string>(sprites[25], "DISABLED PARKING");
    //            }
    //            break;

    //        case "Reserved_Parking_":
    //            {
    //                returnPair = new KeyValuePair<Sprite, string>(sprites[28], "RESERVED PARKING");
    //            }
    //            break;

    //        case "Right_Lane_Must_Turn_Right":
    //            {
    //                returnPair = new KeyValuePair<Sprite, string>(sprites[38], "RIGHT LANE MUST TURN RIGHT");
    //            }
    //            break;

    //        case "Right_Only":
    //            {
    //                returnPair = new KeyValuePair<Sprite, string>(sprites[52], "RIGHT ONLY");
    //            }
    //            break;

    //        case "Road_Work_Ahead":
    //            {
    //                returnPair = new KeyValuePair<Sprite, string>(sprites[33], "ROADWORK AHEAD");
    //            }
    //            break;

    //        case "Speed_Limit_15":
    //            {
    //                returnPair = new KeyValuePair<Sprite, string>(sprites[31], "SPEED LIMIT 15");
    //            }
    //            break;

    //        case "Speed_Limit_25":
    //            {
    //                returnPair = new KeyValuePair<Sprite, string>(sprites[30], "SPEED LIMIT 25");
    //            }
    //            break;

    //        case "Speed_Limit_35":
    //            {
    //                returnPair = new KeyValuePair<Sprite, string>(sprites[29], "SPEED LIMIT 35");
    //            }
    //            break;

    //        case "This_Lane":
    //            {
    //                returnPair = new KeyValuePair<Sprite, string>(sprites[53], "THIS LANE");
    //            }
    //            break;

    //        case "Tow__Away_Zone":
    //            {
    //                returnPair = new KeyValuePair<Sprite, string>(sprites[44], "TOW AWAY ZONE");
    //            }
    //            break;

    //        case "Turn":
    //            {
    //                returnPair = new KeyValuePair<Sprite, string>(sprites[32], "WATCH OUT CURVE");
    //            }
    //            break;

    //        case "Turning_Traffic_Must_Yield_To_Pedestrians":
    //            {
    //                returnPair = new KeyValuePair<Sprite, string>(sprites[50], "YIELD TO PEDS");
    //            }
    //            break;

    //        case "Turning_Vehicles":
    //            {
    //                returnPair = new KeyValuePair<Sprite, string>(sprites[51], "TURNING VEHICLES");
    //            }
    //            break;

    //        case "Yield_To_Peds":
    //            {
    //                returnPair = new KeyValuePair<Sprite, string>(sprites[48], "YIELD TO PEDS");
    //            }
    //            break;

    //        case "StopSign":
    //            {
    //                returnPair = new KeyValuePair<Sprite, string>(sprites[58], "STOP");
    //            }
    //            break;
    //    }
    //    return returnPair;
    //}

    //public Rect GetWorldRect(RectTransform rectTransform/*, Vector2 scale*/)
    //{
    //    Vector3[] _fourCorners = new Vector3[4];
    //    rectTransform.GetWorldCorners(_fourCorners);

    //    //Vector2 scaledSize = new Vector2(scale.x * rectTransform.rect.size.x, scale.y * rectTransform.rect.size.y);

    //    //return new Rect(_fourCorners[1], scaledSize);
    //    float height = Mathf.Abs(_fourCorners[1].y - _fourCorners[3].y);
    //    float width = Mathf.Abs(_fourCorners[2].x - _fourCorners[0].x);
    //    return new Rect(_fourCorners[1].x, _fourCorners[1].y, width, height);
    //}

    //public Rect GetWorldRect3(RectTransform rectTransform)
    //{
    //    Vector3[] _fourCorners = new Vector3[4];
    //    rectTransform.GetWorldCorners(_fourCorners);

    //    GameObject g = new GameObject("sovrapp");
    //    g.transform.position = _fourCorners[1];
    //    GameObject g1 = new GameObject("sovrapp1");
    //    g1.transform.position = _fourCorners[1] +  new Vector3(rectTransform.rect.width, 0 ,0);
    //    GameObject g2 = new GameObject("sovrapp2");
    //    g2.transform.position = _fourCorners[1] + new Vector3(0, rectTransform.rect.height, 0);



    //    return new Rect(_fourCorners[0].x, _fourCorners[0].y, Mathf.Abs(_fourCorners[3].x - _fourCorners[0].x), Mathf.Abs(_fourCorners[1].y - _fourCorners[0].y));
    //}

    //public Rect GetWorldRect4(RectTransform rt, Vector2 scale)
    //{
    //    // Convert the rectangle to world corners and grab the top left
    //    Vector3[] corners = new Vector3[4];
    //    rt.GetWorldCorners(corners);
    //    Vector3 topLeft = corners[1];

    //    // Rescale the size appropriately based on the current Canvas scale
    //    Vector2 scaledSize = new Vector2(scale.x * rt.rect.size.x, scale.y * rt.rect.size.y);
    //    return new Rect(topLeft, scaledSize);
    //}



    //int CompareInfoTags(KeyValuePair<int, CubesAndTags> x, KeyValuePair<int, CubesAndTags> y)
    ////{
    ////    int retval = x.Value.infoTag[i].transform.position.y.CompareTo(y.Value.infoTag[i].transform.position.y);
    ////    //if (retval != 0) //y is not equal
    ////        return retval;
    ////    //else
    ////    //{
    ////    //    int retval2 = x.Value.infoTag[i].transform.position.x.CompareTo(y.Value.infoTag[i].transform.position.x);
    ////    //    return retval2;
    ////    //}
    ////}

    //public static void DrawRect(Rect rect, Color col)
    //{
    //    Vector3 pos = new Vector3(rect.x + rect.width / 2, rect.y + rect.height / 2, 0.0f);
    //    Vector3 scale = new Vector3(rect.width, rect.height, 0.0f);

    //    DrawRect(pos, col, scale);
    //}

    //public static void DrawRect(Vector3 pos, Color col, Vector3 scale)
    //{
    //    Vector3 halfScale = scale * 0.5f;

    //    Vector3[] points = new Vector3[]
    //    {
    //        pos + new Vector3(halfScale.x,      halfScale.y,    halfScale.z),
    //        pos + new Vector3(-halfScale.x,     halfScale.y,    halfScale.z),
    //        pos + new Vector3(-halfScale.x,     -halfScale.y,   halfScale.z),
    //        pos + new Vector3(halfScale.x,      -halfScale.y,   halfScale.z),
    //    };

    //    Debug.DrawLine(points[0], points[1], col);
    //    Debug.DrawLine(points[1], points[2], col);
    //    Debug.DrawLine(points[2], points[3], col);
    //    Debug.DrawLine(points[3], points[0], col);
    //}

    //bool Overlapping(Vector3 upperLeftA, Vector3 lowerRightA, Vector3 upperLeftB, Vector3 lowerRightB)
    //{
    //    return (upperLeftA.x < lowerRightB.x && lowerRightA.x > upperLeftB.x && upperLeftA.y > lowerRightB.y && lowerRightA.y < upperLeftB.y);
    //}

}

public static class RectExt
{
    public static bool OverlapsAlt(this Rect r, Rect other)
    {
        float factor = 1.1f;
        return (double)other.xMax > (double)r.xMin && (double)other.xMin < (double)r.xMax * factor && (double)other.yMax * factor > (double)r.yMin && (double)other.yMin < (double)r.yMax;
    }

    public static bool OverlapsAlt(this Rect r, Rect other, bool allowInverse)
    {
        Rect rect = r;
        if (allowInverse)
        {
            rect = OrderMinMax(rect);
            other = OrderMinMax(other);
        }
        return rect.Overlaps(other);
    }

    private static Rect OrderMinMax(Rect rect)
    {
        if ((double)rect.xMin > (double)rect.xMax)
        {
            float xMin = rect.xMin;
            rect.xMin = rect.xMax;
            rect.xMax = xMin;
        }
        if ((double)rect.yMin > (double)rect.yMax)
        {
            float yMin = rect.yMin;
            rect.yMin = rect.yMax;
            rect.yMax = yMin;
        }
        return rect;
    }
}