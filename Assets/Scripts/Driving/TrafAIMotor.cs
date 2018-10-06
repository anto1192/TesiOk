using System;
using System.Collections.Generic;
using UnityEngine;

public class TrafAIMotor : MonoBehaviour
{
    public bool macchinaTrafficoInchiodata = false;

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
            try
            {
                ChangeProperty(idCorrente, int.Parse(value));
                idCorrente = int.Parse(value);
            } catch (Exception e)
            {
                Debug.Log("Eccezione " + e + " in SetProperty");
            }
            
        }
    }

    public string Semaforo
    {
        set
        {
            if (int.Parse(value) != semaforo)
            {
                try
                {
                    modificaSemaforo(semaforo, int.Parse(value));
                    semaforo = int.Parse(value);
                }
                catch (Exception e)
                {
                    Debug.Log("Eccezione " + e + " in SetSemaforo");
                }

            }

        }
    }


    public TrafSystem system;
    public TrafEntry currentEntry;
    public TrafEntry nextEntry = null;
    public bool hasNextEntry = false;
    public int currentIndex = 0;

    public float currentSpeed;
    public float currentTurn;
    public GameObject nose;
    public Transform raycastOrigin;
    public float waypointThreshold = 0.6f;
    public float maxSpeed = 13.8f;
    public float maxTurn = 45f;

    public float maxAccell = 2.5f; 

    public float maxBrake = 5f;
    public float targetHeight = 0f;

    public bool hasStopTarget = false;
    public bool hasGiveWayTarget = false;
    public bool isInIntersection = false;
    public Vector3 stopTarget;
    public Vector3 targetTangent = Vector3.zero;
    public Vector3 target = Vector3.zero;
    public Vector3 nextTarget = Vector3.zero;

    private Vector3 targetPrecedente = Vector3.zero;
    public float limiteVelocita = 50f;

    public float giveWayRegisterDistance = 40f;
    public const float brakeDistance = 20f;

    public const float yellowLightGoDistance = 10f; 
    public const float stopLength = 10f;
    private VehicleController vehicleController;

    public bool inited = false;
    public float intersectionCornerSpeed = 1f;

    private float nextRaycast = 0f;
    private RaycastHit hitInfo;

    public bool somethingInFront = false;
    public GameObject oggettoRilevato;
    public bool noRaycast = false;
    public bool sterzataMassima = false;

    private float stopEnd = 0f;           

    public bool fixedRoute = false;
    public List<RoadGraphEdge> fixedPath;
    public int currentFixedNode = 0;

    private RaycastHit heightHit;

    public bool frenata;
    private float distanzaIniziale;
    private float distanzaInizialeInchiodata;
    public Vector3 frenataTarget;
    public bool autoScorretta = false;
    public bool evitare;
    private bool direzioneSpostamentoDestra;
    private float velocitaInizialeFrenata;

    public float distanzaInizioValutazioneSemaforo = 40f;
    public float distanzaValutazioneAutoTraffico = 35f;

    public bool sorpasso = false;

    private bool angoloSterzataAlto = false;
    public bool interventoAllaGuidaAccelerazione = false;
    public bool interventoAllaGuidaSterzata = false;

    PID PIDControllerSterzata;
    PID PIDControllerAccelerazione;
    PIDPars _pidPars;

    private GameObject raggioSinistra;
    private GameObject raggioDestra;

    public GameObject ostacoloEvitare;


    private float valoreOriginaleMaxSpeed = 0;

    //Per macchinaTrafficoInchiodata
    DateTime tempoInizio;
    private bool velocitaVera = false;
    public bool luceStop = false;
    public bool forzaLuceStop = false;
    private bool autoPassata = false;

    public bool playAudio = false;

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

        valoreOriginaleMaxSpeed = 13.8f;
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

        if (Physics.Raycast(raycastOrigin.position, raycastOrigin.forward, out blockedInfo, 50f, ~(1 << LayerMask.NameToLayer("BridgeRoad"))) ||
            Physics.Raycast(raggioDestra.transform.position, raggioDestra.transform.forward, out blockedInfo, 50f, ~(1 << LayerMask.NameToLayer("BridgeRoad"))) ||
            Physics.Raycast(raggioSinistra.transform.position, raggioSinistra.transform.forward, out blockedInfo, 50f, ~(1 << LayerMask.NameToLayer("BridgeRoad"))))
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

    private float targetHeightPrecedente = 0;
    //TODO: tend to target height over time
    void CheckHeight()
    {
        if (this.gameObject.layer == 12)
        {
            Physics.Raycast(transform.position + Vector3.up * 2, -transform.up, out heightHit, 100f, ~(1 << LayerMask.NameToLayer("obstacle") | 1 << LayerMask.NameToLayer("Graphics"))); //DARIO
        }
        else
        {
            Physics.Raycast(transform.position + Vector3.up * 2, -transform.up, out heightHit, 100f, ~(1 << LayerMask.NameToLayer("Traffic") | 1 << LayerMask.NameToLayer("Graphics"))); //DARIO

        }
        targetHeight = heightHit.point.y;
        if (heightHit.distance < 1f)
        {
            targetHeight = Mathf.Lerp(targetHeightPrecedente, targetHeight, Time.deltaTime /** 10f*/);          
        }       
        transform.position = new Vector3(transform.position.x, targetHeight, transform.position.z);
        targetHeightPrecedente = targetHeight;
    }

    float targetSpeedIniziale = 0f;

    void FixedUpdate()
    {
        if (!inited)
            return;

        Guida();

        if (!this.tag.Equals("Player"))
        {
            controllaAccensioneLuceStop(targetSpeedIniziale);
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

    float targetSpeedPrecedente = 0;
    private bool targetSpeedRidotto = false;

    private void controllaAccensioneLuceStop(float targetSpeed)
    {
        bool stiamoRallentando = (targetSpeedPrecedente - targetSpeed) >= 0.2f;
        if (hasStopTarget || frenata || stiamoRallentando || (targetSpeed == targetSpeedPrecedente && targetSpeedRidotto && currentSpeed != targetSpeed) || forzaLuceStop)
        {
            if (stiamoRallentando)
            {
                targetSpeedRidotto = true;
            }
            luceStop = true;
        }
        else
        {
            targetSpeedRidotto = false;
            luceStop = false;
        }
        targetSpeedPrecedente = targetSpeed;
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
    public float velocitaAttuale = 0;
    private float velocitaPrecedente = 0;
    public float accelerazione = 0;
    private float accelerazionePrecedente = 0;
    private bool inizioValutazioneSemaforo = false;

    //PER DARIO
    public bool hasNextEntry50 = false;
    public TrafEntry nextEntry50 = null;

    void Guida()
    {
        if (!inited)
            return;

        if (this.tag.Equals("Player"))
        {
            velocitaAttuale = GetComponent<Rigidbody>().velocity.magnitude;
        }
        else
        {
            velocitaAttuale = currentSpeed;
        }

        accelerazione = Mathf.Lerp(accelerazionePrecedente, (velocitaAttuale - velocitaPrecedente) / Time.deltaTime, Time.deltaTime);
        velocitaPrecedente = velocitaAttuale;
        accelerazionePrecedente = accelerazione;

        if (evitare)
        {
            evita();
            return;
        }

        if (macchinaTrafficoInchiodata)
        {
            if (autoPassata)
            {
                TimeSpan differenza = (DateTime.Now - tempoInizio);
                if (differenza.Seconds >= 3)
                {
                    autoPassata = false;
                    maxSpeed = 5f;
                    velocitaVera = true;
                    forzaLuceStop = false;
                }
                else
                {
                    forzaLuceStop = true;
                    maxSpeed = 0;
                }
            }
        }
        


        if (!currentEntry.isIntersection() && currentIndex > 0 && !hasNextEntry)
        {
            if (Vector3.Distance(nose.transform.position, currentEntry.waypoints[currentEntry.waypoints.Count - 1]) <= 60f && this.tag.Equals("Player")) //piu è alto e piu inizia prima la valutazione dell'intersezione (es. semaforo)
            {
                if (semaforo != currentEntry.identifier)
                {
                    Semaforo = "" + currentEntry.identifier; //Setto la proprietà quando inizio a valutare il semaforo
                }
            }

            //PER DARIO
            if (Vector3.Distance(nose.transform.position, currentEntry.waypoints[currentEntry.waypoints.Count - 1]) <= (distanzaInizioValutazioneSemaforo + 10f) && !hasNextEntry50) //10 in piu dello spazio di freanata al semaforo
            {
                var node = system.roadGraph.GetNode(currentEntry.identifier, currentEntry.subIdentifier);

                RoadGraphEdge newNode;

                if (fixedRoute)
                {
                    if ((currentFixedNode+1) >= fixedPath.Count)
                        currentFixedNode = 0;
                    newNode = system.FindJoiningIntersection(node, fixedPath[currentFixedNode+1]);
                }
                else
                {
                    newNode = node.SelectRandom();
                }

                
                if (newNode != null)
                {
                    nextEntry50 = system.GetEntry(newNode.id, newNode.subId);
                    hasNextEntry50 = true;
                }
                        
            }

            if (Vector3.Distance(nose.transform.position, currentEntry.waypoints[currentEntry.waypoints.Count - 1]) <= giveWayRegisterDistance) //era a 20
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
                    if (!this.tag.Equals("Player"))
                    {
                        Destroy(gameObject);
                    }
                    else
                    {
                        Property = "9999";
                        return;
                    }

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
        if (hasNextEntry && !inizioValutazioneSemaforo && Vector3.Distance(nose.transform.position, nextTarget) <= distanzaInizioValutazioneSemaforo)
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
                if (contatore >= 15)
                {
                    //abbiamo saltato il waypoint
                    if ((numeroWaypointSaltati++) >= 15)
                    {
                        if (this.tag.Equals("Player"))
                        {
                            //l'utente è intervenuto alla guida allontandandosi troppo, fermiamo il test
                            Property = "9999"; //settando Property a 9999 verrà chiamato il metodo che interrompe il test
                        } else
                        {
                            Debug.Log(gameObject + "distrutto perchè ha saltato troppi waypoint");
                            Destroy(gameObject);
                        }
                        
                    }
                    Debug.Log("waypoint saltato; " + gameObject);
                    distanzaWaypoint = 0;
                    contatore = 0;
                    modificaTarget(true);
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
                hasStopTarget = false;
            }
            else if (!hasStopTarget && nextEntry.light.State == TrafLightState.YELLOW)
            {
                //yellow, stop if we aren't zooming on through
                //TODO: carry on if we are too fast/close
                //calcolo la riduzione di velocita necessaria per fermarci al semaforo
                float riduzioneVelocitaNecessaria = velocitaAttuale / Vector3.Distance(nextTarget, nose.transform.position);
                // if (Vector3.Distance(nextTarget, nose.transform.position) > yellowLightGoDistance)
                
               if (riduzioneVelocitaNecessaria < 0.7f)
                {
                    hasStopTarget = true;
                    velocitaInizialeFrenata = velocitaAttuale;
                    stopTarget = nextTarget;
                    calcolaDistanzaIniziale();
                } //altrimenti non riesco a fermarmi, continuo
            }
        }

        float targetSpeed = maxSpeed;

        //lancio il raggio per vedere cosa ho davanti
        if (Time.time > nextRaycast)
        {
            hitInfo = new RaycastHit();
            somethingInFront = CheckFrontBlocked(out hitInfo);
            nextRaycast = NextRaycastTime();
        }



        if (somethingInFront && !frenata && !noRaycast)
        {
            if (hitInfo.rigidbody != null && (hitInfo.rigidbody.tag.Equals("TrafficCar") || hitInfo.rigidbody.tag.Equals("TrafficScooter") || hitInfo.rigidbody.tag.Equals("Player")))
            {
                oggettoRilevato = hitInfo.rigidbody.gameObject;
                if (hitInfo.rigidbody.GetComponent<AutoTrafficoNoRayCast>() != null && macchinaTrafficoInchiodata)
                {
                    if (hitInfo.rigidbody.GetComponent<AutoTrafficoNoRayCast>().autoScorretta)
                    {
                        targetSpeed = 0;
                        autoPassata = true;
                        tempoInizio = DateTime.Now;
                    }
                }
                Debug.DrawLine(this.transform.position, hitInfo.transform.position);
                if (hitInfo.distance <= distanzaValutazioneAutoTraffico)
                {
                    if (((this.currentEntry.identifier >= 1000 || Vector3.Distance(this.gameObject.transform.position, currentEntry.waypoints[0]) < 10f) && (currentTurn > 5f || currentTurn < -5f))) //  || velocitaAttuale <= 0)
                    {
                        //se sono fermo, non faccio nulla ->COMMENTATO; VALUTA SE TOGLIERE IL COMMENTO

                        //se sono ad un incrocio mi fermo solo se l'ostacolo è imminente
                        //se non è imminente si tratta di un auto in un'altra corsia
                        //PS: se sono in un incrocio l'id è > 1000, se invece sono appena uscito dall'incrocio ho una distanza ravvicinata col primo waypoint della currentEntry
                        if (this.currentEntry.identifier >= 1000 || Vector3.Distance(this.gameObject.transform.position, currentEntry.waypoints[0]) < 10f)
                        {
                            if (hitInfo.distance < 4f)
                            {
                                frenata = true;
                                velocitaInizialeFrenata = velocitaAttuale;
                                frenataTarget = hitInfo.transform.position;
                                calcolaDistanzaInizialeInchiodata();
                            }
                        }
                    }
                    else
                    {
                        TrafAIMotor macchinaDavanti = hitInfo.rigidbody.GetComponent<TrafAIMotor>();

                        float frontSpeed = macchinaDavanti.currentSpeed;

                        if (frontSpeed > 0.25f)
                        {
                            float velocitaTarget = Mathf.Min(targetSpeed, (frontSpeed - 0.25f));
                            double distanzaSicurezzaUpdate = (Math.Pow((velocitaAttuale * 3.6f / 10f), 2) + 5f); //+ questo valore perchè la distanza viene calcolata dal centro dell'auto del traffico
                            bool sottoDistanzaSicurezza = (distanzaSicurezzaUpdate - hitInfo.distance) > 1f;
                            bool piuVeloceDelTarget = (velocitaAttuale - velocitaTarget) >= 1f;
                            if ((piuVeloceDelTarget || sottoDistanzaSicurezza))
                            {
                                //if ((macchinaDavanti.frenata || macchinaDavanti.hasStopTarget) && (velocitaAttuale - velocitaTarget) <= 2f)
                                if ((macchinaDavanti.frenata || macchinaDavanti.hasStopTarget) && sottoDistanzaSicurezza)
                                {
                                    autoDavantiFrenata = true;
                                    autoDavanti = false;
                                    sogliaNoGasTraffico = 0f;
                                }
                                else
                                {
                                    autoDavantiFrenata = false;
                                    sogliaNoGasTraffico = _pidPars.sogliaNoGasTraffico;
                                    if (autoDavanti == false)
                                    {
                                        //distanzaSicurezza = (Math.Pow((velocitaAttuale * 3.6f / 10f), 2) + 3.5f); //+ questo valore perchè la distanza viene calcolata dal centro dell'auto del traffico
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
                                        velocitàInizialeSicurezza = velocitaAttuale;
                                    }
                                }


                            }
                            else
                            {
                                autoDavanti = false;
                                autoDavantiFrenata = false;
                                sogliaNoGasTraffico = _pidPars.sogliaNoGasTraffico;
                            }

                            if (Math.Abs(hitInfo.distance - distanzaSicurezza) >= 1f && autoDavanti && velocitaAttuale > 0.1f)
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
                            //if (this.tag.Equals("Player"))
                            //{
                            //    Debug.Log("targetSpeed: " + targetSpeed + "; miavelocita " + miaVelocita + ";  autoDavanti: " + autoDavanti + "; distanza: " + hitInfo.distance + "; distanza di sicurezza: " + distanzaSicurezzaUpdate);

                            //}
                        }
                        else
                        {
                            autoDavanti = false;
                            autoDavantiFrenata = false;
                            sogliaNoGasTraffico = _pidPars.sogliaNoGasTraffico;
                            //double distanzaSicurezza1 = (Math.Pow((this.GetComponent<Rigidbody>().velocity.magnitude * 3.6f / 10f), 2) + 3.5f); //+ questo valore perchè la distanza viene calcolata dal centro dell'auto del traffico
                            //la macchina davanti è ferma o estremamente lenta (si può considerare ferma)
                            if (frontSpeed < velocitaAttuale)
                            {
                                frenata = true;
                                velocitaInizialeFrenata = velocitaAttuale;
                                if (!this.tag.Equals("Player") && velocitaInizialeFrenata < 3f)
                                {
                                    velocitaInizialeFrenata = 11f;
                                }
                                frenataTarget = hitInfo.transform.position;
                                calcolaDistanzaInizialeInchiodata();
                                //if (this.tag.Equals("Player"))
                                //{
                                //    Debug.Log("FRENATA true; distanza: " + hitInfo.distance + "; frontSpeed: " + frontSpeed  + "; miavelocita " + velocitaAttuale);

                                //}
                            }
                        }
                        //}
                    }
                }
            }
            else
            {
                oggettoRilevato = null;
            }
        }
        else
        {
            if (frenata)
            {
                autoDavanti = false;
                if (hitInfo.rigidbody == null)
                {
                    frenata = false;
                    autoDavantiFrenata = false;
                    sogliaNoGasTraffico = _pidPars.sogliaNoGasTraffico;

                }
                else
                {
                    TrafAIMotor macchinaDavanti = hitInfo.rigidbody.GetComponent<TrafAIMotor>();
                    float frontSpeed = macchinaDavanti.currentSpeed;
                    float miaVelocita = velocitaAttuale;

                    if ((!somethingInFront) || ((frontSpeed - miaVelocita) >= 0.5f && hitInfo.distance >= 1f) && (hitInfo.rigidbody.tag.Equals("TrafficCar") || hitInfo.rigidbody.tag.Equals("TrafficScooter") || hitInfo.rigidbody.tag.Equals("Player")))

                    {
                        //Se mi sono fermato per via di qualcosa di fronte che ora non c'è piu devo ripartire
                        //se invece c'è qualcosa davanti, frenata è true, ma la distanza è maggiore di 4f (piu è basso piu riparte prima quando si ferma dietro un'auto)
                        //e l'auto è ferma, significa che l'auto davanti a me sta ripartendo quindi posso ripartire anche io
                        frenata = false;
                    }
                }
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
                distanzaCorrente -= 6f;
            }
            else
            {
                distanzaCorrente -= 6f;
            }

            Debug.DrawLine(frenataTarget, transform.position);
            targetSpeed = velocitaInizialeFrenata * distanzaCorrente / distanzaInizialeInchiodata;

            if (targetSpeed <= _pidPars.sogliaFermata)
            {
                targetSpeed = 0;
            }           
        }

        if (!frenata && !autoDavantiFrenata && (hasStopTarget || hasGiveWayTarget))
        {
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
            if (!nextEntry.intersection.stopSign)
            {
                distanzaCorrente -= 5f;              
            }
            
            Debug.DrawLine(stopTarget, transform.position);
            targetSpeed = velocitaInizialeFrenata * distanzaCorrente / distanzaIniziale;


            if (targetSpeed <= _pidPars.sogliaFermata)
            {
                targetSpeed = 0;
            } 
        }

        //slow down if we need to turn      
        if ((currentEntry.isIntersection() || inizioValutazioneSemaforo) && !hasStopTarget && !hasGiveWayTarget && !frenata)
        {
            float min = 3;
            if (targetSpeed <= 3)
            {
                min = targetSpeed;
            }
            targetSpeed = Mathf.Clamp(targetSpeed * intersectionCornerSpeed, min, maxSpeed);
        }
        else
        {
            if ((currentTurn > 5f || currentTurn < -5f) && !sorpasso)
            {
                targetSpeed = targetSpeed * Mathf.Clamp(1 - (Math.Abs(currentTurn) / maxTurn), 0.2f, 1f);
            }

        }
        targetSpeedIniziale = targetSpeed;
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
            if ((hasStopTarget || hasGiveWayTarget) && distanzaCorrente < 6f)
            {
                //fa si che quando ci fermiamo allo stop o per via di una inchiodata la macchina rimanga ferma
                //currentSpeed = 0;
            }
            else
                //currentSpeed += Mathf.Min(maxAccell * Time.deltaTime, targetSpeed - currentSpeed);
                currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, Time.fixedDeltaTime * _pidPars.velocitaAccelerazione);

        }
        else
        {
            //ANTONELLO
            if (hasStopTarget || hasGiveWayTarget || frenata || autoDavanti || autoDavantiFrenata)
            {
                if (targetSpeed > velocitaAttuale)
                {
                    //serve questa condizione perchè evita di accelerare quando sono in fase di fermata
                    targetSpeed = velocitaAttuale;
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
        }

        

    }

    //ANTONELLO
    private void modificaTarget(bool waypointSaltato)
    {
        if (++currentIndex >= currentEntry.waypoints.Count)
        {
            if (currentEntry.isIntersection())
            {
                currentEntry.DeregisterInterest();
                var node = system.roadGraph.GetNode(currentEntry.identifier, currentEntry.subIdentifier);
                var newNode = node.SelectRandom();

                if (newNode == null)
                {
                   if (this.tag.Equals("Player"))
                    {
                        Property = "9999";
                    } else
                    {
                        Destroy(gameObject);
                    }                   
                    inited = false;
                    return;
                }

                currentEntry = system.GetEntry(newNode.id, newNode.subId);
                nextEntry = null;
                hasNextEntry = false;
                hasNextEntry50 = false;
                inizioValutazioneSemaforo = false;
                if (this.tag.Equals("Player"))
                {
                    this.Property = "" + newNode.id;
                }

                targetTangent = (currentEntry.waypoints[1] - currentEntry.waypoints[0]).normalized;
            }
            else
            {
                if ((hasStopTarget || hasGiveWayTarget) && !waypointSaltato)
                {
                    targetPrecedente = target;
                    target = nextEntry.waypoints[0];
                }
                else
                {
                    if (nextEntry != null && nextEntry.identifier != 0)
                    {
                        currentEntry = nextEntry;
                        nextEntry = null;
                        hasNextEntry = false;
                        hasNextEntry50 = false;
                        inizioValutazioneSemaforo = false;
                        targetTangent = Vector3.zero;
                        if (this.tag.Equals("Player"))
                        {
                            this.Property = "" + currentEntry.identifier;
                        }
                    }

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

    private bool ridottoMaxSpeed = false;

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

        if (ridottoMaxSpeed && this.tag.Equals("Player"))
        {
            maxSpeed = valoreOriginaleMaxSpeed;
            ridottoMaxSpeed = false;
        }

        if (steeringAngle > 25f)
        {
            if (sterzataMassima)
            {
                steeringAngle = 45f;
                if (this.tag.Equals("Player") && maxSpeed > 8f)
                {
                    ridottoMaxSpeed = true;
                    valoreOriginaleMaxSpeed = maxSpeed;
                    maxSpeed = 8f;
                }
            } else
            {
                if (this.tag.Equals("Player"))
                {
                    maxSpeed = valoreOriginaleMaxSpeed;
                }
            }
            sterzataAmpia = true;
        }
        if (steeringAngle < -25f)
        {
            if (sterzataMassima)
            {
                steeringAngle = -45f;
                if (this.tag.Equals("Player") && maxSpeed > 8f)
                {
                    ridottoMaxSpeed = true;
                    valoreOriginaleMaxSpeed = maxSpeed;
                    maxSpeed = 8f;
                }
                
            } else
            {
                if (this.tag.Equals("Player"))
                {
                    maxSpeed = valoreOriginaleMaxSpeed;
                }
                
            }
            sterzataAmpia = true;
        }

        if (!sterzataAmpia)
        {
            steeringAngle = PIDControllerSterzata.UpdatePars(steer, turnPrecedente, Time.fixedDeltaTime);
        }

        //Limit the steering angle
        steeringAngle = Mathf.Clamp(steeringAngle, -45, 45);


        float indice = 0;
        float differenzaTurn = Math.Abs(steeringAngle - turnPrecedente);
        float indice1 = Mathf.Clamp(1 - (differenzaTurn / 20f), 0.1f, 1) * _pidPars.indiceSterzataCurva;
        if (sterzataAmpia)
        {
            indice = _pidPars.indiceSterzataMassima;
        }
        else
        {
            indice = _pidPars.indiceSterzataDritto;
        }

        averageSteeringAngle = averageSteeringAngle + ((steeringAngle - averageSteeringAngle) / indice);
        if (Mathf.Abs(averageSteeringAngle - turnPrecedente) <= _pidPars.puntoMortoSterzata)
        {
            averageSteeringAngle = turnPrecedente;
        }

        currentTurn = averageSteeringAngle;
        currentTurn = Mathf.Clamp(Mathf.Lerp(turnPrecedente, currentTurn, steerSpeed * Time.deltaTime), -45f, 45f);
    }

    public float maxThrottle = 0.7f; //era a 0.8
    public float steerSpeed = 4.0f;
    public float steerSpeedCurva = 10.0f;
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
        if (evitare)
        {
            return;
        }

        if (velocitaAttuale < 0.01f && currentThrottle == 0f)
        {
            //sono fermo ad incrocio
            PIDControllerAccelerazione.resetValues();
        }

        float throttlePrecedente = GetComponent<VehicleController>().accellInput;

        float speedDifference = currentSpeed - velocitaAttuale;

        currentThrottle = PIDControllerAccelerazione.UpdatePars(currentSpeed, velocitaAttuale, Time.fixedDeltaTime);
        if (velocitaAttuale > currentSpeed)
        {
            if (hasStopTarget || frenata || hasGiveWayTarget || Math.Abs(speedDifference) > _pidPars.sogliaNoGas || (autoDavanti && Math.Abs(speedDifference) > sogliaNoGasTraffico) || (hasNextEntry && intersectionCornerSpeed < 0.8f) || (autoDavantiFrenata))
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
            }
            else
            {
                //devo solo rallentare ma non ho motivo di frenare
                currentThrottle = Mathf.Clamp(currentThrottle, 0f, maxThrottle);
            }

        }
        else
        {
            if (hasStopTarget || frenata || (hasNextEntry && intersectionCornerSpeed < 0.8f && velocitaAttuale > 5f))
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
    }

    void MoveCarUtenteSterzata()
    {
        if (AppController.Instance.UserInput is SteeringWheelInputController)
        {
            DirectInputWrapper.PlaySpringForce(0, Mathf.RoundToInt(currentTurn / 45f * 10000f), _pidPars.saturazione, _pidPars.coefficiente);            
        }
        vehicleController.steerInput = currentTurn / 45f;
    }


    private void MoveCar()
    {       
        transform.Translate(Vector3.forward * currentSpeed * Time.fixedDeltaTime);
        transform.Rotate(Vector3.up * currentTurn * Time.fixedDeltaTime);
    }


    private bool primaVoltaEvita = true;
    private float throttleOk = 0;

    DateTime inizioSostaDopoPericolo;
    bool inizioSosta = false;
    float durataSosta;
    bool situazionePalla = false;

    private void evita()
    {
        //questo metodo fa si che l'auto eviti un ostacolo imminente e frenando e sterzando bruscamente
        if (hasStopTarget)
        {
            //mi sto fermando allo stop o al semaforo e c'è un ostacolo in prossimità (es. pedone che attraversa la strada)
            //mi fermerò comunque però freno leggermente di piu
            if (primaVoltaEvita)
            {
                currentThrottle = Mathf.Clamp(currentThrottle - 0.3f, -1f, -0.2f);
                throttleOk = currentThrottle;
                vehicleController.accellInput = currentThrottle;
                primaVoltaEvita = false;
            }
            else
            {
                vehicleController.accellInput = throttleOk;
            }
            return;
        }
        primaVoltaEvita = true;
        float sterzata = 0;
        if (direzioneSpostamentoDestra == true)
        {
            sterzata = 25f;
        }
        else
        {
            sterzata = -25f;
        }

        //calcolo la quantita di freno necessaria
        float riduzioneVelocitaNecessaria = velocitaAttuale / Vector3.Distance(ostacoloEvitare.transform.position, nose.transform.position);

        //currentThrottle = Mathf.Clamp(-riduzioneVelocitaNecessaria, -1f, -0.5f);
        currentThrottle = Mathf.Clamp(-riduzioneVelocitaNecessaria, -1f, 0f);
        vehicleController.accellInput = currentThrottle;
        playAudio = true;
        if (Vector3.Distance(transform.position, ostacoloEvitare.transform.position) < 8f && currentThrottle < -0.5f)
        {
            vehicleController.steerInput = sterzata / 45;
        }

        currentSpeed = velocitaAttuale;

        if (inizioSosta)
        {
            SteerCar();
            if (situazionePalla)
            {
                //vehicleController.accellInput = -0.05f;
                vehicleController.accellInput = 0f;
                
            }            
            TimeSpan differenza = (DateTime.Now - inizioSostaDopoPericolo);
            float secondi = differenza.Seconds + differenza.Milliseconds / 1000;
            if (situazionePalla && secondi > 2f)
            {
                vehicleController.accellInput = -0.05f;
            }
            if (secondi > durataSosta)
            {              
                inizioSosta = false;
                evitare = false;
                playAudio = false;
                if (situazionePalla)
                {
                    situazionePalla = false;
                    GameObject palla = GameObject.Find("SF_Ball");
                    GameObject.Destroy(palla);
                }
                
            }
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
            distanzaIniziale = Math.Abs(vettoreDifferenza.x);
            //il +2 garantisce che si fermi un po prima
        }
        else
        {
            distanzaIniziale = Math.Abs(vettoreDifferenza.z); 
        }        
    }

    //ANTONELLO
    private void calcolaDistanzaInizialeInchiodata()
    {
        //Questo metodo serve a calcolare la distanza dal punto in cui abbiamo deciso di doverci fermare
        //e il punto in cui dobbiamo effettivamente fermarci
        Vector3 vettoreDifferenza = frenataTarget - transform.position;
        if (Math.Abs(vettoreDifferenza.x) > Math.Abs(vettoreDifferenza.z))
        {
            distanzaInizialeInchiodata = Math.Abs(vettoreDifferenza.x);
        }
        else
        {
            distanzaInizialeInchiodata = Math.Abs(vettoreDifferenza.z); 
        }
        distanzaInizialeInchiodata -= 6f;
    }




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
        if (go == null)
        {
            go = GameObject.Find("TeslaModelS_2_RiggedLOD");
        }
        if (go == null)
        {
            go = GameObject.Find("TeslaModelS_2_RiggedLOD(Clone)");
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


    internal class GestoreCollisioni : MonoBehaviour
    {
        TrafAIMotor motor = null;

        public void setMotor(TrafAIMotor motor)
        {
            this.motor = motor;
        }

        //ANTONELLO
        void OnTriggerEnter(Collider other)
        {

            if (!other.gameObject.layer.Equals(12) || motor.evitare == true) //|| motor.hasStopTarget == true)
            {
                //layer 12 equivale a obstacle: se l'oggetto incontrato non è un ostacolo allora non faccio niente, sono elementi dell'ambiente oppure auto del traffico, gia gestite tramite raycast
                //se sto gia inchiodando non faccio nulla
                //se hasStopTarget = true significa che l'ostacolo si trova in corrispondenza di un incrocio al quale devo fermami ed ho gia iniziato la procedura, dunque non faccio nulla
                //if (motor.hasStopTarget)
                //{
                //    motor.distanzaIniziale += 2f;
                //}
                return;
            }
                motor.evitare = true;
                motor.ostacoloEvitare = other.gameObject;

                float angolo = Vector3.SignedAngle(transform.forward, other.gameObject.transform.position - transform.position, Vector3.up);
                if (angolo >= 0)
                {
                    //l'oggetto è a destra ma si sta spostando (verso sinistra), lo evito buttandomi a destra
                    motor.direzioneSpostamentoDestra = true;
                }
                else
                {
                    //l'oggetto è a sinistra ma si sta spostando (verso destra), lo evito buttandomi a sinsitra
                    motor.direzioneSpostamentoDestra = false;
                }
        }

        //ANTONELLO
        void OnTriggerExit(Collider other)
        {
            if (!other.gameObject.layer.Equals(12))
            {
                return;
            }
            if (motor.hasStopTarget)
            {
                //Sono gia fermo all'incrocio non devo sostare
                motor.evitare = false;
                return;
            }
            if (!motor.inizioSosta) //|| motor.velocitaAttuale > 2f)
            {
                //motor.evitare = false;
                motor.inizioSostaDopoPericolo = DateTime.Now;
                motor.inizioSosta = true;
                if (other.gameObject.GetComponentInParent<BallObstacle>())
                {
                    motor.situazionePalla = true;
                    motor.durataSosta = 6.99f;
                    return;
                }
                if (other.gameObject.name.Equals("Body1"))
                {
                    //è una macchina del traffico
                    if (other.gameObject.GetComponentInParent<AutoTrafficoNoRayCast>() == null)
                    {
                        //Caso auto davanti a noi che taglia la strada davanti -> non ho bisogno di fermarmi
                        motor.durataSosta = 0f;
                        return;
                    }
                }
                
                if (motor.velocitaAttuale < 2f)
                {
                    motor.durataSosta = 5.99f;
                } else
                {
                    motor.durataSosta = 4.99f;
                }
            }           
        }
    }
}