using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using MKGlowSystem;

public class EnvironmentSensingAltUrbanTrigger : MonoBehaviour
{

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
    public enum Lane { RIGHT, OPPOSITE }
    private HashSet<Transform> curColls = new HashSet<Transform>(); //this is to store the root transform of the colliders 
    private Transform rayCastPos;
    private RectTransform speed;
    private RectTransform needle;
    private Rigidbody rigidbody;

    private DriverCamera driverCam;

    public GameObject SpeedPanel { set { speedPanel = value; } }
    public DriverCamera DriverCam { set { driverCam = value; } }

    public class CubesAndTags
    {
        public List<GameObject> boundingCube;
        public List<GameObject> infoTag;
        public List<Bounds> bounds;

        public CubesAndTags()
        {
            this.boundingCube = new List<GameObject>();
            this.infoTag = new List<GameObject>();
            this.bounds = new List<Bounds>();
        }

        public void AddElements(GameObject boundingCube, GameObject infoTag, Bounds bounds)
        {
            this.boundingCube.Add(boundingCube);
            this.infoTag.Add(infoTag);
            this.bounds.Add(bounds);
        }
    }

    //void OnEnable()
    //{
    //    TrafSpawnerPCH.OnSpawnHeaps += HandleOnSpawnHeaps;
    //}

    //void OnDisable()
    //{
    //    TrafSpawnerPCH.OnSpawnHeaps -= HandleOnSpawnHeaps;
    //}


    private void Awake()
    {
        LoadDictionary();
        LoadMaterials();
        LoadPrefabs();
        LoadSprites();
    }

    void Start()
    {
        LoadCarHierarchy();
        rigidbody = gameObject.GetComponent<Rigidbody>();
        obstaclePrevPos = Vector3.zero; //init position of obstacle spawned
        rot0 = speed.localRotation; //init rotation of speed needle
        mask = (1 << LayerMask.NameToLayer("Traffic")) | (1 << LayerMask.NameToLayer("obstacle")) | (1 << LayerMask.NameToLayer("EnvironmentProp")) | (1 << LayerMask.NameToLayer("Signage") | 1 << LayerMask.NameToLayer("Roads")); //restrict OnTriggerEnter only to the layers of interest

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

        StartCoroutine(NavigationLine(0.25f));
        StartCoroutine(LaneKeeping(0.25f));
    }

    void Update()
    {
        SetSpeed();
    }

