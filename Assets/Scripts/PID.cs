using UnityEngine;
using System.Collections;

public class PID : MonoBehaviour
{

        public float pFactor, iFactor, dFactor;

        float integral;
        float lastError;


        public PID(float pFactor, float iFactor, float dFactor)
        {
            this.pFactor = pFactor;
            this.iFactor = iFactor;
            this.dFactor = dFactor;
        }


        public float UpdatePars(float setpoint, float actual, float timeFrame)
        {
            float present = setpoint - actual;
            integral += present * timeFrame;
            float deriv = (present - lastError) / timeFrame;
            lastError = present;
            return present * pFactor + integral * iFactor + deriv * dFactor;
        }
}
