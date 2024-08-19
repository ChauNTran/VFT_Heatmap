using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitConversions : MonoBehaviour
{

    public static float lineSlope = 1f / (83.174f - 10f);
    public static float lineIntercept = -(lineSlope * 10f);

    public static float dB_to_Cdm2(float db)
    {
        return (float)((1 / System.Math.PI) * Mathf.Pow(10, (db / (-10)) + 4) + 10);
    }

    public static float Cdm2_to_dB(float cdm2)
    {
        return (float)(-10 * (Mathf.Log10((float)((cdm2 - 10) * System.Math.PI)) - 4));
    }

    public static float Cdm2_to_Contrast(float cdm2)
    {
        return (float)(lineSlope * cdm2 + lineIntercept);
    }

    public static float Contrast_to_Cdm2(float contrast)
    {
        return (float)((contrast - lineIntercept) / lineSlope);
    }

    public static float dB_to_Contrast(float dB)
    {
        return Cdm2_to_Contrast(dB_to_Cdm2(dB));
    }

    public static float Contrast_to_dB(float contrast)
    {
        return Cdm2_to_dB(Contrast_to_Cdm2(contrast));
    }
}
