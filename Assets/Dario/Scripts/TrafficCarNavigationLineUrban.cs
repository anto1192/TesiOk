using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficCarNavigationLineUrban : MonoBehaviour {

    private LinesUtilsAlt linesUtilsAlt = new LinesUtilsAlt();
    private EnvironmentSensingAltUrbanTrigger envAltUrban;
    private Material mat;

    private const int DETECTION_DIST = 150 * 150; //150m

    private void Awake()
    {
        LoadMaterial();
        LoadPlayerCar();
    }

    void Start()
    {
        linesUtilsAlt.LineRend = linesUtilsAlt.CreateLineRenderer(ref linesUtilsAlt.lineRendererEmpty, "TrafLineRenderer", 1.0f, new Color32(0x4C, 0x8B, 0xDD, 0xFF), mat, () => linesUtilsAlt.InitGlowTexture());
        linesUtilsAlt.lineRendererEmpty.transform.SetParent(transform);
        linesUtilsAlt.lineRendererEmpty.transform.localPosition = Vector3.zero;
        StartCoroutine(NavigationLine(0.25f));
    }

    IEnumerator NavigationLine(float waitTime)
    {
        while (true)
        {
            yield return new WaitForSeconds(waitTime);
            Vector3 offset = envAltUrban.gameObject.transform.position - transform.position;
            float sqrLen = offset.sqrMagnitude;
            if (sqrLen <= DETECTION_DIST)
            {
                linesUtilsAlt.DrawLine(gameObject);
            }
        }
    }

    void LoadMaterial()
    {
        try
        {
            mat = Resources.Load<Material>("Materials/LineRendererGlow");
        }
        catch (Exception e)
        {
            Debug.Log("Loading material failed with the following exception: ");
            Debug.Log(e);
        }
    }

    void LoadPlayerCar()
    {
        envAltUrban = FindObjectOfType(typeof(EnvironmentSensingAltUrbanTrigger)) as EnvironmentSensingAltUrbanTrigger;
    }

    //void AddTrafficCarRef()
    //{
    //    int properKey = 0;
    //    if (envAltUrban.IDsAndGos.ContainsKey(transform.GetChild(0).GetChild(0).gameObject.GetInstanceID()))
    //        properKey = transform.GetChild(0).GetChild(0).gameObject.GetInstanceID();
    //    else if (envAltUrban.IDsAndGos.ContainsKey(transform.GetChild(0).GetChild(1).gameObject.GetInstanceID()))
    //        properKey = transform.GetChild(0).GetChild(1).gameObject.GetInstanceID();

    //    if (envAltUrban.IDsAndGos.ContainsKey(properKey))
    //        envAltUrban.IDsAndGos[properKey].lineRend = linesUtilsAlt.LineRend.gameObject;
    //}
}
