using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using MKGlowSystem;



public class EnvironmentSensingAltTrigger : MonoBehaviour
{
    //private PostProcessingProfile profile;
    private Material[] mats;
    private Sprite[] sprites;
    private Dictionary<int, float> CSVDictionary = new Dictionary<int, float>(); //this is to store the precomputed values of angle(speed)
    private Dictionary<int, CubesAndTags> IDsAndGos = new Dictionary<int, CubesAndTags>(); //this is to store gameobjects ids and their relative cubes
    private Quaternion rot0; //this is to store the initial rotation of the needle
    private GameObject[] prefabs;
    private GameObject pathEnhanced = null;
    private GameObject centerLine = null;
    private GameObject speedPanel; //this is used to semplify the search into the car hierarchy which is very deep
    private Vector3 obstaclePrevPos; //this is to store the initial position of the obstacle in order to compute its speed
    private LinesUtilsAlt linesUtilsAlt = new LinesUtilsAlt();
    private LayerMask mask;
    public enum Lane {RIGHT, OPPOSITE}
    private HashSet<Transform> curColls = new HashSet<Transform>(); //this is to store the root transform of the colliders 
    private Transform rayCastPos;
    private RectTransform speed;
    private RectTransform needle;
    private Rigidbody rigidbody;

    private CurvySplineSegment curSegment = null;
    private List<CurvySplineSegment> segmentsToSearch = new List<CurvySplineSegment>();
    
    
    private DriverCamera driverCam;

    public GameObject SpeedPanel { set { speedPanel = value; } }
    public DriverCamera DriverCam { set { driverCam = value; } } 

    public class CubesAndTags
    {
        public GameObject boundingCube;
        public GameObject infoTag;
        public Bounds bounds;

        public CubesAndTags(GameObject boundingCube, GameObject infoTag, Bounds bounds)
        {
            this.boundingCube = boundingCube;
            this.infoTag = infoTag;
            this.bounds = bounds;
        }   
    }

    void OnEnable()
    {
        TrafSpawnerPCH.OnSpawnHeaps += HandleOnSpawnHeaps;
    }

    void OnDisable()
    {
        TrafSpawnerPCH.OnSpawnHeaps -= HandleOnSpawnHeaps;
    }

    void Awake() {
        
        LoadDictionary();
        LoadMaterials();
        LoadPrefabs();
        LoadSprites();
    }

    void Start() {

        LoadCarHierarchy();
        rigidbody = gameObject.GetComponent<Rigidbody>();
        obstaclePrevPos = Vector3.zero; //init position of obstacle spawned
        rot0 = speed.localRotation; //init rotation of speed needle
        mask = (1 << LayerMask.NameToLayer("Traffic")) | (1 << LayerMask.NameToLayer("obstacle")) | (1 << LayerMask.NameToLayer("EnvironmentProp")) | (1 << LayerMask.NameToLayer("Signage")); //restrict OnTriggerEnter only to the layers of interest

        foreach (Transform t in driverCam.transform)
            if (t.name.Equals("Center"))
            {
                t.GetComponent<Camera>().allowHDR = true;
                MKGlow mkg = t.gameObject.AddComponent<MKGlow>();
                mkg.GlowType = MKGlowType.Selective;
                mkg.GlowLayer = LayerMask.GetMask("Graphics");
                break;
            }
        CreatePathEnhanced(); 
        CreateCenterLine();
        linesUtilsAlt.LineRend = linesUtilsAlt.CreateLineRenderer(linesUtilsAlt.LineRendererEmpty, "LineRenderer", 2.5f, new Color32(0x3A, 0xC1, 0xAA, 0xFF), mats[3], () => linesUtilsAlt.InitGlowTexture());
        linesUtilsAlt.LineRend2 = linesUtilsAlt.CreateLineRenderer(linesUtilsAlt.CenterLineRendererEmpty, "centerLineRenderer", 0.5f, new Color32(0xFF, 0xFF, 0xFF, 0xFF), mats[3], () => linesUtilsAlt.InitGlowTexture2());
        linesUtilsAlt.CenterLineLerper();

        StartCoroutine(InitNavigationLine());
        StartCoroutine(NavigationLine(0.25f));
        StartCoroutine(LaneKeeping(0.25f));
    }

