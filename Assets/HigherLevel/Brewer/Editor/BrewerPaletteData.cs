using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace HigherLevel.Brewer {
  public enum Nature {Sequential, Diverging, Qualitative}
  public enum ColourBlindSupport {Accommodating, PossiblyConfusing, DefinitelyConfusing};

  [System.Serializable]
  public class BrewerData
  {
    public Palette[] Palettes;

    public Palette GetPalette(Nature nature, string scheme, int colourCount)
    {
      var n = nature.ToString();
      return Palettes.Where(p => p.Nature == n)
        .Where(p => string.Equals(p.Scheme, scheme))
        .Where(p => p.Colours.Length == colourCount)
        .FirstOrDefault();
    }

    public Palette[] GetSchemes(Nature nature, int colourCount, ColourBlindSupport cbSupport)
    {
      var n = nature.ToString();
      return Palettes.Where(p => p.Nature == n)
        .Where(p => p.Colours.Length == colourCount)
        .Where(p => p.ColourBlindSafe <= (int)cbSupport)
        .ToArray();
    }
  }

  [System.Serializable]
  public class Palette {
    public string Nature;
    public Color32[] Colours;
    public string Scheme;
    public int ColourBlindSafe;
  }
}