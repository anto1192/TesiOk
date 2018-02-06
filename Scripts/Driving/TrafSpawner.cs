/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;

public interface ITrafficSpawner
{
    void SetTraffic(bool state);
    bool GetState();
}

public class TrafSpawner : MonoBehaviour, ITrafficSpawner {

    public TrafSystem system;

    public GameObject[] prefabs;
    public GameObject[] fixedPrefabs;

    public int numberToSpawn = 50;
    public int numberFixedToSpawn = 5;

    public int lowDensity = 120;
    public int mediumDensity = 250;
    public int heavyDensity = 500;
    public int maxIdent = 20;
    public int maxSub = 4;
    public float checkRadius = 8f;

    private int[] bridgeIds = new int[] { 168, 168, 170 };
    private const int numberToSpawnOnBridge = 30;



    private TrafEntry entryTraffico = null;
    InterpolatedPosition posTraffico = null;

    public void SpawnHeaps()
    {
        system.ResetIntersections();

        for (int i = 0; i < numberFixedToSpawn; i++)
        {
            SpawnFixed();
        }

        for (int i = 0; i < numberToSpawn; i++)
        {
            Spawn();
        }

        for (int i = 0; i < numberToSpawnOnBridge; i++)
        {
            Spawn(bridgeIds[Random.Range(0, bridgeIds.Length)], Random.Range(0, 3));
        }
    }

    public void SpawnFixed()
    {
        GameObject prefab = fixedPrefabs[Random.Range(0, fixedPrefabs.Length)];
        var pMotor = prefab.GetComponent<TrafAIMotor>();
        int index = Random.Range(0, pMotor.fixedPath.Count);
        int id = pMotor.fixedPath[index].id;
        int subId = pMotor.fixedPath[index].subId;
        float distance = Random.value * 0.8f + 0.1f;
        TrafEntry entry = system.GetEntry(id, subId);
        if (entry == null)
            return;
        InterpolatedPosition pos = entry.GetInterpolatedPosition(distance);

        if (!Physics.CheckSphere(pos.position, checkRadius * 3, 1 << LayerMask.NameToLayer("Traffic")))
        {
            GameObject go = GameObject.Instantiate(prefab, pos.position, Quaternion.identity) as GameObject;
            TrafAIMotor motor = go.GetComponent<TrafAIMotor>();

            motor.currentIndex = pos.targetIndex;
            motor.currentEntry = entry;
            go.transform.LookAt(entry.waypoints[pos.targetIndex]);
            motor.system = system;
            motor.Init();

            motor.currentFixedNode = index;

        }
    }


    public void Spawn()
    {
        int id = Random.Range(0, maxIdent);
        int subId = Random.Range(0, maxSub);
        Spawn(id, subId);
    }

    public void Spawn(int id, int subId)
    {
        float distance = Random.value * 0.8f + 0.1f;
        TrafEntry entry = system.GetEntry(id, subId);
        if (entryTraffico == null)
            entryTraffico = entry;
        //entryTraffico = entry;
        if (entry == null)
            return;
        InterpolatedPosition pos = entry.GetInterpolatedPosition(distance);
        if (posTraffico == null)
        {
            posTraffico = pos;
        }
        

        if (!Physics.CheckSphere(pos.position, checkRadius, 1 << LayerMask.NameToLayer("Traffic")))
        {
            GameObject go = GameObject.Instantiate(prefabs[Random.Range(0, prefabs.Length)], pos.position, Quaternion.identity) as GameObject;
            TrafAIMotor motor = go.GetComponent<TrafAIMotor>();
            go.layer = 16;
            

            motor.currentIndex = pos.targetIndex;
            motor.currentEntry = entry;
            go.transform.LookAt(entry.waypoints[pos.targetIndex]);
            motor.system = system;
            Debug.Log("traffico; x = " + go.transform.position.x + "; y = " + go.transform.position.y + "; z = " + go.transform.position.z);
            Debug.Log("id = " + id + "; subId = " + subId + "; currentIndex = " + motor.currentIndex);
            motor.Init();
        }
    }

    public void Kill()
    {
        var allTraffic = Object.FindObjectsOfType(typeof(TrafAIMotor)) as TrafAIMotor[];
        foreach(var t in allTraffic)
        {
            if (t.gameObject.tag.Equals("Player"))
            {
                //non devo distruggere la mia macchina che se in guida automatica ha associato TrafAIMotor
                continue;
            }
            GameObject.Destroy(t.gameObject);
        }
        
    }

    bool spawned = false;

    public bool GetState()
    {
        return spawned;
    }

