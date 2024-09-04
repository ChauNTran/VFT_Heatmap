using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MakeMeSmall.Demo1_Simple
{
    public class Demo1_Main : MonoBehaviour
    {
        public GameObject InputImage;
        public GameObject OutputImage;
        public Vector3 DownscaledSize = new Vector3(64, 64, 0);

        void Start()
        {
            var inputSprite = InputImage.GetComponent<UnityEngine.UI.Image>().sprite;
        }

        private void OnGUI()
        {
            int x = 10;
            int y = 10;
            const int buttonHeight = 30;
            const int buttonStepY = 40;

            Texture2D inputTexture = extractTexture2D(InputImage);
            Texture2D outputTexture = null;

            if (GUI.Button(new Rect(x, y, 150, buttonHeight), "Subsample"))
            {
                outputTexture = MakeMeSmall.Downscale.Subsample(inputTexture, DownscaledSize);
            }
            y += buttonStepY;
            if (GUI.Button(new Rect(x, y, 150, buttonHeight), "Box"))
            {
                outputTexture = MakeMeSmall.Downscale.Box(inputTexture, DownscaledSize);
            }
            y += buttonStepY;
            if (GUI.Button(new Rect(x, y, 150, buttonHeight), "Bilinear"))
            {
                outputTexture = MakeMeSmall.Downscale.Bilinear(inputTexture, DownscaledSize);
            }
            y += buttonStepY;
            if (GUI.Button(new Rect(x, y, 150, buttonHeight), "Bicubic"))
            {
                outputTexture = MakeMeSmall.Downscale.Bicubic(inputTexture, DownscaledSize);
            }
            y += buttonStepY;
            if (GUI.Button(new Rect(x, y, 150, buttonHeight), "Gaussian"))
            {
                outputTexture = MakeMeSmall.Downscale.Gaussian(inputTexture, DownscaledSize);
            }
            y += buttonStepY;
            if (GUI.Button(new Rect(x, y, 150, buttonHeight), "Perceptual"))
            {
                // Show progress bar
                var param = new MakeMeSmall.Methods.PerceptualParameters();
                param.ProgressBar = new ProgressBar.ProgressBar("Downscaling...");

                // Run with progress bar
                outputTexture = MakeMeSmall.Downscale.Perceptual(inputTexture, DownscaledSize, param);

                // Hide progress bar
                param.ProgressBar.Dispose();
            }
            y += buttonStepY;
            if (GUI.Button(new Rect(x, y, 150, buttonHeight), "ContentApdative"))
            {
                // Show progress bar
                var param = new MakeMeSmall.Methods.ContentAdaptiveParameters();
                param.ProgressBar = new ProgressBar.ProgressBar("Downscaling...");

                // Run with progress bar
                outputTexture = MakeMeSmall.Downscale.ContentAdaptive(inputTexture, DownscaledSize, param);

                // Hide progress bar
                param.ProgressBar.Dispose();
            }

            if (outputTexture)
            {
                replaceTexture(OutputImage, outputTexture);
            }
        }


        // GameObject -> Texture2D
        Texture2D extractTexture2D(GameObject imageObj)
        {
            if (imageObj == null)
            {
                return null;
            }
            var image = imageObj.GetComponent<UnityEngine.UI.Image>();
            if (image == null)
            {
                return null;
            }
            var sprite = InputImage.GetComponent<UnityEngine.UI.Image>().sprite;
            if (sprite == null)
            {
                return null;
            }
            return sprite.texture;
        }

        // Texture2D -> GameObject
        void replaceTexture(GameObject imageObj, Texture2D texture)
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
            var sprite = image.sprite;
            if (sprite == null)
            {
                return;
            }
            // Create and set a new sprite.
            var pivotX = sprite.pivot.x / sprite.rect.width;
            var pivotY = sprite.pivot.y / sprite.rect.height;
            var newRect = sprite.rect;
            newRect.width = texture.width;
            newRect.height = texture.height;
            var newSprite = Sprite.Create(texture, newRect, new Vector2(pivotX, pivotY), sprite.pixelsPerUnit);
            image.sprite = newSprite;
        }

    }
}