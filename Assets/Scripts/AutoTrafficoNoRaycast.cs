/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using System;
using System.Collections.Generic;
using UnityEngine;

public class AutoTrafficoNoRayCast : MonoBehaviour
{
    //ANTONELLO mi serve per notificare il cambiamento di id e subid della entry corrente
    public delegate void Delegato(int idPrecedene, int idCorrente);
    public event Delegato ChangeProperty;
    public event Delegato modificaSemaforo;
    int idCorrente;
    int semaforo;
    public string Property
    {
        set
        {
            ChangeProperty(idCorrente, int.Parse(value));
            idCorrente = int.Parse(value);
        }
    }

    public string Semaforo
    {
        set
        {
            if (int.Parse(value) != semaforo)
            {
                modificaSemaforo(semaforo, int.Parse(value));
                semaforo = int.Parse(value);
            }

        }
    }


    public TrafSystem system;
    public TrafEntry currentEntry;
    //ANTONELLO nextEntry era private
    public TrafEntry nextEntry = null;
    public bool hasNextEntry = false;
    public int currentIndex = 0;

    public float currentSpeed;
    public float currentTurn;
    public GameObject nose;
    public Transform raycastOrigin;
    //public float waypointThreshold = 0.3f;
    //ANTONELLO
    public float waypointThreshold = 0.6f;
    //ANTONELLO
    //public float maxSpeed = 10f;
    public float maxSpeed = 11f;
    public float maxTurn = 45f;
    //public float maxAccell = 3f;
    public float maxAccell = 2.5f; //ANTONELLO
    //public float maxBrake = 4f; ANTONELLO
    public float maxBrake = 5f; //VEDI SE VA BENE; DA TESTARE - ANTONELLO
    public float targetHeight = 0f;

    public bool hasStopTarget = false;
    public bool hasGiveWayTarget = false;
    public bool isInIntersection = false;
    public Vector3 stopTarget;
    public Vector3 targetTangent = Vector3.zero;
    //ANTONELLO - target era private
    public Vector3 target = Vector3.zero;
    public Vector3 nextTarget = Vector3.zero;

    //ANTONELLo
    private Vector3 targetPrecedente = Vector3.zero;

    public const float giveWayRegisterDistance = 12f;
    //ANTONELLO
    //public const float brakeDistance = 10f;
    public const float brakeDistance = 20f; //stabilisce dove arriva il raycast
    //public const float yellowLightGoDistance = 4f;
    //ANTONELLO
    public const float yellowLightGoDistance = 10f; //per evitare che inchiodi quando vede il semaforo arancione troppo tardi
    public const float stopLength = 10f;
    private VehicleController vehicleController;

    private bool inited = false;
    private float intersectionCornerSpeed = 1f;

    private float nextRaycast = 0f;
    private RaycastHit hitInfo;

    //ANTONELLo
    /*private RaycastHit hitSinsitra;
    private RaycastHit hitDestra;*/

    private bool somethingInFront = false;
    //ANTONELLO
    public bool sterzataMassima = false;


    private float stopEnd = 0f;

    //117 118 119 120 121 122  100 101 102 165 129 52 53 34 35             

    public bool fixedRoute = false;
    public List<RoadGraphEdge> fixedPath;
    public int currentFixedNode = 0;

    private RaycastHit heightHit;


    //ANTONELLO
    public bool frenata;
    private float distanzaIniziale;
    private float distanzaInizialeInchiodata;
    public Vector3 frenataTarget;
    public bool autoScorretta = false;
    private bool inchiodata;
    private bool evitare;
    private bool direzioneSpostamentoDestra;
    private float velocitaInizialeFrenata;



    //ANTONELLO
    private bool angoloSterzataAlto = false;
    public bool interventoAllaGuidaAccelerazione = false;
    public bool interventoAllaGuidaSterzata = false;

    PID PIDControllerSterzata;
    PID PIDControllerAccelerazione;
    PIDPars _pidPars;

    private GameObject raggioSinistra;
    private GameObject raggioDestra;

    public GameObject ostacoloEvitare;


