using UnityEngine;
using System.Collections;
using System;
using System.Linq;

namespace MakeMeSmall.Methods
{
    using Size = MakeMeSmall.DataTypes.Size;
    using Kernel = MakeMeSmall.DataTypes.Kernel;

    public class PerceptualParameters : Parameters
    {
    }

    public class Perceptual : IMethod
    {
        public void Downscale(Image input, Image output, Parameters param)
        {
            if (input == null || output == null)
            {
                return;
            }

            ProgressBar.ProgressState state = new ProgressBar.ProgressState(0, 1.0f);

            var sw = System.Diagnostics.Stopwatch.StartNew();

            MakeMeSmall.ProgressBar.ProgressBar.Show(param.ProgressBar,state.SetValue(0.1f));

            Size newSize = new Size(output.width, output.height);
            var H = input;
            Size s = new Size(H.width / newSize.Width, H.height / newSize.Height);
            Size np = new Size(2, 2);
            var L = subSample(convValid(H, P(s), param.ProgressBar, state.CreateSub(0.1f)), newSize);
            MakeMeSmall.ProgressBar.ProgressBar.Show(param.ProgressBar,state.SetValue(0.2f));

            var L2 = subSample(convValid(H * H, P(s), param.ProgressBar, state.CreateSub(0.1f)), newSize);
            MakeMeSmall.ProgressBar.ProgressBar.Show(param.ProgressBar,state.SetValue(0.3f));

            System.Diagnostics.Debug.Assert(L.width == newSize.Width);
            System.Diagnostics.Debug.Assert(L.height == newSize.Height);
            var M = convValid(L, P(np), param.ProgressBar, state.CreateSub(0.1f));
            MakeMeSmall.ProgressBar.ProgressBar.Show(param.ProgressBar,state.SetValue(0.4f));

            var Sl = convValid(L * L, P(np), param.ProgressBar, state.CreateSub(0.1f)) - M * M;
            MakeMeSmall.ProgressBar.ProgressBar.Show(param.ProgressBar,state.SetValue(0.5f));

            var Sh = convValid(L2, P(np), param.ProgressBar, state.CreateSub(0.1f)) - M * M;
            MakeMeSmall.ProgressBar.ProgressBar.Show(param.ProgressBar,state.SetValue(0.6f));

            var R = (Sh / Sl).Sqrt().Clamp01();
            var N = convFull(I(M), P(np), param.ProgressBar, state.CreateSub(0.1f));
            MakeMeSmall.ProgressBar.ProgressBar.Show(param.ProgressBar,state.SetValue(0.7f));

            var T = convFull(R * M, P(np), param.ProgressBar, state.CreateSub(0.1f));
            MakeMeSmall.ProgressBar.ProgressBar.Show(param.ProgressBar,state.SetValue(0.8f));

            M = convFull(M, P(np), param.ProgressBar, state.CreateSub(0.1f));
            MakeMeSmall.ProgressBar.ProgressBar.Show(param.ProgressBar,state.SetValue(0.9f));

            R = convFull(R, P(np), param.ProgressBar, state.CreateSub(0.05f));
            MakeMeSmall.ProgressBar.ProgressBar.Show(param.ProgressBar,state.SetValue(0.95f));

            var D = (M + R * L - T) / N;
            for (int i = 0; i < D.pixels.Length; i++)
            {
                output.pixels[i] = D.pixels[i];
            }

            MakeMeSmall.ProgressBar.ProgressBar.Show(param.ProgressBar,state.SetValue(1.0f));
            Debug.Log(string.Format("Perceptually based downscaling: elapsed = {0} ms", sw.ElapsedMilliseconds));
        }

        Image subSample(Image img1, Size newSize)
        {
            var res = Image.CreateEmpty(newSize.Width, newSize.Height);
            new Subsample().Downscale(img1, res, null);
            return res;
        }

        Image convValid(Image input, Kernel k, ProgressBar.ProgressBar progressBar = null, ProgressBar.ProgressState state = null)
        {
            int iw = input.width;
            int ih = input.height;
            int kw = k.Width;
            int kh = k.Height;
            var kdata = k.Data;
            var img1data = input.pixels;

            Image output = Image.CreateEmpty(iw - (kw - 1), ih - (kh - 1));
            int ow = output.width;
            int oh = output.height;
            var imgdata = output.pixels;
            for (int y = 0; y < oh; y++)
            {
                MakeMeSmall.ProgressBar.ProgressBar.Show(progressBar, state.SetValue((float)y / oh));

                int ooffset = ow * y;
                for (int x = 0; x < ow; x++)
                {
                    double r = 0.0;
                    double g = 0.0;
                    double b = 0.0;
                    for (int ky = 0; ky < kh; ky++)
                    {
                        int koffset = ky * kw;
                        int offset = (y + ky) * iw + x;
                        for (int kx = 0; kx < kw; kx++)
                        {
                            r += kdata[kx + koffset] * img1data[kx + offset].r;
                            g += kdata[kx + koffset] * img1data[kx + offset].g;
                            b += kdata[kx + koffset] * img1data[kx + offset].b;
                        }
                    }
                    imgdata[x + ooffset].r = (float)r;
                    imgdata[x + ooffset].g = (float)g;
                    imgdata[x + ooffset].b = (float)b;
                    imgdata[x + ooffset].a = 1.0f;
                }
            }
            return output;
        }

        Image convFull(Image img1, Kernel k, ProgressBar.ProgressBar progressBar = null, ProgressBar.ProgressState state = null)
        {
            Image img = Image.CreateEmpty(img1.width + (k.Width - 1), img1.height + (k.Height - 1));
            for (int y = 0; y < img.height; y++)
            {
                MakeMeSmall.ProgressBar.ProgressBar.Show(progressBar, state.SetValue((float)y / img.height));
                for (int x = 0; x < img.width; x++)
                {
                    double r = 0.0;
                    double g = 0.0;
                    double b = 0.0;
                    for (int ky = 0; ky < k.Height; ky++)
                    {
                        for (int kx = 0; kx < k.Width; kx++)
                        {
                            int ix = (x + kx - k.Width + 1);
                            int iy = (y + ky - k.Height + 1);
                            if (0 <= ix && ix < img1.width && 0 <= iy && iy < img1.height)
                            {
                                r += k.Data[kx + ky * k.Width] * img1.pixels[ix + iy * img1.width].r;
                                g += k.Data[kx + ky * k.Width] * img1.pixels[ix + iy * img1.width].g;
                                b += k.Data[kx + ky * k.Width] * img1.pixels[ix + iy * img1.width].b;
                            }
                        }
                    }
                    img.pixels[x + img.width * y].r = (float)r;
                    img.pixels[x + img.width * y].g = (float)g;
                    img.pixels[x + img.width * y].b = (float)b;
                    img.pixels[x + img.width * y].a = 1.0f;
                }
            }
            return img;
        }

        Kernel P(Size s)
        {
            // Average filter
            var data = Enumerable.Range(0, s.Width * s.Height).Select(_ => 1.0 / (s.Width * s.Height)).ToArray();
            return new Kernel(data, s.Width, s.Height);
        }

        Image I(Image img1)
        {
            var img = Image.CreateEmpty(img1.width, img1.height);
            for (int i = 0; i < img1.width * img1.height; i++)
            {
                img.pixels[i] = Color.white;
            }
            return img;
        }
    }
}