using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using TMPro;

public class DashBoardController : MonoBehaviour
{
    private EnvironmentSensingAltTrigger envSensing;
    private PlayerCarLines playerCarLines;

    private GameObject warning;
    private GameObject turnLeft;
    private GameObject turnRight;
    private GameObject speedLimit;
    private GameObject laneWarning;
    private GameObject laneWarningLeft;

    private Image speedLimitImage;
    private Image warnImage;
    private Image laneImage;
    private Image laneLeftImage;

    private Transform rayCastPos;
    private Rigidbody rb;
    private VehicleController vehicleController;
    private LayerMask mask;

    private Animator turnLeftAnim;
    private Animator turnRightAnim;
    private enum TurnSignal { LEFT, RIGHT, NONE };
    private TurnSignal lastTurnSignal = TurnSignal.NONE;

    void Start()
    {
        envSensing = transform.parent.Find("colliderEnv").gameObject.GetComponent<EnvironmentSensingAltTrigger>();
        playerCarLines = transform.parent.GetComponent<PlayerCarLines>();
        rayCastPos = envSensing.RayCastPos;
        rb = envSensing.Rb;
        vehicleController = envSensing.Vc;
        mask = envSensing.Mask;

        turnLeftAnim = turnLeft.GetComponent<Animator>();
        turnRightAnim = turnRight.GetComponent<Animator>();
    }

    void OnEnable()
    {
        SetSpeedPanel();

        warning.SetActive(true);
        turnLeft.SetActive(true);
        turnRight.SetActive(true);
        speedLimit.SetActive(true);
        laneWarning.SetActive(true);
        laneWarningLeft.SetActive(true);
    }

    void OnDisable()
    {
        warning.SetActive(false);
        turnLeft.SetActive(false);
        turnRight.SetActive(false);
        speedLimit.SetActive(false);
        laneWarning.SetActive(false);
        laneWarningLeft.SetActive(false);
    }

    void Update()
    {
        SetTurnSignal();
        SetCollisionWarning();
        SetSpeedLimit();
        SetLaneWarning();
    }

    

    void SetSpeedLimit()
    {
        CarExternalInputAutoPathAdvanced autoPath = transform.parent.gameObject.GetComponent<CarExternalInputAutoPathAdvanced>();

        if (autoPath != null)
        {
            speedLimitImage.enabled = true;
            switch (Mathf.RoundToInt(autoPath.limiteVelocita))
            {
                case 45:
                    {
                        speedLimitImage.sprite = ResourceHandler.instance.sprites[32];
                    }
                    break;
                case 60:
                    {
                        speedLimitImage.sprite = ResourceHandler.instance.sprites[34];
                    }
                    break;

                case 75:
                    {
                        speedLimitImage.sprite = ResourceHandler.instance.sprites[35];
                    }
                    break;
            }
        }
    }

    void SetTurnSignal()
    {
        TrafAIMotor trafAIMotor = transform.parent.gameObject.GetComponent<TrafAIMotor>();

        if (trafAIMotor != null)
        {
            if (trafAIMotor.hasNextEntry)
            {//I am waiting at the intersection 
                float dstToTarget = Vector3.Distance(rayCastPos.position, trafAIMotor.nextEntry.waypoints[0]);
                if (dstToTarget <= 10f && trafAIMotor.currentFixedNode != 1) //currentFixedNode condition is used to exclude the first road which is the only exception which I cannot exclude by the angle
                {
                    Vector3 nextRoadWayPoint = trafAIMotor.nextEntry.waypoints[trafAIMotor.nextEntry.waypoints.Count - 1]; //I save the last waypoint of the next road
                    Vector3 heading = (nextRoadWayPoint - rayCastPos.position).normalized; //direction from PlayerCar to the the last waypoint of the next road
                    float angle = Vector3.SignedAngle(rayCastPos.forward, heading, Vector3.up); //angle between heading and direction of PlayerCar
                    //foreach (var s in trafAIMotor.nextEntry.waypoints)
                    //{
                    //    GameObject game = new GameObject("Node");
                    //    game.transform.position = s;
                    //    SphereCollider sphereCol = game.AddComponent<SphereCollider>();
                    //}

                    if (angle < -4f)
                    {
                        turnLeftAnim.SetBool("Turn", true);
                        lastTurnSignal = TurnSignal.LEFT;
                    }
                    else if (angle > 4f)
                    {
                        turnRightAnim.SetBool("Turn", true);
                        lastTurnSignal = TurnSignal.RIGHT;
                    }
                }
            }
            else if ((!trafAIMotor.hasNextEntry && !trafAIMotor.currentEntry.isIntersection()) && Mathf.Abs(vehicleController.steerInput) <= 0.02f) //With the steerInput condition I assure that the turn signal is set off when steer has more or less angle equal to zero
            {
                if (lastTurnSignal.Equals(TurnSignal.LEFT))
                    turnLeftAnim.SetBool("Turn", false);
                else if (lastTurnSignal.Equals(TurnSignal.RIGHT))
                    turnRightAnim.SetBool("Turn", false);
            }
        }
    }

