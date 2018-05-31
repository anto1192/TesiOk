using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficCarNavigationLineUrban : MonoBehaviour {

    private LinesUtilsAlt linesUtilsAlt = new LinesUtilsAlt();
    private GameObject pathEnhanced = null;
    private VehicleController vc;
    private Material mat;

    private const int DETECTION_DIST = 22500; //150m

    private void Awake()
    {
        LoadMaterial();
    }

    void Start()
    {
        linesUtilsAlt.LineRend = linesUtilsAlt.CreateLineRenderer(linesUtilsAlt.LineRendererEmpty, "TrafLineRenderer", 1.0f, new Color32(0x4C, 0x8B, 0xDD, 0xFF), mat, () => linesUtilsAlt.InitGlowTexture());
        vc = FindObjectOfType(typeof(VehicleController)) as VehicleController;
        StartCoroutine(NavigationLine(0.25f));
    }

    IEnumerator NavigationLine(float waitTime)
    {
        while (true)
        {
            yield return new WaitForSeconds(waitTime);
            linesUtilsAlt.DrawLine(gameObject);
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
}
