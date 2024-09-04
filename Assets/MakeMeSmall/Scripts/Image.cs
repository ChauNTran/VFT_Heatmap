using UnityEngine;
using System.Collections;
using System;

namespace MakeMeSmall
{
    public class Image
    {
        const float Eps = 1e-4f;

        public Color[] pixels { get; private set; }
        public int width { get; private set; }
        public int height { get; private set; }
        public Image(Color[] pixels, int width, int height)
        {
            this.pixels = pixels;
            this.width = width;
            this.height = height;
        }

        public static Image FromSprite(Sprite sprite)
        {
            return new Image(sprite);
        }

        public Image(Sprite sprite)
        {
            this.pixels = new Color[0];
            this.width = 0;
            this.height = 0;
            if (sprite != null)
            {
                var texture = sprite.texture;
                if (texture != null)
                {
                    this.pixels = texture.GetPixels();
                    this.width = texture.width;
                    this.height = texture.height;
                }
            }
        }

        public static Image FromTexture2D(Texture2D texture)
        {
            return new Image(texture);
        }

        public Image(Texture2D texture)
        {
            this.pixels = new Color[0];
            this.width = 0;
            this.height = 0;
            if (texture != null)
            {
                this.pixels = texture.GetPixels();
                this.width = texture.width;
                this.height = texture.height;
            }
        }

        public void Save(string path)
        {
            var tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
            tex.SetPixels(pixels);
            byte[] pngData = tex.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, pngData);
        }

        public static Image operator +(Image a, Image b)
        {
            Image res = Image.CreateEmpty(a.width, a.height);
            // In Debug build, operator. can be a bottle neck.
            var aps = a.pixels;
            var bps = b.pixels;
            var ops = res.pixels;
            for (int i = 0; i < a.pixels.Length; i++)
            {
                ops[i].r = aps[i].r + bps[i].r;
                ops[i].g = aps[i].g + bps[i].g;
                ops[i].b = aps[i].b + bps[i].b;
                ops[i].a = 1.0f;
            }
            return res;
        }

        public static Image operator -(Image a, Image b)
        {
            Image res = Image.CreateEmpty(a.width, a.height);
            // In Debug build, operator. can be a bottle neck.
            var aps = a.pixels;
            var bps = b.pixels;
            var ops = res.pixels;
            for (int i = 0; i < a.pixels.Length; i++)
            {
                ops[i].r = aps[i].r - bps[i].r;
                ops[i].g = aps[i].g - bps[i].g;
                ops[i].b = aps[i].b - bps[i].b;
                ops[i].a = 1.0f;
            }
            return res;
        }

        public static Image operator *(Image a, Image b)
        {
            Image res = Image.CreateEmpty(a.width, a.height);
            // Debugビルドだと.演算子のアクセスがボトルネックになりうる
            var aps = a.pixels;
            var bps = b.pixels;
            var ops = res.pixels;
            for (int i = 0; i < a.pixels.Length; i++)
            {
                ops[i].r = aps[i].r * bps[i].r;
                ops[i].g = aps[i].g * bps[i].g;
                ops[i].b = aps[i].b * bps[i].b;
                ops[i].a = 1.0f;
            }
            return res;
        }

        public static Image operator /(Image a, Image b)
        {
            Image res = Image.CreateEmpty(a.width, a.height);
            // In Debug build, operator. can be a bottle neck.
            var aps = a.pixels;
            var bps = b.pixels;
            var ops = res.pixels;
            for (int i = 0; i < a.pixels.Length; i++)
            {
                ops[i].r = bps[i].r <= Eps ? 0.0f : aps[i].r / bps[i].r;
                ops[i].g = bps[i].g <= Eps ? 0.0f : aps[i].g / bps[i].g;
                ops[i].b = bps[i].b <= Eps ? 0.0f : aps[i].b / bps[i].b;
                ops[i].a = 1.0f;
            }
            return res;
        }

        public Image Sqrt()
        {
            Image res = Image.CreateEmpty(width, height);
            var ops = res.pixels;
            for (int i = 0; i < pixels.Length; i++)
            {
                ops[i].r = pixels[i].r <= Eps ? 0.0f : (float)Math.Sqrt(pixels[i].r);
                ops[i].g = pixels[i].g <= Eps ? 0.0f : (float)Math.Sqrt(pixels[i].g);
                ops[i].b = pixels[i].b <= Eps ? 0.0f : (float)Math.Sqrt(pixels[i].b);
                ops[i].a = 1.0f;
            }
            return res;
        }

        public Image Clamp01()
        {
            Image res = Image.CreateEmpty(width, height);
            var ops = res.pixels;
            for (int i = 0; i < pixels.Length; i++)
            {
                ops[i].r = pixels[i].r < 0.0f ? 0.0f : pixels[i].r > 1.0f ? 1.0f : pixels[i].r;
                ops[i].g = pixels[i].g < 0.0f ? 0.0f : pixels[i].g > 1.0f ? 1.0f : pixels[i].g;
                ops[i].b = pixels[i].b < 0.0f ? 0.0f : pixels[i].b > 1.0f ? 1.0f : pixels[i].b;
                ops[i].a = 1.0f;
            }
            return res;
        }

        public static Image CreateEmpty(int w, int h)
        {
            return new Image(new Color[w * h], w, h);
        }
    }
}