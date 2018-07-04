using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetPosizione : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
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

            Vector3 offsetPos = CenterEyeAnchor.position - posizioneTesta.position;
            Vector3 posizioneTarget = transform.position - offsetPos;
            //transform.position = new Vector3(posizioneTarget.x, transform.position.y, posizioneTarget.z);
            transform.position = transform.position - offsetPos;
        }
    }
}