    //ANTONELLO
    public void Init()
    {
        if (_pidPars == null)
            _pidPars = Resources.Load<PIDPars>("PIDPars_steeringWheel");
        target = currentEntry.waypoints[currentIndex];
        //CheckHeight();
        inited = true;
        nextRaycast = 0f;
        //CheckHeight();

        if (!this.tag.Equals("Player"))
        {
            InvokeRepeating("CheckHeight", 0.2f, 0.2f);
        }

        vehicleController = GetComponent<VehicleController>();
        PIDControllerSterzata = new PID(_pidPars.p_sterzata, _pidPars.i_sterzata, _pidPars.d_sterzata);
        PIDControllerAccelerazione = new PID(_pidPars.p_accelerazione, _pidPars.i_accelerazione, _pidPars.d_accelerazione);

        if (raggioDestra != null)
        {
            return;
        }

        ///Inizializzazione posizione partenza raggio raycast
        raggioDestra = new GameObject("raggioDestra");
        raggioDestra.transform.SetParent(this.transform);
        raggioDestra.transform.localPosition = new Vector3(raycastOrigin.localPosition.x + 0.92f, raycastOrigin.localPosition.y, raycastOrigin.localPosition.z);
        raggioDestra.transform.localRotation = Quaternion.identity;
        raggioDestra.transform.localScale = Vector3.zero;
        //raggioDestra.position = new Vector3(raggioDestra.position.x + 0.92f, raggioDestra.position.y, raggioDestra.position.z);
        raggioSinistra = new GameObject("raggioSinistra");
        raggioSinistra.transform.SetParent(this.transform);
        raggioSinistra.transform.localPosition = new Vector3(raycastOrigin.localPosition.x - 0.92f, raycastOrigin.localPosition.y, raycastOrigin.localPosition.z);
        raggioSinistra.transform.localRotation = Quaternion.identity;
        raggioSinistra.transform.localScale = Vector3.zero;


    }

    private void Start()
    {
        if (_pidPars == null)
            _pidPars = Resources.Load<PIDPars>("PIDPars_steeringWheel");
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
        
        //if(Physics.Raycast(raycastOrigin.position, raycastOrigin.forward, out blockedInfo, brakeDistance, ~(1 << LayerMask.NameToLayer("BridgeRoad"))))
        if (Physics.Raycast(raycastOrigin.position, raycastOrigin.forward, out blockedInfo, 35f, ~(1 << LayerMask.NameToLayer("BridgeRoad"))) ||
            Physics.Raycast(raggioDestra.transform.position, raggioDestra.transform.forward, out blockedInfo, 35f, ~(1 << LayerMask.NameToLayer("BridgeRoad"))) ||
            Physics.Raycast(raggioSinistra.transform.position, raggioSinistra.transform.forward, out blockedInfo, 35f, ~(1 << LayerMask.NameToLayer("BridgeRoad"))))
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
        if (this.gameObject.layer == 12)
        {
            Physics.Raycast(transform.position + Vector3.up * 2, -transform.up, out heightHit, 100f, ~(1 << LayerMask.NameToLayer("obstacle")));
        }
        else
        {
            Physics.Raycast(transform.position + Vector3.up * 2, -transform.up, out heightHit, 100f, ~(1 << LayerMask.NameToLayer("Traffic")));

        }
        targetHeight = heightHit.point.y;
        transform.position = new Vector3(transform.position.x, targetHeight, transform.position.z);
    }

    void FixedUpdate()
    {
        if (!inited)
            return;

        //ANTONELLO
        if (!this.tag.Equals("Player"))
        {
            SteerCar();
            MoveCar();
        }
        else
        {
            if (!interventoAllaGuidaAccelerazione)
            {
                MoveCarUtenteAccelerazione();
            }
            if (!interventoAllaGuidaSterzata)
            {
                SteerCar();
                MoveCarUtenteSterzata();
            }
        }


    }


    //ANTONELLO
    private float distanzaWaypoint = 0;
    private float contatore = 0;
    private float numeroWaypointSaltati = 0;

    private bool autoDavanti = false;
    private double distanzaSicurezza = 0;
    private float velocitàInizialeSicurezza = 0;
    private float distanzaInizialeSicurezza = 0;
    private bool autoDavantiFrenata = false;
    private float velocitaAttuale = 0;
    private float velocitaPrecedente = 0;
    public float accelerazione = 0;
    private bool inizioValutazioneSemaforo = false;

