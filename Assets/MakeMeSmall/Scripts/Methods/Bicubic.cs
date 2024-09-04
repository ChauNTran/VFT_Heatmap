using UnityEngine;
using System.Collections;
using System;

namespace MakeMeSmall.Methods
{
    public class BicubicParameters : Parameters
    {
        public double a { get; set; }
        BicubicParameters(double a = 1.0)
        {
            this.a = a;
        }
    }

    public class Bicubic : IMethod
    {
        public void Downscale(Image input, Image output, Parameters param)
        {
            if (input == null || output == null)
            {
                return;
            }

            double a = 1.0;
            if (param is BicubicParameters)
            {
                a = (param as BicubicParameters).a;
            }

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
                    for (int jy = y - 5; jy <= y + 5; jy++)
                    {
                        for (int jx = x - 5; jx <= x + 5; jx++)
                        {
                            var w = weightFunc(Math.Abs(wfx - jx), a) * weightFunc(Math.Abs(wfy - jy), a);
                            if (w == 0) continue;
                            var sx = (jx >= sw - 1) ? x : jx;
                            var sy = (jy >= sh - 1) ? y : jy;
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

        double weightFunc(double d, double a)
        {
            if (d <= 1.0)
            {
                return ((a + 2.0) * d * d * d) - ((a + 3.0) * d * d) + 1;
            }
            else if (d <= 2.0)
            {
                return (a * d * d * d) - (5.0 * a * d * d) + (8.0 * a * d) - (4.0 * a);
            }
            return 0.0;
        }

    }
}