/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;

public class GuidaManuale : MonoBehaviour
{

    private VehicleController controller;
    private TrafAIMotor motor;

    void Awake()
    {
        controller = GetComponent<VehicleController>();
    }

    void Update()
    {
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
        if (inputController.GetSteerInput() != 0)
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
        //controller.steerInput = inputController.GetSteerInput();
        //controller.accellInput = inputController.GetAccelBrakeInput();
    }

    public void setMotor(TrafAIMotor motor)
    {
        this.motor = motor;
    }
}
