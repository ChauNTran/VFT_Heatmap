using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[Serializable]
public class ResultEntry
{
    public int group;
    public int locationX;
    public int locationY;
    public string reversals;
    public string averageReversalLogContrast;
    public float averageReversal;
    public float weibull;
    public string StringifyEntry ()
    {
        return $"{group},{locationX},{locationY},{reversals},{averageReversalLogContrast},{averageReversal},{weibull}";
    }
    public ResultEntry (int _group, int _locationX, int _locationY, string _reversals, string _averageReversalLogContrast, float _averageReversal, float _weibull)
    {
        group = _group;
        locationX = _locationX;
        locationY = _locationY;
        reversals = _reversals;
        averageReversalLogContrast = _averageReversalLogContrast;
        averageReversal = _averageReversal;
        weibull = _weibull;
    }

}

public class LogAnalyzer : MonoBehaviour
{
    public List<ResultEntry> resultEntries;
    public List<string> resultHeader;
    private Dictionary<Tuple<int, int>, StimulusLocation> locations;

    private int FPs = 0;
    private int trials = 0;
    private int fix_checks = 0;
    private int fix_loss = 0;
    private float percent_FN;
    private float percent_FP;
    private float percent_FL;

    private Graph _graph;

    private void Awake()
    {
        _graph = GetComponent<Graph>();
        resultEntries = new();
        locations = new();
        resultHeader = new();
    }

    public void ProcessLogFile(string filepath)
    {
        locations.Clear();
        resultEntries.Clear();
        resultHeader.Clear();

        string content;
        string[] lines;

        try
        {
            StreamReader reader = new StreamReader(File.OpenRead(filepath));
            content = reader.ReadToEnd();
            lines = content.Split("\n");
            Debug.Log(content);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            if (e.Message.Contains("Sharing violation on path"))
                _graph.ShowFeedback("Please close the file before importing");
            return;
        }

        FPs = 0;
        trials = 0;
        fix_checks = 0;
        fix_loss = 0;

        for(int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            if (i == 0)
            {
                resultHeader.Add(line);
                continue;
            }
            else if(i == 1)
            {
                continue;
            }

            if (string.IsNullOrEmpty(line))
                continue;

            trials++;
            string[] values = line.Split(',');

            int group = int.Parse(values[0]);
            int locationX = int.Parse(values[1]);
            int locationY = int.Parse(values[2]);
            string type = values[3];
            float contrastLevel = float.Parse(values[4]);
            float logContrast = float.Parse(values[5]);
            string humphrey = values[6];
            string response = values[7];
            //string reversalYN = values[8];
            //int reversal = int.Parse(values[9]);

            // Group number, Location: X, Location: Y, 3Test type, Contrast levels, 5Log10 Contrasts,	Humphrey dB Contrasts,	7Responses,	Reversal? yes/no,	Reversals,	Average Reversal Contrast
            Tuple<int, int> loc = new Tuple<int, int>(locationX, locationY);

            if (type == "Default")
            {
                //If that location hasn't already been added to the dictionary, add it along with the values from the first line
                if (!locations.ContainsKey(loc))
                {
                    StimulusLocation stim = new StimulusLocation(group, loc.Item1, loc.Item2, true);
                    stim.logContrasts.Add(logContrast);
                    stim.responses.Add(response);

                    locations.Add(loc, stim);
                }
                //Otherwise, add the relevant values for all default tests
                else
                {
                    locations[loc].logContrasts.Add(logContrast);
                    locations[loc].responses.Add(response);

                    //Add reversal contrast if the stimulus had a reversal
                    if (values[8] == "Yes")
                    {
                        locations[loc].reversals.Add(logContrast);
                    }
                }
            }
            else if (type == "FN")
            {
                if (!locations.ContainsKey(loc))
                {
                    StimulusLocation stim = new StimulusLocation(group, loc.Item1, loc.Item2, true);
                    stim.candidateFalseNeg.Add(response);
                }
                else
                {
                    locations[loc].candidateFalseNeg.Add(response);
                }
            }
            else if (type == "False positive")
            {
                FPs++;
            }
            else if (type == "Fixation check")
            {
                fix_checks++;

                if (response == "Yes")
                {
                    fix_loss++;
                }
            }
        }

        //Initiate counters for false negative data
        int fnTotal = 0;
        int fnMissed = 0;
        int fnCandidates = 0;

        int maxAxis = 0;
        int secondToMax = 0;
        int step = 0;

        foreach (var pair in locations)
        {
            //do the Weibull calculations
            pair.Value.finalizeLocation();

            //Get false negative data from the stimulus locations
            fnTotal += pair.Value.numFalseNeg;
            fnMissed += pair.Value.falseNegMissed;
            fnCandidates += pair.Value.candidateFalseNeg.Count;

            if (maxAxis < Mathf.Abs(pair.Value.xPos))
                maxAxis = Mathf.Abs(pair.Value.xPos);

            if (secondToMax < Mathf.Abs(pair.Value.xPos) && maxAxis != Mathf.Abs(pair.Value.xPos))
            {
                secondToMax = Mathf.Abs(pair.Value.xPos);
                step = maxAxis - secondToMax;
            }
                ResultEntry entry = new ResultEntry(
                pair.Value.groupNumber,
                pair.Value.xPos,
                pair.Value.yPos,
                string.Join(';', pair.Value.logContrasts),
                string.Join(';', pair.Value.responses),
                UnitConversions.Contrast_to_dB(Mathf.Pow(10, pair.Value.reversalAverage)),
                UnitConversions.Contrast_to_dB(pair.Value.threshold)
            );
            resultEntries.Add(entry);
        }
        //Debug.Log("trials " + trials);
        //Debug.Log("FPs " + FPs);
        //Output the false negative data
        percent_FN = fnTotal > 0 ? (float)fnMissed / fnTotal : 0;
        percent_FP = trials > 0 ? (float)FPs / trials : 0;
        percent_FL = fix_checks > 0 ? (float)fix_loss / fix_checks : 0;

        string info = "FN: " + (percent_FN * 100f).ToString("00.00") + "%\n";
        info += "FP: " + (percent_FP * 100f).ToString("00.00") + "%\n";
        info += "FL: " + (percent_FL * 100f).ToString("00.00") + "%\n";

        resultHeader.Add("Total trials,Percent false pos,Percent fixation loss");
        resultHeader.Add($"{trials.ToString()},{percent_FP.ToString("0.000")},{percent_FL.ToString("0.000")}");
        resultHeader.Add($"False neg candidates: , {fnCandidates.ToString()} , False Neg percent: , { percent_FN.ToString("0.000")}");
        resultHeader.Add($"Group,Location: x,Location: y,Log contrasts,Responses,Reversal average (dB),Weibull threshold (dB)");

        _graph.CreateGraphAxis(maxAxis, step);
        _graph.CreateHeatmapFromList(resultEntries);
        _graph.OutputInfo(info);
        _graph.EnableSaveResultButton(true);
        
    }

