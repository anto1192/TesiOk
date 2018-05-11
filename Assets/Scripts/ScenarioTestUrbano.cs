using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;

public class ScenarioTestUrbano : MonoBehaviour
{

    static ScenarioTestUrbano scenario;
    private TrafSystem system;
    private GameObject[] prefabs;
    private TrafAIMotor macchinaTraffico;
    private static ScenarioTestUrbano singleton = new ScenarioTestUrbano();
    public static ScenarioTestUrbano getInstance()
    {
        return singleton;
    }

    public ScenarioTestUrbano() {}

    /*Ambiente urbano:

    Guida automatica, senza traffico, prima semaforo verde, poi rosso, poi arancione(mi fermo).
    Attivazione traffico, semaforo verde, poi rosso, poi arancione(continuo).
    Compare una macchina davanti lenta, seguo la distanza di sicurezza, poi accelera, io accelero di conseguenza.
    Prossimo semaforo -> rosso, la macchina davanti a me si ferma, io mi fermo.
    Partiamo, lei prende un'altra direzione.
    Arrivo ad un semaforo(rosso), un pedone attraversa.
    Riparto ed arrivo ad un incrocio complicato in cui piu macchine vanno nella stessa direzione.
    Mi fermo allo stop, un pedone attraversa.
    Al prossimo semaforo (verde), il pedone attraversa (non dovrebbe), io lo vedo gia da una certa distanza, ma devo essere nella situazione in cui devo inchiodare.
    Mentre guido, un cane attraversa improvvisamente, non ce la faccio a fermarmi, lo evito.*/

    public ScenarioTestUrbano(TrafSystem traf, GameObject[] prefabs)
    {
        ScenarioTestUrbano.getInstance().system = traf;
        ScenarioTestUrbano.getInstance().prefabs = prefabs;
        this.system = traf;


        

        List<RoadGraphEdge> percorsoPlayer = ottieniPercorsoPlayer();

        //ottengo il riferimento alla macchina utente
        GameObject go = ottieniRiferimentoPlayer();

        attivaGuidaAutomatica(go, percorsoPlayer, percorsoPlayer[0].id, percorsoPlayer[0].subId); //-> facciamo apparire la macchina utente nella posizione con id e subId specificati, che rappresenta il punto di partenza
                                                         //la macchina seguirà il percorso definito in percorsoPlayer

        TrafAIMotor motor = go.GetComponent<TrafAIMotor>();
        motor.ChangeProperty += new TrafAIMotor.Delegato(gestisciEvento);
        motor.modificaSemaforo += new TrafAIMotor.Delegato(gestisciSemaforo);

    }

    
    public static void gestisciSemaforo(int idPrecedente, int nuovoId)
    {
        Debug.Log("Sono in gestisciSemaforo");
        string nomeMetodo = "semaforo" + nuovoId;
        //ScenarioTestUrbano.getInstance().Invoke(nomeMetodo, 2f);
        MethodInfo mi = ScenarioTestUrbano.getInstance().GetType().GetMethod(nomeMetodo);
        if (mi != null)
        {
            mi.Invoke(ScenarioTestUrbano.getInstance(), null);
        }
        


    }