    void OnDrawGizmos()
    {
        //Gizmos.color = Color.blue;
        //Gizmos.DrawWireSphere(gameObject.transform.position, 3.5f);
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
                        
                            if (other.transform.tag.Equals("ParkedCar"))
                            {
                                BoxCollider boxCol = other as BoxCollider;
                                bounds.center = Quaternion.Euler(other.transform.localEulerAngles) * boxCol.center;
                                bounds.size = boxCol.size;
                            }
                            else if (other.transform.tag.Equals("Sign"))
                            {
                                BoxCollider boxCol = other as BoxCollider;
                                bounds.center = boxCol.center;
                                bounds.size = boxCol.size * 1.25f;
                            }
                            switch (other.transform.name)
                            {
                                case "streetlight":
                                    {
                                        CapsuleCollider capsCol = other as CapsuleCollider;
                                        bounds.center = Quaternion.Euler(other.transform.localEulerAngles) * capsCol.center;
                                        bounds.size = new Vector3(capsCol.radius * 4.0f, capsCol.radius * 4.0f, capsCol.height);
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
                                    }
                                    break;
                            }
                            GameObject boundingCube = CreateBoundingCube(other.transform, other.transform.position + bounds.center, bounds.size);
                            GameObject infoTag = CreateInfoTag(other.transform.position + bounds.center);
                            boundingCube.GetComponent<Renderer>().enabled = false;
                            infoTag.GetComponent<Canvas>().enabled = false;
                            CubesAndTags cubesAndTags = new CubesAndTags();
                            cubesAndTags.AddElements(boundingCube, infoTag, bounds);
                            IDsAndGos.Add(other.gameObject.GetInstanceID(), cubesAndTags);
                    }
                    break;
                case 17:
                    { //Signage
                        Bounds bounds = new Bounds();
                        if (other.transform.parent.name.Equals("StreetArrows") || other.transform.name.Equals("Post"))
                        {
                            break;
                        }

                        BoxCollider boxCol = other as BoxCollider;
                        bounds.center = boxCol.center;
                        bounds.size = boxCol.size * 1.25f;
                        GameObject boundingCube = CreateBoundingCube(other.transform, other.transform.position + bounds.center, bounds.size);
                        GameObject infoTag = CreateInfoTag(other.transform.position + bounds.center);
                        boundingCube.GetComponent<Renderer>().enabled = false;
                        infoTag.GetComponent<Canvas>().enabled = false;
                        CubesAndTags cubesAndTags = new CubesAndTags();
                        cubesAndTags.AddElements(boundingCube, infoTag, bounds);
                        IDsAndGos.Add(other.gameObject.GetInstanceID(), cubesAndTags);
                    }
                    break;
                case 12:
                    {  //obstacle
                        Bounds bounds = ComputeBounds(other.transform);
                        GameObject boundingCube = CreateBoundingCube(other.transform, bounds.center, bounds.size);
                        GameObject infoTag = CreateInfoTag(bounds.center);
                        boundingCube.transform.SetParent(other.transform);
                        boundingCube.transform.localPosition = Vector3.zero;
                        boundingCube.GetComponent<Renderer>().enabled = false;
                        infoTag.GetComponent<Canvas>().enabled = false;
                        CubesAndTags cubesAndTags = new CubesAndTags(); //I need to recompute the bounds since obstacles are dynamic, so I pass an empty bounds here
                        cubesAndTags.AddElements(boundingCube, infoTag, bounds);
                        IDsAndGos.Add(other.gameObject.GetInstanceID(), cubesAndTags);
                    }
                    break;

                case 8:
                    {
                        if (!curColls.Contains(other.transform.root))
                        { //this is in order to check if an object has more colliders than one; if so, one is sufficient so simply discard other otherwise the cube instantiated would be equal to the number of colliders
                            Bounds bounds = ComputeBounds(other.transform.root);
                            bounds = ComputeBounds(other.transform.root);
                            GameObject boundingCube = CreateBoundingCube(other.transform, bounds.center, bounds.size);
                            GameObject infoTag = CreateInfoTag(bounds.center);
                            boundingCube.transform.SetParent(other.transform);
                            boundingCube.transform.localPosition = Vector3.zero;
                            boundingCube.GetComponent<Renderer>().enabled = false;
                            infoTag.GetComponent<Canvas>().enabled = false;
                            CubesAndTags cubesAndTags = new CubesAndTags(); //I need to recompute the bounds since obstacles are dynamic, so I pass an empty bounds here
                            cubesAndTags.AddElements(boundingCube, infoTag, bounds);
                            IDsAndGos.Add(other.gameObject.GetInstanceID(), cubesAndTags);
                            curColls.Add(other.transform.root);
                        }
                    }
                    break;
                case 18: //Roads
                    {
                        if (other.transform.name.StartsWith("TrafficLight"))
                        {
                            Dictionary<string, Transform> trafLightChildren = new Dictionary<string, Transform>();
                            List<Bounds> bounds = new List<Bounds>();
                            foreach (Transform t in other.transform)
                                if (t.name.StartsWith("Light") && t.gameObject.activeInHierarchy) //there are some trafficLights not active so I have to exclude them
                                {
                                    trafLightChildren.Add(t.name, t);
                                    bounds.Add(ComputeBounds(t));
                                }
                            if (trafLightChildren.Count != 0)
                            { //I have to check whether the dictionary is empty or not since there may be some inactive trafficLights which in the next code I will erronously reference
                                GameObject boundingCube0 = CreateBoundingCube(trafLightChildren["Light0"], bounds[0].center, bounds[0].size);
                                GameObject infoTag0 = CreateInfoTag(bounds[0].center);
                                boundingCube0.GetComponent<Renderer>().enabled = false;
                                infoTag0.GetComponent<Canvas>().enabled = false;
                                GameObject boundingCube1 = CreateBoundingCube(trafLightChildren["Light1"], bounds[1].center, bounds[1].size);
                                GameObject infoTag1 = CreateInfoTag(bounds[1].center);
                                boundingCube1.GetComponent<Renderer>().enabled = false;
                                infoTag1.GetComponent<Canvas>().enabled = false;
                                CubesAndTags cubesAndTags = new CubesAndTags(); //I need to recompute the bounds since obstacles are dynamic, so I pass an empty bounds here
                                cubesAndTags.AddElements(boundingCube0, infoTag0, bounds[0]);
                                cubesAndTags.AddElements(boundingCube1, infoTag1, bounds[1]);
                                IDsAndGos.Add(other.gameObject.GetInstanceID(), cubesAndTags);
                            }

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
            Destroy(IDsAndGos[other.gameObject.GetInstanceID()].boundingCube[0]);
            Destroy(IDsAndGos[other.gameObject.GetInstanceID()].infoTag[0]);
            IDsAndGos.Remove(other.gameObject.GetInstanceID());
        }
        

    }

