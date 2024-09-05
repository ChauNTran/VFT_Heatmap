using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SFB;
using System.IO;

[Serializable]
public class LowResPixel
{
    public Tuple<int, int> cord;
    public bool filled;
    public Color pixelColor;
    public LowResPixel(Tuple<int, int> _cord, bool _filled, Color _pixelColor)
    {
        cord = _cord;
        filled = _filled;
        pixelColor = _pixelColor;
    }
}


public class Graph : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private LineRenderer verticalAxis;
    [SerializeField] private LineRenderer horizontalAixs;
    [SerializeField] private GameObject GraphUnitPrefab;
    [SerializeField] private GameObject HorizontalLabelPrefab;
    [SerializeField] private GameObject VerticalLabelPrefab;
    [SerializeField] private GameObject LabelsGo;
    [SerializeField] private GameObject UnitsGo;
    [SerializeField] private Material heatmapMaterial;
    [Header("UI References")]
    [SerializeField] private Button SaveScreenshotButton;
    [SerializeField] private Button SaveResultButton;
    [SerializeField] private TMP_Text pathText;
    [SerializeField] private TMP_Text infoText;
    [SerializeField] private CanvasGroup canvas;
    [SerializeField] private TMP_Text FeedbackText;
    [SerializeField] private TMP_Text VersionText;
    [Header("Graph Creation Settings")]
    [SerializeField] private Color[] colorSet;

    private Vector3 graphOrigin = new Vector3(0, 0, 0);
    private Vector3 graphMaxHor = new Vector3(20, 0, 0);
    private Vector3 graphMaxVert = new Vector3(0, 20, 0);

    private int defaultAxisHalfLength = 9;
    private int defaultStepValue = 2;
    private int axisHalfLength;
    private int stepValue;
    private int unitCountPerAxis;

    private string currentDirectory;
    private string heatmapDirectory;
    private string resultsDirectory;
    private string currentFileName;
    private LogAnalyzer logAnalyzer;
    private Texture2D heightMap;
    private int textureScale = 10;
    public List<LowResPixel> lowresPixels;
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

        CreateGraphAxis(defaultAxisHalfLength, defaultStepValue);
        EnableSaveResultButton(false);

        VersionText.text = Application.version;

    }
    public void CreateGraphAxis(int _axisHalfLength, int _stepValue)
    {
        axisHalfLength = _axisHalfLength;
        stepValue = _stepValue;
        unitCountPerAxis = (axisHalfLength * 2 / _stepValue) + 3; // 10 ideally
        Vector3[] graphPositions = new Vector3[3] { graphMaxVert, graphOrigin, graphMaxHor };
        lineRenderer.positionCount = 3;
        lineRenderer.SetPositions(graphPositions);

        horizontalAixs.positionCount = 2;
        verticalAxis.positionCount = 2;


        float graphSpacingX = 2;
        float graphSpacingY = 2;

        int col = -axisHalfLength;
        int iterator = 0;

        foreach (Transform child in LabelsGo.transform)
        {
            if (child.name != $"{axisHalfLength}-{stepValue}")
                child.gameObject.SetActive(false);
        }

        bool exist = false;
        GameObject labelGroup;
        try
        {
            labelGroup = LabelsGo.transform.Find($"{axisHalfLength}-{stepValue}").gameObject;
            labelGroup.SetActive(true);
            exist = true;
        }
        catch (Exception e)
        {
            exist = false;
        }


        if (!exist)
        {
            labelGroup = new GameObject($"{axisHalfLength}-{stepValue}");
            labelGroup.transform.SetParent(LabelsGo.transform);
            // create labels
            while (col <= axisHalfLength)
            {
                GameObject labelGO = Instantiate(HorizontalLabelPrefab, labelGroup.transform);
                TextMesh labelText = labelGO.GetComponent<TextMesh>();
                labelText.text = col.ToString();
                labelGO.transform.position = graphOrigin + new Vector3(graphSpacingX * iterator, 0, 0) + new Vector3(1, -0.5f, 0);
                iterator += 1;
                col += stepValue;
            }

            int row = -axisHalfLength;
            iterator = 0;

            while (row <= axisHalfLength)
            {
                GameObject labelGO = Instantiate(VerticalLabelPrefab, labelGroup.transform);
                TextMesh labelText = labelGO.GetComponent<TextMesh>();
                labelText.text = row.ToString();
                labelGO.transform.position = graphOrigin + new Vector3(0, graphSpacingY * iterator, 0) + new Vector3(-0.5f, 1, 0);
                iterator += 1;
                row += stepValue;
            }
        }

        verticalAxis.SetPositions(new Vector3[2] {
            new Vector3((axisHalfLength * 2 / stepValue) + 1, 0 ,0),
            new Vector3((axisHalfLength * 2 / stepValue) + 1, 20 ,0) });

        horizontalAixs.SetPositions(new Vector3[2] {
            new Vector3(0, (axisHalfLength * 2 / stepValue) + 1 ,0),
            new Vector3(20, (axisHalfLength * 2 / stepValue) + 1 ,0) });

        Debug.Log(axisHalfLength);
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

    // t is a value that goes from 0 to 1 to interpolate in a C1 continuous way across uniformly sampled data points.
    // when t is 0, this will return B.  When t is 1, this will return C.  Inbetween values will return an interpolation
    // between B and C.  A and D are used to calculate slopes at the edges.
    Color CubicHermite(Color A, Color B, Color C, Color D, float t)
    {
        Color a = (A / -2.0f) + (3.0f * B) / 2.0f - (3.0f * C) / 2.0f + D / 2.0f;
        Color b = A - (5.0f * B) / 2.0f + 2.0f * C - D / 2.0f;
        Color c = A / -2.0f + C / 2.0f;
        Color d = B;

        return a * t * t * t + b * t * t + c * t + d;
    }

    float CubicHermite(float A, float B, float C, float D, float t)
    {
        float a = (A / -2.0f) + (3.0f * B) / 2.0f - (3.0f * C) / 2.0f + D / 2.0f;
        float b = A - (5.0f * B) / 2.0f + 2.0f * C - D / 2.0f;
        float c = A / -2.0f + C / 2.0f;
        float d = B;

        return a * t * t * t + b * t * t + c * t + d;
    }

    Color GetPixelClamped(Texture2D image, int x, int y)
    {
        x = Mathf.Clamp(x, 0, image.width - 1);
        y = Mathf.Clamp(y, 0, image.height - 1);
        return image.GetPixel(x, y);
    }

    Color SampleBicubic(Texture2D image, float u, float v)
    {
        // calculate coordinates -> also need to offset by half a pixel to keep image from shifting down and left half a pixel
        float x = (u * image.width) - 0.5f;
        int xint = (int)(x); // 0  to 9
        float xfract = x - Mathf.Floor(x); // 0 to 1.0
        float y = (v * image.height) - 0.5f;
        int yint = (int)(y); // 0  to 9
        float yfract = y - Mathf.Floor(y); // 0 to 1.0


        // 1st row
        Color p00 = GetPixelClamped(image, xint - 1, yint - 1);
        Color p10 = GetPixelClamped(image, xint + 0, yint - 1);
        Color p20 = GetPixelClamped(image, xint + 1, yint - 1);
        Color p30 = GetPixelClamped(image, xint + 2, yint - 1);

        //// 2nd row
        Color p01 = GetPixelClamped(image, xint - 1, yint + 0);
        Color p11 = GetPixelClamped(image, xint + 0, yint + 0);
        Color p21 = GetPixelClamped(image, xint + 1, yint + 0);
        Color p31 = GetPixelClamped(image, xint + 2, yint + 0);

        //// 3rd row
        Color p02 = GetPixelClamped(image, xint - 1, yint + 1);
        Color p12 = GetPixelClamped(image, xint + 0, yint + 1);
        Color p22 = GetPixelClamped(image, xint + 1, yint + 1);
        Color p32 = GetPixelClamped(image, xint + 2, yint + 1);

        //// 4th row
        Color p03 = GetPixelClamped(image, xint - 1, yint + 2);
        Color p13 = GetPixelClamped(image, xint + 0, yint + 2);
        Color p23 = GetPixelClamped(image, xint + 1, yint + 2);
        Color p33 = GetPixelClamped(image, xint + 2, yint + 2);


        // interpolate bi-cubically!
        // Clamp the values since the curve can put the value below 0 or above 255
        Color col0 = CubicHermite(p00, p10, p20, p30, xfract);
        Color col1 = CubicHermite(p01, p11, p21, p31, xfract);
        Color col2 = CubicHermite(p02, p12, p22, p32, xfract);
        Color col3 = CubicHermite(p03, p13, p23, p33, xfract);
        Color ret = CubicHermite(col0, col1, col2, col3, yfract);

        return ret;
    }

    Color SampleNearest(Texture2D image, float u, float v)
    {
        float x = (u * image.width);
        int xint = (int)(x); // 0  to 9
        float y = (v * image.height);
        int yint = (int)(y); // 0  to 9
        Color ret = GetPixelClamped(image, xint, yint );
        return ret;
    }

    public void CreateHeatmapFromList(List<ResultEntry> entries)
    {
        foreach (Transform child in UnitsGo.transform)
        {
            child.gameObject.SetActive(false);
        }

        // create a heightmap
        Texture2D lowResMap = new Texture2D(unitCountPerAxis, unitCountPerAxis);
        lowResMap.wrapMode = TextureWrapMode.Repeat;
        lowResMap.filterMode = FilterMode.Point;

        int newWidth = (int)((float)lowResMap.width * textureScale);
        int newHeight = (int)((float)lowResMap.height * textureScale);
        heightMap = new Texture2D(newWidth, newHeight); // final texture
        heightMap.wrapMode = TextureWrapMode.Repeat;


        //initialize transparent
        lowresPixels = new();
        for (int row = 0; row < unitCountPerAxis; row++)
        {
            for (int col = 0; col < unitCountPerAxis; col++)
            {
                lowResMap.SetPixel(col, row, colorSet[0]);
                lowresPixels.Add(new LowResPixel(new Tuple<int, int>(col, row), false, colorSet[0])); ;
            }
        }

        //create low-res map
        foreach (ResultEntry entry in entries)
        {
            float percentage = entry.weibull / 40f;

            float colorStep = 1f / (colorSet.Length - 1); // 0.125f
            int groupEnd = Mathf.RoundToInt(percentage / colorStep);
            int groupStart = groupEnd - 1;
            Color minColor = colorSet[groupStart];
            Color maxColor = colorSet[groupEnd];
            float lerpStep = (percentage - (colorStep * groupEnd)) / colorStep;
            Color unitColor = Color.Lerp(minColor, maxColor, lerpStep);

            int pixelX = ((entry.locationX + axisHalfLength) / stepValue);
            int pixelY = ((entry.locationY + axisHalfLength) / stepValue);

            lowResMap.SetPixel(pixelX + 1, pixelY + 1, unitColor);
            LowResPixel filledPixel = lowresPixels.Find(x => x.cord.Equals(new Tuple<int, int>(pixelX + 1, pixelY + 1) ) );
            filledPixel.filled = true;
            filledPixel.pixelColor = unitColor;

            CreateOneUnit(entry.group,
                entry.locationX,
                entry.locationY,
                Mathf.Round(entry.weibull)
            );
        }
        lowResMap.Apply();

        //for (int y = 0; y < heightMap.height; y++)
        //{
        //    float v = (float)y / (float)(heightMap.height - 1);// 0 to 1.0;
        //    for (int x = 0; x < heightMap.width; x++)
        //    {
        //        float u = (float)x / (float)(heightMap.width - 1); // 0 to 1.0
        //        heightMap.SetPixel(x, y, SampleBicubic(lowResMap, u, v));
        //    }
        //}

        for (int y = 0; y < lowResMap.height; y++)
        {
            for (int x = 0; x < lowResMap.width; x++)
            {
                LowResPixel pixel = lowresPixels.Find(p => p.cord.Equals(new Tuple<int, int>(x, y)));

                if(pixel.filled)
                {
                    bool topEdge = (y > 0 && !lowresPixels.Find(p => p.cord.Equals(new Tuple<int, int>(x, y - 1))).filled);
                    bool topRight = (y > 0 && !lowresPixels.Find(p => p.cord.Equals(new Tuple<int, int>(x + 1, y - 1))).filled);
                    bool topLeft = (y > 0 && !lowresPixels.Find(p => p.cord.Equals(new Tuple<int, int>(x - 1, y - 1))).filled);

                    bool bottomEdge = (y < lowResMap.height - 1 && !lowresPixels.Find(p => p.cord.Equals(new Tuple<int, int>(x, y + 1))).filled);
                    bool bottomRight = (y > 0 && !lowresPixels.Find(p => p.cord.Equals(new Tuple<int, int>(x + 1, y + 1))).filled);
                    bool bottomLeft = (y > 0 && !lowresPixels.Find(p => p.cord.Equals(new Tuple<int, int>(x - 1, y + 1))).filled);

                    bool leftEdge = (x > 0 && !lowresPixels.Find(p => p.cord.Equals(new Tuple<int, int>(x - 1, y))).filled);
                    bool rightEdge = (x < lowResMap.height - 1 && !lowresPixels.Find(p => p.cord.Equals(new Tuple<int, int>(x + 1, y))).filled);

                    if (topEdge)
                        lowResMap.SetPixel(x, y - 1, pixel.pixelColor);
                    if (topRight)
                        lowResMap.SetPixel(x + 1, y - 1, pixel.pixelColor);
                    if (topLeft)
                        lowResMap.SetPixel(x - 1, y - 1, pixel.pixelColor);

                    if (bottomEdge)
                        lowResMap.SetPixel(x, y + 1, pixel.pixelColor);
                    if (bottomRight)
                        lowResMap.SetPixel(x + 1, y + 1, pixel.pixelColor);
                    if (bottomLeft)
                        lowResMap.SetPixel(x - 1, y + 1, pixel.pixelColor);


                    if (leftEdge)
                        lowResMap.SetPixel(x - 1, y, pixel.pixelColor);
                    if (rightEdge)
                        lowResMap.SetPixel(x + 1, y, pixel.pixelColor);

                }
            }
        }

        lowResMap.Apply();

        float heightMap_y = (float)(heightMap.height - 1);
        float heightMap_x = (float)(heightMap.width - 1);

        for (int y = 0; y < lowResMap.height; y++) // 12 x 12
        {
            for (int x = 0; x < lowResMap.width; x++)
            {
                LowResPixel pixel = lowresPixels.Find(p => p.cord.Equals(new Tuple<int, int>(x,y)));

                if(!pixel.filled)
                {
                    for(int p_y = (y * textureScale); p_y < (y * textureScale) + textureScale; p_y++)
                    {
                        float v = (float)p_y / heightMap_y;// 0 to 1.0;
                        for (int p_x = (x * textureScale); p_x < (x * textureScale) + textureScale; p_x++)
                        {
                            float u = (float)p_x / heightMap_x;
                            heightMap.SetPixel(p_x, p_y, SampleNearest(lowResMap, u, v));
                        }
                    }
                }
                else
                {

                    for (int p_y = (y * textureScale); p_y < (y * textureScale) + textureScale; p_y++)
                    {
                        float v = (float)p_y / heightMap_y;// 0 to 1.0;
                        for (int p_x = (x * textureScale); p_x < (x * textureScale) + textureScale; p_x++)
                        {
                            float u = (float)p_x / heightMap_x;
                            heightMap.SetPixel(p_x, p_y, SampleBicubic(lowResMap, u, v));
                        }
                    }
                }
            }
        }

        heightMap.Apply();
    }
    private void CreateOneUnit(int group, float locationX, float locationY, float weibull)
    {
        float unitPosX = locationX / (stepValue / 2);
        float unitPosY = locationY / (stepValue / 2);

        string name = $"{unitPosX},{unitPosY}";
        GameObject unitGO;
        try
        {
            unitGO = UnitsGo.transform.Find(name).gameObject;
            unitGO.SetActive(true);
        }
        catch (Exception e)
        {
            unitGO = Instantiate(GraphUnitPrefab, UnitsGo.transform);
            unitGO.name = name;
        }

        GraphUnit unit = unitGO.GetComponent<GraphUnit>();
        unitGO.transform.position = new Vector3(unitPosX, unitPosY, 0f) + new Vector3((axisHalfLength * 2 / stepValue), (axisHalfLength * 2 / stepValue), 0f);


        // 0 - 1  0.0 to 0.125
        // 1 - 2  0.125 to 0.25
        // 2 - 3  0.25 to 0.375
        // 3 - 4  0.375 to 0.5
        // 4 - 5  0.5 to 0.625
        // 5 - 6  0.625 to 0.75
        // 6 - 7  0.75 to 0.875
        // 7 - 8  0.875 to 1.0


        float tileSize = 1f / (unitCountPerAxis);

        Vector2 tiling = new Vector2(tileSize, tileSize);
        Vector2 offset = new Vector2(
            -tileSize * (axisHalfLength - locationX + stepValue) / stepValue + (1f - tileSize),
            -tileSize * (axisHalfLength - locationY + stepValue) / stepValue + (1f - tileSize));

        unit.SetText(weibull);
        unit.SetMaterialOffset(tiling, offset, heightMap);
    }

    public void SaveResult()
    {
        ExtensionFilter extension_csv = new ExtensionFilter("csv", "csv");
        ExtensionFilter[] extensions = new ExtensionFilter[] { extension_csv };
        string resultFileName = currentFileName.Replace("_Log", "_Results");
        string resultPath = StandaloneFileBrowser.SaveFilePanel("Save Result File", resultsDirectory, resultFileName, extensions);
        if (!string.IsNullOrEmpty(resultPath))
        {
            StreamWriter writer = new StreamWriter(resultPath);
            foreach (string header in logAnalyzer.resultHeader)
                writer.WriteLine(header);
            foreach (ResultEntry entry in logAnalyzer.resultEntries)
                writer.WriteLine(entry.StringifyEntry());
            writer.Close();
            ShowFeedback("Saved Result", true);
        }
    }

    public void EnableSaveResultButton(bool en)
    {
        SaveResultButton.interactable = en;
    }
    public void EnableScreenshotButton()
    {
        SaveScreenshotButton.interactable = true;
    }
    public void SaveGraph()
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
        FeedbackText.enabled = true;
        FeedbackText.text = content;
        if (autoHide)
            StartCoroutine(AutoHideFeedback());
    }
    public void HideFeedback()
    {
        FeedbackText.enabled = false;
    }

    IEnumerator AutoHideFeedback()
    {
        yield return new WaitForSeconds(2f);
        HideFeedback();
    }
}