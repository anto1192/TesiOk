using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class inizioTest : MonoBehaviour {

    public ScenarioTestExtraurbano scenario;
    private bool testIniziato = false;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	    if (scenario.scenarioAvviato == false)
        {
            testIniziato = false;
        }
	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Player") && scenario.scenarioAvviato && !testIniziato) 
        {
            scenario.iniziaTest();
            testIniziato = true;
        }
    }


}
