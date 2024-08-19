using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StimulusLocation
{
    public List<float> logContrasts = new();
    public List<float> reversals = new();
    public List<string> responses = new();

    public float reversalAverage = 0;

    public float threshold = 0f;

    string separator = ",";
    public int groupNumber;
    public int xPos;
    public int yPos;
    
    //false negative
    public List<string> candidateFalseNeg = new();
    public int numFalseNeg = 0;
    public int falseNegMissed = 0;

    //arrays for Weibull
    //Initial range of search parameters
    private float minL0 = -3f;
    private float maxL0 = 0f;
    private float inc = 0.0001f;

    List<float> lambda = new List<float>();
    List<float> lambda_10 = new List<float>();

    //Shape parameter: 
    float mink = 2f;
    float maxk = 2.5f;
    float inck = 0.1f;

    List<float> k0 = new List<float>();

    //Weibull function variables
    int numElementsLambda;
    int numElementsK;

    float g = 0;

    List<float> log10c = new List<float>();


    //Correct and incorrect responses
    List<float> CRx = new List<float>();
    List<float> IRx = new List<float>();
    List<float> CRx_10 = new List<float>();
    List<float> IRx_10 = new List<float>();

    //Find maximum likelihood estimate
    float LL0 = float.MinValue;
    float optLambda = float.MinValue;
    float mleK = float.MinValue;

    //Initialize lists for likelihood
    List<float> CR = new List<float>();
    List<float> IR = new List<float>();
    float LL = 0f;

    //csv reading and writing
    string resultsFilename = "";
    bool isLog;

    public StimulusLocation(int group, int x, int y, bool log = false)
    {
        //Initialize location with relevant parameters
        xPos = x;
        yPos = y;
        groupNumber = group;

        //Check if read file is a log file or preliminary results file
        isLog = log;

        //resultsFilename = resFile;
        logContrasts = new();
        reversals = new();
        responses = new();
        candidateFalseNeg = new();
        lambda = new();
        lambda_10 = new();
        k0 = new();
        CRx = new();
        IRx = new();
        CRx_10 = new();
        IRx_10 = new();
        CR = new();
        IR = new();
    }


    public void finalizeLocation()
    {
        if (isLog)
        {
            reversalAverage = average(reversals);
        }

        //Set up Weibull parameters, run Weibull calculation, then print to results file
        Weibull_Setup();
        own_fitweibull();
        final_fn_calculation();
    }

    private void final_fn_calculation()
    {
        foreach (string res in candidateFalseNeg)
        {
            if (UnitConversions.Cdm2_to_dB(UnitConversions.Contrast_to_Cdm2(threshold)) > (21))
            {
                numFalseNeg++;
                if (res.Contains("No"))
                {
                    falseNegMissed++;
                }
            }


        }
    }

    void Weibull_Setup()
    {
        //Scale parameter: 
        int numElementsLambda = (int)((maxL0 - minL0) / inc + 1);
        int numElementsK = (int)((maxk - mink) / inck + 1);
        for (int i = 0; i < numElementsLambda; i++)
        {
            lambda.Add(minL0 + i * inc);
            lambda_10.Add(Mathf.Pow(10, lambda[i]));
        }

        for (int i = 0; i < numElementsK; i++)
        {
            k0.Add(mink + i * inck);
        }

    }

    List<float> Weibull(List<float> x, float k, float lambda, float g)
    {
        List<float> to_ret = new List<float>(x.Count);
        for (int i = 0; i < x.Count; i++)
        {
            float ret = g + (1 - g) * (1 - Mathf.Exp(-Mathf.Pow(x[i] / lambda, k)));
            to_ret.Add(ret);
        }

        return to_ret;
    }

    void own_fitweibull()
    {

        numElementsLambda = (int)((maxL0 - minL0) / inc + 1);
        numElementsK = (int)((maxk - mink) / inck + 1);

        g = 0; //Should this be a global variable?


        for (int i = 0; i < responses.Count; i++)
        {
            //log10c.Add(Mathf.Log10(contrast_level[i]));
            log10c.Add(logContrasts[i]);
        }


        for (int i = 0; i < responses.Count; i++)
        {
            if (responses[i] == "Yes")
            {
                CRx.Add(log10c[i]);
                CRx_10.Add(Mathf.Pow(10, logContrasts[i]));
            }
            else
            {
                IRx.Add(log10c[i]);
                IRx_10.Add(Mathf.Pow(10, logContrasts[i]));
            }
        }


        for (int cc = 0; cc < numElementsLambda; cc++)
        {
            for (int ss = 0; ss < numElementsK; ss++)
            {
                //Log likelihood
                CR = Weibull(CRx_10, k0[ss], lambda_10[cc], g);
                IR = Weibull(IRx_10, k0[ss], lambda_10[cc], g);

                for (int i = 0; i < CR.Count; i++)
                {
                    CR[i] = Mathf.Log(CR[i]);
                    LL += CR[i];
                }
                for (int i = 0; i < IR.Count; i++)
                {
                    IR[i] = Mathf.Log(1 - IR[i]);
                    LL += IR[i];
                }


                //Maximum log likelihood
                if (LL > LL0)
                {
                    optLambda = lambda[cc];
                    mleK = k0[ss];
                    LL0 = LL;
                }
                LL = 0f;
            }
        }


        //manually estimate standard error (note: make into separate function...)


        List<float> xse = new List<float>() { optLambda - inc, optLambda, optLambda + inc };
        List<float> CRse1 = new List<float>();
        List<float> CRse2 = new List<float>();
        List<float> CRse3 = new List<float>();
        List<float> IRse1 = new List<float>();
        List<float> IRse2 = new List<float>();
        List<float> IRse3 = new List<float>();
        float LLse1 = 0f;
        float LLse2 = 0f;
        float LLse3 = 0f;

        CRse1 = Weibull(CRx_10, mleK, Mathf.Pow(10, xse[0]), g);
        CRse2 = Weibull(CRx_10, mleK, Mathf.Pow(10, xse[1]), g);
        CRse3 = Weibull(CRx_10, mleK, Mathf.Pow(10, xse[2]), g);
        for (int i = 0; i < CRx_10.Count; i++)
        {
            CRse1[i] = Mathf.Log(CRse1[i]);
            LLse1 += CRse1[i];
            CRse2[i] = Mathf.Log(CRse2[i]);
            LLse2 += CRse2[i];
            CRse3[i] = Mathf.Log(CRse3[i]);
            LLse3 = CRse3[i];
        }

        IRse1 = Weibull(IRx_10, mleK, Mathf.Pow(10, xse[0]), g);
        IRse2 = Weibull(IRx_10, mleK, Mathf.Pow(10, xse[1]), g);
        IRse3 = Weibull(IRx_10, mleK, Mathf.Pow(10, xse[2]), g);
        for (int i = 0; i < IRx_10.Count; i++)
        {
            IRse1[i] = Mathf.Log(1 - IRse1[i]);
            LLse1 += IRse1[i];
            IRse2[i] = Mathf.Log(1 - IRse2[i]);
            LLse2 += IRse2[i];
            IRse3[i] = Mathf.Log(1 - IRse3[i]);
            LLse3 += IRse3[i];
        }


        threshold = Mathf.Pow(10, optLambda); // weibull


        log10c.Clear();
        CRx.Clear();
        IRx.Clear();
        CRx_10.Clear();
        IRx_10.Clear();
        CR.Clear();
        IR.Clear();
    }
    float average(List<float> nums)
    {
        if (nums.Count == 0)
            return 0;

        float sum = 0;
        foreach (float num in nums)
        {
            sum += num;
        }
        return sum / nums.Count;
    }
}
