using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using TMPro;

public class DashBoardControllerUrban : MonoBehaviour
{
    private GameObject turnLeft;
    private GameObject turnRight;
   
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
        rayCastPos = transform.parent.Find("rayCastPos");
        vehicleController = transform.parent.GetComponent<VehicleController>();
       
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
            if (trafAIMotor.hasNextEntry) //this used to exclude the first road which is the only exception which I cannot exclude by the angle
            {//I am waiting at the intersection
                float dstToTarget = Vector3.Distance(rayCastPos.position, trafAIMotor.nextEntry.waypoints[0]);
                if (dstToTarget <= 20f) 
                {
                    TrafEntry nextRoadWaypoints = trafAIMotor.system.GetEntry(trafAIMotor.fixedPath[trafAIMotor.currentFixedNode].id, trafAIMotor.fixedPath[trafAIMotor.currentFixedNode].subId); //points of the next piece of road
                    Vector3 heading = (nextRoadWaypoints.waypoints[nextRoadWaypoints.waypoints.Count - 1] - rayCastPos.position).normalized;
                    float angle = Vector3.SignedAngle(rayCastPos.forward, heading, Vector3.up); //angle between heading and direction of PlayerCar
                    
                    if (angle < -20f)
                    {
                        if (!hasPlayedON)
                        {
                            turnLeftAudioSource.PlayOneShot(ResourceHandler.instance.audioClips[7]);
                            hasPlayedON = true;
                        }
                        turnLeftAnim.SetBool("Turn", true);
                        lastTurnSignal = TurnSignal.LEFT;
                        hasPlayedOFF = false;
                    }
                    else if (angle > 20f)
                    {
                        if (!hasPlayedON)
                        {
                            turnRightAudioSource.PlayOneShot(ResourceHandler.instance.audioClips[7]);
                            hasPlayedON = true;
                        }
                        turnRightAnim.SetBool("Turn", true);
                        lastTurnSignal = TurnSignal.RIGHT;
                        hasPlayedOFF = false;
                    }
                }
            }
            else if ((!trafAIMotor.hasNextEntry && !trafAIMotor.currentEntry.isIntersection()) && Mathf.Abs(vehicleController.steerInput) <= 0.02f) //With the steerInput condition I assure that the turn signal is set off when steer has more or less angle equal to zero
            {
                if (lastTurnSignal.Equals(TurnSignal.LEFT))
                {
                    turnLeftAnim.SetBool("Turn", false);
                    hasPlayedON = false;
                    if (!hasPlayedOFF)
                    {
                        turnLeftAudioSource.PlayOneShot(ResourceHandler.instance.audioClips[6]);
                        hasPlayedOFF = true;
                    }
                }
                    
                else if (lastTurnSignal.Equals(TurnSignal.RIGHT))
                {
                    turnRightAnim.SetBool("Turn", false);
                    hasPlayedON = false;
                    if (!hasPlayedOFF)
                    {
                        turnRightAudioSource.PlayOneShot(ResourceHandler.instance.audioClips[6]);
                        hasPlayedOFF = true;
                    }
                }
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


