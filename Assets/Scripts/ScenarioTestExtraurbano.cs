
using System;
using System.Reflection;
using UnityEngine;

public class ScenarioTestExtraurbano : MonoBehaviour
{


    public CarAutoPath carAutoPath;
    public GameObject[] prefabs;
    private float checkRadius = 8f;
    public Transform posizionePartenza;
    private GameObject car;
    public bool scenarioAvviato = false;
    public GameObject rocciaTest;
    private bool fineTest = false;


    private GameObject[] macchinaTraffico = new GameObject[20];

    private CarExternalInputAutoPathAdvanced guidaAutomatica = null;

    void Start()
    {
        car = ottieniRiferimentoPlayer();
    }

    private void FixedUpdate()
    {
        if (!scenarioAvviato)
        {
            return;
        }
        if (fineTest)
        {
            //FERMO LA MACCHINA
            car.GetComponent<VehicleController>().accellInput = -0.5f;
        }
        if (fineTest && car.GetComponent<Rigidbody>().velocity.magnitude < 0.01f)
        {
            //l'auto è ferma
            scenarioAvviato = false;
            interrompiScenario();
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
            } else
            {
                scenarioAvviato = false;
                interrompiScenario();

            }

        }
    }

    private void avviaScenarioTest()
    {
        if (car == null)
        {
            car = ottieniRiferimentoPlayer();
        }

        car.transform.position = posizionePartenza.transform.position;
        car.transform.rotation = posizionePartenza.transform.rotation;
        car.GetComponent<xSimScript>().enabled = true;
    }

    public void iniziaTest()
    {

        rocciaTest.SetActive(true);
        AvviaGuidaAutomatica();
        /*GameObject macchinaTraffico0 = CreaMacchinaTraffico(1570);
        macchinaTraffico0.GetComponent<TrafPCH>().maxSpeed = 10f;*/
    }


    int id = 0;

    public void gestisciEvento(int idPrecedente, int nuovoId)
    {
        if (nuovoId == id)
        {
            return;
        }
        id = nuovoId;
        string nomeMetodo = "evento" + nuovoId;
        MethodInfo mi = this.GetType().GetMethod(nomeMetodo);
        if (mi != null)
        {
            mi.Invoke(this, null);
        }
    }





    //LISTA EVENTI

    //SBACCHETTAMENTI
    public void evento1590()
    {
        guidaAutomatica.sbacchettamento = true;
    }

    public void evento1600()
    {
        guidaAutomatica.sbacchettamento = false;
    }

    public void evento1635()
    {
        guidaAutomatica.sbacchettamento = true;
    }

    public void evento1650()
    {
        guidaAutomatica.sbacchettamento = false;
    }

    public void evento1720()
    {
        guidaAutomatica.sbacchettamento = true;
    }

    public void evento1730()
    {
        guidaAutomatica.sbacchettamento = false;
    }

    public void evento1795()
    {
        guidaAutomatica.sbacchettamento = true;
    }

    public void evento1810()
    {
        guidaAutomatica.sbacchettamento = false;
    }

    public void evento1838()
    {
        guidaAutomatica.sbacchettamento = true;
    }

    public void evento1859()
    {
        guidaAutomatica.sbacchettamento = false;
    }

    public void evento1875()
    {
        guidaAutomatica.sbacchettamento = true;
    }

    public void evento1880()
    {
        guidaAutomatica.sbacchettamento = false;
    }

    public void evento1907()
    {
        guidaAutomatica.sbacchettamento = true;
    }

    public void evento1912()
    {
        guidaAutomatica.sbacchettamento = false;
    }

    public void evento2050()
    {
        guidaAutomatica.sbacchettamento = true;
    }



    //EVENTI


    public void evento1565()
    {
        macchinaTraffico[0] = CreaMacchinaTraffico(1170);
        macchinaTraffico[1] = CreaMacchinaTraffico(1100);
    }


    public void evento1749()
    {
        macchinaTraffico[2] = CreaMacchinaTraffico(1022);
    }

    public void evento1680()
    {
        macchinaTraffico[3] = CreaMacchinaTraffico(1075); //questa macchina la evito appena per colpa del sasso
    }


    public void evento1702()
    {
        car.GetComponent<CarExternalInputAutoPathAdvanced>().maxSpeed = 8f;
    }

    public void evento1710()
    {
        car.GetComponent<CarExternalInputAutoPathAdvanced>().maxSpeed = 10f;
    }

    public void evento1712()
    {
        car.GetComponent<CarExternalInputAutoPathAdvanced>().maxSpeed = 15f;
    }

    public void evento1750()
    {
        car.GetComponent<CarExternalInputAutoPathAdvanced>().maxSpeed = 13f;
    }
    public void evento1755()
    {
        car.GetComponent<CarExternalInputAutoPathAdvanced>().maxSpeed = 11f;
        car.GetComponent<CarExternalInputAutoPathAdvanced>().limiteVelocita = 45f;
    }

    public void evento1820()
    {
        macchinaTraffico[4] = CreaMacchinaTraffico(952);
    }

    public void evento1850()
    {
        macchinaTraffico[5] = CreaMacchinaTraffico(922);
    }

    public void evento1860()
    {
        macchinaTraffico[6] = CreaMacchinaTraffico(911);
    }

    public void evento1890()
    {
        macchinaTraffico[6] = CreaMacchinaTraffico(885);
        macchinaTraffico[7] = CreaMacchinaTraffico(875);
       // macchinaTraffico[8] = CreaMacchinaTraffico(850);
    }

    GameObject macchinaTrafficoSorpasso;

    public void evento1891()
    {
        macchinaTraffico[8] = CreaMacchinaTraffico(865);
        macchinaTraffico[8].GetComponent<TrafPCH>().maxSpeed = 5f;
        macchinaTrafficoSorpasso = CreaMacchinaTrafficoSorpasso(822);
    }


    public void evento1935()
    {
        car.GetComponent<CarExternalInputAutoPathAdvanced>().maxSpeed = 15f;
        car.GetComponent<CarExternalInputAutoPathAdvanced>().limiteVelocita = 60f;
    }

    public void evento1923()
    {
        car.GetComponent<CarExternalInputAutoPathAdvanced>().maxSpeed = 5f;
    }

    public void evento1925()
    {
        car.GetComponent<CarExternalInputAutoPathAdvanced>().maxSpeed = 11f;
        macchinaTraffico[9] = CreaMacchinaTraffico(1966);
        macchinaTraffico[9].GetComponent<TrafPCH>().maxSpeed = 5f;
    }

    public void evento1980()
    {
        macchinaTraffico[9].GetComponent<TrafPCH>().maxSpeed = 3f;
        car.GetComponent<CarExternalInputAutoPathAdvanced>().maxSpeed = 10f;
        car.GetComponent<CarExternalInputAutoPathAdvanced>().limiteVelocita = 45f;
    }

    public void evento1990()
    {        
        macchinaTraffico[9].GetComponent<TrafPCH>().maxSpeed = 8f;
    }

    public void evento1995()
    {
        car.GetComponent<CarExternalInputAutoPathAdvanced>().maxSpeed = 10f;
        macchinaTraffico[9].GetComponent<TrafPCH>().maxSpeed = 10f;
    }

    public void evento2005()
    {
        car.GetComponent<CarExternalInputAutoPathAdvanced>().limiteVelocita = 70f;
    }

    public void evento2010()
    {
        car.GetComponent<CarExternalInputAutoPathAdvanced>().maxSpeed = 20f;
        macchinaTraffico[9].GetComponent<TrafPCH>().maxSpeed = 15f;
    }

    public void evento2011()
    {
        macchinaTraffico[9].GetComponent<TrafPCH>().maxSpeed = 16f;
    }

    public void evento2012()
    {
        macchinaTraffico[9].GetComponent<TrafPCH>().maxSpeed = 17f;
    }

    public void evento2013()
    {
        macchinaTraffico[9].GetComponent<TrafPCH>().maxSpeed = 18f;
    }

    public void evento2014()
    {
        macchinaTraffico[9].GetComponent<TrafPCH>().maxSpeed = 19f;
    }

    public void evento2015()
    {
        macchinaTraffico[9].GetComponent<TrafPCH>().maxSpeed = 20f;
        
    }

    public void evento2016()
    {
        macchinaTraffico[9].GetComponent<TrafPCH>().maxSpeed = 25f;
    }

    public void evento2055()
    {
        TrackController.Instance.TriggerObstacle1();
    }

    public void evento2065()
    {
        car.GetComponent<CarExternalInputAutoPathAdvanced>().maxSpeed = 10f;
    }

    public void evento2090()
    {
        Debug.Log("Fine test");
        car.GetComponent<xSimScript>().enabled = false;
        fineTest = true;      

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
        return go;
    }

    public GameObject CreaMacchinaTraffico(int indice)
    {
        int currentIndex = indice;
        RoadPathNode currentNode = carAutoPath.pathNodes[currentIndex];

        Vector3 position = currentNode.position;

        if (!Physics.CheckSphere(position, checkRadius, 1 << LayerMask.NameToLayer("Traffic")))
        {
            GameObject go = GameObject.Instantiate(prefabs[UnityEngine.Random.Range(0, prefabs.Length)], position, Quaternion.identity) as GameObject;

            currentNode = carAutoPath.pathNodes[currentIndex++];

            var motor = go.GetComponent<TrafPCH>();
            motor.currentNode = currentNode;
            motor.currentWaypointIndex = currentIndex;
            //            Debug.Log("IDX " + currentIndex);
            motor.path = carAutoPath;
            go.transform.LookAt(currentNode.position);
            motor.Init();
            return go;
        }
        return null;
    }

    public GameObject CreaMacchinaTrafficoSorpasso(int indice)
    {
        int currentIndex = indice;
        RoadPathNode currentNode = carAutoPath.pathNodes[currentIndex];
        Vector3 position = currentNode.position;
        if (!Physics.CheckSphere(position, checkRadius, 1 << LayerMask.NameToLayer("Traffic")))
        {
            GameObject go = GameObject.Instantiate(prefabs[UnityEngine.Random.Range(0, prefabs.Length)], position, Quaternion.identity) as GameObject;
            currentNode = carAutoPath.pathNodes[currentIndex++];
            var motor = go.AddComponent<MacchinaSorpassoPCH>();
            motor.currentNode = currentNode;
            motor.currentWaypointIndex = currentIndex;
            motor.path = carAutoPath;
            go.transform.LookAt(currentNode.position);
            motor.Init();
            return go;
        }       
        return null;
    }


    private void AvviaGuidaAutomatica()
    {
        car.GetComponent<VehicleInputController>().enabled = false;
        car.AddComponent<GuidaManualePCH>();

        CarExternalInputAutoPathAdvanced newAp = car.GetComponent<CarExternalInputAutoPathAdvanced>();
        if (newAp == null)
        {
            newAp = car.AddComponent<CarExternalInputAutoPathAdvanced>();
        } else
        {
            newAp.enabled = true;
        }
        
        newAp.ChangeProperty += new CarExternalInputAutoPathAdvanced.Delegato(gestisciEvento);
        newAp.waypointThreshold = 4f;
        newAp.maxSpeed = 18f;
        //newAp.maxSpeed = 11f;
        newAp.maxBrake = 1f;
        newAp.maxThrottle = 0.7f;
        newAp.steerSpeed = 10f;
        newAp.throttleSpeed = 1f;
        newAp.brakeSpeed = 0.5f;

        newAp.normalAdd = 0f;
        newAp.pathRadius = 0.35f;

        newAp.path = carAutoPath;
        newAp.waypoint = 1559;
        //newAp.waypoint = 1870;
        Transform posizioneRaycast = null;
        foreach (Transform go in car.GetComponentsInChildren<Transform>()) 
        {
            if (go.name.Equals("rayCastPos"))
            {
                posizioneRaycast = go;
                break;
            }
        }
        newAp.raycastOrigin = posizioneRaycast;
        newAp.limiteVelocita = 70f;
        newAp.Init();
        guidaAutomatica = newAp;
    }

    private void interrompiScenario()
    {
        Destroy(car.GetComponent<CarExternalInputAutoPathAdvanced>());
        car.GetComponent<VehicleInputController>().enabled = true;
        rocciaTest.SetActive(false);
        Destroy(car.GetComponent<GuidaManualePCH>());

        Destroy(macchinaTrafficoSorpasso);

        for (int i = 0; i < macchinaTraffico.Length; i++)
        {
            Destroy(macchinaTraffico[i]);
        }
    }

   

}