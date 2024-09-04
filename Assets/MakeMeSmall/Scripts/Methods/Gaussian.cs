using UnityEngine;
using System.Linq;
using System;
using MakeMeSmall.DataTypes;

namespace MakeMeSmall.Methods
{
    public class Gaussian5x5 : IMethod
    {
        public void Downscale(Image input, Image output, Parameters param)
        {
            if (input == null || output == null)
            {
                return;
            }

            var kData = new double[]
            {
                1, 4, 6, 4, 1,
                4, 16, 24, 16, 4,
                6,24,36,24,6,
                4, 16, 24, 16, 4,
                1, 4, 6, 4, 1,
            }.Select(v => v / 256.0).ToArray();
            Kernel k = new Kernel(kData, 5, 5);
            Kernel.Filter(input, output, k);
        }
    }
}