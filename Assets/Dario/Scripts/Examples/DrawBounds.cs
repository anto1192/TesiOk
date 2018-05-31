using UnityEngine;



public class DrawBounds : MonoBehaviour
{
    public Color color_sphere = new Color(0.0f, 0.0f, 0.0f, 0.5f);
    public Color color_bounds = new Color(1.0f, 1.0f, 0.0f, 0.5f);

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

    void Start()
    {

        GameObject empty = new GameObject("daicazzo!");
        empty.transform.position = gameObject.transform.position;
        GameObject go1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go1.transform.position = ComputeBounds(gameObject).center;
        go1.transform.localScale = ComputeBounds(gameObject).size;
        go1.transform.SetParent(empty.transform);
        empty.transform.rotation = gameObject.transform.rotation;

    }

    //public void OnDrawGizmos()
    //{
    //    Matrix4x4 cubeT = Matrix4x4.TRS(ComputeBounds(gameObject).center, gameObject.transform.rotation, ComputeBounds(gameObject).size);
    //    Matrix4x4 old = Gizmos.matrix;
    //    Gizmos.matrix *= cubeT;
    //    Gizmos.color = new Color(1.0f, 1.0f, 0.0f, 0.5f);
    //    Gizmos.DrawCube(Vector3.zero, Vector3.one);
    //    Gizmos.matrix = old;
    //}
}