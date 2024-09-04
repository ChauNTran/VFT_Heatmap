using System;
using System.Collections.Generic;
using UnityEngine;

namespace MakeMeSmall.Demo2_FileConverter
{
    using DownscaleFunc = System.Action<MakeMeSmall.Image, MakeMeSmall.Image, MakeMeSmall.Methods.Parameters>;

    public class Demo2_Main : MonoBehaviour
    {
        static Demo2_Main Instance { get; set; }
        private Vector3 imageInitialLocalScale = new Vector3(250f, 250f, 1.0f);
        public GameObject FilterGroup;
        public GameObject InputImage;
        public GameObject OutputImage;
        public GameObject PercentageInputField;
        public GameObject OuputImageLabel;
        string currentMethodName = "";
        Dictionary<string, Sprite> method2sprite = new Dictionary<string, Sprite>();
        Sprite inputSprite = null;
        double downscaleScale = 1.0 / 8;

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

        // Each key must be same as the object name of the corresponding toggle object in FilterGroup
        Dictionary<string, DownscaleFuncParam> method2func = new Dictionary<string, DownscaleFuncParam>()
        {
            { "subsample", new DownscaleFuncParam(MakeMeSmall.Downscale.Subsample, new MakeMeSmall.Methods.Parameters()) },
            { "box", new DownscaleFuncParam(MakeMeSmall.Downscale.Box, new MakeMeSmall.Methods.Parameters()) },
            { "gaussian",new DownscaleFuncParam( MakeMeSmall.Downscale.Gaussian, new MakeMeSmall.Methods.Parameters())},
            { "bilinear",new DownscaleFuncParam( MakeMeSmall.Downscale.Bilinear, new MakeMeSmall.Methods.Parameters())},
            { "bicubic", new DownscaleFuncParam(MakeMeSmall.Downscale.Bicubic, new MakeMeSmall.Methods.Parameters())},
            { "content-adaptive", new DownscaleFuncParam(MakeMeSmall.Downscale.ContentAdaptive, new MakeMeSmall.Methods.ContentAdaptiveParameters())},
            { "perceptual", new DownscaleFuncParam(MakeMeSmall.Downscale.Perceptual , new MakeMeSmall.Methods.PerceptualParameters())},
        };

        void Start()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            storeLocalScale(ref imageInitialLocalScale);
            if (PercentageInputField)
            {
                var inputField = PercentageInputField.GetComponent<UnityEngine.UI.InputField>();
                if (inputField)
                {
                    string text = inputField.text;
                    inputField.text = "" + (downscaleScale * 100);
                }
            }
            adjustImageAspectRatio(InputImage);
            applyDownscaleScale(downscaleScale);
        }

        void Update()
        {
        }

        // Load png and update InputImage and OuputImage
        public void OnLoad()
        {
            string filePath = UnityEditor.EditorUtility.OpenFilePanel("Load Texture", "Assets/MakeMeSmall/Demo/Resources/png", "png");
            if (System.IO.File.Exists(filePath))
            {
                // clear downscaled images
                foreach (var sprite in method2sprite.Values)
                {
                    if (sprite != null)
                    {
                        Destroy(sprite.texture);
                    }
                    Destroy(sprite);
                }
                method2sprite.Clear();

                // Load
                if (inputSprite != null)
                {
                    Destroy(inputSprite.texture);
                }
                Destroy(inputSprite);

                // Set input image
                var inputTex = PngFile.Load(filePath);
                inputSprite = Sprite.Create(inputTex, new Rect(0, 0, inputTex.width, inputTex.height), new Vector2(0.5f, 0.5f));
                replaceSprite(InputImage, inputSprite);
                adjustImageAspectRatio(InputImage);

                // Set output image (execute donwscale)
                applyDownscaleMethod(currentMethodName);
            }
        }
        
