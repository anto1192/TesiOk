using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bbtest : MonoBehaviour {

    public GameObject go;
	// Use this for initialization
	void Start () {

        GameObject go1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go1.name = "daicazzo!";
        go1.transform.position = ComputeBounds(go).center;
        go1.transform.rotation = go.transform.rotation;
        go1.transform.localScale = ComputeBounds(go).size;
    }

    public void OnDrawGizmos()
    {
        ComputeBounds(go);
        Gizmos.color = new Color(1.0f, 1.0f, 0.0f, 0.5f);
        Gizmos.DrawCube(ComputeBounds(go).center, ComputeBounds(go).size);
    }

    public static Bounds CalculateRendererBounds(GameObject obj) {
        Quaternion prevRot = obj.transform.localRotation;
        Vector3 prevScale = obj.transform.localScale;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;
        Bounds bounds = new Bounds(obj.transform.position, Vector3.one);
        Renderer[] children = obj.GetComponentsInChildren<Renderer>();
        if (children.Length > 0) {
            foreach (Renderer child in children)
                if (child != null)
                    bounds.Encapsulate(child.bounds);
        } else {

            SkinnedMeshRenderer[] skinnedChildren = obj.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer s in skinnedChildren)
                Debug.Log("SkinnedMeshRenderer: " + s);

            foreach (SkinnedMeshRenderer skinnedChild in skinnedChildren) 
            if (skinnedChild != null)
                bounds.Encapsulate(skinnedChild.bounds);
        }


        //Vector3 localCenter = bounds.center - obj.transform.position;
        //bounds.center = localCenter;
        Debug.Log("Bounds center of " + obj.transform + "is " + bounds.center);
        Debug.Log("Bounds size of " + obj.transform + "is " + bounds.size);
        obj.transform.localRotation = prevRot;
        obj.transform.localScale = prevScale;
        return bounds;
    }

    public static Bounds ComputeBounds(GameObject obj)
    {
        Quaternion currentRotation = obj.transform.rotation;
        obj.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        Bounds b = new Bounds();

        Renderer[] r = obj.GetComponentsInChildren<Renderer>();

        for (int ir = 0; ir < r.Length; ir++)
        {
            Renderer renderer = r[ir];
            if (ir == 0)
                b = renderer.bounds;
            else
                b.Encapsulate(renderer.bounds);
        }

        obj.transform.rotation = currentRotation;

        return b;
    }

    Vector3 CalculateBoundingBox(GameObject aObj) {
        
        MeshFilter mF = aObj.GetComponent<MeshFilter>();

        Vector3[] vertices = mF.mesh.vertices;
        
        Vector3 min, max;
        min = max = aObj.transform.TransformPoint(vertices[0]);
        for (int i = 1; i < vertices.Length; i++)
        {
            Vector3 V = aObj.transform.TransformPoint(vertices[i]);
            for (int n = 0; n < 3; n++)
            {
                if (V[n] > max[n])
                    max[n] = V[n];
                if (V[n] < min[n])
                    min[n] = V[n];
            }
        }
        Debug.DrawLine(min, max, Color.green);
        Debug.Log(min);
        Debug.Log(max);
        Vector3 size = Vector3.zero;
        size.x = (max.x - min.x);
        size.y = max.y - min.y;
        size.z = max.z - min.z;

        return size;
    }

}
