using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentSensingAltController : MonoBehaviour
{

    private Environment selectedEnv = Environment.NONE;
    //private EnvironmentSensingAlt envSensing;
    private EnvironmentSensingAltTrigger envSensing;
    //private EnvironmentSensingAltUrban envSensingUrban;
    private EnvironmentSensingAltUrbanTrigger envSensingUrban;

    public GameObject speedPanel; //this is used to semplify the search into the car hierarchy which is very deep
    public DriverCamera driverCam;

    void OnEnable()
    {
        MasterSelectController.OnCreateScene += HandleOnCreateScene;
    }

    void OnDisable()
    {
        MasterSelectController.OnCreateScene -= HandleOnCreateScene;
    }

    void HandleOnCreateScene(Environment env)
    {
        selectedEnv = env; //store selectedEnvironment in order to decide how to behave based on the current scene
        if (selectedEnv.Equals(Environment.COASTAL))
        {
            //envSensing = gameObject.AddComponent<EnvironmentSensingAlt>();
            envSensing = gameObject.AddComponent<EnvironmentSensingAltTrigger>();
            envSensing.DriverCam = driverCam;
            envSensing.SpeedPanel = speedPanel;
            envSensing.enabled = true;

        } else if(selectedEnv.Equals(Environment.URBAN)) {
            //envSensingUrban = gameObject.AddComponent<EnvironmentSensingAltUrban>();
            envSensingUrban = gameObject.AddComponent<EnvironmentSensingAltUrbanTrigger>();
            //envSensingUrban.DriverCam = driverCam;
            envSensingUrban.DriverCam = GameObject.Find("DriverCamera").GetComponent<DriverCamera>();
            Debug.Log(GameObject.Find("DriverCamera").GetComponent<DriverCamera>());
            envSensingUrban.SpeedPanel = speedPanel;
            envSensingUrban.enabled = true;
        }
        
        this.enabled = false;
    }
}