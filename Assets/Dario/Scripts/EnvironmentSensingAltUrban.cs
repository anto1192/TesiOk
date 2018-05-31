using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using MKGlowSystem;


public class EnvironmentSensingAltUrban : MonoBehaviour {

    //private PostProcessingProfile profile;
    private Material[] mats;
    private Sprite[] sprites;
    private Dictionary<int, float> CSVDictionary = new Dictionary<int, float>(); //this is to store the precomputed values of angle(speed)
    private Dictionary<GameObject, GameObject> cubesAndTags = new Dictionary<GameObject, GameObject>(); //this is to store cubes and tags
    private Quaternion rot0; //this is to store the initial rotation of the needle
    private string[] infos = new string[3];
    private GameObject[] prefabs;
    private GameObject pathEnhanced = null;
    private GameObject centerLine = null;
    private GameObject speedPanel; //this is used to semplify the search into the car hierarchy which is very deep
    private Vector3 obstaclePrevPos; //this is to store the initial position of the obstacle in order to compute its speed
    private LinesUtils linesUtils = new LinesUtils();
    private float Timer = 0.1f;
    private List<Collider> cacheColls; //this is to cache some colliders in order to avoid the repetition of some OverlapSphere
    private Transform rayCastPos;
    private RectTransform speed;
    private RectTransform needle;

    private DriverCamera driverCam;

    public GameObject SpeedPanel { set { speedPanel = value; } }
    public DriverCamera DriverCam { set { driverCam = value; } }

    private void Awake() {
        LoadDictionary();
        LoadMaterials();
        LoadPrefabs();
        LoadSprites();
    }

    void Start() {
        LoadCarHierarchy();
        obstaclePrevPos = Vector3.zero; //init position of obstacle spawned
        rot0 = speed.localRotation; //init rotation of speed needle

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
        linesUtils.LineRend = linesUtils.CreateLineRenderer(linesUtils.LineRendererEmpty, "LineRenderer", 2.5f, new Color32(0x3A, 0xC1, 0xAA, 0xFF), mats[2], () => linesUtils.InitGlowTexture());
        linesUtils.LineRend2 = linesUtils.CreateLineRenderer(linesUtils.CenterLineRendererEmpty, "centerLineRenderer", 0.5f, new Color32(0xFF, 0xFF, 0xFF, 0xFF), mats[2], () => linesUtils.InitGlowTexture2());
        linesUtils.CenterLineLerper();
    }

    void Update() {
        EnvironmentDetect();
        NavigationLine();
        LaneKeeping();
        SetSpeed();
    }

    void OnDrawGizmos()
    {
        float currentSpeed = gameObject.GetComponent<VehicleController>().CurrentSpeed;
        int safetyDist = Mathf.RoundToInt(Mathf.Pow(currentSpeed / 10, 2));
        if (safetyDist < 50)
            safetyDist = 50;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(gameObject.transform.position, safetyDist); //this is for debugging purposes: see OverlapSphere bounds

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(gameObject.transform.position, 3.5f);
    }

