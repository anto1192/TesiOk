using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PIDPars", menuName = "Pid/Pars", order = 1)]
public class PIDPars : ScriptableObject
{
    public float p_sterzata = 1f;
    public float i_sterzata = 1f;
    public float d_sterzata = 0.6f;
    public float puntoMortoSterzata = 0.1f;
    public float differenzaSterzata = 0.1f;
    public float indiceSterzataDritto = 4f;
    public float indiceSterzataCurva = 30f;
    public float indiceSterzataMassima = 2f;
    public float p_accelerazione = 0.3f;
    public float i_accelerazione = 0.005f;
    public float d_accelerazione = 0.001f;
    public float differenzaAccelerazione = 0.1f;
    public float indiceAccelerazione = 20f;
    public float puntoMortoFrenata = 0.01f;
    public float sogliaFermata = 2f;

    public float sogliaNoGasTraffico = 0.75f; //se la differenza di velocità è minore di questa soglia non freno, rallento solo
    public float sogliaNoGas = 2f;

    public float p_sterzataPCH = 4.5f;
    public float i_sterzataPCH = 1.4f;
    public float d_sterzataPCH = 1f;
    public float velocitaFrenata = 15f;

    public float offsetSpringForce = 1.2f;

    public int offset = 0;
    public int saturazione = 0;
    public int coefficiente = 0;

    public int violenzaPiattaforma = 7;
}
