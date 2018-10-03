using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using TMPro;

public class DashBoardControllerUrban : MonoBehaviour
{
    private EnvironmentSensingAltUrbanTriggerSelective envSensingUrban;
   
    private GameObject turnLeft;
    private GameObject turnRight;
   
    private GameObject laneWarning;
    private GameObject laneWarningLeft;
    private bool hasPlayedON = false;
    private bool hasPlayedOFF = false;

    private AudioSource turnLeftAudioSource;
    private AudioSource turnRightAudioSource;

    

    private Transform rayCastPos;
    private VehicleController vehicleController;

    private Animator turnLeftAnim;
    private Animator turnRightAnim;
    private enum TurnSignal { LEFT, RIGHT, NONE };
    private TurnSignal lastTurnSignal = TurnSignal.NONE;

    void Start()
    {
        envSensingUrban = transform.parent.Find("colliderEnv").gameObject.GetComponent<EnvironmentSensingAltUrbanTriggerSelective>();
        
        rayCastPos = envSensingUrban.RayCastPos;
       
        vehicleController = envSensingUrban.Vc;
        
        turnLeftAnim = turnLeft.GetComponent<Animator>();
        turnRightAnim = turnRight.GetComponent<Animator>();

        turnLeftAudioSource = turnLeft.GetComponent<AudioSource>();
        turnRightAudioSource = turnRight.GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        SetSpeedPanel();

        
        turnLeft.SetActive(true);
        turnRight.SetActive(true);
        
    }

    void OnDisable()
    {
        
        turnLeft.SetActive(false);
        turnRight.SetActive(false);
        
    }

    void Update()
    {
        SetTurnSignal();
    }

    void SetTurnSignal()
    {
        TrafAIMotor trafAIMotor = transform.parent.gameObject.GetComponent<TrafAIMotor>();

        if (trafAIMotor != null)
        {
            AudioSource currentAudioSource = null;
            Animator currentAnim = null;

            if (trafAIMotor.hasNextEntry /*&& trafAIMotor.nextEntry.identifier != 1088*/) //this used to exclude the first road which is the only exception which I cannot exclude by the angle
            {//I am waiting at the intersection
                float dstToTarget = Vector3.Distance(rayCastPos.position, trafAIMotor.nextEntry.waypoints[0]);
                if (dstToTarget <= 20f) 
                {
                    TrafEntry nextRoadWaypoints = trafAIMotor.system.GetEntry(trafAIMotor.fixedPath[trafAIMotor.currentFixedNode].id, trafAIMotor.fixedPath[trafAIMotor.currentFixedNode].subId); //points of the next piece of road
                    Vector3 heading = (nextRoadWaypoints.waypoints[nextRoadWaypoints.waypoints.Count - 1] - rayCastPos.position).normalized;
                    float angle = Vector3.SignedAngle(rayCastPos.forward, heading, Vector3.up); //angle between heading and direction of PlayerCar
                    print(angle);

                    if (angle < -20f)
                    {
                        currentAnim = turnLeftAnim;
                        currentAudioSource = turnLeftAudioSource;
                        lastTurnSignal = TurnSignal.LEFT;
                    }
                    else if (angle > 20f)
                    {
                        currentAnim = turnRightAnim;
                        currentAudioSource = turnRightAudioSource;
                        lastTurnSignal = TurnSignal.RIGHT;
                    }

                    hasPlayedOFF = false;
                    if (!hasPlayedON)
                    {
                        currentAudioSource.PlayOneShot(ResourceHandler.instance.audioClips[7]);
                        hasPlayedON = true;
                    }

                    currentAnim.SetBool("Turn", true);
                }
            }
            else if ((!trafAIMotor.hasNextEntry && !trafAIMotor.currentEntry.isIntersection()) && Mathf.Abs(vehicleController.steerInput) <= 0.02f && hasPlayedON == true) //With the steerInput condition I assure that the turn signal is set off when steer has more or less angle equal to zero
            {
                if (lastTurnSignal.Equals(TurnSignal.LEFT))
                {
                    currentAnim = turnLeftAnim;
                    currentAudioSource = turnLeftAudioSource;
                }
                    
                else if (lastTurnSignal.Equals(TurnSignal.RIGHT))
                {
                    currentAnim = turnRightAnim;
                    currentAudioSource = turnRightAudioSource;
                }

                hasPlayedON = false;
                if (!hasPlayedOFF)
                {
                    currentAudioSource.PlayOneShot(ResourceHandler.instance.audioClips[6]);
                    hasPlayedOFF = true;
                }

                currentAnim.SetBool("Turn", false);
            }
        }
    }

    void SetSpeedPanel()
    {
        Transform speedPanel = transform.GetChild(0).GetChild(0);
        
        turnLeft = speedPanel.Find("TurnLeft").gameObject;
        turnRight = speedPanel.Find("TurnRight").gameObject;
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