    void Update() {

            //NavigationLine();
            //LaneKeeping();
            SetSpeed();
    }

    void OnDrawGizmos()
    {
        //this is for debugging purposes: see OverlapSphere bounds

        //Gizmos.color = Color.blue;
        //Gizmos.DrawWireSphere(gameObject.transform.position, 3.5f);
        DebugExtension.DrawCone(rayCastPos.position, rayCastPos.forward * 150f, Color.magenta, 11);
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
                        if (other.transform.tag.Equals("Tree"))
                        {
                            CapsuleCollider capsCol = other as CapsuleCollider;
                            bounds.center = Quaternion.Euler(other.transform.localEulerAngles) * new Vector3(capsCol.center.x * other.transform.localScale.x, capsCol.center.y * other.transform.localScale.y, capsCol.center.z * other.transform.localScale.z);
                            bounds.size = new Vector3(capsCol.radius * 2.0f * other.transform.localScale.x, capsCol.radius * 2.0f * other.transform.localScale.y, capsCol.height * other.transform.localScale.z);
                        }
                        else if (other.transform.tag.Equals("Rock"))
                        {
                            SphereCollider sphereCol = other as SphereCollider;
                            bounds.center = Quaternion.Euler(other.transform.localEulerAngles) * new Vector3(sphereCol.center.x * other.transform.localScale.x, sphereCol.center.y * other.transform.localScale.y, sphereCol.center.z * other.transform.localScale.z);
                            bounds.size = new Vector3(sphereCol.radius * 2.0f * other.transform.localScale.x, sphereCol.radius * 2.0f * other.transform.localScale.y, sphereCol.radius * 2.0f * other.transform.localScale.z);
                        }
                        GameObject boundingCube = CreateBoundingCube(other.transform, other.transform.position + bounds.center, bounds.size);
                        GameObject infoTag = CreateInfoTag(other.transform.position + bounds.center);
                        boundingCube.GetComponent<Renderer>().enabled = false;
                        infoTag.GetComponent<Canvas>().enabled = false;
                        CubesAndTags cubesAndTags = new CubesAndTags(boundingCube, infoTag, bounds);
                        IDsAndGos.Add(other.gameObject.GetInstanceID(), cubesAndTags);
                    }
                    break;
                case 17:
                    { //Signage
                        Bounds bounds = new Bounds();
                        BoxCollider boxCol = other as BoxCollider;
                        bounds.center = boxCol.center;
                        bounds.size = boxCol.size * 1.25f;
                        GameObject boundingCube = CreateBoundingCube(other.transform, other.transform.position + bounds.center, bounds.size);
                        GameObject infoTag = CreateInfoTag(other.transform.position + bounds.center);
                        boundingCube.GetComponent<Renderer>().enabled = false;
                        infoTag.GetComponent<Canvas>().enabled = false;
                        CubesAndTags cubesAndTags = new CubesAndTags(boundingCube, infoTag, bounds);
                        IDsAndGos.Add(other.gameObject.GetInstanceID(), cubesAndTags);
                    }
                    break;
                case 12:
                    {  //obstacle
                        Bounds bounds = new Bounds();
                        GameObject boundingCube = null;
                        GameObject infoTag = null;
                        if (other.transform.root.name.StartsWith("Mercedes"))
                        {
                            if (!curColls.Contains(other.transform.root))
                            { //this is in order to check if an object has more colliders than one; if so, one is sufficient so simply discard other otherwise the cube instantiated would be equal to the number of colliders
                                bounds = ComputeBounds(other.transform.root);
                                curColls.Add(other.transform.root);
                                boundingCube = CreateBoundingCube(other.transform.root, bounds.center, bounds.size);
                                boundingCube.transform.SetParent(other.transform.root);
                                infoTag = CreateInfoTag(bounds.center);
                                boundingCube.GetComponent<Renderer>().enabled = false;
                                infoTag.GetComponent<Canvas>().enabled = false;
                                CubesAndTags cubesAndTags = new CubesAndTags(boundingCube, infoTag, bounds); //I need to recompute the bounds since obstacles are dynamic, so I pass an empty bounds here
                                IDsAndGos.Add(other.gameObject.GetInstanceID(), cubesAndTags);
                            } 
                        }
                        else
                        {
                            bounds = ComputeBounds(other.transform);
                            boundingCube = CreateBoundingCube(other.transform, bounds.center, bounds.size);
                            infoTag = CreateInfoTag(bounds.center);
                            boundingCube.transform.SetParent(other.transform);
                            boundingCube.transform.localPosition = Vector3.zero;
                            boundingCube.GetComponent<Renderer>().enabled = false;
                            infoTag.GetComponent<Canvas>().enabled = false;
                            CubesAndTags cubesAndTags = new CubesAndTags(boundingCube, infoTag, bounds); //I need to recompute the bounds since obstacles are dynamic, so I pass an empty bounds here
                            IDsAndGos.Add(other.gameObject.GetInstanceID(), cubesAndTags);
                        }      
                    }
                    break;
                case 8:
                    {   //Traffic
                        if (!curColls.Contains(other.transform.root))
                        { //this is in order to check if an object has more colliders than one; if so, one is sufficient so simply discard other otherwise the cube instantiated would be equal to the number of colliders
                            Bounds bounds = ComputeBounds(other.transform.root);
                            GameObject boundingCube = CreateBoundingCube(other.transform, bounds.center, bounds.size);
                            GameObject infoTag = CreateInfoTag(bounds.center);
                            boundingCube.transform.SetParent(other.transform);
                            boundingCube.transform.localPosition = Vector3.zero;
                            boundingCube.GetComponent<Renderer>().enabled = false;
                            infoTag.GetComponent<Canvas>().enabled = false;
                            CubesAndTags cubesAndTags = new CubesAndTags(boundingCube, infoTag, bounds); //I need to recompute the bounds since obstacles are dynamic, so I pass an empty bounds here
                            IDsAndGos.Add(other.gameObject.GetInstanceID(), cubesAndTags);
                            curColls.Add(other.transform.root);
                        }
                    }
                    break;
            }
        }

    }

    void OnTriggerExit(Collider other)
    {
        if (IDsAndGos.ContainsKey(other.gameObject.GetInstanceID()))
        {
            Destroy(IDsAndGos[other.gameObject.GetInstanceID()].boundingCube);
            Destroy(IDsAndGos[other.gameObject.GetInstanceID()].infoTag);
            IDsAndGos.Remove(other.gameObject.GetInstanceID());
            if (curColls.Count != 0) //this is necessary otherwise if I don't clear the list from the 2nd time I would intercept a previous intercepted car I will not detect it since there is already a collider inside the list!
                curColls.Clear();
            
        }

    }

    void OnTriggerStay(Collider other)
    {
        if (IDsAndGos.ContainsKey(other.gameObject.GetInstanceID()))
        {
            float dist = CalculateDistance(rayCastPos.position, other.transform.position); //this distance does not include bounds since it cannot always be precomputed, for example, this is the case of moving objects
            Vector3 trasl = Vector3.zero;
            
            switch (other.gameObject.layer)
            {
                case 16:
                    {
                        Bounds bounds = IDsAndGos[other.gameObject.GetInstanceID()].bounds;
                        if (dist <= 10.0f)
                        {
                            if (other.transform.tag.Equals("Rock"))
                            {
                                trasl = Quaternion.Euler(-other.transform.localEulerAngles) * bounds.size; //infoTag trasl
                                trasl = new Vector3(Mathf.Abs(trasl.z) * 0.5f + 2.0f, Mathf.Abs(trasl.y) * 0.5f, 0);
                                UpdateInfoTag(other, "0" + " KPH", Mathf.RoundToInt(dist).ToString() + " M", "", sprites[64], bounds, dist, trasl);
                            } else if (other.transform.tag.Equals("Tree"))
                            {
                                trasl = Quaternion.Euler(-other.transform.localEulerAngles) * bounds.size; //infoTag trasl
                                trasl = new Vector3(Mathf.Abs(trasl.z) * 0.5f + 2.0f, 0, 0);
                                if (other.transform.name.Equals("tree_PCH(2)"))
                                    Debug.Log(trasl);
                                UpdateInfoTag(other, "0" + " KPH", Mathf.RoundToInt(dist).ToString() + " M", "", sprites[64], bounds, dist, trasl);
                            }
                        }
                        else
                        {
                            IDsAndGos[other.gameObject.GetInstanceID()].boundingCube.GetComponent<Renderer>().enabled = false;
                            IDsAndGos[other.gameObject.GetInstanceID()].infoTag.GetComponent<Canvas>().enabled = false;
                        }
                    }
                    break;
                case 17: //Signage
                    {
                        Bounds bounds = IDsAndGos[other.gameObject.GetInstanceID()].bounds;
                        if (Vector3.Dot(gameObject.transform.TransformDirection(Vector3.forward), other.transform.TransformDirection(Vector3.forward)) < 0)
                        {
                            UpdateInfoTag(other, "", "", "", sprites[58], bounds, dist, Vector3.zero);
                        }
                        else
                        {
                            IDsAndGos[other.gameObject.GetInstanceID()].boundingCube.GetComponent<Renderer>().enabled = false;
                            IDsAndGos[other.gameObject.GetInstanceID()].infoTag.GetComponent<Canvas>().enabled = false;
                        }
                    }
                    break;
                case 12: //obstacle
                    {
                        Bounds bounds = IDsAndGos[other.gameObject.GetInstanceID()].bounds;
                        UpdateInfoTag(other, Mathf.RoundToInt(CalculateObstacleSpeed(other.transform)).ToString() + " KPH", Mathf.RoundToInt(dist).ToString() + " M", "", sprites[28], bounds, dist, Vector3.zero);
                    }
                    break;
                case 8: //TrafficCar
                    {
                        Bounds bounds = IDsAndGos[other.gameObject.GetInstanceID()].bounds;
                        TrafPCH trafPCH = other.transform.root.GetComponent<TrafPCH>();
                        if (trafPCH != null)
                        {
                            float speed = trafPCH.currentSpeed;
                            float distToWarn = RiskAssessmentFormula(rigidbody.velocity.magnitude, speed);
                            float cone = Mathf.Cos(11 * Mathf.Deg2Rad);
                            Vector3 heading = (other.transform.position - rayCastPos.position).normalized;
                            float cosine = Vector3.Dot(rayCastPos.TransformDirection(Vector3.forward), heading);
                            if (cosine > cone) //the forward vehicle is in my trajectory
                            {
                                //DebugExtension.DrawCone(rayCastPos.position, Color.yellow , Mathf.Acos(cosine) * 180 / Mathf.PI);
                                if (dist <= Mathf.Abs(distToWarn))
                                {
                                    Debug.Log("other car is: " + other.transform.root.name + distToWarn);
                                    UpdateInfoTag(other, Mathf.RoundToInt(speed * 3.6f).ToString() + " KPH", Mathf.RoundToInt(dist).ToString() + " M", "", sprites[65], bounds, dist, Vector3.zero);
                                    IDsAndGos[other.gameObject.GetInstanceID()].infoTag.GetComponent<Blinking>().StartBlinking();
                                }
                                else
                                {
                                    UpdateInfoTag(other, Mathf.RoundToInt(speed * 3.6f).ToString() + " KPH", Mathf.RoundToInt(dist).ToString() + " M", "", sprites[0], bounds, dist, Vector3.zero);
                                    IDsAndGos[other.gameObject.GetInstanceID()].infoTag.GetComponent<Blinking>().StopBlinking();
                                }
                                
                            } else
                            {
                                IDsAndGos[other.gameObject.GetInstanceID()].infoTag.GetComponent<Blinking>().StopBlinking();
                                UpdateInfoTag(other, Mathf.RoundToInt(speed * 3.6f).ToString() + " KPH", Mathf.RoundToInt(dist).ToString() + " M", "", sprites[0], bounds, dist, Vector3.zero);
                            }
                        }
                    }
                    break;
            }
        }
    }

    GameObject CreateBoundingCube(Transform tr, Vector3 center, Vector3 size)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //go.GetComponent<BoxCollider>().isTrigger = true; //this in order to avoid to register collision of incoming Car rigidbody
        Destroy(go.GetComponent<BoxCollider>());
        go.layer = LayerMask.NameToLayer("Graphics");
        go.transform.position = center;
        go.transform.rotation = tr.rotation;
        go.transform.localScale = size;
        go.GetComponent<Renderer>().material = mats[2];
        return go;
    }

    GameObject CreateInfoTag(Vector3 center)
    {
        GameObject infoTag = Instantiate(prefabs[2], center, transform.rotation);
        return infoTag;
    }

    void UpdateInfoTag(Collider other, string text, string text1, string text2, Sprite sprite, Bounds bounds, float dist, Vector3 trasl)
    {
        Renderer visibility = IDsAndGos[other.gameObject.GetInstanceID()].boundingCube.GetComponent<Renderer>();
        Canvas canvas = IDsAndGos[other.gameObject.GetInstanceID()].infoTag.GetComponent<Canvas>();
        Transform textPanel = IDsAndGos[other.gameObject.GetInstanceID()].infoTag.transform.GetChild(0);
        Transform imagePanel = IDsAndGos[other.gameObject.GetInstanceID()].infoTag.transform.GetChild(1);

        IDsAndGos[other.gameObject.GetInstanceID()].infoTag.transform.rotation = transform.rotation;
        if (other.gameObject.layer.Equals(16))
        {
            Vector3 relativePoint = transform.InverseTransformPoint(other.transform.position); //this is to recognize if the object is to the left/right with respect to the car
            if (relativePoint.x < 0.0f)
                IDsAndGos[other.gameObject.GetInstanceID()].infoTag.transform.position = other.transform.position + bounds.center + IDsAndGos[other.gameObject.GetInstanceID()].infoTag.transform.TransformDirection(Vector3.right.x * trasl.x, Vector3.up.y * trasl.y, 0); //object is to the left  
            else if (relativePoint.x >= 0.0f)
                IDsAndGos[other.gameObject.GetInstanceID()].infoTag.transform.position = other.transform.position + bounds.center + IDsAndGos[other.gameObject.GetInstanceID()].infoTag.transform.TransformDirection(Vector3.right.x * -trasl.x, Vector3.up.y * trasl.y, 0); //object is to the right or directly ahead
        }
        else if (other.gameObject.layer.Equals(17))
        {
            IDsAndGos[other.gameObject.GetInstanceID()].infoTag.transform.position = other.transform.position + bounds.center + IDsAndGos[other.gameObject.GetInstanceID()].infoTag.transform.TransformDirection(-Vector3.forward);
        }
        else if (other.gameObject.layer.Equals(8) || other.gameObject.layer.Equals(12))
        {
            Vector3 relativePoint = transform.InverseTransformPoint(other.transform.position); //this is to recognize if the object is to the left/right with respect to the car
            if (relativePoint.x < 0.0f)
                IDsAndGos[other.gameObject.GetInstanceID()].infoTag.transform.position = other.transform.position + IDsAndGos[other.gameObject.GetInstanceID()].infoTag.transform.TransformDirection(Vector3.right.x * bounds.size.x, Vector3.up.y * bounds.size.y * 0.5f, 0); //object is to the left  
            else if (relativePoint.x >= 0.0f)
                IDsAndGos[other.gameObject.GetInstanceID()].infoTag.transform.position = other.transform.position + IDsAndGos[other.gameObject.GetInstanceID()].infoTag.transform.TransformDirection(Vector3.right.x * -bounds.size.x, Vector3.up.y * bounds.size.y * 0.5f, 0); //object is to the right or directly ahead
        }

        imagePanel.transform.GetChild(0).GetComponent<Image>().sprite = sprite;
        textPanel.transform.GetChild(0).GetComponent<Text>().text = text;
        textPanel.transform.GetChild(1).GetComponent<Text>().text = text1;
        textPanel.transform.GetChild(2).GetComponent<Text>().text = text2;
        visibility.enabled = true;
        canvas.enabled = true;
    }

    void LoadCarHierarchy()
    {
        Dictionary<string, RectTransform> speedPanelChildren = new Dictionary<string, RectTransform>();
        foreach (RectTransform rt in speedPanel.transform)
            if (rt.name.Equals("Speed") || rt.name.Equals("Needle"))
                speedPanelChildren.Add(rt.name, rt);
        
        speed = speedPanelChildren["Speed"];
        needle = speedPanelChildren["Needle"];
        
        foreach (Transform t in transform)
            if (t.name.Equals("rayCastPos")) {
                rayCastPos = t;
                break;
            }

        SphereCollider sphereCol = gameObject.AddComponent<SphereCollider>();
        sphereCol.radius = 150;
        sphereCol.isTrigger = true;

        //Debug.Log(speed);
        //Debug.Log(needle);
        //Debug.Log(rayCastPos);
    }

    void LoadDictionary() {
        ReadCSVFile(Application.streamingAssetsPath + "/angle(speedKph).csv");
    }

    void LoadSprites() {

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

    void LoadMaterials() {
        
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

    void LoadPrefabs() {
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

    public static Bounds ComputeBounds(Transform t)
    {
        Quaternion currentRotation = t.rotation;
        t.rotation = Quaternion.Euler(0f, 0f, 0f);

        Bounds b = new Bounds();

        Renderer[] r = t.GetComponentsInChildren<Renderer>();

        for (int ir = 0; ir < r.Length; ir++)
        {
            Renderer renderer = r[ir];
            if (ir == 0)
                b = renderer.bounds;
            else
                b.Encapsulate(renderer.bounds);
        }

        t.rotation = currentRotation;

        return b;
    }

    float CalculateDistance(Vector3 src, Vector3 dst) {

        //this way distance in not completely accurate since it does not take into account the distance from the center to the maximum extension of the object
        float dist = Vector3.Distance(src, dst);
        Debug.DrawLine(src, dst, Color.green);
        return dist;
    }

    Transform CalculatePointDistance(Collider[] colls, string pointType) {
        Transform furthest = null; //maybe even the nearest may go well since I consider the nextwayPoint from the current chosen
        float farDist = 0;
        for (int i = 0; i < colls.Length; ++i)
        {
            if (colls[i].tag.Equals(pointType))
            { //consider only Centerpoints and ignore other Colliders that belong to Graphics 
                float thisDist = (transform.position - colls[i].transform.position).sqrMagnitude; //this is squaredMagnitude i.e. magnitude without square root
                if (thisDist > farDist)
                {
                    farDist = thisDist;
                    furthest = colls[i].transform;
                }
            }
        }
        return furthest;
    }

    float RiskAssessmentFormula(float subjectVehicleSpeed, float obstacleSpeed)
    {
        float freeRunningTime = 0.66f;
        float deceleration = 7.0f;
        return subjectVehicleSpeed * freeRunningTime + (Mathf.Pow(subjectVehicleSpeed, 2) / (2 * deceleration) - Mathf.Pow(obstacleSpeed, 2) / (2 * deceleration));
    }

    //Transform CalculatePointDistance(string pointType) {
    //    Transform furthest = null; //maybe even the nearest may go well since I consider the nextwayPoint from the current chosen
    //    float farDist = 0;
    //    for (int i = 0; i < cacheColls.Count; ++i)
    //    {
    //        if (cacheColls[i].tag.Equals(pointType))
    //        { //consider only Centerpoints and ignore other Colliders that belong to Graphics 
    //            float thisDist = (transform.position - cacheColls[i].transform.position).sqrMagnitude; //this is squaredMagnitude i.e. magnitude without square root
    //            if (thisDist > farDist)
    //            {
    //                farDist = thisDist;
    //                furthest = cacheColls[i].transform;
    //            }
    //        }
    //    }
    //    return furthest;
    //}

    float CalculateObstacleSpeed(Transform tr) {

        //if (Application.isPlaying)
        //{
            Vector3 obstacleNextPos = tr.position;
            float speed = (obstacleNextPos - obstaclePrevPos).magnitude / Time.deltaTime; //speed works in play mode, in editor mode since you stop playing, Time.deltatime gives erratic results
            obstaclePrevPos = obstacleNextPos;
            return speed;
        //}


    }

    Lane MonitorLane(Vector3 Point)
    {
        if (Vector3.Dot(gameObject.transform.TransformDirection(Vector3.forward), Point) < 0 )
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
            
            float carTf = curvySpline.GetNearestPointTFExt(transform.position, segmentsToSearch.ToArray());

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

    void CreatePathEnhanced() {
        //pathEnhanced = new GameObject("PathEnhanced"); //since the scene has been loaded it can be instantiated a new empty with all the nodes of the path
        //pathEnhanced.AddComponent<AutoPathHelper>();
        pathEnhanced = Instantiate(prefabs[10]);
    }

    void CreateCenterLine() {
        centerLine = Instantiate(prefabs[8]);
    }

    void SetSpeed() {
        float targetAngle;
        speed.GetComponent<Text>().text = Mathf.RoundToInt(rigidbody.velocity.magnitude * 3.6f).ToString(); //speed in kph
        if (CSVDictionary.TryGetValue(Mathf.RoundToInt(rigidbody.velocity.magnitude * 3.6f), out targetAngle))
            needle.localRotation = Quaternion.Euler(0, 0, targetAngle) * rot0;
    }

    void ReadCSVFile(string path)
    {
        StreamReader inputStream = new StreamReader(path);
        while (!inputStream.EndOfStream)
        {
            string line = inputStream.ReadLine();
            string[] values = line.Split(',');
            CSVDictionary.Add(int.Parse(values[0]), float.Parse(values[1]));
        }
        inputStream.Close();
    }

    void HandleOnSpawnHeaps(bool spawned) {
        TrafPCH[] allTraffic = FindObjectsOfType(typeof(TrafPCH)) as TrafPCH[];
        if (spawned == true)
        {
            foreach (TrafPCH t in allTraffic)
                t.gameObject.AddComponent<TrafficCarNavigationLine>();
        }
        else
        {
            foreach (LineRenderer line in FindObjectsOfType(typeof(LineRenderer)))
                if (line.gameObject.name.Equals("TrafLineRenderer"))
                    Destroy(line.gameObject);

            //foreach (TrafPCH t in allTraffic)
            //{
            //    MeshCollider other = t.transform.GetChild(0).GetChild(0).GetComponent<MeshCollider>();
            //    if (IDsAndGos.ContainsKey(other.gameObject.GetInstanceID()))
            //    {
            //        Destroy(IDsAndGos[other.gameObject.GetInstanceID()].boundingCube);
            //        Destroy(IDsAndGos[other.gameObject.GetInstanceID()].infoTag);
            //        IDsAndGos.Remove(other.gameObject.GetInstanceID());
            //    }
            //}
        }
    }

}



//RaycastHit hit;
//Vector3 right = rayCastPos.TransformDirection(Vector3.right);
//        if (Physics.Raycast(rayCastPos.position, -right, out hit, 50f, 1 << LayerMask.NameToLayer("Graphics")) && hit.collider.tag.Equals("Guardrail"))
//        { //remember that in order to let this work it needs to be enabled Physics--->Queries hit backfaces, otherwise raycast won't detect the guardrail
//            Debug.DrawLine(rayCastPos.position, hit.point, Color.yellow);
    
//        }
//        else
//        {
//            if (checkLane)
//            {
                
//                Debug.DrawLine(rayCastPos.position, rayCastPos.position + (-right* 50f), Color.magenta);
//            }

//        }