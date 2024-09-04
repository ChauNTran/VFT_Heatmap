using UnityEngine;
using System.Collections;
using System;

namespace MakeMeSmall.DataTypes
{
    class Kernel
    {
        public double[] Data { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Kernel(double[] data, int w, int h)
        {
            Data = data;
            Width = w;
            Height = h;
            System.Diagnostics.Debug.Assert(Data.Length == Width * Height);
        }

        public static Image Filter(Image input, Image output, Kernel k)
        {
            float rx = (float)input.width / output.width;
            float ry = (float)input.height / output.height;
            for (int oy = 0; oy < output.height; oy++)
            {
                for (int ox = 0; ox < output.width; ox++)
                {
                    int cx = (int)((0.5 + ox) * rx);
                    int cy = (int)((0.5 + oy) * ry);
                    int o_idx = ox + oy * output.width;
                    double r = 0, g = 0, b = 0;
                    for (int ky = 0; ky < k.Height; ky++)
                    {
                        int py = cy - k.Height / 2 + ky;
                        for (int kx = 0; kx < k.Width; kx++)
                        {
                            int px = cx - k.Width / 2 + kx;
                            int i_idx = px + py * input.width;
                            if (0 <= px && px < input.width && 0 <= py && py < input.height)
                            {
                                double w = k.Data[kx + ky * k.Width];
                                b += w * input.pixels[i_idx].b;
                                g += w * input.pixels[i_idx].g;
                                r += w * input.pixels[i_idx].r;
                            }
                        }
                    }
                    output.pixels[o_idx].b = (float)b;
                    output.pixels[o_idx].g = (float)g;
                    output.pixels[o_idx].r = (float)r;
                    output.pixels[o_idx].a = (float)1;
                }
            }
            return output;
        }
    }
}