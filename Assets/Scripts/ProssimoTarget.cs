using UnityEngine;
using System.Collections;

public class ProssimoTarget : MonoBehaviour
{

    private Vector3 target;
    private bool incrocio;

    public ProssimoTarget(Vector3 target, bool incrocio)
    {
        this.target = target;
        this.incrocio = incrocio;
    }
}
