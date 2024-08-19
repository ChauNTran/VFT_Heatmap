using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SFB;
using System.IO;




public class Graph : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private GameObject GraphUnitPrefab;
    [SerializeField] private GameObject HorizontalLabelPrefab;
    [SerializeField] private GameObject VerticalLabelPrefab;
    [SerializeField] private GameObject LabelsGo;
    [SerializeField] private GameObject UnitsGo;

    [Header("UI References")]
    [SerializeField] private Button SaveResultButton;
    [SerializeField] private TMP_Text pathText;
    [SerializeField] private TMP_Text infoText;
    [SerializeField] private CanvasGroup canvas;
    [SerializeField] private TMP_Text FeedbackText;
    [SerializeField] private TMP_Text VersionText;

    [Header ("Graph Creation Settings")]
    //[SerializeField] private Color minColor;
    //[SerializeField] private Color maxColor;
    [SerializeField] private Color[] colorSet;

    private Vector3 graphOrigin = new Vector3(0, 0, 0);
    private Vector3 graphMaxHor = new Vector3(20, 0, 0);
    private Vector3 graphMaxVert = new Vector3(0, 20, 0);

    private int defaultAxisLength = 9;
    private int defaultStepValue = 2;
    private int axisLength;
    private int stepValue;

    private string currentDirectory;
    private string heatmapDirectory;
    private string resultsDirectory;
    private string currentFileName;
    private LogAnalyzer logAnalyzer;
    private void Awake()
    {
        logAnalyzer = GetComponent<LogAnalyzer>();
        System.Array.Reverse(colorSet);
    }
    private void Start()
    {
        currentDirectory = Directory.GetCurrentDirectory();
        heatmapDirectory = currentDirectory + "\\Heatmaps";
        resultsDirectory = currentDirectory + "\\Results";
        currentFileName = "heatmap";

        if (!Directory.Exists(heatmapDirectory))
            Directory.CreateDirectory(heatmapDirectory);

        if (!Directory.Exists(resultsDirectory))
            Directory.CreateDirectory(resultsDirectory);

        CreateGraphAxis(defaultAxisLength, defaultStepValue);
        EnableSaveResultButton(false);
        VersionText.text = Application.version;
    }
    public void CreateGraphAxis(int _axisLength, int _stepValue)
    {
        axisLength = _axisLength;
        stepValue = _stepValue;

        Vector3[] graphPositions = new Vector3[3] { graphMaxVert, graphOrigin, graphMaxHor };
        lineRenderer.positionCount = 3;
        lineRenderer.SetPositions(graphPositions);

        float graphSpacingX = 2;
        float graphSpacingY = 2;

        int col = -axisLength;
        int iterator = 0;

        foreach(Transform child in LabelsGo.transform)
        {
            if (child.name != $"{axisLength}-{stepValue}")
                child.gameObject.SetActive(false);
        }

        bool exist = false;
        GameObject labelGroup;
        try
        {
            labelGroup = LabelsGo.transform.Find($"{axisLength}-{stepValue}").gameObject;
            labelGroup.SetActive(true);
            exist = true;
        }
        catch(Exception e)
        {
            exist = false;
        }


        if (!exist)
        {
            labelGroup = new GameObject($"{axisLength}-{stepValue}");
            labelGroup.transform.SetParent(LabelsGo.transform);
            // create labels
            while (col <= axisLength)
            {
                GameObject labelGO = Instantiate(HorizontalLabelPrefab, labelGroup.transform);
                TextMesh labelText = labelGO.GetComponent<TextMesh>();
                labelText.text = col.ToString();
                labelGO.transform.position = graphOrigin + new Vector3(graphSpacingX * iterator, 0, 0) + new Vector3(1,-0.5f,0);
                iterator += 1;
                col += stepValue;
            }

            int row = -axisLength;
            iterator = 0;

            while (row <= axisLength)
            {
                GameObject labelGO = Instantiate(VerticalLabelPrefab, labelGroup.transform);
                TextMesh labelText = labelGO.GetComponent<TextMesh>();
                labelText.text = row.ToString();
                labelGO.transform.position = graphOrigin + new Vector3(0, graphSpacingY * iterator, 0) + new Vector3(-0.5f, 1, 0);
                iterator += 1;
                row += stepValue;
            }
        }

    }
    public void ImportFile()
    {
        StartCoroutine(Import());
    }
    IEnumerator Import()
    {

        string title = "Open Log/Result File";

        ExtensionFilter extension_csv = new ExtensionFilter("csv", "csv");
        ExtensionFilter[] extensions = new ExtensionFilter[] { extension_csv };

        string[] path = StandaloneFileBrowser.OpenFilePanel(title, heatmapDirectory, extensions, false);

        if (path.Length > 0)
        {
            ShowFeedback("Loading...");

            yield return new WaitForSeconds(0.05f);

            var splits = path[0].Split("\\");
            currentFileName = splits[splits.Length - 1].Split(".")[0];
            pathText.text = path[0];

            if (currentFileName.Contains("Log"))
                logAnalyzer.ProcessLogFile(path[0]);
            else if (currentFileName.Contains("Result"))
                logAnalyzer.ProcressResultFile(path[0]);
            else
                pathText.text = "File Not Valid";
            
            HideFeedback();
        }
        yield return null;
    }
    public void OutputInfo(string info)
    {
        infoText.text = info;
    }
    public void CreateHeatmapFromList(List<ResultEntry> entries)
    {
        foreach(Transform child in UnitsGo.transform)
        {
            child.gameObject.SetActive(false);
        }

        foreach(ResultEntry entry in entries)
        {
            CreateOneUnit(entry.group,
                entry.locationX / (stepValue / 2),
                entry.locationY / (stepValue / 2),
                Mathf.Round(entry.weibull)
            );
        }
    }
    private void CreateHeatmap(string resultContent)
    {
        
        string[] lines = resultContent.Split("\n");

        if (lines.Length > 6)
        {
            for (int l = 5; l < lines.Length; l++)
            {
                string currentLine = lines[l];
                string[] values = currentLine.Split(',');

                if (values.Length == 7)
                {
                    int group = int.Parse(values[0]);
                    float locationX = float.Parse(values[1]);
                    float locationY = float.Parse(values[2]);
                    int reversals = int.Parse(values[3]);
                    float averageReversalContrast = float.Parse(values[4]);
                    float averageReversal = float.Parse(values[5]);
                    float weibull = float.Parse(values[6]);

                }
                else if(values.Length == 8)
                {
                    int group = int.Parse(values[0]);
                    float locationX = float.Parse(values[1]);
                    float locationY = float.Parse(values[2]);
                    int reversals = int.Parse(values[3]);
                    float averageReversalLogContrast = float.Parse(values[4]);
                    float averageReversalContrast = float.Parse(values[5]);
                    float averageReversal = float.Parse(values[6]);
                    float weibull = float.Parse(values[7]);
                }

            }
        }
    }
    private void CreateOneUnit(int group, float locationX, float locationY, float weibull)
    {
        string name = $"{locationX}-{locationY}";
        GameObject unitGO;
        try
        {
            unitGO = UnitsGo.transform.Find(name).gameObject;
            unitGO.SetActive(true);
        }
        catch(Exception e)
        {
            unitGO = Instantiate(GraphUnitPrefab, UnitsGo.transform);
            unitGO.name = name;
        }

        GraphUnit unit = unitGO.GetComponent<GraphUnit>();
        unitGO.transform.position = new Vector3(locationX, locationY, 0f) + new Vector3(9f, 9f, 0f);
        float percentage = weibull / 50f;

        // 0 - 1  0.0 to 0.125
        // 1 - 2  0.125 to 0.25
        // 2 - 3  0.25 to 0.375
        // 3 - 4  0.375 to 0.5
        // 4 - 5  0.5 to 0.625
        // 5 - 6  0.625 to 0.75
        // 6 - 7  0.75 to 0.875
        // 7 - 8  0.875 to 1.0

        Debug.Log(percentage);

        float stepValue = 1f / (colorSet.Length - 1); // 0.125f

        int groupEnd = Mathf.RoundToInt(percentage / stepValue);
        int groupStart = groupEnd - 1;

        Color minColor = colorSet[groupStart];
        Color maxColor = colorSet[groupEnd];

        float lerpStep = (percentage - (stepValue * groupEnd))/ stepValue;

        Color unitColor = Color.Lerp(minColor, maxColor, lerpStep);

        unit.SetText(weibull);
        unit.SetColor(unitColor);
    }

    public void SaveResult()
    {
        ExtensionFilter extension_csv = new ExtensionFilter("csv", "csv");
        ExtensionFilter[] extensions = new ExtensionFilter[] { extension_csv };
        string resultFileName = currentFileName.Replace("_Log", "_Results");
        string resultPath = StandaloneFileBrowser.SaveFilePanel("Save Result File", resultsDirectory, resultFileName, extensions);
        if(!string.IsNullOrEmpty(resultPath))
        {
            StreamWriter writer = new StreamWriter(resultPath);
            foreach (string header in logAnalyzer.resultHeader)
                writer.WriteLine(header);
            foreach (ResultEntry entry in logAnalyzer.resultEntries)
                writer.WriteLine( entry.StringifyEntry());
            writer.Close();
            ShowFeedback("Saved Result", true);
        }
    }

    public void EnableSaveResultButton(bool en)
    {
        SaveResultButton.interactable = en;
    }
    public void SaveGraph ()
    {

        ExtensionFilter extension_png = new ExtensionFilter("png", "png");
        ExtensionFilter[] extensions = new ExtensionFilter[] { extension_png };
        string heatMapPath = StandaloneFileBrowser.SaveFilePanel("Save Heatmap File", heatmapDirectory, currentFileName, extensions);
        if (!string.IsNullOrEmpty(heatMapPath))
        {
            StartCoroutine(SaveGraphRoutine(heatMapPath));
        }

    }

    IEnumerator SaveGraphRoutine(string heatMapPath)
    {
        canvas.alpha = 0f;
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(heatMapPath);
        bool stopChecking = false;

        while (stopChecking)
        {
            yield return new WaitForSeconds(0.5f);
            stopChecking = Directory.Exists(heatMapPath);
        }
        ShowFeedback("Saved Heatmap", true);
        canvas.alpha = 1f;
    }

    public void ShowFeedback(string content, bool autoHide = false)
    {
        Debug.Log("show feedback");
        FeedbackText.enabled = true;
        FeedbackText.text = content;
        if (autoHide)
            StartCoroutine(AutoHideFeedback());
    }
    public void HideFeedback ()
    {
        FeedbackText.enabled = false;
    }

    IEnumerator AutoHideFeedback()
    {
        yield return new WaitForSeconds(2f);
        HideFeedback();
    }
}