/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;
using System;

public class GuidaManualePCH : MonoBehaviour
{

    private VehicleController controller;

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
            controller.accellInput = inputController.GetAccelBrakeInput();
        }

        if (inputController.GetSteerInput() != 0 && !(AppController.Instance.UserInput is SteeringWheelInputController))
        {
            //motor.interventoAllaGuidaSterzata = true;
            controller.steerInput = inputController.GetSteerInput();
        }
        else
        {
            //if (motor.interventoAllaGuidaSterzata)
            //{
            //    motor.interventoAllaGuidaSterzata = false;
            //}
        }

        if (AppController.Instance.UserInput is SteeringWheelInputController)
        {
            //if (Math.Abs(inputController.GetSteerInput() - controller.steerInput) > maxDifferenza && Math.Abs(controller.steerInput) < 0.65f)
            //{
            //    maxDifferenza = Math.Abs(inputController.GetSteerInput() - controller.steerInput);
            //}
            //Debug.Log("Volante: " + inputController.GetSteerInput() + "; sterzata effettiva: " + controller.steerInput + "; differenza: " + Math.Abs(inputController.GetSteerInput() - controller.steerInput) + "; maxDifferenza = " + maxDifferenza);
        }
        //controller.steerInput = inputController.GetSteerInput();
        //controller.accellInput = inputController.GetAccelBrakeInput();
    }

    
}
