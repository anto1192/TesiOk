
using System.Reflection;
using UnityEngine;

public class ScenarioTestExtraurbano : MonoBehaviour
{



    public static void gestisciEvento(int idPrecedente, int nuovoId)
    {
        string nomeMetodo = "evento" + nuovoId;
        MethodInfo mi = ScenarioTestUrbano.getInstance().GetType().GetMethod(nomeMetodo);
        Debug.Log("Sono in gestisciEvento " + nomeMetodo + "; mi == null? " + mi == null);
        if (mi != null)
        {
            mi.Invoke(ScenarioTestUrbano.getInstance(), null);
        }
    }

    private GameObject ottieniRiferimentoPlayer()
    {
        GameObject go = GameObject.Find("XE_Rigged");
        if (go == null)
        {
            go = GameObject.Find("XE_Rigged(Clone)");
        }
        if (go == null)
        {
            go = GameObject.Find("TeslaModelS_2_Rigged (1)");
        }
        if (go == null)
        {
            go = GameObject.Find("TeslaModelS_2_Rigged (1)(Clone)");
        }
        return go;
    }
}