        // Save OutputImage as png
        public void OnSave()
        {
            if (method2sprite.ContainsKey(currentMethodName))
            {
                var tex = method2sprite[currentMethodName].texture;
                if (tex != null)
                {
                    var now = DateTime.Now;
                    string filename = string.Format(
                            "result_{0}-{1}{2}{3}{4}{5}{6}.png"
                            , currentMethodName
                            , now.Year
                            , now.Month
                            , now.Day
                            , now.Hour
                            , now.Minute
                            , now.Second);

                    string filepath = UnityEditor.EditorUtility.SaveFilePanel("Save Texture", "Assets/", filename, "png");
                    if (filepath.Length > 0)
                    {
                        PngFile.Save(tex, filepath);
                    }
                }
            }
        }

        // Change the downscaling size
        public void OnDownscaleScaleChanged()
        {
            if (PercentageInputField)
            {
                var inputField = PercentageInputField.GetComponent<UnityEngine.UI.InputField>();
                if (inputField)
                {
                    string text = inputField.text;
                    Debug.Log("text = " + text);
                    double percentage;
                    if (double.TryParse(text, out percentage))
                    {
                        double prevScale = downscaleScale;
                        double newScale = percentage * 0.01;
                        newScale = Math.Max(1e-2, newScale);
                        newScale = Math.Min(1 - 1e-2, newScale);
                        if (newScale != prevScale)
                        {
                            method2sprite.Clear();
                            applyDownscaleScale(newScale); // internally changes downscaleScale
                            applyDownscaleMethod(currentMethodName);
                        }
                    }
                    inputField.text = "" + (downscaleScale * 100);
                }
            }
        }

        // allowFilterToggleEvent is used to prevent re-entering. 
        bool allowFilterToggleEvent = false;

        // Change the current downscaling method.
        internal static void OnFilterToggleValueChanged(GameObject toggle, bool newValue)
        {
            if (Instance == null || Instance.allowFilterToggleEvent)
            {
                return;
            }

            if (Instance.allowFilterToggleEvent)
            {
                return;
            }

            Instance.allowFilterToggleEvent = true;
            if (Instance.FilterGroup != null)
            {
                for (int i = 0; i < Instance.FilterGroup.transform.childCount; i++)
                {
                    var child = Instance.FilterGroup.transform.GetChild(i).gameObject;
                    if (toggle == child)
                    {
                        child.GetComponent<UnityEngine.UI.Toggle>().isOn = true;
                        Instance.applyDownscaleMethod(toggle.name);
                    }
                    else
                    {
                        child.GetComponent<UnityEngine.UI.Toggle>().isOn = false;
                    }
                }
            }

            Instance.allowFilterToggleEvent = false;
        }

        // Set and execute the downscaling scale 
        void applyDownscaleScale(double scale)
        {
            downscaleScale = scale;
            var inputSize = getTextureSize(InputImage);
            if (inputSize != Vector3.zero)
            {
                var outputSize = inputSize * (float)downscaleScale;
                OuputImageLabel.GetComponent<UnityEngine.UI.Text>().text = string.Format("Downscaled ({0}x{1}px)", (int)outputSize.x, (int)outputSize.y);
            }
        }

        // Set and execute the downscaling method 
        void applyDownscaleMethod(string methodName)
        {
            if (method2sprite.ContainsKey(methodName))
            {
                replaceSprite(OutputImage, method2sprite[methodName]);
            }
            else
            {
                var sprite = executeDownscale(methodName, (int)(1.0 / downscaleScale));
                method2sprite[methodName] = sprite;
                replaceSprite(OutputImage, sprite);
            }
            adjustImageAspectRatio(OutputImage);
            currentMethodName = methodName;
        }

        // Cache to prevent downscale() from doing unneccessary memory allocation
        Color[] outPixels = new Color[0];

