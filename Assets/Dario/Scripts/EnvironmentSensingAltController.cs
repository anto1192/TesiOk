using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentSensingAltController : MonoBehaviour
{

    private Environment selectedEnv = Environment.NONE;
    
    private EnvironmentSensingAltTrigger envSensing;
    private EnvironmentSensingAltUrbanTrigger envSensingUrban;

    private float radius = 150;

    public DriverCamera driverCam;
    public ResourceHandler resourceHandler;
    public GameObject leapCam;

    void OnEnable()
    {
        CreateResourceHandler();
        MasterSelectController.OnCreateScene += HandleOnCreateScene;
    }

    void OnDisable()
    {
        MasterSelectController.OnCreateScene -= HandleOnCreateScene;
    }

    void HandleOnCreateScene(Environment env)
    {
        GameObject ColliderEnv = new GameObject("colliderEnv");
        ColliderEnv.transform.SetParent(transform);
        ColliderEnv.transform.localPosition = Vector3.zero;
        ColliderEnv.transform.localRotation = Quaternion.identity;

        SphereCollider sphereCol = ColliderEnv.AddComponent<SphereCollider>();
        sphereCol.radius = radius;
        sphereCol.isTrigger = true;
        sphereCol.gameObject.layer = LayerMask.NameToLayer("PlayerCar");

        selectedEnv = env; //store selectedEnvironment in order to decide how to behave based on the current scene
        if (selectedEnv.Equals(Environment.COASTAL))
        {
            transform.gameObject.AddComponent<PlayerCarLines>();

            envSensing = ColliderEnv.transform.gameObject.AddComponent<EnvironmentSensingAltTrigger>();
            envSensing.DriverCam = driverCam;
            envSensing.LeapCam = leapCam;
            envSensing.enabled = true;

        } else if(selectedEnv.Equals(Environment.URBAN)) {
            
            transform.gameObject.AddComponent<PlayerCarLinesUrban>();

            envSensingUrban = ColliderEnv.transform.gameObject.AddComponent<EnvironmentSensingAltUrbanTrigger>();
            envSensingUrban.DriverCam = driverCam;
            envSensingUrban.LeapCam = leapCam;
            envSensingUrban.enabled = true;
        }

        Time.timeScale = 1.0f;
        transform.Find("InteriorLight").gameObject.SetActive(true);

        Destroy(this);
    }

    void CreateResourceHandler()
    {
        if (ResourceHandler.instance == null)
            Instantiate(resourceHandler);
    }
}