    void EnvironmentDetect()
    {
        float currentSpeed = gameObject.GetComponent<VehicleController>().CurrentSpeed;
        int safetyDist = Mathf.RoundToInt(Mathf.Pow(currentSpeed / 10, 2));
        if (safetyDist < 50)
            safetyDist = 50;
        LayerMask mask = (/*1 << LayerMask.NameToLayer("Traffic")) | (1 << LayerMask.NameToLayer("obstacle")) |*/ 1 << LayerMask.NameToLayer("EnvironmentProp") | 1 << LayerMask.NameToLayer("Signage") | 1 << LayerMask.NameToLayer("Roads")); //restrict OverlapSphere only to the layers of interest
        Collider[] colls = Physics.OverlapSphere(gameObject.transform.position, safetyDist, mask);
        HashSet<Transform> curColls = new HashSet<Transform>(); //this is to store the root transform of the colliders returned by OverlapSphere

        Timer -= Time.deltaTime; //thanks to this timer cubes and tags are instantiated only every 0.1 ms
        if (Timer <= 0f)
        {
            foreach (KeyValuePair<GameObject, GameObject> pair in cubesAndTags)
            {
                Destroy(pair.Key);
                Destroy(pair.Value);
                //Debug.Log("destroyed" + g + "update is: " + i);
            }
            cubesAndTags.Clear();

            foreach (Collider c in colls)
            {
                switch (c.gameObject.layer)
                {
                    case 16:  //EnvironmentProp
                        {
                            Bounds bounds = new Bounds();
                            if (c.transform.name.StartsWith("BasicCar"))
                            {
                                BoxCollider boxCol = c as BoxCollider;
                                bounds.center = Quaternion.Euler(c.transform.localEulerAngles) * boxCol.center;
                                bounds.size = boxCol.size;
                                Vector3 traslL = Quaternion.Euler(c.transform.localEulerAngles) * bounds.size; //infoTag trasl
                                traslL = new Vector3(traslL.x * 0.5f + 2.0f, traslL.y * 0.5f, 0);
                                Vector3 traslR = new Vector3(-traslL.x, traslL.y, traslL.z);

                                float dist = CalculateDistance(rayCastPos.position, c.transform.position + bounds.center);
                                if (dist <= 50.0f)
                                {
                                    infos[0] = "0" + " KPH"; //speed of EnvironmentProp is null
                                    infos[1] = Mathf.RoundToInt(dist).ToString() + " M";
                                    infos[2] = "";
                                    cubesAndTags.Add(CreateBoundingCube(c.transform, c.transform.position + bounds.center, bounds.size), CreateInfoTag(c.transform.position + bounds.center, traslL, traslR, sprites[28], transform.rotation, infos, Color.white));
                                }
                            }
                            else if (c.transform.tag.Equals("Sign"))
                            {
                                BoxCollider boxCol = c as BoxCollider;
                                bounds.center = boxCol.center;
                                bounds.size = boxCol.size * 1.25f;
                                float dist = CalculateDistance(rayCastPos.position, c.transform.position + bounds.center);
                                if (dist <= 50.0f)
                                {
                                    infos[0] = "0" + " KPH"; //speed of EnvironmentProp is null
                                    infos[1] = Mathf.RoundToInt(dist).ToString() + " M";
                                    infos[2] = "";
                                    cubesAndTags.Add(CreateBoundingCube(c.transform, c.transform.position + bounds.center, bounds.size), new GameObject()); //at the moment i don't need an InfoTag
                                }
                            }
                            switch (c.transform.name)
                            {
                                case "streetlight":
                                    {
                                        CapsuleCollider capsCol = c as CapsuleCollider;
                                        bounds.center = Quaternion.Euler(c.transform.localEulerAngles) * capsCol.center;
                                        bounds.size = new Vector3(capsCol.radius * 4.0f, capsCol.radius * 4.0f, capsCol.height);
                                        Vector3 traslL = Quaternion.Euler(c.transform.localEulerAngles) * bounds.size; //infoTag trasl
                                        traslL = new Vector3(traslL.x * 0.5f + 2.0f, 0, 0);
                                        Vector3 traslR = new Vector3(-traslL.x, traslL.y, traslL.z);

                                        float dist = CalculateDistance(rayCastPos.position, c.transform.position + bounds.center);
                                        if (dist <= 10.0f)
                                        {
                                            infos[0] = "0" + " KPH"; //speed of EnvironmentProp is null
                                            infos[1] = Mathf.RoundToInt(dist).ToString() + " M";
                                            infos[2] = "";
                                            RaycastHit hit;
                                            Vector3 fwd = rayCastPos.TransformDirection(Vector3.forward);
                                            bool isHit = Physics.Raycast(rayCastPos.position, fwd, out hit, 10.0f);
                                            if (isHit)
                                            {
                                                //Debug.DrawLine(rayCastPos.position, hit.point, Color.yellow);
                                                GameObject boundingCube = CreateBoundingCube(c.transform, c.transform.position + bounds.center, bounds.size);
                                                GameObject infotag = CreateInfoTag(c.transform.position + bounds.center, traslL, traslR, sprites[29], transform.rotation, infos, Color.red);
                                                StartCoroutine(HideUnhide(infotag, isHit));
                                                cubesAndTags.Add(boundingCube, infotag);
                                            }
                                            else
                                            {
                                                //Debug.DrawLine(rayCastPos.position, rayCastPos.position + (fwd * 10.0f), Color.magenta);
                                                cubesAndTags.Add(CreateBoundingCube(c.transform, c.transform.position + bounds.center, bounds.size), CreateInfoTag(c.transform.position + bounds.center, traslL, traslR, sprites[28], transform.rotation, infos, Color.white));
                                            }
                                                

                                        }
                                    }
                                    break;

                                case "tree_dec01":
                                    {
                                        Transform oldParent = c.transform.parent;
                                        c.transform.parent = null; //I am forced to unparent since I can't take into account the parent scale in the bounding box computation
                                        CapsuleCollider capsCol = c as CapsuleCollider;
                                        bounds.center = Quaternion.Euler(c.transform.localEulerAngles) * new Vector3(capsCol.center.x * c.transform.localScale.x, capsCol.center.y * c.transform.localScale.y, capsCol.center.z * c.transform.localScale.z);
                                        bounds.size = new Vector3(capsCol.radius * 2.0f * c.transform.localScale.x, capsCol.radius * 2.0f * c.transform.localScale.y, capsCol.height * c.transform.localScale.z);
                                        Vector3 traslL = Quaternion.Euler(c.transform.localEulerAngles) * bounds.size; //infoTag trasl
                                        traslL = new Vector3(traslL.x * 0.5f + 3.0f, 0, 0);
                                        Debug.Log(c.transform.name + ", " + traslL);
                                        Vector3 traslR = new Vector3(-traslL.x, traslL.y, traslL.z);

                                        c.transform.parent = oldParent;

                                        float dist = CalculateDistance(rayCastPos.position, c.transform.position + bounds.center);
                                        if (dist <= 10.0f)
                                        {
                                            infos[0] = "0" + " KPH"; //speed of EnvironmentProp is null
                                            infos[1] = Mathf.RoundToInt(dist).ToString() + " M";
                                            infos[2] = "";
                                            cubesAndTags.Add(CreateBoundingCube(c.transform, c.transform.position + bounds.center, bounds.size), CreateInfoTag(c.transform.position + bounds.center, traslL, traslR, sprites[28], transform.rotation, infos, Color.white));
                                        }
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
                                        BoxCollider boxCol = c as BoxCollider;
                                        bounds.center = Quaternion.Euler(c.transform.localEulerAngles) * new Vector3(boxCol.center.x * c.transform.localScale.x, boxCol.center.y * c.transform.localScale.y, boxCol.center.z * c.transform.localScale.z);
                                        bounds.size = new Vector3(boxCol.size.x * c.transform.localScale.x, boxCol.size.y * c.transform.localScale.y, boxCol.size.z * c.transform.localScale.z);
                                        Vector3 traslL = Quaternion.Euler(c.transform.localEulerAngles) * bounds.size; //infoTag trasl
                                        traslL = new Vector3(traslL.x * 0.5f + 2.0f, traslL.y * 0.5f, 0);
                                        Vector3 traslR = new Vector3(-traslL.x, traslL.y, traslL.z);

                                        float dist = CalculateDistance(rayCastPos.position, c.transform.position + bounds.center);
                                        if (dist <= 10.0f)
                                        {
                                            infos[0] = "0" + " KPH"; //speed of EnvironmentProp is null
                                            infos[1] = Mathf.RoundToInt(dist).ToString() + " M";
                                            infos[2] = "";
                                            cubesAndTags.Add(CreateBoundingCube(c.transform, c.transform.position + bounds.center, bounds.size), CreateInfoTag(c.transform.position + bounds.center, traslL, traslR, sprites[28], transform.rotation, infos, Color.white));
                                        }
                                    }
                                    break;
                            }
                        }
                        break;
                    case 17:  //Signage
                        {
                            Bounds bounds = new Bounds();
                            if (c.transform.parent.name.Equals("StreetArrows") || c.transform.name.Equals("Post"))
                            {
                                break;
                            }

                            BoxCollider boxCol = c as BoxCollider;
                            bounds.center = boxCol.center;
                            bounds.size = boxCol.size * 1.25f;
                            float dist = CalculateDistance(rayCastPos.position, c.transform.position + bounds.center);
                            if (dist <= 50.0f)
                            {
                                infos[0] = "0" + " KPH"; //speed of EnvironmentProp is null
                                infos[1] = Mathf.RoundToInt(dist).ToString() + " M";
                                infos[2] = "";
                                cubesAndTags.Add(CreateBoundingCube(c.transform, c.transform.position + bounds.center, bounds.size), new GameObject());
                            }
                        }
                        break;
                    case 12: //obstacle
                        { 
                            Bounds bounds = ComputeBounds(c.transform);
                            infos[0] = Mathf.RoundToInt(CalculateObstacleSpeed(c.transform)).ToString() + " KPH";
                            infos[1] = Mathf.RoundToInt(CalculateDistance(rayCastPos.position, bounds.center)).ToString() + " M";
                            infos[2] = "";
                            //cubesAndTags.Add(CreateBoundingCube(c.transform, bounds.center, bounds.size), CreateInfoTag(bounds.center, bounds.size, sprites[28], infos));
                        }
                        break;
                    case 8:
                        {   //Traffic
                        if (!curColls.Contains(c.transform.root))
                        { //this is in order to check if an object has more colliders than one; if so, one is sufficient so simply discard other otherwise the cube instantiated would be equal to the number of colliders
                            Bounds bounds = ComputeBounds(c.transform.root);
                            TrafPCH trafPCH = c.transform.root.GetComponent<TrafPCH>();
                            if (trafPCH != null)
                            {
                                float speed = trafPCH.currentSpeed;
                                infos[0] = Mathf.RoundToInt(speed * 3.6f).ToString() + " KPH";
                                infos[1] = Mathf.RoundToInt(CalculateDistance(rayCastPos.position, bounds.center)).ToString() + " M";
                                infos[2] = "";
                                //cubesAndTags.Add(CreateBoundingCube(c.transform.root, bounds.center, bounds.size), CreateInfoTag(bounds.center, bounds.size, sprites[0], infos));
                                curColls.Add(c.transform.root);
                            }
                        }
                        }
                        break;
                    case 18: //Roads
                        {
                            if (c.transform.name.StartsWith("TrafficLight"))
                            {
                                TrafLightState currentState = c.transform.GetComponent<TrafficLightContainer>().State;
                                Dictionary<string, Transform> trafLightChildren = new Dictionary<string, Transform>();
                                List<Bounds> bounds = new List<Bounds>();
                                foreach (Transform t in c.transform)
                                    if (t.name.StartsWith("Light") && t.gameObject.activeInHierarchy) //there are some trafficLights not active so I have to exclude them
                                    {
                                        trafLightChildren.Add(t.name, t);
                                        bounds.Add(ComputeBounds(t));
                                    }
                                if (trafLightChildren.Count != 0) { //I have to check whether the dictionary is empty or not since there may be some inactive trafficLights which in the next code I will erronously reference
                                    infos[0] = infos[1] = "";
                                    if (currentState.Equals(TrafLightState.GREEN))
                                    {
                                        infos[2] = "GO";
                                        cubesAndTags.Add(CreateBoundingCube(trafLightChildren["Light0"], bounds[0].center, bounds[0].size), CreateInfoTag(bounds[0].center, new Vector3(0, 0, -bounds[0].size.x), new Vector3(0, 0, -bounds[0].size.x), sprites[25], Quaternion.Euler(0f,90f,0f) * trafLightChildren["Light0"].rotation, infos, Color.white)); //I add another 90 degrees to position the tag orthogonally with respect to the trafficLight
                                        cubesAndTags.Add(CreateBoundingCube(trafLightChildren["Light1"], bounds[1].center, bounds[1].size), CreateInfoTag(bounds[1].center, new Vector3(0, 0, -bounds[1].size.x), new Vector3(0, 0, -bounds[1].size.x), sprites[25], Quaternion.Euler(0f, 90f, 0f) * trafLightChildren["Light1"].rotation, infos, Color.white));
                                    }
                                    else if (currentState.Equals(TrafLightState.YELLOW))
                                    {
                                        infos[2] = "GO";
                                        cubesAndTags.Add(CreateBoundingCube(trafLightChildren["Light0"], bounds[0].center, bounds[0].size), CreateInfoTag(bounds[0].center, new Vector3(0, 0, -bounds[0].size.x), new Vector3(0, 0, -bounds[0].size.x), sprites[27], Quaternion.Euler(0f, 90f, 0f) * trafLightChildren["Light0"].rotation, infos, Color.white));
                                        cubesAndTags.Add(CreateBoundingCube(trafLightChildren["Light1"], bounds[1].center, bounds[1].size), CreateInfoTag(bounds[1].center, new Vector3(0, 0, -bounds[1].size.x), new Vector3(0, 0, -bounds[1].size.x), sprites[27], Quaternion.Euler(0f, 90f, 0f) * trafLightChildren["Light1"].rotation, infos, Color.white));
                                    }
                                    else
                                    {
                                        infos[2] = "STOP";
                                        cubesAndTags.Add(CreateBoundingCube(trafLightChildren["Light0"], bounds[0].center, bounds[0].size), CreateInfoTag(bounds[0].center, new Vector3(0, 0, -bounds[0].size.x), new Vector3(0, 0, -bounds[0].size.x), sprites[26], Quaternion.Euler(0f, 90f, 0f) * trafLightChildren["Light0"].rotation, infos, Color.white));
                                        cubesAndTags.Add(CreateBoundingCube(trafLightChildren["Light1"], bounds[1].center, bounds[1].size), CreateInfoTag(bounds[1].center, new Vector3(0, 0, -bounds[1].size.x), new Vector3(0, 0, -bounds[1].size.x), sprites[26], Quaternion.Euler(0f, 90f, 0f) * trafLightChildren["Light1"].rotation, infos, Color.white));
                                    }
                                }
                                
                            }
                        }
                        break;
                }
            }
            Timer = 0.1f;
        }
        //i++;
    }

