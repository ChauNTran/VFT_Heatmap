using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using DownscaleFunc = System.Func<UnityEngine.Texture2D, UnityEngine.Vector3, MakeMeSmall.Methods.Parameters, UnityEngine.Texture2D>;

public class Demo3_Main : MonoBehaviour
{
    class DownscaleFuncParam
    {
        public DownscaleFunc Function { get; private set; }
        public MakeMeSmall.Methods.Parameters Parameters { get; private set; }
        public DownscaleFuncParam(DownscaleFunc func, MakeMeSmall.Methods.Parameters param)
        {
            Function = func;
            Parameters = param;
        }
    }

    Dictionary<string, DownscaleFuncParam> method2func = new Dictionary<string, DownscaleFuncParam>()
    {
        { "subsample", new DownscaleFuncParam(MakeMeSmall.Downscale.Subsample, new MakeMeSmall.Methods.Parameters()) },
        { "box", new DownscaleFuncParam(MakeMeSmall.Downscale.Box, new MakeMeSmall.Methods.Parameters()) },
        { "gaussian",new DownscaleFuncParam( MakeMeSmall.Downscale.Gaussian, new MakeMeSmall.Methods.Parameters())},
        { "bilinear",new DownscaleFuncParam( MakeMeSmall.Downscale.Bilinear, new MakeMeSmall.Methods.Parameters())},
        { "bicubic", new DownscaleFuncParam(MakeMeSmall.Downscale.Bicubic, new MakeMeSmall.Methods.Parameters())},
        { "perceptual", new DownscaleFuncParam(MakeMeSmall.Downscale.Perceptual , new MakeMeSmall.Methods.PerceptualParameters())},
        { "content-adaptive", new DownscaleFuncParam(MakeMeSmall.Downscale.ContentAdaptive, new MakeMeSmall.Methods.ContentAdaptiveParameters())},
    };

    public GameObject InputFolderPathInputFiled;
    public GameObject OutputFolderPathInputFiled;
    public GameObject FilterDropdown;
    public GameObject DownscaleSizeXInputFiled;
    public GameObject DownscaleSizeYInputFiled;
    public GameObject ConvertButtonText;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnConvertButtonClicked()
    {
        string methodName = extractDropdownText(FilterDropdown).ToLower();
        if (method2func.ContainsKey(methodName) == false)
        {
            Debug.LogWarning("Method name ("+ methodName +") must be one of ["  + string.Join(",", method2func.Keys.ToArray()) +  "].");
            return;
        }

        string newSizeXText = extractInputFieldText(DownscaleSizeXInputFiled);
        float newSizeX, newSizeY;

        if (false == float.TryParse(newSizeXText, out newSizeX))
        {
            Debug.LogWarning("Parsing error: The text of new size X (" + newSizeXText + " px) is invalid.");
            return;
        }

        string newSizeYText = extractInputFieldText(DownscaleSizeYInputFiled);
        if (false == float.TryParse(newSizeYText, out newSizeY))
        {
            Debug.LogWarning("Parsing error: The text of new size Y (" + newSizeXText + " px) is invalid.");
            return;
        }
        
        string inputFolderPath = extractInputFieldText(InputFolderPathInputFiled);
        if (false == System.IO.Directory.Exists(inputFolderPath))
        {
            Debug.LogWarning("Not found the input folder path: " + inputFolderPath);
            return;
        }
        
        string outputFolderPath = extractInputFieldText(OutputFolderPathInputFiled);

        try
        {
            var files = System.IO.Directory.GetFiles(inputFolderPath).Where(path => path.ToLower().EndsWith(".png")).ToArray();
            for (int i = 0; i < files.Length; i++)
            {
                string filepath = files[i];
                var funcparam = method2func[methodName];

                // Show cancelable progress bar
                if (UnityEditor.EditorUtility.DisplayCancelableProgressBar("Downscaling " + inputFolderPath, System.IO.Path.GetFileName(filepath), (float)i / files.Length))
                {
                    break;
                }

                // Downscale an image
                var newSize = new Vector3(newSizeX, newSizeY, 0.0f);
                downscalePngFile(funcparam.Function, newSize, funcparam.Parameters, filepath, outputFolderPath);
            }
        }
        finally
        {
            // Hide progress bar
            UnityEditor.EditorUtility.ClearProgressBar();

            // Open outputFolderPath in Explorer
            string fullOutputFolderPath = System.IO.Path.GetFullPath(outputFolderPath);
            System.Diagnostics.Process.Start(fullOutputFolderPath);
        }
    }

    public void OnInputFolderDialogButtonClicked()
    {
        string inputFolderPath = UnityEditor.EditorUtility.OpenFolderPanel("Select input folder", "/MakeMeSmall/Demo/Resources", "");
        if (System.IO.Directory.Exists(inputFolderPath))
        {
            replaceInputFieldText(inputFolderPath, InputFolderPathInputFiled);

            // Set output path if it is not set, 
            string outputPath = extractInputFieldText(OutputFolderPathInputFiled);
            if (string.IsNullOrEmpty(outputPath))
            {
                replaceInputFieldText(inputFolderPath + "_converted", OutputFolderPathInputFiled);
            }

            if (ConvertButtonText)
            {
                var text = ConvertButtonText.GetComponent<UnityEngine.UI.Text>();
                if (text)
                {
                    var pngFileCount = System.IO.Directory.GetFiles(inputFolderPath).Where(path => path.ToLower().EndsWith(".png")).Count();
                    text.text = "Convert " + pngFileCount + " images";
                }
            }
        }
    }


    public void OnOutputFolderDialogButtonClicked()
    {
        string ouptutFolderPath = UnityEditor.EditorUtility.OpenFolderPanel("Select input folder", "/MakeMeSmall/Demo/Resources", "");
        if (System.IO.Directory.Exists(ouptutFolderPath))
        {
            replaceInputFieldText(ouptutFolderPath, OutputFolderPathInputFiled);
        }
    }

    void downscalePngFile(DownscaleFunc downscaleFunc, Vector3 newSize, MakeMeSmall.Methods.Parameters param, string filepath, string outputFolderPath)
    {
        if (false == System.IO.Directory.Exists(outputFolderPath))
        {
            Debug.Log("created folder: " + outputFolderPath);
            System.IO.Directory.CreateDirectory(outputFolderPath);
        }

        Texture2D inputTexture = MakeMeSmall.PngFile.Load(filepath);
        if (inputTexture)
        {
            // downscale
            var downscaledTexture = downscaleFunc(inputTexture, newSize, param);

            // save result image
            string saveFilepath = System.IO.Path.Combine(outputFolderPath, System.IO.Path.GetFileName(filepath));
            MakeMeSmall.PngFile.Save(downscaledTexture, saveFilepath);
            Debug.Log("Saved: " + saveFilepath);

            GameObject.Destroy(inputTexture);
        }
    }

    string extractDropdownText(GameObject inputFiledObj)
    {
        string text = "";
        var dropdown= inputFiledObj.GetComponent<UnityEngine.UI.Dropdown>();
        if (dropdown)
        {
            text = dropdown.captionText.text;
        }
        return text;
    }

    string extractInputFieldText(GameObject inputFiledObj)
    {
        string text = "";
        var inputField = inputFiledObj.GetComponent<UnityEngine.UI.InputField>();
        if (inputField)
        {
            text = inputField.text;
        }
        return text;
    }

    void replaceInputFieldText(string text, GameObject inputFiledObj)
    {
        var inputField = inputFiledObj.GetComponent<UnityEngine.UI.InputField>();
        if (inputField)
        {
            inputField.text = text;
        }
    }
}
