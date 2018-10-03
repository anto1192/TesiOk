using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TabletLine : MonoBehaviour {

    Queue<Vector3> queue;
    LineRenderer lineRenderer;
    private GameObject playerCar;
    private TrafAIMotor trafAIMotor;
    public float soglia = 40f;

	void Start ()
    {
        playerCar = GameObject.Find("TeslaModelS_MOD");
        
        lineRenderer = transform.GetComponent<LineRenderer>();

        Vector3[] initialPos = new Vector3[lineRenderer.positionCount];
        lineRenderer.GetPositions(initialPos);
        queue = new Queue<Vector3>(initialPos);

        //StartCoroutine(ExecuteUpdateQueue());
    }

    private void FixedUpdate()
    {
        trafAIMotor = playerCar.GetComponent<TrafAIMotor>();
        if (trafAIMotor != null && trafAIMotor.velocitaAttuale >= 0.1f)
        {
            for(int i = 0; i < 10; i++)
            {
                Vector3 point1 = queue.ElementAt(i);
                float distToPoint1 = Vector3.Distance(point1, trafAIMotor.transform.position);
                Debug.DrawLine(trafAIMotor.transform.position, point1, Color.blue);
                Vector3 heading = (point1 - trafAIMotor.transform.position).normalized;
                float dot = Vector3.Dot(trafAIMotor.transform.forward, heading);
                float normalDist = distToPoint1 * dot;
                Debug.DrawLine(trafAIMotor.transform.position, trafAIMotor.transform.position + (trafAIMotor.transform.forward * normalDist), Color.blue);
                if (normalDist <= soglia)
                    queue.Dequeue();
            }

            lineRenderer.positionCount = queue.Count;
            lineRenderer.SetPositions(queue.ToArray());
        }
    }

    //IEnumerator UpdateQueue()
    //{
    //    while (queue.Count > 0 /*&& trafAIMotor.currentSpeed >= 0.1f*/)
    //    {
    //        //for (int i = 0; i < Mathf.RoundToInt(trafAIMotor.velocitaAttuale) && queue.Count > 0; i++)
    //        //    queue.Dequeue();
    //        //lineRenderer.positionCount = queue.Count;
    //        //lineRenderer.SetPositions(queue.ToArray());
    //        //yield return new WaitForSeconds(.02f);
            

    //        yield return ;
    //    }
    //}

    //IEnumerator ExecuteUpdateQueue()
    //{
    //    while (true)
    //    {
    //        trafAIMotor = playerCar.GetComponent<TrafAIMotor>();
    //        if (trafAIMotor != null)
    //        {
    //            StartCoroutine(UpdateQueue());
    //            yield break;
    //        }
    //        else
    //            yield return null;
    //    }
    //} 



}
