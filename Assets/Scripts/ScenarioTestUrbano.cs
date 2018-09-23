using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using System;

public class ScenarioTestUrbano : MonoBehaviour
{
    public static event System.Action<GameObject> OnSpawnHeaps;
    public TrafSystem system;
    public GameObject[] prefabs;
    public GameObject prefabMacchinaOstacolo;
    public GameObject prefabScooter;


    //private TrafAIMotor macchinaTraffico;
    private TrafAIMotor macchinaTraffico;
    private AutoTrafficoNoRayCast macchinaTrafficoScorretta;

    private bool scenarioAvviato = false;
    private bool fineTest = false;
    private GameObject car;
    private float limiteVelocita = 13.8f;

    public int numeroInizioTest = 0;

    private PIDPars _pidPars;


    private void avviaScenarioTest() {
        if (_pidPars == null)
            _pidPars = Resources.Load<PIDPars>("PIDPars_steeringWheel");


        List<RoadGraphEdge> percorsoPlayer = ottieniPercorsoPlayer();

        if (car == null)
        {
            car = ottieniRiferimentoPlayer();
        }

        attivaGuidaAutomatica(car, percorsoPlayer, percorsoPlayer[0].id, percorsoPlayer[0].subId); //-> facciamo apparire la macchina utente nella posizione con id e subId specificati, che rappresenta il punto di partenza
                                                                                                   //la macchina seguirà il percorso definito in percorsoPlayer

        //car.GetComponent<xSimScript>().enabled = true;

        TrafAIMotor motor = car.GetComponent<TrafAIMotor>();
        motor.ChangeProperty += new TrafAIMotor.Delegato(gestisciEvento);
        motor.modificaSemaforo += new TrafAIMotor.Delegato(gestisciSemaforo);




        //modifico i waypoint del primo incrocio
        List<Vector3> waypPrimaStrada = system.GetEntry(120, 0).waypoints;
        waypPrimaStrada[waypPrimaStrada.Count - 1] = new Vector3(2224.73f, 10.03f, 385.28f);
        system.GetEntry(120, 0).waypoints = waypPrimaStrada;

        List<Vector3> waypPrimoIncrocio = system.GetEntry(1088, 3).waypoints;
        waypPrimoIncrocio[0] = new Vector3(2224.65f, 11.13f, 383.96f);
        waypPrimoIncrocio[1] = new Vector3(2224.3f, 11.13f, 379.614f);
        waypPrimoIncrocio[2] = new Vector3(2223.8f, 11.13f, 374.22f);
        waypPrimoIncrocio[3] = new Vector3(2223.2f, 11.13f, 368.66f);
        system.GetEntry(1088, 3).waypoints = waypPrimoIncrocio;


        //modifico i waypoint del secondo incrocio
        List<Vector3> waypSecondoIncrocio = system.GetEntry(1089, 5).waypoints;
        waypSecondoIncrocio[0] = new Vector3(2223.464f, 11.13f, 193.14f);
        waypSecondoIncrocio[1] = new Vector3(2222.942f, 11.13f, 191.74f);
        waypSecondoIncrocio[2] = new Vector3(2221.993f, 11.13f, 190.3f);
        waypSecondoIncrocio[3] = new Vector3(2220.24f, 11.13f, 189.62f);
        system.GetEntry(1089, 5).waypoints = waypSecondoIncrocio;



        system.GetEntry(39, 3).waypoints[0] = new Vector3(2217.644f, 12f, 190f);


        //modifico i waypoint al primo incrocio con l'arancione (al terzo incrocio)
        List<Vector3> waypPrimoIncrocioArancione = system.GetEntry(1082, 0).waypoints;
        
        for (int i = 0; i < waypPrimoIncrocioArancione.Count; i++)
        {
            waypPrimoIncrocioArancione[i] = new Vector3(waypPrimoIncrocioArancione[i].x, waypPrimoIncrocioArancione[i].y, 190.92f);
        }
        system.GetEntry(1082, 0).waypoints = waypPrimoIncrocioArancione;

        //modifico i waypoint all'incrocio 1081
        List<Vector3> listaWaypIncrocio1081 = system.GetEntry(1081, 18).waypoints;
        listaWaypIncrocio1081[0] = new Vector3(listaWaypIncrocio1081[0].x, listaWaypIncrocio1081[0].y, 190.43f);
        listaWaypIncrocio1081[1] = new Vector3(listaWaypIncrocio1081[1].x, listaWaypIncrocio1081[1].y, 191.07f);
        system.GetEntry(1081, 18).waypoints = listaWaypIncrocio1081;


        //modifico i waypoint all'incrocio 1007
        List<Vector3> listaWaypIncrocio1007 = system.GetEntry(1007, 4).waypoints;
        listaWaypIncrocio1007[0] = new Vector3(865.36f, 10.63f, 383.02f);
        listaWaypIncrocio1007[1] = new Vector3(866.1843f, 10.63f, 382.39f);
        listaWaypIncrocio1007[2] = new Vector3(867.0445f, 10.63f, 381.52f);
        listaWaypIncrocio1007[3] = new Vector3(868.1345f, 10.63f, 380.35f);
        system.GetEntry(1007, 4).waypoints = listaWaypIncrocio1007;


        //modifico i waypoint all'incrocio 1008
        List<Vector3> listaWaypIncrocio1008 = system.GetEntry(1008, 19).waypoints;
        listaWaypIncrocio1008[1] = new Vector3(1161.151f, 11.19462f, 946.95f);
        listaWaypIncrocio1008[2] = new Vector3(1163.327f, 11.19462f, 947.86f);
        listaWaypIncrocio1008[3] = new Vector3(1165.658f, 11.19462f, 948.49f);
        /*foreach (Vector3 wp in system.GetEntry(1008, 19).waypoints)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.position = wp;
            Collider co = go.GetComponent<Collider>();
            co.isTrigger = true;
            Debug.Log("wp: " + wp.x + "; " + wp.y + "; " + wp.z);
        }*/

        List<Vector3> listaWaypStrada5 = system.GetEntry(5, 0).waypoints;
        Vector3 wayp5_0 = system.GetEntry(5, 0).waypoints[0];
        system.GetEntry(5, 0).waypoints[0] = new Vector3(wayp5_0.x, wayp5_0.y, 380.73f);
        /*foreach (Vector3 wp in system.GetEntry(5, 0).waypoints)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.position = wp;
            Collider co = go.GetComponent<Collider>();
            co.isTrigger = true;
            Debug.Log("wp: " + wp.x + "; " + wp.y + "; " + wp.z);
        }*/
    }

    void Start()
    {
        car = ottieniRiferimentoPlayer();
    }