    GameObject CreateBoundingCube(Transform tr, Vector3 center, Vector3 size)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.position = center;
        go.transform.rotation = tr.rotation;
        go.transform.localScale = size;
        go.layer = LayerMask.NameToLayer("Graphics");
        go.GetComponent<Renderer>().material = mats[1];
        go.GetComponent<BoxCollider>().isTrigger = true; //this in order to avoid to register collision of incoming Car rigidbody
        return go;
    }

    GameObject CreateInfoTag(Vector3 center, Vector3 traslL, Vector3 traslR, Sprite sprite, Quaternion rot, string[] infos, Color col)
    {
        GameObject infoTag = Instantiate(prefabs[2], center, rot);
        Vector3 relativePoint = transform.InverseTransformPoint(center); //this is to recognize if the object is to the left/right with respect to the car
        if (relativePoint.x < 0.0f)
            //object is to the left
            infoTag.transform.Translate(traslL, Space.Self);
        else if (relativePoint.x >= 0.0f)
            //object is to the right or directly ahead
            infoTag.transform.Translate(traslR, Space.Self);

        Dictionary<string, Transform> infoTagChildren = new Dictionary<string, Transform>();
        foreach (Transform t in infoTag.transform)
        {
            foreach (Transform tra in t.transform)
                infoTagChildren.Add(tra.name, tra);
        }
        infoTagChildren["Image"].GetComponent<Image>().sprite = sprite;
        Text kph = infoTagChildren["KPH"].GetComponent<Text>();
        Text dist = infoTagChildren["DIST"].GetComponent<Text>();
        kph.text = infos[0];
        dist.text = infos[1];
        kph.color = dist.color = col;
        infoTagChildren["TRAFLIGHT"].GetComponent<Text>().text = infos[2];
        return infoTag;
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
            if (t.name.Equals("rayCastPos"))
            {
                rayCastPos = t;
                break;
            }

        //Debug.Log(speed);
        //Debug.Log(needle);
        //Debug.Log(rayCastPos);
    }

    void LoadDictionary()
    {
        ReadCSVFile(Application.streamingAssetsPath + "/angle(speedKph).csv");
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
        Debug.DrawLine(src, dst, Color.green);
        return dist;
    }

    Transform CalculatePointDistance(Collider[] colls, string pointType)
    {
        Transform nearest = null; //in URBAN I considr the nearest point otherwise the furthest would be a point located in the near lanes
        float nearDist = 9999;
        for (int i = 0; i < colls.Length; ++i)
        {
            if (colls[i].tag.Equals(pointType))
            { //consider only Centerpoints and ignore other Colliders that belong to Graphics 
                float thisDist = (transform.position - cacheColls[i].transform.position).sqrMagnitude; //this is squaredMagnitude i.e. magnitude without square root
                if (thisDist < nearDist)
                {
                    nearDist = thisDist;
                    nearest = cacheColls[i].transform;
                }
            }
        }
        return nearest;
    }

    Transform CalculatePointDistance(string pointType)
    {
        Transform furthest = null; //maybe even the nearest may go well since I consider the nextwayPoint from the current chosen
        float farDist = 0;
        for (int i = 0; i < cacheColls.Count; ++i)
        {
            if (cacheColls[i].tag.Equals(pointType))
            { //consider only Centerpoints and ignore other Colliders that belong to Graphics 
                float thisDist = (transform.position - cacheColls[i].transform.position).sqrMagnitude; //this is squaredMagnitude i.e. magnitude without square root
                if (thisDist > farDist)
                {
                    farDist = thisDist;
                    furthest = cacheColls[i].transform;
                }
            }
        }
        return furthest;
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

    int MonitorLane(Collider[] colls)
    {
        int j = 0; //index to track if there is at least one waypoint in the oncoming lane
        for (int i = 0; i < colls.Length; ++i)
        {
            if (Vector3.Dot(gameObject.transform.TransformDirection(Vector3.forward), colls[i].transform.TransformDirection(Vector3.forward)) < 0 && colls[i].tag.Equals("Waypoint"))
                j++;
        }
        return j;
    }

    int MonitorLane(string pointType)
    {
        int j = 0; //index to track if there is at least one waypoint in the oncoming lane
        for (int i = 0; i < cacheColls.Count; ++i)
        {
            if (Vector3.Dot(gameObject.transform.TransformDirection(Vector3.forward), cacheColls[i].transform.TransformDirection(Vector3.forward)) < 0 && cacheColls[i].tag.Equals(pointType))
                j++;
        }
        return j;
    }

    void NavigationLine()
    {
        LayerMask mask = (1 << LayerMask.NameToLayer("Graphics"));   //restrict OverlapSphere only to the layers of interest
        Collider[] colls = Physics.OverlapSphere(gameObject.transform.position, 3.5f, mask); //remember that in order to let this work it needs to be enabled Physics--->Queries hit triggers
        foreach (Collider c in colls)
            Debug.Log(c);
        cacheColls = new List<Collider>(colls);

        Transform nearest = CalculatePointDistance(colls, "Waypoint");
        if (nearest != null /*&& MonitorLane(colls) == 0*/) //I am crossing the incoming lane
            linesUtils.DrawLineUrban(nearest, gameObject);
        else
            linesUtils.LineRend.positionCount = 0; //delete the LineRenderer
    }

    void CreatePathEnhanced()
    {
        pathEnhanced = new GameObject("PathEnhancedUrban");
        pathEnhanced.AddComponent<AutoPathHelperUrban>();
    }

    void CreateCenterLine()
    {
        centerLine = Instantiate(prefabs[1]);
    }

    void LaneKeeping()
    {
        Transform furthest = CalculatePointDistance("Centerpoint");
        if (furthest != null)
        {
            int index = furthest.GetSiblingIndex(); //this is the index of the furthest node within the circle, considering centerLine
            int lane = MonitorLane("Waypoint"); //this is to understand if I am partially in the oncoming lane
            Vector3 projectedPoint = linesUtils.CalculateProjectedPoint(transform.position, centerLine.transform.GetChild(index).position, centerLine.transform.GetChild(index - 1).position);
            float dist = Vector3.Distance(transform.position, projectedPoint);
            
            if (dist < 4.0f && lane == 0)
                linesUtils.CenterLineColor = linesUtils.ChangeMatByDistance(dist);
            else if (dist >= 4.0f && lane == 0)
                linesUtils.CenterLineColor = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
            else if (lane != 0)
                linesUtils.CenterLineColor = new Color32(0xFF, 0x00, 0x00, 0xFF);
            linesUtils.DrawLineUrban(furthest, gameObject, projectedPoint, MonitorLane("Centerpoint")); //this is to understand the trip direction in order to draw the centerLine
        }
        cacheColls.Clear();


    }

    void SetSpeed()
    {
        float targetAngle;
        VehicleController vehicleController = gameObject.GetComponent<VehicleController>();
        speed.GetComponent<Text>().text = Mathf.RoundToInt(vehicleController.CurrentSpeed).ToString(); //speed in kph
        if (CSVDictionary.TryGetValue(Mathf.RoundToInt(vehicleController.CurrentSpeed), out targetAngle))
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

    IEnumerator HideUnhide(GameObject infoTag, bool isHit)
    {
        Dictionary<string, Transform> infoTagChildren = new Dictionary<string, Transform>();
        foreach (Transform t in infoTag.transform)
        {
            foreach (Transform tra in t.transform)
                infoTagChildren.Add(tra.name, tra);
        }
        while (isHit == true)
        {
            yield return (new WaitForSeconds(0.5f));
            infoTagChildren["KPH"].GetComponent<Text>().enabled = true;
            infoTagChildren["DIST"].GetComponent<Text>().enabled = true;
            yield return (new WaitForSeconds(0.5f));
            infoTagChildren["KPH"].GetComponent<Text>().enabled = false;
            infoTagChildren["DIST"].GetComponent<Text>().enabled = false;
        }
    }
}
