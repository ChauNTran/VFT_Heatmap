using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using MakeMeSmall.DataTypes;

namespace MakeMeSmall.Methods
{
    public class Box : IMethod
    {
        public void Downscale(Image input, Image output, Parameters param)
        {
            if (input == null || output == null)
            {
                return;
            }

            int ksizex = 11;
            int ksizey = 11;
            var kData = Enumerable.Range(0, ksizex * ksizey).Select(_ => 1.0 / (ksizex * ksizey)).ToArray();
            Kernel k = new Kernel(kData, ksizex, ksizey);
            Kernel.Filter(input, output, k);
        }
    }
}