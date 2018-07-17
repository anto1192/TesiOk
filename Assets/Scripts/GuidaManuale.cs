/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;
using System;

public class GuidaManuale : MonoBehaviour
{

    private VehicleController controller;
    private TrafAIMotor motor;
    private float maxDifferenza = 0;
    private float inputPrecedente = 9999;
    private bool primoIntervento = false;
    private float sterzataRiferimento;

    private ResetPosizione posizione = null;

    void Awake()
    {
        controller = GetComponent<VehicleController>();
        posizione = gameObject.GetComponentInChildren<ResetPosizione>();
    }


    void Update()
    {
        if (posizione != null)
        {
            if (!posizione.interventoAllaGuidaConsentito)
            {
                return;
            }
        } else
        {
            Debug.Log("posizione Null");
        }
        //grab input values
        var inputController = AppController.Instance.UserInput;
        //Debug.Log("sterzata: " + inputController.GetSteerInput() + "; accelerazione: " + inputController.GetAccelBrakeInput());

        if (inputController.GetAccelBrakeInput() != 0)
        {
            motor.interventoAllaGuidaAccelerazione = true;
            controller.accellInput = inputController.GetAccelBrakeInput();
        }  else
        {
            if (motor.interventoAllaGuidaAccelerazione)
            {
                motor.interventoAllaGuidaAccelerazione = false;
            }
        }
        
        if (inputController.GetSteerInput() != 0 && !(AppController.Instance.UserInput is SteeringWheelInputController))
        {
            motor.interventoAllaGuidaSterzata = true;
            controller.steerInput = inputController.GetSteerInput();
        } else
        {
            if (motor.interventoAllaGuidaSterzata)
            {
                motor.interventoAllaGuidaSterzata = false;
            }
        }

        if (AppController.Instance.UserInput is SteeringWheelInputController)
        {
            float inputAttuale = inputController.GetSteerInput();
            if (Math.Abs(inputAttuale - inputPrecedente) > 0.0001f && inputPrecedente != 9999)
            //if (inputAttuale != inputPrecedente && inputPrecedente != 9999)
            {
                //intervento all aguida
                if (!primoIntervento)
                {
                    primoIntervento = true;
                    sterzataRiferimento = controller.steerInput;
                }
                motor.interventoAllaGuidaSterzata = true;
                controller.steerInput = sterzataRiferimento + inputController.GetSteerInput();
            }
            else
            {
                primoIntervento = false;
                if (motor.interventoAllaGuidaSterzata)
                {
                    motor.interventoAllaGuidaSterzata = false;
                }
            }
            inputPrecedente = inputAttuale;
        }

    }

    public void setMotor(TrafAIMotor motor)
    {
        this.motor = motor;
    }
}
