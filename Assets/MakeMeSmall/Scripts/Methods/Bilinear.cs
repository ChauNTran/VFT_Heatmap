using UnityEngine;
using System.Collections;
using System;

namespace MakeMeSmall.Methods
{
    public class Bilinear : IMethod
    {
        public void Downscale(Image input, Image output, Parameters param)
        {
            if (input == null || output == null)
            {
                return;
            }

            // const double a = 1.0;

            int sw = input.width, sh = input.height;
            double wf = (double)sw / output.width;
            double hf = (double)sh / output.height;

            for (var oy = 0; oy < output.height; oy++)
            {
                for (var ox = 0; ox < output.width; ox++)
                {
                    double wfx = wf * ox;
                    double wfy = hf * oy;
                    int x = (int)wfx;
                    int y = (int)wfy;
                    double r = 0.0;
                    double g = 0.0;
                    double b = 0.0;
                    for (int jy = y; jy <= y + 1; jy++)
                    {
                        for (int jx = x; jx <= x + 1; jx++)
                        {
                            var w = (1 - Math.Abs(wfx - jx)) * (1 - Math.Abs(wfy - jy));
                            if (w == 0) continue;
                            var sx = (jx >= sw - 1) ? x : jx;
                            var sy = (jy >= sh - 1) ? y : jy;
                            var sc = input.pixels[sx + sy * input.width];
                            r += input.pixels[sx + sy * input.width].r * w;
                            g += input.pixels[sx + sy * input.width].g * w;
                            b += input.pixels[sx + sy * input.width].b * w;
                        }
                    }
                    output.pixels[ox + oy * output.width].r = (float)r;
                    output.pixels[ox + oy * output.width].g = (float)g;
                    output.pixels[ox + oy * output.width].b = (float)b;
                    output.pixels[ox + oy * output.width].a = (float)1;
                }
            }
        }

    }
}