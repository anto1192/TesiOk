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

public class CarExternalInputAutoPathAdvanced : MonoBehaviour
{
    public Transform raycastOrigin;
    public CarAutoPath path;
    public int currentWaypointIndex;
    private RoadPathNode currentNode;
    private Vector3 currentWaypoint;
    private RoadPathNode nextNode;
    private int nextWaypointIndex;
    private Vector3 nextWaypoint;

    public float waypointThreshold = 3f;
    public float maxSpeed = 20f;
    public float brakeDistance = 15f;
    public float minBrakeDist = 1.5f;

    public float maxThrottle = 0.8f;
    public float maxBrake = 0.4f;
    public float steerSpeed = 4.0f;
    public float throttleSpeed = 3.0f;
    public float brakeSpeed = 1f;
    private float m_targetSteer = 0.0f;
    private float m_targetThrottle = 0.0f;

    private float targetSpeed = 0f;
    private float currentThrottle = 0f;

    private float maxSteer;

    public float steerTargetDist = 16f;

    public bool reverse = false;

    public bool backArc = false;

    public bool straightAtFinal = true;


    public float predictLength = 10f;
    public float normalAdd = 5f;
    public float pathRadius = 1f;

    private VehicleController vehicleController;

    //ANTONELLO
    PID PIDControllerSterzata;
    PID PIDControllerAccelerazione;
    PIDPars _pidPars;
    float averageSteeringAngle = 0f;
    public int waypoint = 0;
    private float nextRaycast = 0f;
    private RaycastHit hitInfo;
    private bool somethingInFront = false;
    private bool autoDavanti = false;
    private double distanzaSicurezza = 0;
    private float velocitàInizialeSicurezza = 0;
    private float distanzaInizialeSicurezza = 0;
    private GameObject raggioSinistra;
    private GameObject raggioDestra;
    public bool sbacchettamento = false;
    public float limiteVelocita;

    public delegate void Delegato(int idPrecedene, int idCorrente);
    public event Delegato ChangeProperty;
    int idCorrente;
    public string Property
    {
        set
        {
            int nuovoId = int.Parse(value);
            if (idCorrente != nuovoId)
            {
                ChangeProperty(idCorrente, int.Parse(value));
                idCorrente = int.Parse(value);
            }
            
        }
    }

    public void Init()
    {
        vehicleController = GetComponent<VehicleController>();

        maxSteer = vehicleController.maxSteeringAngle;
        vehicleController.maxSteeringAngle = 45f;

        currentWaypointIndex = waypoint;
        currentNode = path.pathNodes[currentWaypointIndex++];
        currentWaypoint = currentNode.position;

        UpdateNextWaypoint();


        targetSpeed = maxSpeed;
        currentThrottle = 0f;

        if (_pidPars == null)
            _pidPars = Resources.Load<PIDPars>("PIDPars_steeringWheel");

        PIDControllerSterzata = new PID(_pidPars.p_sterzataPCH, _pidPars.i_sterzataPCH, _pidPars.d_sterzataPCH);
        PIDControllerAccelerazione = new PID(_pidPars.p_accelerazione, _pidPars.i_accelerazione, _pidPars.d_accelerazione);

        if (raggioDestra != null)
        {
            return;
        }

        ///Inizializzazione posizione partenza raggio raycast
        raggioDestra = new GameObject("raggioDestra");
        raggioDestra.transform.SetParent(this.transform);
        raggioDestra.transform.localPosition = new Vector3(raycastOrigin.localPosition.x + 0.92f, raycastOrigin.localPosition.y, raycastOrigin.localPosition.z);
        raggioDestra.transform.localRotation  = Quaternion.identity;
        raggioDestra.transform.localScale = Vector3.zero;
        //raggioDestra.position = new Vector3(raggioDestra.position.x + 0.92f, raggioDestra.position.y, raggioDestra.position.z);
        raggioSinistra = new GameObject("raggioSinistra");
        raggioSinistra.transform.SetParent(this.transform);
        raggioSinistra.transform.localPosition = new Vector3(raycastOrigin.localPosition.x - 0.92f, raycastOrigin.localPosition.y, raycastOrigin.localPosition.z);
        raggioSinistra.transform.localRotation = Quaternion.identity;
        raggioSinistra.transform.localScale = Vector3.zero;
        
        //raggioSinistra.position = new Vector3(raggioSinistra.position.x - 0.92f, raggioSinistra.position.y, raggioSinistra.position.z);
    }

