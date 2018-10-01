using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleSelection : MonoBehaviour {

    TrafAIMotor motor = null;
    GameObject macchinaTrafficoDavanti;
    GameObject pericolo;
    public TrafAIMotor nuovaAuto = null;
    public List<TrafAIMotor> autoTraffico = new List<TrafAIMotor>();
    public List<GameObject> autoIncrocio = new List<GameObject>();
    public List<GameObject> autoVisualizzate = new List<GameObject>();

    private EnvironmentSensingAltUrbanTriggerSelective envSelective;  //DARIO


    private GameObject oggettoInRimozione = null;

    void FixedUpdate()
    {

        
        if (motor == null)
        {
            if (gameObject.GetComponent<TrafAIMotor>() == null)
            {
                return;
            }
            motor = gameObject.GetComponent<TrafAIMotor>();
            envSelective = transform.Find("colliderEnv").GetComponent<EnvironmentSensingAltUrbanTriggerSelective>(); //DARIO
        }

        //CONTROLLO AUTO DAVANTI
        /*if (motor.somethingInFront && motor.oggettoRilevato != null)
        {
            //c'è qualcosa davanti la macchina e si tratta di una macchina del traffico

            if (macchinaTrafficoDavanti != motor.oggettoRilevato)
            {
                //devo visualizzare il box
                macchinaTrafficoDavanti = motor.oggettoRilevato;
                AggiungiBox(macchinaTrafficoDavanti);               
                
            }
        }
        else
        {
            if (macchinaTrafficoDavanti != null)
            {
                //non devo visualizzare piu il box
                RimuoviBoxLinea(macchinaTrafficoDavanti);               
                macchinaTrafficoDavanti = null;
            }
        }*/
        if (nuovaAuto != null)
        {
            autoTraffico.Add(nuovaAuto);
            nuovaAuto = null;
        }

        //Controllo la lista delle auto del traffico eliminado quelle distrutte nella scena
        List<int> listaIndici = new List<int>();
        int contatore = 0;
        foreach (TrafAIMotor auto in autoTraffico)
        {
            if (auto == null)
            {
                listaIndici.Add(contatore);              
            }
            contatore++;

        }
        for (int i = listaIndici.Count-1; i >= 0; i--)
        {
            autoTraffico.RemoveAt(listaIndici[i]);
        }


        //cONTROLLO AUTO DAVANTI
        foreach(TrafAIMotor auto in autoTraffico)
        {
            bool controlloStessoCurrentEntry = auto.currentEntry.identifier == motor.currentEntry.identifier && auto.currentEntry.subIdentifier == motor.currentEntry.subIdentifier;
            bool controlloNextEntry = (motor.nextEntry != null && auto.currentEntry.identifier == motor.nextEntry.identifier && auto.currentEntry.subIdentifier == motor.nextEntry.subIdentifier);
            bool controlloProssimoNode = (auto.currentEntry.identifier == motor.fixedPath[motor.currentFixedNode].id && auto.currentEntry.subIdentifier == motor.fixedPath[motor.currentFixedNode].subId);
            bool controlloProssimoNode2 = ((motor.currentFixedNode + 1) < motor.fixedPath.Count && auto.currentEntry.identifier == motor.fixedPath[motor.currentFixedNode + 1].id && auto.currentEntry.subIdentifier == motor.fixedPath[motor.currentFixedNode + 1].subId);
            if (controlloProssimoNode)
            {

                    Vector3 heading = (motor.system.GetEntry(motor.fixedPath[motor.currentFixedNode].id, 0).waypoints[motor.system.GetEntry(motor.fixedPath[motor.currentFixedNode].id, 0).waypoints.Count-1] - motor.nose.transform.position).normalized;
                    
                    if ((Mathf.Abs(Vector3.SignedAngle(motor.nose.transform.forward, heading, Vector3.up))) > 30)
                    {
                        //la prossima strada da percorrere non è davanti a me (si tratta di un incrocio a T o a X
                        controlloProssimoNode = false;
                    }
                
            }
            if (controlloProssimoNode2)
            {
                Vector3 heading = (motor.system.GetEntry(motor.fixedPath[motor.currentFixedNode + 1].id, 0).waypoints[motor.system.GetEntry(motor.fixedPath[motor.currentFixedNode + 1].id, 0).waypoints.Count-1] - motor.nose.transform.position).normalized;
                float angolo = Mathf.Abs(Vector3.SignedAngle(motor.nose.transform.forward, heading, Vector3.up));
                if (angolo > 30)
                {
                    //la prossima strada da percorrere non è davanti a me (si tratta di un incrocio a T o a X
                    controlloProssimoNode2 = false;
                }
            }

            bool controlloStessoStartIncrocio = false;

            if (auto.currentEntry.identifier > 1000 && (motor.currentEntry.identifier > 1000 || (motor.nextEntry != null && motor.nextEntry.identifier > 1000))) {
                //entrambe le macchine sono in un'incrocio o si apprestano ad entrarci
                int id, subId = 0;
                if (motor.currentEntry.identifier > 1000)
                {
                    id = motor.currentEntry.identifier;
                    subId = motor.currentEntry.subIdentifier;
                } else
                {
                    id = motor.nextEntry.identifier;
                    subId = motor.nextEntry.subIdentifier;
                }

                if (auto.system.GetEntry(auto.currentEntry.identifier, auto.currentEntry.subIdentifier).path.start == motor.system.GetEntry(id, subId).path.start)
                {
                    controlloStessoStartIncrocio = true;
                }
            }

            //if ((auto.currentEntry.identifier == motor.currentEntry.identifier && auto.currentEntry.subIdentifier == motor.currentEntry.subIdentifier)
            //    || (motor.nextEntry!= null && auto.currentEntry.identifier == motor.nextEntry.identifier && auto.currentEntry.subIdentifier == motor.nextEntry.subIdentifier)
            //    || (auto.currentEntry.identifier == motor.fixedPath[motor.currentFixedNode].id && auto.currentEntry.subIdentifier == motor.fixedPath[motor.currentFixedNode].subId)
            //    || (auto.currentEntry.identifier > 1000 && (motor.currentEntry.identifier > 1000 || (motor.nextEntry != null && motor.nextEntry.identifier > 1000)) && ((auto.system.GetEntry(auto.currentEntry.identifier, auto.currentEntry.subIdentifier).path.start == auto.system.GetEntry(motor.currentEntry.identifier, motor.currentEntry.subIdentifier).path.start) || (auto.system.GetEntry(auto.currentEntry.identifier, auto.currentEntry.subIdentifier).path.start == auto.system.GetEntry(motor.nextEntry.identifier, motor.nextEntry.subIdentifier).path.start)))
            //    || ((motor.currentFixedNode + 1) < motor.fixedPath.Count && auto.currentEntry.identifier == motor.fixedPath[motor.currentFixedNode + 1].id && auto.currentEntry.subIdentifier == motor.fixedPath[motor.currentFixedNode + 1].subId))
            if (controlloStessoCurrentEntry || (!motor.hasStopTarget && (controlloNextEntry || controlloProssimoNode || controlloProssimoNode2 || controlloStessoStartIncrocio)))
            {

                //anche quando hanno lo stesso punto d'ingresso negli incroci
                if (!autoVisualizzate.Contains(auto.gameObject))
                {
                    autoVisualizzate.Add(auto.gameObject);
                    AggiungiBox(auto.gameObject);
                }
                
            }
            else
            {
                if (autoVisualizzate.Contains(auto.gameObject))
                {
                    StartCoroutine(AttesaRimuoviBox(auto.gameObject));
                    //RimuoviBoxLinea(auto.gameObject);
                    autoVisualizzate.Remove(auto.gameObject);
                }               
            }
        }


        //CONTROLLO INCROCI
        if (motor.currentEntry.identifier > 1000 || (motor.nextEntry != null && motor.nextEntry.identifier > 1000))
        {
            //sono in un incrocio o sto per entrarci
            int id, subId = 0;
            if (motor.currentEntry.identifier > 1000)
            {
                id = motor.currentEntry.identifier;
                subId = motor.currentEntry.subIdentifier;
            }
            else
            {
                id = motor.nextEntry.identifier;
                subId = motor.nextEntry.subIdentifier;
            }

            int myPathsubId = motor.system.GetEntry(id, subId).path.subId;

            //Controllo se devo dar precedenza
            List<int> listaPrecedenze = motor.system.GetEntry(id, subId).path.giveWayTo;
            if (listaPrecedenze.Count != 0)
            {
                //devo dare precedenza a qualcuno
                //controllo che ci sia qualcuno in quella strada

                foreach (TrafAIMotor motorTraffico in autoTraffico)
                {
                    if (motorTraffico.currentEntry.identifier == id || (motorTraffico.nextEntry != null && motorTraffico.nextEntry.identifier == id))
                    {
                        //la macchina del traffico è nello stesso incrocio di dove sono io

                        //controllo se è lei che deve ricevere precedenza
                        int trafficSubId = 0;
                        if (motorTraffico.currentEntry.identifier == id)
                        {
                            trafficSubId = motorTraffico.currentEntry.subIdentifier;
                        }
                        else
                        {
                            trafficSubId = motorTraffico.nextEntry.subIdentifier;
                        }
                        int trafficPathsubId = motor.system.GetEntry(id, trafficSubId).path.subId;
                        if (listaPrecedenze.Contains(trafficPathsubId))
                        {
                            //questa macchina deve ricevere precedenza
                            if (!autoIncrocio.Contains(motorTraffico.gameObject))
                            {
                                //devo visualizzare il box e la linea di navigazione
                                AggiungiBoxLinea(motorTraffico.gameObject);
                                autoIncrocio.Add(motorTraffico.gameObject);
                            }
                        }
                    }
                }
            }


            //controllo se altri devono dare precedenza a me
            foreach (TrafAIMotor motorTraffico in autoTraffico)
            {
                if ((motorTraffico.currentEntry != null && motorTraffico.currentEntry.identifier == id) || (motorTraffico.nextEntry != null && motorTraffico.nextEntry.identifier == id))
                {
                    //la macchina del traffico è nello stesso incrocio di dove sono io

                    //controllo se deve darmi precedenza
                    int trafficSubId = 0;
                    if (motorTraffico.currentEntry.identifier == id)
                    {
                        trafficSubId = motorTraffico.currentEntry.subIdentifier;
                    }
                    else
                    {
                        trafficSubId = motorTraffico.nextEntry.subIdentifier;
                    }

                    if (motorTraffico.system.GetEntry(id, trafficSubId).path.giveWayTo.Contains(myPathsubId))
                    {
                        //deve darmi precedenza
                        if (!autoIncrocio.Contains(motorTraffico.gameObject))
                        {
                            //devo visualizzare il box e la linea di navigazione
                            AggiungiBoxLinea(motorTraffico.gameObject);
                            autoIncrocio.Add(motorTraffico.gameObject);
                        }
                    }
                }
            }
        } else
        {
            //sono uscito dall'incrocio
            if (autoIncrocio.Count != 0)
            {
                foreach(GameObject autoTraffico in autoIncrocio)
                {
                    if (autoTraffico != null)
                    {
                        //devo rimuovere il box e la linea di navigazione
                        StartCoroutine(AttesaRimuoviBox(autoTraffico));
                    }                   
                }
                autoIncrocio.RemoveRange(0, autoIncrocio.Count -1); //elimina tutti glie elementi
            }
        }




    }

    IEnumerator AttesaRimuoviBox(GameObject go)
    {
        oggettoInRimozione = go;
        yield return new WaitForSeconds(3f);
        if (oggettoInRimozione != null)
        {
            //può succedere che sia stata prima chiamata la funzione per rimuovere il box
            //e, durante l'attesa, sia stato aggiunto nuovamente il box. Dopo l'attesa il box verrebbe rimosso
            //PS: notifiche spurie
            RimuoviBoxLinea(go);
            oggettoInRimozione = null;
        }
       
    }


    private void AggiungiBox(GameObject go)
    {
        if (oggettoInRimozione != null)
        {
            if (oggettoInRimozione == go)
            {
                oggettoInRimozione = null;
            }
        }

        Collider other = go.transform.Find("CollidersBody/Body1").GetComponent<Collider>();
        if (envSelective.IDsAndGos.ContainsKey(other.gameObject.GetInstanceID()))
        {
            CubesAndTags cubesAndTags = envSelective.IDsAndGos[other.gameObject.GetInstanceID()];
            cubesAndTags.boundingCube[0].GetComponent<Renderer>().enabled = true;
            cubesAndTags.infoTag[0].GetComponent<Canvas>().enabled = true;
        }
    }

    private void AggiungiBoxLinea(GameObject go)
    {
        if (oggettoInRimozione != null)
        {
            if (oggettoInRimozione == go)
            {
                oggettoInRimozione = null;
            }
        }

        Collider other = go.transform.Find("CollidersBody/Body1").GetComponent<Collider>();
        if (envSelective.IDsAndGos.ContainsKey(other.gameObject.GetInstanceID()))
        {
            CubesAndTags cubesAndTags = envSelective.IDsAndGos[other.gameObject.GetInstanceID()];
            cubesAndTags.boundingCube[0].GetComponent<Renderer>().enabled = true;
            cubesAndTags.infoTag[0].GetComponent<Canvas>().enabled = true;
            go.GetComponent<TrafficCarNavigationLineUrban>().enabled = true;
            Transform trafLineRenderer = go.transform.Find("TrafLineRenderer");
            if (trafLineRenderer != null)
                trafLineRenderer.GetComponent<LineRenderer>().enabled = true;
        }       
    }

    private void RimuoviBoxLinea(GameObject go)
    {
        Collider other = go.transform.Find("CollidersBody/Body1").GetComponent<Collider>();
        if (envSelective.IDsAndGos.ContainsKey(other.gameObject.GetInstanceID()))
        {
            CubesAndTags cubesAndTags = envSelective.IDsAndGos[other.gameObject.GetInstanceID()];
            cubesAndTags.boundingCube[0].GetComponent<Renderer>().enabled = false;
            cubesAndTags.infoTag[0].GetComponent<Canvas>().enabled = false;
            Transform trafLineRenderer = go.transform.Find("TrafLineRenderer");
            if (trafLineRenderer != null)
                trafLineRenderer.GetComponent<LineRenderer>().enabled = false;
        } 
    }
}
