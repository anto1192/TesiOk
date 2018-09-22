/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MacchinaSorpassoPCH : MonoBehaviour
{
    public Transform raycastOrigin;
    public CarAutoPath path;
    public int currentWaypointIndex;
    public RoadPathNode currentNode;
    private Vector3 currentWaypoint;
    private RoadPathNode nextNode;
    private int nextWaypointIndex;
    private Vector3 nextWaypoint;
    private RoadPathNode prevNode;
    private Vector3 prevWaypoint;

    private Vector3 prevTangent;
    private Vector3 currentTangent;

    public float waypointThreshold = 3f;
    public float maxSpeed = 20f;
    public float brakeDistance = 15f;
    public float minBrakeDist = 1.5f;

    public float maxThrottle = 0.8f;
    public float maxBrake = 0.4f;
    public float steerSpeed = 4.0f;
    public float throttleSpeed = 3.0f;
    public float brakeSpeed = 1f;
    private float m_targetSteer = 0.0f;


    private float targetSpeed = 0f;

    public float steerTargetDist = 16f;

    public bool reverse = false;

    public bool backArc = false;

    public bool straightAtFinal = true;


    public float predictLength = 10f;
    public float normalAdd = 5f;
    public float pathRadius = 1f;

    public float maxTurnAngle = 50f;
    public float turnAngleDivider = 60f;

    public float currentSpeed;
    public float currentTurn;

    RaycastHit hitInfo;

    Vector3 wp0;
    Vector3 wp1;
    Vector3 wp2;
    Vector3 wp3;
    Vector3 wp4;
    Vector3 wp5;
    Vector3 wp6;
    Vector3 wp7;
    Vector3 wp8;
    Vector3 wp9;

    void Start()
    {
        hitInfo = new RaycastHit();
        
    }

    public void Init()
    {

        /*wp0 = GameObject.Find("wp0").transform.position;
        wp1 = GameObject.Find("wp1").transform.position;
        wp2 = GameObject.Find("wp2").transform.position;
        wp3 = GameObject.Find("wp3").transform.position;
        wp4 = GameObject.Find("wp4").transform.position;
        wp5 = GameObject.Find("wp5").transform.position;
        wp6 = GameObject.Find("wp6").transform.position;
        wp7 = GameObject.Find("wp7").transform.position;
        wp8 = GameObject.Find("wp8").transform.position;
        wp9 = GameObject.Find("wp9").transform.position;*/

        wp0 = new Vector3(-1283.71f, 142.58f, 1387.47f);
        wp1 = new Vector3(-1285.2f, 142.58f, 1388.12f);       
        wp2 = new Vector3(-1293.8f, 142.7f, 1391.3f);
        wp3 = new Vector3(-1302.41f, 142.535f, 1392.83f);
        wp4 = new Vector3(-1310.52f, 142.535f, 1395.57f);
        wp5 = new Vector3(-1318.09f, 142.535f, 1398.64f);
        wp6 = new Vector3(-1325.74f, 142.82f, 1402.11f);
        wp7 = new Vector3(-1331.6f, 142.92f, 1406.4f);
        wp8 = new Vector3(-1336.1f, 142.85f, 1411.1f);
        wp9 = new Vector3(-1341.94f, 143.16f, 1415.09f);


        GetComponent<TrafPCH>().enabled = false;
        Vector3[] lisWayp = new Vector3[10];

        lisWayp[0] = wp0;
        lisWayp[1] = wp1;
        lisWayp[2] = wp2;
        lisWayp[3] = wp3;
        lisWayp[4] = wp4;
        lisWayp[5] = wp5;
        lisWayp[6] = wp6;
        lisWayp[7] = wp7;
        lisWayp[8] = wp8;
        lisWayp[9] = wp9;
        listaWaypoint = ChaikinCurve(lisWayp, 3);
        int index = 0;
        foreach (Vector3 punto in listaWaypoint)
        {
            GameObject cubo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cubo.transform.position = punto;
            cubo.GetComponent<Collider>().isTrigger = true;
            RaycastHit hit;
            Physics.Raycast(cubo.transform.position + Vector3.up * 5, -transform.up, out hit, 100f, (1 << LayerMask.NameToLayer("Roads")));
            cubo.transform.position = new Vector3(punto.x, hit.point.y, punto.z);
            cubo.SetActive(false);
            listaWaypoint[index] = cubo.transform.position;
            Destroy(cubo);
            index++;
        }
        currentWaypoint = currentNode.position;
        nextWaypointIndex = currentWaypointIndex;
        UpdateNextWaypoint();

        prevWaypoint = currentWaypoint;
        prevNode = currentNode;


        currentWaypoint = nextWaypoint;
        currentNode = nextNode;
        currentWaypointIndex = nextWaypointIndex;
        UpdateNextWaypoint();


        if (prevNode.tangent == Vector3.zero)
        {
            prevTangent = (currentWaypoint - prevWaypoint).normalized;
        }
        else
        {
            prevTangent = (prevNode.tangent - prevWaypoint).normalized;
        }

        if (currentNode.tangent == Vector3.zero)
        {
            currentTangent = (nextWaypoint - currentWaypoint).normalized;
        }
        else
        {
            currentTangent = (currentNode.tangent - currentWaypoint).normalized;
        }


        targetSpeed = maxSpeed;

    }


    int contatore = 0;
    
    List<Vector3> listaWaypoint = new List<Vector3>();



    private void UpdateNextWaypoint()
    {
        if (!(contatore != 0 && contatore < listaWaypoint.Count))
        {
            nextWaypointIndex = currentWaypointIndex + 1;
            if (nextWaypointIndex >= path.pathNodes.Count)
                nextWaypointIndex = 0;
        } 
        
        

        if (nextWaypointIndex > 872 && nextWaypointIndex < 885 && contatore < listaWaypoint.Count) //877
        {
            nextNode = path.pathNodes[nextWaypointIndex];
            nextWaypoint = listaWaypoint[contatore];
            contatore++;
            return;
        }
        if (contatore == listaWaypoint.Count)
        {
            nextWaypointIndex = 884;
            contatore = 0;
        }

        nextNode = path.pathNodes[nextWaypointIndex];
        nextWaypoint = nextNode.position;

    }

    private Vector3 GetPredictedPoint()
    {
        return transform.position + GetComponent<Rigidbody>().velocity * Time.deltaTime * predictLength;
    }


    private Vector3 GetNormalPoint(Vector3 predicted, Vector3 A, Vector3 B)
    {
        Vector3 ap = (predicted - A);
        Vector3 ab = (B - A).normalized;

        float dot = Vector3.Dot(ap, ab);
        float dotNorm = Mathf.Abs(Vector3.Dot(ap.normalized, ab));

        return A + (ab * dot) + ab * (normalAdd * (1 - dotNorm));

    }


    Vector3 seek(Vector3 target, Vector3 location)
    {
        Vector3 desired = (target - location).normalized;
        return desired * maxSpeed;
    }

    float currentPc = 0f;
    Vector3 normal;

    void FixedUpdate()
    {





        if (currentNode.isInintersection)
            targetSpeed = 4f;
        else
            targetSpeed = maxSpeed;

        // if (Vector3.Distance(predicted, normal) < pathRadius)
        //     m_targetSteer = 0f;




        float speedDifference = targetSpeed - GetComponent<Rigidbody>().velocity.magnitude;

        currentSpeed += Mathf.Clamp(speedDifference * Time.deltaTime * throttleSpeed, -maxBrake, maxSpeed);

        MoveCar();
    }

    float currentTurnPrecedente = 0f;

    void MoveCar()
    {

        float thisDist = Vector3.Distance(currentWaypoint, prevWaypoint);

        currentPc += currentSpeed / thisDist * Time.fixedDeltaTime;


        if (currentPc > 1f)
        {

            prevWaypoint = currentWaypoint;
            prevNode = currentNode;
            prevTangent = currentTangent;

            currentWaypoint = nextWaypoint;
            currentNode = nextNode;
            currentWaypointIndex = nextWaypointIndex;
            UpdateNextWaypoint();

            if (currentNode.tangent == Vector3.zero)
                currentTangent = (nextWaypoint - currentWaypoint).normalized;
            else
            {
                currentTangent = (currentNode.tangent - currentWaypoint).normalized;
            }


            currentPc -= 1f;

            currentPc = (currentPc * thisDist) / Vector3.Distance(currentWaypoint, prevWaypoint);

        }

        Vector3 currentSpot = HermiteMath.HermiteVal(prevWaypoint, currentWaypoint, prevTangent, currentTangent, currentPc);



        //transform.Rotate(Vector3.up * currentTurn * Time.fixedDeltaTime);
        RaycastHit hit;
        Physics.Raycast(transform.position + Vector3.up * 5, -transform.up, out hit, 100f, ~(1 << LayerMask.NameToLayer("Traffic")));

        Vector3 hTarget = new Vector3(currentSpot.x, hit.point.y, currentSpot.z);


        Vector3 tangent;
        if (currentPc < 0.95f)
        {
            tangent = HermiteMath.HermiteVal(prevWaypoint, currentWaypoint, prevTangent, currentTangent, currentPc + 0.05f) - currentSpot;
        }
        else
        {
            tangent = currentSpot - HermiteMath.HermiteVal(prevWaypoint, currentWaypoint, prevTangent, currentTangent, currentPc - 0.05f);
        }
        tangent.y = 0f;
        tangent = tangent.normalized;


        //GetComponent<Rigidbody>().MoveRotation(Quaternion.FromToRotation(Vector3.up, hit.normal) * Quaternion.LookRotation(tangent));
        //transform.Rotate(Vector3.up * m_targetSteer  * Time.fixedDeltaTime);
        transform.LookAt(new Vector3(nextWaypoint.x, transform.position.y, nextWaypoint.z));
        GetComponent<Rigidbody>().MovePosition(hTarget);
        
        //transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);
    }

    public List<Vector3> ChaikinCurve(Vector3[] pts, int passes)
    {
        Stack<Vector3[]> stack = new Stack<Vector3[]>();
        stack.Push(pts);

        for (int k = 1; k <= passes; k++)
        {
            Vector3[] oldPts = stack.Pop();
            Vector3[] newPts = new Vector3[(oldPts.Length - 2) * 2 + 2];
            newPts[0] = oldPts[0];
            newPts[newPts.Length - 1] = oldPts[oldPts.Length - 1];

            int j = 1;
            for (int i = 0; i < oldPts.Length - 2; i++)
            {
                newPts[j] = oldPts[i] + (oldPts[i + 1] - oldPts[i]) * 0.75f;
                newPts[j + 1] = oldPts[i + 1] + (oldPts[i + 2] - oldPts[i + 1]) * 0.25f;
                j += 2;
            }           
            stack.Push(newPts);
        }
        return new List<Vector3>(stack.Pop());
    } //Chaikin algorithm is used to smooth SF navigation lines since they have discrete point that would result in a jagged line drawn

}