    private void UpdateNextWaypoint()
    {
        nextWaypointIndex = currentWaypointIndex + 1;
        if (nextWaypointIndex >= path.pathNodes.Count)
            nextWaypointIndex = 0;

        nextNode = path.pathNodes[nextWaypointIndex];
        nextWaypoint = nextNode.position;
        this.Property = "" + nextWaypointIndex;

    }

    private Vector3 GetPredictedPoint()
    {
        return transform.position + GetComponent<Rigidbody>().velocity.normalized * predictLength;
    }


    private Vector3 GetNormalPoint(Vector3 predicted, Vector3 A, Vector3 B)
    {
        Vector3 ap = predicted - A;
        Vector3 ab = (B - A).normalized;

        return A + (ab * Vector3.Dot(ap, ab)) + ab * normalAdd;

    }


    Vector3 seek(Vector3 target, Vector3 location)
    {
        Vector3 desired = (target - location).normalized;
        return desired * maxSpeed;
    }

    private bool evitare = false;
    public bool inchiodare = false;
    void FixedUpdate()
    {
        
        var predicted = GetPredictedPoint();

        var normal = GetNormalPoint(predicted, currentWaypoint, nextWaypoint);
        if (evitare && hitInfo.collider != null && hitInfo.collider.gameObject.layer == 12)
        {
            evita();
            return;
        }
        if (inchiodare && hitInfo.collider != null && hitInfo.collider.gameObject.layer == 12)
        {
            inchioda();
            return;
        }

        //check if we are heading past the current waypoint
        if (Vector3.Dot(normal - nextWaypoint, nextWaypoint - currentWaypoint) >= 0)
        {
            currentWaypoint = nextWaypoint;
            currentNode = nextNode;
            currentWaypointIndex = nextWaypointIndex;

            UpdateNextWaypoint();

            predicted = GetPredictedPoint();
            normal = GetNormalPoint(predicted, currentWaypoint, nextWaypoint);
        }





        //lancio il raggio per vedere cosa ho davanti
        if (Time.time > nextRaycast)
        {
            hitInfo = new RaycastHit();
            somethingInFront = CheckFrontBlocked(out hitInfo);
            nextRaycast = NextRaycastTime();
        }
        if (somethingInFront)
        {
            if (hitInfo.collider.gameObject.name.Equals("RocciaTest"))
            {
                vehicleController.accellInput = -0.18f;
                if (hitInfo.distance < 15f)
                {
                    vehicleController.steerInput = -8f / 45f;
                }
                return;
            }
            if (hitInfo.rigidbody != null && ((hitInfo.rigidbody.tag.Equals("TrafficCar")) || hitInfo.rigidbody.gameObject.layer.Equals(12)))
            {
                if (hitInfo.rigidbody.tag.Equals("TrafficCar"))
                {
                    Debug.DrawLine(this.transform.position, hitInfo.transform.position);
                    if (hitInfo.distance <= 35f)
                    {
                        TrafPCH macchinaDavanti = hitInfo.rigidbody.GetComponent<TrafPCH>();
                        float frontSpeed = macchinaDavanti.currentSpeed;

                        float velocitaTarget = Mathf.Min(targetSpeed, (frontSpeed - 0.25f));
                        float miaVelocita = this.GetComponent<Rigidbody>().velocity.magnitude;
                        double distanzaSicurezzaUpdate = (Math.Pow((this.GetComponent<Rigidbody>().velocity.magnitude * 3.6f / 10f), 2) + 5f); //+ questo valore perchè la distanza viene calcolata dal centro dell'auto del traffico
                        bool sottoDistanzaSicurezza = (distanzaSicurezzaUpdate - hitInfo.distance) > 1f;
                        bool piuVeloceDelTarget = (miaVelocita - velocitaTarget) >= 1f;
                        if ((piuVeloceDelTarget || sottoDistanzaSicurezza))
                        {
                            if (autoDavanti == false)
                            {
                                //distanzaSicurezza = (Math.Pow((this.GetComponent<Rigidbody>().velocity.magnitude * 3.6f / 10f), 2) + 3.5f); //+ questo valore perchè la distanza viene calcolata dal centro dell'auto del traffico
                                //distanzaSicurezza = (Math.Pow((frontSpeed * 3.6f / 10f), 2) + 3.5f); //+ questo valore perchè la distanza viene calcolata dal centro dell'auto del traffico
                                if (sottoDistanzaSicurezza)
                                {
                                    distanzaSicurezza = distanzaSicurezzaUpdate;
                                }
                                else
                                {
                                    distanzaSicurezza = (Math.Pow((frontSpeed * 3.6f / 10f), 2) + 5f);
                                }

                                autoDavanti = true;
                                distanzaInizialeSicurezza = hitInfo.distance;
                                velocitàInizialeSicurezza = this.GetComponent<Rigidbody>().velocity.magnitude;
                            }



                        }
                        else
                        {
                            autoDavanti = false;
                        }

                        if (Math.Abs(hitInfo.distance - distanzaSicurezza) >= 1f && autoDavanti && miaVelocita > 0.1f)
                        {
                            /* se Vf è la velocita finale, Vi quella iniziale, Df la distanza finale, Di la distanza iniziale, Dc la distanza corrente
                            allora Vx = (Vf + Vi) / ((Df + Di)/Dc)*/
                            targetSpeed = (velocitaTarget + velocitàInizialeSicurezza) / (((float)distanzaSicurezza + distanzaInizialeSicurezza) / hitInfo.distance);
                        }
                        else
                        {
                            autoDavanti = false;
                            targetSpeed = velocitaTarget;
                        }

                    }
                }
                if (hitInfo.rigidbody.gameObject.layer.Equals(12))
                {

                    float distanza = Vector3.Distance(transform.position, hitInfo.rigidbody.transform.position);
                    double spazioFrenata = (Math.Pow(GetComponent<Rigidbody>().velocity.magnitude, 2)) / (2 * 2.3f);
                    if (spazioFrenata > distanza)
                    {
                        evitare = true;
                    }
                    else
                    {
                        inchiodare = true;
                    }
                }
            }
            else
            {
                evitare = inchiodare = false;

                if (currentNode.isInintersection)
                    targetSpeed = 4f;
                else
                    targetSpeed = maxSpeed;
            }
        }
        else
        {
            evitare = inchiodare = false;

            if (currentNode.isInintersection)
                targetSpeed = 4f;
            else
                targetSpeed = maxSpeed;
        }


            Debug.DrawLine(transform.position, normal);

        //STEER CAR
        Vector3 steerVector = new Vector3(normal.x, transform.position.y, normal.z) - transform.position;

        /*float steer = Vector3.Angle(transform.forward, steerVector);
        float angolo = Vector3.SignedAngle(transform.forward, steerVector, Vector3.up);
        steer -= 3.5f;
        Vector3 heading = (new Vector3(normal.x, transform.position.y, normal.z) - transform.position).normalized;
        float cosine = Vector3.Dot(transform.TransformDirection(Vector3.forward), heading);
        float angolo1 = Mathf.Rad2Deg * Mathf.Acos(cosine);*/
        m_targetSteer = Vector3.SignedAngle(transform.forward, normal - transform.position, Vector3.up);
        /*Vector3 desired = seek(new Vector3(normal.x, transform.position.y, normal.z), transform.position);
        Vector3 steers = desired -  GetComponent<Rigidbody>().velocity;
        float steer2 = Vector3.Angle(transform.forward, steers);*/


        //m_targetSteer = (Vector3.Cross(transform.forward, steerVector).y < 0 ? -steer : steer) / vehicleController.maxSteeringAngle;
        // m_targetSteer = Mathf.Clamp((Vector3.Cross(transform.forward, steerVector).y < 0 ? -steer : steer), -45, 45);

        /*if (Vector3.Distance(predicted, normal) < pathRadius)
            m_targetSteer = 0f;*/

        PIDControllerSterzata.pFactor = _pidPars.p_sterzataPCH;
        PIDControllerSterzata.iFactor = _pidPars.i_sterzataPCH;
        if (sbacchettamento)
        {
            PIDControllerSterzata.dFactor = 0;
        } else {
            PIDControllerSterzata.dFactor = _pidPars.d_sterzataPCH;
        }
        




        /*PIDControllerAccelerazione.pFactor = _pidPars.p_accelerazione;
        PIDControllerAccelerazione.iFactor = _pidPars.i_accelerazione;
        PIDControllerAccelerazione.dFactor = _pidPars.d_accelerazione;*/



        float turnPrecedente = vehicleController.steerInput * 45f;


        float steeringAngle = PIDControllerSterzata.UpdatePars(m_targetSteer, turnPrecedente, Time.fixedDeltaTime);
        //float steeringAngle = m_targetSteer;
        //Limit the steering angle
        steeringAngle = Mathf.Clamp(steeringAngle, -45, 45);

        float differenzaTurn = Math.Abs(steeringAngle - turnPrecedente);


        averageSteeringAngle = averageSteeringAngle + ((steeringAngle - averageSteeringAngle) / _pidPars.indiceSterzataCurva);
        if (Mathf.Abs(averageSteeringAngle - turnPrecedente) <= _pidPars.puntoMortoSterzata)
        {
            averageSteeringAngle = turnPrecedente;
        }

        averageSteeringAngle = Mathf.Clamp(Mathf.Lerp(turnPrecedente, averageSteeringAngle, steerSpeed * Time.fixedDeltaTime), -45f, 45f);
        //steeringAngle = Mathf.Clamp(Mathf.Lerp(turnPrecedente, steeringAngle, steerSpeed * Time.fixedDeltaTime), -45f, 45f);
        vehicleController.steerInput = averageSteeringAngle / 45f;
        //vehicleController.steerInput = steeringAngle / 45f;
        //FINE STEERCAR


        //DA TESTARE - ANTONELLO
        if (averageSteeringAngle > 5f || averageSteeringAngle < -5f)
        {
            //targetSpeed = targetSpeed * Mathf.Clamp(1 - (currentTurn / maxTurn), 0.1f, 1f);
            targetSpeed = targetSpeed * Mathf.Clamp(1 - (Math.Abs(averageSteeringAngle) / 45), 0.2f, 1f);
        }

        /*float speedDifference = targetSpeed - GetComponent<Rigidbody>().velocity.magnitude;

        if (speedDifference < 0)
        {
            //m_targetThrottle = 0f;
            //  m_CarControl.motorInput = 0f;
            // m_targetBrake = (rigidbody.velocity.magnitude / targetSpeed) * maxBrake;
        }
        else
        {
            m_targetThrottle = maxThrottle * Mathf.Clamp(1 - Mathf.Abs(m_targetSteer), 0.2f, 1f);
            speedDifference *= m_targetThrottle;
        }*/





        /*currentThrottle = Mathf.Clamp(Mathf.MoveTowards(currentThrottle, speedDifference/2f, throttleSpeed * Time.deltaTime), -maxBrake, maxThrottle);

        // m_CarControl.steerInput = Mathf.MoveTowards(m_CarControl.steerInput, m_targetSteer, steerSpeed * Time.deltaTime);
        //vehicleController.steerInput = Mathf.Lerp(vehicleController.steerInput, m_targetSteer, steerSpeed * Time.deltaTime);
        vehicleController.accellInput = currentThrottle;*/
        //vehicleController.motorInput = Mathf.Clamp(currentThrottle, 0f, m_targetThrottle);
        //vehicleController.brakeInput = Mathf.Abs(Mathf.Clamp(currentThrottle, -maxBrake, 0f));

        MoveCarUtenteAccelerazione();



    }

