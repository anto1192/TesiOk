/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class TrafAIMotor : MonoBehaviour
{

    public TrafSystem system;
    public TrafEntry currentEntry;
    private TrafEntry nextEntry = null;
    private bool hasNextEntry = false;
    public int currentIndex = 0;

    public float currentSpeed;
    public float currentTurn;
    public GameObject nose;
    public Transform raycastOrigin;
    public float waypointThreshold = 0.3f;
    //ANTONELLO
    //public float maxSpeed = 10f;
    public float maxSpeed = 11f;
    public float maxTurn = 45f;
    public float maxAccell = 3f;
    //public float maxBrake = 4f; ANTONELLO
    public float maxBrake = 5f; //VEDI SE VA BENE; DA TESTARE - ANTONELLO
    public float targetHeight = 0f;

    public bool hasStopTarget = false;
    public bool hasGiveWayTarget = false;
    public bool isInIntersection = false;
    public Vector3 stopTarget;
    public Vector3 targetTangent = Vector3.zero;
    private Vector3 target = Vector3.zero;
    private Vector3 nextTarget = Vector3.zero;

    public const float giveWayRegisterDistance = 12f;
    //ANTONELLO
    //public const float brakeDistance = 10f;
    public const float brakeDistance = 20f; //stabilisce dove arriva il raycast
    //public const float yellowLightGoDistance = 4f;
    //ANTONELLO
    public const float yellowLightGoDistance = 10f; //per evitare che inchiodi quando vede il semaforo arancione troppo tardi
    public const float stopLength = 6f;
    
    private bool inited = false;
    private float intersectionCornerSpeed = 1f;

    private float nextRaycast = 0f;
    private RaycastHit hitInfo;
    private bool somethingInFront = false;


    private float stopEnd = 0f;

    //117 118 119 120 121 122  100 101 102 165 129 52 53 34 35             

    public bool fixedRoute = false;
    public List<RoadGraphEdge> fixedPath;
    public int currentFixedNode = 0;

    private RaycastHit heightHit;


    //ANTONELLO
    private bool inchiodata;
    private float distanzaIniziale;
    private float distanzaInizialeInchiodata;
    public Vector3 inchiodataTarget;

    //ANTONELLO
    private string messaggioDaStampare = "";
    private string messaggioSemaforo = "";
    

    public void Init()
    {
        target = currentEntry.waypoints[currentIndex];
        CheckHeight();
        inited = true;
        nextRaycast = 0f;
        CheckHeight();

        if (!this.tag.Equals("Player"))
        {
            InvokeRepeating("CheckHeight", 0.2f, 0.2f);
        }
        
    }


    public float NextRaycastTime()
    {
        return Time.time + UnityEngine.Random.value / 4 + 0.1f;
    }

    //check for something in front of us, populate blockedInfo if something was found
    private bool CheckFrontBlocked(out RaycastHit blockedInfo)
    {
        Collider[] colls = Physics.OverlapSphere(raycastOrigin.position, 0.2f, 1 << LayerMask.NameToLayer("Traffic"));
        foreach (var c in colls)
        {
            if(c.transform.root != transform.root)
            {
                blockedInfo = new RaycastHit();
                blockedInfo.distance = 0f;
                return true;
            }
        }

        if(Physics.Raycast(raycastOrigin.position, raycastOrigin.forward, out blockedInfo, brakeDistance, ~(1 << LayerMask.NameToLayer("BridgeRoad"))))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void GetNextPath()
    {

    }

    //TODO: tend to target height over time
    void CheckHeight()
    {
        Physics.Raycast(transform.position + Vector3.up * 2, -transform.up, out heightHit, 100f, ~(1 << LayerMask.NameToLayer("Traffic")));
        targetHeight =  heightHit.point.y;
        
        transform.position = new Vector3(transform.position.x, targetHeight, transform.position.z);    
    }

    void FixedUpdate()
    {
        if(!inited)
            return;

        //ANTONELLO
        if (!this.tag.Equals("Player"))
        {
            MoveCar();
        }
    }

    

    void Update()
    {
        if(!inited)
            return;

        if(!currentEntry.isIntersection() && currentIndex > 0 && !hasNextEntry)
        {
            //last waypoint in this entry, grab the next path when we are in range
            //ANTONELLO
            //if(Vector3.Distance(nose.transform.position, currentEntry.waypoints[currentEntry.waypoints.Count - 1]) <= giveWayRegisterDistance)
            if (Vector3.Distance(nose.transform.position, currentEntry.waypoints[currentEntry.waypoints.Count - 1]) <= 20f) //piu è alto e piu inizia prima la valutazione dell'intersezione (es. semaforo)
            {
                var node = system.roadGraph.GetNode(currentEntry.identifier, currentEntry.subIdentifier);
 
                RoadGraphEdge newNode;

                if (fixedRoute)
                {
                    if(++currentFixedNode >= fixedPath.Count)
                        currentFixedNode = 0;
                    newNode = system.FindJoiningIntersection(node, fixedPath[currentFixedNode]);
                } else {
                    newNode = node.SelectRandom();
                }

                if(newNode == null)
                {
                    Debug.Log("no edges on " + currentEntry.identifier + "_" + currentEntry.subIdentifier);
                    Destroy(gameObject);
                    inited = false;
                    return;
                }

                nextEntry = system.GetEntry(newNode.id, newNode.subId);
                nextTarget = nextEntry.waypoints[0];
                hasNextEntry = true;

                nextEntry.RegisterInterest(this);

                //see if we need to slow down for this intersection
                intersectionCornerSpeed = Mathf.Clamp(1 - Vector3.Angle(nextEntry.path.start.transform.forward, nextEntry.path.end.transform.forward) / 90f, 0.4f, 1f);


            }
        }

        //check if we have reached the target waypoint
        if(Vector3.Distance(nose.transform.position, target) <= waypointThreshold )// && !hasStopTarget && !hasGiveWayTarget)
        {           
            if(++currentIndex >= currentEntry.waypoints.Count)
            {
                Debug.Log("Numero waypoints: " + currentEntry.waypoints.Count + "; currentIndex: " + currentIndex);
                if(currentEntry.isIntersection())
                {
                    currentEntry.DeregisterInterest();
                    var node = system.roadGraph.GetNode(currentEntry.identifier, currentEntry.subIdentifier);
                    var newNode = node.SelectRandom();

                    //CONTROLLA QUESTO PEZZO DI CODICE; POTREBBE DISTRUGGERE LA MACCHINA
                    if(newNode == null)
                    {
                        Debug.Log("no edges on " + currentEntry.identifier + "_" + currentEntry.subIdentifier);
                        Destroy(gameObject);
                        inited = false;
                        return;
                    }
                    
                    currentEntry = system.GetEntry(newNode.id, newNode.subId);
                    nextEntry = null;
                    hasNextEntry = false;

                    targetTangent = (currentEntry.waypoints[1] - currentEntry.waypoints[0]).normalized;
                    Vector3[] nuovaPosizione = currentEntry.GetPoints();
                    foreach (Vector3 pos in nuovaPosizione)
                    {
                        Debug.Log("nuova posizione: x = " + pos.x + "; y = " + pos.y + "; z = " + pos.z);
                    }
                }
                else
                {
                    if(hasStopTarget || hasGiveWayTarget)
                    {
                        target = nextEntry.waypoints[0];
                    }
                    else
                    {
                        currentEntry = nextEntry;
                        nextEntry = null;
                        hasNextEntry = false;
                        targetTangent = Vector3.zero;
                    }
                }

                if(!hasStopTarget && !hasGiveWayTarget)
                    currentIndex = 0;


            }
            if(currentIndex > 1)
            {
                targetTangent = Vector3.zero;
            }

            if(!hasStopTarget && !hasGiveWayTarget)
                target = currentEntry.waypoints[currentIndex];
        } /*else //si dovrebbe poter eliminare
        {
            maxSpeed = 11f;
            //Debug.Log("Impostata max speed a 11");
        }*/



        SteerCar();

        if(hasNextEntry && nextEntry.isIntersection() && nextEntry.intersection.stopSign) 
        {
            if(stopEnd == 0f) {
                hasStopTarget = true;
                stopTarget = nextTarget;
                calcolaDistanzaIniziale();
                stopEnd = Time.time + stopLength;
            } else if(Time.time > stopEnd) {
                if(nextEntry.intersection.stopQueue.Peek() == this) {
                    hasGiveWayTarget = false;
                    hasStopTarget = false;
                    stopEnd = 0f;
                } 
            }
        }


        if(hasNextEntry && nextEntry.isIntersection() && !nextEntry.intersection.stopSign)
        {
            //check next entry for stop needed
            if(nextEntry.MustGiveWay())
            {
                hasGiveWayTarget = true;
                stopTarget = target;
                calcolaDistanzaIniziale();
            }
            else
            {
                hasGiveWayTarget = false;
            }
        }

        if(!hasGiveWayTarget && hasNextEntry && nextEntry.light != null)
        {
            if(!hasStopTarget && nextEntry.light.State == TrafLightState.RED)
            {
                //light is red, stop here           
                //ANTONELLO
                messaggioSemaforo = "semaforo rosso, mi fermo";
                hasStopTarget = true;
                stopTarget = nextTarget;

                calcolaDistanzaIniziale();
                
            }
            else if (!hasStopTarget && nextEntry.light.State == TrafLightState.GREEN)
            {

                //green light, continue   
                //ANTONELLO
                messaggioSemaforo = "semaforo verde, non mi fermo e continuo";
                //hasStopTarget = false;
            }
            else if(hasStopTarget && nextEntry.light.State == TrafLightState.GREEN)
            {

                //green light, go!   
                //ANTONELLO
                messaggioSemaforo = "semaforo verde, parto";             
                hasStopTarget = false;
                //return;
            }
            else if(!hasStopTarget && nextEntry.light.State == TrafLightState.YELLOW)
            {
                //yellow, stop if we aren't zooming on through
                //TODO: carry on if we are too fast/close

                if(Vector3.Distance(nextTarget, nose.transform.position) > yellowLightGoDistance)
                {
                    //ANTONELLO
                    messaggioSemaforo = "semaforo arancione, mi fermo; " + Vector3.Distance(nextTarget, nose.transform.position);
                    hasStopTarget = true;
                    stopTarget = nextTarget;

                    calcolaDistanzaIniziale();

                } else
                {
                    //ANTONELLO
                    messaggioSemaforo = "semaforo arancione, non riesco a fermarmi";
                    //hasStopTarget = false;                  
                }
            }


        }

        float targetSpeed = maxSpeed;
        

        if (Time.time > nextRaycast)
        {
            hitInfo = new RaycastHit();
            somethingInFront = CheckFrontBlocked(out hitInfo);
            nextRaycast = NextRaycastTime();
        }

        if (somethingInFront && !inchiodata)
        {
            //ANTONELLO
            if (hitInfo.rigidbody != null && hitInfo.rigidbody.tag.Equals("TrafficCar") && this.tag.Equals("Player"))
            {
                TrafAIMotor macchinaTraffico = hitInfo.rigidbody.GetComponent<TrafAIMotor>();
                Debug.Log("macchina del traffico difronte a noi; currentSpeed: " + macchinaTraffico.currentSpeed + "; currentTurn: " + macchinaTraffico.currentTurn);
                Debug.DrawLine(this.transform.position, hitInfo.transform.position);
            }
            if (hitInfo.distance < 10f && hitInfo.rigidbody != null && (hitInfo.rigidbody.tag.Equals("TrafficCar") || hitInfo.rigidbody.tag.Equals("Player")))
            {
                if (this.GetComponent<Rigidbody>().velocity.magnitude <= 0)
                {
                    //sono fermo, non faccio nulla
                    if (this.tag.Equals("Player"))
                    {
                        Debug.Log("C'è qualcosa davanti ma sono fermo");
                    }
                } else
                {

                
                    inchiodata = true;
                    inchiodataTarget = hitInfo.transform.position;

                    calcolaDistanzaInizialeInchiodata();

                    if (this.tag.Equals("Player"))
                    {
                        Debug.Log("Distanza inchiodata: " + hitInfo.distance + "; mi fermo; velocità della macchina del traffico: " + hitInfo.rigidbody.velocity.magnitude);
                    }
                }
            } else {
                /*if (!hasNextEntry || !nextEntry.isIntersection())
                {
                    hasStopTarget = false;
                }
                //*/
                float frontSpeed = Mathf.Clamp((hitInfo.distance - 1f) / 3f, 0f, maxSpeed);
                if (frontSpeed < 0.2f)
                    frontSpeed = 0f;

                targetSpeed = Mathf.Min(targetSpeed, frontSpeed);
                //ANTONELLO
                if (this.tag.Equals("Player"))
                {
                    if (hitInfo.rigidbody != null)
                    {
                        Debug.Log("c'è qualcosa davanti a noi; targetSpeed = " + targetSpeed + "; frontSpeed = " + frontSpeed + "; rigidbody: " + hitInfo.rigidbody.name + " tag: " + hitInfo.rigidbody.tag + "; distanza = " + hitInfo.distance);
                    } else
                    {
                        if (hitInfo.collider != null)
                        {
                            Debug.Log("c'è qualcosa davanti a noi; targetSpeed = " + targetSpeed + "; frontSpeed = " + frontSpeed + "; collider: " + hitInfo.collider.name + "; distanza = " + hitInfo.distance);
                        } else
                        {
                            Debug.Log("c'è qualcosa davanti a noi; targetSpeed = " + targetSpeed + "; frontSpeed = " + frontSpeed + "; non ha ne un collider ne un rigidbody");
                        }
                    }
                }
            }
        } else
        {
            if ((!somethingInFront && inchiodata) || (somethingInFront && hitInfo.distance >= 10f))
            {
                //Se mi sono fermato per via di qualcosa di fronte che ora non c'è piu devo ripartire
                //hasStopTarget = false;
                inchiodata = false;
            }
            
        }

        if (inchiodata)
        {
            //ANTONELLO
            //se ho come target quello di fermarmi, diminuisco la velocità proporzionalmente fino al punto in cui devo fermarmi
            Vector3 vettoreDifferenza = inchiodataTarget - transform.position;
            float distanzaCorrente = 0;
            if (Math.Abs(vettoreDifferenza.x) > Math.Abs(vettoreDifferenza.z))
            {
                distanzaCorrente = Math.Abs(vettoreDifferenza.x);
            }
            else
            {
                distanzaCorrente = Math.Abs(vettoreDifferenza.z);
            }
            distanzaCorrente -= 5f;
            Debug.DrawLine(inchiodataTarget, transform.position);
            targetSpeed = 11f * distanzaCorrente / distanzaInizialeInchiodata;
            if (targetSpeed <= 2)
            {
                targetSpeed = 0;
                Debug.Log("target < 2");
            }
            else
                Debug.Log("distanza Iniziale Inchiodata = " + distanzaInizialeInchiodata + "; distanzaCorrente = " + distanzaCorrente + "; targetSpeed: " + targetSpeed);

        }

        if (!inchiodata && (hasStopTarget || hasGiveWayTarget))
        {
            /*Vector3 targetVec = (stopTarget - nose.transform.position);

            float stopSpeed = Mathf.Clamp(targetVec.magnitude * (Vector3.Dot(targetVec, nose.transform.forward) > 0 ? 1f : 0f) / 3f, 0f, maxSpeed);
            if(stopSpeed < 0.24f)
                stopSpeed = 0f;

            targetSpeed = Mathf.Min(targetSpeed, stopSpeed);
            //Debug.Log("hasStopTarget true; targetSpeed: " + targetSpeed);*/

            //ANTONELLO
            //se ho come target quello di fermarmi, diminuisco la velocità proporzionalmente fino al punto in cui devo fermarmi
            Vector3 vettoreDifferenza = stopTarget - transform.position;
            float distanzaCorrente = 0;
            if (Math.Abs(vettoreDifferenza.x) > Math.Abs(vettoreDifferenza.z))
            {
                distanzaCorrente = Math.Abs(vettoreDifferenza.x);
            }
            else
            {
                distanzaCorrente = Math.Abs(vettoreDifferenza.z);
            }
            distanzaCorrente -= 2f;
            Debug.DrawLine(stopTarget, transform.position);
            targetSpeed = 11f * distanzaCorrente / distanzaIniziale;
            if (targetSpeed <= 2)
            {
                targetSpeed = 0;
                Debug.Log("target < 2");
            } else 
            Debug.Log("distanza Iniziale = " + distanzaIniziale + "; distanzaCorrente = " + distanzaCorrente + "; targetSpeed: " + targetSpeed);
        }

        //slow down if we need to turn
        //if(currentEntry.isIntersection() || hasNextEntry)
        //ANTONELLO
        if ((currentEntry.isIntersection() || hasNextEntry) && !hasStopTarget && !inchiodata)
        {
            float velocitaIniziale = targetSpeed;
            targetSpeed = targetSpeed * intersectionCornerSpeed;
            messaggioDaStampare = "Sono in curva, rallento da " + velocitaIniziale + " a " + targetSpeed + "; intersectionCornerSpeed : " + intersectionCornerSpeed;
        }
        else
        {
            //DA TESTARE - ANTONELLO
            //if (currentTurn > 10f || currentTurn < 10f)
            //{
                //targetSpeed = targetSpeed * Mathf.Clamp(1 - (currentTurn / maxTurn), 0.1f, 1f);
                //messaggioDaStampare = "else di sono in curva";
            //}
            
        }

        if(targetSpeed > currentSpeed)
        {
            Vector3 vettoreDifferenza = stopTarget - transform.position;
            float distanzaCorrente = 0;
            if (Math.Abs(vettoreDifferenza.x) > Math.Abs(vettoreDifferenza.z))
            {
                distanzaCorrente = Math.Abs(vettoreDifferenza.x);
            }
            else
            {
                distanzaCorrente = Math.Abs(vettoreDifferenza.z);
            }
            Debug.Log("distanza corrente ripartenza: " + distanzaCorrente + "; targetSpee:" +targetSpeed);
            if ((targetSpeed - currentSpeed) < 1 && (hasStopTarget || inchiodata) && distanzaCorrente < 3f)
            {
                //fa si che quando ci fermiamo allo stop o per via di una inchiodata la macchina rimanga ferma
                currentSpeed = 0;
            } else
                currentSpeed += Mathf.Min(maxAccell * Time.deltaTime, targetSpeed - currentSpeed);
            
        }
        else 
        {
            
            currentSpeed -= Mathf.Min(maxBrake * Time.deltaTime, currentSpeed - targetSpeed);
            if(currentSpeed < 0)
                currentSpeed = 0;
            //Debug.Log("CurrentSpeed: " + currentSpeed);

            //ANTONELLO
            if (hasStopTarget || inchiodata)
            {
                currentSpeed = targetSpeed;
            }

        }


        //ANTONELLO
        if (this.tag.Equals("Player"))
        {
            MoveCarUtente();
        } else
        {
            //MoveCar();
        }
        
    }

    void SteerCar()
    {
        
        float targetDist = Vector3.Distance(target, transform.position);
        //head towards target
        Vector3 newTarget = target;
        if(targetTangent != Vector3.zero && targetDist > 6f)
        {
            newTarget = target - (targetTangent * (targetDist - 6f));
        } 
        Vector3 steerVector = new Vector3(newTarget.x, transform.position.y, newTarget.z) - transform.position;
        float steer = Vector3.Angle(transform.forward, steerVector);
        if(steer > 140f)
        {
            steer = currentTurn;
        }
        currentTurn = Mathf.Clamp((Vector3.Cross(transform.forward, steerVector).y < 0 ? -steer : steer), -maxTurn, maxTurn);

    }

    public float maxThrottle = 0.8f;
    public float steerSpeed = 4.0f;
    public float throttleSpeed = 3.0f;
    public float brakeSpeed = 1f;
    private float m_targetSteer = 0.0f;
    private float m_targetThrottle = 0.0f;
    private float currentThrottle = 0f;


    //METODO PER FAR MUOVERE LA MACCHINA USANDO IL VEHICLE CONTROLLER - ANTONELLO
    void MoveCarUtente()
    {
        float throttlePrecedente = currentThrottle;
        //transform.Rotate(0f, currentTurn * Time.deltaTime, 0f);
        //GetComponent<Rigidbody>().MoveRotation(Quaternion.FromToRotation(Vector3.up, heightHit.normal) * Quaternion.Euler(0f, transform.eulerAngles.y + currentTurn * Time.fixedDeltaTime, 0f));
        //GetComponent<Rigidbody>().MovePosition(transform.position + transform.forward * currentSpeed * Time.fixedDeltaTime);
        //transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);

        
        var predicted = GetPredictedPoint();
        VehicleController vehicleController = GetComponent<VehicleController>();

        Vector3 steerVector = new Vector3(heightHit.normal.x, transform.position.y, heightHit.normal.z) - transform.position;

        //Vector3 desired = seek(new Vector3(normal.x, transform.position.y, normal.z), transform.position);
        //Vector3 steers = desired -  rigidbody.velocity;
        //steers.limit(maxforce);
        //applyForce(steers);

        float steer = Vector3.Angle(transform.forward, steerVector);
        m_targetSteer = (Vector3.Cross(transform.forward, steerVector).y < 0 ? -steer : steer) / vehicleController.maxSteeringAngle;

        if (Vector3.Distance(predicted, heightHit.normal) < 1f)
            m_targetSteer = 0f;


        float speedDifference = currentSpeed - GetComponent<Rigidbody>().velocity.magnitude;// * 2;
        float velocitaAttuale = GetComponent<Rigidbody>().velocity.magnitude;
        //Debug.Log("Current speed (quella a cui dovremmo andare): " + currentSpeed + "; velocitaAttuale: " + velocitaAttuale + "; speedDifference: " + speedDifference);
        if (speedDifference < 0)
        {
            m_targetThrottle = 0f;
            //m_CarControl.motorInput = 0f;
            m_targetThrottle = -(currentSpeed - GetComponent<Rigidbody>().velocity.magnitude / currentSpeed) * maxBrake;
            speedDifference *= m_targetThrottle;
            currentThrottle = m_targetThrottle;
            if (currentThrottle < -1)
            {
                currentThrottle = -1;
            }
            if (currentThrottle > 0)
            {
                currentThrottle = 0;
            }
            if (currentSpeed == 0)
            {
                currentThrottle = -1;
            }
        }
        else
        {
            //m_targetThrottle = maxThrottle * Mathf.Clamp(1 - Mathf.Abs(m_targetSteer), 0.2f, 1f);
            m_targetThrottle = maxThrottle;
            speedDifference *= m_targetThrottle;
            currentThrottle = Mathf.Clamp(Mathf.MoveTowards(currentThrottle, speedDifference / 2f, throttleSpeed * Time.deltaTime), -maxBrake, maxThrottle);
            if (currentThrottle > 1)
            {
                currentThrottle = maxThrottle;
            }
            if (currentThrottle < -1)
            {
                currentThrottle = -1;
            }
        }

        /*if (hasStopTarget)
        {
            //Debug.Log("Sono in MoveCarUtente; hasStopTarget true, currentThrottle prima = " + currentThrottle);
            currentThrottle = Mathf.Clamp(currentThrottle, -1, 0);
            if (GetComponent<Rigidbody>().velocity.magnitude <= 0.2f)
            {
                currentThrottle = -1;
            }
            //Debug.Log("Dopo = " + currentThrottle + ", velocita: " + GetComponent<Rigidbody>().velocity.magnitude);
        } else
        {
            //currentThrottle = Mathf.Clamp(currentThrottle, 0, maxThrottle);
        }*/

        if (Math.Abs(currentThrottle - throttlePrecedente) >= 0.1)
        {
            
            Debug.Log("troppa differenza nel throttle; throttlePrecedente = " + throttlePrecedente + "; nuovo = " + currentThrottle);
            currentThrottle = (currentThrottle + throttlePrecedente) / 2;
            Debug.Log("Throttle risultante = " + currentThrottle);
        }

        //Debug.Log("CurrentThrottle: " + currentThrottle + "; currentTurn = " + currentTurn + "; currentTurn/45 = " + currentTurn/45f);
        // m_CarControl.steerInput = Mathf.MoveTowards(m_CarControl.steerInput, m_targetSteer, steerSpeed * Time.deltaTime);
        vehicleController.accellInput = currentThrottle;
        if (currentTurn > 25f)
        {
            Debug.Log("massima sterzata; currentTurn = " + currentTurn);
            vehicleController.steerInput = 1f;
            return;
        }
        if (currentTurn < -25f)
        {
            Debug.Log("massima sterzata; currentTurn = " + currentTurn);
            vehicleController.steerInput = -1f;
            return;
        }
        vehicleController.steerInput = currentTurn/45f;
        //vehicleController.steerInput = Mathf.Lerp(vehicleController.steerInput, currentTurn / 45f, steerSpeed * Time.deltaTime);

        //vehicleController.motorInput = Mathf.Clamp(currentThrottle, 0f, m_targetThrottle);
        //vehicleController.brakeInput = Mathf.Abs(Mathf.Clamp(currentThrottle, -maxBrake, 0f));
    }

    private void MoveCar()
    {
        GetComponent<Rigidbody>().MoveRotation(Quaternion.FromToRotation(Vector3.up, heightHit.normal) * Quaternion.Euler(0f, transform.eulerAngles.y + currentTurn * Time.fixedDeltaTime, 0f));
        GetComponent<Rigidbody>().MovePosition(transform.position + transform.forward * currentSpeed * Time.fixedDeltaTime);
    }

    private Vector3 GetPredictedPoint()
    {
        return transform.position + GetComponent<Rigidbody>().velocity.normalized * 10f;
    }

    //ANTONELLO
    private void calcolaDistanzaIniziale()
    {
        //Questo metodo serve a calcolare la distanza dal punto in cui abbiamo deciso di doverci fermare
        //e il punto in cui dobbiamo effettivamente fermarci
        Vector3 vettoreDifferenza = stopTarget - transform.position;
        if (Math.Abs(vettoreDifferenza.x) > Math.Abs(vettoreDifferenza.z))
        {
            distanzaIniziale = Math.Abs(vettoreDifferenza.x); // + 2f;
            //il +2 garantisce che si fermi un po prima
        }
        else
        {
            distanzaIniziale = Math.Abs(vettoreDifferenza.z); // + 2f;
            //stopTarget.z -= 1f;
        }
        Debug.Log("distanzaIniziale = " + distanzaIniziale);
    }

    //ANTONELLO
    private void calcolaDistanzaInizialeInchiodata()
    {
        //Questo metodo serve a calcolare la distanza dal punto in cui abbiamo deciso di doverci fermare
        //e il punto in cui dobbiamo effettivamente fermarci
        Vector3 vettoreDifferenza = inchiodataTarget - transform.position;
        if (Math.Abs(vettoreDifferenza.x) > Math.Abs(vettoreDifferenza.z))
        {
            distanzaInizialeInchiodata = Math.Abs(vettoreDifferenza.x); // + 5f;
            //il +2 garantisce che si fermi un po prima
        }
        else
        {
            distanzaInizialeInchiodata = Math.Abs(vettoreDifferenza.z); // + 5f;
            //stopTarget.z -= 1f;
        }
        Debug.Log("distanzaInizialeInchiodata = " + distanzaInizialeInchiodata);
    }


    //ANTONELLO
    private void OnGUI()
    {
        if (this.tag.Equals("Player"))
        {
            GUI.Label(new Rect(10, 240, 500, 200), messaggioDaStampare);
            GUI.Label(new Rect(10, 260, 500, 200), messaggioSemaforo);
        }
    }


    internal class GestoreCollisioni : MonoBehaviour
    {

        TrafAIMotor motor = null;

        public void setMotor(TrafAIMotor motor)
        {
            this.motor = motor;
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        //ANTONELLO
        /*void OnTriggerEnter(Collider other)
        {
            Debug.Log("Collisione: other = " + other.name);
            currentSpeed = -1f;
        }*/

        //ANTONELLO
        void OnTriggerEnter(Collider other)
        {
            bool ambiente = false;

            if (other.gameObject.layer.Equals(16) || other.gameObject.layer.Equals(17) || other.gameObject.layer.Equals(18)) //16 equivale a EnvironmentProp
                ambiente = true;
            Debug.Log("Collisione stay: other = " + other.name + "; layer EnvironmentProp? " + ambiente + "; layer = " + other.gameObject.layer);
            //if (other.name.Equals("Cube"))
            //{

            if (other.gameObject.tag.Equals("StopTrigger"))
            {
                Debug.Log("StopTrigger, lo ignoro");
                return;
            }

            if (!ambiente) { 
                //Debug.Log("Cubo");
                Debug.Log("Collisione");
                //currentSpeed = -1f;
                motor.inchiodata = true;
                motor.stopTarget = other.transform.position;
                motor.maxBrake = 10f;
            }

        }


        //ANTONELLO
        void OnTriggerExit(Collider other)
        {
            Debug.Log("sono in onTriggerExit; other = " + other.name);
            //if (other.name.Equals("Cube"))
            //{
                //Debug.Log("Cubo exit");
                Debug.Log("Collisione exit");
                //currentSpeed = -1f;
                motor.inchiodata = false;
                motor.maxBrake = 5f;
            //}
        }

        
    }

}