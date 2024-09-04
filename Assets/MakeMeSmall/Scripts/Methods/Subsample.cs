using UnityEngine;
using System.Collections;
using System;

namespace MakeMeSmall.Methods
{
    public class Subsample : IMethod
    {
        public void Downscale(Image input, Image output, Parameters param = null)
        {
            if (input == null || output == null)
            {
                return;
            }

            double pw = (double)input.width / output.width;
            double ph = (double)input.height / output.height;
            for (int y = 0; y < output.height; y++)
            {
                for (int x = 0; x < output.width; x++)
                {
                    // Sample the center of the patch
                    int sx = (int)((0.5 + x) * pw);
                    int sy = (int)((0.5 + y) * ph);
                    output.pixels[x + output.width * y].a = 1.0f; // input.pixels[sx + input.width * sy].a;
                    output.pixels[x + output.width * y].r = input.pixels[sx + input.width * sy].r;
                    output.pixels[x + output.width * y].g = input.pixels[sx + input.width * sy].g;
                    output.pixels[x + output.width * y].b = input.pixels[sx + input.width * sy].b;
                }
            }
        }
    }
}