    private void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (car.GetComponent<xSimScript>().enabled)
            {
                car.GetComponent<xSimScript>().enabled = false;
            } else
            {
                car.GetComponent<xSimScript>().enabled = true;
            }           
        }
        if (fineTest)
        {
            //disattivo la piattaforma
            car.GetComponent<xSimScript>().enabled = false;
            //FERMO LA MACCHINA
            car.GetComponent<VehicleController>().accellInput = -0.5f;
        }
        if (fineTest && car.GetComponent<Rigidbody>().velocity.magnitude < 0.01f)
        {
            //l'auto è ferma
            scenarioAvviato = false;
            fineGuidaAutomatica();
            fineTest = false;
        }
    }

    void OnGUI()
    {
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.S)
        {
            if (!scenarioAvviato)
            {
                avviaScenarioTest();
                scenarioAvviato = true;
                fineTest = false;
            }
            else
            {
                scenarioAvviato = false;
                //fineGuidaAutomatica();
                fineTest = true;

            }
        }
    }




    public void gestisciSemaforo(int idPrecedente, int nuovoId)
    {
        string nomeMetodo = "semaforo" + nuovoId;
        //ScenarioTestUrbano.getInstance().Invoke(nomeMetodo, 2f);
        MethodInfo mi = this.GetType().GetMethod(nomeMetodo);
        if (mi != null)
        {
            mi.Invoke(this, null);
        }



    }

    public void gestisciEvento(int idPrecedente, int nuovoId)
    {
        string nomeMetodo = "evento" + nuovoId;
        MethodInfo mi = this.GetType().GetMethod(nomeMetodo);
        //Debug.Log("Sono in gestisciEvento " + nomeMetodo + "; mi == null? " + mi == null);
        if (mi != null)
        {
            mi.Invoke(this, null);
        }
        /*if (nuovoId == 18)
        {
            string nomeMetodo = "evento" + nuovoId;
            MethodInfo mi = ScenarioTestUrbano.getInstance().GetType().GetMethod(nomeMetodo);
            if (mi != null)
            {
                mi.Invoke(ScenarioTestUrbano.getInstance(), null);
            }
        }*/
    }


    //METODI PER IL CONTROLLO DEI SEMAFORI
    IEnumerator courutineSemaforoVerde(TrafficLightContainer contenitore)
    {
        while (true)
        {
            //yield return null;
            yield return new WaitForSeconds(2f);
            contenitore.Set(TrafLightState.GREEN);
            yield return new WaitForSeconds(15f);
            contenitore.Set(TrafLightState.YELLOW);
            yield return new WaitForSeconds(3f);
            contenitore.Set(TrafLightState.RED);
            yield return new WaitForSeconds(20f);
        }
    }

    IEnumerator courutineSemaforoVerdeLungo(TrafficLightContainer contenitore)
    {
        while (true)
        {
            //yield return null;
            yield return new WaitForSeconds(2f);
            contenitore.Set(TrafLightState.GREEN);
            yield return new WaitForSeconds(25f);
            contenitore.Set(TrafLightState.YELLOW);
            yield return new WaitForSeconds(3f);
            contenitore.Set(TrafLightState.RED);
            yield return new WaitForSeconds(30f);
        }
    }

    IEnumerator courutineSemaforoRosso(TrafficLightContainer contenitore)
    {
        while (true)
        {
            //yield return null;
            contenitore.Set(TrafLightState.RED);
            yield return new WaitForSeconds(22f);
            contenitore.Set(TrafLightState.GREEN);
            yield return new WaitForSeconds(15f);
            contenitore.Set(TrafLightState.YELLOW);
            yield return new WaitForSeconds(3f);
        }
    }

    IEnumerator courutineSemaforoRossoLungo(TrafficLightContainer contenitore)
    {
        while (true)
        {
            //yield return null;
            contenitore.Set(TrafLightState.RED);
            yield return new WaitForSeconds(32f);
            contenitore.Set(TrafLightState.GREEN);
            yield return new WaitForSeconds(25f);
            contenitore.Set(TrafLightState.YELLOW);
            yield return new WaitForSeconds(3f);
        }
    }

    IEnumerator courutineSemaforoArancionePasso(TrafficLightContainer contenitore1, TrafficLightContainer contenitore2)
    {
        while (true)
        {
            //yield return null;
            contenitore1.Set(TrafLightState.GREEN);
            contenitore2.Set(TrafLightState.RED);
            yield return new WaitForSeconds(5f);
            contenitore1.Set(TrafLightState.YELLOW);
            yield return new WaitForSeconds(3f);
            contenitore1.Set(TrafLightState.RED);
            yield return new WaitForSeconds(2f);
            contenitore2.Set(TrafLightState.GREEN);
            yield return new WaitForSeconds(15f);
            contenitore2.Set(TrafLightState.YELLOW);
            yield return new WaitForSeconds(3f);
            contenitore2.Set(TrafLightState.RED);
            yield return new WaitForSeconds(2f);
            contenitore1.Set(TrafLightState.GREEN);
            yield return new WaitForSeconds(10f);
        }
    }

    IEnumerator courutineSemaforoArancionePasso3Lights(TrafficLightContainer contenitore1, TrafficLightContainer contenitore2, TrafficLightContainer contenitore3)
    {
        while (true)
        {
            //yield return null;
            contenitore1.Set(TrafLightState.GREEN);
            contenitore2.Set(TrafLightState.RED);
            contenitore3.Set(TrafLightState.RED);
            yield return new WaitForSeconds(5f);
            contenitore1.Set(TrafLightState.YELLOW);
            yield return new WaitForSeconds(3.5f);
            contenitore1.Set(TrafLightState.RED);
            yield return new WaitForSeconds(2f);
            contenitore2.Set(TrafLightState.GREEN);
            contenitore3.Set(TrafLightState.GREEN);
            yield return new WaitForSeconds(15f);
            contenitore2.Set(TrafLightState.YELLOW);
            contenitore3.Set(TrafLightState.YELLOW);
            yield return new WaitForSeconds(3f);
            contenitore2.Set(TrafLightState.RED);
            contenitore3.Set(TrafLightState.RED);
            yield return new WaitForSeconds(2f);
            contenitore1.Set(TrafLightState.GREEN);
            yield return new WaitForSeconds(10f);
        }
    }

    IEnumerator courutineSemaforoArancioneMiFermo(TrafficLightContainer contenitore1, TrafficLightContainer contenitore2)
    {
        while (true)
        {
            //yield return null;
            contenitore1.Set(TrafLightState.GREEN);
            contenitore2.Set(TrafLightState.RED);
            yield return new WaitForSeconds(2.5f);  //3
            contenitore1.Set(TrafLightState.YELLOW);
            yield return new WaitForSeconds(3f);
            contenitore1.Set(TrafLightState.RED);
            yield return new WaitForSeconds(2f);
            contenitore2.Set(TrafLightState.GREEN);
            yield return new WaitForSeconds(15f);
            contenitore2.Set(TrafLightState.YELLOW);
            yield return new WaitForSeconds(3f);
            contenitore2.Set(TrafLightState.RED);
            yield return new WaitForSeconds(2f);
            contenitore1.Set(TrafLightState.GREEN);
            yield return new WaitForSeconds(13f);
        }
    }

    public void semaforo120()
    {
        //Debug.Log("sono in semaforo 120");
        TrafEntry entry = system.GetEntry(1088, 3);
        TrafficLightContainer container = entry.light;
        TrafficLight[] lights = container.gameObject.GetComponentsInParent<TrafficLight>();
        lights[0].StopAllCoroutines();
        lights[0].StartCoroutine(courutineSemaforoVerde(container));
    }

    public void semaforo119()
    {
        //Debug.Log("sono in semaforo 119");            
        TrafEntry entry = system.GetEntry(1089, 5);
        TrafficLightContainer container = entry.light;
        TrafficLight[] lights = container.gameObject.GetComponentsInParent<TrafficLight>();
        lights[0].StopAllCoroutines();
        lights[0].StartCoroutine(courutineSemaforoRosso(container));
    }

    public void semaforo39()
    {
        //Debug.Log("sono in semaforo 39");
        TrafEntry entry = system.GetEntry(1082, 0);
        TrafEntry entry1 = system.GetEntry(1082, 6);
        TrafficLightContainer container = entry.light;
        TrafficLightContainer container1 = entry1.light;
        TrafficLight[] lights = container.gameObject.GetComponentsInParent<TrafficLight>();
        lights[0].StopAllCoroutines();
        lights[0].StartCoroutine(courutineSemaforoArancioneMiFermo(container, container1));
        semaforo38a();
    }

    public void semaforo38a()
    {
        TrafEntry entry = system.GetEntry(1081, 18);
        TrafEntry entry1 = system.GetEntry(1081, 14);
        TrafEntry entry2 = system.GetEntry(1081, 9);
        TrafficLightContainer container = entry.light;
        TrafficLightContainer container1 = entry1.light;
        TrafficLightContainer container2 = entry2.light;
        TrafficLight[] lights = container.gameObject.GetComponentsInParent<TrafficLight>();
        lights[0].StopAllCoroutines();
        lights[0].StartCoroutine(courutineSemaforoVerde(container));
        lights[0].StartCoroutine(courutineSemaforoRosso(container1));
        lights[0].StartCoroutine(courutineSemaforoRosso(container2));

    }

    public void semaforo38()
    {
        TrafEntry entry = system.GetEntry(1081, 18);
        TrafEntry entry1 = system.GetEntry(1081, 14);
        TrafEntry entry2 = system.GetEntry(1081, 9);
        TrafficLightContainer container = entry.light;
        TrafficLightContainer container1 = entry1.light;
        TrafficLightContainer container2 = entry2.light;
        TrafficLight[] lights = container.gameObject.GetComponentsInParent<TrafficLight>();
        lights[0].StopAllCoroutines();
        lights[0].StartCoroutine(courutineSemaforoRosso(container));
        lights[0].StartCoroutine(courutineSemaforoVerde(container1));
        lights[0].StartCoroutine(courutineSemaforoVerde(container2));

        semaforo49a();

    }

    public void semaforo49a()
    {
        TrafEntry entry = system.GetEntry(1083, 1);
        TrafEntry entry1 = system.GetEntry(1083, 2);
        TrafEntry entry2 = system.GetEntry(1083, 4);
        TrafficLightContainer container = entry.light;
        TrafficLightContainer container1 = entry1.light;
        TrafficLightContainer container2 = entry2.light;
        TrafficLight[] lights = container.gameObject.GetComponentsInParent<TrafficLight>();
        lights[0].StopAllCoroutines();
        lights[0].StartCoroutine(courutineSemaforoVerde(container));
        lights[0].StartCoroutine(courutineSemaforoVerde(container1));
        lights[0].StartCoroutine(courutineSemaforoRosso(container2));
    }

    public void semaforo49()
    {
        TrafEntry entry = system.GetEntry(1083, 1);
        TrafEntry entry1 = system.GetEntry(1083, 2);
        TrafEntry entry2 = system.GetEntry(1083, 4);
        TrafficLightContainer container = entry.light;
        TrafficLightContainer container1 = entry1.light;
        TrafficLightContainer container2 = entry2.light;
        TrafficLight[] lights = container.gameObject.GetComponentsInParent<TrafficLight>();
        lights[0].StopAllCoroutines();
        lights[0].StartCoroutine(courutineSemaforoRosso(container));
        lights[0].StartCoroutine(courutineSemaforoRosso(container1));
        lights[0].StartCoroutine(courutineSemaforoVerde(container2));

        List<RoadGraphEdge> percorso49_2 = ottieniPercorso49_2();
        List<RoadGraphEdge> percorso49_3 = ottieniPercorso49_3();
        List<RoadGraphEdge> percorso49_4 = ottieniPercorso49_4();
        List<RoadGraphEdge> percorso49_5 = ottieniPercorso49_5();
        CreaMacchinaTraffico(42, 3, 0f, percorso49_2);
        CreaMacchinaTraffico(42, 2, 0.8f, percorso49_3);
        CreaMacchinaTraffico(42, 1, 0.2f, percorso49_4);
        CreaMacchinaTraffico(42, 0, 0f, percorso49_5);
    }

    public void semaforo50()
    {
        TrafEntry entry = system.GetEntry(1010, 4);
        TrafEntry entry1 = system.GetEntry(1010, 2);
        TrafEntry entry2 = system.GetEntry(1010, 0);
        TrafficLightContainer container = entry.light;
        TrafficLightContainer container1 = entry1.light;
        TrafficLightContainer container2 = entry2.light;
        TrafficLight[] lights = container.gameObject.GetComponentsInParent<TrafficLight>();
        lights[0].StopAllCoroutines();
        lights[0].StartCoroutine(courutineSemaforoArancionePasso(container1, container2));
    }

    public void semaforo50a()
    {
        TrafEntry entry = system.GetEntry(1010, 4);
        TrafEntry entry1 = system.GetEntry(1010, 2);
        TrafEntry entry2 = system.GetEntry(1010, 0);
        TrafficLightContainer container = entry.light;
        TrafficLightContainer container1 = entry1.light;
        TrafficLightContainer container2 = entry2.light;
        TrafficLight[] lights = container.gameObject.GetComponentsInParent<TrafficLight>();
        lights[0].StopAllCoroutines();
        lights[0].StartCoroutine(courutineSemaforoRosso(container));
        lights[0].StartCoroutine(courutineSemaforoVerde(container1));
        lights[0].StartCoroutine(courutineSemaforoVerde(container2));
    }

    public void semaforo63a()
    {
        TrafEntry entry = system.GetEntry(1036, 2);
        TrafEntry entry1 = system.GetEntry(1036, 8);
        TrafficLightContainer container = entry.light;
        TrafficLightContainer container1 = entry1.light;
        TrafficLight[] lights = container.gameObject.GetComponentsInParent<TrafficLight>();
        lights[0].StopAllCoroutines();
        lights[0].StartCoroutine(courutineSemaforoRosso(container));
        lights[0].StartCoroutine(courutineSemaforoVerde(container1));
    }

    public void semaforo64a()
    {
        TrafEntry entry = system.GetEntry(1035, 10);
        TrafEntry entry1 = system.GetEntry(1035, 0);
        TrafficLightContainer container = entry.light;
        TrafficLightContainer container1 = entry1.light;
        TrafficLight[] lights = container.gameObject.GetComponentsInParent<TrafficLight>();
        lights[0].StopAllCoroutines();
        lights[0].StartCoroutine(courutineSemaforoVerde(container));
        lights[0].StartCoroutine(courutineSemaforoRosso(container1));
    }

    public void semaforo65a()
    {
        TrafEntry entry = system.GetEntry(1039, 2);
        TrafEntry entry1 = system.GetEntry(1039, 8);
        TrafficLightContainer container = entry.light;
        TrafficLightContainer container1 = entry1.light;
        TrafficLight[] lights = container.gameObject.GetComponentsInParent<TrafficLight>();
        lights[0].StopAllCoroutines();
        lights[0].StartCoroutine(courutineSemaforoRosso(container));
        lights[0].StartCoroutine(courutineSemaforoVerde(container1));
    }

    private void semaforo13a()
    {
        TrafEntry entry = system.GetEntry(1012, 6);
        TrafEntry entry1 = system.GetEntry(1012, 0);
        TrafEntry entry2 = system.GetEntry(1012, 2);
        TrafficLightContainer container = entry.light;
        TrafficLightContainer container1 = entry1.light;
        TrafficLightContainer container2 = entry2.light;
        TrafficLight[] lights = container.gameObject.GetComponentsInParent<TrafficLight>();
        lights[0].StopAllCoroutines();
        lights[0].StartCoroutine(courutineSemaforoVerdeLungo(container));
        lights[0].StartCoroutine(courutineSemaforoRossoLungo(container1));
        lights[0].StartCoroutine(courutineSemaforoRossoLungo(container2));
    }

    TrafAIMotor macchinaTagliaStrada = null;

    public void semaforo13()
    {
        TrafEntry entry = system.GetEntry(1012, 6);
        TrafEntry entry1 = system.GetEntry(1012, 0);
        TrafEntry entry2 = system.GetEntry(1012, 2);
        TrafficLightContainer container = entry.light;
        TrafficLightContainer container1 = entry1.light;
        TrafficLightContainer container2 = entry2.light;
        TrafficLight[] lights = container.gameObject.GetComponentsInParent<TrafficLight>();
        lights[0].StopAllCoroutines();
        lights[0].StartCoroutine(courutineSemaforoVerde(container));
        lights[0].StartCoroutine(courutineSemaforoRosso(container1));
        lights[0].StartCoroutine(courutineSemaforoRosso(container2));


        //List<RoadGraphEdge> percorso13_0 = ottieniPercorso13_0();
        List<RoadGraphEdge> percorso13_1 = ottieniPercorso13_1();
        List<RoadGraphEdge> percorso13_2 = ottieniPercorso13_2();
        List<RoadGraphEdge> percorso13_3 = ottieniPercorso13_3();
        List<RoadGraphEdge> percorso13_4 = ottieniPercorso13_4();
        List<RoadGraphEdge> percorso13_5 = ottieniPercorso13_5();
        //CreaMacchinaTraffico(11, 2, 0.3f, percorso13_0);
        CreaMacchinaTraffico(11, 2, 0.6f, percorso13_1);
        macchinaTagliaStrada = CreaMacchinaTraffico(12, 0, 0.3f, percorso13_2);
        //CreaMacchinaTraffico(11, 3, 0.7f, percorso13_3);
        CreaMacchinaTrafficoNoRaycast(11, 3, 0.95f, percorso13_3).autoScorretta = true;
        CreaMacchinaTraffico(12, 1, 0.6f, percorso13_4);
        CreaMacchinaTraffico(12, 0, 0.8f, percorso13_5);
    }



    public void semaforo14a()
    {
        //faccio diventare rosso quello di prima, in modo che le auto ferme all'incrocio prendono il verde e vadano subito
        TrafEntry entry3 = system.GetEntry(1012, 6);
        TrafEntry entry4 = system.GetEntry(1012, 0);
        TrafEntry entry5 = system.GetEntry(1012, 2);
        TrafficLightContainer container3 = entry3.light;
        TrafficLightContainer container4 = entry4.light;
        TrafficLightContainer container5 = entry5.light;
        TrafficLight[] lights1 = container3.gameObject.GetComponentsInParent<TrafficLight>();
        lights1[0].StopAllCoroutines();
        lights1[0].StartCoroutine(courutineSemaforoRossoLungo(container3));
        lights1[0].StartCoroutine(courutineSemaforoVerdeLungo(container4));
        lights1[0].StartCoroutine(courutineSemaforoVerdeLungo(container5));



        TrafEntry entry = system.GetEntry(1048, 2);
        TrafEntry entry1 = system.GetEntry(1048, 4);
        TrafficLightContainer container = entry.light;
        TrafficLightContainer container1 = entry1.light;
        TrafficLight[] lights = container.gameObject.GetComponentsInParent<TrafficLight>();
        lights[0].StopAllCoroutines();
        lights[0].StartCoroutine(courutineSemaforoRosso(container));
        lights[0].StartCoroutine(courutineSemaforoVerde(container1));
    }

    public void semaforo15a()
    {
        TrafEntry entry = system.GetEntry(1031, 10);
        TrafEntry entry1 = system.GetEntry(1031, 0);
        TrafficLightContainer container = entry.light;
        TrafficLightContainer container1 = entry1.light;
        TrafficLight[] lights = container.gameObject.GetComponentsInParent<TrafficLight>();
        lights[0].StopAllCoroutines();
        lights[0].StartCoroutine(courutineSemaforoVerde(container));
        lights[0].StartCoroutine(courutineSemaforoRosso(container1));
    }

    public void semaforo15()
    {
        semaforo15a();  
    }

    public void semaforo16()
    {
        car.GetComponent<TrafAIMotor>().maxSpeed = limiteVelocita;
        soloSemaforo16();
        evento16a();
        //if (macchinaTraffico != null)
        //{
        //    macchinaTraffico.maxSpeed = limiteVelocita;
        //}
        
    }

    private void soloSemaforo16()
    {
        TrafEntry entry = system.GetEntry(1049, 5);
        TrafEntry entry1 = system.GetEntry(1049, 0);
        TrafficLightContainer container = entry.light;
        TrafficLightContainer container1 = entry1.light;
        TrafficLight[] lights = container.gameObject.GetComponentsInParent<TrafficLight>();
        lights[0].StopAllCoroutines();
        lights[0].StartCoroutine(courutineSemaforoRosso(container));
        lights[0].StartCoroutine(courutineSemaforoVerde(container1));
    }

    public void semaforo16a()
    {
        soloSemaforo16();
        evento16a();
    }

    public void semaforo18()
    {
        /*TrafEntry entry = system.GetEntry(1049, 5);
        TrafEntry entry1 = system.GetEntry(1049, 0);
        TrafficLightContainer container = entry.light;
        TrafficLightContainer container1 = entry1.light;
        TrafficLight[] lights = container.gameObject.GetComponentsInParent<TrafficLight>();
        lights[0].StopAllCoroutines();
        lights[0].StartCoroutine(courutineSemaforoRosso(container));
        lights[0].StartCoroutine(courutineSemaforoVerde(container1));*/
        TrafAIMotor motor = ottieniRiferimentoPlayer().GetComponent<TrafAIMotor>();
        if (motor.currentEntry.subIdentifier != 0)
        {
            motor.currentEntry.subIdentifier = 0;
            motor.maxSpeed = 3f;
        }

        
    }

    public void semaforo5()
    {
        TrafEntry entry = system.GetEntry(1003, 7);
        TrafEntry entry1 = system.GetEntry(1003, 8);
        TrafficLightContainer container = entry.light;
        TrafficLightContainer container1 = entry1.light;
        TrafficLight[] lights = container.gameObject.GetComponentsInParent<TrafficLight>();
        lights[0].StopAllCoroutines();
        lights[0].StartCoroutine(courutineSemaforoRosso(container));
        lights[0].StartCoroutine(courutineSemaforoVerde(container1));

        //TrackController.Instance.TriggerObstacle1();

        TrafEntry entry2 = system.GetEntry(1031, 3);
        TrafEntry entry3 = system.GetEntry(1031, 8);
        TrafficLightContainer container2 = entry2.light;
        TrafficLightContainer container3 = entry3.light;
        TrafficLight[] lights1 = container2.gameObject.GetComponentsInParent<TrafficLight>();
        lights1[0].StopAllCoroutines();
        lights1[0].StartCoroutine(courutineSemaforoVerde(container2));
        lights1[0].StartCoroutine(courutineSemaforoRosso(container3));
    }

    public void semaforo5a()
    {
        TrafEntry entry = system.GetEntry(1003, 7);
        TrafEntry entry1 = system.GetEntry(1003, 8);
        TrafficLightContainer container = entry.light;
        TrafficLightContainer container1 = entry1.light;
        TrafficLight[] lights = container.gameObject.GetComponentsInParent<TrafficLight>();
        lights[0].StopAllCoroutines();
        lights[0].StartCoroutine(courutineSemaforoVerdeLungo(container));
        lights[0].StartCoroutine(courutineSemaforoRossoLungo(container1));
    }

    public void semaforo3a()
    {
        TrafEntry entry = system.GetEntry(1031, 3);
        TrafEntry entry1 = system.GetEntry(1031, 8);
        TrafficLightContainer container = entry.light;
        TrafficLightContainer container1 = entry1.light;
        TrafficLight[] lights = container.gameObject.GetComponentsInParent<TrafficLight>();
        lights[0].StopAllCoroutines();
        lights[0].StartCoroutine(courutineSemaforoRosso(container));
        lights[0].StartCoroutine(courutineSemaforoVerde(container1));
    }

    public void semaforo4a()
    {
        TrafEntry entry = system.GetEntry(1014, 7);
        TrafEntry entry1 = system.GetEntry(1014, 0);
        TrafEntry entry2 = system.GetEntry(1014, 2);
        TrafficLightContainer container = entry.light;
        TrafficLightContainer container1 = entry1.light;
        TrafficLightContainer container2 = entry2.light;
        TrafficLight[] lights = container.gameObject.GetComponentsInParent<TrafficLight>();
        lights[0].StopAllCoroutines();
        lights[0].StartCoroutine(courutineSemaforoVerde(container));
        lights[0].StartCoroutine(courutineSemaforoRosso(container1));
        lights[0].StartCoroutine(courutineSemaforoRosso(container2));
    }

    public void semaforo4b()
    {
        TrafEntry entry = system.GetEntry(1014, 7);
        TrafEntry entry1 = system.GetEntry(1014, 0);
        TrafEntry entry2 = system.GetEntry(1014, 2);
        TrafficLightContainer container = entry.light;
        TrafficLightContainer container1 = entry1.light;
        TrafficLightContainer container2 = entry2.light;
        TrafficLight[] lights = container.gameObject.GetComponentsInParent<TrafficLight>();
        lights[0].StopAllCoroutines();
        lights[0].StartCoroutine(courutineSemaforoRosso(container));
        lights[0].StartCoroutine(courutineSemaforoVerde(container1));
        lights[0].StartCoroutine(courutineSemaforoVerde(container2));
    }

    public void semaforo4()
    {
        semaforo4a();
    }

    public void semaforo125a()
    {
        TrafEntry entry = system.GetEntry(1008, 4);
        TrafEntry entry1 = system.GetEntry(1008, 0);
        TrafEntry entry2 = system.GetEntry(1008, 2);
        TrafficLightContainer container = entry.light;
        TrafficLightContainer container1 = entry1.light;
        TrafficLightContainer container2 = entry2.light;
        TrafficLight[] lights = container.gameObject.GetComponentsInParent<TrafficLight>();
        lights[0].StopAllCoroutines();
        lights[0].StartCoroutine(courutineSemaforoRosso(container));
        lights[0].StartCoroutine(courutineSemaforoVerde(container1));
        lights[0].StartCoroutine(courutineSemaforoVerde(container2));
    }

    public void semaforo125()
    {
        //semaforo125a();
        semaforo159a();
        /*List<Vector3> listaWaypoint = system.GetEntry(1008, 9).waypoints;
        Vector3 wp3Ok = listaWaypoint[3];
        wp3Ok.x = 1162.84f;
        wp3Ok.z = 950.3f;
        listaWaypoint[3] = wp3Ok;

        ottieniRiferimentoPlayer().GetComponent<TrafAIMotor>().currentEntry.waypoints = listaWaypoint;*/
    }

    public void semaforo159a()
    {
        TrafEntry entry = system.GetEntry(1011, 8);
        TrafEntry entry1 = system.GetEntry(1011, 2);
        TrafEntry entry2 = system.GetEntry(1011, 4);
        TrafficLightContainer container = entry.light;
        TrafficLightContainer container1 = entry1.light;
        TrafficLightContainer container2 = entry2.light;
        TrafficLight[] lights = container.gameObject.GetComponentsInParent<TrafficLight>();
        lights[0].StopAllCoroutines();
        lights[0].StartCoroutine(courutineSemaforoRosso(container));
        lights[0].StartCoroutine(courutineSemaforoRosso(container1));
        lights[0].StartCoroutine(courutineSemaforoVerde(container2));
    }

    public void semaforo159b()
    {
        TrafEntry entry = system.GetEntry(1011, 8);
        TrafEntry entry1 = system.GetEntry(1011, 2);
        TrafEntry entry2 = system.GetEntry(1011, 4);
        TrafficLightContainer container = entry.light;
        TrafficLightContainer container1 = entry1.light;
        TrafficLightContainer container2 = entry2.light;
        TrafficLight[] lights = container.gameObject.GetComponentsInParent<TrafficLight>();
        lights[0].StopAllCoroutines();
        lights[0].StartCoroutine(courutineSemaforoVerde(container));
        lights[0].StartCoroutine(courutineSemaforoVerde(container1));
        lights[0].StartCoroutine(courutineSemaforoRosso(container2));
    }

    public void semaforo159()
    {
        _pidPars.velocitaFrenata = 2.8f;
        semaforo159b();

        List<RoadGraphEdge> percorso159_5 = ottieniPercorso159_5();
        List<RoadGraphEdge> percorso159_0 = ottieniPercorso159_0();

        CreaMacchinaTraffico(158, 2, 0f, percorso159_0);

        //AutoTrafficoNoRayCast macchinaTrafficoScorretta = CreaMacchinaTrafficoNoRaycast(162, 0, 0.97f, percorso159_5); //macchinaTraffico che non rispetta il semaforo e va piu veloce
        //macchinaTrafficoScorretta.maxSpeed = 15f;
        //AutoTrafficoNoRayCast macchinaTrafficoScorretta = CreaMacchinaTrafficoNoRaycast(162, 0, 0.57f, percorso159_5); //macchinaTraffico che non rispetta il semaforo e va piu veloce
        //macchinaTrafficoScorretta = CreaMacchinaTrafficoNoRaycast(162, 0, 0.7f, percorso159_5); //macchinaTraffico che non rispetta il semaforo e va piu veloce
        macchinaTrafficoScorretta = CreaMacchinaTrafficoNoRaycast(162, 0, _pidPars.partenzaAuto, percorso159_5);


        macchinaTrafficoScorretta.autoScorretta = true;
        macchinaTrafficoScorretta.maxSpeed = 9f;
        SetLayer(12, macchinaTrafficoScorretta.transform);


        List<RoadGraphEdge> percorso159_4 = ottieniPercorso159_4();
        //macchinaTrafficoScorrettaDavanti = CreaMacchinaTraffico(162, 3, 0.807f, percorso159_4);
        //macchinaTrafficoScorrettaDavanti.autoScorretta = true;
        //macchinaTrafficoScorrettaDavanti.maxSpeed = 5.2f;

        //ottieniRiferimentoPlayer().GetComponent<TrafAIMotor>().maxSpeed = 10f;

        /*List<RoadGraphEdge> percorso159_1 = ottieniPercorso159_1();
        CreaMacchinaTraffico(158, 2, 0.9f, percorso159_1);*/





    }

    public void semaforo163()
    {
        TrafEntry entry = system.GetEntry(1017, 17);
        TrafEntry entry1 = system.GetEntry(1017, 0);
        TrafEntry entry2 = system.GetEntry(1017, 2);
        TrafficLightContainer container = entry.light;
        TrafficLightContainer container1 = entry1.light;
        TrafficLightContainer container2 = entry2.light;
        TrafficLight[] lights = container.gameObject.GetComponentsInParent<TrafficLight>();
        lights[0].StopAllCoroutines();
        lights[0].StartCoroutine(courutineSemaforoRosso(container));
        lights[0].StartCoroutine(courutineSemaforoVerde(container1));
        lights[0].StartCoroutine(courutineSemaforoVerde(container2));

        //macchinaTrafficoScorrettaDavanti.autoScorretta = false;
        
    }

    public void semaforo163a()
    {
        TrafEntry entry = system.GetEntry(1017, 17);
        TrafEntry entry1 = system.GetEntry(1017, 0);
        TrafEntry entry2 = system.GetEntry(1017, 2);
        TrafficLightContainer container = entry.light;
        TrafficLightContainer container1 = entry1.light;
        TrafficLightContainer container2 = entry2.light;
        TrafficLight[] lights = container.gameObject.GetComponentsInParent<TrafficLight>();
        lights[0].StopAllCoroutines();
        lights[0].StartCoroutine(courutineSemaforoRosso(container));
        lights[0].StartCoroutine(courutineSemaforoRosso(container1));
        lights[0].StartCoroutine(courutineSemaforoRosso(container2));

        //macchinaTrafficoScorrettaDavanti.autoScorretta = false;
    }


    public void semaforo30a()
    {
        TrafEntry entry = system.GetEntry(1052, 10);
        TrafEntry entry1 = system.GetEntry(1052, 0);
        TrafEntry entry2 = system.GetEntry(1052, 2);
        TrafficLightContainer container = entry.light;
        TrafficLightContainer container1 = entry1.light;
        TrafficLightContainer container2 = entry2.light;
        TrafficLight[] lights = container.gameObject.GetComponentsInParent<TrafficLight>();
        lights[0].StopAllCoroutines();
        lights[0].StartCoroutine(courutineSemaforoVerdeLungo(container));
        lights[0].StartCoroutine(courutineSemaforoRossoLungo(container1));
        lights[0].StartCoroutine(courutineSemaforoRossoLungo(container2));
    }

    public void semaforo30()
    {
        semaforo30a();
    }

    public void semaforo158()
    {
        TrafEntry entry = system.GetEntry(1011, 8);
        TrafEntry entry1 = system.GetEntry(1011, 2);
        TrafEntry entry2 = system.GetEntry(1011, 4);
        TrafficLightContainer container = entry.light;
        TrafficLightContainer container1 = entry1.light;
        TrafficLightContainer container2 = entry2.light;
        TrafficLight[] lights = container.gameObject.GetComponentsInParent<TrafficLight>();
        lights[0].StopAllCoroutines();
        lights[0].StartCoroutine(courutineSemaforoVerde(container));
        lights[0].StartCoroutine(courutineSemaforoVerde(container1));
        lights[0].StartCoroutine(courutineSemaforoRosso(container2));
    }

    public void semaforo159_2()
    {
        TrafEntry entry = system.GetEntry(1008, 4);
        TrafEntry entry1 = system.GetEntry(1008, 0);
        TrafEntry entry2 = system.GetEntry(1008, 2);
        TrafficLightContainer container = entry.light;
        TrafficLightContainer container1 = entry1.light;
        TrafficLightContainer container2 = entry2.light;
        TrafficLight[] lights = container.gameObject.GetComponentsInParent<TrafficLight>();
        lights[0].StopAllCoroutines();
        lights[0].StartCoroutine(courutineSemaforoRossoLungo(container));
        lights[0].StartCoroutine(courutineSemaforoVerdeLungo(container1));
        lights[0].StartCoroutine(courutineSemaforoVerdeLungo(container2));
    }


   /* public void evento1089()
    {
        List<Vector3> listaWaypoint = ottieniListaWaypoint();
        Vector3 wp3Ok = listaWaypoint[3];
        wp3Ok.x = 2223f;
        wp3Ok.z = 188.2f;
        listaWaypoint[3] = wp3Ok;

        ottieniRiferimentoPlayer().GetComponent<TrafAIMotor>().currentEntry.waypoints = listaWaypoint;

    }*/


    public void evento39()
    {
        List<RoadGraphEdge> percorso39_0 = ottieniPercorso39_0();
        List<RoadGraphEdge> percorso39_1 = ottieniPercorso39_1();
        CreaMacchinaTraffico(45, 1, 0, percorso39_0);
        CreaMacchinaTraffico(45, 0, 0.5f, percorso39_1);
        ottieniRiferimentoPlayer().GetComponent<TrafAIMotor>().sterzataMassima = true;
    }

    public void evento1082()
    {
        /*List<Vector3> listaWaypoint = ottieniListaWaypoint();
        for(int i = 0; i < listaWaypoint.Count; i++)
        {
            Vector3 wp = listaWaypoint[i];
            wp.z = 191f;
            listaWaypoint[i] = wp;
        }

        ottieniRiferimentoPlayer().GetComponent<TrafAIMotor>().currentEntry.waypoints = listaWaypoint;*/
    }

    public void evento38()
    {
        //Debug.Log("sono in evento38");
        List<RoadGraphEdge> percorso38_0 = ottieniPercorso38_0();
        List<RoadGraphEdge> percorso38_1 = ottieniPercorso38_1();
        List<RoadGraphEdge> percorso38_2 = ottieniPercorso38_2();
        CreaMacchinaTraffico(36, 0, 0.5f, percorso38_0);
        CreaMacchinaTraffico(49, 1, 0f, percorso38_1);
        CreaMacchinaTraffico(49, 0, 0.3f, percorso38_2);

        ottieniRiferimentoPlayer().GetComponent<TrafAIMotor>().sterzataMassima = false;
    }

    public void evento49()
    {
        List<RoadGraphEdge> percorso38_1 = ottieniPercorso38_1();
        List<RoadGraphEdge> percorso38_2 = ottieniPercorso38_2();
        CreaMacchinaTraffico(49, 1, 0f, percorso38_1);
        CreaMacchinaTraffico(49, 0, 0.1f, percorso38_2);

        List<RoadGraphEdge> percorso49_0 = ottieniPercorso49_0();
        List<RoadGraphEdge> percorso49_1 = ottieniPercorso49_1();
        CreaMacchinaTraffico(50, 0, 0.2f, percorso49_0);
        CreaMacchinaTraffico(50, 1, 0f, percorso49_1);


    }

    public void evento50()
    {
        semaforo50a();
        List<RoadGraphEdge> percorso49_0 = ottieniPercorso49_0();
        List<RoadGraphEdge> percorso49_1 = ottieniPercorso49_1();
        CreaMacchinaTraffico(50, 0, 0.2f, percorso49_0);
        CreaMacchinaTraffico(50, 1, 0f, percorso49_1);

        List<RoadGraphEdge> percorso50_0 = ottieniPercorso50_0();
        //List<RoadGraphEdge> percorso50_1 = ottieniPercorso50_1();
        List<RoadGraphEdge> percorso50_2 = ottieniPercorso50_2();
        List<RoadGraphEdge> percorso50_3 = ottieniPercorso50_3();
        CreaMacchinaTraffico(51, 0, 0.95f, percorso50_0);
        //CreaMacchinaTraffico(51, 1, 0.2f, percorso50_1);
        CreaMacchinaTraffico(62, 0, 0.4f, percorso50_2);
        CreaMacchinaTraffico(62, 0, 0f, percorso50_3);



    }

    public void evento63()
    {
        semaforo63a();
        List<RoadGraphEdge> percorso63_0 = ottieniPercorso63_0();
        List<RoadGraphEdge> percorso63_1 = ottieniPercorso63_1();
        List<RoadGraphEdge> percorso63_2 = ottieniPercorso63_2();
        CreaMacchinaTraffico(52, 0, 0.5f, percorso63_0);
        CreaMacchinaTraffico(52, 1, 0.2f, percorso63_1);
        CreaMacchinaTraffico(52, 0, 0f, percorso63_0);
        CreaMacchinaTraffico(52, 1, 0f, percorso63_1);
        CreaMacchinaTraffico(52, 3, 0.7f, percorso63_2);
    }

    public void evento1036()
    {
        //List<Vector3> listaWaypoint = ottieniListaWaypoint();
        /*for (int i = 0; i < listaWaypoint.Count; i++)
        {
            Vector3 wp = listaWaypoint[i];
            wp.z = 191f;
            listaWaypoint[i] = wp;
        }

        ottieniRiferimentoPlayer().GetComponent<TrafAIMotor>().currentEntry.waypoints = listaWaypoint;*/
    }

    public void evento64()
    {
        List<Vector3> listaWaypoint = ottieniListaWaypoint();
        Vector3 wp = listaWaypoint[0];
        wp.z = 572f;
        listaWaypoint[0] = wp;


        semaforo64a();
        List<RoadGraphEdge> percorso64_0 = ottieniPercorso64_0();
        List<RoadGraphEdge> percorso64_1 = ottieniPercorso64_1();
        List<RoadGraphEdge> percorso64_2 = ottieniPercorso64_2();
        List<RoadGraphEdge> percorso64_3 = ottieniPercorso64_3();
        CreaMacchinaTraffico(54, 0, 0.8f, percorso64_0);
        CreaMacchinaTraffico(54, 1, 0.7f, percorso64_1);
        CreaMacchinaTraffico(54, 2, 0.6f, percorso64_2);
        //CreaMacchinaTraffico(65, 3, 0f, percorso64_3);
    }

    public void evento65()
    {
        List<Vector3> listaWaypoint = ottieniListaWaypoint();
        Vector3 wp = listaWaypoint[0];
        wp.z = 572f;
        listaWaypoint[0] = wp;


        semaforo65a();
        List<RoadGraphEdge> percorso65_0 = ottieniPercorso65_0();
        List<RoadGraphEdge> percorso65_1 = ottieniPercorso65_1();
        List<RoadGraphEdge> percorso65_2 = ottieniPercorso65_2();
        List<RoadGraphEdge> percorso65_3 = ottieniPercorso65_3();
        List<RoadGraphEdge> percorso65_4 = ottieniPercorso65_4();
        macchinaTraffico = CreaMacchinaTrafficoInchiodata(25, 3, 0.3f, percorso65_0);
        macchinaTraffico.maxSpeed = limiteVelocita;
        TrafAIMotor macchinaLenta = CreaMacchinaTraffico(25, 2, 0.75f, percorso65_1);
        macchinaLenta.maxSpeed = 3f;
        CreaMacchinaTraffico(25, 1, 0.4f, percorso65_2);
        CreaMacchinaTraffico(25, 0, 0.6f, percorso65_3);
        CreaMacchinaTraffico(25, 3, 0f, percorso65_4);
        CreaMacchinaTraffico(25, 3, 0.8f, percorso65_4);
    }

    public void evento13()
    {
        semaforo13a();
        macchinaTraffico.maxSpeed = 4f;
        TrafEntry entry = system.GetEntry(1049, 5);
        TrafficLightContainer container = entry.light;
        TrafficLight[] lights = container.gameObject.GetComponentsInParent<TrafficLight>();
        lights[0].StartCoroutine(attesa13());
    }

    public IEnumerator attesa13()
    {
        yield return new WaitForSeconds(12f);
        macchinaTraffico.maxSpeed = limiteVelocita;
    }


    public void evento14()
    {
        semaforo14a();
        //List<RoadGraphEdge> percorso14_0 = ottieniPercorso14_0();
        //List<RoadGraphEdge> percorso14_1 = ottieniPercorso14_1();
        //List<RoadGraphEdge> percorso14_2 = ottieniPercorso14_2();
        ///*TrafAIMotor macchinaPiuLenta = CreaMacchinaTraffico(20, 3, 0.5f, percorso14_0);
        //macchinaPiuLenta.maxSpeed = 8f;*/
        //CreaMacchinaTraffico(20, 2, 0.6f, percorso14_1);
        //CreaMacchinaTraffico(20, 1, 0.8f, percorso14_2);


        List<RoadGraphEdge> percorso13_1 = ottieniPercorso13_1();
        TrafAIMotor autoLato = CreaMacchinaTraffico(11, 2, 0.45f, percorso13_1);
        autoLato.maxSpeed = 12f;
    }

    public void evento15()
    {
        semaforo15a();
        if (macchinaTraffico != null)
        {
            macchinaTraffico.maxSpeed = 11f;
        }
        
        List<RoadGraphEdge> percorso15_0 = ottieniPercorso15_0();
        List<RoadGraphEdge> percorso15_1 = ottieniPercorso15_1();
        CreaMacchinaTraffico(3, 3, 0.4f, percorso15_0);
        //CreaMacchinaTraffico(3, 0, 0.6f, percorso15_1); //QUESTA SCOMPARE!!! ATTENZIONE


        eventoTaglioStrada();
        

    }

    private void eventoTaglioStrada()
    {
        
        if (macchinaTagliaStrada == null)
        {
            return;
        }
        if (macchinaTagliaStrada.currentEntry.identifier != 15)
        {
            return;
        }

        if (Vector3.Distance(macchinaTagliaStrada.transform.position, macchinaTagliaStrada.currentEntry.waypoints[macchinaTagliaStrada.currentEntry.waypoints.Count - 1]) < 20)
        {
            macchinaTagliaStrada.maxSpeed = 0f;
            return;
        }

        if (Vector3.Distance(car.transform.position, car.GetComponent<TrafAIMotor>().currentEntry.waypoints[car.GetComponent<TrafAIMotor>().currentEntry.waypoints.Count -1]) < 50)
        {
            float xMacchinaTagliaStrada = macchinaTagliaStrada.transform.position.x;
            float miaX = car.transform.position.x;
            float xMacchinaDavanti = macchinaTraffico.transform.position.x;
            if ((miaX - xMacchinaTagliaStrada) > 5f && (xMacchinaTagliaStrada - xMacchinaDavanti) > 5f)
            {
                //la macchina che deve tagliarmi la strada deve essere pu avanti rispetto a me ma piu indietro rispetto alla macchina che ho davanti
                //se si trova in queste condizioni mi tagli la strada altrimenti si ferma per evitare il blocco presente nella strada

                macchinaTagliaStrada.maxSpeed = 5f;
                tagliaStrada();
                //TrafEntry entry = system.GetEntry(1049, 5);
                //TrafficLightContainer container = entry.light;
                //TrafficLight[] lights = container.gameObject.GetComponentsInParent<TrafficLight>();
                //lights[0].StartCoroutine(attesa15_2());
                SetLayer(12, macchinaTagliaStrada.transform);
                StartCoroutine(CourutineCambioLayer());
                return;
            }
            macchinaTagliaStrada.maxSpeed = 0f; //non puo tagliarmi la strada; si ferma
            return;
        }

        float distanza = macchinaTagliaStrada.transform.position.x - car.transform.position.x;
        //Debug.Log("distanza mia macchina e macchinatagliastrada " + distanza);
        //if (distanza < -30)
        if (distanza < -30)
        {
            macchinaTagliaStrada.maxSpeed = Mathf.Clamp(macchinaTagliaStrada.maxSpeed -1, 3f, limiteVelocita);
            macchinaTagliaStrada.forzaLuceStop = true;
            TrafEntry entry = system.GetEntry(1049, 5);
            TrafficLightContainer container = entry.light;
            TrafficLight[] lights = container.gameObject.GetComponentsInParent<TrafficLight>();
            lights[0].StartCoroutine(attesa15());
            //attesa15();
            return;
        }
        //if (distanza > -5)
        if (distanza > -13)
        {
            //macchinaTagliaStrada.maxSpeed = Mathf.Clamp(macchinaTagliaStrada.maxSpeed + 1, 3f, limiteVelocita);
            macchinaTagliaStrada.maxSpeed = limiteVelocita;
            macchinaTagliaStrada.forzaLuceStop = false;
            TrafEntry entry = system.GetEntry(1049, 5);
            TrafficLightContainer container = entry.light;
            TrafficLight[] lights = container.gameObject.GetComponentsInParent<TrafficLight>();
            lights[0].StartCoroutine(attesa15());
            return;
        }
        macchinaTagliaStrada.maxSpeed = macchinaTraffico.maxSpeed;
        //if (Vector3.Distance(car.transform.position, car.GetComponent<TrafAIMotor>().target) < 65)
        

        

        //macchinaTagliaStrada.maxSpeed++;
        TrafEntry entry1 = system.GetEntry(1049, 5);
        TrafficLightContainer container1 = entry1.light;
        TrafficLight[] lights1 = container1.gameObject.GetComponentsInParent<TrafficLight>();
        lights1[0].StartCoroutine(attesa15());
    }

    public IEnumerator CourutineCambioLayer()
    {
        //if (Vector3.Distance(car.transform.position, car.GetComponent<TrafAIMotor>().target) > 15)
        if (macchinaTagliaStrada.GetComponent<TrafAIMotor>().currentEntry.identifier != 1031)
        {
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(CourutineCambioLayer());
        } else
        {
            if (car.GetComponent<TrafAIMotor>().evitare == false)
            {
                //se non è false la macchina rimarrebbe piantata perchè la macchina del traffico non uscirebbe mai dalla collisione
                SetLayer(8, macchinaTagliaStrada.transform);
            }
            
        }
        



    }

    public IEnumerator attesa15()
    {
        yield return new WaitForSeconds(0.5f);
        eventoTaglioStrada();

    }

    public IEnumerator attesa15_2()
    {
        if (Vector3.Distance(car.transform.position, macchinaTagliaStrada.transform.position) > 12f)
        {         
            yield return new WaitForSeconds(0.3f);
        }       
        SetLayer(12, macchinaTagliaStrada.transform);
    }
    

    private void tagliaStrada()
    {
        //la macchina mi taglia la strada
        TrafEntry entry2 = system.GetEntry(15, 2);
        TrafEntry entry3 = system.GetEntry(15, 3);
        entry3.waypoints[1] = entry2.waypoints[1];
        macchinaTagliaStrada.target = entry2.waypoints[1];
        macchinaTagliaStrada.currentEntry = entry3;

        Debug.DrawLine(macchinaTagliaStrada.transform.position, entry3.waypoints[1]);
    }


    public void evento1031()
    {
        SetLayer(8, macchinaTagliaStrada.transform);
    }


    public void evento16()
    {
        semaforo16a();


        if (macchinaTagliaStrada.maxSpeed == 0)
        {
            //la macchina non mi ha tagliato la strada ma è ferma; gli faccio evitare il blocco di cemento e la faccio proseguire
            macchinaTagliaStrada.maxSpeed = 7f;
            tagliaStrada();           
        } else
        {
            //mi ha tagliato la strada, risetto il livello a Traffic
            SetLayer(8, macchinaTagliaStrada.transform);
        }
        GameObject.Destroy(GameObject.Find("Barriera"));
        car.GetComponent<TrafAIMotor>().maxSpeed = 11f;
    }

    public void evento16a()
    {
        List<RoadGraphEdge> percorso16_0 = ottieniPercorso16_0();
        List<RoadGraphEdge> percorso16_1 = ottieniPercorso16_1();
        //List<RoadGraphEdge> percorso16_2 = ottieniPercorso16_2();
        //List<RoadGraphEdge> percorso16_3 = ottieniPercorso16_3();
        List<RoadGraphEdge> percorso16_4 = ottieniPercorso16_4();
        CreaMacchinaTraffico(17, 1, 0.4f, percorso16_0);
        CreaMacchinaTraffico(17, 3, 0.2f, percorso16_1);
        //CreaMacchinaTraffico(17, 0, 0f, percorso16_2);
        //CreaMacchinaTraffico(17, 0, 0.1f, percorso16_3);
        CreaMacchinaTraffico(17, 0, 0.5f, percorso16_4);
    }

    public IEnumerator attesa18()
    {
        yield return new WaitForSeconds(1f);
        try
        {
            if (macchinaTraffico.currentEntry.identifier == 18)
            {
                macchinaTraffico.maxSpeed = 2f;
            } else
            {
                ottieniRiferimentoPlayer().GetComponent<TrafAIMotor>().maxSpeed = 2f;
            }                     
        } catch (Exception e)
        {
            //c'è stato qualche problema con macchinaTraffico
            ottieniRiferimentoPlayer().GetComponent<TrafAIMotor>().maxSpeed = 2f;
        }
        
    }

    TrafAIMotor macchinaAccodarmi = null;
    public void evento18()
    {
        //semaforo18a();
        

        soloSemaforo16();

        List<RoadGraphEdge> percorso18_0 = ottieniPercorso18_0();
        List<RoadGraphEdge> percorso18_1 = ottieniPercorso18_1();
        //CreaMacchinaTraffico(17, 0, 0.5f, percorso18_0);
        macchinaAccodarmi = CreaMacchinaTraffico(17, 0, 0.99f, percorso18_1);


        TrafEntry entry = system.GetEntry(1049, 5);
        TrafficLightContainer container = entry.light;
        TrafficLight[] lights = container.gameObject.GetComponentsInParent<TrafficLight>();
        lights[0].StartCoroutine(attesa18());

        evento18a();

        

    }

    public IEnumerator attesa18_2()
    {
        yield return new WaitForSeconds(0.5f);
        evento18a();
    }

    public void evento18a()
    {
        GameObject go = ottieniRiferimentoPlayer();
        try
        { 
            if (macchinaAccodarmi.transform.position.z > go.transform.position.z)
            {
                TrafficLightContainer container = system.GetEntry(1049, 5).light;
                TrafficLight[] lights = container.gameObject.GetComponentsInParent<TrafficLight>();
                lights[0].StartCoroutine(attesa18_2());
                return;
            }
        } catch (Exception e)
        {
            Debug.Log("eccezione macchinaAccodarmi: " + e);
        }


        go.GetComponent<TrafAIMotor>().maxSpeed = 11f;


        TrafEntry entry = system.GetEntry(18, 0);
        Vector3 puntoArrivo = go.GetComponent<TrafAIMotor>().target;
        puntoArrivo.x = 865.9f;
        entry.waypoints[go.GetComponent<TrafAIMotor>().currentIndex] = puntoArrivo;
        go.GetComponent<TrafAIMotor>().target = puntoArrivo;
        Debug.DrawLine(go.transform.position, entry.waypoints[go.GetComponent<TrafAIMotor>().currentIndex]);

        go.GetComponent<TrafAIMotor>().currentEntry = entry;
        go.GetComponent<TrafAIMotor>().sterzataMassima = true;
    }

    /*public void evento1007()
    {
        List<Vector3> listaWaypoint = ottieniListaWaypoint();
        Vector3 wp3Ok = listaWaypoint[3];
        wp3Ok.x = 866.41f;
        wp3Ok.z = 380f;
        listaWaypoint[3] = wp3Ok;

        ottieniRiferimentoPlayer().GetComponent<TrafAIMotor>().currentEntry.waypoints = listaWaypoint;
    }*/

    public void evento5()
    {
        ottieniRiferimentoPlayer().GetComponent<TrafAIMotor>().maxSpeed = limiteVelocita;

        semaforo5a();
        List<RoadGraphEdge> percorso5_0 = ottieniPercorso5_0();
        List<RoadGraphEdge> percorso5_1 = ottieniPercorso5_1();
        List<RoadGraphEdge> percorso5_2 = ottieniPercorso5_2();
        List<RoadGraphEdge> percorso5_3 = ottieniPercorso5_3();
        List<RoadGraphEdge> percorso5_4 = ottieniPercorso5_4();
        List<RoadGraphEdge> percorso5_5 = ottieniPercorso5_5();
        List<RoadGraphEdge> percorso5_6 = ottieniPercorso5_6();
        List<RoadGraphEdge> percorso5_7 = ottieniPercorso5_7();
        CreaMacchinaTraffico(2, 0, 0f, percorso5_0);
        CreaMacchinaTraffico(2, 0, 0.2f, percorso5_1);
        CreaMacchinaTraffico(2, 1, 0.3f, percorso5_2);
        macchinaTraffico = CreaMacchinaTraffico(2, 2, 0.15f, percorso5_3);
        //CreaMacchinaTraffico(2, 3, 0.5f, percorso5_4);
        CreaMacchinaTraffico(2, 3, 0.35f, percorso5_5);
        CreaMacchinaTraffico(2, 3, 0f, percorso5_6);
        CreaMacchinaTraffico(2, 3, 0.6f, percorso5_7);
    }

    public void evento3()
    {
        semaforo3a();
        List<RoadGraphEdge> percorso3_0 = ottieniPercorso3_0();
        List<RoadGraphEdge> percorso3_1 = ottieniPercorso3_1();
        List<RoadGraphEdge> percorso3_2 = ottieniPercorso3_2();
        List<RoadGraphEdge> percorso3_3 = ottieniPercorso3_3();
        List<RoadGraphEdge> percorso3_4 = ottieniPercorso3_4();
        List<RoadGraphEdge> percorso3_5 = ottieniPercorso3_5();
        List<RoadGraphEdge> percorso3_6 = ottieniPercorso3_6();
        CreaMacchinaTraffico(15, 3, 0f, percorso3_0);
        //CreaMacchinaTraffico(15, 3, 0.15f, percorso3_1);
        //CreaMacchinaTraffico(15, 3, 0.3f, percorso3_2);
        // CreaMacchinaTraffico(15, 3, 0.45f, percorso3_3);
        CreaMacchinaTraffico(15, 2, 0.6f, percorso3_5);
        CreaMacchinaTraffico(15, 1, 0.8f, percorso3_4);
        CreaMacchinaTraffico(15, 0, 0.2f, percorso3_6);

        CreaMacchinaTraffico(15, 2, 0.15f, percorso3_5);
        CreaMacchinaTraffico(15, 1, 0.4f, percorso3_4);
        CreaMacchinaTraffico(15, 0, 0.1f, percorso3_6);

        

    }

    public void evento4()
    {
        semaforo4a();
        List<RoadGraphEdge> percorso4_0 = ottieniPercorso4_0();
        List<RoadGraphEdge> percorso4_1 = ottieniPercorso4_1();
        List<RoadGraphEdge> percorso4_2 = ottieniPercorso4_2();
        List<RoadGraphEdge> percorso4_3 = ottieniPercorso4_3();
        List<RoadGraphEdge> percorso4_4 = ottieniPercorso4_4();
        List<RoadGraphEdge> percorso4_5 = ottieniPercorso4_5();
        List<RoadGraphEdge> percorso4_6 = ottieniPercorso4_6();
        List<RoadGraphEdge> percorso4_7 = ottieniPercorso4_7();

        CreaMacchinaTraffico(24, 1, 0f, percorso4_0);
        CreaMacchinaTraffico(24, 0, 0.15f, percorso4_1);
        CreaMacchinaTraffico(24, 1, 0.3f, percorso4_0);
        CreaMacchinaTraffico(24, 0, 0.45f, percorso4_1);
        CreaMacchinaTraffico(24, 1, 0.6f, percorso4_0);
        CreaMacchinaTraffico(24, 0, 0.75f, percorso4_1);

        CreaMacchinaTraffico(23, 2, 0f, percorso4_2);
        CreaMacchinaTraffico(23, 3, 0.15f, percorso4_3);

        CreaMacchinaTraffico(23, 3, 0.87f, percorso4_4);
        CreaMacchinaTraffico(23, 3, 0.95f, percorso4_7);

        CreaMacchinaTraffico(23, 3, 0.5f, percorso4_5);
        CreaMacchinaTraffico(23, 3, 0.65f, percorso4_6);

        CreaMacchinaTraffico(23, 2, 0.3f, percorso4_2);


        List<RoadGraphEdge> percorsoMacchinaTagliaStrada4 = ottieniPercorsoMacchinTagliaStrada4();
        scooterTagliaStrada = CreaScooter(1031, 2, 0.5f, percorsoMacchinaTagliaStrada4);
        scooterTagliaStrada.noRaycast = true;
        scooterTagliaStrada.GetComponent<AudioSource>().mute = true;
        //SetLayer(8, scooterTagliaStrada.transform);
        eventoTaglioStrada2();
    }

    TrafAIMotor scooterTagliaStrada = null;

    private void eventoTaglioStrada2()
    {

        if (scooterTagliaStrada == null)
        {
            return;
        }
        if (scooterTagliaStrada.currentEntry.identifier != 4 && scooterTagliaStrada.currentEntry.identifier != 1031)
        {
            return;
        }

        float distanza = Mathf.Abs(scooterTagliaStrada.transform.position.z - car.transform.position.z);
        //Debug.Log("distanza mia macchina e macchinatagliastrada " + distanza);
        if (distanza > 40)
        {
            
            scooterTagliaStrada.maxSpeed = Mathf.Clamp(scooterTagliaStrada.maxSpeed + 1, 3f, limiteVelocita);
            TrafEntry entry = system.GetEntry(1049, 5);
            TrafficLightContainer container = entry.light;
            TrafficLight[] lights = container.gameObject.GetComponentsInParent<TrafficLight>();
            lights[0].StartCoroutine(attesa4());
            //attesa15();
            return;
        }

        

        /*if (distanza < 5)
        {
            macchinaTagliaStrada2.maxSpeed = Mathf.Clamp(macchinaTagliaStrada2.maxSpeed - 1, 3f, limiteVelocita);
            TrafEntry entry = system.GetEntry(1049, 5);
            TrafficLightContainer container = entry.light;
            TrafficLight[] lights = container.gameObject.GetComponentsInParent<TrafficLight>();
            lights[0].StartCoroutine(attesa4());
            return;
        }*/
        scooterTagliaStrada.maxSpeed = 11f;
        /*if (Vector3.Distance(car.transform.position, car.GetComponent<TrafAIMotor>().target) < 65)
        {
            macchinaTagliaStrada2.maxSpeed = 7f;
            tagliaStrada2();
            return;
        }*/
        if (scooterTagliaStrada.currentEntry.identifier != 4)
        {
            TrafEntry entry = system.GetEntry(1049, 5);
            TrafficLightContainer container = entry.light;
            TrafficLight[] lights = container.gameObject.GetComponentsInParent<TrafficLight>();
            lights[0].StartCoroutine(attesa4());
            return;
        }

        if (scooterTagliaStrada.GetComponent<AudioSource>().mute == true && Vector3.Distance(scooterTagliaStrada.transform.position, scooterTagliaStrada.currentEntry.waypoints[scooterTagliaStrada.currentEntry.waypoints.Count - 1]) < 50f)
        {
            scooterTagliaStrada.GetComponent<AudioSource>().mute = false;
        }

        //Debug.Log("Distanza dal waypoint: " + Vector3.Distance(scooterTagliaStrada.transform.position, scooterTagliaStrada.currentEntry.waypoints[scooterTagliaStrada.currentEntry.waypoints.Count - 1]));
        if (Vector3.Distance(scooterTagliaStrada.transform.position, scooterTagliaStrada.currentEntry.waypoints[scooterTagliaStrada.currentEntry.waypoints.Count - 1]) < 15f) 
        {
            //Debug.Log("cambio corsia");
            TrafEntry entry = system.GetEntry(125, 0);
            scooterTagliaStrada.currentEntry = entry;
            scooterTagliaStrada.nextEntry = null;
            scooterTagliaStrada.hasNextEntry = false;
            scooterTagliaStrada.target = entry.waypoints[0];
            return;
        }

        TrafEntry entry1 = system.GetEntry(1049, 5);
        TrafficLightContainer container1 = entry1.light;
        TrafficLight[] lights1 = container1.gameObject.GetComponentsInParent<TrafficLight>();
        lights1[0].StartCoroutine(attesa4());
    }

    public IEnumerator attesa4()
    {
        yield return new WaitForSeconds(0.5f);
        eventoTaglioStrada2();

    }

    /*private void tagliaStrada2()
    {
        //la macchina mi taglia la strada
        TrafEntry entry2 = system.GetEntry(15, 2);
        TrafEntry entry3 = system.GetEntry(15, 3);
        entry3.waypoints[1] = entry2.waypoints[1];
        macchinaTagliaStrada.target = entry2.waypoints[1];
        macchinaTagliaStrada.currentEntry = entry3;

        Debug.DrawLine(macchinaTagliaStrada.transform.position, entry3.waypoints[1]);
    }*/

    public void evento125()
    {
        macchinaTraffico.autoScorretta = true;
        scooterTagliaStrada.GetComponent<AudioSource>().volume = 0.1f;
        semaforo4b();
        semaforo125a();
        List<RoadGraphEdge> percorso125_0 = ottieniPercorso125_0();
        List<RoadGraphEdge> percorso125_1 = ottieniPercorso125_1();
        List<RoadGraphEdge> percorso125_2 = ottieniPercorso125_2();
        List<RoadGraphEdge> percorso125_3 = ottieniPercorso125_3();
        List<RoadGraphEdge> percorso125_4 = ottieniPercorso125_4();

        //CreaMacchinaTraffico(160, 0, 0f, percorso125_0);
        /*CreaMacchinaTraffico(160, 1, 0.15f, percorso125_1);
        CreaMacchinaTraffico(160, 1, 0.3f, percorso125_2);*/

        CreaMacchinaTraffico(159, 3, 0f, percorso125_3);
        CreaMacchinaTraffico(159, 2, 0.15f, percorso125_4);

        // CreaMacchinaTraffico(160, 0, 0.45f, percorso125_0);
        /*CreaMacchinaTraffico(160, 1, 0.6f, percorso125_1);
        CreaMacchinaTraffico(160, 1, 0.75f, percorso125_2);

        CreaMacchinaTraffico(159, 3, 0.45f, percorso125_3);
        CreaMacchinaTraffico(159, 2, 0.6f, percorso125_4);*/

        scooterTagliaStrada.maxSpeed = 15f;

    }

    bool secondaVolta = false;
    bool secondaVolta1011 = false;

    /*public void evento1008()
    {
        if (secondaVolta)
        {
            return;
        }

        //List<Vector3> listaWaypoint = ottieniListaWaypoint();
        List<Vector3> listaWaypoint = system.GetEntry(1008, 9).waypoints;
        Vector3 wp3Ok = listaWaypoint[3];
        wp3Ok.x = 1162.84f;
        wp3Ok.z = 950.3f;
        listaWaypoint[3] = wp3Ok;

        ottieniRiferimentoPlayer().GetComponent<TrafAIMotor>().currentEntry.waypoints = listaWaypoint;
        
    }*/

    //TrafAIMotor macchinaTrafficoScorrettaDavanti = null;

    public void evento159()
    {
        if (secondaVolta)
        {
            evento159a();
            return;
        }
        car.GetComponent<TrafAIMotor>().distanzaInizioValutazioneSemaforo = 32f;
        secondaVolta = true;
        semaforo159b();
        List<RoadGraphEdge> percorso159_0 = ottieniPercorso159_0();
        List<RoadGraphEdge> percorso159_1 = ottieniPercorso159_1();
        List<RoadGraphEdge> percorso159_2 = ottieniPercorso159_2();
        List<RoadGraphEdge> percorso159_3 = ottieniPercorso159_3();
        //List<RoadGraphEdge> percorso159_4 = ottieniPercorso159_4();
        //macchinaTrafficoScorrettaDavanti = CreaMacchinaTraffico(162, 3, 0f, percorso159_4);
        //macchinaTrafficoScorrettaDavanti.autoScorretta = true;
        //macchinaTrafficoScorrettaDavanti.maxSpeed = 5.5f;
        //SetLayer(12, macchinaTrafficoScorrettaDavanti.transform);


        List<RoadGraphEdge> percorso159_6 = ottieniPercorso159_6();

        CreaMacchinaTraffico(158, 2, 0.9f, percorso159_0);
        //CreaMacchinaTraffico(158, 2, 0.1f, percorso159_1);
        //CreaMacchinaTraffico(158, 2, 0.7f, percorso159_2);
        CreaMacchinaTraffico(158, 3, 0.9f, percorso159_3);



        //CreaMacchinaTraffico(162, 1, 0.3f, percorso159_6);
    }



    public void evento163()
    {
        List<RoadGraphEdge> percorso163_0 = ottieniPercorso163_0();
        List<RoadGraphEdge> percorso163_1 = ottieniPercorso163_1();
        List<RoadGraphEdge> percorso163_2 = ottieniPercorso163_2();
        List<RoadGraphEdge> percorso163_3 = ottieniPercorso163_3();
        List<RoadGraphEdge> percorso163_4 = ottieniPercorso163_4();
        List<RoadGraphEdge> percorso163_5 = ottieniPercorso163_5();
        List<RoadGraphEdge> percorso163_6 = ottieniPercorso163_6();
        List<RoadGraphEdge> percorso163_7 = ottieniPercorso163_7();

        CreaMacchinaTraffico(22, 2, 0f, percorso163_0);
        CreaMacchinaTraffico(22, 2, 0.15f, percorso163_1);
        //CreaMacchinaTraffico(22, 2, 0.3f, percorso163_2);
        CreaMacchinaTraffico(22, 3, 0.45f, percorso163_3);
        CreaMacchinaTraffico(23, 0, 0.6f, percorso163_4);
        //CreaMacchinaTraffico(23, 1, 0.75f, percorso163_5);
        CreaMacchinaTraffico(23, 0, 0f, percorso163_6);
        //CreaMacchinaTraffico(23, 1, 0f, percorso163_7);

        //elimino il primo waypoint -> evita di andare sul marciapiede
        GameObject go = ottieniRiferimentoPlayer();
        TrafAIMotor traf = go.GetComponent<TrafAIMotor>();
        List<Vector3> listaWaypoint = traf.currentEntry.waypoints;
        listaWaypoint.RemoveAt(0);
        Vector3 waypModificato = new Vector3(1329f, 10.03f, 929.98f);
        listaWaypoint[0] = waypModificato;
        traf.currentEntry.waypoints = listaWaypoint;

        //foreach (Vector3 wayp in listaWaypoint)
        //{
        //    Debug.Log("Wayp: " + wayp.x + " - " + wayp.y + " - " + wayp.z);
        //}


        ottieniRiferimentoPlayer().GetComponent<TrafAIMotor>().maxSpeed = limiteVelocita;
        //macchinaTrafficoScorrettaDavanti.maxSpeed = 8f;


        semaforo163a();
        ottieniRiferimentoPlayer().GetComponent<TrafAIMotor>().sterzataMassima = false;

        _pidPars.velocitaFrenata = 4f;
    }


    public void evento22()
    {
        car.GetComponent<TrafAIMotor>().distanzaInizioValutazioneSemaforo = 32f;
        TrackController.Instance.LanciaOstacolo(10);
        //TrackController.Instance.TriggerObstacle2();
        List<RoadGraphEdge> percorso22_0 = ottieniPercorso22_0();
        List<RoadGraphEdge> percorso22_1 = ottieniPercorso22_1();
        List<RoadGraphEdge> percorso22_2 = ottieniPercorso22_2();
        List<RoadGraphEdge> percorso22_3 = ottieniPercorso22_3();
        List<RoadGraphEdge> percorso22_4 = ottieniPercorso22_4();
        List<RoadGraphEdge> percorso22_5 = ottieniPercorso22_5();
        List<RoadGraphEdge> percorso22_6 = ottieniPercorso22_6();
        List<RoadGraphEdge> percorso22_7 = ottieniPercorso22_7();

        CreaMacchinaTraffico(12, 2, 0.2f, percorso22_0);
        //CreaMacchinaTraffico(12, 2, 0.15f, percorso22_1);
        //CreaMacchinaTraffico(12, 3, 0.3f, percorso22_2);
        //CreaMacchinaTraffico(21, 2, 0.45f, percorso22_3);
        CreaMacchinaTraffico(21, 3, 0.3f, percorso22_4);
        //CreaMacchinaTraffico(30, 0, 0.75f, percorso22_5);
        //CreaMacchinaTraffico(30, 1, 0f, percorso22_6);
        //CreaMacchinaTraffico(30, 0, 0f, percorso22_7);


        //macchinaTrafficoScorrettaDavanti.autoScorretta = true;
    }


    public void evento30()
    {
        car.GetComponent<TrafAIMotor>().distanzaInizioValutazioneSemaforo = 40f;
        semaforo30a();
        //TrackController.Instance.LanciaOstacolo(11);
        //TrackController.Instance.LanciaOstacolo(13);
        List<RoadGraphEdge> percorso30_0 = ottieniPercorso30_0();
        List<RoadGraphEdge> percorso30_1 = ottieniPercorso30_1();
        List<RoadGraphEdge> percorso30_2 = ottieniPercorso30_2();
        List<RoadGraphEdge> percorso30_3 = ottieniPercorso30_3();
        List<RoadGraphEdge> percorso30_4 = ottieniPercorso30_4();
        List<RoadGraphEdge> percorso30_5 = ottieniPercorso30_5();
        List<RoadGraphEdge> percorso30_6 = ottieniPercorso30_6();
        List<RoadGraphEdge> percorso30_7 = ottieniPercorso30_7();

        CreaMacchinaTraffico(158, 0, 0f, percorso30_0);
        CreaMacchinaTraffico(158, 1, 0.15f, percorso30_1);
        CreaMacchinaTraffico(158, 0, 0.3f, percorso30_2);
        CreaMacchinaTraffico(158, 0, 0.45f, percorso30_3);
        CreaMacchinaTraffico(157, 2, 0.6f, percorso30_4);
        CreaMacchinaTraffico(157, 3, 0.75f, percorso30_5);
        CreaMacchinaTraffico(157, 2, 0f, percorso30_6);
        CreaMacchinaTraffico(157, 2, 0f, percorso30_7);
        
    }

    public void evento158()
    {
        //elimino il primo waypoint -> evita di andare sul marciapiede
        List<Vector3> listaWaypoint = ottieniListaWaypoint();
        Vector3 wp = listaWaypoint[0];
        wp.z = 958f;
        listaWaypoint[0] = wp;
        ottieniRiferimentoPlayer().GetComponent<TrafAIMotor>().currentEntry.waypoints = listaWaypoint;
        //GameObject go = ottieniRiferimentoPlayer();
        /*TrafAIMotor traf = go.GetComponent<TrafAIMotor>();
        List<Vector3> listaWaypoint = traf.currentEntry.waypoints;
        listaWaypoint.RemoveAt(0);
        traf.currentEntry.waypoints = listaWaypoint;*/


        TrackController.Instance.LanciaOstacolo(12);
    }

    public void evento159a()
    {
        semaforo159_2();
        //TrackController.Instance.LanciaOstacolo(12);
        ottieniRiferimentoPlayer().GetComponent<TrafAIMotor>().sterzataMassima = true;
    }


    public void evento164()
    {
        TrafEntry entry2 = system.GetEntry(164, 2);
        TrafEntry entry1 = system.GetEntry(164, 1);
        int numeroWaypoint = entry1.waypoints.Count;
        entry1.waypoints[numeroWaypoint - 1] = entry2.waypoints[numeroWaypoint - 1];
        entry1.waypoints[numeroWaypoint - 2] = entry2.waypoints[numeroWaypoint - 2];
        entry1.waypoints[numeroWaypoint - 3] = entry2.waypoints[numeroWaypoint - 3];
        entry1.waypoints[numeroWaypoint - 4] = entry2.waypoints[numeroWaypoint - 4];
        ottieniRiferimentoPlayer().GetComponent<TrafAIMotor>().currentEntry = entry1;
    }



     public void evento1011()
     {
        if (secondaVolta1011)
        {
            return;
        }
        secondaVolta1011 = true;
        /*if (macchinaTrafficoScorretta.currentEntry.identifier != 1011)
        {
            car.GetComponent<TrafAIMotor>().maxSpeed = 1f;
        } else
        {*/
            car.GetComponent<TrafAIMotor>().maxSpeed = 2f;
        //}
     }
     
    public void evento9999()
    {
        Debug.Log("Test interrotto!!!!");
        //fineGuidaAutomatica();       
        fineTest = true;
    }




    /*
    private void evento200()
    {
        Debug.Log("evento200");
        GameObject go = ottieniRiferimentoPlayer();
        TrafEntry entry = system.GetEntry(3, 0);
        go.GetComponent<TrafAIMotor>().nextEntry = entry;
    }*/




    private List<RoadGraphEdge> ottieniPercorsoPlayer()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();
        RoadGraphEdge edge4 = new RoadGraphEdge();
        RoadGraphEdge edge5 = new RoadGraphEdge();
        RoadGraphEdge edge6 = new RoadGraphEdge();
        RoadGraphEdge edge7 = new RoadGraphEdge();
        RoadGraphEdge edge8 = new RoadGraphEdge();
        RoadGraphEdge edge9 = new RoadGraphEdge();
        RoadGraphEdge edge10 = new RoadGraphEdge();
        RoadGraphEdge edge11 = new RoadGraphEdge();
        RoadGraphEdge edge12 = new RoadGraphEdge();
        RoadGraphEdge edge13 = new RoadGraphEdge();
        RoadGraphEdge edge14 = new RoadGraphEdge();
        RoadGraphEdge edge15 = new RoadGraphEdge();
        RoadGraphEdge edge16 = new RoadGraphEdge();
        RoadGraphEdge edge17 = new RoadGraphEdge();
        RoadGraphEdge edge18 = new RoadGraphEdge();
        RoadGraphEdge edge19 = new RoadGraphEdge();
        RoadGraphEdge edge20 = new RoadGraphEdge();
        RoadGraphEdge edge21 = new RoadGraphEdge();
        RoadGraphEdge edge22 = new RoadGraphEdge();
        RoadGraphEdge edge23 = new RoadGraphEdge();
        RoadGraphEdge edge24 = new RoadGraphEdge();
        RoadGraphEdge edge25 = new RoadGraphEdge();

        RoadGraphEdge edge1a = new RoadGraphEdge();
        RoadGraphEdge edge2a = new RoadGraphEdge();
        RoadGraphEdge edge3a = new RoadGraphEdge();
        RoadGraphEdge edge4a = new RoadGraphEdge();
        RoadGraphEdge edge5a = new RoadGraphEdge();
        RoadGraphEdge edge6a = new RoadGraphEdge();
        RoadGraphEdge edge7a = new RoadGraphEdge();
        RoadGraphEdge edge8a = new RoadGraphEdge();
        RoadGraphEdge edge9a = new RoadGraphEdge();
        RoadGraphEdge edge10a = new RoadGraphEdge();
        RoadGraphEdge edge11a = new RoadGraphEdge();
        RoadGraphEdge edge12a = new RoadGraphEdge();
        RoadGraphEdge edge13a = new RoadGraphEdge();
        RoadGraphEdge edge14a = new RoadGraphEdge();
        RoadGraphEdge edge15a = new RoadGraphEdge();
        RoadGraphEdge edge16a = new RoadGraphEdge();
        RoadGraphEdge edge17a = new RoadGraphEdge();
        RoadGraphEdge edge18a = new RoadGraphEdge();
        RoadGraphEdge edge19a = new RoadGraphEdge();
        RoadGraphEdge edge20a = new RoadGraphEdge();
        RoadGraphEdge edge21a = new RoadGraphEdge();
        RoadGraphEdge edge22a = new RoadGraphEdge();
        RoadGraphEdge edge23a = new RoadGraphEdge();
        RoadGraphEdge edge24a = new RoadGraphEdge();

        edge1.id = 120;
        edge1.subId = 0;

        edge1a.id = 1088;
        edge1a.subId = 3;

        edge2.id = 119;
        edge2.subId = 0;

        edge2a.id = 1089;
        edge2a.subId = 5;

        edge3.id = 39;
        edge3.subId = 3;

        edge3a.id = 1082;
        edge3a.subId = 0;

        edge4.id = 38;
        edge4.subId = 3;

        edge4a.id = 1081;
        edge4a.subId = 18;

        edge5.id = 49;
        edge5.subId = 2;

        edge5a.id = 1083;
        edge5a.subId = 1;

        edge6.id = 50;
        edge6.subId = 2;

        edge6a.id = 1010;
        edge6a.subId = 14;

        edge7.id = 63;
        edge7.subId = 2;

        edge7a.id = 1036;
        edge7a.subId = 2;

        edge8.id = 64;
        edge8.subId = 2;

        edge8a.id = 1035;
        edge8a.subId = 10;

        edge9.id = 65;
        edge9.subId = 2;

        edge9a.id = 1039;
        edge9a.subId = 2;

        edge10.id = 13;
        edge10.subId = 2;

        edge10a.id = 1012;
        edge10a.subId = 6;

        edge11.id = 14;
        edge11.subId = 2;

        edge11a.id = 1048;
        edge11a.subId = 2;

        edge12.id = 15;
        edge12.subId = 2;

        edge12a.id = 1031;
        edge12a.subId = 10;

        edge13.id = 16;
        edge13.subId = 2;

        edge13a.id = 1049;
        edge13a.subId = 5;

        edge14.id = 18;
        edge14.subId = 1;

        edge14a.id = 1007;
        edge14a.subId = 4;

        edge15.id = 5;
        edge15.subId = 0;

        edge15a.id = 1003;
        edge15a.subId = 7;

        edge16.id = 3;
        edge16.subId = 3;

        edge16a.id = 1031;
        edge16a.subId = 3;

        edge17.id = 4;
        edge17.subId = 3;

        edge17a.id = 1014;
        edge17a.subId = 7;

        edge18.id = 125;
        edge18.subId = 0;

        edge18a.id = 1008;
        edge18a.subId = 19;

        edge19.id = 159;
        edge19.subId = 0;

        edge19a.id = 1011;
        edge19a.subId = 8;

        edge20.id = 163;
        edge20.subId = 3;

        edge20a.id = 1017;
        edge20a.subId = 17;

        edge21.id = 22;
        edge21.subId = 1;

        edge21a.id = 1045;
        edge21a.subId = 13;

        edge22.id = 30;
        edge22.subId = 2;

        edge22a.id = 1052;
        edge22a.subId = 8;

        edge23.id = 158;
        edge23.subId = 3;

        edge23a.id = 1011;
        edge23a.subId = 3;

        edge24.id = 159;
        edge24.subId = 3;

        edge24a.id = 1008;
        edge24a.subId = 10;

        edge25.id = 164;
        edge25.subId = 1;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge1a);
        percorso.Add(edge2);
        percorso.Add(edge2a);
        percorso.Add(edge3);
        percorso.Add(edge3a);
        percorso.Add(edge4);
        percorso.Add(edge4a);
        percorso.Add(edge5);
        percorso.Add(edge5a);
        percorso.Add(edge6);
        percorso.Add(edge6a);
        percorso.Add(edge7);
        percorso.Add(edge7a);
        percorso.Add(edge8);
        percorso.Add(edge8a);
        percorso.Add(edge9);
        percorso.Add(edge9a);
        percorso.Add(edge10);
        percorso.Add(edge10a);
        percorso.Add(edge11);
        percorso.Add(edge11a);
        percorso.Add(edge12);
        percorso.Add(edge12a);
        percorso.Add(edge13);
        percorso.Add(edge13a);
        percorso.Add(edge14);
        percorso.Add(edge14a);
        percorso.Add(edge15);
        percorso.Add(edge15a);
        percorso.Add(edge16);
        percorso.Add(edge16a);
        percorso.Add(edge17);
        percorso.Add(edge17a);
        percorso.Add(edge18);
        percorso.Add(edge18a);
        percorso.Add(edge19);
        percorso.Add(edge19a);
        percorso.Add(edge20);
        percorso.Add(edge20a);
        percorso.Add(edge21);
        percorso.Add(edge21a);
        percorso.Add(edge22);
        percorso.Add(edge22a);
        percorso.Add(edge23);
        percorso.Add(edge23a);
        percorso.Add(edge24);
        percorso.Add(edge24a);
        percorso.Add(edge25);

        List<RoadGraphEdge> percorsoOk = new List<RoadGraphEdge>();

        int indiceIniziale = numeroInizioTest;
        //caso numeroInizioTest pari
        if ((numeroInizioTest % 2) == 0)
        {
            percorsoOk.Add(percorso[indiceIniziale]);
            percorsoOk.Add(percorso[indiceIniziale++]);           
        } else
        {
            percorsoOk.Add(percorso[indiceIniziale]);
        }
        indiceIniziale++;

        for (int i = indiceIniziale; i < percorso.Count; i+=2)
        {
            percorsoOk.Add(percorso[i]);
        }

        return percorsoOk;
    }

    private List<RoadGraphEdge> ottieniPercorso39_0()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();

        edge1.id = 1082;
        edge1.subId = 8;

        edge2.id = 38;
        edge2.subId = 0;

        edge3.id = 36;
        edge3.subId = 2;



        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso39_1()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();
        RoadGraphEdge edge4 = new RoadGraphEdge();

        edge1.id = 1082;
        edge1.subId = 8;

        edge2.id = 38;
        edge2.subId = 2;

        edge3.id = 37;
        edge3.subId = 2;

        edge4.id = 32;
        edge4.subId = 2;



        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);
        percorso.Add(edge4);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso38_0()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();

        edge1.id = 1081;
        edge1.subId = 0;

        edge2.id = 49;
        edge2.subId = 3;

        edge3.id = 43;
        edge3.subId = 2;



        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso38_1()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1081;
        edge1.subId = 0;

        edge2.id = 36;
        edge2.subId = 2;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso38_2()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();

        edge1.id = 1081;
        edge1.subId = 0;

        edge2.id = 37;
        edge2.subId = 3;

        edge3.id = 32;
        edge3.subId = 3;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso49_0()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1083;
        edge1.subId = 0;

        edge2.id = 49;
        edge2.subId = 0;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso49_1()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1083;
        edge1.subId = 0;

        edge2.id = 49;
        edge2.subId = 1;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso49_2()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1083;
        edge1.subId = 0;

        edge2.id = 49;
        edge2.subId = 0;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso49_3()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1083;
        edge1.subId = 6;

        edge2.id = 43;
        edge2.subId = 2;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso49_4()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1083;
        edge1.subId = 0;

        edge2.id = 43;
        edge2.subId = 1;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso49_5()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();

        edge1.id = 1083;
        edge1.subId = 0;

        edge2.id = 50;
        edge2.subId = 3;

        edge3.id = 51;
        edge3.subId = 3;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso50_0()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();

        edge1.id = 1010;
        edge1.subId = 0;

        edge2.id = 63;
        edge2.subId = 0;

        edge3.id = 53;
        edge3.subId = 0;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso50_1()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1010;
        edge1.subId = 0;

        edge2.id = 50;
        edge2.subId = 1;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso50_2()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();

        edge1.id = 1010;
        edge1.subId = 0;

        edge2.id = 63;
        edge2.subId = 0;

        edge3.id = 53;
        edge3.subId = 0;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso50_3()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1010;
        edge1.subId = 0;

        edge2.id = 50;
        edge2.subId = 1;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso63_0()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1036;
        edge1.subId = 0;

        edge2.id = 53;
        edge2.subId = 0;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso63_1()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1036;
        edge1.subId = 0;

        edge2.id = 53;
        edge2.subId = 1;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso63_2()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();

        edge1.id = 1036;
        edge1.subId = 0;

        edge2.id = 64;
        edge2.subId = 3;

        edge3.id = 55;
        edge3.subId = 2;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso64_0()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1035;
        edge1.subId = 0;

        edge2.id = 55;
        edge2.subId = 0;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso64_1()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1035;
        edge1.subId = 0;

        edge2.id = 55;
        edge2.subId = 1;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso64_2()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1035;
        edge1.subId = 0;

        edge2.id = 55;
        edge2.subId = 2;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso64_3()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();

        edge1.id = 1039;
        edge1.subId = 0;

        edge2.id = 13;
        edge2.subId = 3;

        edge3.id = 12;
        edge3.subId = 3;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso65_0()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();
        RoadGraphEdge edge4 = new RoadGraphEdge();
        RoadGraphEdge edge5 = new RoadGraphEdge();
        RoadGraphEdge edge6 = new RoadGraphEdge();
        RoadGraphEdge edge7 = new RoadGraphEdge();

        edge1.id = 1039;
        edge1.subId = 0;

        edge2.id = 13;
        edge2.subId = 2;

        edge3.id = 14;
        edge3.subId = 2;

        edge4.id = 15;
        edge4.subId = 2;

        edge5.id = 16;
        edge5.subId = 2;

        edge6.id = 18;
        edge6.subId = 1;

        edge7.id = 19;
        edge7.subId = 1;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);
        percorso.Add(edge4);
        percorso.Add(edge5);
        percorso.Add(edge6);
        percorso.Add(edge7);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso65_1()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1039;
        edge1.subId = 0;

        edge2.id = 26;
        edge2.subId = 2;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso65_2()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1039;
        edge1.subId = 0;

        edge2.id = 26;
        edge2.subId = 1;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso65_3()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1039;
        edge1.subId = 0;

        edge2.id = 26;
        edge2.subId = 0;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso65_4()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();

        edge1.id = 1039;
        edge1.subId = 0;

        edge2.id = 13;
        edge2.subId = 0;

        edge3.id = 11;
        edge3.subId = 1;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso13_0()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();
        RoadGraphEdge edge4 = new RoadGraphEdge();
        RoadGraphEdge edge5 = new RoadGraphEdge();
        RoadGraphEdge edge6 = new RoadGraphEdge();

        edge1.id = 1012;
        edge1.subId = 0;

        edge2.id = 14;
        edge2.subId = 0;

        edge3.id = 15;
        edge3.subId = 0;

        edge4.id = 16;
        edge4.subId = 0;

        edge5.id = 18;
        edge5.subId = 0;

        edge6.id = 19;
        edge6.subId = 0;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);
        percorso.Add(edge4);
        percorso.Add(edge5);
        percorso.Add(edge6);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso13_1()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();
        RoadGraphEdge edge4 = new RoadGraphEdge();
        RoadGraphEdge edge5 = new RoadGraphEdge();
        RoadGraphEdge edge6 = new RoadGraphEdge();

        edge1.id = 1012;
        edge1.subId = 0;

        edge2.id = 14;
        edge2.subId = 0;

        edge3.id = 15;
        edge3.subId =0;

        edge4.id = 16;
        edge4.subId = 0;

        edge5.id = 18;
        edge5.subId = 0;

        edge6.id = 19;
        edge6.subId = 0;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);
        percorso.Add(edge4);
        percorso.Add(edge5);
        percorso.Add(edge6);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso13_2()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();
        RoadGraphEdge edge4 = new RoadGraphEdge();
        RoadGraphEdge edge5 = new RoadGraphEdge();
        RoadGraphEdge edge6 = new RoadGraphEdge();

        edge1.id = 1012;
        edge1.subId = 0;

        edge2.id = 14;
        edge2.subId = 3;

        edge3.id = 15;
        edge3.subId = 3;

        edge4.id = 16;
        edge4.subId = 3;

        edge5.id = 18;
        edge5.subId = 3;

        edge6.id = 19;
        edge6.subId = 3;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);
        percorso.Add(edge4);
        percorso.Add(edge5);
        percorso.Add(edge6);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso13_3()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1012;
        edge1.subId = 0;

        edge2.id = 12;
        edge2.subId = 3;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso13_4()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1012;
        edge1.subId = 0;

        edge2.id = 11;
        edge2.subId = 1;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso13_5()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1012;
        edge1.subId = 0;

        edge2.id = 11;
        edge2.subId = 0;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso14_0()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();

        edge1.id = 1048;
        edge1.subId = 0;

        edge2.id = 15;
        edge2.subId = 3;

        edge3.id = 4;
        edge3.subId = 2;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso14_1()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();
        RoadGraphEdge edge4 = new RoadGraphEdge();
        RoadGraphEdge edge5 = new RoadGraphEdge();

        edge1.id = 1048;
        edge1.subId = 0;

        edge2.id = 15;
        edge2.subId = 2;

        edge3.id = 16;
        edge3.subId = 2;

        edge4.id = 18;
        edge4.subId = 2;

        edge5.id = 19;
        edge5.subId = 2;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);
        percorso.Add(edge4);
        percorso.Add(edge5);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso14_2()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();
        RoadGraphEdge edge4 = new RoadGraphEdge();
        RoadGraphEdge edge5 = new RoadGraphEdge();

        edge1.id = 1048;
        edge1.subId = 0;

        edge2.id = 15;
        edge2.subId = 0;

        edge3.id = 16;
        edge3.subId = 0;

        edge4.id = 18;
        edge4.subId = 0;

        edge5.id = 19;
        edge5.subId = 0;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);
        percorso.Add(edge4);
        percorso.Add(edge5);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso15_0()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1031;
        edge1.subId = 0;

        edge2.id = 4;
        edge2.subId = 3;

        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso15_1()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();
        RoadGraphEdge edge4 = new RoadGraphEdge();

        edge1.id = 1031;
        edge1.subId = 0;

        edge2.id = 15;
        edge2.subId = 0;

        edge3.id = 18;
        edge3.subId = 0;

        edge4.id = 19;
        edge4.subId = 0;

        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);
        percorso.Add(edge4);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso16_0()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();

        edge1.id = 1049;
        edge1.subId = 0;

        edge2.id = 18;
        edge2.subId = 1;

        edge3.id = 19;
        edge3.subId = 1;

        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso16_1()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();

        edge1.id = 1049;
        edge1.subId = 0;

        edge2.id = 18;
        edge2.subId = 3;

        edge3.id = 19;
        edge3.subId = 3;

        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso16_2()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();
        RoadGraphEdge edge4 = new RoadGraphEdge();

        edge1.id = 1049;
        edge1.subId = 0;

        edge2.id = 18;
        edge2.subId = 0;

        edge3.id = 5;
        edge3.subId = 2;

        edge4.id = 6;
        edge4.subId = 2;

        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);
        percorso.Add(edge4);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso16_3()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();

        edge1.id = 1049;
        edge1.subId = 0;

        edge2.id = 18;
        edge2.subId = 0;

        edge3.id = 5;
        edge3.subId = 1;

        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso16_4()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();

        edge1.id = 1049;
        edge1.subId = 0;

        edge2.id = 18;
        edge2.subId = 0;

        edge3.id = 5;
        edge3.subId = 2;

        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso18_0()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();
        RoadGraphEdge edge4 = new RoadGraphEdge();
        RoadGraphEdge edge5 = new RoadGraphEdge();

        edge1.id = 1049;
        edge1.subId = 0;

        edge2.id = 18;
        edge2.subId = 0;

        edge3.id = 5;
        edge3.subId = 0;

        edge4.id = 3;
        edge4.subId = 0;

        edge5.id = 16;
        edge5.subId = 0;

        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);
        percorso.Add(edge4);
        percorso.Add(edge5);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso18_1()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();
        RoadGraphEdge edge4 = new RoadGraphEdge();

        edge1.id = 1049;
        edge1.subId = 0;

        edge2.id = 18;
        edge2.subId = 0;

        edge3.id = 5;
        edge3.subId = 1;

        edge4.id = 6;
        edge4.subId = 1;

        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);
        percorso.Add(edge4);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso5_0()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();

        edge1.id = 1003;
        edge1.subId = 0;

        edge2.id = 3;
        edge2.subId = 0;

        edge3.id = 16;
        edge3.subId = 0;

        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso5_1()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();
        RoadGraphEdge edge4 = new RoadGraphEdge();
        RoadGraphEdge edge5 = new RoadGraphEdge();

        edge1.id = 1003;
        edge1.subId = 0;

        edge2.id = 3;
        edge2.subId = 0;

        edge3.id = 4;
        edge3.subId = 0;

        edge4.id = 125;
        edge4.subId = 3;

        edge5.id = 164;
        edge5.subId = 3;

        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);
        percorso.Add(edge4);
        percorso.Add(edge5);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso5_2()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();
        RoadGraphEdge edge4 = new RoadGraphEdge();
        RoadGraphEdge edge5 = new RoadGraphEdge();

        edge1.id = 1003;
        edge1.subId = 0;

        edge2.id = 3;
        edge2.subId = 1;

        edge3.id = 4;
        edge3.subId = 1;

        edge4.id = 125;
        edge4.subId = 2;

        edge5.id = 164;
        edge5.subId = 2;

        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);
        percorso.Add(edge4);
        percorso.Add(edge5);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso5_3()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();
        RoadGraphEdge edge4 = new RoadGraphEdge();
        RoadGraphEdge edge5 = new RoadGraphEdge();

        edge1.id = 1003;
        edge1.subId = 0;

        edge2.id = 3;
        edge2.subId = 2;

        edge3.id = 4;
        edge3.subId = 2;

        edge4.id = 125;
        edge4.subId = 1;

        edge5.id = 164;
        edge5.subId = 1;

        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);
        percorso.Add(edge4);
        percorso.Add(edge5);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso5_4()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();
        RoadGraphEdge edge4 = new RoadGraphEdge();
        RoadGraphEdge edge5 = new RoadGraphEdge();

        edge1.id = 1003;
        edge1.subId = 0;

        edge2.id = 3;
        edge2.subId = 3;

        edge3.id = 4;
        edge3.subId = 3;

        edge4.id = 125;
        edge4.subId = 0;

        edge5.id = 164;
        edge5.subId = 0;

        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);
        percorso.Add(edge4);
        percorso.Add(edge5);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso5_5()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();
        RoadGraphEdge edge4 = new RoadGraphEdge();

        edge1.id = 1003;
        edge1.subId = 0;

        edge2.id = 3;
        edge2.subId = 3;

        edge3.id = 4;
        edge3.subId = 3;

        edge4.id = 23;
        edge4.subId = 0;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);
        percorso.Add(edge4);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso5_6()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1003;
        edge1.subId = 0;

        edge2.id = 6;
        edge2.subId = 1;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso5_7()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1003;
        edge1.subId = 0;

        edge2.id = 6;
        edge2.subId = 2;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso3_0()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();
        RoadGraphEdge edge4 = new RoadGraphEdge();

        edge1.id = 1031;
        edge1.subId = 0;

        edge2.id = 4;
        edge2.subId = 1;

        edge3.id = 125;
        edge3.subId = 2;

        edge4.id = 164;
        edge4.subId = 2;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);
        percorso.Add(edge4);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso3_1()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();
        RoadGraphEdge edge4 = new RoadGraphEdge();

        edge1.id = 1031;
        edge1.subId = 0;

        edge2.id = 4;
        edge2.subId = 3;

        edge3.id = 125;
        edge3.subId = 0;

        edge4.id = 164;
        edge4.subId = 0;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);
        percorso.Add(edge4);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso3_2()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();
        RoadGraphEdge edge4 = new RoadGraphEdge();

        edge1.id = 1031;
        edge1.subId = 0;

        edge2.id = 4;
        edge2.subId = 3;

        edge3.id = 125;
        edge3.subId = 0;

        edge4.id = 159;
        edge4.subId = 1;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);
        percorso.Add(edge4);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso3_3()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();
        RoadGraphEdge edge4 = new RoadGraphEdge();

        edge1.id = 1031;
        edge1.subId = 0;

        edge2.id = 4;
        edge2.subId = 3;

        edge3.id = 125;
        edge3.subId = 0;

        edge4.id = 159;
        edge4.subId = 0;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);
        percorso.Add(edge4);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso3_4()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1031;
        edge1.subId = 0;

        edge2.id = 16;
        edge2.subId = 1;



        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso3_5()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1031;
        edge1.subId = 0;

        edge2.id = 16;
        edge2.subId = 2;



        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso3_6()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1031;
        edge1.subId = 0;

        edge2.id = 16;
        edge2.subId = 0;



        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso4_0()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1014;
        edge1.subId = 0;

        edge2.id = 23;
        edge2.subId = 1;



        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso4_1()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1014;
        edge1.subId = 0;

        edge2.id = 23;
        edge2.subId = 0;



        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso4_2()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1014;
        edge1.subId = 0;

        edge2.id = 24;
        edge2.subId = 2;



        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso4_3()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1014;
        edge1.subId = 0;

        edge2.id = 24;
        edge2.subId = 3;



        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso4_4()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();
        RoadGraphEdge edge4 = new RoadGraphEdge();

        edge1.id = 1014;
        edge1.subId = 0;

        edge2.id = 125;
        edge2.subId = 0;

        edge3.id = 159;
        edge3.subId = 0;

        edge4.id = 158;
        edge4.subId = 0;



        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);
        percorso.Add(edge4);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso4_5()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();

        edge1.id = 1014;
        edge1.subId = 0;

        edge2.id = 125;
        edge2.subId = 1;

        edge3.id = 164;
        edge3.subId = 1;



        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso4_6()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();

        edge1.id = 1014;
        edge1.subId = 0;

        edge2.id = 125;
        edge2.subId = 2;

        edge3.id = 164;
        edge3.subId = 2;



        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso4_7()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();

        edge1.id = 1014;
        edge1.subId = 0;

        edge2.id = 125;
        edge2.subId = 3;

        edge3.id = 164;
        edge3.subId = 3;



        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso125_0()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();

        edge1.id = 1008;
        edge1.subId = 0;

        edge2.id = 159;
        edge2.subId = 0;

        edge3.id = 163;
        edge3.subId = 1;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso125_1()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();

        edge1.id = 1008;
        edge1.subId = 0;

        edge2.id = 159;
        edge2.subId = 1;

        edge3.id = 158;
        edge3.subId = 1;

        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso125_2()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1008;
        edge1.subId = 0;

        edge2.id = 164;
        edge2.subId = 1;




        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);


        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso125_3()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1008;
        edge1.subId = 0;

        edge2.id = 164;
        edge2.subId = 2;

        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);


        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso125_4()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1008;
        edge1.subId = 0;

        edge2.id = 160;
        edge2.subId = 2;

        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);


        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso159_0()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();

        edge1.id = 1011;
        edge1.subId = 0;

        edge2.id = 163;
        edge2.subId = 1;

        edge3.id = 20;
        edge3.subId = 2;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso159_1()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();

        edge1.id = 1011;
        edge1.subId = 0;

        edge2.id = 163;
        edge2.subId = 2;

        edge3.id = 20;
        edge3.subId = 1;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso159_2()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1011;
        edge1.subId = 0;

        edge2.id = 159;
        edge2.subId = 2;

        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);


        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso159_3()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1011;
        edge1.subId = 0;

        edge2.id = 159;
        edge2.subId = 3;

        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);


        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso159_4()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();
        RoadGraphEdge edge4 = new RoadGraphEdge();

        edge1.id = 1011;
        edge1.subId = 0;

        edge2.id = 163;
        edge2.subId = 3;

        edge3.id = 20; //era a 22
        edge3.subId = 0;

        /*edge4.id = 21;
        edge4.subId = 0;*/


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);
        //percorso.Add(edge4);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso159_5()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();

        edge1.id = 1011;
        edge1.subId = 0;

        edge2.id = 163;
        edge2.subId = 0;

        edge3.id = 23;
        edge3.subId = 3;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso159_6()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1011;
        edge1.subId = 0;

        edge2.id = 163;
        edge2.subId = 1;

        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);


        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso163_0()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1017;
        edge1.subId = 0;

        edge2.id = 20;
        edge2.subId = 2;

        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);


        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso163_1()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1017;
        edge1.subId = 0;

        edge2.id = 20;
        edge2.subId = 1;

        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);


        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso163_2()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1017;
        edge1.subId = 0;

        edge2.id = 23;
        edge2.subId = 2;

        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);


        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso163_3()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1017;
        edge1.subId = 0;

        edge2.id = 23;
        edge2.subId = 3;

        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);


        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso163_4()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();

        edge1.id = 1017;
        edge1.subId = 0;

        edge2.id = 22;
        edge2.subId = 0;

        edge3.id = 21;
        edge3.subId = 0;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso163_5()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();
        RoadGraphEdge edge4 = new RoadGraphEdge();
        RoadGraphEdge edge5 = new RoadGraphEdge();

        edge1.id = 1017;
        edge1.subId = 0;

        edge2.id = 22;
        edge2.subId = 1;

        edge3.id = 30;
        edge3.subId = 3;

        edge4.id = 157;
        edge4.subId = 0;

        edge5.id = 156;
        edge5.subId = 0;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);
        percorso.Add(edge4);
        percorso.Add(edge5);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso163_6()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1017;
        edge1.subId = 0;

        edge2.id = 20;
        edge2.subId = 1;

        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);


        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso163_7()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();
        RoadGraphEdge edge4 = new RoadGraphEdge();

        edge1.id = 1017;
        edge1.subId = 0;

        edge2.id = 22;
        edge2.subId = 1;

        edge3.id = 30;
        edge3.subId = 2;

        edge4.id = 158;
        edge4.subId = 2;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);
        percorso.Add(edge4);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso22_0()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();

        edge1.id = 1045;
        edge1.subId = 0;

        edge2.id = 30;
        edge2.subId = 2;

        edge3.id = 158;
        edge3.subId = 2;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso22_1()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1045;
        edge1.subId = 0;

        edge2.id = 22;
        edge2.subId = 2;

        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);


        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso22_2()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();
        RoadGraphEdge edge4 = new RoadGraphEdge();

        edge1.id = 1045;
        edge1.subId = 0;

        edge2.id = 30;
        edge2.subId = 3;

        edge3.id = 157;
        edge3.subId = 0;

        edge4.id = 156;
        edge4.subId = 0;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);
        percorso.Add(edge4);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso22_3()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();

        edge1.id = 1045;
        edge1.subId = 0;

        edge2.id = 30;
        edge2.subId = 2;

        edge3.id = 158;
        edge3.subId = 2;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso22_4()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();
        RoadGraphEdge edge4 = new RoadGraphEdge();

        edge1.id = 1045;
        edge1.subId = 0;

        edge2.id = 30;
        edge2.subId = 3;

        edge3.id = 157;
        edge3.subId = 1;

        edge4.id = 156;
        edge4.subId = 1;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);
        percorso.Add(edge4);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso22_5()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1045;
        edge1.subId = 0;

        edge2.id = 21;
        edge2.subId = 0;

        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);


        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso22_6()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1045;
        edge1.subId = 0;

        edge2.id = 21;
        edge2.subId = 1;

        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);


        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso22_7()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1045;
        edge1.subId = 0;

        edge2.id = 22;
        edge2.subId = 3;

        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);


        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso30_0()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();

        edge1.id = 1052;
        edge1.subId = 0;

        edge2.id = 157;
        edge2.subId = 0;

        edge3.id = 156;
        edge3.subId = 0;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso30_1()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();
        RoadGraphEdge edge3 = new RoadGraphEdge();

        edge1.id = 1052;
        edge1.subId = 0;

        edge2.id = 157;
        edge2.subId = 1;

        edge3.id = 156;
        edge3.subId = 1;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso30_2()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1052;
        edge1.subId = 0;

        edge2.id = 30;
        edge2.subId = 0;

        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso30_3()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1052;
        edge1.subId = 0;

        edge2.id = 30;
        edge2.subId = 1;

        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso30_4()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1052;
        edge1.subId = 0;

        edge2.id = 158;
        edge2.subId = 2;

        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso30_5()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1052;
        edge1.subId = 0;

        edge2.id = 158;
        edge2.subId = 3;

        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso30_6()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1052;
        edge1.subId = 0;

        edge2.id = 30;
        edge2.subId = 1;

        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorso30_7()
    {
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge1.id = 1052;
        edge1.subId = 0;

        edge2.id = 30;
        edge2.subId = 0;

        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);

        return percorso;
    }

    private List<RoadGraphEdge> ottieniPercorsoMacchinTagliaStrada4()
    {
        RoadGraphEdge edge0 = new RoadGraphEdge();
        RoadGraphEdge edge1 = new RoadGraphEdge();
        RoadGraphEdge edge2 = new RoadGraphEdge();

        edge0.id = 4;
        edge0.subId = 2;

        edge1.id = 125;
        edge1.subId = 1;

        edge2.id = 164;
        edge2.subId = 0;

        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge0);
        percorso.Add(edge1);
        percorso.Add(edge2);        

        return percorso;
    }




    //private bool guidaAutomatica = false;
    public int maxIdent = 20;
    public int maxSub = 4;
    public float checkRadius = 8f;


    public void fineGuidaAutomatica()
    {
        GameObject go = ottieniRiferimentoPlayer();
        Destroy(go.GetComponent<TrafAIMotor>());
        Destroy(go.GetComponent<GuidaManuale>());
        go.GetComponent<VehicleInputController>().enabled = true;
       // go.GetComponent<VehicleController>().accellInput = 0f;
        //guidaAutomatica = false;

        foreach (Transform game in go.GetComponentsInChildren<Transform>())
        {
            if (game.name.Equals("nose") || game.name.Equals("colliderOstacoli") || game.name.Equals("raggioDestra") || game.name.Equals("raggioSinistra"))
            {
                Destroy(game.gameObject);
            }
        }

        secondaVolta = false;
        secondaVolta1011 = false;
        
    }

    private void attivaGuidaAutomatica(GameObject go, List<RoadGraphEdge> percorso, int id, int subId)
    {

        float distance = 0f;
        if (id == 120)
        {
            distance = 0.5f;
        }



        TrafEntry entry = system.GetEntry(id, subId); //-> serve a ottenere la entry di un determinato punto, bisogna indicare il giusto id e subid


        if (entry == null)
        {
            Debug.Log("Entry = null");
            return;
        }

        InterpolatedPosition pos = entry.GetInterpolatedPosition(distance);

        

        if (!Physics.CheckSphere(pos.position, checkRadius, 1 << LayerMask.NameToLayer("Traffic")))
        {

            GameObject nose = new GameObject("nose");
            nose.transform.SetParent(go.transform);
            if (go.name.Contains("XE"))
            {
                nose.transform.localPosition = new Vector3(0, 0.5f, 2f); //->per la jaguar
            }
            else
            {
                nose.transform.localPosition = new Vector3(0, 0.5f, 2.61f); //->per la tesla
            }


            nose.transform.localRotation = Quaternion.identity;
            nose.transform.localScale = new Vector3(2f, 2f, 2f);


            TrafAIMotor motor = go.AddComponent<TrafAIMotor>();
            go.GetComponent<VehicleInputController>().enabled = false;

            //GuidaManuale guidaManuale = go.AddComponent<GuidaManuale>();
            //guidaManuale.setMotor(motor);


            GameObject colliderOstacoli = new GameObject("colliderOstacoli");
            colliderOstacoli.transform.SetParent(go.transform);
            BoxCollider boxColliderOstacoli = colliderOstacoli.AddComponent<BoxCollider>();
            boxColliderOstacoli.isTrigger = true;
            //colliderOstacoli.transform.localPosition = new Vector3(0f, 0.65f, 7f);
            colliderOstacoli.transform.localPosition = new Vector3(0f, 0.65f, 9.5f);
            colliderOstacoli.transform.localScale = new Vector3(1.7f, 1f, 1f);
            colliderOstacoli.transform.localRotation = Quaternion.identity;
            //boxColliderOstacoli.size = new Vector3(3.5f, 0.75f, 10f);
            boxColliderOstacoli.size = new Vector3(3.5f, 0.75f, 15f);
            TrafAIMotor.GestoreCollisioni gestore = colliderOstacoli.AddComponent<TrafAIMotor.GestoreCollisioni>();
            gestore.setMotor(motor);


            //L'ISTRUZIONE SOTTO FA TELETRASPORTARE L'AUTO NELLA POSIZIONE pos.position
            go.transform.position = pos.position;

            //go.AddComponent<TrafWheels>();

            motor.currentIndex = pos.targetIndex;
            motor.currentEntry = entry;

            //L'istruzione sotto ruota la macchina in direzione del waypoint target
            go.transform.LookAt(entry.waypoints[pos.targetIndex]);

            motor.system = system;
            motor.nose = nose;
            motor.raycastOrigin = nose.transform;
            motor.targetHeight = 0f;
            motor.waypointThreshold = 6f;

            //Debug.Log("pos.position: x = " + pos.position.x + "; y = " + pos.position.y + "; z = " + pos.position.z + "; targetIndex = " + pos.targetIndex);
            //guidaAutomatica = true;

            List<RoadGraphEdge> percorsoPlayer = percorso.GetRange(1, percorso.Count - 1);

            motor.fixedRoute = true;
            motor.fixedPath = percorsoPlayer;

            //aspettiamo due secondi prima di avviare la macchina e avviare la piattaforma
            StartCoroutine(Attesa2Secondi(motor));

            //motor.Init();


        }
        else
        {
            attivaGuidaAutomatica(go, percorso, id, subId); //è una ricorsione, fa si che si ripete la funzione finchè tutto vada bene
        }


    }

    IEnumerator Attesa2Secondi(TrafAIMotor motor)
    {
        yield return new WaitForSeconds(2f);
        motor.Init();
        car.GetComponent<xSimScript>().enabled = true;
    }

    //ANTONELLO
    private float calcolaDistanza(TrafEntry entry, GameObject go)
    {
        //questa funzione calcola vicino a che waypoint della entry ci troviamo
        //e calcola la distanza dall'inizio della entry fino al waypoint vicino al quale ci troviamo

        int numeroWaypoint = 1;
        foreach (Vector3 waypoint in entry.waypoints)
        {
            float miaX = go.transform.position.x;
            float miaY = go.transform.position.y;
            float miaZ = go.transform.position.z;
            float waypointX = waypoint.x;
            float waypointY = waypoint.y;
            float waypointZ = waypoint.z;
            if (System.Math.Abs(waypointX - miaX) <= 2f || System.Math.Abs(waypointZ - miaZ) <= 2f)
            {
                break;
            }
            numeroWaypoint++;
        }

        if (numeroWaypoint != 1)
        {
            float totalDist = 0f;
            for (int i = 1; i < entry.waypoints.Count; i++)
            {
                totalDist += Vector3.Distance(entry.waypoints[i], entry.waypoints[i - 1]);
            }


            float workingDist = 0f;

            for (int i = 1; i < numeroWaypoint; i++)
            {
                float thisDist = Vector3.Distance(entry.waypoints[i], entry.waypoints[i - 1]);
                workingDist += thisDist;
            }
            return (workingDist) / totalDist;
        }
        return 0;

    }

    public TrafAIMotor CreaMacchinaTraffico(int id, int subId, float distance, List<RoadGraphEdge> percorso)
    {
        //id e subId stabiliscono la entry
        //distance da che distanza dall'inizio della entry la macchina deve apparire
        //percorso indica il percorso che la macchina deve seguire, dopo che l'ha terminato, l'auto si distrugge
        TrafEntry entry = system.GetEntry(id, subId);

        if (entry == null)
            return null;
        InterpolatedPosition pos = entry.GetInterpolatedPosition(distance);


        if (!Physics.CheckSphere(pos.position, checkRadius, 1 << LayerMask.NameToLayer("Traffic")))
        {
            GameObject go = GameObject.Instantiate(prefabs[UnityEngine.Random.Range(0, prefabs.Length)], pos.position, Quaternion.identity) as GameObject;
            TrafAIMotor motor = go.GetComponent<TrafAIMotor>();


            motor.currentIndex = pos.targetIndex;
            motor.currentEntry = entry;
            go.transform.LookAt(entry.waypoints[pos.targetIndex]);
            motor.system = system;
            motor.fixedRoute = true;
            motor.fixedPath = percorso;
            motor.maxSpeed = 11f; //40km/h
            motor.sterzataMassima = true;
            motor.Init();

            if (OnSpawnHeaps != null)
            {
                OnSpawnHeaps(go);
            }

            return motor;
        }
        return null;
    }

    public TrafAIMotor CreaScooter(int id, int subId, float distance, List<RoadGraphEdge> percorso)
    {
        //id e subId stabiliscono la entry
        //distance da che distanza dall'inizio della entry la macchina deve apparire
        //percorso indica il percorso che la macchina deve seguire, dopo che l'ha terminato, l'auto si distrugge
        TrafEntry entry = system.GetEntry(id, subId);

        if (entry == null)
            return null;
        InterpolatedPosition pos = entry.GetInterpolatedPosition(distance);


        if (!Physics.CheckSphere(pos.position, checkRadius, 1 << LayerMask.NameToLayer("Traffic")))
        {
            GameObject go = GameObject.Instantiate(prefabScooter, pos.position, Quaternion.identity) as GameObject;
            TrafAIMotor motor = go.GetComponent<TrafAIMotor>();
            

            motor.currentIndex = pos.targetIndex;
            motor.currentEntry = entry;
            go.transform.LookAt(entry.waypoints[pos.targetIndex]);
            motor.system = system;
            motor.fixedRoute = true;
            motor.fixedPath = percorso;
            motor.maxSpeed = 11f; //40km/h
            motor.Init();

            if (OnSpawnHeaps != null)
            {
                OnSpawnHeaps(go);
            }

            return motor;
        }
        return null;
    }


    public AutoTrafficoNoRayCast CreaMacchinaTrafficoNoRaycast(int id, int subId, float distance, List<RoadGraphEdge> percorso)
    {
        //id e subId stabiliscono la entry
        //distance da che distanza dall'inizio della entry la macchina deve apparire
        //percorso indica il percorso che la macchina deve seguire, dopo che l'ha terminato, l'auto si distrugge
        TrafEntry entry = system.GetEntry(id, subId);

        if (entry == null)
            return null;
        InterpolatedPosition pos = entry.GetInterpolatedPosition(distance);


        if (!Physics.CheckSphere(pos.position, checkRadius, 1 << LayerMask.NameToLayer("Traffic")))
        {
            GameObject go = GameObject.Instantiate(prefabMacchinaOstacolo, pos.position, Quaternion.identity) as GameObject;
            go.GetComponent<TrafAIMotor>().enabled = false;
            AutoTrafficoNoRayCast motor = go.AddComponent<AutoTrafficoNoRayCast>();
            motor.nose = go.GetComponent<TrafAIMotor>().nose;
            motor.raycastOrigin = go.GetComponent<TrafAIMotor>().raycastOrigin;

            motor.currentIndex = pos.targetIndex;
            motor.currentEntry = entry;
            go.transform.LookAt(entry.waypoints[pos.targetIndex]);
            motor.system = system;
            motor.fixedRoute = true;
            motor.sterzataMassima = true;
            motor.fixedPath = percorso;
            motor.Init();

            if (OnSpawnHeaps != null)
            {
                OnSpawnHeaps(go);
            }

            return motor;
        }
        return null;
    }


    public TrafAIMotor CreaMacchinaTrafficoInchiodata(int id, int subId, float distance, List<RoadGraphEdge> percorso)
    {
        //id e subId stabiliscono la entry
        //distance da che distanza dall'inizio della entry la macchina deve apparire
        //percorso indica il percorso che la macchina deve seguire, dopo che l'ha terminato, l'auto si distrugge
        TrafEntry entry = system.GetEntry(id, subId);

        if (entry == null)
            return null;
        InterpolatedPosition pos = entry.GetInterpolatedPosition(distance);


        if (!Physics.CheckSphere(pos.position, checkRadius, 1 << LayerMask.NameToLayer("Traffic")))
        {
            GameObject go = GameObject.Instantiate(prefabs[UnityEngine.Random.Range(0, prefabs.Length)], pos.position, Quaternion.identity) as GameObject;
            //MacchinaTrafficoInchiodata motor = go.AddComponent<MacchinaTrafficoInchiodata>();
            //motor.nose = go.GetComponent<TrafAIMotor>().nose;
            //motor.raycastOrigin = go.GetComponent<TrafAIMotor>().raycastOrigin;
            TrafAIMotor motor = go.GetComponent<TrafAIMotor>();
            motor.macchinaTrafficoInchiodata = true;

            motor.currentIndex = pos.targetIndex;
            motor.currentEntry = entry;
            go.transform.LookAt(entry.waypoints[pos.targetIndex]);
            motor.system = system;
            motor.fixedRoute = true;
            motor.sterzataMassima = true;
            motor.fixedPath = percorso;
            motor.Init();

            if (OnSpawnHeaps != null)
            {
                OnSpawnHeaps(go);
            }

            return motor;
        }
        return null;
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
            go = GameObject.Find("TeslaModelS_2_Rigged");
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
        if (go == null)
        {
            go = GameObject.Find("TeslaModelS_MOD");
        }
        if (go == null)
        {
            go = GameObject.Find("TeslaModelS_MOD(Clone)");
        }
        return go;
    }

    private List<Vector3> ottieniListaWaypoint()
    {
        GameObject go = ottieniRiferimentoPlayer();
        TrafAIMotor traf = go.GetComponent<TrafAIMotor>();
        List<Vector3> listaWaypoint = traf.currentEntry.waypoints;
        //Debug.Log("waypont:");
        //foreach (Vector3 wp in listaWaypoint)
        //{
        //    Debug.Log(wp.x + "," + wp.y + "," + wp.z);
        //}
        return listaWaypoint;
    }

    private void SetLayer(int newLayer, Transform trans)
    {
        //cambia il layer del gameobject e di tutti i suoi figli
        trans.gameObject.layer = newLayer;
        foreach (Transform child in trans)
        {
            if (!child.gameObject.name.Equals("Cube") && !child.gameObject.name.Equals("TrafLineRenderer"))
            {
                child.gameObject.layer = newLayer;
            }            
            if (child.gameObject.name.Equals("Body1") && newLayer == 12)
            {
                child.gameObject.tag = "Obstacle";
            }
            if (child.childCount > 0)
            {
                SetLayer(newLayer, child.transform);
            }
        }
    }
}


