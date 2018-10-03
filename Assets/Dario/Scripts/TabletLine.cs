using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabletLine : MonoBehaviour {

    Queue<Vector3> queue;
    LineRenderer lineRenderer;
    private VehicleController vehicleController;


	void Start ()
    {
        vehicleController = (VehicleController)FindObjectOfType(typeof(VehicleController));
        lineRenderer = transform.GetComponent<LineRenderer>();

        Vector3[] initialPos = new Vector3[lineRenderer.positionCount];
        lineRenderer.GetPositions(initialPos);
        queue = new Queue<Vector3>(initialPos);

        StartCoroutine(UpdateQueue());   
	}
	
    IEnumerator UpdateQueue()
    {
        while (queue.Count > 0 )
        {
            for (int i = 0; i < 10 && queue.Count > 0; i++)
                queue.Dequeue();
            lineRenderer.positionCount = queue.Count;
            lineRenderer.SetPositions(queue.ToArray());
            yield return new WaitForSeconds(.5f);
        }
    }
	
	
}
