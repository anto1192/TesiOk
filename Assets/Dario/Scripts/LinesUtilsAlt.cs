using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class LinesUtilsAlt : LinesUtils
{

    private const int SPLINE_GIZMO_SMOOTHNESS = 10;

    public void DrawLine(GameObject car, CurvySpline curvySpline, float carTf)
    {
        float increaseAmount = 1f / (curvySpline.Length * SPLINE_GIZMO_SMOOTHNESS);
        int direction = 1;
        float endTf = carTf;
        curvySpline.MoveByLengthFast(ref endTf, ref direction, 75.0f, CurvyClamping.Clamp); //this is to determine a constant length of the curve drawn, since Tf isn't proportional to the curve length, so you can't use a constant value!

        for (float i = carTf; i < endTf; i += increaseAmount)
        {
            Vector3 pos = curvySpline.Interpolate(i);
            LinePoints.Add(pos);
        }

        lineRenderer.positionCount = LinePoints.Count;
        lineRenderer.SetPositions(LinePoints.ToArray());
        LinePoints.Clear();
    } //NavigationLine of PlayerCar and TrafficCar in PCH

    public void DrawLine(GameObject car)
    {
        List<Vector3> initialList = new List<Vector3>();
        TrafAIMotor trafAIMotor = car.GetComponent<TrafAIMotor>();

        if (trafAIMotor != null)
        {
            bool hasNextEntryOk;
            TrafEntry nextEntryOk;
            int index;
            if (car.CompareTag("Player"))
            {
                //tesla
                hasNextEntryOk = trafAIMotor.hasNextEntry50;
                nextEntryOk = trafAIMotor.nextEntry50;
                if (trafAIMotor.hasNextEntry)
                {
                    index = trafAIMotor.currentFixedNode;
                }
                else
                {
                    index = trafAIMotor.currentFixedNode + 1;
                }
            }
            else
            {
                hasNextEntryOk = trafAIMotor.hasNextEntry;
                nextEntryOk = trafAIMotor.nextEntry;
                index = trafAIMotor.currentFixedNode;
            }
            if (hasNextEntryOk)
            {//I am waiting at the intersection
                TrafEntry nextRoadWaypoints = trafAIMotor.system.GetEntry(trafAIMotor.fixedPath[index].id, trafAIMotor.fixedPath[index].subId); //points of the next piece of road
                if (nextRoadWaypoints.waypoints.Count == 2)
                {
                    LinePoints.Add(car.transform.position);
                    List<Vector3> smoothedPoints = ChaikinCurve(nextEntryOk.waypoints.ToArray(), 4); //intersection points
                    initialList.AddRange(smoothedPoints);
                    initialList.Add(nextRoadWaypoints.waypoints[nextRoadWaypoints.waypoints.Count - 1]);
                }
                else if (nextRoadWaypoints.waypoints.Count > 2) 
                {
                    List<Vector3> smoothedPoints = new List<Vector3>();
                    smoothedPoints.Add(car.transform.position);
                    smoothedPoints.Add(nextEntryOk.waypoints[0]); //intersection points
                    smoothedPoints.AddRange(nextRoadWaypoints.waypoints);
                    initialList.AddRange(ChaikinCurve(smoothedPoints.ToArray(), 2));
                }
            }
            else if (!hasNextEntryOk  && trafAIMotor.currentEntry.isIntersection())
            {//I am crossing the intersection
                TrafEntry nextRoadWaypoints = trafAIMotor.system.GetEntry(trafAIMotor.fixedPath[trafAIMotor.currentFixedNode].id, trafAIMotor.fixedPath[trafAIMotor.currentFixedNode].subId); //points of the next piece of road
                if (nextRoadWaypoints.waypoints.Count == 2)
                {
                    LinePoints.Add(car.transform.position);
                    List<Vector3> smoothedPoints = ChaikinCurve(trafAIMotor.currentEntry.waypoints.ToArray(), 4);
                    int nearest = CalculatePointDistance(smoothedPoints, car);
                    Vector3 heading = smoothedPoints[nearest] - car.transform.position;
                    if (Vector3.Dot(heading, car.transform.forward) > 0)
                        for (int i = nearest; i < smoothedPoints.Count; i++)
                            initialList.Add(smoothedPoints[i]);
                    else
                        for (int i = nearest + 1; i < smoothedPoints.Count; i++)
                            initialList.Add(smoothedPoints[i]);
                    initialList.Add(nextRoadWaypoints.waypoints[nextRoadWaypoints.waypoints.Count - 1]);
                }   
                else if (nextRoadWaypoints.waypoints.Count > 2) //if more than 2, smooth and add all of them
                {
                    List<Vector3> smoothedPoints = new List<Vector3>();
                    smoothedPoints.Add(car.transform.position);
                    smoothedPoints.Add(trafAIMotor.currentEntry.waypoints[0]); //intersection points
                    smoothedPoints.AddRange(nextRoadWaypoints.waypoints);
                    smoothedPoints = ChaikinCurve(smoothedPoints.ToArray(), 2);
                    int nearest = CalculatePointDistance(smoothedPoints, car);
                    Vector3 heading = smoothedPoints[nearest] - car.transform.position;
                    if (Vector3.Dot(heading, car.transform.forward) > 0)
                        for (int i = nearest; i < smoothedPoints.Count; i++)
                            initialList.Add(smoothedPoints[i]);
                    else
                        for (int i = nearest + 1; i < smoothedPoints.Count; i++)
                            initialList.Add(smoothedPoints[i]);
                    //initialList.AddRange(smoothedPoints);
                }
            }
            else if (!hasNextEntryOk && !trafAIMotor.currentEntry.isIntersection())
            {//I am on curves or on straight lines
                LinePoints.Add(car.transform.position);
                if (trafAIMotor.currentEntry.waypoints.Count == 2)
                    initialList.Add(trafAIMotor.currentEntry.waypoints[trafAIMotor.currentEntry.waypoints.Count - 1]);
                else if (trafAIMotor.currentEntry.waypoints.Count > 2)
                {
                    List<Vector3> pointsToSmooth = ChaikinCurve(trafAIMotor.currentEntry.waypoints.ToArray(), 2);
                    int nearest = CalculatePointDistance(pointsToSmooth, car);
                    Vector3 heading = pointsToSmooth[nearest] - car.transform.position;
                    if (Vector3.Dot(heading, car.transform.forward) > 0)
                        for (int i = nearest; i < pointsToSmooth.Count; i++)
                            initialList.Add(pointsToSmooth[i]);
                    else
                        for (int i = nearest + 1; i < pointsToSmooth.Count; i++)
                            initialList.Add(pointsToSmooth[i]);

                }
            }

            foreach (var p in initialList)
            {
                Vector3 currentSpot = p;
                RaycastHit hit;
                Physics.Raycast(currentSpot + Vector3.up * 5, -Vector3.up, out hit, 100f, (1 << LayerMask.NameToLayer("EnvironmentProp"))); //the layer is EnvironmentProp and not Roads since there is a hidden mesh before roads!
                Vector3 correctedPoint = new Vector3(currentSpot.x, hit.point.y + 0.05f, currentSpot.z);
                LinePoints.Add(correctedPoint);
            }

            lineRenderer.positionCount = LinePoints.Count;
            lineRenderer.SetPositions(LinePoints.ToArray());
            LinePoints.Clear();
        }
    } //NavigationLine of PlayerCar and TrafficCar in SF

    public void DrawCenterLine(float carTf, PlayerCarLines.Lane lane, CurvySpline curvySpline, Texture2D lineRendererTxt)
    {
        float increaseAmount = 1f / (curvySpline.Length * SPLINE_GIZMO_SMOOTHNESS);
        
        float endTf = carTf;
        
        if (lane.Equals(PlayerCarLines.Lane.RIGHT))
        {
            int direction = 1;
            curvySpline.MoveByLengthFast(ref endTf, ref direction, 75.0f, CurvyClamping.Clamp); //this is to determine a constant length of the curve drawn, since Tf isn't proportional to the curve length, so you can't use a constant value!
            for (float i = carTf; i < endTf; i += increaseAmount)
            {
                Vector3 pos = curvySpline.Interpolate(i);
                CenterPoints.Add(pos);
            }

        }
        else
        {
            int direction = -1;
            curvySpline.MoveByLengthFast(ref endTf, ref direction, 75.0f, CurvyClamping.Clamp); //this is to determine a constant length of the curve drawn, since Tf isn't proportional to the curve length, so you can't use a constant value!
            for (float i = carTf; i > endTf; i -= increaseAmount)
            {
                Vector3 pos = curvySpline.Interpolate(i);
                CenterPoints.Add(pos);
            }
        }

        lineRenderer2.positionCount = CenterPoints.Count;
        lineRenderer2.SetPositions(CenterPoints.ToArray());
        lineRenderer2.colorGradient = MakeLineRendererGradient(centerLineColor);
        lineRenderer2.material.SetColor("_Color", CenterLineColor); //set tint color of particle shader
        Texture2D txt = UpdateParams(centerLineColor, lineRendererTxt);
        lineRenderer2.material.SetTexture("_MainTex", txt);
        CenterPoints.Clear();
    } //centerLine of PlayerCar in PCH

    public void DrawCenterLine(GameObject car, float carTf, CurvySpline curvySpline, Texture2D lineRendererTxt)
    {
        Vector3 heading = curvySpline.Interpolate(0) - car.transform.position;
        float increaseAmount = 1f / (curvySpline.Length * SPLINE_GIZMO_SMOOTHNESS);
        if (Vector3.Dot(heading, car.transform.forward) > 0) //starting point is in front of me
        {
            for (float i = carTf; i > 0; i -= increaseAmount)
            {
                Vector3 pos = curvySpline.Interpolate(i);
                CenterPoints.Add(pos);
            }
        }
        else //starting point is behind me
        {
            for (float i = carTf; i < 1; i += increaseAmount)
            {
                Vector3 pos = curvySpline.Interpolate(i);
                CenterPoints.Add(pos);
            }
        }

        lineRenderer2.positionCount = CenterPoints.Count;
        lineRenderer2.SetPositions(CenterPoints.ToArray());
        lineRenderer2.colorGradient = MakeLineRendererGradient(centerLineColor);
        lineRenderer2.material.SetColor("_Color", CenterLineColor); //set tint color of lineRenderer
        Texture2D txt = UpdateParams(centerLineColor, lineRendererTxt);
        lineRenderer2.material.SetTexture("_MainTex", txt);
        CenterPoints.Clear();
    } //centerLine of PlayerCar in SF

    private int CalculatePointDistance(List<Vector3> points, GameObject car)
    {
        int nearest = 0; //in URBAN I considr the nearest point otherwise the furthest would be a point located in the near lanes
        float nearDist = 9999;
        for (int i = 0; i < points.Count - 1; i++)
        {
            float thisDist = (car.transform.position - points[i]).sqrMagnitude; //this is squaredMagnitude i.e. magnitude without square root
            if (thisDist < nearDist)
            {
                nearDist = thisDist;
                nearest = i;
            }
        }
        return nearest;
    }

    private List<Vector3> MakeSmoothCurve(Vector3[] arrayToCurve, float smoothness)
    {
        List<Vector3> points;
        List<Vector3> curvedPoints;
        int pointsLength = 0;
        int curvedLength = 0;

        if (smoothness < 1.0f) smoothness = 1.0f;

        pointsLength = arrayToCurve.Length;

        curvedLength = (pointsLength * Mathf.RoundToInt(smoothness)) - 1;
        curvedPoints = new List<Vector3>(curvedLength);

        float t = 0.0f;
        for (int pointInTimeOnCurve = 0; pointInTimeOnCurve < curvedLength + 1; pointInTimeOnCurve++)
        {
            t = Mathf.InverseLerp(0, curvedLength, pointInTimeOnCurve);

            points = new List<Vector3>(arrayToCurve);

            for (int j = pointsLength - 1; j > 0; j--)
            {
                for (int i = 0; i < j; i++)
                {
                    points[i] = (1 - t) * points[i] + t * points[i + 1];
                }
            }

            curvedPoints.Add(points[0]);
        }

        return curvedPoints;
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





//foreach (var s in nextRoadWaypoints.waypoints)
//{
//    GameObject game = new GameObject("Node");
//    game.transform.position = s;
//    SphereCollider sphereCol = game.AddComponent<SphereCollider>();
//    sphereCol.isTrigger = true;
//}
