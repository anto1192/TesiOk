using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficCarTurnOnOffSignal : MonoBehaviour {

    public GameObject turnLeft;
    public GameObject turnRight;
    private Animator turnLeftAnim;
    private Animator turnRightAnim;
    private enum TurnSignal { LEFT, RIGHT, NONE };
    private TurnSignal lastTurnSignal = TurnSignal.NONE;
    public Transform nose;

    
    void Start ()
    {
        turnLeftAnim = turnLeft.GetComponent<Animator>();
        turnRightAnim = turnRight.GetComponent<Animator>();
    }
	
	
	void Update ()
    {
        SetTurnSignal();
    }

    void SetTurnSignal()
    {
        TrafAIMotor trafAIMotor = transform.root.gameObject.GetComponent<TrafAIMotor>();

        if (trafAIMotor != null)
        {
            if (trafAIMotor.hasNextEntry && trafAIMotor.nextEntry.identifier != 1088) //this used to exclude the first road which is the only exception which I cannot exclude by the angle
            {//I am waiting at the intersection
                float dstToTarget = Vector3.Distance(nose.position, trafAIMotor.nextEntry.waypoints[0]);
                if (dstToTarget <= 20f)
                {
                    Vector3 nextRoadWayPoint = trafAIMotor.nextEntry.waypoints[trafAIMotor.nextEntry.waypoints.Count - 1]; //I save the last waypoint of the next road
                    Vector3 heading = (nextRoadWayPoint - nose.position).normalized; //direction from PlayerCar to the the last waypoint of the next road
                    float angle = Vector3.SignedAngle(nose.forward, heading, Vector3.up); //angle between heading and direction of PlayerCar

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
            else if ((!trafAIMotor.hasNextEntry && !trafAIMotor.currentEntry.isIntersection()) && Mathf.Abs(trafAIMotor.currentTurn) <= 1f) //With the steerInput condition I assure that the turn signal is set off when steer has more or less angle equal to zero
            {
                if (lastTurnSignal.Equals(TurnSignal.LEFT))
                    turnLeftAnim.SetBool("Turn", false);
                else if (lastTurnSignal.Equals(TurnSignal.RIGHT))
                    turnRightAnim.SetBool("Turn", false);
            }
        }
    }
}
