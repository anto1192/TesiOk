using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerLogic : MonoBehaviour
{
    private void Start()
    {
        SphereCollider sphereCol = gameObject.AddComponent<SphereCollider>();
        sphereCol.isTrigger = true;
    }

    private void Update()
    {
        float currentSpeed = gameObject.GetComponent<VehicleController>().CurrentSpeed;
        int safetyDist = Mathf.RoundToInt(Mathf.Pow(currentSpeed / 10, 2));
        if (safetyDist < 50)
            safetyDist = 50;
        gameObject.GetComponent<SphereCollider>().radius = safetyDist;
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Tree")
        {
            Bounds bounds = CalculateRendererBounds(other.transform);
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.position = new Vector3(bounds.center.x, bounds.center.y, bounds.center.z);
            go.transform.rotation = other.transform.rotation;
            go.transform.localScale = new Vector3(bounds.size.x, bounds.size.y, bounds.size.z);
            go.layer = LayerMask.NameToLayer("Graphics");
        }
        
    }

    void OnTriggerExit(Collider other)
    {
        Destroy(other.gameObject);
    }

    public static Bounds CalculateRendererBounds(Transform tr)
    {

        Quaternion prevRot = tr.localRotation;
        Vector3 prevScale = tr.localScale;
        tr.localRotation = Quaternion.identity;
        tr.localScale = Vector3.one;
        Bounds bounds = new Bounds(tr.position, Vector3.one);
        Renderer[] children = tr.GetComponentsInChildren<Renderer>();
        if (children.Length > 0)
        {
            foreach (Renderer child in children)
                if (child != null)
                    bounds.Encapsulate(child.bounds);
        }
        else
        {
            SkinnedMeshRenderer[] skinnedChildren = tr.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer skinnedChild in skinnedChildren)
                if (skinnedChild != null)
                    bounds.Encapsulate(skinnedChild.bounds);
        }
        //Vector3 localCenter = bounds.center - obj.transform.position;
        //bounds.center = localCenter;
        Debug.Log("Bounds center of " + tr + "is " + bounds.center);
        Debug.Log("Bounds size of " + tr + "is " + bounds.size);
        tr.localRotation = prevRot;
        tr.localScale = prevScale;
        return bounds;
    }

}