    private void inchioda()
    {
        //questo metodo fa si che l'auto inchiodi, in modo da non colpire un ostacolo davanti a noi
        if (currentThrottle != -1f)
        {
            currentThrottle = -1f;
            //VehicleController vehicleController = GetComponent<VehicleController>();
            vehicleController.accellInput = currentThrottle;
        } //altrimenti sto gia inchiodando, non faccio niente
        hitInfo = new RaycastHit();
        somethingInFront = CheckFrontBlocked(out hitInfo);

    }

    private void evita()
    {
        //questo metodo fa si che l'auto eviti un ostacolo imminente e frenando e sterzando bruscamente
        float sterzata = 20f;
        currentThrottle = -1f;

        
        vehicleController.accellInput = currentThrottle;
        hitInfo = new RaycastHit();
        somethingInFront = CheckFrontBlocked(out hitInfo);
        if (hitInfo.distance < 10f)
        {
            vehicleController.steerInput = sterzata / 45;
        }

    }





    public void OnDisable()
    {
        vehicleController.steerInput = 0f;
        vehicleController.accellInput = 0f;
        vehicleController.maxSteeringAngle = maxSteer;

        GetComponent<VehicleInputController>().enabled = true;
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
            if (c.transform.root != transform.root)
            {
                blockedInfo = new RaycastHit();
                blockedInfo.distance = 0f;
                return true;
            }
        }

