using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleDetection : MonoBehaviour {

    public float sensorLength = 8f;
    public GameObject glowingCar;
    public GameObject dangerSignal;
    private bool gC; //bool to control that the spawning of glowingCarClone happens once.
    private bool dS; // bool to control that the spawning of dangerSignalClone happens once.
    private RaycastHit hit;
    private GameObject glowingCarClone;
    private GameObject dangerSignalClone;
    private Gradient g;
    private Transform rayCastPos;



    private void Sensors()
    {
        Vector3 fwd = rayCastPos.TransformDirection(Vector3.forward);
        if (Physics.Raycast(rayCastPos.position, fwd, out hit, sensorLength, 1 << LayerMask.NameToLayer("Traffic")))
        {
            if (hit.rigidbody != null) {

                Debug.DrawLine(rayCastPos.position, hit.point, Color.green);
                if (!gC) 
                    CreateOrDestroyCarGlowing(hit);
                //glowingCarClone.GetComponent<Renderer>().material.color = ChangeMatByDistance(hit);
                glowingCarClone.GetComponent<Renderer>().material.color = ChangeMatByDistance2(hit, g);
                CreateOrDisableDangerSignal(hit);
                //StartCoroutine(HideUnhide(dangerSignalClone));
            }
        } else {
            Debug.DrawLine(rayCastPos.position, rayCastPos.position + (fwd * sensorLength), Color.red);
            CreateOrDestroyCarGlowing(hit);
            DestroyDangerSignal();
        }
        
    }

    void Start() {

        rayCastPos = GameObject.Find("rayCastPos").transform;
        gC = false;
        dS = false;
        MakeGradient();
    }

    private void FixedUpdate() {

        Sensors();
    }

    void CreateOrDestroyCarGlowing(RaycastHit h) {

        Material mMaterial;

        if (!gC) {
            if (h.rigidbody != null) { //this control is fundamental since at the beginning gC is false. If the raycast doesn't hit anything this function is called and due to !gC it tries to instantiate every fixedUpdate. And U don't recognize the waste of resources since hit is null and instantiate fails! 
                //Debug.Log("se entro qui vuol dire ke sto istanziando!");
                glowingCarClone = Instantiate(glowingCar, new Vector3(hit.transform.position.x, hit.transform.position.y + 0.2f, hit.transform.position.z - 2.65f), Quaternion.identity, hit.transform);
                mMaterial = glowingCarClone.GetComponent<Renderer>().material;
                mMaterial.color = Color.white;
                glowingCarClone.GetComponent<Renderer>().material.color = mMaterial.color;
                gC = true;
            }                   
        } else {
                Destroy(glowingCarClone);
                gC = false;
            }  
        }
    
    
    Color ChangeMatByDistance(RaycastHit h)
    {  //This is a simple interpolation from red to white. Since i didn't like the transition I proposed another version which makes a gradient: from red to white passing through yellow. It is ChangeMatByDistance2
        Color end = Color.white;
        Color start = Color.red;
        return Color.Lerp(start, end, h.distance/sensorLength);
     }

    void MakeGradient() {
        GradientColorKey[] gck;
        GradientAlphaKey[] gak;
        g = new Gradient();
        gck = new GradientColorKey[3];
        gck[0].color = Color.red;
        gck[0].time = 0.0f;
        gck[1].color = Color.yellow;
        gck[1].time = 0.5f;
        gck[2].color = Color.white;
        gck[2].time = 1.0f;
        gak = new GradientAlphaKey[2];
        gak[0].alpha = 1.0f;
        gak[0].time = 0.0f;
        gak[1].alpha = 1.0f;
        gak[1].time = 1.0f;
        g.SetKeys(gck, gak);
    }

    Color ChangeMatByDistance2(RaycastHit h, Gradient grad) {
        return grad.Evaluate(h.distance/sensorLength);
    }

    void CreateOrDisableDangerSignal(RaycastHit h) {

        if (!dS) {
            if (h.distance / sensorLength <= 0.25f) {
                if (dangerSignalClone != null)
                    dangerSignalClone.GetComponent<Renderer>().enabled = true;
                else
                    dangerSignalClone = Instantiate(dangerSignal, new Vector3(hit.transform.position.x, hit.transform.position.y + 0.6f, hit.transform.position.z - 2.8f), Quaternion.identity, hit.transform);                    
                dS = true;
            }
        } else {
            if (h.distance / sensorLength > 0.25f) {
                dangerSignalClone.GetComponent<Renderer>().enabled = false;
                dS = false;
            }                    
        }
        
    }

    void DestroyDangerSignal() {
        if (dangerSignalClone != null) {
            Destroy(dangerSignalClone);
            dS = false;
        }
            
    }
            

    IEnumerator HideUnhide(GameObject g) {
        while (true)
        {
            yield return (new WaitForSeconds(0.5f));
            g.GetComponent<Renderer>().enabled = true;
            yield return (new WaitForSeconds(0.5f));
            g.GetComponent<Renderer>().enabled = false;
        }
    }
}


                



// Vector3 sensorStartPos = transform.position; //I need that the position of the starting point is in world space
//Debug.Log(sensorStartPos.ToString("F4"));
//sensorStartPos.z += rayOffsetSize.z * 0.5f;
//sensorStartPos += ((Vector3)(transform.worldToLocalMatrix * new Vector3(0, 0, rayOffsetSize.z) * 0.5f));
//if (Physics.Raycast(sensorStartPos, transform.localToWorldMatrix * Vector3.forward, out hit, sensorLength)) {
//Debug.Log("transform.forward is: " + transform.forward);
//Debug.Log("Vector3.forward is: " + Vector3.forward);
//