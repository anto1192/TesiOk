using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class piegaScooter : MonoBehaviour {

    private TrafAIMotor motor;
    private float sterzataPrecedente = 0;

	// Use this for initialization
	void Start () {
        motor = gameObject.GetComponentInParent<TrafAIMotor>();
        angoloPrecedente = transform.rotation.eulerAngles.y;
    }

    float angoloPrecedente = 0;

	void FixedUpdate () {
        float sterzata = motor.currentTurn;
        float piegatura = Mathf.Clamp(-sterzata, -7f, 7f);
        float nuovoAngolo = angoloPrecedente + sterzata;
        transform.rotation = Quaternion.Euler(0, nuovoAngolo * Time.fixedDeltaTime, piegatura);
        if (Mathf.Abs(transform.rotation.eulerAngles.x) > 1)
        {
            transform.Rotate(-transform.rotation.eulerAngles.x, sterzata * Time.fixedDeltaTime, piegatura);
        }        
        angoloPrecedente = nuovoAngolo;
    }
}
