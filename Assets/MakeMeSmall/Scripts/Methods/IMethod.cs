using UnityEngine;
using System.Collections;
using System;

namespace MakeMeSmall.Methods
{
    public class Parameters
    {
        public MakeMeSmall.ProgressBar.ProgressBar ProgressBar { get; set;  }
    }

    interface IMethod
    {
        void Downscale(Image input, Image output, Parameters param);
    }
}