        if (Physics.Raycast(raycastOrigin.position, raycastOrigin.forward, out blockedInfo, 40f, ~(1 << LayerMask.NameToLayer("BridgeRoad"))) ||
            Physics.Raycast(raggioDestra.transform.position, raggioDestra.transform.forward, out blockedInfo, 40f, ~(1 << LayerMask.NameToLayer("BridgeRoad"))) ||
            Physics.Raycast(raggioSinistra.transform.position, raggioSinistra.transform.forward, out blockedInfo, 40f, ~(1 << LayerMask.NameToLayer("BridgeRoad"))))
        {
            return true;
        }
        else
        {
            return false;
        }
    }



    private float frenataPrecedente = 0;
    void MoveCarUtenteAccelerazione()
    {

        PIDControllerAccelerazione.pFactor = _pidPars.p_accelerazione;
        PIDControllerAccelerazione.iFactor = _pidPars.i_accelerazione;
        PIDControllerAccelerazione.dFactor = _pidPars.d_accelerazione;
        float throttlePrecedente = GetComponent<VehicleController>().accellInput;

        float speedDifference = targetSpeed - GetComponent<Rigidbody>().velocity.magnitude;
        float velocitaAttuale = GetComponent<Rigidbody>().velocity.magnitude;
        /*if (currentSpeed <= velocitaAttuale && velocitaAttuale <= 0.001f)
        {
            //sono fermo e devo stare fermo
            vehicleController.accellInput = 0f;
            return;
        }*/
        //currentThrottle = PIDControllerAccelerazione.UpdatePars(currentSpeed, velocitaAttuale, Time.deltaTime);
        currentThrottle = PIDControllerAccelerazione.UpdatePars(targetSpeed, velocitaAttuale, Time.fixedDeltaTime);
        if (velocitaAttuale > targetSpeed)
        {
            if (Math.Abs(speedDifference) > _pidPars.sogliaNoGas || (autoDavanti && Math.Abs(speedDifference) > _pidPars.sogliaNoGasTraffico))
            {
                //devo fermarmi o rallentare
                currentThrottle = Mathf.Clamp(currentThrottle, -1f, 0f);
                if (currentThrottle < 0)
                {
                    frenataPrecedente = currentThrottle;
                }
                else
                {
                    currentThrottle = frenataPrecedente;
                }
                /*currentThrottle = ((velocitaAttuale - currentSpeed) / (Time.deltaTime * -2.2f));
                currentThrottle = Mathf.Clamp(currentThrottle, -1f, 0f);
               *///Debug.Log("Sto frenando; throttle (target): " + currentThrottle + "; velocita attuale: " + velocitaAttuale + "; velocita Target: " + currentSpeed);
            }
            else
            {
                //devo solo rallentare ma non ho motivo di frenare
                currentThrottle = Mathf.Clamp(currentThrottle, 0f, maxThrottle);
            }

        }
        else
        {
            currentThrottle = Mathf.Clamp(currentThrottle, 0f, maxThrottle);
        }




        float differenzaThrottle = Math.Abs(currentThrottle - throttlePrecedente);
        if (differenzaThrottle < _pidPars.puntoMortoFrenata)
        {
            currentThrottle = throttlePrecedente;
        }
        else
        {
            if (differenzaThrottle >= _pidPars.differenzaAccelerazione)
            {
                if (throttlePrecedente < currentThrottle)
                {
                    currentThrottle = throttlePrecedente + (differenzaThrottle / _pidPars.indiceAccelerazione);
                }
                if (throttlePrecedente > currentThrottle)
                {
                    currentThrottle = throttlePrecedente - (differenzaThrottle / _pidPars.indiceAccelerazione);
                }
            }
        }




        vehicleController.accellInput = Mathf.MoveTowards(throttlePrecedente, currentThrottle, throttleSpeed * Time.fixedDeltaTime);
        //vehicleController.accellInput = currentThrottle; 
    }


}