    void SetSpeedPanel()
    {
        Transform speedPanel = transform.GetChild(0).GetChild(0);
        warning = speedPanel.Find("Warning").gameObject;
        turnLeft = speedPanel.Find("TurnLeft").gameObject;
        turnRight = speedPanel.Find("TurnRight").gameObject;
        speedLimit = speedPanel.Find("SpeedLimit").gameObject;
        laneWarning = speedPanel.Find("LaneWarning").gameObject;
        laneWarningLeft = speedPanel.Find("LaneWarningLeft").gameObject;

        warnImage = warning.GetComponent<Image>();
        speedLimitImage = speedLimit.GetComponent<Image>();
        laneImage = laneWarning.GetComponent<Image>();
        laneLeftImage = laneWarningLeft.GetComponent<Image>();
    }

    void SetLaneWarning()
    {
        laneImage.enabled = true;
        laneLeftImage.enabled = true;
        switch (playerCarLines.laneState)
        {
            case  PlayerCarLines.LaneState.GREEN:
                {
                    laneLeftImage.color = Color.green;
                }
                break;
            case PlayerCarLines.LaneState.YELLOW:
                {
                    laneLeftImage.color = Color.yellow;
                }
                break;

            case PlayerCarLines.LaneState.RED:
                {
                    laneLeftImage.color = Color.red;
                }
                break;
        }
    }

    void SetCollisionWarning()
    {
        List<CubesAndTags> objectsList = envSensing.IDsAndGos.Values.Where(x => x.dangerState != CubesAndTags.DangerState.NONE).ToList();

        if (objectsList.Count != 0)
        {
            CubesAndTags nearest = null; //nearest object whose AudioSource is playing
            float nearDist = 9999;
            foreach (var item in objectsList)
            {
                float thisDist = (transform.position - item.other.transform.position).sqrMagnitude; //this is squaredMagnitude i.e. magnitude without square root
                if (thisDist < nearDist)
                {
                    nearDist = thisDist;
                    nearest = item;
                }
            }

            switch (nearest.dangerState)
            {
                case CubesAndTags.DangerState.YELLOW:
                    {
                        warnImage.sprite = ResourceHandler.instance.sprites[29]; //yellow warning 
                        warnImage.enabled = true;
                    }
                    break;
                case CubesAndTags.DangerState.RED:
                    {
                        warnImage.sprite = ResourceHandler.instance.sprites[30]; //red warning
                        warnImage.enabled = true;
                    }
                    break;
            }
        }
        else
            warnImage.enabled = false;
    }
}


//void DrawPath()
//{
//    TrafAIMotor trafAIMotor = transform.parent.gameObject.GetComponent<TrafAIMotor>();

//    if (trafAIMotor != null)
//    {
//        foreach (var s in trafAIMotor.fixedPath)
//        {
//            TrafEntry entry = trafAIMotor.system.GetEntry(s.id, s.subId); //points of the next piece of road
//            foreach (var p in entry.waypoints)
//            {
//                GameObject game = new GameObject("Node");
//                game.transform.position = p;
//                SphereCollider sphereCol = game.AddComponent<SphereCollider>();
//            }
//        }
//    }
//}