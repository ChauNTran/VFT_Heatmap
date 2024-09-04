using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MakeMeSmall.DataTypes
{
    class Size
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public Size(int w, int h)
        {
            Width = w;
            Height = h;
        }
    }

}