        Sprite executeDownscale(string methodName, int invScaleRatio)
        {
            if (method2func.ContainsKey(methodName))
            {
                int ratio = invScaleRatio;

                // get input pixels
                var inputSprite = InputImage.GetComponent<UnityEngine.UI.Image>().sprite;
                Texture2D inputTexture = inputSprite.texture;
                Color[] pixels = inputTexture.GetPixels();

                // setup ouput pixels
                if (outPixels.Length != pixels.Length)
                {
                    outPixels = new Color[pixels.Length / ratio];
                }

                // downscale
                int iw = inputTexture.width;
                int ih = inputTexture.height;
                int ow = inputTexture.width / ratio;
                int oh = inputTexture.height / ratio;
                var inputMMSImage = new MakeMeSmall.Image(pixels, iw, ih);
                var outputMMSImage = new MakeMeSmall.Image(outPixels, ow, oh);
                var funcparam = method2func[methodName];
                var done = setupDownscaleParameters(funcparam.Parameters, methodName);
                try
                {
                    funcparam.Function(inputMMSImage, outputMMSImage, funcparam.Parameters);
                }
                finally
                {
                    if (done != null)
                    {
                        done();
                    }
                }

                // generate texture2d
                var outTexture = new Texture2D(inputTexture.width / ratio, inputTexture.height / ratio, TextureFormat.RGBA32, false);
                outTexture.filterMode = FilterMode.Point;
                outTexture.SetPixels(outPixels);
                outTexture.Apply();

                // generate sprite
                var rect = inputSprite.rect;
                rect.width = (int)(rect.width / ratio);
                rect.height = (int)(rect.height / ratio);
                var pivotX = inputSprite.pivot.x / inputSprite.rect.width;
                var pivotY = inputSprite.pivot.y / inputSprite.rect.height;
                var sprite = Sprite.Create(outTexture, rect, new Vector2(pivotX, pivotY), inputSprite.pixelsPerUnit);

                return sprite;
            }

            Debug.Log("No downscale function was matched with \"" + methodName + "\"");
            return null;
        }

        /// <returns>done: Procedure that should be run just after downscaling</returns>
        Action setupDownscaleParameters(MakeMeSmall.Methods.Parameters param, string methodName)
        {
            param.ProgressBar = new ProgressBar.ProgressBar("Downscaling (" + methodName + ") ...");
            Action done = () => { param.ProgressBar.Dispose(); };
            return done;
        }

        // Adjust size of imageObj so that it just fits in imageInitialLocalScale
        void adjustImageAspectRatio(GameObject imageObj)
        {
            if (imageObj == null)
            {
                return;
            }
            var image = imageObj.GetComponent<UnityEngine.UI.Image>();
            if (image != null && image.sprite != null && image.sprite.texture != null)
            {
                int tx = image.sprite.texture.width;
                int ty = image.sprite.texture.height;
                var rectTrans = imageObj.GetComponent<RectTransform>();
                if (rectTrans != null && tx >= 1 && ty >= 1)
                {
                    var scale = imageInitialLocalScale;
                    if (tx < ty)
                    {
                        scale.x = scale.y * tx / ty;
                    }
                    else
                    {
                        scale.y = scale.x * ty / tx;
                    }
                    rectTrans.localScale = scale;
                }
            }
        }


        //-----------------------------------------------------------------------
        //
        // Utilities
        //
        //-----------------------------------------------------------------------

        // Save the local scale of InputImage or OutputImage.
        // This scale is used to draw loaded/downscaled images in display
        void storeLocalScale(ref Vector3 localScale)
        {
            RectTransform rectTrans = null;
            if (InputImage != null)
            {
                rectTrans = InputImage.GetComponent<RectTransform>();
            }
            if (rectTrans == null)
            {
                if (OutputImage != null)
                {
                    rectTrans = OutputImage.GetComponent<RectTransform>();
                }
            }
            if (rectTrans != null)
            {
                localScale = rectTrans.localScale;
            }
        }

        // Replace the sprite of imageObj with sprite
        void replaceSprite(GameObject imageObj, Sprite sprite)
        {
            if (imageObj == null)
            {
                return;
            }
            var image = imageObj.GetComponent<UnityEngine.UI.Image>();
            if (image == null)
            {
                return;
            }
            image.sprite = sprite;
        }

        Vector3 getTextureSize(GameObject imageObj)
        {
            if (imageObj == null)
            {
                return Vector3.zero;
            }
            var image = imageObj.GetComponent<UnityEngine.UI.Image>();
            if (image == null)
            {
                return Vector3.zero;
            }
            if (image.sprite == null || image.sprite.texture == null)
            {
                return Vector3.zero;
            }
            return new Vector3(image.sprite.texture.width, image.sprite.texture.height);
        }
    }
}