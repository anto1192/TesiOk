using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using MKGlowSystem;
using System.Linq;
using TMPro;


public class EnvironmentSensingAltUrbanTriggerSelective : MonoBehaviour
{
    private bool dashORHud = false; //enable/disable dash

    private LinesUtils linesUtils = new LinesUtils();
    private RiskAssessmentSelective RiskAssessmentSelective;
    private TrafficLightContainer trafLight; //this is to track the last trafficLight

    private Vector3 infoTagStartScale = Vector3.zero;

    [SerializeField]
    private AnimationCurve infoTagResize = new AnimationCurve();

    private Dictionary<int, float> CSVDictionary = new Dictionary<int, float>(); //this is to store the precomputed values of angle(speed)
    public Dictionary<int, CubesAndTags> IDsAndGos = new Dictionary<int, CubesAndTags>(); //this is to store gameobjects ids and their relative cubes
    private Dictionary<string, int> CSVSignDictionary = new Dictionary<string, int>(); //this is to store the precomputed values of angle(speed)

    private Quaternion rot0; //this is to store the initial rotation of the needle

    private LayerMask mask;

    private Transform rayCastPos;
    private Transform speed;
    private Rigidbody rb;
    private VehicleController vehicleController;
    private DriverCamera driverCam;
    private GameObject leapCam;

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

    void Start()
    {
        LoadCarHierarchy();

        infoTagStartScale = ResourceHandler.instance.prefabs[3].transform.localScale;

        CreateInfoTagResizeCurve();

        linesUtils.CenterLineLerper(Color.red, Color.yellow, Color.green, new Color32(0x00, 0x80, 0xFF, 0x00));

        mask = (1 << LayerMask.NameToLayer("Traffic")) | (1 << LayerMask.NameToLayer("obstacle")) | (1 << LayerMask.NameToLayer("EnvironmentProp")) | (1 << LayerMask.NameToLayer("Signage") | 1 << LayerMask.NameToLayer("Roads")); //restrict OnTriggerEnter only to the layers of interest

        StartCoroutine(CleanObjects(0.25f));

        RiskAssessmentSelective = new RiskAssessmentSelective(infoTagStartScale, driverCam, infoTagResize, rayCastPos, gameObject, mask, linesUtils);
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

    void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & mask) != 0) //matched one!
        {
            switch (other.gameObject.layer)
            {
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
                        if (other.transform.root.CompareTag("TrafficCar"))
                        {
                            if (other.transform.name.Equals("Body1"))
                            {
                                bounds = ComputeBounds(other.transform.root);
                                try
                                {
                                    boundingCube = other.transform.Find("Cube").gameObject;
                                }
                                catch (NullReferenceException)
                                {
                                    Debug.Log("object incriminated is: " + other.transform.position);
                                }

                                infoTag = CreateInfoTag(other.transform.position + bounds.center);
                                UpdateIDsAndGos(boundingCube, infoTag, other, bounds);
                                TrafficCarNavigationLineUrban trafficCarNavigationLineUrban = other.transform.root.gameObject.AddComponent<TrafficCarNavigationLineUrban>();
                                trafficCarNavigationLineUrban.enabled = false;
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

                            infoTag = CreateInfoTag(other.transform.position + bounds.center);
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
                            infoTag = CreateInfoTag(other.transform.position + bounds.center);
                            UpdateIDsAndGos(boundingCube, infoTag, other, bounds);
                            TrafficCarNavigationLineUrban trafficCarNavigationLineUrban = other.transform.root.gameObject.AddComponent<TrafficCarNavigationLineUrban>();
                            trafficCarNavigationLineUrban.enabled = false;
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
                            infoTagPost = CreateInfoTag(other.transform.position + boundsPost.center);
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
                    case 17: //Signage
                        {
                            float dotProduct = Vector3.Dot(gameObject.transform.TransformDirection(Vector3.forward), objectsList[i].other.transform.TransformDirection(Vector3.up));
                            Animator anim = objectsList[i].infoTag[0].GetComponent<Animator>();
                            if (dotProduct >= -1 && dotProduct <= -0.707f)
                            {
                                if ((objectsList[i].other.transform.name.Equals("StopSign") || objectsList[i].other.transform.name.Equals("RoadWork")) && dist <= 40)
                                {
                                    Bounds bounds = objectsList[i].bounds[0];
                                    int spriteIndex = CSVSignDictionary[objectsList[i].other.name];
                                    objectsList[i].boundingCube[0].GetComponent<Renderer>().enabled = true;
                                    objectsList[i].infoTag[0].GetComponent<Canvas>().enabled = true;
                                    RiskAssessmentSelective.UpdateInfoTag(objectsList[i], bounds, "", ResourceHandler.instance.sprites[spriteIndex], dist, Vector3.zero, 0);
                                    anim.SetBool("Blink", true);
                                }
                                else
                                {
                                    objectsList[i].boundingCube[0].GetComponent<Renderer>().enabled = false;
                                    objectsList[i].infoTag[0].GetComponent<Canvas>().enabled = false;
                                    anim.SetBool("Blink", false);
                                }
                            }
                            else
                            {
                                if (objectsList[i].other.transform.name.Equals("StopSign") || objectsList[i].other.transform.name.Equals("RoadWork"))
                                    anim.SetBool("Blink", false);
                            }
                        }
                        break;
                    case 12:
                        {
                            Bounds bounds = objectsList[i].bounds[0];
                            Sprite sprite = objectsList[i].other.GetComponent<Image>().sprite;
                            if (objectsList[i].other.CompareTag("Obstacle"))
                                RiskAssessmentSelective.BoundingCubeLerperObstacleDefSF(objectsList[i], bounds, CalculateObstacleSpeed(objectsList[i]), 0, sprite, Vector3.zero, 0);
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
                                if (objectsList[i].other.transform.root.CompareTag("TrafficCar"))
                                    RiskAssessmentSelective.BoundingCubeLerperSF(objectsList[i], bounds, speed, acceleration, sprite, Vector3.zero, 0);
                                else
                                    RiskAssessmentSelective.BoundingCubeLerperScooterSF(objectsList[i], bounds, speed, sprite, Vector3.zero, 0);
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
                                                Panel.transform.GetChild(3).GetComponent<Image>().sprite = ResourceHandler.instance.sprites[27];
                                                Panel.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "GO";
                                                Panel.transform.GetChild(2).GetComponent<TextMeshProUGUI>().color = new Color32(0x3B, 0xAA, 0x34, 0xFF);

                                                anim.SetInteger("trafLightState", (int)TrafLightState.GREEN);

                                            }
                                            else if (currentState.Equals(TrafLightState.YELLOW))
                                            {
                                                Panel.transform.GetChild(3).GetComponent<Image>().sprite = ResourceHandler.instance.sprites[28];
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
                                                Panel.transform.GetChild(3).GetComponent<Image>().sprite = ResourceHandler.instance.sprites[29];
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
                                RiskAssessmentSelective.BoundingCubeLerperSF(objectsList[i], boundsPost, sprite, ReturnTrasl(boundsPost, objectsList[i].other), 2);
                            }
                            else
                            {
                                Bounds boundsPost = objectsList[i].bounds[0];
                                Sprite sprite = objectsList[i].other.GetComponent<Image>().sprite;
                                RiskAssessmentSelective.BoundingCubeLerperSF(objectsList[i], boundsPost, sprite, ReturnTrasl(boundsPost, objectsList[i].other), 0);
                            }
                        }
                        break;
                }
            }
        }
    }
}