    public static void gestisciEvento(int idPrecedente, int nuovoId)
    {
        string nomeMetodo = "evento" + nuovoId;
        MethodInfo mi = ScenarioTestUrbano.getInstance().GetType().GetMethod(nomeMetodo);
        Debug.Log("Sono in gestisciEvento " + nomeMetodo + "; mi == null? " + mi == null);
        if (mi != null)
        {
            mi.Invoke(ScenarioTestUrbano.getInstance(), null);
        }
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
            yield return new WaitForSeconds(3f);
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
            yield return new WaitForSeconds(4f);
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
            yield return new WaitForSeconds(11f);
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


        List<RoadGraphEdge> percorso13_0 = ottieniPercorso13_0();
        List<RoadGraphEdge> percorso13_1 = ottieniPercorso13_1();
        List<RoadGraphEdge> percorso13_2 = ottieniPercorso13_2();
        List<RoadGraphEdge> percorso13_3 = ottieniPercorso13_3();
        List<RoadGraphEdge> percorso13_4 = ottieniPercorso13_4();
        List<RoadGraphEdge> percorso13_5 = ottieniPercorso13_5();
        CreaMacchinaTraffico(11, 2, 0f, percorso13_0);
        CreaMacchinaTraffico(11, 2, 0.3f, percorso13_1);
        CreaMacchinaTraffico(12, 0, 0f, percorso13_2);
        CreaMacchinaTraffico(11, 3, 0.7f, percorso13_3);
        CreaMacchinaTraffico(12, 1, 0.6f, percorso13_4);
        CreaMacchinaTraffico(12, 0, 0.8f, percorso13_5);
    }