    public void SetTraffic(bool state)
    {
        if(spawned && !state)
        {
            Kill();
            spawned = false;
        }
        else if(!spawned && state)
        {
            SpawnHeaps();
            spawned = true;
        }
    }

    void OnGUI()
    {
        if(Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.U)
        {
            numberToSpawn = mediumDensity;
            if(spawned)
                Kill();
            else
                SpawnHeaps();

            spawned = !spawned;
        }

        else if(Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.H) {
            if (spawned)
                Kill();

            numberToSpawn = heavyDensity;
            SpawnHeaps();
            spawned = true;                
        }

        else if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.L)
        {
            if (spawned)
                Kill();

            numberToSpawn = lowDensity;
            SpawnHeaps();
            spawned = true;
        }

        //ANTONELLO
        else if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.A)
        {
            Debug.Log("Premuto A");
            if (!guidaAutomatica)
            {
                guidaAutomaticaSF();
            } else
            {
                fineGuidaAutomaticaSF();
            }
            
        }
    }
    //ANTONELLO
    private bool guidaAutomatica = false;
    //ANTONELLO
    private void fineGuidaAutomaticaSF()
    {
        GameObject go = GameObject.Find("XE_Rigged");
        if (go == null)
        {
            go = GameObject.Find("XE_Rigged(Clone)");
        }
        Destroy(go.GetComponent<TrafAIMotor>());
        guidaAutomatica = false;
    }
    //ANTONELLO
    private void guidaAutomaticaSF()
    {
        Debug.Log("sono in guidaAtuomaticaSF");
        //Vector3 tempPos = transform.position;
        int id = Random.Range(0, maxIdent);
        int subId = Random.Range(0, maxSub);
        float distance = Random.value * 0.8f + 0.1f;

        TrafEntry entry = system.GetEntry(id, subId);
        //TrafEntry entry = entryTraffico;
        Debug.Log("stampa: " + entry.ToString());
        if (entry == null)
        {
            Debug.Log("Entry = null");
            return;
        }
        InterpolatedPosition pos = entry.GetInterpolatedPosition(distance);
        //InterpolatedPosition pos = posTraffico;
        Debug.Log("pos.position: x = " + pos.position.x + "; y = " + pos.position.y + "; z = " + pos.position.z + "; targetIndex = " + pos.targetIndex);


        if (!Physics.CheckSphere(pos.position, checkRadius, 1 << LayerMask.NameToLayer("Traffic")))
        {
            GameObject go = GameObject.Find("XE_Rigged");   
            if (go == null)
            {
                go = GameObject.Find("XE_Rigged(Clone)");
            }
            GameObject nose = new GameObject("nose");
            nose.transform.SetParent(go.transform);        
            nose.transform.localPosition = new Vector3(0, 0.5f, 2f);
            nose.transform.localRotation = Quaternion.identity;
            nose.transform.localScale = new Vector3(2f, 2f, 2f);


            TrafAIMotor motor = go.AddComponent<TrafAIMotor>();


            GameObject colliderOstacoli = new GameObject("colliderOstacoli");            
            colliderOstacoli.transform.SetParent(go.transform);
            BoxCollider boxColliderOstacoli = colliderOstacoli.AddComponent<BoxCollider>();
            boxColliderOstacoli.isTrigger = true;
            colliderOstacoli.transform.localPosition = new Vector3(0f, 0.65f, 7f);
            colliderOstacoli.transform.localScale = new Vector3(1f, 1f, 1f);
            colliderOstacoli.transform.localRotation = Quaternion.identity;
            boxColliderOstacoli.size = new Vector3(3f, 0.75f, 10f);
            TrafAIMotor.GestoreCollisioni gestore = colliderOstacoli.AddComponent<TrafAIMotor.GestoreCollisioni>();
            gestore.setMotor(motor);

            //Debug.Log("creato oggetto nose");
            go.transform.position = pos.position;
            //go.transform.rotation = Quaternion.identity;
            
            //go.AddComponent<TrafWheels>();


            motor.currentIndex = pos.targetIndex;
            motor.currentEntry = entry;
            go.transform.LookAt(entry.waypoints[pos.targetIndex]);
            motor.system = system;
            motor.nose = nose;
            motor.raycastOrigin = nose.transform;
            motor.targetHeight = 0f;
            motor.waypointThreshold = 3f;

            Debug.Log("pos.position: x = " + pos.position.x + "; y = " + pos.position.y + "; z = " + pos.position.z + "; targetIndex = " + pos.targetIndex);
            guidaAutomatica = true;
            motor.Init();
            
            
        } else
        {
            guidaAutomaticaSF();
        }
        
    }


    

}