    void Update()
    {
        if (!inited)
            return;

        if (inchiodata)
        {
            inchioda();
            return;
        }

        if (evitare)
        {
            evita();
            return;
        }

        if (this.tag.Equals("Player"))
        {
            velocitaAttuale = GetComponent<Rigidbody>().velocity.magnitude;
        }
        else
        {
            velocitaAttuale = currentSpeed;
        }

        accelerazione = (velocitaAttuale - velocitaPrecedente) / Time.deltaTime;
        velocitaPrecedente = velocitaAttuale;


        if (!currentEntry.isIntersection() && currentIndex > 0 && !hasNextEntry)
        {
            if (Vector3.Distance(nose.transform.position, currentEntry.waypoints[currentEntry.waypoints.Count - 1]) <= 60f && this.tag.Equals("Player")) //piu è alto e piu inizia prima la valutazione dell'intersezione (es. semaforo)
            {
                if (semaforo != currentEntry.identifier)
                {
                    Semaforo = "" + currentEntry.identifier; //Setto la proprietà quando inizio a valutare il semaforo
                }
            }


            //last waypoint in this entry, grab the next path when we are in range
            //ANTONELLO
            //if(Vector3.Distance(nose.transform.position, currentEntry.waypoints[currentEntry.waypoints.Count - 1]) <= giveWayRegisterDistance)
            if (Vector3.Distance(nose.transform.position, currentEntry.waypoints[currentEntry.waypoints.Count - 1]) <= 40f) //era a 20
            {
                var node = system.roadGraph.GetNode(currentEntry.identifier, currentEntry.subIdentifier);

                RoadGraphEdge newNode;

                if (fixedRoute)
                {
                    if (++currentFixedNode >= fixedPath.Count)
                        currentFixedNode = 0;
                    newNode = system.FindJoiningIntersection(node, fixedPath[currentFixedNode]);
                }
                else
                {
                    newNode = node.SelectRandom();
                }

                if (newNode == null)
                {
                    Debug.Log("no edges on " + currentEntry.identifier + "_" + currentEntry.subIdentifier);
                    //ANTONELLO
                    if (!this.tag.Equals("Player"))
                    {
                        Destroy(gameObject);
                    }
                    else
                    {
                        Property = "9999";
                        inchioda();
                        return;
                    }

                    inited = false;
                    return;
                }

                nextEntry = system.GetEntry(newNode.id, newNode.subId);
                nextTarget = nextEntry.waypoints[0];
                hasNextEntry = true;


                nextEntry.RegisterInterest(gameObject.GetComponent<TrafAIMotor>());

                //see if we need to slow down for this intersection
                intersectionCornerSpeed = Mathf.Clamp(1 - Vector3.Angle(nextEntry.path.start.transform.forward, nextEntry.path.end.transform.forward) / 90f, 0.4f, 1f);


            }

        }
        if (hasNextEntry && !inizioValutazioneSemaforo && Vector3.Distance(nose.transform.position, nextTarget) <= 30f)
        {
            inizioValutazioneSemaforo = true;
        }

        float distanzaWaypointFramePrecedente = distanzaWaypoint;
        distanzaWaypoint = Vector3.Distance(nose.transform.position, target);

        //check if we have reached the target waypoint
        if (distanzaWaypoint <= waypointThreshold)// && !hasStopTarget && !hasGiveWayTarget)
        {
            distanzaWaypoint = 0;
            modificaTarget(false);
            numeroWaypointSaltati = 0;
        }
        else
        {
            if (distanzaWaypointFramePrecedente != 0 && (distanzaWaypoint - distanzaWaypointFramePrecedente) > 0)
            {
                contatore++;
                if (contatore >= 3)
                {
                    //abbiamo saltato il waypoint
                    if ((numeroWaypointSaltati++) >= 7)
                    {
                        Destroy(gameObject);
                    }
                    Debug.Log("waypoint saltato");
                    distanzaWaypoint = 0;
                    contatore = 0;
                    modificaTarget(true);
                    //hasStopTarget = false;
                    //hasGiveWayTarget = false;
                }

            }
            else
            {
                contatore = 0;
            }
        }

        Debug.DrawLine(this.transform.position, target);

        //SteerCar();

        if (hasNextEntry && inizioValutazioneSemaforo && nextEntry.isIntersection() && nextEntry.intersection.stopSign && !autoScorretta)
        {
            if (stopEnd == 0f)
            {
                hasStopTarget = true;
                velocitaInizialeFrenata = velocitaAttuale;
                stopTarget = nextTarget;
                calcolaDistanzaIniziale();
                stopEnd = Time.time + stopLength;
            }
            else if (Time.time > stopEnd)
            {
                if (nextEntry.intersection.stopQueue.Peek() == this)
                {
                    hasGiveWayTarget = false;
                    hasStopTarget = false;
                    stopEnd = 0f;
                    float distanza = Vector3.Distance(this.transform.position, stopTarget);
                    if (distanza < 10f)
                    {
                        //evita che quando ci fermiamo a uno stop (un pochino prima dello stopTarget), riparte e si riferma allo stopTarget effettivo
                        //nextEntry.intersection.stopSign = false;
                        currentEntry = nextEntry;
                        nextEntry = null;
                        hasNextEntry = false;
                        inizioValutazioneSemaforo = false;
                    }
                }
            }
        }


        if (hasNextEntry && !hasGiveWayTarget && inizioValutazioneSemaforo && nextEntry.isIntersection() && !nextEntry.intersection.stopSign)
        {
            //check next entry for stop needed
            if (nextEntry.MustGiveWay())
            {
                hasGiveWayTarget = true;
                velocitaInizialeFrenata = velocitaAttuale;
                stopTarget = nextTarget;
                calcolaDistanzaIniziale();
            }
            else
            {
                hasGiveWayTarget = false;
            }
        }
        else
        {
            if (hasGiveWayTarget && !nextEntry.MustGiveWay())
            {
                hasGiveWayTarget = false;
            }
        }


        if (!hasGiveWayTarget && hasNextEntry && inizioValutazioneSemaforo && nextEntry.light != null)
        {

            if (!hasStopTarget && nextEntry.light.State == TrafLightState.RED)
            {
                //light is red, stop here           
                //ANTONELLO
                if (!autoScorretta)
                {
                    hasStopTarget = true;
                    velocitaInizialeFrenata = velocitaAttuale;
                    stopTarget = nextTarget;

                    calcolaDistanzaIniziale();
                } //else autoScorretta = true, non rispetta i semafori rossi e passacomunque

            }
            else if (hasStopTarget && nextEntry.light.State == TrafLightState.GREEN)
            {

                //green light, go!   
                //ANTONELLO           
                hasStopTarget = false;

            }
            else if (!hasStopTarget && nextEntry.light.State == TrafLightState.YELLOW)
            {
                //yellow, stop if we aren't zooming on through
                //TODO: carry on if we are too fast/close

                if (Vector3.Distance(nextTarget, nose.transform.position) > yellowLightGoDistance)
                {
                    //ANTONELLO
                    hasStopTarget = true;
                    velocitaInizialeFrenata = velocitaAttuale;
                    stopTarget = nextTarget;

                    calcolaDistanzaIniziale();

                } //altrimenti non riesco a fermarmi, continuo
            }


        }

        float targetSpeed = maxSpeed;
        //float targetSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, 100f * Time.deltaTime);

        //lancio il raggio per vedere cosa ho davanti
        if (Time.time > nextRaycast)
        {
            hitInfo = new RaycastHit();
            somethingInFront = CheckFrontBlocked(out hitInfo);
            nextRaycast = NextRaycastTime();
        }

        if (somethingInFront)
        {
            if (hitInfo.collider.gameObject.tag.Equals("Player"))
            {
                currentSpeed = 0;
                return;
            }
        }


        if (frenata)
        {
            //ANTONELLO
            //se ho come target quello di fermarmi, diminuisco la velocità proporzionalmente fino al punto in cui devo fermarmi
            Vector3 vettoreDifferenza = frenataTarget - transform.position;
            float distanzaCorrente = 0;
            if (Math.Abs(vettoreDifferenza.x) > Math.Abs(vettoreDifferenza.z))
            {
                distanzaCorrente = Math.Abs(vettoreDifferenza.x);
            }
            else
            {
                distanzaCorrente = Math.Abs(vettoreDifferenza.z);
            }
            if (this.tag.Equals("Player"))
            {
                distanzaCorrente -= 5f;
            }
            else
            {
                distanzaCorrente -= 6f;
            }

            Debug.DrawLine(frenataTarget, transform.position);
            //if (velocitaInizialeFrenata >= 6f)
            //{
            targetSpeed = velocitaInizialeFrenata * distanzaCorrente / distanzaInizialeInchiodata;
            /*} else
            {
                targetSpeed = 6f * distanzaCorrente / distanzaInizialeInchiodata;
            }*/

            if (targetSpeed <= _pidPars.sogliaFermata)
            {
                targetSpeed = 0;
                //Debug.Log("target < 2");
            }
            //else
            //Debug.Log("distanza Iniziale Inchiodata = " + distanzaInizialeInchiodata + "; distanzaCorrente = " + distanzaCorrente + "; targetSpeed: " + targetSpeed);

        }

        if (!frenata && !autoDavantiFrenata && (hasStopTarget || hasGiveWayTarget))
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
            targetSpeed = velocitaInizialeFrenata * distanzaCorrente / distanzaIniziale;
            /*if (velocitaInizialeFrenata >= 6f)
            {
                targetSpeed = velocitaInizialeFrenata * distanzaCorrente / distanzaIniziale;
            }
            else
            {
                targetSpeed = 6f * distanzaCorrente / distanzaIniziale;
            }*/
            if (targetSpeed <= _pidPars.sogliaFermata)
            {
                targetSpeed = 0;
                //Debug.Log("target < 2");
            } //else 
            //Debug.Log("distanza Iniziale = " + distanzaIniziale + "; distanzaCorrente = " + distanzaCorrente + "; targetSpeed: " + targetSpeed);
        }

