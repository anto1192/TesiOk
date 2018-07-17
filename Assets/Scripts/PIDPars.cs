using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PIDPars", menuName = "Pid/Pars", order = 1)]
public class PIDPars : ScriptableObject
{
    public float p_sterzata = 4.7f;
    public float i_sterzata = 1.4f;
    public float d_sterzata = 0.8f;
    public float puntoMortoSterzata = 0.05f;
    public float differenzaSterzata = 0.1f;
    public float indiceSterzataDritto = 25f;
    public float indiceSterzataCurva = 6f;
    public float indiceSterzataMassima = 11f;
    public float p_accelerazione = 0.21f;
    public float i_accelerazione = 0.007f;
    public float d_accelerazione = 0.01f;
    public float differenzaAccelerazione = 0.1f;
    public float indiceAccelerazione = 15f;
    public float puntoMortoFrenata = 0.01f;
    public float sogliaFermata = 0.6f;

    public float sogliaNoGasTraffico = 1.5f; //se la differenza di velocità è minore di questa soglia non freno, rallento solo
    public float sogliaNoGas = 2.5f;

    public float p_sterzataPCH = 41f;
    public float i_sterzataPCH = 0.3f;
    public float d_sterzataPCH = 0.3f;
    public float velocitaFrenata = 10f;
    public float velocitaAccelerazione = 10f;
    public float offsetSpringForce = 1.6f;

    public int offset = 0;
    public int saturazione = 10000;
    public int coefficiente = 10000;

    public int violenzaPiattaforma = 7;

    public float partenzaAuto = 0.95f;
}
