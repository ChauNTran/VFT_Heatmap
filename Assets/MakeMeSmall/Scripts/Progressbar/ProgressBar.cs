using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MakeMeSmall.ProgressBar
{
    public class ProgressBarCancelException : Exception
    {
    }

    public class ProgressBar : IDisposable
    {
        public string Title { get; set; }

        public ProgressBar(string title)
        {
            Title = title;
        }

        public void Show(ProgressState state, string label = "")
        {
            float progress = state.GetProgress();
            float clamped = (float)Math.Max(0.0, Math.Min(1.0, progress));
            string progressText = (int)(clamped * 100) + "%" + " " + label;
            bool canceled = UnityEditor.EditorUtility.DisplayCancelableProgressBar(Title, progressText, clamped);
            if (canceled)
            {
                throw new ProgressBarCancelException();
            }
        }

        public void Dispose()
        {
            UnityEditor.EditorUtility.ClearProgressBar();
        }

        public static void Show(ProgressBar progressBar, ProgressState state, string label = "")
        {
            if (progressBar != null && state != null)
            {
                progressBar.Show(state, label);
            }
        }
    }

    public class ProgressState
    {
        public float Value = 0.0f;
        public float RangeMin = 0.0f;
        public float RangeMax = 1.0f;

        public ProgressState(float rangeMin, float rangeMax)
        {
            this.Value = 0;
            this.RangeMin = rangeMin;
            this.RangeMax = rangeMax;
        }

        public ProgressState SetValue(float value)
        {
            Value = value;
            return this;
        }

        public ProgressState AddValue(float value)
        {
            Value += value;
            return this;
        }

        public float GetProgress()
        {
            float progress = RangeMin + (RangeMax - RangeMin) * Value;
            if (progress < RangeMin || RangeMax < progress)
            {
                UnityEngine.Debug.Break();
            }
            return progress;
        }

        public ProgressState CreateSub(float MaxStep)
        {
            return new ProgressState(GetProgress(), GetProgress() + MaxStep * (RangeMax - RangeMin));
        }
    }

}
