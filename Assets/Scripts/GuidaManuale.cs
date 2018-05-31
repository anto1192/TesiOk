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

    void Awake()
    {
        controller = GetComponent<VehicleController>();
    }

    /*DateTime tempo;
    float velocitaIniziale; */

    void Update()
    {
        //grab input values
        var inputController = AppController.Instance.UserInput;
        //Debug.Log("sterzata: " + inputController.GetSteerInput() + "; accelerazione: " + inputController.GetAccelBrakeInput());

        if (inputController.GetAccelBrakeInput() != 0)
        {
            /*if (motor.interventoAllaGuidaAccelerazione == false && inputController.GetAccelBrakeInput() == -1)
            {
                tempo = DateTime.Now;
                velocitaIniziale = motor.GetComponent<Rigidbody>().velocity.magnitude;
            }*/
            motor.interventoAllaGuidaAccelerazione = true;
            controller.accellInput = inputController.GetAccelBrakeInput();
            /*Debug.Log("Velocità: " + motor.GetComponent<Rigidbody>().velocity.magnitude);
            if (motor.GetComponent<Rigidbody>().velocity.magnitude < 0.01)
            {
                TimeSpan differenza = tempo - DateTime.Now;
                float millisecondi = differenza.Milliseconds + differenza.Seconds * 1000;
                Debug.Log("Sono fermo; tempo frenata da " + velocitaIniziale + " di " + millisecondi + " millisecondi");
            }*/
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
            Debug.Log("Volante: " + inputController.GetSteerInput());
        }
        //controller.steerInput = inputController.GetSteerInput();
        //controller.accellInput = inputController.GetAccelBrakeInput();
    }

    public void setMotor(TrafAIMotor motor)
    {
        this.motor = motor;
    }
}
