using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RiskAssessmentSelective
{
    private LinesUtils linesUtils;
    private Transform rayCastPos;
    private GameObject gameObject;
    private Rigidbody rigidbody;
    private VehicleController vehicleController;
    private LayerMask mask;
    private Vector3 infoTagStartScale;
    private DriverCamera driverCam;
    private AnimationCurve infoTagResize;
    private VisualisationVars visualisationVars;

    public RiskAssessmentSelective(Vector3 infoTagStartScale, DriverCamera driverCam, AnimationCurve infoTagResize, Transform rayCastPos, GameObject gameObject, LayerMask mask, LinesUtils linesUtils)
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

        if (Vector3.Angle(rayCastPos.TransformDirection(Vector3.forward), dirToTarget) < viewAngle / 2)
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

                    

                }
            }
            else
            {
                cubeRend.enabled = false;
                canvas.enabled = false;

                animTag.SetBool("BlinkLoop", false);

                

            }
        }
        else
        {
            cubeRend.enabled = false;
            canvas.enabled = false;

            animTag.SetBool("BlinkLoop", false);

            

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

        float viewAngle = 5f;

        if (dstToTarget <= 8f)
            viewAngle = 25f;

        if (trafAIMotor.hasNextEntry) //If I am waiting at the intersection
        {
            float dist = Vector3.Distance(rayCastPos.position, trafAIMotor.currentEntry.waypoints[trafAIMotor.currentEntry.waypoints.Count - 1]);
            //Debug.Log(dist);

            if (dist <= 6.0f) //and I am located near the line that defines the intersection. This second condition is necessary since hasNextEntry is set earlier, not in correspondence with the line of the crossing.
            {
                cubeRend.enabled = false;
                canvas.enabled = false;

                anim.SetBool("BlinkLoop", false);

                

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

                    cubeRend.enabled = true;
                    canvas.enabled = true;
                    UpdateInfoTag(cubesAndTags, bounds, Mathf.RoundToInt(obstacleSpeed * 3.6f).ToString(), sprite, dstToTarget, trasl, i);

                    anim.SetFloat("Multiplier", 3.0f);
                    anim.SetBool("BlinkLoop", blink);

                    PlayAudio(audio, dstToTargetEncoded / Mathf.Abs(distToWarnEncoded), cubesAndTags);

                }
                else
                {
                    cubeRend.enabled = false;
                    canvas.enabled = false;

                    anim.SetBool("BlinkLoop", false);

                    

                    cubesAndTags.dangerState = CubesAndTags.DangerState.NONE;
                }
            }
            else
            {
                cubeRend.enabled = false;
                canvas.enabled = false;

                anim.SetBool("BlinkLoop", false);

                

                cubesAndTags.dangerState = CubesAndTags.DangerState.NONE;
            }
        }
        else
        {
            cubeRend.enabled = false;
            canvas.enabled = false;

            anim.SetBool("BlinkLoop", false);

            

            cubesAndTags.dangerState = CubesAndTags.DangerState.NONE;
        }
    } //this is for dynamic objects

    public void BoundingCubeLerperObstacleDefSF(CubesAndTags cubesAndTags, Bounds bounds, float obstacleSpeed, float acceleration, Sprite sprite, Vector3 trasl, int i)
    {
        Vector3 targetPoint = new Vector3(cubesAndTags.other.transform.position.x, rayCastPos.transform.position.y, cubesAndTags.other.transform.position.z);
        float dstToTarget = Vector3.Distance(rayCastPos.position, targetPoint);
        Renderer cubeRend = cubesAndTags.boundingCube[i].GetComponent<Renderer>();
        Canvas canvas = cubesAndTags.infoTag[i].GetComponent<Canvas>();

        UpdateInfoTag(cubesAndTags, bounds, Mathf.RoundToInt(obstacleSpeed * 3.6f).ToString(), sprite, dstToTarget, trasl, i);

        Animator anim = cubesAndTags.infoTag[i].GetComponent<Animator>();
        AudioSource audio = cubesAndTags.boundingCube[i].GetComponent<AudioSource>();

        Vector3 targetDst = targetPoint + cubesAndTags.other.transform.TransformDirection(0, 0, Vector3.forward.z * 50f);
        Debug.DrawLine(targetPoint, targetDst, Color.magenta);

        Vector3 myDst = rayCastPos.position + rayCastPos.transform.TransformDirection(0, 0, Vector3.forward.z * 150f);

        Vector2 intersection = Vector2.zero;
        if (LineSegmentsIntersection(new Vector2(targetPoint.x, targetPoint.z), new Vector2(targetDst.x, targetDst.z), new Vector2(rayCastPos.position.x, rayCastPos.position.z), new Vector2(myDst.x, myDst.z), out intersection) && (obstacleSpeed >= 0.1f && rigidbody.velocity.magnitude >= 0.1f) /*&& Mathf.Abs(vehicleController.steerInput) <= 0.4f*/)
        {
            Vector3 intersection3D = new Vector3(intersection.x, rayCastPos.position.y, intersection.y);
            Debug.DrawLine(rayCastPos.position, intersection3D, Color.black);
            float distToWarn = rigidbody.velocity.magnitude * ResourceHandler.instance.visualisationVars.freeRunningTime + 0.5f * (Mathf.Pow(rigidbody.velocity.magnitude, 2) / ResourceHandler.instance.visualisationVars.systemAccSF); //DARIO

            if (dstToTarget <= distToWarn)
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
                UpdateInfoTag(cubesAndTags, bounds, Mathf.RoundToInt(obstacleSpeed * 3.6f).ToString(), sprite, dstToTarget, trasl, i);

                anim.SetFloat("Multiplier", 3.0f);
                anim.SetBool("BlinkLoop", blink);

                PlayAudio(audio, dstToTargetEncoded / distToWarnEncoded, cubesAndTags);

                cubesAndTags.prevState = dstToTargetEncoded / distToWarnEncoded;

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

                Color32 topColor = linesUtils.ChangeMatByDistance(value, ref blink, ref cubesAndTags);
                Color32 bottomColor = linesUtils.ChangeMatByDistance(value, ref blink, ref cubesAndTags);

                topColor.a = 0;
                bottomColor.a = 0x51;
                cubeRend.material.SetColor("_Color1", topColor);
                cubeRend.material.SetColor("_Color2", bottomColor);

                anim.SetBool("BlinkLoop", false);

                cubesAndTags.dangerState = CubesAndTags.DangerState.NONE;

                if (cubesAndTags.prevState >= 0.99f)
                {
                    cubesAndTags.gradient = CubesAndTags.Gradient.OFF;
                    cubeRend.enabled = false;
                    canvas.enabled = false;
                }
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

            Color32 topColor = linesUtils.ChangeMatByDistance(value, ref blink, ref cubesAndTags);
            Color32 bottomColor = linesUtils.ChangeMatByDistance(value, ref blink, ref cubesAndTags);

            topColor.a = 0;
            bottomColor.a = 0x51;
            cubeRend.material.SetColor("_Color1", topColor);
            cubeRend.material.SetColor("_Color2", bottomColor);

            anim.SetBool("BlinkLoop", false);

            cubesAndTags.dangerState = CubesAndTags.DangerState.NONE;

            if (cubesAndTags.prevState >= 0.99f)
            {
                cubesAndTags.gradient = CubesAndTags.Gradient.OFF;
                cubeRend.enabled = false;
                canvas.enabled = false;
            }    
        }   
    }//this is for dynamic objects

    public void BoundingCubeLerperScooterSF(CubesAndTags cubesAndTags, Bounds bounds, float obstacleSpeed, Sprite sprite, Vector3 trasl, int i)
    {
        Vector3 targetPoint = new Vector3(cubesAndTags.other.transform.position.x, rayCastPos.transform.position.y, cubesAndTags.other.transform.position.z);
        float dstToTarget = Vector3.Distance(rayCastPos.position, targetPoint);
        Vector3 dirToTarget = (targetPoint - rayCastPos.transform.position);
        Renderer cubeRend = cubesAndTags.boundingCube[i].GetComponent<Renderer>();
        Canvas canvas = cubesAndTags.infoTag[i].GetComponent<Canvas>();

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

                        cubeRend.enabled = true;
                        canvas.enabled = true;
                        UpdateInfoTag(cubesAndTags, bounds, Mathf.RoundToInt(obstacleSpeed * 3.6f).ToString(), sprite, dstToTarget, trasl, i);

                        anim.SetFloat("Multiplier", 3.0f);
                        anim.SetBool("BlinkLoop", blink);

                        PlayAudio(audio, dstToTargetEncoded / Mathf.Abs(distToWarnEncoded), cubesAndTags);

                    }
                    else
                    {
                        cubeRend.enabled = false;
                        canvas.enabled = false;

                        anim.SetBool("BlinkLoop", false);

                        

                        cubesAndTags.dangerState = CubesAndTags.DangerState.NONE;
                    }
                }
                else
                {
                    cubeRend.enabled = false;
                    canvas.enabled = false;

                    anim.SetBool("BlinkLoop", false);

                    

                    cubesAndTags.dangerState = CubesAndTags.DangerState.NONE;
                }
            }
            return;
        }

        if (Vector3.Angle(rayCastPos.TransformDirection(Vector3.forward), dirToTarget) < 75f)
        {
            if (dstToTarget <= ResourceHandler.instance.visualisationVars.DangerousCarDistToWarn && vehicleController.CurrentSpeed >= 0.1f)
            {
                bool blink = false;
                Color32 topColor = linesUtils.ChangeMatByDistance(dstToTarget / Mathf.Abs(ResourceHandler.instance.visualisationVars.DangerousCarDistToWarn), ref blink, ref cubesAndTags);
                topColor.a = 0;
                Color32 bottomColor = linesUtils.ChangeMatByDistance(dstToTarget / Mathf.Abs(ResourceHandler.instance.visualisationVars.DangerousCarDistToWarn), ref blink, ref cubesAndTags);
                bottomColor.a = 0x51;
                cubeRend.material.SetColor("_Color1", topColor);
                cubeRend.material.SetColor("_Color2", bottomColor);

                cubeRend.enabled = true;
                canvas.enabled = true;
                UpdateInfoTag(cubesAndTags, bounds, Mathf.RoundToInt(obstacleSpeed * 3.6f).ToString(), sprite, dstToTarget, trasl, i);

                anim.SetFloat("Multiplier", 3.0f);
                anim.SetBool("BlinkLoop", blink);

                PlayAudio(audio, dstToTarget / ResourceHandler.instance.visualisationVars.DangerousCarDistToWarn, cubesAndTags);

            }
            else
            {
                cubeRend.enabled = false;
                canvas.enabled = false;

                anim.SetBool("BlinkLoop", false);

                

                cubesAndTags.dangerState = CubesAndTags.DangerState.NONE;
            }
        }
        else
        {
            cubeRend.enabled = false;
            canvas.enabled = false;

            anim.SetBool("BlinkLoop", false);

            

            cubesAndTags.dangerState = CubesAndTags.DangerState.NONE;
        }
    } //this is for dynamic objects (scooter)



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
            float height = cubesAndTags.infoTag[i].GetComponent<RectTransform>().rect.height * cubesAndTags.infoTag[i].transform.localScale.y;
            cubesAndTags.infoTag[i].transform.position = cubesAndTags.other.transform.position + bounds.center + /*cubesAndTags.infoTag[i].transform.TransformDirection(-Vector3.forward)*/cubesAndTags.infoTag[i].transform.TransformDirection(0, Vector3.up.y * height, 0);
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
        if (cubesAndTags.alreadyPlayed.Equals(false))
        {
            if (normDist <= 0.2f)
            {
                audio.PlayOneShot(ResourceHandler.instance.audioClips[9]);
                cubesAndTags.alreadyPlayed = true;
            }
        }
    }
}
