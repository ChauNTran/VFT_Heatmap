using UnityEngine;
using System.IO;

namespace MakeMeSmall
{
    public class PngFile
    {
        static byte[] readPngFile(string path)
        {
            FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            BinaryReader bin = new BinaryReader(fileStream);
            byte[] values = bin.ReadBytes((int)bin.BaseStream.Length);

            bin.Close();

            return values;
        }

        public static Texture2D Load(string filepath)
        {
            byte[] readBinary = readPngFile(filepath);
            int pos = 16;
            int width = 0;
            for (int i = 0; i < 4; i++)
            {
                width = width * 256 + readBinary[pos++];
            }
            int height = 0;
            for (int i = 0; i < 4; i++)
            {
                height = height * 256 + readBinary[pos++];
            }
            Texture2D texture = new Texture2D(width, height);
            texture.LoadImage(readBinary);
            return texture;
        }

        public static void Save(Texture2D tex, string filepath)
        {
            byte[] pngData = tex.EncodeToPNG();
            System.IO.File.WriteAllBytes(filepath, pngData);
        }
    }
}