    public void semaforo14a()
    {
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

    public void semaforo16()
    {
        TrafEntry entry = system.GetEntry(1049, 5);
        TrafEntry entry1 = system.GetEntry(1049, 0);
        TrafficLightContainer container = entry.light;
        TrafficLightContainer container1 = entry1.light;
        TrafficLight[] lights = container.gameObject.GetComponentsInParent<TrafficLight>();
        lights[0].StopAllCoroutines();
        lights[0].StartCoroutine(courutineSemaforoRosso(container));
        lights[0].StartCoroutine(courutineSemaforoVerde(container1));
        evento16a();
    }

    public void semaforo16a()
    {
        TrafEntry entry = system.GetEntry(1049, 5);
        TrafEntry entry1 = system.GetEntry(1049, 0);
        TrafficLightContainer container = entry.light;
        TrafficLightContainer container1 = entry1.light;
        TrafficLight[] lights = container.gameObject.GetComponentsInParent<TrafficLight>();
        lights[0].StopAllCoroutines();
        lights[0].StartCoroutine(courutineSemaforoRosso(container));
        lights[0].StartCoroutine(courutineSemaforoVerde(container1));
        evento16a();
    }

    public void semaforo5()
    {
        TrafEntry entry = system.GetEntry(1003, 7);
        TrafEntry entry1 = system.GetEntry(1003, 8);
        TrafficLightContainer container = entry.light;
        TrafficLightContainer container1 = entry1.light;
        TrafficLight[] lights = container.gameObject.GetComponentsInParent<TrafficLight>();
        lights[0].StopAllCoroutines();
        lights[0].StartCoroutine(courutineSemaforoArancioneMiFermo(container, container1));

        TrackController.Instance.TriggerObstacle1();

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
        lights[0].StartCoroutine(courutineSemaforoVerde(container));
        lights[0].StartCoroutine(courutineSemaforoRosso(container1));
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
        semaforo159b();

        List<RoadGraphEdge> percorso159_5 = ottieniPercorso159_5();
        List<RoadGraphEdge> percorso159_0 = ottieniPercorso159_0();

        CreaMacchinaTraffico(158, 2, 0f, percorso159_0);

        TrafAIMotor macchinaTrafficoScorretta = CreaMacchinaTraffico(162, 0, 0.899f, percorso159_5); //macchinaTraffico che non rispetta il semaforo e va piu veloce
        //macchinaTrafficoScorretta.maxSpeed = 15f;
        macchinaTrafficoScorretta.autoScorretta = true;
        SetLayer(12, macchinaTrafficoScorretta.transform);


       
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
        lights[0].StartCoroutine(courutineSemaforoVerde(container));
        lights[0].StartCoroutine(courutineSemaforoRosso(container1));
        lights[0].StartCoroutine(courutineSemaforoRosso(container2));
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
        lights[0].StartCoroutine(courutineSemaforoRosso(container));
        lights[0].StartCoroutine(courutineSemaforoVerde(container1));
        lights[0].StartCoroutine(courutineSemaforoVerde(container2));
    }


    public void evento1089()
    {
        List<Vector3> listaWaypoint = ottieniListaWaypoint();
        Vector3 wp3Ok = listaWaypoint[3];
        wp3Ok.x = 2223f;
        wp3Ok.z = 188.2f;
        listaWaypoint[3] = wp3Ok;
        
        ottieniRiferimentoPlayer().GetComponent<TrafAIMotor>().currentEntry.waypoints = listaWaypoint;
    }


    public void evento39()
    {
        List<RoadGraphEdge> percorso39_0 = ottieniPercorso39_0();
        List<RoadGraphEdge> percorso39_1 = ottieniPercorso39_1();
        CreaMacchinaTraffico(45, 1, 0, percorso39_0);
        CreaMacchinaTraffico(45, 0, 0.5f,percorso39_1);
    }

    public void evento1082()
    {
        List<Vector3> listaWaypoint = ottieniListaWaypoint();
        for(int i = 0; i < listaWaypoint.Count; i++)
        {
            Vector3 wp = listaWaypoint[i];
            wp.z = 191f;
            listaWaypoint[i] = wp;
        }

        ottieniRiferimentoPlayer().GetComponent<TrafAIMotor>().currentEntry.waypoints = listaWaypoint;
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
        List<RoadGraphEdge> percorso50_1 = ottieniPercorso50_1();
        List<RoadGraphEdge> percorso50_2 = ottieniPercorso50_2();
        List<RoadGraphEdge> percorso50_3 = ottieniPercorso50_3();
        CreaMacchinaTraffico(51, 0, 0.7f, percorso50_0);
        CreaMacchinaTraffico(51, 1, 0.2f, percorso50_1);
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
        CreaMacchinaTraffico(65, 3, 0f, percorso64_3);
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
        ScenarioTestUrbano.getInstance().macchinaTraffico = CreaMacchinaTraffico(25, 3, 0.3f, percorso65_0);
        CreaMacchinaTraffico(25, 2, 0f, percorso65_1);
        CreaMacchinaTraffico(25, 1, 0.4f, percorso65_2);
        CreaMacchinaTraffico(25, 0, 0.6f, percorso65_3);
        CreaMacchinaTraffico(25, 3, 0f, percorso65_4);
        CreaMacchinaTraffico(25, 3, 0.8f, percorso65_4);
    }

    public void evento13()
    {
        ScenarioTestUrbano.getInstance().macchinaTraffico.maxSpeed = 5f;
    }

    public void evento14()
    {
        semaforo14a();
        List<RoadGraphEdge> percorso14_0 = ottieniPercorso14_0();
        List<RoadGraphEdge> percorso14_1 = ottieniPercorso14_1();
        List<RoadGraphEdge> percorso14_2 = ottieniPercorso14_2();
        CreaMacchinaTraffico(20, 3, 0.3f, percorso14_0);
        CreaMacchinaTraffico(20, 2, 0.6f, percorso14_1);
        CreaMacchinaTraffico(20, 1, 0.8f, percorso14_2);

    }

    public void evento15()
    {
        semaforo15a();
        ScenarioTestUrbano.getInstance().macchinaTraffico.maxSpeed = 11f;
        List<RoadGraphEdge> percorso15_0 = ottieniPercorso15_0();
        List<RoadGraphEdge> percorso15_1 = ottieniPercorso15_1();
        CreaMacchinaTraffico(3, 3, 0.4f, percorso15_0);
        CreaMacchinaTraffico(3, 0, 0.6f, percorso15_1); //QUESTA SCOMPARE!!! ATTENZIONE
    }

    public void evento16()
    {
        semaforo16a();
    }

    public void evento16a()
    {
        List<RoadGraphEdge> percorso16_0 = ottieniPercorso16_0();
        List<RoadGraphEdge> percorso16_1 = ottieniPercorso16_1();
        List<RoadGraphEdge> percorso16_2 = ottieniPercorso16_2();
        List<RoadGraphEdge> percorso16_3 = ottieniPercorso16_3();
        List<RoadGraphEdge> percorso16_4 = ottieniPercorso16_4();
        CreaMacchinaTraffico(17, 1, 0.4f, percorso16_0);
        CreaMacchinaTraffico(17, 3, 0.2f, percorso16_1);
        CreaMacchinaTraffico(17, 0, 0f, percorso16_2);
        CreaMacchinaTraffico(17, 0, 0.1f, percorso16_3);
        CreaMacchinaTraffico(17, 0, 0.5f, percorso16_4);
    }

    public IEnumerator attesa18()
    {
        Debug.Log("sono in courutine");
        //yield return new WaitForSeconds(16f);
        yield return new WaitForSeconds(25f);
        Debug.Log("Fine attesa");
        evento18a();
    }

    public void evento18()
    {       
        if (ScenarioTestUrbano.getInstance().macchinaTraffico != null)
        {
            ScenarioTestUrbano.getInstance().macchinaTraffico.maxSpeed = 3f;
        } else
        {
            ottieniRiferimentoPlayer().GetComponent<TrafAIMotor>().maxSpeed = 3f;
        }

        List<RoadGraphEdge> percorso18_0 = ottieniPercorso18_0();
        List<RoadGraphEdge> percorso18_1 = ottieniPercorso18_1();
        CreaMacchinaTraffico(17, 0, 0.5f, percorso18_0);
        CreaMacchinaTraffico(17, 0, 0.3f, percorso18_1);


        TrafEntry entry = system.GetEntry(1049, 5);
        TrafficLightContainer container = entry.light;
        TrafficLight[] lights = container.gameObject.GetComponentsInParent<TrafficLight>();
        lights[0].StartCoroutine(attesa18());      
    }

    public void evento18a()
    {
        GameObject go = ottieniRiferimentoPlayer();
        if (go.GetComponent<TrafAIMotor>().maxSpeed != 11f)
        {
            go.GetComponent<TrafAIMotor>().maxSpeed = 11f;
        }

        TrafEntry entry = system.GetEntry(18, 0);
        Vector3 puntoArrivo = go.GetComponent<TrafAIMotor>().target;
        puntoArrivo.x = 865.9f;
        entry.waypoints[go.GetComponent<TrafAIMotor>().currentIndex] = puntoArrivo;
        go.GetComponent<TrafAIMotor>().target = puntoArrivo;
        Debug.DrawLine(go.transform.position, entry.waypoints[go.GetComponent<TrafAIMotor>().currentIndex]);

        go.GetComponent<TrafAIMotor>().currentEntry = entry;
    }

    public void evento1007()
    {
        List<Vector3> listaWaypoint = ottieniListaWaypoint();
        Vector3 wp3Ok = listaWaypoint[3];
        wp3Ok.x = 866.41f;
        wp3Ok.z = 380f;
        listaWaypoint[3] = wp3Ok;

        ottieniRiferimentoPlayer().GetComponent<TrafAIMotor>().currentEntry.waypoints = listaWaypoint;
    }

    public void evento5()
    {
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
        CreaMacchinaTraffico(2, 2, 0.15f, percorso5_3);
        CreaMacchinaTraffico(2, 3, 0.5f, percorso5_4);
        CreaMacchinaTraffico(2, 3, 0f, percorso5_5);
        CreaMacchinaTraffico(2, 3, 0.4f, percorso5_6);
        CreaMacchinaTraffico(2, 3, 0.7f, percorso5_7);
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
        CreaMacchinaTraffico(15, 3, 0.15f, percorso3_1);
        CreaMacchinaTraffico(15, 3, 0.3f, percorso3_2);
        CreaMacchinaTraffico(15, 3, 0.45f, percorso3_3);
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

    }

    public void evento125()
    {
        semaforo4b();
        semaforo125a();
        List<RoadGraphEdge> percorso125_0 = ottieniPercorso125_0();
        List<RoadGraphEdge> percorso125_1 = ottieniPercorso125_1();
        List<RoadGraphEdge> percorso125_2 = ottieniPercorso125_2();
        List<RoadGraphEdge> percorso125_3 = ottieniPercorso125_3();
        List<RoadGraphEdge> percorso125_4 = ottieniPercorso125_4();

        CreaMacchinaTraffico(160, 0, 0f, percorso125_0);
        CreaMacchinaTraffico(160, 1, 0.15f, percorso125_1);
        CreaMacchinaTraffico(160, 1, 0.3f, percorso125_2);

        CreaMacchinaTraffico(159, 3, 0f, percorso125_3);
        CreaMacchinaTraffico(159, 2, 0.15f, percorso125_4);

        CreaMacchinaTraffico(160, 0, 0.45f, percorso125_0);
        CreaMacchinaTraffico(160, 1, 0.6f, percorso125_1);
        CreaMacchinaTraffico(160, 1, 0.75f, percorso125_2);

        CreaMacchinaTraffico(159, 3, 0.45f, percorso125_3);
        CreaMacchinaTraffico(159, 2, 0.6f, percorso125_4);
    }

    bool secondaVolta = false;

    public void evento1008()
    {
        if (secondaVolta)
        {
            return;
        }
        List<Vector3> listaWaypoint = ottieniListaWaypoint();
        Vector3 wp3Ok = listaWaypoint[3];
        wp3Ok.x = 1162.84f;
        wp3Ok.z = 950.3f;
        listaWaypoint[3] = wp3Ok;

        ottieniRiferimentoPlayer().GetComponent<TrafAIMotor>().currentEntry.waypoints = listaWaypoint;
    }

    TrafAIMotor macchinaTrafficoScorrettaDavanti = null;

    public void evento159()
    {
        if (secondaVolta)
        {
            evento159a();
            return;
        }
        secondaVolta = true;
        semaforo159b();
        List<RoadGraphEdge> percorso159_0 = ottieniPercorso159_0();
        List<RoadGraphEdge> percorso159_1 = ottieniPercorso159_1();
        List<RoadGraphEdge> percorso159_2 = ottieniPercorso159_2();
        List<RoadGraphEdge> percorso159_3 = ottieniPercorso159_3();
        List<RoadGraphEdge> percorso159_4 = ottieniPercorso159_4();
        macchinaTrafficoScorrettaDavanti = CreaMacchinaTraffico(162, 3, 0f, percorso159_4);
        macchinaTrafficoScorrettaDavanti.autoScorretta = true;
        macchinaTrafficoScorrettaDavanti.maxSpeed = 7f;
        SetLayer(12, macchinaTrafficoScorrettaDavanti.transform);


        List<RoadGraphEdge> percorso159_6 = ottieniPercorso159_6();

        CreaMacchinaTraffico(158, 2, 0f, percorso159_0);
        CreaMacchinaTraffico(158, 2, 0.1f, percorso159_1);
        //CreaMacchinaTraffico(158, 2, 0.7f, percorso159_2);
        CreaMacchinaTraffico(158, 3, 0.9f, percorso159_3);

        
        
        CreaMacchinaTraffico(162, 1, 0.3f, percorso159_6);
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
        CreaMacchinaTraffico(22, 2, 0.3f, percorso163_2);
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
        traf.currentEntry.waypoints = listaWaypoint;

        if (macchinaTrafficoScorrettaDavanti != null)
        {
            //risetto alla macchina scorretta che ci ritroviamo davanti il livello "traffic", altrimenti verrà sempre considerata un ostacolo
            SetLayer(8, macchinaTrafficoScorrettaDavanti.transform);
        }

    }

    
    public void evento22()
    {
        TrackController.Instance.LanciaOstacolo(10);
        List<RoadGraphEdge> percorso22_0 = ottieniPercorso22_0();
        List<RoadGraphEdge> percorso22_1 = ottieniPercorso22_1();
        List<RoadGraphEdge> percorso22_2 = ottieniPercorso22_2();
        List<RoadGraphEdge> percorso22_3 = ottieniPercorso22_3();
        List<RoadGraphEdge> percorso22_4 = ottieniPercorso22_4();
        List<RoadGraphEdge> percorso22_5 = ottieniPercorso22_5();
        List<RoadGraphEdge> percorso22_6 = ottieniPercorso22_6();
        List<RoadGraphEdge> percorso22_7 = ottieniPercorso22_7();

        CreaMacchinaTraffico(12, 2, 0.5f, percorso22_0);
        //CreaMacchinaTraffico(12, 2, 0.15f, percorso22_1);
        //CreaMacchinaTraffico(12, 3, 0.3f, percorso22_2);
        //CreaMacchinaTraffico(21, 2, 0.45f, percorso22_3);
        CreaMacchinaTraffico(21, 3, 0.3f, percorso22_4);
        //CreaMacchinaTraffico(30, 0, 0.75f, percorso22_5);
        //CreaMacchinaTraffico(30, 1, 0f, percorso22_6);
        //CreaMacchinaTraffico(30, 0, 0f, percorso22_7);
    }

    public void evento30()
    {
        semaforo30a();
        TrackController.Instance.LanciaOstacolo(11);
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
    }

    public void evento159a()
    {
        semaforo159_2();
        TrackController.Instance.LanciaOstacolo(12);        
    }

   /* public void evento1011()
    {
        GameObject go = ottieniRiferimentoPlayer();
        TrafAIMotor traf = go.GetComponent<TrafAIMotor>();
        List<Vector3> listaWaypoint = traf.currentEntry.waypoints;
        int contatore = 0;
        foreach (Vector3 wp in listaWaypoint)
        {
            contatore++;
            Debug.Log("waypoint " + contatore + ": " + wp.x + "," + wp.y + "," + wp.z);
        }
        List<Vector3> listaWaypointOk = new List<Vector3>();
        listaWaypointOk.Add(listaWaypoint[0]);
        Vector3 nuovoWaypoint = listaWaypoint[3];
        nuovoWaypoint.x = 1328f;
        listaWaypointOk.Add(nuovoWaypoint);
        traf.currentEntry.waypoints = listaWaypointOk;
    }
    */
    public void evento9999()
    {
        Debug.Log("Test interrotto!!!!");       
        fineGuidaAutomatica();
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
        edge14.subId = 2;

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
        //percorso.Add(edge2a);
        percorso.Add(edge3);
        //percorso.Add(edge3a);
        percorso.Add(edge4);
        //percorso.Add(edge4a);
        percorso.Add(edge5);
        //percorso.Add(edge5a);
        percorso.Add(edge6);
        //percorso.Add(edge6a);
        percorso.Add(edge7);
        //percorso.Add(edge7a);
        percorso.Add(edge8);
        //percorso.Add(edge8a);
        percorso.Add(edge9);
        //percorso.Add(edge9a);
        percorso.Add(edge10);
        //percorso.Add(edge10a);
        percorso.Add(edge11);
        //percorso.Add(edge11a);
        percorso.Add(edge12);
        //percorso.Add(edge12a);
        percorso.Add(edge13);
        //percorso.Add(edge13a);
        percorso.Add(edge14);
        //percorso.Add(edge14a);
        percorso.Add(edge15);
        //percorso.Add(edge15a);
        percorso.Add(edge16);
        //percorso.Add(edge16a);
        percorso.Add(edge17);
        //percorso.Add(edge17a);
        percorso.Add(edge18);
        //percorso.Add(edge18a);
        percorso.Add(edge19);
        //percorso.Add(edge19a);
        percorso.Add(edge20);
        //percorso.Add(edge20a);
        percorso.Add(edge21);
        //percorso.Add(edge21a);
        percorso.Add(edge22);
        //percorso.Add(edge22a);
        percorso.Add(edge23);
        //percorso.Add(edge23a);
        percorso.Add(edge24);
        //percorso.Add(edge24a);
        percorso.Add(edge25);

        return percorso;
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

        edge1.id = 1036;
        edge1.subId = 0;

        edge2.id = 64;
        edge2.subId = 3;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);

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
        edge6.subId = 2;

        edge7.id = 19;
        edge7.subId = 2;


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
        edge2.subId = 1;

        edge3.id = 15;
        edge3.subId = 1;

        edge4.id = 16;
        edge4.subId = 1;

        edge5.id = 18;
        edge5.subId = 1;

        edge6.id = 19;
        edge6.subId = 1;


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
        edge2.subId = 1;

        edge3.id = 16;
        edge3.subId = 1;

        edge4.id = 18;
        edge4.subId = 1;

        edge5.id = 19;
        edge5.subId = 1;


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
        edge4.subId = 1;


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
        edge2.subId =1;



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
        edge2.subId =3;

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

        edge3.id = 22;
        edge3.subId = 0;

        edge4.id = 21;
        edge4.subId = 0;


        List<RoadGraphEdge> percorso = new List<RoadGraphEdge>();
        percorso.Add(edge1);
        percorso.Add(edge2);
        percorso.Add(edge3);
        percorso.Add(edge4);

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
        //guidaAutomatica = false;
    }

