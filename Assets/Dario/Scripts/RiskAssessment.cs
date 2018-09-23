using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using TMPro;

public class RiskAssessment
{
    /*private LinesUtils linesUtils;
    private Transform rayCastPos;
    private GameObject gameObject;
    private Rigidbody rigidbody;
    private VehicleController vehicleController;
    private LayerMask mask;
    private Vector3 infoTagStartScale;
    private DriverCamera driverCam;
    private AnimationCurve infoTagResize;
    private VisualisationVars visualisationVars;

    public RiskAssessment(Vector3 infoTagStartScale, DriverCamera driverCam, AnimationCurve infoTagResize, Transform rayCastPos, GameObject gameObject, LayerMask mask, LinesUtils linesUtils)
    {
        this.infoTagStartScale = infoTagStartScale;
        this.driverCam = driverCam;
        this.infoTagResize = infoTagResize;
        this.rayCastPos = rayCastPos;
        this.gameObject = gameObject;
        this.mask = mask;
        this.linesUtils = linesUtils;

        this.rigidbody = gameObject.transform.parent.GetComponent<Rigidbody>();
        this.vehicleController = gameObject.transform.parent.GetComponent<VehicleController>();
    }

    public void BoundingCubeLerperSF(CubesAndTags cubesAndTags, Bounds bounds, Sprite sprite, Vector3 trasl, int i)
    {
        Renderer cubeRend = cubesAndTags.boundingCube[i].GetComponent<Renderer>();
        Canvas canvas = cubesAndTags.infoTag[i].GetComponent<Canvas>();
        Vector3 targetPoint = new Vector3(cubesAndTags.other.transform.position.x, rayCastPos.transform.position.y, cubesAndTags.other.transform.position.z);
        Vector3 dirToTarget = (targetPoint - rayCastPos.transform.position);
        Animator animTag = cubesAndTags.infoTag[i].GetComponent<Animator>();
        AudioSource audio = cubesAndTags.boundingCube[i].GetComponent<AudioSource>();

        float dstToTarget = Vector3.Distance(rayCastPos.position, targetPoint);
        float viewAngle = 5f;

        if (dstToTarget <= 8f)
            viewAngle = 25f;
        
        if (Vector3.Angle(rayCastPos.TransformDirection(Vector3.forward), dirToTarget) < viewAngle / 2 )
        {
            Debug.DrawLine(rayCastPos.position, targetPoint, Color.red);
            
            if (Physics.Raycast(rayCastPos.position, dirToTarget, dstToTarget, mask))
            {
                float distToWarn = RiskAssessmentFormulaD(rigidbody.velocity.magnitude, 0, 0, dstToTarget, ResourceHandler.instance.visualisationVars.systemAccStaticSF); //velocity component of obstacles is null since they are orthogonal to the playerCar
                if (dstToTarget <= Mathf.Abs(distToWarn) && Mathf.Abs(vehicleController.steerInput) <= 0.001f)
                {
                    float distToWarnEncoded = Mathf.Pow(distToWarn, 2.5f);
                    float dstToTargetEncoded = Mathf.Pow(dstToTarget, 2.5f);
                    bool blink = false;
                    Color32 topColor = linesUtils.ChangeMatByDistance(dstToTargetEncoded / Mathf.Abs(distToWarnEncoded), ref blink, ref cubesAndTags);
                    topColor.a = 0;
                    Color32 bottomColor = linesUtils.ChangeMatByDistance(dstToTargetEncoded / Mathf.Abs(distToWarnEncoded), ref blink, ref cubesAndTags);
                    bottomColor.a = 0x51;
                    cubeRend.material.SetColor("_Color1", topColor);
                    cubeRend.material.SetColor("_Color2", bottomColor);
                    cubeRend.enabled = true;
                    canvas.enabled = true;

                    cubesAndTags.alreadyEvaluated = true;

                    animTag.SetFloat("Multiplier", 3.0f);
                    animTag.SetBool("BlinkLoop", blink);

                    PlayAudio(audio, dstToTargetEncoded / Mathf.Abs(distToWarnEncoded), cubesAndTags);

                    UpdateInfoTag(cubesAndTags, bounds, "0", sprite, dstToTarget, trasl, i);
                }
                else
                {
                    cubeRend.enabled = false;
                    canvas.enabled = false;

                    animTag.SetBool("BlinkLoop", false);

                    audio.Stop();
                    
                }
            }
            else
            {
                cubeRend.enabled = false;
                canvas.enabled = false;

                animTag.SetBool("BlinkLoop", false);

                audio.Stop();
                
            }
        }
        else
        {
            cubeRend.enabled = false;
            canvas.enabled = false;

            animTag.SetBool("BlinkLoop", false);

            audio.Stop();

        }

    } //this is for static objects. In order to show a smooth vanishing when exiting the gradient state I can do a fading animation and control it by a bool that is set in the gradient if and reset after the animation has been played

    public void BoundingCubeLerperSF(CubesAndTags cubesAndTags, Bounds bounds, float obstacleSpeed, float acceleration, Sprite sprite, Vector3 trasl, int i)
    {
        TrafAIMotor trafAIMotor = gameObject.transform.parent.GetComponent<TrafAIMotor>();
        Vector3 targetPoint = new Vector3(cubesAndTags.other.transform.position.x, rayCastPos.transform.position.y, cubesAndTags.other.transform.position.z);
        Vector3 dirToTarget = (targetPoint - rayCastPos.transform.position);
        float dstToTarget = Vector3.Distance(rayCastPos.position, targetPoint);
        Renderer cubeRend = cubesAndTags.boundingCube[i].GetComponent<Renderer>();
        Canvas canvas = cubesAndTags.infoTag[i].GetComponent<Canvas>();
        Animator anim = cubesAndTags.infoTag[i].GetComponent<Animator>();
        AudioSource audio = cubesAndTags.boundingCube[i].GetComponent<AudioSource>();

        cubeRend.enabled = true;
        canvas.enabled = true;

        UpdateInfoTag(cubesAndTags, bounds, Mathf.RoundToInt(obstacleSpeed * 3.6f).ToString(), sprite, dstToTarget, trasl, i);

        float viewAngle = 5f;

        if (dstToTarget <= 8f)
            viewAngle = 25f;

        if (trafAIMotor.hasNextEntry) //If I am waiting at the intersection
        {
            float dist = Vector3.Distance(rayCastPos.position, trafAIMotor.currentEntry.waypoints[trafAIMotor.currentEntry.waypoints.Count - 1]);
            //Debug.Log(dist);

            if (dist <= 5.5f) //and I am located near the line that defines the intersection. This second condition is necessary since hasNextEntry is set earlier, not in correspondence with the line of the crossing.
            {
                cubeRend.material.SetColor("_Color1", new Color32(0x00, 0x80, 0xFF, 0x00));
                cubeRend.material.SetColor("_Color2", new Color32(0x00, 0x80, 0xFF, 0x51));

                anim.SetBool("BlinkLoop", false);

                audio.Stop();

                cubesAndTags.dangerState = CubesAndTags.DangerState.NONE;

                return;
            }
        }

        if (Vector3.Angle(rayCastPos.TransformDirection(Vector3.forward), dirToTarget) < viewAngle / 2) //the car is in front of me
        {
            if (Physics.Raycast(rayCastPos.position, dirToTarget, dstToTarget, mask))
            {
                float distToWarn = RiskAssessmentFormulaD(rigidbody.velocity.magnitude, obstacleSpeed, 0, dstToTarget, ResourceHandler.instance.visualisationVars.systemAccSF); //velocity component of obstacles is null since they are orthogonal to the playerCar
                if (dstToTarget <= Mathf.Abs(distToWarn))
                {
                    float distToWarnEncoded = Mathf.Pow(distToWarn, 2.5f);
                    float dstToTargetEncoded = Mathf.Pow(dstToTarget, 2.5f);
                    bool blink = false;
                    Color32 topColor = linesUtils.ChangeMatByDistance(dstToTargetEncoded / Mathf.Abs(distToWarnEncoded), ref blink, ref cubesAndTags);
                    topColor.a = 0;
                    Color32 bottomColor = linesUtils.ChangeMatByDistance(dstToTargetEncoded / Mathf.Abs(distToWarnEncoded), ref blink, ref cubesAndTags);
                    bottomColor.a = 0x51;
                    cubeRend.material.SetColor("_Color1", topColor);
                    cubeRend.material.SetColor("_Color2", bottomColor);
                    
                    anim.SetFloat("Multiplier", 3.0f);
                    anim.SetBool("BlinkLoop", blink);

                    PlayAudio(audio, dstToTargetEncoded / Mathf.Abs(distToWarnEncoded), cubesAndTags);

                    SetDashBoardColor(dstToTargetEncoded / Mathf.Abs(distToWarnEncoded), cubesAndTags);
                }
                else
                {
                    cubeRend.material.SetColor("_Color1", new Color32(0x00, 0x80, 0xFF, 0x00));
                    cubeRend.material.SetColor("_Color2", new Color32(0x00, 0x80, 0xFF, 0x51));

                    anim.SetBool("BlinkLoop", false);

                    audio.Stop();

                    cubesAndTags.dangerState = CubesAndTags.DangerState.NONE;
                }
            }
            else
            {
                cubeRend.material.SetColor("_Color1", new Color32(0x00, 0x80, 0xFF, 0x00));
                cubeRend.material.SetColor("_Color2", new Color32(0x00, 0x80, 0xFF, 0x51));

                anim.SetBool("BlinkLoop", false);

                audio.Stop();

                cubesAndTags.dangerState = CubesAndTags.DangerState.NONE;
            }
        }
        else
        {
            cubeRend.material.SetColor("_Color1", new Color32(0x00, 0x80, 0xFF, 0x00));
            cubeRend.material.SetColor("_Color2", new Color32(0x00, 0x80, 0xFF, 0x51));

            anim.SetBool("BlinkLoop", false);

            audio.Stop();

            cubesAndTags.dangerState = CubesAndTags.DangerState.NONE;
        }
    } //this is for dynamic objects

    public void BoundingCubeLerperObstacleDefSF(CubesAndTags cubesAndTags, Bounds bounds, float obstacleSpeed, float acceleration, Sprite sprite, Vector3 trasl, int i)
    {
        Vector3 targetPoint = new Vector3(cubesAndTags.other.transform.position.x, rayCastPos.transform.position.y, cubesAndTags.other.transform.position.z);
        float dstToTarget = Vector3.Distance(rayCastPos.position, targetPoint);
        Renderer cubeRend = cubesAndTags.boundingCube[i].GetComponent<Renderer>();
        Canvas canvas = cubesAndTags.infoTag[i].GetComponent<Canvas>();
        cubeRend.enabled = true;
        canvas.enabled = true;

        UpdateInfoTag(cubesAndTags, bounds, Mathf.RoundToInt(obstacleSpeed * 3.6f).ToString(), sprite, dstToTarget, trasl, i);

        Animator anim = cubesAndTags.infoTag[i].GetComponent<Animator>();
        AudioSource audio = cubesAndTags.boundingCube[i].GetComponent<AudioSource>();

        Vector3 targetDst = targetPoint + cubesAndTags.other.transform.TransformDirection(0, 0, Vector3.forward.z * 50f);
        Debug.DrawLine(targetPoint, targetDst, Color.magenta);

        Vector3 myDst = rayCastPos.position + rayCastPos.transform.TransformDirection(0, 0, Vector3.forward.z * 150f);

        Vector2 intersection = Vector2.zero;
        if (LineSegmentsIntersection(new Vector2(targetPoint.x, targetPoint.z), new Vector2(targetDst.x, targetDst.z), new Vector2(rayCastPos.position.x, rayCastPos.position.z), new Vector2(myDst.x, myDst.z), out intersection) && (obstacleSpeed >= 0.1f && rigidbody.velocity.magnitude >= 0.1f))
        {
            Vector3 intersection3D = new Vector3(intersection.x, rayCastPos.position.y, intersection.y);
            Debug.DrawLine(rayCastPos.position, intersection3D, Color.black);
            float distToWarn = rigidbody.velocity.magnitude * ResourceHandler.instance.visualisationVars.freeRunningTime + 0.5f * (Mathf.Pow(rigidbody.velocity.magnitude, 2) / ResourceHandler.instance.visualisationVars.systemAccSF); //DARIO

            if (dstToTarget <= distToWarn)
            {
                bool blink = false;
                Color32 topColor = linesUtils.ChangeMatByDistance(dstToTarget / Mathf.Abs(distToWarn), ref blink, ref cubesAndTags);
                topColor.a = 0;
                Color32 bottomColor = linesUtils.ChangeMatByDistance(dstToTarget / Mathf.Abs(distToWarn), ref blink, ref cubesAndTags);
                bottomColor.a = 0x51;
                cubeRend.material.SetColor("_Color1", topColor);
                cubeRend.material.SetColor("_Color2", bottomColor);

                anim.SetFloat("Multiplier", 3.0f);
                anim.SetBool("BlinkLoop", blink);

                Debug.Log("obstacle is: " + cubesAndTags.other.name + dstToTarget / distToWarn);

                PlayAudio(audio, dstToTarget / distToWarn, cubesAndTags);

                SetDashBoardColor(dstToTarget / distToWarn, cubesAndTags);

                cubesAndTags.prevState = dstToTarget / distToWarn;

                cubesAndTags.gradient = CubesAndTags.Gradient.ON;
            }
            else
            {
                bool blink = false;
                float value = 0f;
                if (cubesAndTags.gradient == CubesAndTags.Gradient.ON)
                {
                    value = Mathf.Pow(cubesAndTags.prevState, 0.7f);
                    cubesAndTags.prevState = value;
                }
                else
                    value = 1;
                
                Debug.Log("obstacle is: " + cubesAndTags.other.name + value);
                Color32 topColor = linesUtils.ChangeMatByDistance(value, ref blink, ref cubesAndTags);
                Color32 bottomColor = linesUtils.ChangeMatByDistance(value, ref blink, ref cubesAndTags);
                
                topColor.a = 0;
                bottomColor.a = 0x51;
                cubeRend.material.SetColor("_Color1", topColor);
                cubeRend.material.SetColor("_Color2", bottomColor);

                anim.SetBool("BlinkLoop", false);

                audio.Stop();

                cubesAndTags.dangerState = CubesAndTags.DangerState.NONE;

                if (value == 1)
                    cubesAndTags.gradient = CubesAndTags.Gradient.OFF;
            }
        }
        else
        {
            bool blink = false;
            float value = 0f;
            if (cubesAndTags.gradient == CubesAndTags.Gradient.ON)
            {
                value = Mathf.Pow(cubesAndTags.prevState, 0.7f);
                cubesAndTags.prevState = value;
            }
            else
                value = 1;

            Debug.Log("obstacle is: " + cubesAndTags.other.name + value);
            Color32 topColor = linesUtils.ChangeMatByDistance(value, ref blink, ref cubesAndTags);
            Color32 bottomColor = linesUtils.ChangeMatByDistance(value, ref blink, ref cubesAndTags);
            
            topColor.a = 0;
            bottomColor.a = 0x51;
            cubeRend.material.SetColor("_Color1", topColor);
            cubeRend.material.SetColor("_Color2", bottomColor);

            anim.SetBool("BlinkLoop", false);

            audio.Stop();

            cubesAndTags.dangerState = CubesAndTags.DangerState.NONE;

            if (value == 1)
                cubesAndTags.gradient = CubesAndTags.Gradient.OFF;
        }
    }//this is for dynamic objects

    public void BoundingCubeLerperScooterSF(CubesAndTags cubesAndTags, Bounds bounds, float obstacleSpeed, Sprite sprite, Vector3 trasl, int i)
    {
        Vector3 targetPoint = new Vector3(cubesAndTags.other.transform.position.x, rayCastPos.transform.position.y, cubesAndTags.other.transform.position.z);
        float dstToTarget = Vector3.Distance(rayCastPos.position, targetPoint);
        Vector3 dirToTarget = (targetPoint - rayCastPos.transform.position);
        Renderer cubeRend = cubesAndTags.boundingCube[i].GetComponent<Renderer>();
        Canvas canvas = cubesAndTags.infoTag[i].GetComponent<Canvas>();
        cubeRend.enabled = true;
        canvas.enabled = true;

        UpdateInfoTag(cubesAndTags, bounds, Mathf.RoundToInt(obstacleSpeed * 3.6f).ToString(), sprite, dstToTarget, trasl, i);

        Animator anim = cubesAndTags.infoTag[i].GetComponent<Animator>();
        AudioSource audio = cubesAndTags.boundingCube[i].GetComponent<AudioSource>();
        TrafAIMotor trafAIMotor = cubesAndTags.other.transform.root.GetComponent<TrafAIMotor>();

        float viewAngle = 5f;

        if (dstToTarget <= 8f)
            viewAngle = 25f;

        if (trafAIMotor.hasNextEntry)
        {
            if (Vector3.Angle(rayCastPos.TransformDirection(Vector3.forward), dirToTarget) < viewAngle / 2) //the car is in front of me
            {
                if (Physics.Raycast(rayCastPos.position, dirToTarget, dstToTarget, mask))
                {
                    float distToWarn = RiskAssessmentFormulaD(rigidbody.velocity.magnitude, obstacleSpeed, 0, dstToTarget, ResourceHandler.instance.visualisationVars.systemAccSF); //velocity component of obstacles is null since they are orthogonal to the playerCar
                    if (dstToTarget <= Mathf.Abs(distToWarn))
                    {
                        float distToWarnEncoded = Mathf.Pow(distToWarn, 2.5f);
                        float dstToTargetEncoded = Mathf.Pow(dstToTarget, 2.5f);
                        bool blink = false;
                        Color32 topColor = linesUtils.ChangeMatByDistance(dstToTargetEncoded / Mathf.Abs(distToWarnEncoded), ref blink, ref cubesAndTags);
                        topColor.a = 0;
                        Color32 bottomColor = linesUtils.ChangeMatByDistance(dstToTargetEncoded / Mathf.Abs(distToWarnEncoded), ref blink, ref cubesAndTags);
                        bottomColor.a = 0x51;
                        cubeRend.material.SetColor("_Color1", topColor);
                        cubeRend.material.SetColor("_Color2", bottomColor);

                        anim.SetFloat("Multiplier", 3.0f);
                        anim.SetBool("BlinkLoop", blink);

                        PlayAudio(audio, dstToTargetEncoded / Mathf.Abs(distToWarnEncoded), cubesAndTags);

                        SetDashBoardColor(dstToTargetEncoded / Mathf.Abs(distToWarnEncoded), cubesAndTags);
                    }
                    else
                    {
                        cubeRend.material.SetColor("_Color1", new Color32(0x00, 0x80, 0xFF, 0x00));
                        cubeRend.material.SetColor("_Color2", new Color32(0x00, 0x80, 0xFF, 0x51));

                        anim.SetBool("BlinkLoop", false);

                        audio.Stop();

                        cubesAndTags.dangerState = CubesAndTags.DangerState.NONE;
                    }
                }
                else
                {
                    cubeRend.material.SetColor("_Color1", new Color32(0x00, 0x80, 0xFF, 0x00));
                    cubeRend.material.SetColor("_Color2", new Color32(0x00, 0x80, 0xFF, 0x51));

                    anim.SetBool("BlinkLoop", false);

                    audio.Stop();

                    cubesAndTags.dangerState = CubesAndTags.DangerState.NONE;
                }
            }
            return;
        }

        if (dstToTarget <= ResourceHandler.instance.visualisationVars.obstacleDistToWarn && vehicleController.CurrentSpeed >= 0.1f)
        {
            bool blink = false;
            Color32 topColor = linesUtils.ChangeMatByDistance(dstToTarget / Mathf.Abs(ResourceHandler.instance.visualisationVars.obstacleDistToWarn), ref blink, ref cubesAndTags);
            topColor.a = 0;
            Color32 bottomColor = linesUtils.ChangeMatByDistance(dstToTarget / Mathf.Abs(ResourceHandler.instance.visualisationVars.obstacleDistToWarn), ref blink, ref cubesAndTags);
            bottomColor.a = 0x51;
            cubeRend.material.SetColor("_Color1", topColor);
            cubeRend.material.SetColor("_Color2", bottomColor);

            anim.SetFloat("Multiplier", 3.0f);
            anim.SetBool("BlinkLoop", blink);

            PlayAudio(audio, dstToTarget / ResourceHandler.instance.visualisationVars.obstacleDistToWarn, cubesAndTags);

            SetDashBoardColor(dstToTarget / ResourceHandler.instance.visualisationVars.obstacleDistToWarn, cubesAndTags);
        }
        else
        {
            cubeRend.material.SetColor("_Color1", new Color32(0x00, 0x80, 0xFF, 0x00));
            cubeRend.material.SetColor("_Color2", new Color32(0x00, 0x80, 0xFF, 0x51));

            anim.SetBool("BlinkLoop", false);

            audio.Stop();

            cubesAndTags.dangerState = CubesAndTags.DangerState.NONE;
        }  
    } //this is for dynamic objects (scooter)



    //public void BoundingCubeLerperObstacle2(CubesAndTags cubesAndTags, Bounds bounds, float obstacleSpeed, float acceleration, Sprite sprite, Vector3 trasl, int i)
    //{
    //    Vector3 targetPoint = new Vector3(cubesAndTags.other.transform.position.x, rayCastPos.position.y, cubesAndTags.other.transform.position.z);
    //    float dstToTarget = Vector3.Distance(rayCastPos.position, targetPoint);
    //    Renderer cubeRend = cubesAndTags.boundingCube[i].GetComponent<Renderer>();
    //    Canvas canvas = cubesAndTags.infoTag[i].GetComponent<Canvas>();
    //    cubeRend.enabled = true;
    //    canvas.enabled = true;

    //    Animator anim = cubesAndTags.infoTag[i].GetComponent<Animator>();

    //    AudioSource audio = cubesAndTags.boundingCube[i].GetComponent<AudioSource>();

    //    float dotProduct = Vector3.Dot(gameObject.transform.TransformDirection(Vector3.forward), cubesAndTags.other.transform.TransformDirection(Vector3.forward));

    //    if (dotProduct < Mathf.Cos(20 * Mathf.Deg2Rad) && dotProduct > Mathf.Cos(160 * Mathf.Deg2Rad)) //the obstacle is at the intersection
    //    {
    //        Vector3 targetDst = targetPoint + cubesAndTags.other.transform.TransformDirection(0, 0, Vector3.forward.z * 50f);
    //        Debug.DrawLine(targetPoint, targetDst, Color.magenta);

    //        Vector3 myDst = rayCastPos.position + rayCastPos.transform.TransformDirection(0, 0, Vector3.forward.z * 150f);
    //        Debug.DrawLine(rayCastPos.position, myDst, Color.magenta);

    //        //Debug.DrawLine(rayCastPos.position, cubesAndTags.boundingCube[0].transform.position , Color.yellow);
    //        //Debug.DrawLine(rayCastPos.position, cubesAndTags.boundingCube[0].transform.position + new Vector3(bounds.extents.x, 0, bounds.extents.z), Color.yellow);
    //        //Debug.DrawLine(rayCastPos.position, cubesAndTags.boundingCube[0].transform.position - new Vector3(bounds.extents.x, 0, bounds.extents.z), Color.yellow);

    //        Vector2 intersection = Vector2.zero;
    //        LineSegmentsIntersection(new Vector2(targetPoint.x, targetPoint.z), new Vector2(targetDst.x, targetDst.z), new Vector2(rayCastPos.position.x, rayCastPos.position.z), new Vector2(myDst.x, myDst.z), out intersection);
    //        Vector3 intersection3D = new Vector3(intersection.x, rayCastPos.position.y, intersection.y);
    //        Debug.DrawLine(rayCastPos.position, intersection3D, Color.green);
    //        float myDstToIntersection = Vector3.Distance(rayCastPos.position, intersection3D);
    //        float targetDstToIntersection = Vector3.Distance(targetPoint, intersection3D);

    //        float myTTC = myDstToIntersection / rigidbody.velocity.magnitude; //this is the playerCar time-to-collision
    //        float targetTTC = targetDstToIntersection / obstacleSpeed; //this is the obstacle time-to-collision

    //        if (Math.Abs(myTTC - targetTTC) < 2)
    //        {
    //            float softDecelleration = 2; //I consider a soft decelleration
    //            float myTTA = rigidbody.velocity.magnitude / softDecelleration; //this is the PlayerCar time-to-avoidance i.e. the time necessary to avoid a collision

    //            if (myTTA < targetTTC)
    //            {
    //                cubeRend.material.SetColor("_Color1", new Color32(0x00, 0x80, 0xFF, 0x00));
    //                cubeRend.material.SetColor("_Color2", new Color32(0x00, 0x80, 0xFF, 0x51));

    //                anim.SetBool("BlinkLoop", false);

    //                audio.Stop();
    //            }
    //            else
    //            {
    //                bool blink = false;
    //                Color32 topColor = linesUtils.ChangeMatByDistance(targetTTC, Mathf.Abs(myTTA), ref blink, ref cubesAndTags);
    //                topColor.a = 0;
    //                Color32 bottomColor = linesUtils.ChangeMatByDistance(targetTTC, Mathf.Abs(myTTA), ref blink, ref cubesAndTags);
    //                bottomColor.a = 0x51;
    //                cubeRend.material.SetColor("_Color1", topColor);
    //                cubeRend.material.SetColor("_Color2", bottomColor);

    //                anim.SetFloat("Multiplier", 3.0f);
    //                anim.SetBool("BlinkLoop", blink);

    //                PlayAudio(audio, targetTTC / Mathf.Abs(myTTA), cubesAndTags);

    //            }
    //        }
    //        else
    //        {
    //            cubeRend.material.SetColor("_Color1", new Color32(0x00, 0x80, 0xFF, 0x00));
    //            cubeRend.material.SetColor("_Color2", new Color32(0x00, 0x80, 0xFF, 0x51));

    //            anim.SetBool("BlinkLoop", false);

    //            audio.Stop();
    //        }
    //    }
    //    UpdateInfoTag(cubesAndTags, bounds, Mathf.RoundToInt(obstacleSpeed * 3.6f).ToString(), sprite, dstToTarget, trasl, i);
    //} //this is for dynamic objects

    //public void BoundingCubeLerperObstacle4(CubesAndTags cubesAndTags, Bounds bounds, float obstacleSpeed, float acceleration, Sprite sprite, Vector3 trasl, int i)
    //{
    //    Vector3 targetPoint = new Vector3(cubesAndTags.other.transform.position.x, cubesAndTags.other.transform.position.y, cubesAndTags.other.transform.position.z);
    //    float dstToTarget = Vector3.Distance(rayCastPos.position, targetPoint);
    //    Renderer cubeRend = cubesAndTags.boundingCube[i].GetComponent<Renderer>();
    //    Canvas canvas = cubesAndTags.infoTag[i].GetComponent<Canvas>();
    //    cubeRend.enabled = true;
    //    canvas.enabled = true;

    //    Animator anim = cubesAndTags.infoTag[i].GetComponent<Animator>();

    //    AudioSource audio = cubesAndTags.boundingCube[i].GetComponent<AudioSource>();

    //    Vector3 targetDst = targetPoint + cubesAndTags.other.transform.TransformDirection(0, 0, Vector3.forward.z * 50f);
    //    Debug.DrawLine(targetPoint, targetDst, Color.magenta);

    //    Vector3 myDst = rayCastPos.position + rayCastPos.transform.TransformDirection(0, 0, Vector3.forward.z * 150f);
    //    Debug.DrawLine(rayCastPos.position, myDst, Color.magenta);

    //    Vector2 intersection = Vector2.zero;
    //    LineSegmentsIntersection(new Vector2(targetPoint.x, targetPoint.z), new Vector2(targetDst.x, targetDst.z), new Vector2(rayCastPos.position.x, rayCastPos.position.z), new Vector2(myDst.x, myDst.z), out intersection);
    //    Vector3 intersection3D = new Vector3(intersection.x, rayCastPos.position.y, intersection.y);
    //    float myDstToIntersection = Vector3.Distance(rayCastPos.position, intersection3D);
    //    float targetDstToIntersection = Vector3.Distance(targetPoint, intersection3D);

    //    float myTTC = myDstToIntersection / rigidbody.velocity.magnitude; //this is the playerCar time-to-collision
    //    float targetTTC = targetDstToIntersection / obstacleSpeed; //this is the obstacle time-to-collision

    //    if (Math.Abs(myTTC - targetTTC) < 2)
    //    {
    //        float distToWarn = rigidbody.velocity.magnitude * ResourceHandler.instance.visualisationVars.freeRunningTime + Mathf.Pow(rigidbody.velocity.magnitude, 2) / (2 * ResourceHandler.instance.visualisationVars.systemAccSF);
    //        if (dstToTarget <= Mathf.Abs(distToWarn))
    //        {
    //            float distToWarnEncoded = Mathf.Pow(distToWarn, 2.5f);
    //            float dstToTargetEncoded = Mathf.Pow(dstToTarget, 2.5f);
    //            bool blink = false;
    //            Color32 topColor = linesUtils.ChangeMatByDistance(dstToTargetEncoded, Mathf.Abs(distToWarnEncoded), ref blink, ref cubesAndTags);
    //            topColor.a = 0;
    //            Color32 bottomColor = linesUtils.ChangeMatByDistance(dstToTargetEncoded, Mathf.Abs(distToWarnEncoded), ref blink, ref cubesAndTags);
    //            bottomColor.a = 0x51;
    //            cubeRend.material.SetColor("_Color1", topColor);
    //            cubeRend.material.SetColor("_Color2", bottomColor);
    //            cubeRend.enabled = true;
    //            canvas.enabled = true;

    //            anim.SetFloat("Multiplier", 3.0f);
    //            anim.SetBool("BlinkLoop", blink);

    //            PlayAudio(audio, dstToTarget / distToWarn, cubesAndTags);
    //        }
    //        else
    //        {
    //            cubeRend.material.SetColor("_Color1", new Color32(0x00, 0x80, 0xFF, 0x00));
    //            cubeRend.material.SetColor("_Color2", new Color32(0x00, 0x80, 0xFF, 0x51));

    //            anim.SetBool("BlinkLoop", false);

    //            audio.Stop();
    //        }
    //    }
    //    else
    //    {
    //        cubeRend.material.SetColor("_Color1", new Color32(0x00, 0x80, 0xFF, 0x00));
    //        cubeRend.material.SetColor("_Color2", new Color32(0x00, 0x80, 0xFF, 0x51));

    //        anim.SetBool("BlinkLoop", false);

    //        audio.Stop();
    //    }
    //    UpdateInfoTag(cubesAndTags, bounds, Mathf.RoundToInt(obstacleSpeed * 3.6f).ToString(), sprite, dstToTarget, trasl, i);
    //} //this is for dynamic objects

    



    public void BoundingCubeLerperPCH(CubesAndTags cubesAndTags, Bounds bounds, Sprite sprite, Vector3 trasl, int i)
    {
        Renderer cubeRend = cubesAndTags.boundingCube[i].GetComponent<Renderer>();
        Canvas canvas = cubesAndTags.infoTag[i].GetComponent<Canvas>();
        Vector3 targetPoint = cubesAndTags.other.transform.position;
        Vector3 dirToTarget = (targetPoint - rayCastPos.transform.position);
        Animator animTag = cubesAndTags.infoTag[i].GetComponent<Animator>();
        Animator animCube = cubesAndTags.boundingCube[i].GetComponent<Animator>();
        AudioSource audio = cubesAndTags.boundingCube[i].GetComponent<AudioSource>();

        float dstToTarget = Vector3.Distance(rayCastPos.position, targetPoint);
        float viewAngle = 15f;

        if (dstToTarget <= 8f)
            viewAngle = 25f;

        if (Vector3.Angle(rayCastPos.TransformDirection(Vector3.forward), dirToTarget) < viewAngle / 2)
        {
            if (Physics.Raycast(rayCastPos.position, dirToTarget, dstToTarget, mask))
            {
                float distToWarn = RiskAssessmentFormulaD(rigidbody.velocity.magnitude, 0, 0, dstToTarget, ResourceHandler.instance.visualisationVars.systemAccStaticPCH); //velocity component of obstacles is null since they are orthogonal to the playerCar
                if (dstToTarget <= Mathf.Abs(distToWarn) && Mathf.Abs(vehicleController.steerInput) == 0f)
                {
                    Debug.DrawLine(rayCastPos.position, targetPoint, Color.red);
                    float distToWarnEncoded = Mathf.Pow(distToWarn, 2.5f);
                    float dstToTargetEncoded = Mathf.Pow(dstToTarget, 2.5f);
                    bool blink = false;
                    Color32 topColor = linesUtils.ChangeMatByDistance(dstToTargetEncoded / Mathf.Abs(distToWarnEncoded), ref blink, ref cubesAndTags);
                    topColor.a = 0;
                    Color32 bottomColor = linesUtils.ChangeMatByDistance(dstToTargetEncoded / Mathf.Abs(distToWarnEncoded), ref blink, ref cubesAndTags);
                    bottomColor.a = 0x51;
                    cubeRend.material.SetColor("_Color1", topColor);
                    cubeRend.material.SetColor("_Color2", bottomColor);
                    cubeRend.enabled = true;
                    canvas.enabled = true;

                    cubesAndTags.alreadyEvaluated = true;

                    animTag.SetFloat("Multiplier", 3.0f);
                    animTag.SetBool("BlinkLoop", blink);

                    PlayAudio(audio, dstToTargetEncoded / Mathf.Abs(distToWarnEncoded), cubesAndTags);

                    UpdateInfoTag(cubesAndTags, bounds, "0", sprite, dstToTarget, trasl, i);

                    //cubesAndTags.prevState = dstToTargetEncoded / Mathf.Abs(distToWarnEncoded);
                }
                else
                {
                    cubeRend.enabled = false;
                    canvas.enabled = false;

                    animTag.SetBool("BlinkLoop", false);

                    audio.Stop();
                }
            }
            else
            {
                cubeRend.enabled = false;
                canvas.enabled = false;

                animTag.SetBool("BlinkLoop", false);

                audio.Stop();
            }
        }
        else
        {
            cubeRend.enabled = false;
            canvas.enabled = false;

            animTag.SetBool("BlinkLoop", false);

            audio.Stop();
        }
    } //this is for static objects. OK!

    public void BoundingCubeLerperPCH(CubesAndTags cubesAndTags, Bounds bounds, float obstacleSpeed, float acceleration, Sprite sprite, Vector3 trasl, int i)
    {
        Vector3 targetPoint = cubesAndTags.other.transform.position;
        Vector3 dirToTarget = (targetPoint - rayCastPos.transform.position);
        float dstToTarget = Vector3.Distance(rayCastPos.position, targetPoint);
        Renderer cubeRend = cubesAndTags.boundingCube[i].GetComponent<Renderer>();
        Canvas canvas = cubesAndTags.infoTag[i].GetComponent<Canvas>();
        cubeRend.enabled = true;
        canvas.enabled = true;

        UpdateInfoTag(cubesAndTags, bounds, Mathf.RoundToInt(obstacleSpeed * 3.6f).ToString(), sprite, dstToTarget, trasl, i);

        Animator anim = cubesAndTags.infoTag[i].GetComponent<Animator>();
        AudioSource audio = cubesAndTags.boundingCube[i].GetComponent<AudioSource>();

        float viewAngle = 45f;

        if (dstToTarget <= 20f)
            viewAngle = 25f;

        if (Vector3.Angle(rayCastPos.TransformDirection(Vector3.forward), dirToTarget) < viewAngle / 2) //the car is in front of me
        {
            if (Physics.Raycast(rayCastPos.position, dirToTarget, dstToTarget, mask))
            {
                float distToWarn = RiskAssessmentFormulaD(rigidbody.velocity.magnitude, obstacleSpeed, 0, dstToTarget, ResourceHandler.instance.visualisationVars.systemAccPCH); //velocity component of obstacles is null since they are orthogonal to the playerCar
                if (dstToTarget <= Mathf.Abs(distToWarn))
                {
                    float distToWarnEncoded = Mathf.Pow(distToWarn, 2.5f);
                    float dstToTargetEncoded = Mathf.Pow(dstToTarget, 2.5f);
                    bool blink = false;
                    Color32 topColor = linesUtils.ChangeMatByDistance(dstToTargetEncoded / Mathf.Abs(distToWarnEncoded), ref blink, ref cubesAndTags);
                    topColor.a = 0;
                    Color32 bottomColor = linesUtils.ChangeMatByDistance(dstToTargetEncoded / Mathf.Abs(distToWarnEncoded), ref blink, ref cubesAndTags);
                    bottomColor.a = 0x51;
                    cubeRend.material.SetColor("_Color1", topColor);
                    cubeRend.material.SetColor("_Color2", bottomColor);
                    cubeRend.enabled = true;
                    canvas.enabled = true;

                    anim.SetFloat("Multiplier", 3.0f);
                    anim.SetBool("BlinkLoop", blink);

                    PlayAudio(audio, dstToTargetEncoded / Mathf.Abs(distToWarnEncoded), cubesAndTags);

                    UpdateInfoTag(cubesAndTags, bounds, "0", sprite, dstToTarget, trasl, i);

                    //cubesAndTags.prevState = dstToTargetEncoded / Mathf.Abs(distToWarnEncoded);
                    SetDashBoardColor(dstToTargetEncoded / Mathf.Abs(distToWarnEncoded), cubesAndTags);
                }
                else
                {
                    cubeRend.material.SetColor("_Color1", new Color32(0x00, 0x80, 0xFF, 0x00));
                    cubeRend.material.SetColor("_Color2", new Color32(0x00, 0x80, 0xFF, 0x51));

                    anim.SetBool("BlinkLoop", false);

                    audio.Stop();

                    cubesAndTags.dangerState = CubesAndTags.DangerState.NONE;
                }
            }
            else
            {
                cubeRend.material.SetColor("_Color1", new Color32(0x00, 0x80, 0xFF, 0x00));
                cubeRend.material.SetColor("_Color2", new Color32(0x00, 0x80, 0xFF, 0x51));

                anim.SetBool("BlinkLoop", false);

                audio.Stop();

                cubesAndTags.dangerState = CubesAndTags.DangerState.NONE;
            }
        }
        else
        {
            cubeRend.material.SetColor("_Color1", new Color32(0x00, 0x80, 0xFF, 0x00));
            cubeRend.material.SetColor("_Color2", new Color32(0x00, 0x80, 0xFF, 0x51));

            anim.SetBool("BlinkLoop", false);

            audio.Stop();

            cubesAndTags.dangerState = CubesAndTags.DangerState.NONE;
        }
    } //this is for dynamic objects

    public void BoundingCubeLerperObstaclePCH(CubesAndTags cubesAndTags, Bounds bounds, float obstacleSpeed, float acceleration, Sprite sprite, Vector3 trasl, int i)
    {
        Vector3 targetPoint = cubesAndTags.other.transform.position;
        Vector3 heading = (targetPoint - rayCastPos.position).normalized; //direction from PlayerCar to Obstacle
        float dot = Vector3.Dot(rayCastPos.transform.forward, heading); //cosine of angle between heading and direction of PlayerCar
        float dstToTarget = Vector3.Distance(rayCastPos.position, targetPoint);

        Renderer cubeRend = cubesAndTags.boundingCube[i].GetComponent<Renderer>();
        Canvas canvas = cubesAndTags.infoTag[i].GetComponent<Canvas>();
        cubeRend.enabled = true;
        canvas.enabled = true;

        UpdateInfoTag(cubesAndTags, bounds, Mathf.RoundToInt(obstacleSpeed * 3.6f).ToString(), sprite, dstToTarget, trasl, i);

        Animator anim = cubesAndTags.infoTag[i].GetComponent<Animator>();
        AudioSource audio = cubesAndTags.boundingCube[i].GetComponent<AudioSource>();

        float distToCollision = dstToTarget * dot; //this is the distance from PlayerCar to the endPoint of the triangle cathetus
        Debug.DrawLine(rayCastPos.position, targetPoint, Color.blue);
        Vector3 endPoint = rayCastPos.position + (rayCastPos.transform.forward * distToCollision);
        Debug.DrawLine(rayCastPos.position, endPoint, Color.blue);

        Vector3 obstacleHeading = endPoint - targetPoint; //this is to understand if the obstacle has passed the endPoint
        if (Vector3.Dot(obstacleHeading, cubesAndTags.other.transform.forward) > 0) //endPoint is in front of me
        {
            if (dstToTarget <= ResourceHandler.instance.visualisationVars.obstacleDistToWarn)
            {
                bool blink = false;
                Color32 topColor = linesUtils.ChangeMatByDistance(dstToTarget / Mathf.Abs(ResourceHandler.instance.visualisationVars.obstacleDistToWarn), ref blink, ref cubesAndTags);
                topColor.a = 0;
                Color32 bottomColor = linesUtils.ChangeMatByDistance(dstToTarget / Mathf.Abs(ResourceHandler.instance.visualisationVars.obstacleDistToWarn), ref blink, ref cubesAndTags);
                bottomColor.a = 0x51;
                cubeRend.material.SetColor("_Color1", topColor);
                cubeRend.material.SetColor("_Color2", bottomColor);

                anim.SetFloat("Multiplier", 3.0f);
                anim.SetBool("BlinkLoop", blink);

                PlayAudio(audio, dstToTarget / ResourceHandler.instance.visualisationVars.obstacleDistToWarn, cubesAndTags);

                SetDashBoardColor(dstToTarget / ResourceHandler.instance.visualisationVars.obstacleDistToWarn, cubesAndTags);

                cubesAndTags.prevState = dstToTarget / ResourceHandler.instance.visualisationVars.obstacleDistToWarn;

                cubesAndTags.gradient = CubesAndTags.Gradient.ON;
            }
            else
            {
                bool blink = false;
                float value = 0f;
                if (cubesAndTags.gradient == CubesAndTags.Gradient.ON)
                {
                    value = Mathf.Pow(cubesAndTags.prevState, 0.7f);
                    cubesAndTags.prevState = value;
                }
                else
                    value = 1;

                Debug.Log("obstacle is: " + cubesAndTags.other.name + value);
                Color32 topColor = linesUtils.ChangeMatByDistance(value, ref blink, ref cubesAndTags);
                Color32 bottomColor = linesUtils.ChangeMatByDistance(value, ref blink, ref cubesAndTags);

                topColor.a = 0;
                bottomColor.a = 0x51;
                cubeRend.material.SetColor("_Color1", topColor);
                cubeRend.material.SetColor("_Color2", bottomColor);

                anim.SetBool("BlinkLoop", false);

                audio.Stop();

                cubesAndTags.dangerState = CubesAndTags.DangerState.NONE;

                if (value == 1)
                    cubesAndTags.gradient = CubesAndTags.Gradient.OFF;
            }
        }
        else
        {
            bool blink = false;
            float value = 0f;
            if (cubesAndTags.gradient == CubesAndTags.Gradient.ON)
            {
                value = Mathf.Pow(cubesAndTags.prevState, 0.7f);
                cubesAndTags.prevState = value;
            }
            else
                value = 1;

            Debug.Log("obstacle is: " + cubesAndTags.other.name + value);
            Color32 topColor = linesUtils.ChangeMatByDistance(value, ref blink, ref cubesAndTags);
            Color32 bottomColor = linesUtils.ChangeMatByDistance(value, ref blink, ref cubesAndTags);

            topColor.a = 0;
            bottomColor.a = 0x51;
            cubeRend.material.SetColor("_Color1", topColor);
            cubeRend.material.SetColor("_Color2", bottomColor);

            anim.SetBool("BlinkLoop", false);

            audio.Stop();

            cubesAndTags.dangerState = CubesAndTags.DangerState.NONE;

            if (value == 1)
                cubesAndTags.gradient = CubesAndTags.Gradient.OFF;
        } 
    } //this is for dynamic objects

    public void BoundingCubeLerperDangerousCarPCH(CubesAndTags cubesAndTags, Bounds bounds, float obstacleSpeed, float distToWarn, float viewAngle, Sprite sprite, Vector3 trasl, int i)
    {
        Vector3 targetPoint = cubesAndTags.other.transform.position;
        Vector3 dirToTarget = (targetPoint - rayCastPos.transform.position);
        float dstToTarget = Vector3.Distance(rayCastPos.position, targetPoint);

        Renderer cubeRend = cubesAndTags.boundingCube[i].GetComponent<Renderer>();
        Canvas canvas = cubesAndTags.infoTag[i].GetComponent<Canvas>();
        cubeRend.enabled = true;
        canvas.enabled = true;

        UpdateInfoTag(cubesAndTags, bounds, Mathf.RoundToInt(obstacleSpeed * 3.6f).ToString(), sprite, dstToTarget, trasl, i);

        Animator anim = cubesAndTags.infoTag[i].GetComponent<Animator>();
        AudioSource audio = cubesAndTags.boundingCube[i].GetComponent<AudioSource>();

        if (Vector3.Angle(rayCastPos.TransformDirection(Vector3.forward), dirToTarget) < ResourceHandler.instance.visualisationVars.viewAngle / 2)
        {
            if (dstToTarget <= distToWarn)
            {
                bool blink = false;
                Color32 topColor = linesUtils.ChangeMatByDistance(dstToTarget / distToWarn, ref blink, ref cubesAndTags);
                topColor.a = 0;
                Color32 bottomColor = linesUtils.ChangeMatByDistance(dstToTarget / distToWarn, ref blink, ref cubesAndTags);
                bottomColor.a = 0x51;
                cubeRend.material.SetColor("_Color1", topColor);
                cubeRend.material.SetColor("_Color2", bottomColor);

                anim.SetFloat("Multiplier", 3.0f);
                anim.SetBool("BlinkLoop", blink);

                PlayAudio(audio, dstToTarget / distToWarn, cubesAndTags);

                SetDashBoardColor(dstToTarget / distToWarn, cubesAndTags);

                cubesAndTags.prevState = dstToTarget / distToWarn;

                cubesAndTags.gradient = CubesAndTags.Gradient.ON;
            }
            else
            {
                bool blink = false;
                float value = 0f;
                if (cubesAndTags.gradient == CubesAndTags.Gradient.ON)
                {
                    value = Mathf.Pow(cubesAndTags.prevState, 0.7f);
                    cubesAndTags.prevState = value;
                }
                else
                    value = 1;

                Debug.Log("obstacle is: " + cubesAndTags.other.name + value);
                Color32 topColor = linesUtils.ChangeMatByDistance(value, ref blink, ref cubesAndTags);
                Color32 bottomColor = linesUtils.ChangeMatByDistance(value, ref blink, ref cubesAndTags);

                topColor.a = 0;
                bottomColor.a = 0x51;
                cubeRend.material.SetColor("_Color1", topColor);
                cubeRend.material.SetColor("_Color2", bottomColor);

                anim.SetBool("BlinkLoop", false);

                audio.Stop();

                cubesAndTags.dangerState = CubesAndTags.DangerState.NONE;

                if (value == 1)
                    cubesAndTags.gradient = CubesAndTags.Gradient.OFF;
            }
        } else
        {
            bool blink = false;
            float value = 0f;
            if (cubesAndTags.gradient == CubesAndTags.Gradient.ON)
            {
                value = Mathf.Pow(cubesAndTags.prevState, 0.7f);
                cubesAndTags.prevState = value;
            }
            else
                value = 1;

            Debug.Log("obstacle is: " + cubesAndTags.other.name + value);
            Color32 topColor = linesUtils.ChangeMatByDistance(value, ref blink, ref cubesAndTags);
            Color32 bottomColor = linesUtils.ChangeMatByDistance(value, ref blink, ref cubesAndTags);

            topColor.a = 0;
            bottomColor.a = 0x51;
            cubeRend.material.SetColor("_Color1", topColor);
            cubeRend.material.SetColor("_Color2", bottomColor);

            anim.SetBool("BlinkLoop", false);

            audio.Stop();

            cubesAndTags.dangerState = CubesAndTags.DangerState.NONE;

            if (value == 1)
                cubesAndTags.gradient = CubesAndTags.Gradient.OFF;
        }
    } //this is for dynamic objects






    //public void InfoTagWarn(CubesAndTags cubesAndTags, Bounds bounds, Sprite sprite, Vector3 trasl, int i)
    //{
    //    Renderer cubeRend = cubesAndTags.boundingCube[i].GetComponent<Renderer>();
    //    Canvas canvas = cubesAndTags.infoTag[i].GetComponent<Canvas>();
    //    Vector3 targetPoint = cubesAndTags.other.transform.position;
    //    Vector3 dirToTarget = (targetPoint - rayCastPos.transform.position);

    //    Animator anim = cubesAndTags.infoTag[i].GetComponent<Animator>();
    //    AudioSource audio = cubesAndTags.boundingCube[i].GetComponent<AudioSource>();

    //    Transform Panel = cubesAndTags.infoTag[i].transform.GetChild(0);
    //    Image warnSprite = Panel.transform.GetChild(2).GetComponent<Image>();

    //    if (Vector3.Angle(rayCastPos.TransformDirection(Vector3.forward), dirToTarget) < ResourceHandler.instance.visualisationVars.viewAngle / 2)
    //    {
    //        float dstToTarget = Vector3.Distance(rayCastPos.position, targetPoint);
    //        if (Physics.Raycast(rayCastPos.position, dirToTarget, dstToTarget, mask))
    //        {
    //            Debug.DrawLine(rayCastPos.position, targetPoint, Color.red);
    //            float distToWarn = RiskAssessmentFormulaD(rigidbody.velocity.magnitude, 0, 0, dstToTarget);

    //            if (dstToTarget <= Mathf.Abs(distToWarn))
    //            {
    //                float normalizedDist = dstToTarget / Mathf.Abs(distToWarn);
    //                bool blink = false;
    //                if (normalizedDist >= 0 && normalizedDist <= 0.5f)
    //                {
    //                    warnSprite.sprite = ResourceHandler.instance.sprites[29]; //sprite of redSign
    //                    blink = true;
    //                }
    //                else if (normalizedDist > 0.5f && normalizedDist <= 0.8f)
    //                {
    //                    warnSprite.sprite = ResourceHandler.instance.sprites[28]; //sprite of yellowSign
    //                    blink = true;

    //                }
    //                else if (normalizedDist > 0.8f && normalizedDist <= 1.0f)
    //                {
    //                    warnSprite.sprite = ResourceHandler.instance.sprites[27]; //sprite of greenDot
    //                }
    //                cubeRend.enabled = true;
    //                canvas.enabled = true;

    //                anim.SetFloat("Multiplier", 3.0f);
    //                anim.SetBool("BlinkLoop", blink);

    //                PlayAudio(audio, blink);

    //                UpdateInfoTag(cubesAndTags, bounds, "0", sprite, dstToTarget, trasl, i);
    //            }
    //            else
    //            {
    //                cubeRend.enabled = false;
    //                canvas.enabled = false;

    //                anim.SetBool("BlinkLoop", false);

    //                audio.Stop();
    //            }
    //        }
    //        else
    //        {
    //            cubeRend.enabled = false;
    //            canvas.enabled = false;

    //            anim.SetBool("BlinkLoop", false);

    //            audio.Stop();
    //        }
    //    }
    //    else
    //    {
    //        cubeRend.enabled = false;
    //        canvas.enabled = false;

    //        anim.SetBool("BlinkLoop", false);

    //        audio.Stop();
    //    }
    //} //this is for static objects

    //public void InfoTagWarn(CubesAndTags cubesAndTags, Bounds bounds, float obstacleSpeed, float acceleration, Sprite sprite, Vector3 trasl, int i)
    //{
    //    Renderer cubeRend = cubesAndTags.boundingCube[i].GetComponent<Renderer>();
    //    Canvas canvas = cubesAndTags.infoTag[i].GetComponent<Canvas>();
    //    Vector3 targetPoint = cubesAndTags.other.transform.position;
    //    Vector3 dirToTarget = (targetPoint - rayCastPos.transform.position);
    //    float dstToTarget = Vector3.Distance(rayCastPos.position, targetPoint);
    //    cubeRend.enabled = true;
    //    canvas.enabled = true;

    //    Animator anim = cubesAndTags.infoTag[i].GetComponent<Animator>();
    //    AudioSource audio = cubesAndTags.boundingCube[i].GetComponent<AudioSource>();

    //    Transform Panel = cubesAndTags.infoTag[i].transform.GetChild(0);
    //    Image warnSprite = Panel.transform.GetChild(2).GetComponent<Image>();

    //    if (Vector3.Angle(rayCastPos.TransformDirection(Vector3.forward), dirToTarget) < ResourceHandler.instance.visualisationVars.viewAngle / 2)
    //    {
    //        if (Physics.Raycast(rayCastPos.position, dirToTarget, dstToTarget, mask))
    //        {
    //            Debug.DrawLine(rayCastPos.position, targetPoint, Color.red);
    //            float distToWarn = RiskAssessmentFormulaD(rigidbody.velocity.magnitude, 0, 0, dstToTarget);
    //            if (dstToTarget <= Mathf.Abs(distToWarn))
    //            {
    //                float normalizedDist = dstToTarget / Mathf.Abs(distToWarn);
    //                bool blink = false;
    //                if (normalizedDist >= 0 && normalizedDist <= 0.5f)
    //                {
    //                    warnSprite.sprite = ResourceHandler.instance.sprites[29]; //sprite of redSign
    //                    blink = true;
    //                }
    //                else if (normalizedDist > 0.5f && normalizedDist <= 0.8f)
    //                {
    //                    warnSprite.sprite = ResourceHandler.instance.sprites[28]; //sprite of yellowSign
    //                    blink = true;

    //                }
    //                else if (normalizedDist > 0.8f && normalizedDist <= 1.0f)
    //                {
    //                    warnSprite.sprite = ResourceHandler.instance.sprites[27]; //sprite of greenDot
    //                }

    //                anim.SetFloat("Multiplier", 3.0f);
    //                anim.SetBool("BlinkLoop", blink);

    //                PlayAudio(audio, blink);

    //                UpdateInfoTag(cubesAndTags, bounds, "0", sprite, dstToTarget, trasl, i);
    //            }
    //            else
    //            {
    //                warnSprite.sprite = ResourceHandler.instance.sprites[27];

    //                anim.SetBool("BlinkLoop", false);

    //                audio.Stop();
    //            }
    //        }
    //        else
    //        {
    //            warnSprite.sprite = ResourceHandler.instance.sprites[27];

    //            anim.SetBool("BlinkLoop", false);

    //            audio.Stop();
    //        }
    //    }
    //    else
    //    {
    //        warnSprite.sprite = ResourceHandler.instance.sprites[27];

    //        anim.SetBool("BlinkLoop", false);

    //        audio.Stop();
    //    }
    //    UpdateInfoTag(cubesAndTags, bounds, Mathf.RoundToInt(obstacleSpeed * 3.6f).ToString(), sprite, dstToTarget, trasl, i);
    //} //this is for dynamic objects

    //public void InfoTagWarnObstacle2(CubesAndTags cubesAndTags, Bounds bounds, float obstacleSpeed, float acceleration, Sprite sprite, Vector3 trasl, int i)
    //{
    //    Vector3 targetPoint = new Vector3(cubesAndTags.other.transform.position.x, cubesAndTags.other.transform.position.y, cubesAndTags.other.transform.position.z);
    //    float dstToTarget = Vector3.Distance(rayCastPos.position, targetPoint);
    //    Renderer cubeRend = cubesAndTags.boundingCube[i].GetComponent<Renderer>();
    //    Canvas canvas = cubesAndTags.infoTag[i].GetComponent<Canvas>();
    //    cubeRend.enabled = true;
    //    canvas.enabled = true;

    //    Animator anim = cubesAndTags.infoTag[i].GetComponent<Animator>();
    //    AudioSource audio = cubesAndTags.boundingCube[i].GetComponent<AudioSource>();

    //    Transform Panel = cubesAndTags.infoTag[i].transform.GetChild(0);
    //    Image warnSprite = Panel.transform.GetChild(2).GetComponent<Image>();

    //    float dotProduct = Vector3.Dot(gameObject.transform.TransformDirection(Vector3.forward), cubesAndTags.other.transform.TransformDirection(Vector3.forward));
    //    if (dotProduct < Mathf.Cos(20 * Mathf.Deg2Rad) && dotProduct > Mathf.Cos(160 * Mathf.Deg2Rad)) //the obstacle is at the intersection
    //    {
    //        Vector3 targetDst = targetPoint + cubesAndTags.other.transform.TransformDirection(0, 0, Vector3.forward.z * 50f);
    //        Debug.DrawLine(targetPoint, targetDst, Color.magenta);

    //        Vector3 myDst = rayCastPos.position + rayCastPos.transform.TransformDirection(0, 0, Vector3.forward.z * 150f);
    //        Debug.DrawLine(rayCastPos.position, myDst, Color.magenta);

    //        Vector2 intersection = Vector2.zero;
    //        LineSegmentsIntersection(new Vector2(targetPoint.x, targetPoint.z), new Vector2(targetDst.x, targetDst.z), new Vector2(rayCastPos.position.x, rayCastPos.position.z), new Vector2(myDst.x, myDst.z), out intersection);
    //        Vector3 intersection3D = new Vector3(intersection.x, rayCastPos.position.y, intersection.y);
    //        float myDstToIntersection = Vector3.Distance(rayCastPos.position, intersection3D);
    //        float targetDstToIntersection = Vector3.Distance(targetPoint, intersection3D);

    //        float myTTC = myDstToIntersection / rigidbody.velocity.magnitude; //this is the playerCar time-to-collision
    //        float targetTTC = targetDstToIntersection / obstacleSpeed; //this is the obstacle time-to-collision

    //        if (Math.Abs(myTTC - targetTTC) < 2)
    //        {
    //            float softDecelleration = 2; //I consider a soft decelleration
    //            float myTTA = rigidbody.velocity.magnitude / softDecelleration; //this is the PlayerCar time-to-avoidance i.e. the time necessary to avoid a collision

    //            if (myTTA < targetTTC)
    //            {
    //                warnSprite.sprite = ResourceHandler.instance.sprites[27];

    //                anim.SetBool("BlinkLoop", false);

    //                audio.Stop();
    //            }
    //            else
    //            {
    //                float normalizedTime = targetTTC / Mathf.Abs(myTTA);
    //                bool blink = false;

    //                if (normalizedTime >= 0 && normalizedTime <= 0.5f)
    //                {
    //                    warnSprite.sprite = ResourceHandler.instance.sprites[29]; //sprite of redSign
    //                    blink = true;
    //                }
    //                else if (normalizedTime > 0.5f && normalizedTime <= 0.8f)
    //                {
    //                    warnSprite.sprite = ResourceHandler.instance.sprites[28]; //sprite of yellowSign
    //                    blink = true;

    //                }
    //                else if (normalizedTime > 0.8f && normalizedTime <= 1.0f)
    //                {
    //                    warnSprite.sprite = ResourceHandler.instance.sprites[27]; //sprite of greenDot
    //                }

    //                anim.SetFloat("Multiplier", 3.0f);
    //                anim.SetBool("BlinkLoop", blink);

    //                PlayAudio(audio, blink);
    //            }
    //        }
    //        else
    //        {
    //            warnSprite.sprite = ResourceHandler.instance.sprites[27];

    //            anim.SetBool("BlinkLoop", false);

    //            audio.Stop();
    //        }
    //    }
    //    UpdateInfoTag(cubesAndTags, bounds, Mathf.RoundToInt(obstacleSpeed * 3.6f).ToString(), sprite, dstToTarget, trasl, i);
    //} //this is for dynamic objects

    public void UpdateInfoTag(CubesAndTags cubesAndTags, Bounds bounds, string text, Sprite sprite, float dstToTarget, Vector3 trasl, int i)
    {
        Transform Panel = cubesAndTags.infoTag[i].transform.GetChild(0);

        if (cubesAndTags.other.gameObject.layer.Equals(16) && !cubesAndTags.other.CompareTag("Sign"))
        {
            cubesAndTags.infoTag[i].transform.position = cubesAndTags.other.transform.position + bounds.center + cubesAndTags.infoTag[i].transform.TransformDirection(0, Vector3.up.y * trasl.y, -Vector3.forward.z * trasl.z);
            Panel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text + " KPH"; //KPH
            Panel.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = Mathf.RoundToInt(dstToTarget).ToString() + " M"; //DIST
            Panel.transform.GetChild(3).GetComponent<Image>().sprite = sprite; //SPRITE
        }
        else if ((cubesAndTags.other.gameObject.layer.Equals(16) && cubesAndTags.other.CompareTag("Sign")) || cubesAndTags.other.gameObject.layer.Equals(17))
        {
            cubesAndTags.infoTag[i].transform.position = cubesAndTags.other.transform.position + bounds.center + cubesAndTags.infoTag[i].transform.TransformDirection(-Vector3.forward);
            Panel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = Mathf.RoundToInt(dstToTarget).ToString() + " M"; //DIST
            Panel.transform.GetChild(1).GetComponent<Image>().sprite = sprite; //SPRITE
        }
        else if (cubesAndTags.other.gameObject.layer.Equals(8))
        {
            cubesAndTags.infoTag[i].transform.position = cubesAndTags.other.bounds.center + cubesAndTags.infoTag[i].transform.TransformDirection(0, Vector3.up.y * bounds.size.y, 0); //I have considered the Collider bounds which gives me the world position of the center of the object. I have to do this otherwise the Scooterman has its center on the ground which is not good in order to position the infoTag correctly.
            Panel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text + " KPH"; //KPH
            Panel.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = Mathf.RoundToInt(dstToTarget).ToString() + " M"; //DIST
            Panel.transform.GetChild(3).GetComponent<Image>().sprite = sprite; //SPRITE
        }
        else if (cubesAndTags.other.gameObject.layer.Equals(12))
        {
            cubesAndTags.infoTag[i].transform.position = cubesAndTags.other.transform.position + cubesAndTags.infoTag[i].transform.TransformDirection(0, Vector3.up.y * (bounds.size.y + cubesAndTags.infoTag[0].transform.localScale.y * cubesAndTags.infoTag[0].GetComponent<RectTransform>().rect.height * 0.5f), 0);
            Panel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text + " KPH"; //KPH
            Panel.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = Mathf.RoundToInt(dstToTarget).ToString() + " M"; //DIST
            Panel.transform.GetChild(3).GetComponent<Image>().sprite = sprite; //SPRITE
        }

        else if (cubesAndTags.other.gameObject.layer.Equals(18))
        {
            cubesAndTags.infoTag[i].transform.position = cubesAndTags.other.transform.position + bounds.center + cubesAndTags.infoTag[i].transform.TransformDirection(0, Vector3.up.y * trasl.y, -Vector3.forward.z * trasl.z);
            Panel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text + " KPH"; //KPH
            Panel.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = Mathf.RoundToInt(dstToTarget).ToString() + " M"; //DIST
            Panel.transform.GetChild(3).GetComponent<Image>().sprite = sprite; //SPRITE    
        }

        float distMax = 150f;
        float distMin = 5.5f;
        float scaleMax = (infoTagStartScale.x * distMax) / distMin;
        float offset = Vector3.Distance(cubesAndTags.infoTag[i].transform.position, driverCam.transform.position);
        float newScale = infoTagResize.Evaluate(offset / distMax) * scaleMax;
        cubesAndTags.infoTag[i].transform.localScale = new Vector3(newScale, newScale, newScale);
        cubesAndTags.infoTag[i].transform.LookAt((2 * cubesAndTags.infoTag[i].transform.position - driverCam.transform.position));
    }

    float RiskAssessmentFormulaD(float subjectVehicleSpeed, float obstacleSpeed, float obstacleAcc, float dist, float systemAcc)
    {
        if (Mathf.Abs(obstacleAcc) <= 0.1f) //it is a static object or trafficCar
        {
            return (subjectVehicleSpeed - obstacleSpeed) * ResourceHandler.instance.visualisationVars.freeRunningTime + Mathf.Pow(subjectVehicleSpeed - obstacleSpeed, 2) / (2 * systemAcc);
        }
        else
        {
            float obstacleComponent = Mathf.Pow(obstacleSpeed, 2) / (2 * systemAcc);
            return subjectVehicleSpeed * ResourceHandler.instance.visualisationVars.freeRunningTime + Mathf.Pow(subjectVehicleSpeed, 2) / (2 * systemAcc) - obstacleComponent;
        }

    }

    bool LineSegmentsIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector3 p4, out Vector2 intersection)
    {
        intersection = Vector2.zero;

        var d = (p2.x - p1.x) * (p4.y - p3.y) - (p2.y - p1.y) * (p4.x - p3.x);

        if (d == 0.0f)
        {
            return false;
        }

        var u = ((p3.x - p1.x) * (p4.y - p3.y) - (p3.y - p1.y) * (p4.x - p3.x)) / d;
        var v = ((p3.x - p1.x) * (p2.y - p1.y) - (p3.y - p1.y) * (p2.x - p1.x)) / d;

        if (u < 0.0f || u > 1.0f || v < 0.0f || v > 1.0f)
        {
            return false;
        }

        intersection.x = p1.x + u * (p2.x - p1.x);
        intersection.y = p1.y + u * (p2.y - p1.y);

        return true;
    }

    void PlayAudio(AudioSource audio, float normDist, CubesAndTags cubesAndTags)
    {
        if (normDist <= 0.2f)
        {
            if (!audio.isPlaying)
            {
                audio.clip = ResourceHandler.instance.audioClips[1];
                audio.Play();
                Debug.Log(cubesAndTags.other.transform.root.name);
            }
            else if (audio.isPlaying && audio.clip == ResourceHandler.instance.audioClips[2])
            {
                audio.Stop();
            }

        }
        else if (normDist <= 0.5f && normDist > 0.2f)
        {
            if (!audio.isPlaying)
            {
                audio.clip = ResourceHandler.instance.audioClips[2];
                audio.Play();
                Debug.Log(cubesAndTags.other.transform.root.name);
            }
            else if (audio.isPlaying && audio.clip == ResourceHandler.instance.audioClips[1])
            {
                audio.Stop();
            }
        }
        else 
            audio.Stop();
    }

    void SetDashBoardColor(float normDist, CubesAndTags cubesAndTags)
    {
        if (normDist <= 0.2f)
            cubesAndTags.dangerState = CubesAndTags.DangerState.RED;
        else if (normDist <= 0.5f && normDist > 0.2f)
            cubesAndTags.dangerState = CubesAndTags.DangerState.YELLOW;
        else
            cubesAndTags.dangerState = CubesAndTags.DangerState.NONE;

    }*/
}
