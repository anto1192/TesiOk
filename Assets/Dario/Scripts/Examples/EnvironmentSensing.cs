using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentSensing : MonoBehaviour {

    //private List<Collider> oldColls = new List<Collider>();
    private Dictionary<Collider, GameObject> oldColls = new Dictionary<Collider, GameObject>();
    private GameObject go = null;
    // Use this for initialization
    void Start() {
        

    }

    // Update is called once per frame
    void Update() {
        EnvironmentDetect();
        
    }

    void OnDrawGizmos() {

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(gameObject.transform.position, 50.0f);
    }

    public void EnvironmentDetect() {
        
        Vector3 cubeOffset = Vector3.zero;
        LayerMask mask = (1 << LayerMask.NameToLayer("Traffic")) | (1 << LayerMask.NameToLayer("obstacle")) | (1 << LayerMask.NameToLayer("EnvironmentProp")) | (1 << LayerMask.NameToLayer("Signage"));
        Collider[] colls = Physics.OverlapSphere(gameObject.transform.position, 50.0f, mask);
        //List<Collider> newColls = new List<Collider>(colls);
        Dictionary<Collider, GameObject> newColls = new Dictionary<Collider, GameObject>();
        foreach (Collider c in colls) 
            newColls.Add(c, null);
        
            
        //GameObject[] objects = new GameObject[newColls.Count];

        //if(oldColls.Count != 0){
            foreach (var item in oldColls.Keys)
            {
                if (!newColls.ContainsKey(item)) {
                    Destroy(oldColls[item]);
                    oldColls.Remove(item);
                       
                }
            }
        //}

        //if (newColls.Count != 0) {
            foreach (var item in newColls.Keys)
            {
            Debug.Log(item);
                if (!oldColls.ContainsKey(item)) {

                    if (item.gameObject.isStatic) {
                    cubeOffset = item.gameObject.GetComponent<Renderer>().bounds.center;
                } else {
                    cubeOffset = item.gameObject.transform.position;
                }
                go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.transform.position = new Vector3(cubeOffset.x, cubeOffset.y, cubeOffset.z);
                go.transform.localScale = new Vector3(3, 3, 3);
                newColls[item] = go;
                oldColls.Add(item, newColls[item]);    
            }
            if (item.gameObject.transform != oldColls[item].gameObject.transform) {
                go.transform.position = new Vector3(item.gameObject.transform.position.x, item.gameObject.transform.position.y, item.gameObject.transform.position.z);
            }




            }
        //}
    }
    

   
    

}

    



   
