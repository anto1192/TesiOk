using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinesUtils { //maybe I have to consider if substituting the two lineRenderer/Texture2D variables with Lists can be a better choice

    protected GameObject lineRendererEmpty = null; //this is to store the Empty which contains the LineRenderer Component of the main navigation line
    protected GameObject centerLineRendererEmpty = null; //this is to store the Empty which contains the LineRenderer Component of the center line
    protected List<Vector3> LinePoints = new List<Vector3>();
    protected List<Vector3> CenterPoints = new List<Vector3>();
    protected LineRenderer lineRenderer; //lineRenderer of the main navigation line for Player or TrafficCars
    protected LineRenderer lineRenderer2; //lineRenderer of the center line for Player
    protected Gradient centerLineLerperGradient; //this is to return the right centerLineColor based on the distance of the car from the centerLine
    protected Color32 centerLineColor;
    protected Texture2D glowTexture; //create on-the-fly 16x16 texture useful to enable glow effect for LineRenderer
    protected Texture2D glowTexture2;//create on-the-fly 16x16 texture useful to enable glow effect for LineRenderer2
    public GameObject LineRendererEmpty { get { return lineRendererEmpty; } }
    public GameObject CenterLineRendererEmpty { get { return centerLineRendererEmpty; } }
    public LineRenderer LineRend { get { return lineRenderer; } set { lineRenderer = value; } }
    public LineRenderer LineRend2 { get { return lineRenderer2; } set { lineRenderer2 = value; } }
    public Color32 CenterLineColor { get { return centerLineColor; } set { centerLineColor = value; } }


    public void DrawLine(Transform tr, GameObject car, GameObject path)
    {
        int index = tr.GetSiblingIndex(); //this is the index of the furthest node within the circle, considering autopathEnhanced
        index++; //I consider the next node because in this way I am sure that the first point is car.transform.position and the second is further that car.transform.position
        LinePoints.Add(car.transform.position);
        for (int i = 0; i < 15; i++)
            LinePoints.Add(path.transform.GetChild(index + i).position);
        lineRenderer.positionCount = LinePoints.Count;
        lineRenderer.SetPositions(LinePoints.ToArray());
        LinePoints.Clear();
    }

    public void DrawLine(Transform tr, GameObject car, GameObject path, Vector3 projected, int direction)
    {
        int index = tr.GetSiblingIndex(); //this is the index of the furthest node within the circle, considering GuardrailPCH
        
        Debug.DrawLine(car.transform.position, projected, Color.magenta);
        CenterPoints.Add(projected);
        if (direction == 0)
        { //car is aligned with Centerpoints so line direction has to be + i
            index++;
            for (int i = 0; i < 15; i++)
                CenterPoints.Add(path.transform.GetChild(index + i).position);
        }
        else {
            index--;
            for (int i = 0; i < 15; i++)
                CenterPoints.Add(path.transform.GetChild(index - i).position);
        }

        lineRenderer2.positionCount = CenterPoints.Count;
        lineRenderer2.SetPositions(CenterPoints.ToArray());
        lineRenderer2.colorGradient = MakeLineRendererGradient(centerLineColor);
        lineRenderer2.material.SetColor("_Color", CenterLineColor); //set tint color of particle shader
        glowTexture2 = UpdateGlowParams(centerLineColor);
        lineRenderer2.material.SetTexture("_MKGlowTex", glowTexture2);
        CenterPoints.Clear();
    }  //centerLine

    public virtual void DrawLineUrban(Transform tr, GameObject car)
    {
        LinePoints.Add(car.transform.position);
        Vector3[] waypoints = tr.GetComponent<SFNodeInfo>().GetPoints();
        if (waypoints.Length <= 15)
            for (int i = 1; i < waypoints.Length; ++i) //consider next index from current
                LinePoints.Add(waypoints[i]);
        else
            for (int i = 1; i < 15; ++i)
                LinePoints.Add(waypoints[i]);
        lineRenderer.positionCount = LinePoints.Count;
        lineRenderer.SetPositions(LinePoints.ToArray());
        LinePoints.Clear();
    }

    public virtual void DrawLineUrban(Transform tr, GameObject car, Vector3 projected, int direction)
    {
        Debug.DrawLine(car.transform.position, projected, Color.magenta);
        CenterPoints.Add(projected);
        Vector3[] waypoints = tr.GetComponent<SFNodeInfo>().GetPoints();
        if (waypoints.Length <= 15)
            for (int i = 1; i < waypoints.Length; ++i) //consider next index from current
                if (direction == 0)
                    CenterPoints.Add(waypoints[i]);
                else
                    CenterPoints.Add(waypoints[-i]);
        else
            for (int i = 1; i < 15; ++i) //consider next index from current
                if (direction == 0)
                    CenterPoints.Add(waypoints[i]);
                else
                    CenterPoints.Add(waypoints[-i]);

        lineRenderer2.positionCount = CenterPoints.Count;
        lineRenderer2.SetPositions(CenterPoints.ToArray());
        lineRenderer2.colorGradient = MakeLineRendererGradient(centerLineColor);
        lineRenderer2.material.SetColor("_Color", CenterLineColor); //set tint color of particle shader
        glowTexture2 = UpdateGlowParams(centerLineColor);
        lineRenderer2.material.SetTexture("_MKGlowTex", glowTexture2);
        CenterPoints.Clear();
    }

    public LineRenderer CreateLineRenderer(GameObject empty, string name, float lineWidth, Color32 col, Material mat, Func<Texture2D> initTxt)
    {
        LineRenderer lineR;
        empty = new GameObject(name);
        empty.layer = LayerMask.NameToLayer("Graphics");
        //empty.transform.localEulerAngles = new Vector3(90, 90, 0);
        empty.transform.rotation = Quaternion.Euler(90, 0, 0);
        
        lineR = empty.AddComponent<LineRenderer>();
        lineR.useWorldSpace = true;
        lineR.alignment = LineAlignment.Local;
        lineR.material = mat;
        lineR.material.SetColor("_Color", col); //set tint color of particle shader
        lineR.material.SetTexture("_MKGlowTex", initTxt()); //set texture passed by Func, of LineRenderer

        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0, lineWidth);
        curve.AddKey(1, lineWidth);
        lineR.widthCurve = curve;
        lineR.colorGradient = MakeLineRendererGradient(col);
        return lineR;
    }

    public Vector3 CalculateProjectedPoint(Vector3 pos1, Vector3 pos2, Vector3 pos3)
    {
        Vector3 a = pos1 - pos2;
        //Debug.DrawLine(pos1, pos2, Color.magenta);
        Vector3 b = pos3 - pos2;
        //Debug.DrawLine(pos3, pos2, Color.magenta);
        Vector3 projectedPoint = Vector3.Project(a, b) + pos2;
        return projectedPoint;
    }

    public Gradient MakeLineRendererGradient(Color32 col)
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(col, 0.0f), new GradientColorKey(col, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
            );

        return gradient;

    }

    public void CenterLineLerper()
    {
        centerLineLerperGradient = new Gradient();
        centerLineLerperGradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.red, 0.0f), new GradientColorKey(Color.yellow, 0.5f), new GradientColorKey(Color.white, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
            );
    }

    public Color32 ChangeMatByDistance(float dist)
    {
        float normDist = dist / 4.0f;
        return centerLineLerperGradient.Evaluate(normDist);
    }

    public Texture2D InitGlowTexture()
    {
        glowTexture = UpdateGlowParams(new Color32(0x3A, 0xC1, 0xAA, 0xFF));
        return glowTexture;
    }

    public Texture2D InitGlowTexture2()
    {
        glowTexture2 = UpdateGlowParams(new Color32(0xFF, 0xFF, 0xFF, 0xFF));
        return glowTexture2;
    }

    protected Texture2D UpdateGlowParams(Color32 col)
    {
        Texture2D txt = new Texture2D(16, 16);
        Color32[] fillColorArray = txt.GetPixels32();
        for (int i = 0; i < fillColorArray.Length; ++i)
            fillColorArray[i] = col;

        txt.SetPixels32(fillColorArray);
        txt.Apply();
        return txt;
    }
}
