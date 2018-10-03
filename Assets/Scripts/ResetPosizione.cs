using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ResetPosizione : MonoBehaviour {

    private DeviceState state;
    // Use this for initialization
    void Start () {
		if (!UnityEngine.XR.XRDevice.isPresent)
        {
            gameObject.SetActive(false);
            interventoAllaGuidaConsentito = true;

            //abilito la driverCamera
            GameObject driverCamera = GameObject.Find("DriverCamera");
            GameObject center = driverCamera.transform.Find("Center").gameObject;
            center.GetComponent<Camera>().enabled = true;
        } else
        {
            //disabilito la driverCamera
            GameObject driverCamera = GameObject.Find("DriverCamera");
            GameObject center = driverCamera.transform.Find("Center").gameObject;
            center.GetComponent<Camera>().enabled = false;
        }
    }

    public Transform posizioneAnterioreSinistra; //posizione guidatore
    public Transform posizioneAnterioreDestra;
    public Transform posizionePosterioreSinistra;
    public Transform posizionePosterioreDestra;
    public Transform CenterEyeAnchor;
    public Transform manoDestra;
    public Transform manoSinistra;
    public Transform volanteDestra;
    public Transform volanteSinistra;
    private Vector3 ultimaPosizione = Vector3.zero;
    private Vector3 ultimaRotazione = Vector3.zero;


    public bool interventoAllaGuidaConsentito = false;

    // Update is called once per frame
    void Update()
    {
        state = DirectInputWrapper.GetStateManaged(0);       
        if (Input.GetKeyDown(KeyCode.F5) || state.rgbButtons[9] == 128)  //tasto Y
        {
            if (posizioneAnterioreSinistra != null)
            {
                Transform t = leggiFileDisco();
                if (t != null && Vector3.Distance(transform.localPosition, t.position) < 10f)
                {
                    //Vector3.Distance(transform.position, t.position) < 10f controlla che la posizione salvata non sia troppo distante da quella attuale, in tal caso significa che è una posizione memorizzata ma non corretta
                    transform.localPosition = t.position;
                    transform.localRotation = t.rotation;
                    ultimaPosizione = t.position;
                    ultimaRotazione = t.rotation.eulerAngles;
                    Debug.Log("Ripristinata ultima posizione salvata");
                    return;
                }
                ResetPosizioneVisore(CenterEyeAnchor, posizioneAnterioreSinistra);
                ultimaPosizione = transform.localPosition;
                ultimaRotazione = transform.localRotation.eulerAngles;
                interventoAllaGuidaConsentito = true;
                Debug.Log("Reset posizione tramite posizione visore");
            }
            else
            {
                Debug.Log("Errore! Assegna la posizione target del visore");
            }
            return;
        }
        if (Input.GetKeyDown(KeyCode.F10) || state.rgbButtons[7] == 128)  //tasto LT
        {
            if (ultimaPosizione != Vector3.zero)
            {
                salvaPosizioneSuDisco();
            }           
        }
        if (Input.GetKeyDown(KeyCode.F9) || state.rgbButtons[2] == 128)  //tasto RT
        {
            if (posizioneAnterioreSinistra != null)
            {
                ResetPosizioneVisoreGuidatore();
                ultimaPosizione = transform.localPosition;
                ultimaRotazione = transform.localRotation.eulerAngles;
                interventoAllaGuidaConsentito = true;
            }
            return;
        }
        if (Input.GetKeyDown(KeyCode.KeypadPlus) || state.rgbButtons[4] == 128)  //tasto + sulla tastiera oppure paletta cambio marcia su
        {
            transform.Rotate(0f, 0.1f, 0f);
        }
        if (Input.GetKeyDown(KeyCode.KeypadMinus) || state.rgbButtons[5] == 128)  //tasto - sulla tastiera oppure paletta cambio marcia giu
        {
            transform.Rotate(0f, -0.1f, 0f);
        }
        if (Input.GetKeyDown(KeyCode.Keypad8) || (state.rgdwPOV[0] == 0 && state.rgdwPOV[1] != 0))   //tasto 8 sulla tastiera oppure freccia sopra sul volante
        {
            //transform.Translate(0, 0, 0.001f);
            transform.Translate(0.001f, 0, 0);
        }
        if (Input.GetKeyDown(KeyCode.Keypad2) || state.rgdwPOV[0] == 18000)  //tasto 2 sulla tastiera oppure freccia sotto sul volante
        {
            //transform.Translate(0, 0, -0.001f);
            transform.Translate(-0.001f, 0, 0);
        }
        if (Input.GetKeyDown(KeyCode.Keypad6) || state.rgdwPOV[0] == 9000)  //tasto * sulla tastiera oppure freccia destra sul volante
        {
            //transform.Translate(0.001f, 0, 0);
            transform.Translate(0, 0, -0.001f);
        }
        if (Input.GetKeyDown(KeyCode.Keypad4) || state.rgdwPOV[0] == 27000)  //tasto / sulla tastiera oppure freccia sinistra sul volante
        {
            //transform.Translate(-0.001f, 0, 0);
            transform.Translate(0, 0, 0.001f);
        }

        if (Input.GetKeyDown(KeyCode.F6) || state.rgbButtons[25] == 128) //tasto B
        {
            if (posizioneAnterioreSinistra != null)
            {
                ResetPosizioneVisore(CenterEyeAnchor, posizioneAnterioreDestra);                
                interventoAllaGuidaConsentito = false;
            }
            return;
        }
        if (Input.GetKeyDown(KeyCode.F7) || state.rgbButtons[21] == 128) //tasto X
        {
            if (posizioneAnterioreSinistra != null)
            {
                ResetPosizioneVisore(CenterEyeAnchor, posizionePosterioreSinistra);
                interventoAllaGuidaConsentito = false;
            }
            return;
        }
        if (Input.GetKeyDown(KeyCode.F8) || state.rgbButtons[1] == 128) //tasto A
        {
            if (posizioneAnterioreSinistra != null)
            {
                ResetPosizioneVisore(CenterEyeAnchor, posizionePosterioreDestra);
                interventoAllaGuidaConsentito = false;
            }
            return;
        }
    }

    private void ResetPosizioneVisoreGuidatore()
    {
        //questo reset centra le mani visualizzate nell'ambiente virtuale sul volante
        //e' dunque necessario chiedere all'utente di mettere le mani sul volante reale dopodichè resettare la posizione
        if (manoDestra == null || manoSinistra == null || volanteDestra == null || volanteSinistra == null || manoDestra.gameObject.activeInHierarchy == false || manoSinistra.gameObject.activeInHierarchy == false)
        {
            //le mani o il volante non sono stati settati oppure le mani non sono visualizzate nell'ambiente virtuale
            //eseguo il reset normale
            ResetPosizioneVisore(CenterEyeAnchor, posizioneAnterioreSinistra);
            Debug.Log("Reset posizione tramite posizione visore");
            return;
        }

        GameObject puntoEquidistanteMani = new GameObject("puntoEquidistanteMani");
        //puntoEquidistanteMani.transform.parent = transform.parent;
        puntoEquidistanteMani.transform.position = (manoDestra.position + manoSinistra.position) / 2;
        puntoEquidistanteMani.transform.rotation = CenterEyeAnchor.rotation;

        GameObject puntoEquidistanteVolante = new GameObject("puntoEquidistanteVolante");
        puntoEquidistanteVolante.transform.position = (volanteDestra.position + volanteSinistra.position) / 2;
        puntoEquidistanteVolante.transform.rotation = posizioneAnterioreSinistra.rotation;

        //Debug.DrawLine(puntoEquidistanteMani.transform.position, puntoEquidistanteVolante.transform.position);

        ResetPosizioneVisore(puntoEquidistanteMani.transform, puntoEquidistanteVolante.transform);

        Debug.Log("Reset posizione tramite posizione mani leap motion");

        Destroy(puntoEquidistanteMani);
        Destroy(puntoEquidistanteVolante);
    }

    //private void ResetPosizioneVisoreGuidatore(Transform posizioneTesta) {
    //    //ResetPosizioneVisore(posizioneResetMani);
    //    ResetPosizioneVisore(CenterEyeAnchor, posizioneAnterioreDestra);
    //    Vector3 posizioneLMHead = transform.position;
    //    ResetPosizioneVisore(CenterEyeAnchor, posizioneTesta);
    //    Vector3 posizioneLMHeadOk = transform.position;

    //    Vector3 offsetPos = posizioneLMHead - posizioneLMHeadOk;
    //    HandModels.transform.position = HandModels.transform.position + offsetPos;
    //}


    

    private void ResetPosizioneVisore(Transform posizionePartenza, Transform posizioneTesta)
    {
        if ((CenterEyeAnchor != null))
        {
            //aggiusto la rotazione
            float offsetRot = posizionePartenza.rotation.eulerAngles.y - posizioneTesta.eulerAngles.y;
            transform.Rotate(0f, -offsetRot, 0f);
            //transform.rotation = posizioneTesta.rotation;

            //aggiusto la posizione
            Vector3 offsetPos = posizionePartenza.position - posizioneTesta.position;          
            Vector3 posizioneTarget = transform.position - offsetPos;
            transform.position = transform.position - offsetPos;

            //HandModels.transform.localPosition = Vector3.zero;
        }
    }



    private Transform leggiFileDisco()
    {
        StreamReader inputStream = null;
        try {
            string nomeFile = Application.streamingAssetsPath + "/defaultResetPosition.txt";
            inputStream = new StreamReader(nomeFile);
            string line = inputStream.ReadLine();
            string[] primaLinea = new string[3];
            primaLinea = line.Split(' ');
            line = inputStream.ReadLine();
            string[] secondaLinea = new string[3];
            secondaLinea = line.Split(' ');

            Vector3 posizione = new Vector3(float.Parse(primaLinea[0]), float.Parse(primaLinea[1]), float.Parse(primaLinea[2]));
            Vector3 rotazione = new Vector3(float.Parse(secondaLinea[0]), float.Parse(secondaLinea[1]), float.Parse(secondaLinea[2]));

            GameObject go = new GameObject();
            go.transform.position = posizione;
            go.transform.eulerAngles = rotazione;

            Transform t = go.transform;
            Destroy(go);
            inputStream.Close();
            return t;
        }
        catch (Exception e)
        {
            Debug.Log("eccezzione nella lettura del file: " + e);
            if (inputStream != null)
            {
                inputStream.Close();
            }          
            return null;
        }

    }

    private void salvaPosizioneSuDisco()
    {
        StreamWriter streamWriter = null;
        try
        {
            string nomeFile = Application.streamingAssetsPath + "/defaultResetPosition.txt";
            streamWriter = new StreamWriter(nomeFile);
            streamWriter.WriteLine(ultimaPosizione.x + " " + ultimaPosizione.y + " " + ultimaPosizione.z);
            streamWriter.WriteLine(ultimaRotazione.x + " " + ultimaRotazione.y + " " + ultimaRotazione.z);
            streamWriter.Close();
        } catch (Exception e)
        {
            Debug.Log("Eccezione nella scrittura del file: " + e);
            if (streamWriter != null)
            {
                streamWriter.Close();
            }
            
        }
        
    }

}
