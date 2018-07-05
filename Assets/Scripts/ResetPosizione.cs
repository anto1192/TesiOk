using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetPosizione : MonoBehaviour {

	// Use this for initialization
	void Start () {
		if (!UnityEngine.XR.XRDevice.isPresent)
        {
            gameObject.SetActive(false);
        }
	}

    public Transform posizioneTesta;
    public Transform CenterEyeAnchor;

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (posizioneTesta != null)
            {
                ResetPosizioneVisore(posizioneTesta);
            }
            else
            {
                Debug.Log("Errore! Assegna la posizione target del visore");
            }

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
