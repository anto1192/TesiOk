/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using System;
using System.Collections.Generic;
using UnityEngine;

public class TrafAIMotor : MonoBehaviour
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
    private bool hasNextEntry = false;
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
    private Vector3 nextTarget = Vector3.zero;

    public const float giveWayRegisterDistance = 12f;
    //ANTONELLO
    //public const float brakeDistance = 10f;
    public const float brakeDistance = 20f; //stabilisce dove arriva il raycast
    //public const float yellowLightGoDistance = 4f;
    //ANTONELLO
    public const float yellowLightGoDistance = 10f; //per evitare che inchiodi quando vede il semaforo arancione troppo tardi
    public const float stopLength = 6f;
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
    /*private bool qualcosaDestra = false;
    private bool qualcosaSinistra = false;*/


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

    //ANTONELLO
    public void Init()
    {
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
        

    }




    public float NextRaycastTime()
    {
        return Time.time + UnityEngine.Random.value / 4 + 0.1f;
        var ciao = new Tuple<String, int>(); 
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
        if (this.gameObject.layer == 12)
        {
            Physics.Raycast(transform.position + Vector3.up * 2, -transform.up, out heightHit, 100f, ~(1 << LayerMask.NameToLayer("obstacle")));
        } else
        {
            Physics.Raycast(transform.position + Vector3.up * 2, -transform.up, out heightHit, 100f, ~(1 << LayerMask.NameToLayer("Traffic")));

        }
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
            SteerCar();
            MoveCar();
        } else
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

    void Update()
    {
        if(!inited)
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

        if(!currentEntry.isIntersection() && currentIndex > 0 && !hasNextEntry)
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
            if (Vector3.Distance(nose.transform.position, currentEntry.waypoints[currentEntry.waypoints.Count - 1]) <= 20f) 
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
                    //ANTONELLO
                    if (!this.tag.Equals("Player"))
                    {
                        Destroy(gameObject);
                    } else
                    {
                        inchioda();
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

        float distanzaWaypointFramePrecedente = distanzaWaypoint;
        distanzaWaypoint = Vector3.Distance(nose.transform.position, target);

        //check if we have reached the target waypoint
        if (distanzaWaypoint <= waypointThreshold)// && !hasStopTarget && !hasGiveWayTarget)
        {
            distanzaWaypoint = 0;
            modificaTarget(false);
            numeroWaypointSaltati = 0;
        } else
        {
            if (distanzaWaypointFramePrecedente != 0 && (distanzaWaypoint - distanzaWaypointFramePrecedente) > 0)
            {
                contatore++;
                if (contatore >= 10)
                {
                    //abbiamo saltato il waypoint
                    if ((numeroWaypointSaltati++) >= 10)
                    {
                        //l'utente è intervenuto alla guida allontandandosi troppo, fermiamo il test
                        Property = "9999"; //settando Property a 9999 verrà chiamato il metodo che interrompe il test
                    }
                    Debug.Log("waypoint saltato");
                    distanzaWaypoint = 0;
                    contatore = 0;
                    modificaTarget(true);
                    //hasStopTarget = false;
                    //hasGiveWayTarget = false;
                }
                
            } else
            {
                contatore = 0;
            }
        }

        Debug.DrawLine(this.transform.position, target);

        //SteerCar();

        if(hasNextEntry && nextEntry.isIntersection() && nextEntry.intersection.stopSign) 
        {
            if(stopEnd == 0f) {
                hasStopTarget = true;
                velocitaInizialeFrenata = this.GetComponent<Rigidbody>().velocity.magnitude;
                stopTarget = nextTarget;
                calcolaDistanzaIniziale();
                stopEnd = Time.time + stopLength;
            } else if(Time.time > stopEnd) {
                if(nextEntry.intersection.stopQueue.Peek() == this) {
                    hasGiveWayTarget = false;
                    hasStopTarget = false;
                    stopEnd = 0f;
                    float distanza = Vector3.Distance(this.transform.position, stopTarget);
                    if (distanza < 10f)
                    {
                        //evita che quando ci fermiamo a uno stop (un pochino prima dello stopTarget), riparte e si riferma allo stopTarget effettivo
                        nextEntry.intersection.stopSign = false;
                    }
                } 
            }
        }


        if(hasNextEntry && nextEntry.isIntersection() && !nextEntry.intersection.stopSign)
        {
            //check next entry for stop needed
            if(nextEntry.MustGiveWay())
            {
                hasGiveWayTarget = true;
                velocitaInizialeFrenata = this.GetComponent<Rigidbody>().velocity.magnitude;
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
                if (!autoScorretta)
                {
                    hasStopTarget = true;
                    velocitaInizialeFrenata = this.GetComponent<Rigidbody>().velocity.magnitude;
                    stopTarget = nextTarget;

                    calcolaDistanzaIniziale();
                } //else autoScorretta = true, non rispetta i semafori rossi e passacomunque
                
            }
            else if(hasStopTarget && nextEntry.light.State == TrafLightState.GREEN)
            {

                //green light, go!   
                //ANTONELLO           
                hasStopTarget = false;

            }
            else if(!hasStopTarget && nextEntry.light.State == TrafLightState.YELLOW)
            {
                //yellow, stop if we aren't zooming on through
                //TODO: carry on if we are too fast/close

                if (Vector3.Distance(nextTarget, nose.transform.position) > yellowLightGoDistance)
                {
                    //ANTONELLO
                    hasStopTarget = true;
                    velocitaInizialeFrenata = this.GetComponent<Rigidbody>().velocity.magnitude;
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



        if (somethingInFront && !frenata)
        {
            if (hitInfo.rigidbody != null && (hitInfo.rigidbody.tag.Equals("TrafficCar") || hitInfo.rigidbody.tag.Equals("Player")))
            {
                Debug.DrawLine(this.transform.position, hitInfo.transform.position);
                if (hitInfo.distance <= 28f)
                {
                    if (((this.currentEntry.identifier >= 1000 || Vector3.Distance(this.gameObject.transform.position, currentEntry.waypoints[0]) < 10f) && (currentTurn > 5f || currentTurn < -5f))) //  || this.GetComponent<Rigidbody>().velocity.magnitude <= 0)
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
                                velocitaInizialeFrenata = this.GetComponent<Rigidbody>().velocity.magnitude;
                                frenataTarget = hitInfo.transform.position;
                                calcolaDistanzaInizialeInchiodata();
                            }
                        }
                    } else
                    {
                        TrafAIMotor macchinaDavanti = hitInfo.rigidbody.GetComponent<TrafAIMotor>();
                        if (this.tag.Equals("Player") && (macchinaDavanti.frenata || macchinaDavanti.hasStopTarget) && macchinaDavanti.currentSpeed > 8f)
                        {
                            //se l'auto davanti si sta fermando (ma è in movimento), mi fermo anche io
                            //if (macchinaDavanti.currentSpeed > 1f)
                            //{                                                              
                                frenata = true;
                                velocitaInizialeFrenata = this.GetComponent<Rigidbody>().velocity.magnitude;
                                Vector3 targetAutoDavanti = Vector3.zero;
                                if (macchinaDavanti.frenata)
                                {
                                    targetAutoDavanti = macchinaDavanti.frenataTarget;
                                
                                } else
                                {
                                    targetAutoDavanti = macchinaDavanti.stopTarget;
                                }
                                Vector3 vettoreDifferenza = targetAutoDavanti - hitInfo.transform.position;
                                if (Math.Abs(vettoreDifferenza.x) > Math.Abs(vettoreDifferenza.z))
                                {
                                    if (hitInfo.transform.position.x > targetAutoDavanti.x) { 
                                        frenataTarget = new Vector3(targetAutoDavanti.x + 4f, targetAutoDavanti.y, targetAutoDavanti.z);
                                    } else
                                    {
                                        frenataTarget = new Vector3(targetAutoDavanti.x - 4f, targetAutoDavanti.y, targetAutoDavanti.z);
                                    }
                                } 
                                else
                                {
                                    if (hitInfo.transform.position.x > targetAutoDavanti.x)
                                    {
                                        frenataTarget = new Vector3(targetAutoDavanti.x, targetAutoDavanti.y, targetAutoDavanti.z + 4f);
                                    }
                                    else
                                    {
                                        frenataTarget = new Vector3(targetAutoDavanti.x, targetAutoDavanti.y, targetAutoDavanti.z -4f);
                                    }
                                }
                                calcolaDistanzaInizialeInchiodata();
                            //}
                            
                        } else { 
                            
                            //negli altri casi valuto la velocità dell'auto davanti
                            float frontSpeed = hitInfo.rigidbody.velocity.magnitude;
                        
                            if (frontSpeed > 1f)
                            {
                                //se l'auto non è ferma
                                float velocitaTarget = Mathf.Min(targetSpeed, (frontSpeed - 0.2f));
                                float differenzaVelocita = Math.Abs(this.GetComponent<Rigidbody>().velocity.magnitude - velocitaTarget);

                                if (hitInfo.distance <= 28f && hitInfo.distance > 20f)
                                {
                                    if (differenzaVelocita >= 5f)
                                    {
                                        //cè una bella differenza di velocità tra me e la macchina davanti, quindi comincio a rallentare, rallento con target di velocita la sua velocita + 3
                                        targetSpeed = Mathf.Lerp(this.GetComponent<Rigidbody>().velocity.magnitude, targetSpeed+3f, throttleSpeed * Time.deltaTime);
                                    }
                                }

                                if (hitInfo.distance <= 20f && hitInfo.distance >= 5f)
                                {
                                    //la distanza si è ridotta a meno di 20
                                
                                    if (differenzaVelocita >= 3f && hitInfo.distance >= 14f)
                                    {
                                        //c'è parecchia differenza di velocita ma la macchina davanti è ancora lontana, rallento con target di velocita la sua velocita + 1.5
                                        targetSpeed = Mathf.Lerp(this.GetComponent<Rigidbody>().velocity.magnitude, targetSpeed + 1.5f, throttleSpeed * Time.deltaTime);
                                    }
                                    else { 
                                        //la distanza è minore di 12
                                        if (differenzaVelocita >= 2f)
                                        {
                                            //c'è un po di differenza di velocità, uso l'interpolazione lineare per settare la velocità target uguale a quella dell'auto di fronte
                                            targetSpeed = Mathf.Lerp(this.GetComponent<Rigidbody>().velocity.magnitude, velocitaTarget, throttleSpeed * Time.deltaTime);
                                        } else
                                        {
                                            //poca differenza di velocita (probabilmetne sono accodato all'auto), setto direttamente la velocita target uguale a quella dell'auto di fronte
                                            targetSpeed = velocitaTarget;
                                        }
                                    }
                                    if (this.tag.Equals("Player"))
                                    {
                                        Debug.Log("TargetSpeed: " + targetSpeed + "; distanza: " + hitInfo.distance);
                                    }


                                    if (targetSpeed <= 2.5f)
                                    {
                                        targetSpeed += 3f;
                                    }
                                }
                                /*if (hitInfo.distance < 5f && hitInfo.distance >= 4f && frontSpeed > 2f)
                                {
                                    //se la distanza si è ridotta a 5f provo a rallentare un po prima di iniziare la fase di frenata
                                    if (frontSpeed >= 3f)
                                    {
                                        targetSpeed = Mathf.Min(targetSpeed, (frontSpeed - 2f));
                                    } else
                                    {
                                        targetSpeed = Mathf.Min(targetSpeed, (frontSpeed));
                                    }
                                
                                }*/
                            
                                //se la distanza si riduce a 5 mi fermo in ogni caso (se la macchina è piu lenta di me)
                                if ((hitInfo.distance < 5f) && frontSpeed < this.GetComponent<Rigidbody>().velocity.magnitude)
                                {
                                    frenata = true;
                                    velocitaInizialeFrenata = this.GetComponent<Rigidbody>().velocity.magnitude;
                                    frenataTarget = hitInfo.transform.position;
                                    calcolaDistanzaInizialeInchiodata();
                                }
                            }
                            else
                            {
                                //la macchina davanti è ferma o estremamente lenta (si può considerare ferma)
                                if (frontSpeed < this.GetComponent<Rigidbody>().velocity.magnitude) { 
                                    frenata = true;
                                    velocitaInizialeFrenata = this.GetComponent<Rigidbody>().velocity.magnitude;
                                    frenataTarget = hitInfo.transform.position;
                                    calcolaDistanzaInizialeInchiodata();
                                }
                            }
                        }
                    }
                } /*else {
                    //float frontSpeed = Mathf.Clamp((hitInfo.distance - 1f) / 3f, 0f, maxSpeed);
                    float frontSpeed = hitInfo.rigidbody.GetComponent<TrafAIMotor>().currentSpeed;
                    if (frontSpeed > 5f)
                    {
                    

                        targetSpeed = Mathf.Min(targetSpeed, frontSpeed);
                        targetSpeed = Mathf.MoveTowards(this.GetComponent<Rigidbody>().velocity.magnitude, targetSpeed, throttleSpeed * Time.deltaTime);
                        if (this.tag.Equals("Player"))
                        {
                            Debug.Log("TargetSpeed: " + targetSpeed);
                        }
                    }
                }*/
            }
        } else
        {           
            if (frenata)
            {
                //if ((!somethingInFront) || (somethingInFront && hitInfo.distance >= 2f && this.GetComponent<Rigidbody>().velocity.magnitude <= 1f) && (hitInfo.rigidbody.tag.Equals("TrafficCar") || hitInfo.rigidbody.tag.Equals("Player")))
                if (hitInfo.rigidbody == null)
                {
                    frenata = false;
                } else {

                    float frontSpeed = hitInfo.rigidbody.velocity.magnitude;
                    float miaVelocita = this.GetComponent<Rigidbody>().velocity.magnitude;

                    if ((!somethingInFront) || ((frontSpeed - miaVelocita) >= 0.5f && hitInfo.distance >= 1f) && (hitInfo.rigidbody.tag.Equals("TrafficCar") || hitInfo.rigidbody.tag.Equals("Player")))

                    {
                        //Se mi sono fermato per via di qualcosa di fronte che ora non c'è piu devo ripartire
                        //se invece c'è qualcosa davanti, frenata è true, ma la distanza è maggiore di 5f (piu è basso piu riparte prima quando si ferma dietro un'auto)
                        //e l'auto è ferma, significa che l'auto davanti a me sta ripartendo quindi posso ripartire anche io
                        frenata = false;
                        if (this.tag.Equals("Player"))
                        {
                            Debug.Log("Frenata false, hitinfo.distance = " + hitInfo.distance);
                        }
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
                distanzaCorrente -= 4f;
            } else
            {
                distanzaCorrente -= 2f;
            }
            
            Debug.DrawLine(frenataTarget, transform.position);
            if (velocitaInizialeFrenata >= 6f)
            {
                targetSpeed = velocitaInizialeFrenata * distanzaCorrente / distanzaInizialeInchiodata;
            } else
            {
                targetSpeed = 6f * distanzaCorrente / distanzaInizialeInchiodata;
            }
            
            if (targetSpeed <= 2)
            {
                targetSpeed = 0;
                //Debug.Log("target < 2");
            }
            //else
                //Debug.Log("distanza Iniziale Inchiodata = " + distanzaInizialeInchiodata + "; distanzaCorrente = " + distanzaCorrente + "; targetSpeed: " + targetSpeed);

        }

        if (!frenata && (hasStopTarget || hasGiveWayTarget))
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
            distanzaCorrente -= 3f;
            Debug.DrawLine(stopTarget, transform.position);
            //targetSpeed = velocitaInizialeFrenata * distanzaCorrente / distanzaIniziale;
            if (velocitaInizialeFrenata >= 6f)
            {
                targetSpeed = velocitaInizialeFrenata * distanzaCorrente / distanzaIniziale;
            }
            else
            {
                targetSpeed = 6f * distanzaCorrente / distanzaIniziale;
            }
            if (targetSpeed <= 1f)
            {
                targetSpeed = 0;
                //Debug.Log("target < 2");
            } //else 
            //Debug.Log("distanza Iniziale = " + distanzaIniziale + "; distanzaCorrente = " + distanzaCorrente + "; targetSpeed: " + targetSpeed);
        }

        //slow down if we need to turn
        //if(currentEntry.isIntersection() || hasNextEntry)
        //ANTONELLO
        if ((currentEntry.isIntersection() || hasNextEntry) && !hasStopTarget && !frenata)
        {
            targetSpeed = targetSpeed * intersectionCornerSpeed;
        }
        else
        {
            //DA TESTARE - ANTONELLO
            if (currentTurn > 5f || currentTurn < -5f)
            {
                //targetSpeed = targetSpeed * Mathf.Clamp(1 - (currentTurn / maxTurn), 0.1f, 1f);
                targetSpeed = targetSpeed * Mathf.Clamp(1 - (currentTurn / maxTurn), 0.3f, 1f);
            }
            
        }

        if(targetSpeed > currentSpeed)
        {          
            float distanzaCorrente = 0;
            if (hasStopTarget) {
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
            if ((hasStopTarget) && distanzaCorrente < 6f)
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
            if (hasStopTarget || frenata)
            {
                /*if (targetSpeed <= currentSpeed)
                {
                    //serve questa condizione perchè evita di accelerare quando sono in fase di fermata
                    currentSpeed = targetSpeed; //dovrei aver risolto in altro modo
                }*/
                currentSpeed = targetSpeed;
            }

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
                    Debug.Log("no edges on " + currentEntry.identifier + "_" + currentEntry.subIdentifier);
                    Destroy(gameObject);
                    inited = false;
                    return;
                }

                currentEntry = system.GetEntry(newNode.id, newNode.subId);
                nextEntry = null;
                hasNextEntry = false;
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
                    target = nextEntry.waypoints[0];
                }
                else
                {
                    if (nextEntry != null && nextEntry.identifier != 0) //ANTONELLO
                    {
                        currentEntry = nextEntry;
                        nextEntry = null;
                        hasNextEntry = false;
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
            target = currentEntry.waypoints[currentIndex];
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
        float turnPrecedente = 0;
        if (this.tag.Equals("Player"))
        {
            turnPrecedente = vehicleController.steerInput * 45f;
        } else
        {
            turnPrecedente = currentTurn;
        }

        currentTurn = Mathf.Clamp((Vector3.Cross(transform.forward, steerVector).y < 0 ? -steer : steer), -maxTurn, maxTurn);
        //currentTurn = Mathf.MoveTowards(turnPrecedente, currentTurn, Time.deltaTime);
        //DateTime ora;
        if (Math.Abs(currentTurn - turnPrecedente) >= 0.2f )
        {
            angoloSterzataAlto = true;
            //ora = DateTime.Now;
            //currentTurn = (currentTurn + turnPrecedente) / 2f;
            float differenzaTurn = Math.Abs(currentTurn - turnPrecedente);
            /*    if (currentTurn > turnPrecedente)
                {
                    currentTurn = turnPrecedente + (differenzaTurn / 5);
                }
                else
                {
                    currentTurn = turnPrecedente - (differenzaTurn / 5);
                }*/
            currentTurn = Mathf.Lerp(turnPrecedente, currentTurn, 5);
            
        } else
        {
            if (angoloSterzataAlto)
            {
                angoloSterzataAlto = false;
            }
        }
    }

    public float maxThrottle = 0.8f;
    public float steerSpeed = 4.0f;
    public float throttleSpeed = 3.0f;
    public float brakeSpeed = 1f;
    private float m_targetSteer = 0.0f;
    private float m_targetThrottle = 0.0f;
    private float currentThrottle = 0f;


    //METODO PER FAR MUOVERE LA MACCHINA USANDO IL VEHICLE CONTROLLER - ANTONELLO
    void MoveCarUtenteAccelerazione()
    {
        float throttlePrecedente = currentThrottle;

        /*var predicted = GetPredictedPoint();
        Vector3 steerVector = new Vector3(heightHit.normal.x, transform.position.y, heightHit.normal.z) - transform.position;

        float steer = Vector3.Angle(transform.forward, steerVector);
        m_targetSteer = (Vector3.Cross(transform.forward, steerVector).y < 0 ? -steer : steer) / vehicleController.maxSteeringAngle;

        if (Vector3.Distance(predicted, heightHit.normal) < 1f)
            m_targetSteer = 0f;*/


        float speedDifference = currentSpeed - GetComponent<Rigidbody>().velocity.magnitude;// * 2;
        float velocitaAttuale = GetComponent<Rigidbody>().velocity.magnitude;

        if (speedDifference <= 0)
        {
            //m_targetThrottle = -(currentSpeed - GetComponent<Rigidbody>().velocity.magnitude / currentSpeed) * maxBrake;
            currentThrottle = -(GetComponent<Rigidbody>().velocity.magnitude / currentSpeed) * maxBrake;
            currentThrottle = Mathf.Clamp(currentThrottle, -1, 0);
            if (currentSpeed == 0)
            {
                //se sono fermo, tengo premuto il freno al massimo
                currentThrottle = -1;
            }
        }
        else
        {
            m_targetThrottle = maxThrottle * Mathf.Clamp(1 - Mathf.Abs(currentTurn/45), 0.2f, 1f);
            //m_targetThrottle = maxThrottle;
            speedDifference *= m_targetThrottle;
            currentThrottle = Mathf.Clamp(Mathf.MoveTowards(currentThrottle, speedDifference / 2f, throttleSpeed * Time.deltaTime), -maxBrake, maxThrottle);
            currentThrottle = Mathf.Clamp(currentThrottle, 0, maxThrottle);
        }


        //vehicleController.accellInput = Mathf.MoveTowards(throttlePrecedente, currentThrottle, throttleSpeed * Time.deltaTime);
        vehicleController.accellInput = currentThrottle; 
    }

    void MoveCarUtenteSterzata()
    {
        if (currentTurn > 25f)
        {
            /*Debug.Log("massima sterzata a destra; currentTurn = " + currentTurn);
            vehicleController.steerInput = Mathf.MoveTowards(vehicleController.steerInput, 1f, steerSpeed * Time.deltaTime);
            return;*/
            currentTurn = 45f;
        }
        if (currentTurn < -25f)
        {
            /*Debug.Log("massima sterzata a sinistra; currentTurn = " + currentTurn);
            vehicleController.steerInput = Mathf.MoveTowards(vehicleController.steerInput, -1f, steerSpeed * Time.deltaTime);
            return;*/
            currentTurn = -45f;
        }

        if (currentEntry.identifier >= 1000)
        {
            //sono in un incrocio
            vehicleController.steerInput = Mathf.Lerp(vehicleController.steerInput, currentTurn / 45f, steerSpeed * Time.deltaTime);
            return;
        }
        if (angoloSterzataAlto)
        {
            vehicleController.steerInput = Mathf.Lerp(vehicleController.steerInput, currentTurn / 45f, steerSpeed * Time.deltaTime);
        }
        else
        {
            vehicleController.steerInput = Mathf.MoveTowards(vehicleController.steerInput, currentTurn / 45f, steerSpeed * Time.deltaTime);
        }
    }

    private void MoveCar()
    {
        GetComponent<Rigidbody>().MoveRotation(Quaternion.FromToRotation(Vector3.up, heightHit.normal) * Quaternion.Euler(0f, transform.eulerAngles.y + currentTurn * Time.fixedDeltaTime, 0f));
        GetComponent<Rigidbody>().MovePosition(transform.position + transform.forward * currentSpeed * Time.fixedDeltaTime);
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
        } else
        {
            sterzata = -45f;
        }
        
        
        //if (currentThrottle != -1f && currentTurn != sterzata)
        //{
            currentThrottle = -1f;
            
            vehicleController.steerInput = sterzata / 45;
            vehicleController.accellInput = currentThrottle;
        //} //altrimenti ho gia ricevuto i comandi per evitare l'ostacolo, non faccio niente
        
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
            go = GameObject.Find("TeslaModelS_2_Rigged (1)");
        }
        if (go == null)
        {
            go = GameObject.Find("TeslaModelS_2_Rigged (1)(Clone)");
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

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }


            
        //ANTONELLO
        void OnTriggerEnter(Collider other)
        {

            if (!other.gameObject.layer.Equals(12) || motor.inchiodata == true || motor.hasStopTarget == true)
            {
                //layer 12 equivale a obstacle: se l'oggetto incontrato non è un ostacolo allora non faccio niente, sono elementi dell'ambiente oppure auto del traffico, gia gestite tramite raycast
                //se sto gia inchiodando non faccio nulla
                //se hasStopTarget = true significa che l'ostacolo si trova in corrispondenza di un incrocio al quale devo fermami ed ho gia iniziato la procedura, dunque non faccio nulla
                return;
            }
            float distanza = Vector3.Distance(motor.transform.position, other.gameObject.transform.position);
            if (distanza < 11f && motor.currentSpeed > 7f)
            {
                //l'ostacolo è troppo vicino, non riesco a inchiodare, lo evito
                motor.evitare = true;

                //PS: se l'ostacolo improvvisamente compare davanti l'auto significa che è in movimento
                float angolo = Vector3.SignedAngle(motor.transform.position, other.gameObject.transform.position, motor.transform.forward);
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

            } else
            {
                motor.inchiodata = true;
            }        
        }


        //ANTONELLO
        void OnTriggerExit(Collider other)
        {
            if (!other.gameObject.layer.Equals(12))
            {
                return;
            }
            motor.inchiodata = false;
            motor.evitare = false;
        }

       


    }

}