        //slow down if we need to turn
        //if(currentEntry.isIntersection() || hasNextEntry)
        //ANTONELLO
        //if ((currentEntry.isIntersection() || hasNextEntry) && !hasStopTarget && !frenata)
        if ((currentEntry.isIntersection() || inizioValutazioneSemaforo) && !hasStopTarget && !hasGiveWayTarget && !frenata)
        {
            float min = 4;
            if (targetSpeed <= 4)
            {
                min = targetSpeed;
            }
            targetSpeed = Mathf.Clamp(targetSpeed * intersectionCornerSpeed, min, maxSpeed);
            /*targetSpeed = Mathf.MoveTowards(targetSpeed, targetSpeed * intersectionCornerSpeed, Time.fixedDeltaTime * 5f);
            if (this.tag.Equals("Player"))
            {
                Debug.Log("IntersectionCornerSpeed: " + intersectionCornerSpeed + "; targetSpeed: " + targetSpeed);
            }*/

        }
        else
        {
            //DA TESTARE - ANTONELLO
            if (currentTurn > 5f || currentTurn < -5f)
            {
                //targetSpeed = targetSpeed * Mathf.Clamp(1 - (currentTurn / maxTurn), 0.1f, 1f);
                targetSpeed = targetSpeed * Mathf.Clamp(1 - (Math.Abs(currentTurn) / maxTurn), 0.2f, 1f);
            }

        }
        float targetSpeedIniziale = targetSpeed;
        if (targetSpeed > currentSpeed)
        {
            float distanzaCorrente = 0;
            if (hasStopTarget || hasGiveWayTarget)
            {
                Vector3 vettoreDifferenza = stopTarget - transform.position;
                if (Math.Abs(vettoreDifferenza.x) > Math.Abs(vettoreDifferenza.z))
                {
                    distanzaCorrente = Math.Abs(vettoreDifferenza.x);
                }
                else
                {
                    distanzaCorrente = Math.Abs(vettoreDifferenza.z);
                }
            }
            //Debug.Log("distanza corrente ripartenza: " + distanzaCorrente + "; targetSpee:" +targetSpeed);
            //if ((targetSpeed - currentSpeed) < 1 && (hasStopTarget || frenata) && distanzaCorrente < 3f)
            if ((hasStopTarget || hasGiveWayTarget) && distanzaCorrente < 6f)
            {
                //fa si che quando ci fermiamo allo stop o per via di una inchiodata la macchina rimanga ferma
                //currentSpeed = 0;
            }
            else
                //currentSpeed += Mathf.Min(maxAccell * Time.deltaTime, targetSpeed - currentSpeed);
                currentSpeed = targetSpeed;
            //currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, Time.fixedDeltaTime * _pidPars.velocitaFrenata);

        }
        else
        {
            //ANTONELLO
            if (hasStopTarget || hasGiveWayTarget || frenata || autoDavanti || autoDavantiFrenata)
            {
                if (targetSpeed > currentSpeed)
                {
                    //serve questa condizione perchè evita di accelerare quando sono in fase di fermata
                    targetSpeed = currentSpeed;
                }
                //currentSpeed = targetSpeed;
            }
            else
            {
                //targetSpeed -= Mathf.Min(maxBrake * Time.deltaTime, currentSpeed - targetSpeed);
                //Per fare in modo che arriva a targetSpeed dalla velocità corrente non troppo immediatamente
                targetSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, Time.fixedDeltaTime * _pidPars.velocitaFrenata);
            }