    public void ProcressResultFile(string filepath)
    {
        StreamReader reader = new StreamReader(File.OpenRead(filepath));
        string resultContent = reader.ReadToEnd();
        string[] lines = resultContent.Split("\n");
   
        resultEntries.Clear();

        if (lines.Length > 6)
        {
            int maxAxis = 0;
            int secondToMax = 0;
            int step = 0;

            for (int l = 0; l < lines.Length; l++)
            {
                string currentLine = lines[l];
                string[] values = currentLine.Split(',');

                if (l == 2)
                {
                    //Total trials, Percent false pos, Percent fixation loss
                    percent_FP = float.Parse(values[1]);
                    percent_FL = float.Parse(values[2]);
                    continue;
                }
                else if(l == 3)
                {
                    percent_FN = float.Parse(values[3]);
                    continue;
                }
                else if (l == 0 || l == 1 || l == 4)
                    continue;


                if (values.Length == 7)
                {
                    int group = int.Parse(values[0]);
                    int locationX = int.Parse(values[1]);
                    int locationY = int.Parse(values[2]);
                    string reversals = values[3];
                    string averageReversalContrast = values[4];
                    float averageReversal = float.Parse(values[5]);
                    float weibull = float.Parse(values[6]);

                    if (maxAxis < Mathf.Abs(locationX))
                        maxAxis = Mathf.Abs(locationX);

                    if (secondToMax < Mathf.Abs(locationX) && maxAxis != Mathf.Abs(locationX))
                    {
                        secondToMax = Mathf.Abs(locationX);
                        step = maxAxis - secondToMax;
                    }

                    resultEntries.Add(new ResultEntry(
                        group,
                        locationX,
                        locationY,
                        reversals,
                        averageReversalContrast,
                        averageReversal,
                        weibull));
                }
                else if (values.Length == 8)
                {
                    int group = int.Parse(values[0]);
                    int locationX = int.Parse(values[1]);
                    int locationY = int.Parse(values[2]);
                    string reversals = values[3];
                    string averageReversalLogContrast = values[4];
                    string averageReversalContrast = values[5];
                    float averageReversal = float.Parse(values[6]);
                    float weibull = float.Parse(values[7]);

                    if (maxAxis < Mathf.Abs(locationX))
                        maxAxis = Mathf.Abs(locationX);

                    if (secondToMax < Mathf.Abs(locationX) && maxAxis != Mathf.Abs(locationX))
                    {
                        secondToMax = Mathf.Abs(locationX);
                        step = maxAxis - secondToMax;
                    }


                    resultEntries.Add(new ResultEntry(
                        group,
                        locationX,
                        locationY,
                        reversals,
                        averageReversalContrast,
                        averageReversal,
                        weibull));
                }

            }

            string info = "FN Percentage: " + percent_FN.ToString("0.000") + "\n";
            info += "FP Percentage: " + percent_FP.ToString("0.000") + "\n";
            info += "FL Percentage: " + percent_FL.ToString("0.000") + "\n";

            _graph.CreateGraphAxis(maxAxis, step);
            _graph.CreateHeatmapFromList(resultEntries);
            _graph.OutputInfo(info);
            _graph.EnableSaveResultButton(false);
        }
        
    }

}