    private void attivaGuidaAutomatica(GameObject go, List<RoadGraphEdge> percorso, int id, int subId)
    {


        float distance = 0;


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
            //nose.transform.localPosition = new Vector3(0, 0.5f, 2f); ->per la jaguar
            nose.transform.localPosition = new Vector3(0, 0.5f, 2.61f);
            nose.transform.localRotation = Quaternion.identity;
                nose.transform.localScale = new Vector3(2f, 2f, 2f);


                TrafAIMotor motor = go.AddComponent<TrafAIMotor>();
            go.GetComponent<VehicleInputController>().enabled = false;

            GuidaManuale guidaManuale = go.AddComponent<GuidaManuale>();
            guidaManuale.setMotor(motor);


            GameObject colliderOstacoli = new GameObject("colliderOstacoli");
                colliderOstacoli.transform.SetParent(go.transform);
                BoxCollider boxColliderOstacoli = colliderOstacoli.AddComponent<BoxCollider>();
                boxColliderOstacoli.isTrigger = true;
                colliderOstacoli.transform.localPosition = new Vector3(0f, 0.65f, 7f);
                colliderOstacoli.transform.localScale = new Vector3(1.5f, 1f, 1f);
                colliderOstacoli.transform.localRotation = Quaternion.identity;
                boxColliderOstacoli.size = new Vector3(3f, 0.75f, 10f);
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

                Debug.Log("pos.position: x = " + pos.position.x + "; y = " + pos.position.y + "; z = " + pos.position.z + "; targetIndex = " + pos.targetIndex);
                //guidaAutomatica = true;

            List<RoadGraphEdge> percorsoPlayer = percorso.GetRange(1, percorso.Count - 1);

                motor.fixedRoute = true;              
                motor.fixedPath = percorsoPlayer;

                motor.Init();


            }
            else
            {
                attivaGuidaAutomatica(go, percorso, id, subId); //è una ricorsione, fa si che si ripete la funzione finchè tutto vada bene
            }

        
    }

    /*
    private void guidaAutomaticaSF()
    {
        Debug.Log("sono in guidaAtuomaticaSF");
        //Vector3 tempPos = transform.position;
        int id = Random.Range(0, maxIdent);
        int subId = Random.Range(0, maxSub);
        //float distance = Random.value * 0.8f + 0.1f;



        GameObject go = ottieniRiferimentoPlayer();
        TrackController trackController = TrackController.Instance;       
        TrafEntry entry = trackController.GetCurrentTrafEntry(); //-> ottengo la entry corrente
        //TrafEntry entryOk = null;


        //lo scopo della funziona calcolaDistanza è settare la variabile distance che farà si che verrà settato correttamente il waypoint target
        float distance = calcolaDistanza(entry, go);


        //entry = system.GetEntry(id, subId); //-> serve a ottenere la entry di un determinato punto, bisogna indicare il giusto id e subid

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
            nose.transform.localPosition = new Vector3(0, 0.5f, 2f);
            nose.transform.localRotation = Quaternion.identity;
            nose.transform.localScale = new Vector3(2f, 2f, 2f);


            TrafAIMotor motor = go.AddComponent<TrafAIMotor>();


            GameObject colliderOstacoli = new GameObject("colliderOstacoli");
            colliderOstacoli.transform.SetParent(go.transform);
            BoxCollider boxColliderOstacoli = colliderOstacoli.AddComponent<BoxCollider>();
            boxColliderOstacoli.isTrigger = true;
            colliderOstacoli.transform.localPosition = new Vector3(0f, 0.65f, 7f);
            colliderOstacoli.transform.localScale = new Vector3(1f, 1f, 1f);
            colliderOstacoli.transform.localRotation = Quaternion.identity;
            boxColliderOstacoli.size = new Vector3(3f, 0.75f, 10f);
            TrafAIMotor.GestoreCollisioni gestore = colliderOstacoli.AddComponent<TrafAIMotor.GestoreCollisioni>();
            gestore.setMotor(motor);


            //L'ISTRUZIONE SOTTO FA TELETRASPORTARE L'AUTO NELLA POSIZIONE pos.position
            //go.transform.position = pos.position;

            //go.AddComponent<TrafWheels>();

            motor.currentIndex = pos.targetIndex;
            motor.currentEntry = entry;

            //L'istruzione sotto ruota la macchina in direzione del waypoint target
            //go.transform.LookAt(entry.waypoints[pos.targetIndex]);

            motor.system = system;
            motor.nose = nose;
            motor.raycastOrigin = nose.transform;
            motor.targetHeight = 0f;
            motor.waypointThreshold = 3f;

            Debug.Log("pos.position: x = " + pos.position.x + "; y = " + pos.position.y + "; z = " + pos.position.z + "; targetIndex = " + pos.targetIndex);
            guidaAutomatica = true;





            motor.Init();


        }
        else
        {
            guidaAutomaticaSF(); //è una ricorsione, fa si che si ripete la funzione finchè tutto vada bene
        }

    }
*/
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
            GameObject go = GameObject.Instantiate(prefabs[Random.Range(0, prefabs.Length)], pos.position, Quaternion.identity) as GameObject;
            TrafAIMotor motor = go.GetComponent<TrafAIMotor>();
            go.layer = 16;


            motor.currentIndex = pos.targetIndex;
            motor.currentEntry = entry;
            go.transform.LookAt(entry.waypoints[pos.targetIndex]);
            motor.system = system;
            motor.fixedRoute = true;
            motor.fixedPath = percorso;
            motor.Init();
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
            go = GameObject.Find("TeslaModelS_2_Rigged (1)");
        }
        if (go == null)
        {
            go = GameObject.Find("TeslaModelS_2_Rigged (1)(Clone)");
        }
        return go;
    }
    
    private List<Vector3> ottieniListaWaypoint()
    {
        GameObject go = ottieniRiferimentoPlayer();
        TrafAIMotor traf = go.GetComponent<TrafAIMotor>();
        List<Vector3> listaWaypoint = traf.currentEntry.waypoints;
        Debug.Log("waypont:");
        foreach (Vector3 wp in listaWaypoint)
        {
            Debug.Log(wp.x + "," + wp.y + "," + wp.z);
        }
        return listaWaypoint;
    }

    private void SetLayer(int newLayer, Transform trans)
    {
        //cambia il layer di un gameObject e di tutti i suoi figli
        trans.gameObject.layer = newLayer;
        foreach (Transform child in trans)
        {
            child.gameObject.layer = newLayer;
            if (child.childCount > 0)
            {
                SetLayer(newLayer, child.transform);
            }
        }
    }
}