            if (targetSpeed < _pidPars.sogliaFermata)
                currentSpeed = 0;
            else
                currentSpeed = targetSpeed;
            //Debug.Log("CurrentSpeed: " + currentSpeed);


            //if (this.tag.Equals("Player"))
            //{
            //    Debug.Log("TargetSpeedIniziale: " + targetSpeedIniziale + "; currentSpeed impsotato: " + currentSpeed);
            //}


        }



    }

    //ANTONELLO
    private void modificaTarget(bool waypointSaltato)
    {
        if (++currentIndex >= currentEntry.waypoints.Count)
        {
            //Debug.Log("Numero waypoints: " + currentEntry.waypoints.Count + "; currentIndex: " + currentIndex);
            if (currentEntry.isIntersection())
            {
                currentEntry.DeregisterInterest();
                var node = system.roadGraph.GetNode(currentEntry.identifier, currentEntry.subIdentifier);
                var newNode = node.SelectRandom();

                //CONTROLLA QUESTO PEZZO DI CODICE; POTREBBE DISTRUGGERE LA MACCHINA
                if (newNode == null)
                {
                    //Debug.Log("no edges on " + currentEntry.identifier + "_" + currentEntry.subIdentifier);
                    Destroy(gameObject);
                    inited = false;
                    return;
                }

                currentEntry = system.GetEntry(newNode.id, newNode.subId);
                nextEntry = null;
                hasNextEntry = false;
                inizioValutazioneSemaforo = false;
                if (this.tag.Equals("Player"))
                {
                    this.Property = "" + newNode.id;
                }


                targetTangent = (currentEntry.waypoints[1] - currentEntry.waypoints[0]).normalized;

            }
            else
            {
                if ((hasStopTarget || hasGiveWayTarget) && !waypointSaltato) //ANTONELLO
                {
                    targetPrecedente = target;
                    target = nextEntry.waypoints[0];
                }
                else
                {
                    if (nextEntry != null && nextEntry.identifier != 0) //ANTONELLO
                    {
                        currentEntry = nextEntry;
                        nextEntry = null;
                        hasNextEntry = false;
                        inizioValutazioneSemaforo = false;
                        targetTangent = Vector3.zero;
                        if (this.tag.Equals("Player"))
                        {
                            this.Property = "" + currentEntry.identifier;
                        }
                    }

                    //ANTONELLO
                    if (waypointSaltato && (hasStopTarget || hasGiveWayTarget))
                    {
                        hasStopTarget = false;
                        hasGiveWayTarget = false;
                    }


                }
            }

            if ((!hasStopTarget && !hasGiveWayTarget) || waypointSaltato)
                currentIndex = 0;


        }
        if (currentIndex > 1)
        {
            targetTangent = Vector3.zero;
        }

        if ((!hasStopTarget && !hasGiveWayTarget) || waypointSaltato)
        {
            targetPrecedente = target;
            target = currentEntry.waypoints[currentIndex];
        }

    }

    public float centerSteerDifference = 2f;
    float averageSteeringAngle = 0f;

    void SteerCar()
    {
        PIDControllerSterzata.pFactor = _pidPars.p_sterzata;
        PIDControllerSterzata.iFactor = _pidPars.i_sterzata;
        PIDControllerSterzata.dFactor = _pidPars.d_sterzata;
        PIDControllerAccelerazione.pFactor = _pidPars.p_accelerazione;
        PIDControllerAccelerazione.iFactor = _pidPars.i_accelerazione;
        PIDControllerAccelerazione.dFactor = _pidPars.d_accelerazione;


        float targetDist = Vector3.Distance(target, transform.position);
        Vector3 newTarget = target;
        if (targetTangent != Vector3.zero && targetDist > 6f)
        {
            newTarget = target - (targetTangent * (targetDist - 6f));
        }
        Vector3 steerVector = new Vector3(newTarget.x, transform.position.y, newTarget.z) - transform.position;
        float steer = Vector3.Angle(transform.forward, steerVector);
        if (steer > 140f)
        {
            //steer = currentTurn;
            steer = 0;
        }
        steer = Mathf.Clamp((Vector3.Cross(transform.forward, steerVector).y < 0 ? -steer : steer), -maxTurn, maxTurn);

        float turnPrecedente;
        if (this.tag.Equals("Player"))
        {
            turnPrecedente = vehicleController.steerInput * 45f;
        }
        else
        {
            turnPrecedente = currentTurn;
        }

        float steeringAngle = steer;

        bool sterzataAmpia = false;
        //Debug.Log("indice: " + indice);

        if (steeringAngle > 25f)
        {
            if (sterzataMassima)
            {
                steeringAngle = 45f;
            }
            sterzataAmpia = true;
        }
        if (steeringAngle < -25f)
        {
            if (sterzataMassima)
            {
                steeringAngle = -45f;
            }
            //steeringAngle = -45f;
            sterzataAmpia = true;
        }

        if (!sterzataAmpia)
        {
            steeringAngle = PIDControllerSterzata.UpdatePars(steer, turnPrecedente, Time.fixedDeltaTime);
        }



        //Limit the steering angle
        steeringAngle = Mathf.Clamp(steeringAngle, -45, 45);

        bool trattoStradaDritto = false;

        if (Math.Abs(this.transform.position.x - target.x) <= 1 || Math.Abs(this.transform.position.z - target.z) <= 1)
        {
            trattoStradaDritto = true;
        }




        float indice = 0;
        float differenzaTurn = Math.Abs(steeringAngle - turnPrecedente);
        float indice1 = Mathf.Clamp(1 - (differenzaTurn / 20f), 0.1f, 1) * _pidPars.indiceSterzataCurva;
        if (sterzataAmpia)
        {
            indice = _pidPars.indiceSterzataMassima;
        }
        else
        {
            /*if (trattoStradaDritto) //|| differenzaTurn > 5f)
            {
                indice = _pidPars.indiceSterzataDritto;
            }
            else
            {
                indice = _pidPars.indiceSterzataCurva;
            }*/
            indice = _pidPars.indiceSterzataDritto;
        }
        //indice = indice1;

        //Debug.Log("indice settato: " + indice + "; indice secondo regola: " + indice1);

        averageSteeringAngle = averageSteeringAngle + ((steeringAngle - averageSteeringAngle) / indice);
        if (Mathf.Abs(averageSteeringAngle - turnPrecedente) <= _pidPars.puntoMortoSterzata)
        {
            averageSteeringAngle = turnPrecedente;
        }
        currentTurn = averageSteeringAngle;

        currentTurn = Mathf.Clamp(Mathf.Lerp(turnPrecedente, currentTurn, steerSpeed * Time.deltaTime), -45f, 45f);



    }

    public float maxThrottle = 0.8f;
    public float steerSpeed = 4.0f;
    public float throttleSpeed = 3.0f;
    public float brakeSpeed = 1f;
    private float m_targetSteer = 0.0f;
    private float m_targetThrottle = 0.0f;
    private float currentThrottle = 0f;

    private float frenataPrecedente = 0;
    private float sogliaNoGasTraffico;

    //METODO PER FAR MUOVERE LA MACCHINA USANDO IL VEHICLE CONTROLLER - ANTONELLO
    void MoveCarUtenteAccelerazione()
    {
        float throttlePrecedente = GetComponent<VehicleController>().accellInput;

        float speedDifference = currentSpeed - velocitaAttuale;

        /*if (currentSpeed <= velocitaAttuale && velocitaAttuale <= 0.001f)
        {
            //sono fermo e devo stare fermo
            vehicleController.accellInput = 0f;
            return;
        }*/
        //currentThrottle = PIDControllerAccelerazione.UpdatePars(currentSpeed, velocitaAttuale, Time.deltaTime);
        currentThrottle = PIDControllerAccelerazione.UpdatePars(currentSpeed, velocitaAttuale, Time.fixedDeltaTime);
        if (velocitaAttuale > currentSpeed)
        {
            if (hasStopTarget || frenata || hasGiveWayTarget || Math.Abs(speedDifference) > _pidPars.sogliaNoGas || (autoDavanti && Math.Abs(speedDifference) > sogliaNoGasTraffico))
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
            if (hasStopTarget || frenata)
            {
                currentThrottle = frenataPrecedente;
            }
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

    void MoveCarUtenteSterzata()
    {

        if (AppController.Instance.UserInput is SteeringWheelInputController)
        {
            //DirectInputWrapper.PlaySpringForce(0, Mathf.RoundToInt(currentTurn / 45f * 10000f), _pidPars.saturazione, _pidPars.coefficiente);            
        }
        vehicleController.steerInput = currentTurn / 45f;
    }


    private void MoveCar()
    {
        //GetComponent<Rigidbody>().MoveRotation(Quaternion.FromToRotation(Vector3.up, heightHit.normal) * Quaternion.Euler(0f, transform.eulerAngles.y + currentTurn * Time.fixedDeltaTime, 0f));
        //GetComponent<Rigidbody>().MovePosition(transform.position + transform.forward * currentSpeed * Time.deltaTime);

        transform.Translate(Vector3.forward * currentSpeed * Time.fixedDeltaTime);
        transform.Rotate(Vector3.up * currentTurn * Time.fixedDeltaTime);
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

    }

    private void evita()
    {
        //questo metodo fa si che l'auto eviti un ostacolo imminente e frenando e sterzando bruscamente
        float sterzata = 0;
        if (direzioneSpostamentoDestra == true)
        {
            sterzata = 45f;
        }
        else
        {
            sterzata = -45f;
        }


        currentThrottle = -1f;
        vehicleController.accellInput = currentThrottle;

        if (Vector3.Distance(transform.position, ostacoloEvitare.transform.position) < 8f)
        {
            vehicleController.steerInput = sterzata / 45;
        }

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
        //Debug.Log("distanzaIniziale = " + distanzaIniziale);
    }

    //ANTONELLO
    private void calcolaDistanzaInizialeInchiodata()
    {
        //Questo metodo serve a calcolare la distanza dal punto in cui abbiamo deciso di doverci fermare
        //e il punto in cui dobbiamo effettivamente fermarci
        Vector3 vettoreDifferenza = frenataTarget - transform.position;
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
        //Debug.Log("distanzaInizialeInchiodata = " + distanzaInizialeInchiodata);
    }


    //ANTONELLO
    /*private void OnGUI()
    {
        if (this.tag.Equals("Player"))
        {
            GUI.Label(new Rect(10, 240, 500, 200), messaggioDaStampare);
            GUI.Label(new Rect(10, 260, 500, 200), messaggioSemaforo);
        }
    }*/

    private GameObject ottieniRiferimentoPlayer()
    {
        GameObject go = GameObject.Find("XE_Rigged");
        if (go == null)
        {
            go = GameObject.Find("XE_Rigged(Clone)");
        }
        if (go == null)
        {
            go = GameObject.Find("TeslaModelS_2_Rigged)");
        }
        if (go == null)
        {
            go = GameObject.Find("TeslaModelS_2_Rigged(Clone)");
        }
        return go;
    }

    public ProssimoTarget ottieniTarget()
    {
        if (nextEntry != null)
        {
            Vector3 t = nextEntry.waypoints[nextEntry.waypoints.Count - 1];
            return new ProssimoTarget(t, true);
        }
        return new ProssimoTarget(target, false);
    }


   
}