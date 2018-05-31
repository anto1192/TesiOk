using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{

    private float Timer = 0.1f;
    List<GameObject> list = new List<GameObject>();

    // Update is called once per frame
    void Update()
    {
        LayerMask mask = (1 << LayerMask.NameToLayer("Traffic")) | (1 << LayerMask.NameToLayer("obstacle")) | (1 << LayerMask.NameToLayer("EnvironmentProp")) | (1 << LayerMask.NameToLayer("Signage")); //restrict OverlapSphere only to the layers of interest
        Collider[] colls = Physics.OverlapSphere(gameObject.transform.position, 50.0f, mask);

        Timer -= Time.deltaTime;
        if (Timer <= 0f)
        {
            foreach (GameObject g in list)
            {
                Destroy(g);
            }
            list.Clear();


            foreach (Collider c in colls) {

                Bounds bounds = CalculateRendererBounds(c.transform);
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.transform.position = new Vector3(bounds.center.x, bounds.center.y, bounds.center.z);
                go.transform.rotation = c.transform.rotation;
                go.transform.localScale = new Vector3(bounds.size.x, bounds.size.y, bounds.size.z);
                list.Add(go);
                go.layer = LayerMask.NameToLayer("Graphics");
            }
            Timer = 0.1f;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(gameObject.transform.position, 50.0f); //this is for debugging purposes: see OverlapSphere bounds
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
