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
    public Dictionary<int, CubesAndTags> IDsAndGos = new Dictionary<int, CubesAndTags>(); //this is to store gameobjects ids and their relative cubes

    
    private Quaternion rot0; //this is to store the initial rotation of the needle
    private GameObject[] prefabs;
    
    private GameObject speedPanel; //this is used to semplify the search into the car hierarchy which is very deep
    private Vector3 obstaclePrevPos; //this is to store the initial position of the obstacle in order to compute its speed
    
    private LayerMask mask;
    
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

        public void DestroyCubesAndTags()
        {
            foreach (GameObject go in this.boundingCube)
                Destroy(go);
            foreach (GameObject go in this.infoTag)
                Destroy(go);
        }
    }

    //void OnEnable()
    //{
    //    ScenarioTestUrbano.OnSpawnHeaps += HandleOnSpawnHeaps;
    //}

    //void OnDisable()
    //{
    //    ScenarioTestUrbano.OnSpawnHeaps -= HandleOnSpawnHeaps;
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

        obstaclePrevPos = Vector3.zero; //init position of obstacle spawned
        
        mask = (1 << LayerMask.NameToLayer("Traffic")) | (1 << LayerMask.NameToLayer("obstacle")) | (1 << LayerMask.NameToLayer("EnvironmentProp")) | (1 << LayerMask.NameToLayer("Signage") | 1 << LayerMask.NameToLayer("Roads")); //restrict OnTriggerEnter only to the layers of interest

        //foreach (Transform t in driverCam.transform)
        //    if (t.name.Equals("Center"))
        //    {
        //        t.GetComponent<Camera>().allowHDR = true;
        //        MKGlow mkg = t.gameObject.AddComponent<MKGlow>();
        //        mkg.GlowType = MKGlowType.Selective;
        //        mkg.GlowLayer = LayerMask.GetMask("Graphics");
        //        break;
        //    }

        StartCoroutine(CleanObjects(0.25f));
    }

    void Update()
    {
        SetSpeed();
    }

    void OnDrawGizmos()
    {
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
                        
                            if (other.transform.tag.Equals("ParkedCar"))
                            {
                                BoxCollider boxCol = other as BoxCollider;
                                bounds.center = Quaternion.Euler(other.transform.localEulerAngles) * boxCol.center;
                                bounds.size = boxCol.size;
                                UpdateIDsAndGos(other, bounds);
                            }
                            else if (other.transform.tag.Equals("Sign"))
                            {
                                BoxCollider boxCol = other as BoxCollider;
                                bounds.center = boxCol.center;
                                bounds.size = boxCol.size * 1.25f;
                                UpdateIDsAndGos(other, bounds);
                            }
                            switch (other.transform.name)
                            {
                                case "streetlight":
                                    {
                                        CapsuleCollider capsCol = other as CapsuleCollider;
                                        bounds.center = Quaternion.Euler(other.transform.localEulerAngles) * capsCol.center;
                                        bounds.size = new Vector3(capsCol.radius * 4.0f, capsCol.radius * 4.0f, capsCol.height);
                                        UpdateIDsAndGos(other, bounds);
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
                                        UpdateIDsAndGos(other, bounds);
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
                                        UpdateIDsAndGos(other, bounds);
                                    }
                                    break;
                            }    
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
                        UpdateIDsAndGos(other, bounds);
                    }
                    break;
                case 12:
                    {  //obstacle
                        Bounds bounds = new Bounds();
                        GameObject boundingCube = null;
                        GameObject infoTag = null;
                        if (other.transform.root.name.StartsWith("Mercedes") /*|| other.transform.root.tag.Equals("TrafficCar")*/)
                        {
                            if (other.transform.name.Equals("Body1"))
                            { //this is in order to check if an object has more colliders than one; if so, one is sufficient so simply discard other otherwise the cube instantiated would be equal to the number of colliders
                                bounds = ComputeBounds(other.transform.root);
                                boundingCube = CreateBoundingCube(other.transform.root, bounds.center, bounds.size);
                                boundingCube.transform.SetParent(other.transform.root);
                                infoTag = CreateInfoTag(bounds.center);
                                boundingCube.GetComponent<Renderer>().enabled = false;
                                infoTag.GetComponent<Canvas>().enabled = false;
                                CubesAndTags cubesAndTags = new CubesAndTags(); //I need to recompute the bounds since obstacles are dynamic, so I pass an empty bounds here
                                cubesAndTags.AddElements(boundingCube, infoTag, bounds);
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
                            CubesAndTags cubesAndTags = new CubesAndTags(); //I need to recompute the bounds since obstacles are dynamic, so I pass an empty bounds here
                            cubesAndTags.AddElements(boundingCube, infoTag, bounds);
                            IDsAndGos.Add(other.gameObject.GetInstanceID(), cubesAndTags);
                        }      
                    }
                    break;

                case 8:
                    {
                        if (other.transform.name.Equals("Body1") && other.transform.root.tag.Equals("TrafficCar"))
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

                            other.transform.root.gameObject.AddComponent<TrafficCarNavigationLineUrban>();
                        }
                    }
                    break;
                case 18: //Roads
                    {
                        if (other.transform.name.StartsWith("TrafficLight") /*&& other.transform.parent.name.Equals("Inter85")*/)
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
        CleanObjects(other);
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
                                trasl = bounds.size; //infoTag trasl
                                trasl = new Vector3(Mathf.Abs(trasl.x) * 0.5f + 1.0f, Mathf.Abs(trasl.y) * 0.5f, 0);
                                UpdateInfoTag(other, "0" + " KPH", Mathf.RoundToInt(dist).ToString() + " M", "", sprites[64], bounds, dist, 0, trasl);
                            } 
                            
                            switch (other.transform.name)
                            {
                                case "streetlight":

                                case "Lamppost":

                                case "tree_dec01":
                                    {
                                        trasl = bounds.size; //infoTag trasl
                                        trasl = new Vector3(Mathf.Abs(trasl.x) * 0.5f + 2.0f, -Mathf.Abs(trasl.z) * 0.25f, 0);
                                        UpdateInfoTag(other, "0" + " KPH", Mathf.RoundToInt(dist).ToString() + " M", "", sprites[64], bounds, dist, 0, trasl);
                                    }
                                    break;
                                
                                case "Dumpster":

                                case "barrier_concrete":
                                    {
                                        trasl = bounds.size; //infoTag trasl
                                        trasl = new Vector3(Mathf.Abs(trasl.x) * 0.5f + 2.0f, Mathf.Abs(trasl.z) * 0.5f, 0);
                                        UpdateInfoTag(other, "0" + " KPH", Mathf.RoundToInt(dist).ToString() + " M", "", sprites[64], bounds, dist, 0, trasl);
                                    }
                                    break;

                                case "busstop":
                                    {
                                        trasl = bounds.size; //infoTag trasl
                                        trasl = new Vector3(Mathf.Abs(trasl.y) * 0.5f + 2.0f, 0, 0);
                                        UpdateInfoTag(other, "0" + " KPH", Mathf.RoundToInt(dist).ToString() + " M", "", sprites[64], bounds, dist, 0, trasl);
                                    }
                                    break;

                                case "barrier_metal":
                                    {
                                        trasl = bounds.size; //infoTag trasl
                                        trasl = new Vector3(Mathf.Abs(trasl.z) * 0.5f + 2.0f, Mathf.Abs(trasl.y) * 0.5f, 0);
                                        UpdateInfoTag(other, "0" + " KPH", Mathf.RoundToInt(dist).ToString() + " M", "", sprites[64], bounds, dist, 0, trasl);
                                    }
                                    break;

                                case "Table_For2":

                                case "Table_For4":
                                    {
                                        trasl = bounds.size; //infoTag trasl
                                        trasl = new Vector3(Mathf.Abs(trasl.x) * 0.5f + 2.0f, Mathf.Abs(trasl.y) * 0.5f, 0);
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
                            KeyValuePair <Sprite, string> kvp = ReturnSignPic(other.name, sprites);
                            if (Vector3.Dot(gameObject.transform.TransformDirection(Vector3.forward), other.transform.TransformDirection(Vector3.up)) < 0 && kvp.Key != null)
                            {
                                UpdateInfoTag(other, "", "", kvp.Value, kvp.Key, bounds, dist, 0, Vector3.zero);
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
                        Bounds bounds = IDsAndGos[other.gameObject.GetInstanceID()].bounds[0];
                        KeyValuePair<Sprite, string> kvp = ReturnSignPic(other.name, sprites);
                        if (Vector3.Dot(gameObject.transform.TransformDirection(Vector3.forward), other.transform.TransformDirection(Vector3.up)) < 0 && kvp.Key != null)
                        {
                            UpdateInfoTag(other, "", "", kvp.Value, kvp.Key, bounds, dist, 0, Vector3.zero);
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
                        UpdateInfoTag(other, Mathf.RoundToInt(CalculateObstacleSpeed(other.transform)).ToString() + " KPH", Mathf.RoundToInt(dist).ToString() + " M", "", sprites[64], bounds, dist, 0, Vector3.zero);
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
                        //if (other.transform.name.StartsWith("TrafficLight"))
                        //{
                            //Bounds bounds =  IDsAndGos[other.gameObject.GetInstanceID()].bounds[0]; //I don't need them! Maybe I'd reconsider to avoid to pass them in OnTriggerEnter()
                            //Bounds bounds1 = IDsAndGos[other.gameObject.GetInstanceID()].bounds[1];
                            Transform Panel = IDsAndGos[other.gameObject.GetInstanceID()].infoTag[0].transform.GetChild(0);
                            Transform Panel1 = IDsAndGos[other.gameObject.GetInstanceID()].infoTag[1].transform.GetChild(0);
                            

                            Panel.transform.GetChild(0).GetComponent<Text>().text = Panel.transform.GetChild(1).GetComponent<Text>().text = "";
                            Panel1.transform.GetChild(0).GetComponent<Text>().text = Panel1.transform.GetChild(1).GetComponent<Text>().text = "";
                            Panel.transform.GetChild(2).GetComponent<Text>().fontSize = Panel1.transform.GetChild(2).GetComponent<Text>().fontSize = 110;

                            TrafLightState currentState = other.transform.GetComponent<TrafficLightContainer>().State;
                            if (currentState.Equals(TrafLightState.GREEN))
                            {
                                Panel.transform.GetChild(3).GetComponent<Image>().sprite = Panel1.transform.GetChild(3).GetComponent<Image>().sprite = sprites[61];
                                Panel.transform.GetChild(2).GetComponent<Text>().text = Panel1.transform.GetChild(2).GetComponent<Text>().text = "GO";
                            }
                            else if (currentState.Equals(TrafLightState.YELLOW))
                            {
                                Panel.transform.GetChild(3).GetComponent<Image>().sprite = Panel1.transform.GetChild(3).GetComponent<Image>().sprite = sprites[62];
                                Panel.transform.GetChild(2).GetComponent<Text>().text = Panel1.transform.GetChild(2).GetComponent<Text>().text = "GO";
                            }
                            else
                            {
                                Panel.transform.GetChild(3).GetComponent<Image>().sprite = Panel1.transform.GetChild(3).GetComponent<Image>().sprite = sprites[63];
                                Panel.transform.GetChild(2).GetComponent<Text>().text = Panel1.transform.GetChild(2).GetComponent<Text>().text = "STOP";
                            }
                            IDsAndGos[other.gameObject.GetInstanceID()].infoTag[0].transform.rotation = Quaternion.Euler(0f, 90f, 0f) * IDsAndGos[other.gameObject.GetInstanceID()].boundingCube[0].transform.rotation;
                            IDsAndGos[other.gameObject.GetInstanceID()].infoTag[0].transform.position = IDsAndGos[other.gameObject.GetInstanceID()].boundingCube[0].transform.position + IDsAndGos[other.gameObject.GetInstanceID()].infoTag[0].transform.TransformDirection(-Vector3.forward);
                            IDsAndGos[other.gameObject.GetInstanceID()].infoTag[1].transform.rotation = Quaternion.Euler(0f, 90f, 0f) * IDsAndGos[other.gameObject.GetInstanceID()].boundingCube[1].transform.rotation;
                            IDsAndGos[other.gameObject.GetInstanceID()].infoTag[1].transform.position = IDsAndGos[other.gameObject.GetInstanceID()].boundingCube[1].transform.position + IDsAndGos[other.gameObject.GetInstanceID()].infoTag[1].transform.TransformDirection(-Vector3.forward);

                            IDsAndGos[other.gameObject.GetInstanceID()].boundingCube[0].GetComponent<Renderer>().enabled = IDsAndGos[other.gameObject.GetInstanceID()].boundingCube[1].GetComponent<Renderer>().enabled = true;
                            IDsAndGos[other.gameObject.GetInstanceID()].infoTag[0].GetComponent<Canvas>().enabled = IDsAndGos[other.gameObject.GetInstanceID()].infoTag[1].GetComponent<Canvas>().enabled = true;
                        //}
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

    void UpdateIDsAndGos(Collider other, Bounds bounds)
    {
        GameObject boundingCube = CreateBoundingCube(other.transform, other.transform.position + bounds.center, bounds.size);
        GameObject infoTag = CreateInfoTag(other.transform.position + bounds.center);
        boundingCube.GetComponent<Renderer>().enabled = false;
        infoTag.GetComponent<Canvas>().enabled = false;
        CubesAndTags cubesAndTags = new CubesAndTags();
        cubesAndTags.AddElements(boundingCube, infoTag, bounds);
        IDsAndGos.Add(other.gameObject.GetInstanceID(), cubesAndTags);
    }

    void UpdateInfoTag(Collider other, string text, string text1, string text2, Sprite sprite, Bounds bounds, float dist, float dist1, Vector3 trasl)
    {
        Renderer visibility = IDsAndGos[other.gameObject.GetInstanceID()].boundingCube[0].GetComponent<Renderer>();
        Canvas canvas = IDsAndGos[other.gameObject.GetInstanceID()].infoTag[0].GetComponent<Canvas>();
        Transform Panel = IDsAndGos[other.gameObject.GetInstanceID()].infoTag[0].transform.GetChild(0);

        Renderer visibility1 = null;
        Canvas canvas1 = null;
        Transform Panel1 = null;
        

        if (IDsAndGos[other.gameObject.GetInstanceID()].boundingCube.Count > 1)
        {
            visibility1 = IDsAndGos[other.gameObject.GetInstanceID()].boundingCube[1].GetComponent<Renderer>();
            canvas1 = IDsAndGos[other.gameObject.GetInstanceID()].infoTag[1].GetComponent<Canvas>();
            dist1 = CalculateDistance(rayCastPos.position, other.transform.position); //this distance does not include bounds since it cannot always be precomputed, for example, this is the case of moving objects
            Panel1 = IDsAndGos[other.gameObject.GetInstanceID()].infoTag[1].transform.GetChild(0);
            
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
            IDsAndGos[other.gameObject.GetInstanceID()].infoTag[0].transform.position = other.transform.position + bounds.center + IDsAndGos[other.gameObject.GetInstanceID()].infoTag[0].transform.TransformDirection(-Vector3.forward);
        } else if (other.gameObject.layer.Equals(8))
        {
            Vector3 relativePoint = transform.InverseTransformPoint(other.transform.position); //this is to recognize if the object is to the left/right with respect to the car
            if (relativePoint.x < 0.0f)
                IDsAndGos[other.gameObject.GetInstanceID()].infoTag[0].transform.position = other.transform.position + IDsAndGos[other.gameObject.GetInstanceID()].infoTag[0].transform.TransformDirection(Vector3.right.x * bounds.size.x, Vector3.up.y * bounds.size.y * 0.5f, 0); //object is to the left  
            else if (relativePoint.x >= 0.0f)
                IDsAndGos[other.gameObject.GetInstanceID()].infoTag[0].transform.position = other.transform.position + IDsAndGos[other.gameObject.GetInstanceID()].infoTag[0].transform.TransformDirection(Vector3.right.x * -bounds.size.x, Vector3.up.y * bounds.size.y * 0.5f, 0); //object is to the right or directly ahead
        }
        else if (other.gameObject.layer.Equals(12))
        {
            Vector3 relativePoint = transform.InverseTransformPoint(other.transform.position); //this is to recognize if the object is to the left/right with respect to the car
            if (relativePoint.x < 0.0f)
                IDsAndGos[other.gameObject.GetInstanceID()].infoTag[0].transform.position = other.transform.position + IDsAndGos[other.gameObject.GetInstanceID()].infoTag[0].transform.TransformDirection(Vector3.right.x * (bounds.size.z + 0.5f), Vector3.up.y * bounds.size.y * 0.5f, 0); //object is to the left  
            else if (relativePoint.x >= 0.0f)
                IDsAndGos[other.gameObject.GetInstanceID()].infoTag[0].transform.position = other.transform.position + IDsAndGos[other.gameObject.GetInstanceID()].infoTag[0].transform.TransformDirection(Vector3.right.x * -(bounds.size.z + 0.5f), Vector3.up.y * bounds.size.y * 0.5f, 0); //object is to the right or directly ahead
        }

        Panel.transform.GetChild(0).GetComponent<Text>().text = text;
        Panel.transform.GetChild(1).GetComponent<Text>().text = text1;
        Panel.transform.GetChild(2).GetComponent<Text>().text = text2;
        Panel.transform.GetChild(3).GetComponent<Image>().sprite = sprite;

        Vector3 windShieldPos = rayCastPos.position + rayCastPos.up * 0.5f;
        windShieldPos -= rayCastPos.forward * 1.8f;
        Vector3 offset = IDsAndGos[other.gameObject.GetInstanceID()].infoTag[0].transform.position - windShieldPos;
        visibility.enabled = true;
        if (offset.sqrMagnitude < 3 * 3)  //if infoTags are too close to the playerCar I disable them
        {
            canvas.enabled = false;
        }
        else
        {
            canvas.enabled = true;
        }
    }

    void LoadCarHierarchy()
    {
        Dictionary<string, RectTransform> speedPanelChildren = new Dictionary<string, RectTransform>();
        foreach (RectTransform rt in speedPanel.transform)
            if (rt.name.Equals("Speed") || rt.name.Equals("Needle"))
                speedPanelChildren.Add(rt.name, rt);

        speed = speedPanelChildren["Speed"];
        needle = speedPanelChildren["Needle"];

        foreach (Transform t in transform.parent)
            if (t.name.Equals("rayCastPos"))
            {
                rayCastPos = t;
                break;
            }

        rigidbody = transform.parent.gameObject.GetComponent<Rigidbody>();

        rot0 = speed.localRotation; //init rotation of speed needle
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

    KeyValuePair<Sprite, string> ReturnSignPic(string signName, Sprite[] sprites)
    {
        if (signName.StartsWith("OneWayRight") || signName.StartsWith("One_Way_Right")) { return new KeyValuePair<Sprite, string>(sprites[41], "ONE WAY LEFT"); }
        else if (signName.StartsWith("OneWayLeft") || signName.StartsWith("One_Way_Left"))  { return new KeyValuePair<Sprite, string>(sprites[40], "ONE WAY RIGHT"); }
        else if (signName.StartsWith("DoNotEnter") || signName.StartsWith("NotNotEnter_NoPole")) { return new KeyValuePair<Sprite, string>(sprites[57], "DO NOT ENTER"); }
        else if (signName.StartsWith("2_Hr_Parking")) { return new KeyValuePair<Sprite, string>(sprites[57], "PARKING") ; }
        else if (signName.StartsWith("15_Minute_Parking")) { return new KeyValuePair<Sprite, string>(sprites[56], "PARKING"); }
        else if (signName.StartsWith("30_Minute_Parking")) { return new KeyValuePair<Sprite, string>(sprites[42], "PARKING"); }
        else if (signName.StartsWith("Bicycle")) { return new KeyValuePair<Sprite, string>(sprites[34], "WATCH FOR CYCLISTS"); }
        else if (signName.StartsWith("Disabled_Parking")) { return new KeyValuePair<Sprite, string>(sprites[55], "DISABLED PARKING"); }
        else if (signName.StartsWith("Do_Not_Block_Intersection")) { return new KeyValuePair<Sprite, string>(sprites[35], "DO NOT BLOCK INTERSECTION"); }
        else if (signName.StartsWith("Left_Lane_Must_Turn_Left")) { return new KeyValuePair<Sprite, string>(sprites[39], "LEFT LANE MUST TURN LEFT"); }
        else if (signName.StartsWith("Left_Turn_Signal_Yield_on_Green")) { return new KeyValuePair<Sprite, string>(sprites[49], "ONE WAY"); }
        else if (signName.StartsWith("No_Left_Turn") || signName.StartsWith("NoTurnLeft_NoPole")) { return new KeyValuePair<Sprite, string>(sprites[36], "NO LEFT TURN"); }
        else if (signName.StartsWith("No_Right_Turn") || signName.StartsWith("NoTurnRight_NoPole")) { return new KeyValuePair<Sprite, string>(sprites[37], "NO RIGHT TURN"); }
        else if (signName.StartsWith("No_Parking")) { return new KeyValuePair<Sprite, string>(sprites[27], "NO PARKING"); }
        else if (signName.StartsWith("No_Parking_Bus_Stop")) { return new KeyValuePair<Sprite, string>(sprites[47], "NO PARKING"); }
        else if (signName.StartsWith("No_Truck_Parking")) { return new KeyValuePair<Sprite, string>(sprites[26], "NO PARKING"); }
        else if (signName.StartsWith("NoParkingAnyTime")) { return new KeyValuePair<Sprite, string>(sprites[23], "NO PARKING"); }
        else if (signName.StartsWith("Parking")) { return new KeyValuePair<Sprite, string>(sprites[45], "PARKING"); }
        else if (signName.StartsWith("Parking_2")) { return new KeyValuePair<Sprite, string>(sprites[46], "PARKING"); }
        else if (signName.StartsWith("PickUp_DropOff")) { return new KeyValuePair<Sprite, string>(sprites[24], "PICK-UP DROPOFF"); }
        else if (signName.StartsWith("Reserved_Disabled_Parking")) { return new KeyValuePair<Sprite, string>(sprites[25], "DISABLED PARKING"); }
        else if (signName.StartsWith("Reserved_Parking_")) { return new KeyValuePair<Sprite, string>(sprites[28], "RESERVED PARKING"); }
        else if (signName.StartsWith("Right_Lane_Must_Turn_Right")) { return new KeyValuePair<Sprite, string>(sprites[38], "RIGHT LANE MUST TURN RIGHT"); }
        else if (signName.StartsWith("Right_Only")) { return new KeyValuePair<Sprite, string>(sprites[52], "RIGHT ONLY"); }
        else if (signName.StartsWith("Road_Work_Ahead")) { return new KeyValuePair<Sprite, string>(sprites[33], "ROADWORK AHEAD"); }
        else if (signName.StartsWith("Speed_Limit_15")) { return new KeyValuePair<Sprite, string>(sprites[31], "SPEED LIMIT 15"); }
        else if (signName.StartsWith("Speed_Limit_25")) { return new KeyValuePair<Sprite, string>(sprites[30], "SPEED LIMIT 25"); }
        else if (signName.StartsWith("Speed_Limit_35")) { return new KeyValuePair<Sprite, string>(sprites[29], "SPEED LIMIT 35"); }
        else if (signName.StartsWith("This_Lane")) { return new KeyValuePair<Sprite, string>(sprites[53], "THIS LANE"); }
        else if (signName.StartsWith("Tow__Away_Zone")) { return new KeyValuePair<Sprite, string>(sprites[44], "TOW AWAY ZONE"); }
        else if (signName.StartsWith("Turn")) { return new KeyValuePair<Sprite, string>(sprites[32], "WATCH OUT CURVE"); }
        else if (signName.StartsWith("Turning_Traffic_Must_Yield_To_Pedestrians")) { return new KeyValuePair<Sprite, string>(sprites[50], "YIELD TO PEDS"); }
        else if (signName.StartsWith("Turning_Vehicles")) { return new KeyValuePair<Sprite, string>(sprites[51], "TURNING VEHICLES"); }
        else if (signName.StartsWith("Yield_To_Peds")) { return new KeyValuePair<Sprite, string>(sprites[48], "YIELD TO PEDS"); }
        else if (signName.StartsWith("StopSign")) { return new KeyValuePair<Sprite, string>(sprites[58], "STOP"); }
        else return new KeyValuePair<Sprite, string>(null, ""); 
    }

    void CleanObjects(Collider other)
    {
        if (IDsAndGos.ContainsKey(other.gameObject.GetInstanceID()))
        {
            if (other.gameObject.layer.Equals(LayerMask.NameToLayer("Traffic")))
            { 
                Destroy(other.transform.root.GetComponent<TrafficCarNavigationLineUrban>()); //I destroy the script that draws the navigation Line
                Destroy(other.transform.root.GetComponentInChildren<LineRenderer>().gameObject); //I destroy the Empty that contains the LineRenderer
            }

            IDsAndGos[other.gameObject.GetInstanceID()].DestroyCubesAndTags();
            IDsAndGos.Remove(other.gameObject.GetInstanceID());
            
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
                if (IDsAndGos[key].boundingCube[0] == null) //TrafficCar no more exists, so delete it, its LineRenderer and its infoTag
                {
                    Destroy(IDsAndGos[key].infoTag[0]); //I don't use DestroyCubesAndTags since the boundingCube is already destroyed
                    IDsAndGos.Remove(key);
                }
            }
        }
    }
}
