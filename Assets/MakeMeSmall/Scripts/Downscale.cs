using UnityEngine;
using System.Collections;
using System;

using DownscaleFunc = System.Action<MakeMeSmall.Image, MakeMeSmall.Image, MakeMeSmall.Methods.Parameters>;
namespace MakeMeSmall
{
    public class Downscale
    {
        private static Texture2D downscaleTexture(DownscaleFunc downscaleFunc, Texture2D inputTexture, Vector3 newSize, MakeMeSmall.Methods.Parameters param = null)
        {
            if (downscaleFunc == null)
            {
                Debug.LogWarningFormat("downscaleFunc is null");
                return null;
            }
            if (inputTexture == null)
            {
                Debug.LogWarningFormat("inputTexture is null");
                return null;
            }
            int newWidth = (int)newSize.x;
            int newHeight = (int)newSize.y;
            if (newWidth <= 0 || inputTexture.width <= newWidth)
            {
                Debug.LogWarningFormat("Width of downscaled size ({0}) must be > 0 and < {1}", newSize.x, inputTexture.width);
                return null;
            }
            if (newWidth <= 0 || inputTexture.width <= newWidth)
            {
                Debug.LogWarningFormat("Height of downscaled size ({0}) must be > 0 and < {1}", newSize.y, inputTexture.height);
                return null;
            }
            var input = Image.FromTexture2D(inputTexture);
            var output = Image.CreateEmpty(newWidth, newHeight);
            downscaleFunc(input, output, param); // Execute downscaling function
            var outTexture = new Texture2D(newWidth, newHeight, TextureFormat.RGBA32, false);
            outTexture.filterMode = FilterMode.Point;
            outTexture.SetPixels(output.pixels);
            outTexture.Apply();
            return outTexture;
        }

        public static void Subsample(Image input, Image output, MakeMeSmall.Methods.Parameters param = null)
        {
            try
            {
                new Methods.Subsample().Downscale(input, output, param);
            }
            catch (ProgressBar.ProgressBarCancelException)
            {
                Debug.LogWarning("Canceled Subsample");
            }
        }

        public static Texture2D Subsample(Texture2D inputTexture, Vector3 newSize, MakeMeSmall.Methods.Parameters param = null)
        {
            return downscaleTexture(Subsample, inputTexture, newSize, param);
        }

        public static void Box(Image input, Image output, MakeMeSmall.Methods.Parameters param = null)
        {
            try
            {
                new Methods.Box().Downscale(input, output, param);
            }
            catch (ProgressBar.ProgressBarCancelException)
            {
                Debug.LogWarning("Canceled Box");
            }

        }

        public static Texture2D Box(Texture2D inputTexture, Vector3 newSize, MakeMeSmall.Methods.Parameters param = null)
        {
            return downscaleTexture(Box, inputTexture, newSize, param);
        }

        public static void Gaussian(Image input, Image output, MakeMeSmall.Methods.Parameters param = null)
        {
            try
            {
                new Methods.Gaussian5x5().Downscale(input, output, param);
            }
            catch (ProgressBar.ProgressBarCancelException)
            {
                Debug.LogWarning("Canceled Gaussian");
            }

        }

        public static Texture2D Gaussian(Texture2D inputTexture, Vector3 newSize, MakeMeSmall.Methods.Parameters param = null)
        {
            return downscaleTexture(Gaussian, inputTexture, newSize, param);
        }

        public static void Bilinear(Image input, Image output, MakeMeSmall.Methods.Parameters param = null)
        {
            try
            {
                new Methods.Bilinear().Downscale(input, output, param);
            }
            catch (ProgressBar.ProgressBarCancelException)
            {
                Debug.LogWarning("Canceled Bilinear");
            }

        }

        public static Texture2D Bilinear(Texture2D inputTexture, Vector3 newSize, MakeMeSmall.Methods.Parameters param = null)
        {
            return downscaleTexture(Bilinear, inputTexture, newSize, param);
        }

        public static void Bicubic(Image input, Image output, MakeMeSmall.Methods.Parameters param = null)
        {
            try
            {
                new Methods.Bicubic().Downscale(input, output, param);
            }
            catch (ProgressBar.ProgressBarCancelException)
            {
                Debug.LogWarning("Canceled Bicubic");
            }

        }

        public static Texture2D Bicubic(Texture2D inputTexture, Vector3 newSize, MakeMeSmall.Methods.Parameters param = null)
        {
            return downscaleTexture(Bicubic, inputTexture, newSize, param);
        }

        public static void Perceptual(Image input, Image output, MakeMeSmall.Methods.Parameters param = null)
        {
            try
            {
                new Methods.Perceptual().Downscale(input, output, param);
            }
            catch (ProgressBar.ProgressBarCancelException)
            {
                Debug.LogWarning("Canceled Perceptual");
            }

        }

        public static Texture2D Perceptual(Texture2D inputTexture, Vector3 newSize, MakeMeSmall.Methods.Parameters param = null)
        {
            return downscaleTexture(Perceptual, inputTexture, newSize, param);
        }

        public static void ContentAdaptive(Image input, Image output, MakeMeSmall.Methods.Parameters param = null)
        {
            try
            {
                new Methods.ContentAdaptive().Downscale(input, output, param);
            }
            catch (ProgressBar.ProgressBarCancelException)
            {
                Debug.LogWarning("Canceled ContentAdaptive");
            }
        }

        public static Texture2D ContentAdaptive(Texture2D inputTexture, Vector3 newSize, MakeMeSmall.Methods.Parameters param = null)
        {
            return downscaleTexture(ContentAdaptive, inputTexture, newSize, param);
        }
    }
}