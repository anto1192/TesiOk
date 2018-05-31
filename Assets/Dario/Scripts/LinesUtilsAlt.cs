using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class LinesUtilsAlt : LinesUtils
{

    private const int SPLINE_GIZMO_SMOOTHNESS = 10;

    public void DrawLine(GameObject car, CurvySpline curvySpline, float carTf)
    {
        //CurvySpline curvySpline = path.GetComponent<CurvySpline>();
        //carTf = curvySpline.GetNearestPointTF(car.transform.position);
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
    }

    public void DrawLine(GameObject car)
    {
        List<Vector3> initialList = new List<Vector3>();

        if (car.GetComponent<TrafAIMotor>() != null)
        {
            LinePoints.Add(car.transform.position);
            TrafAIMotor trafAIMotor = car.GetComponent<TrafAIMotor>();
            if (trafAIMotor.hasNextEntry && trafAIMotor.nextEntry.isIntersection())
            {//I am waiting at the intersection
                List<Vector3> smoothedPoints = MakeSmoothCurve(trafAIMotor.nextEntry.waypoints.ToArray(), 5.0f);
                initialList.AddRange(smoothedPoints);
            }
            else if (!trafAIMotor.hasNextEntry && trafAIMotor.currentEntry.isIntersection())
            {//I am crossing the intersection
                List<Vector3> smoothedPoints = MakeSmoothCurve(trafAIMotor.currentEntry.waypoints.ToArray(), 5.0f);
                initialList.AddRange(smoothedPoints);
            }
            else if (!trafAIMotor.hasNextEntry && !trafAIMotor.currentEntry.isIntersection())
            {//I am on curves or on straight lines
                if (trafAIMotor.currentEntry.waypoints.Count == 2)
                    initialList.Add(trafAIMotor.currentEntry.waypoints[trafAIMotor.currentEntry.waypoints.Count - 1]);
                else if (trafAIMotor.currentEntry.waypoints.Count > 2)
                {
                    //foreach (var s in trafAIMotor.currentEntry.waypoints)
                    //{
                    //    GameObject game = new GameObject("Node");
                    //    game.transform.position = s;
                    //    SphereCollider sphereCol = game.AddComponent<SphereCollider>();
                    //}
                    int nearest = CalculatePointDistance(trafAIMotor.currentEntry.waypoints, car);
                    Vector3 heading = trafAIMotor.currentEntry.waypoints[nearest] - car.transform.position;
                    if (Vector3.Dot(heading, car.transform.forward) > 0)
                        for (int i = nearest; i < trafAIMotor.currentEntry.waypoints.Count - 1; i++)
                            initialList.Add(trafAIMotor.currentEntry.waypoints[i]);
                    else
                        for (int i = nearest + 1; i < trafAIMotor.currentEntry.waypoints.Count - 1; i++)
                            initialList.Add(trafAIMotor.currentEntry.waypoints[i]);
                }
            }

            foreach (var p in initialList)
            {
                Vector3 currentSpot = p;
                RaycastHit hit;
                Physics.Raycast(currentSpot + Vector3.up * 5, -Vector3.up, out hit, 100f, (1 << LayerMask.NameToLayer("EnvironmentProp"))); //the layer is EnvironmentProp and not Roads since there is a hidden mesh before roads!
                Vector3 correctedPoint = new Vector3(currentSpot.x, hit.point.y + 0.2f, currentSpot.z);
                LinePoints.Add(correctedPoint);
            }

            lineRenderer.positionCount = LinePoints.Count;
            lineRenderer.SetPositions(LinePoints.ToArray());
            LinePoints.Clear();
        }
    }

    public void DrawCenterLine(float carTf, EnvironmentSensingAltTrigger.Lane lane, CurvySpline curvySpline)
    {
        float increaseAmount = 1f / (curvySpline.Length * SPLINE_GIZMO_SMOOTHNESS);
        
        float endTf = carTf;
        
        if (lane.Equals(EnvironmentSensingAltTrigger.Lane.RIGHT))
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
        glowTexture2 = UpdateGlowParams(centerLineColor);
        lineRenderer2.material.SetTexture("_MKGlowTex", glowTexture2);
        CenterPoints.Clear();
    }

    public void DrawCenterLine(GameObject car, float carTf, CurvySpline curvySpline)
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
        lineRenderer2.material.SetColor("_Color", CenterLineColor); //set tint color of particle shader
        glowTexture2 = UpdateGlowParams(centerLineColor);
        lineRenderer2.material.SetTexture("_MKGlowTex", glowTexture2);
        CenterPoints.Clear();
    }

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

    //arrayToCurve is original Vector3 array, smoothness is the number of interpolations. 
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
}