    void OnTriggerStay(Collider other)
    {
        if (IDsAndGos.ContainsKey(other.gameObject.GetInstanceID())) //it is a Collider which is effectively present in the dictionary. This is to exclude the colliders which share the same level of the dictionary collider but are not in the dictionary
        {
            float dist = CalculateDistance(rayCastPos.position, other.transform.position); //this distance does not include bounds since it cannot always be precomputed, for example, this is the case of moving objects
            float dist1 = 0;
            Vector3 trasl = Vector3.zero;

            switch (other.gameObject.layer)
            {
                case 16:
                    {
                        Bounds bounds = IDsAndGos[other.gameObject.GetInstanceID()].bounds[0];
                        if (dist <= 10.0f)
                        {
                            if (other.transform.tag.Equals("ParkedCar"))
                            {
                                trasl = Quaternion.Euler(-other.transform.localEulerAngles) * bounds.size; //infoTag trasl
                                trasl = new Vector3(Mathf.Abs(trasl.z) * 0.5f + 2.0f, Mathf.Abs(trasl.y) * 0.5f, 0);
                                UpdateInfoTag(other, "0" + " KPH", Mathf.RoundToInt(dist).ToString() + " M", "", sprites[64], bounds, dist, 0, trasl);
                            } 
                            
                            switch (other.transform.name)
                            {
                                case "streetlight":
                                    {
                                        trasl = Quaternion.Euler(-other.transform.localEulerAngles) * bounds.size; //infoTag trasl
                                        trasl = new Vector3(Mathf.Abs(trasl.z) * 0.5f + 2.0f, 0, 0);
                                        UpdateInfoTag(other, "0" + " KPH", Mathf.RoundToInt(dist).ToString() + " M", "", sprites[64], bounds, dist, 0, trasl);
                                    }
                                    break;
                                case "tree_dec01":
                                    {
                                        trasl = Quaternion.Euler(-other.transform.localEulerAngles) * bounds.size; //infoTag trasl
                                        trasl = new Vector3(Mathf.Abs(trasl.z) * 0.5f + 3.0f, 0, 0);
                                        UpdateInfoTag(other, "0" + " KPH", Mathf.RoundToInt(dist).ToString() + " M", "", sprites[64], bounds, dist, 0, trasl);
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
                                        trasl = Quaternion.Euler(-other.transform.localEulerAngles) * bounds.size; //infoTag trasl
                                        trasl = new Vector3(Mathf.Abs(trasl.z) * 0.5f + 2.0f, Mathf.Abs(trasl.y) * 0.5f, 0);
                                        UpdateInfoTag(other, "0" + " KPH", Mathf.RoundToInt(dist).ToString() + " M", "", sprites[64], bounds, dist, 0, trasl);
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            IDsAndGos[other.gameObject.GetInstanceID()].boundingCube[0].GetComponent<Renderer>().enabled = false;
                            IDsAndGos[other.gameObject.GetInstanceID()].infoTag[0].GetComponent<Canvas>().enabled = false;
                        }
                        if (other.transform.tag.Equals("Sign"))
                        {
                            Sprite sprite = ReturnSignPic(other.name, sprites);
                            if (Vector3.Dot(gameObject.transform.TransformDirection(Vector3.forward), other.transform.TransformDirection(Vector3.up)) < 0 && sprite != null)
                            {
                                UpdateInfoTag(other, "", "", "", sprite, new Bounds(Vector3.zero, Vector3.zero), dist, 0, Vector3.zero);
                            } else
                            {
                                IDsAndGos[other.gameObject.GetInstanceID()].boundingCube[0].GetComponent<Renderer>().enabled = false;
                                IDsAndGos[other.gameObject.GetInstanceID()].infoTag[0].GetComponent<Canvas>().enabled = false;
                            }
                        }
                    }
                    break;
                case 17: //Signage
                    {
                        Sprite sprite = ReturnSignPic(other.name, sprites);
                        if (Vector3.Dot(gameObject.transform.TransformDirection(Vector3.forward), other.transform.TransformDirection(Vector3.up)) < 0 && sprite != null)
                        {
                            UpdateInfoTag(other, "", "", "", sprite, new Bounds(Vector3.zero, Vector3.zero), dist, 0, Vector3.zero);
                        }
                        else
                        {
                            IDsAndGos[other.gameObject.GetInstanceID()].boundingCube[0].GetComponent<Renderer>().enabled = false;
                            IDsAndGos[other.gameObject.GetInstanceID()].infoTag[0].GetComponent<Canvas>().enabled = false;
                        }
                    }
                    break;
                case 12:
                    {
                        Bounds bounds = IDsAndGos[other.gameObject.GetInstanceID()].bounds[0];
                        UpdateInfoTag(other, Mathf.RoundToInt(CalculateObstacleSpeed(other.transform)).ToString() + " KPH", Mathf.RoundToInt(dist).ToString() + " M", "", sprites[28], bounds, dist, 0, Vector3.zero);
                    }
                    break;
                case 8:
                    {
                        Bounds bounds = IDsAndGos[other.gameObject.GetInstanceID()].bounds[0];
                        TrafAIMotor trafAIMotor = other.transform.root.GetComponent<TrafAIMotor>();
                        if (trafAIMotor != null)
                        {
                            float speed = trafAIMotor.currentSpeed;
                            UpdateInfoTag(other, Mathf.RoundToInt(speed * 3.6f).ToString() + " KPH", Mathf.RoundToInt(dist).ToString() + " M", "", sprites[0], bounds, dist, 0, Vector3.zero);
                        }
                    }
                    break;

                case 18: //Roads
                    {
                        if (other.transform.name.StartsWith("TrafficLight"))
                        {
                            //Bounds bounds =  IDsAndGos[other.gameObject.GetInstanceID()].bounds[0]; //I don't need them! Maybe I'd reconsider to avoid to pass them in OnTriggerEnter()
                            //Bounds bounds1 = IDsAndGos[other.gameObject.GetInstanceID()].bounds[1];
                            Transform textPanel = IDsAndGos[other.gameObject.GetInstanceID()].infoTag[0].transform.GetChild(0);
                            Transform imagePanel = IDsAndGos[other.gameObject.GetInstanceID()].infoTag[0].transform.GetChild(1);
                            Transform textPanel1 = IDsAndGos[other.gameObject.GetInstanceID()].infoTag[1].transform.GetChild(0);
                            Transform imagePanel1 = IDsAndGos[other.gameObject.GetInstanceID()].infoTag[1].transform.GetChild(1);

                            textPanel.transform.GetChild(0).GetComponent<Text>().text = textPanel.transform.GetChild(1).GetComponent<Text>().text = "";
                            textPanel1.transform.GetChild(0).GetComponent<Text>().text = textPanel1.transform.GetChild(1).GetComponent<Text>().text = "";

                            TrafLightState currentState = other.transform.GetComponent<TrafficLightContainer>().State;
                            if (currentState.Equals(TrafLightState.GREEN))
                            {
                                imagePanel.transform.GetChild(0).GetComponent<Image>().sprite = imagePanel1.transform.GetChild(0).GetComponent<Image>().sprite = sprites[61];
                                textPanel.transform.GetChild(2).GetComponent<Text>().text = textPanel1.transform.GetChild(2).GetComponent<Text>().text = "GO";
                            }
                            else if (currentState.Equals(TrafLightState.YELLOW))
                            {
                                imagePanel.transform.GetChild(0).GetComponent<Image>().sprite = imagePanel1.transform.GetChild(0).GetComponent<Image>().sprite = sprites[62];
                                textPanel.transform.GetChild(2).GetComponent<Text>().text = textPanel1.transform.GetChild(2).GetComponent<Text>().text = "GO";
                            }
                            else
                            {
                                imagePanel.transform.GetChild(0).GetComponent<Image>().sprite = imagePanel1.transform.GetChild(0).GetComponent<Image>().sprite = sprites[63];
                                textPanel.transform.GetChild(2).GetComponent<Text>().text = textPanel1.transform.GetChild(2).GetComponent<Text>().text = "STOP";
                            }
                            IDsAndGos[other.gameObject.GetInstanceID()].infoTag[0].transform.rotation = Quaternion.Euler(0f, 90f, 0f) * IDsAndGos[other.gameObject.GetInstanceID()].boundingCube[0].transform.rotation;
                            IDsAndGos[other.gameObject.GetInstanceID()].infoTag[0].transform.position = IDsAndGos[other.gameObject.GetInstanceID()].boundingCube[0].transform.position + IDsAndGos[other.gameObject.GetInstanceID()].infoTag[0].transform.TransformDirection(-Vector3.forward);
                            IDsAndGos[other.gameObject.GetInstanceID()].infoTag[1].transform.rotation = Quaternion.Euler(0f, 90f, 0f) * IDsAndGos[other.gameObject.GetInstanceID()].boundingCube[1].transform.rotation;
                            IDsAndGos[other.gameObject.GetInstanceID()].infoTag[1].transform.position = IDsAndGos[other.gameObject.GetInstanceID()].boundingCube[1].transform.position + IDsAndGos[other.gameObject.GetInstanceID()].infoTag[1].transform.TransformDirection(-Vector3.forward);

                            IDsAndGos[other.gameObject.GetInstanceID()].boundingCube[0].GetComponent<Renderer>().enabled = IDsAndGos[other.gameObject.GetInstanceID()].boundingCube[1].GetComponent<Renderer>().enabled = true;
                            IDsAndGos[other.gameObject.GetInstanceID()].infoTag[0].GetComponent<Canvas>().enabled = IDsAndGos[other.gameObject.GetInstanceID()].infoTag[1].GetComponent<Canvas>().enabled = true;
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

    void UpdateInfoTag(Collider other, string text, string text1, string text2, Sprite sprite, Bounds bounds, float dist, float dist1, Vector3 trasl)
    {
        Renderer visibility = IDsAndGos[other.gameObject.GetInstanceID()].boundingCube[0].GetComponent<Renderer>();
        Canvas canvas = IDsAndGos[other.gameObject.GetInstanceID()].infoTag[0].GetComponent<Canvas>();
        Transform textPanel = IDsAndGos[other.gameObject.GetInstanceID()].infoTag[0].transform.GetChild(0);
        Transform imagePanel = IDsAndGos[other.gameObject.GetInstanceID()].infoTag[0].transform.GetChild(1);

        Renderer visibility1 = null;
        Canvas canvas1 = null;
        Transform textPanel1 = null;
        Transform imagePanel1 = null;

        if (IDsAndGos[other.gameObject.GetInstanceID()].boundingCube.Count > 1)
        {
            visibility1 = IDsAndGos[other.gameObject.GetInstanceID()].boundingCube[1].GetComponent<Renderer>();
            canvas1 = IDsAndGos[other.gameObject.GetInstanceID()].infoTag[1].GetComponent<Canvas>();
            dist1 = CalculateDistance(rayCastPos.position, other.transform.position); //this distance does not include bounds since it cannot always be precomputed, for example, this is the case of moving objects
            textPanel1 = IDsAndGos[other.gameObject.GetInstanceID()].infoTag[1].transform.GetChild(0);
            imagePanel1 = IDsAndGos[other.gameObject.GetInstanceID()].infoTag[1].transform.GetChild(1);
        }

        IDsAndGos[other.gameObject.GetInstanceID()].infoTag[0].transform.rotation = transform.rotation;
        if (other.gameObject.layer.Equals(16) && !other.tag.Equals("Sign"))
        {
            Vector3 relativePoint = transform.InverseTransformPoint(other.transform.position); //this is to recognize if the object is to the left/right with respect to the car
            if (relativePoint.x < 0.0f)
                IDsAndGos[other.gameObject.GetInstanceID()].infoTag[0].transform.position = other.transform.position + bounds.center + IDsAndGos[other.gameObject.GetInstanceID()].infoTag[0].transform.TransformDirection(Vector3.right.x * trasl.x, Vector3.up.y * trasl.y, 0); //object is to the left  
            else if (relativePoint.x >= 0.0f)
                IDsAndGos[other.gameObject.GetInstanceID()].infoTag[0].transform.position = other.transform.position + bounds.center + IDsAndGos[other.gameObject.GetInstanceID()].infoTag[0].transform.TransformDirection(Vector3.right.x * -trasl.x, Vector3.up.y * trasl.y, 0); //object is to the right or directly ahead
        } else if ((other.gameObject.layer.Equals(16) && other.tag.Equals("Sign")) || other.gameObject.layer.Equals(17))
        {
            IDsAndGos[other.gameObject.GetInstanceID()].infoTag[0].transform.position = other.transform.position + IDsAndGos[other.gameObject.GetInstanceID()].infoTag[0].transform.TransformDirection(-Vector3.forward);
        } else if (other.gameObject.layer.Equals(8) || other.gameObject.layer.Equals(12))
        {
            Vector3 relativePoint = transform.InverseTransformPoint(other.transform.position); //this is to recognize if the object is to the left/right with respect to the car
            if (relativePoint.x < 0.0f)
                IDsAndGos[other.gameObject.GetInstanceID()].infoTag[0].transform.position = other.transform.position + IDsAndGos[other.gameObject.GetInstanceID()].infoTag[0].transform.TransformDirection(Vector3.right.x * bounds.size.x, Vector3.up.y * bounds.size.y * 0.5f, 0); //object is to the left  
            else if (relativePoint.x >= 0.0f)
                IDsAndGos[other.gameObject.GetInstanceID()].infoTag[0].transform.position = other.transform.position + IDsAndGos[other.gameObject.GetInstanceID()].infoTag[0].transform.TransformDirection(Vector3.right.x * -bounds.size.x, Vector3.up.y * bounds.size.y * 0.5f, 0); //object is to the right or directly ahead
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
            if (t.name.Equals("rayCastPos"))
            {
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

    Lane MonitorLane(Vector3 Point)
    {
        if (Vector3.Dot(gameObject.transform.TransformDirection(Vector3.forward), Point) < 0)
            return Lane.OPPOSITE;
        else
            return Lane.RIGHT;
    }

    void CreatePathEnhanced()
    {
        //pathEnhanced = new GameObject("PathEnhancedUrban");
        //pathEnhanced.AddComponent<AutoPathHelperUrban>();
        pathEnhanced = Instantiate(prefabs[12]);
    }

    void CreateCenterLine()
    {
        centerLine = Instantiate(prefabs[9]);
    }

    void SetSpeed()
    {
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

    Sprite ReturnSignPic(string signName, Sprite[] sprites)
    {
        if (signName.StartsWith("OneWayRight") || signName.StartsWith("One_Way_Right")) { return sprites[41]; }
        else if (signName.StartsWith("OneWayLeft") || signName.StartsWith("One_Way_Left"))  { return sprites[40]; }
        else if (signName.StartsWith("DoNotEnter") || signName.StartsWith("NotNotEnter_NoPole")) { return sprites[57]; }
        else if (signName.StartsWith("2_Hr_Parking")) { return sprites[43]; }
        else if (signName.StartsWith("15_Minute_Parking")) { return sprites[56]; }
        else if (signName.StartsWith("30_Minute_Parking")) { return sprites[42]; }
        else if (signName.StartsWith("Bicycle")) { return sprites[34]; }
        else if (signName.StartsWith("Disabled_Parking")) { return sprites[55]; }
        else if (signName.StartsWith("Do_Not_Block_Intersection")) { return sprites[35]; }
        else if (signName.StartsWith("Left_Lane_Must_Turn_Left")) { return sprites[39]; }
        else if (signName.StartsWith("Left_Turn_Signal_Yield_on_Green")) { return sprites[49]; }
        else if (signName.StartsWith("No_Left_Turn") || signName.StartsWith("NoTurnLeft_NoPole")) { return sprites[36]; }
        else if (signName.StartsWith("No_Right_Turn") || signName.StartsWith("NoTurnRight_NoPole")) { return sprites[37]; }
        else if (signName.StartsWith("No_Parking")) { return sprites[27]; }
        else if (signName.StartsWith("No_Parking_Bus_Stop")) { return sprites[47]; }
        else if (signName.StartsWith("No_Truck_Parking")) { return sprites[26]; }
        else if (signName.StartsWith("NoParkingAnyTime")) { return sprites[23]; }
        else if (signName.StartsWith("Parking")) { return sprites[45]; }
        else if (signName.StartsWith("Parking_2")) { return sprites[46]; }
        else if (signName.StartsWith("PickUp_DropOff")) { return sprites[24]; }
        else if (signName.StartsWith("Reserved_Disabled_Parking")) { return sprites[25]; }
        else if (signName.StartsWith("Reserved_Parking_")) { return sprites[28]; }
        else if (signName.StartsWith("Right_Lane_Must_Turn_Right")) { return sprites[38]; }
        else if (signName.StartsWith("Right_Only")) { return sprites[52]; }
        else if (signName.StartsWith("Road_Work_Ahead")) { return sprites[33]; }
        else if (signName.StartsWith("Speed_Limit_15")) { return sprites[31]; }
        else if (signName.StartsWith("Speed_Limit_25")) { return sprites[30]; }
        else if (signName.StartsWith("Speed_Limit_35")) { return sprites[29]; }
        else if (signName.StartsWith("This_Lane")) { return sprites[53]; }
        else if (signName.StartsWith("Tow__Away_Zone")) { return sprites[44]; }
        else if (signName.StartsWith("Turn")) { return sprites[32]; }
        else if (signName.StartsWith("Turning_Traffic_Must_Yield_To_Pedestrians")) { return sprites[50]; }
        else if (signName.StartsWith("Turning_Vehicles")) { return sprites[51]; }
        else if (signName.StartsWith("Yield_To_Peds")) { return sprites[48]; }
        else if (signName.StartsWith("StopSign")) { return sprites[58]; }
        else return null;
    }

    IEnumerator NavigationLine(float waitTime)
    {
        while (true)
        {
            yield return new WaitForSeconds(waitTime);
            linesUtilsAlt.DrawLine(gameObject);
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
