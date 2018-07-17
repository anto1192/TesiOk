using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetPosizione : MonoBehaviour {

    private DeviceState state;
    // Use this for initialization
    void Start () {
		if (!UnityEngine.XR.XRDevice.isPresent)
        {
            gameObject.SetActive(false);
            interventoAllaGuidaConsentito = true;
        }
    }

    public Transform posizioneAnterioreSinistra; //posizione guidatore
    public Transform posizioneAnterioreDestra;
    public Transform posizionePosterioreSinistra;
    public Transform posizionePosterioreDestra;
    public Transform CenterEyeAnchor;

    public bool interventoAllaGuidaConsentito = false;

    // Update is called once per frame
    void Update()
    {
        state = DirectInputWrapper.GetStateManaged(0);
        if (Input.GetKeyDown(KeyCode.F5) || state.rgbButtons[9] == 128)  //tasto Y
        {
            if (posizioneAnterioreSinistra != null)
            {
                ResetPosizioneVisore(posizioneAnterioreSinistra);
                interventoAllaGuidaConsentito = true;
            }
            else
            {
                Debug.Log("Errore! Assegna la posizione target del visore");
            }
            return;
        }
        if (Input.GetKeyDown(KeyCode.F6) || state.rgbButtons[25] == 128) //tasto B
        {
            if (posizioneAnterioreSinistra != null)
            {
                ResetPosizioneVisore(posizioneAnterioreDestra);
                interventoAllaGuidaConsentito = false;
            }
            return;
        }
        if (Input.GetKeyDown(KeyCode.F7) || state.rgbButtons[21] == 128) //tasto X
        {
            if (posizioneAnterioreSinistra != null)
            {
                ResetPosizioneVisore(posizionePosterioreSinistra);
                interventoAllaGuidaConsentito = false;
            }
            return;
        }
        if (Input.GetKeyDown(KeyCode.F8) || state.rgbButtons[1] == 128) //tasto A
        {
            if (posizioneAnterioreSinistra != null)
            {
                ResetPosizioneVisore(posizionePosterioreDestra);
                interventoAllaGuidaConsentito = false;
            }
            return;
        }
    }

    private void ResetPosizioneVisore(Transform posizioneTesta)
    {
        if ((CenterEyeAnchor != null))
        {
            //aggiusto la rotazione
            float offsetRot = CenterEyeAnchor.rotation.eulerAngles.y - posizioneTesta.eulerAngles.y;
            transform.Rotate(0f, -offsetRot, 0f);

            //aggiusto la posizione
            Vector3 offsetPos = CenterEyeAnchor.position - posizioneTesta.position;          
            Vector3 posizioneTarget = transform.position - offsetPos;
            transform.position = transform.position - offsetPos;
        }
    }
}
