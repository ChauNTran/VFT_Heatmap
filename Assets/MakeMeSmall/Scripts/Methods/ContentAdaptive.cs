using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using MakeMeSmall.DataTypes;
using MakeMeSmall.MathD;

namespace MakeMeSmall.Methods
{
    using Kernel = ContentAdaptiveInternal.Kernel;
    using Config = ContentAdaptiveInternal.Config;
    using Position = ContentAdaptiveInternal.Position;
    using For = ContentAdaptiveInternal.For;

    public class ContentAdaptiveParameters : Parameters
    {
    }

    public class ContentAdaptive : IMethod
    {
        Vec2[] m;
        Mat2x2[] S;
        Vec3[] v;
        double[] s;
        Vec3[] c; // CIELAB, [0, 1]
        List<double[]> w_;
        List<double[]> g_;
        double[] w(Kernel k)
        {
            return w_[k.index];
        }
        double[] g(Kernel k)
        {
            return g_[k.index];
        }

        public void Downscale(Image input, Image output, Parameters param)
        {
            if (input == null || output == null)
            {
                return;
            }

            downscale(input, output, param);
            Debug.Log("Content adaptive donwscaling: finished");
        }

        //--------------------------------------------------------------------------

        void runIteration(Config config, int iteration, ProgressBar.ProgressBar progressBar, ProgressBar.ProgressState state)
        {
            MakeMeSmall.ProgressBar.ProgressBar.Show(progressBar, state, "EStep (iteration = " + iteration + ")");
            eStep(config, iteration, progressBar, state.CreateSub(0.3f));
            state.AddValue(0.3f);

            MakeMeSmall.ProgressBar.ProgressBar.Show(progressBar, state, "MStep (iteration = " + iteration + ")");
            mStep(config, iteration, progressBar, state.CreateSub(0.3f));
            state.AddValue(0.3f);

            MakeMeSmall.ProgressBar.ProgressBar.Show(progressBar, state, "CStep (iteration = " + iteration + ")");
            cStep(config, iteration, progressBar, state.CreateSub(0.4f));
            state.AddValue(0.4f);
        }

        // return image whose size is [config.wo, config.ho].
        void createOutputImage(Config config, Image output)
        {
            int countAll = 0;
            For.AllKernels(config, (_, k) =>
            {
                List<Vec3> debug = new List<Vec3>();

                Vec3 sumColor = new Vec3(0, 0, 0);
                double sumWeight = 0;
                int count = 0;
                For.AllPixelsOfRegion(config, k, (_0, i) =>
                {
                    countAll++;
                    double weight = w(k)[index(config, i, k)];
                    int x = (int)(i.p.x + m[k.index].x - k.xi - (int)(1.5 * config.rx));
                    int y = (int)(i.p.y + m[k.index].y - k.yi - (int)(1.5 * config.rx));
                    if (x < 0 || config.wi <= x || y < 0 || config.hi <= y)
                    {
                        return;
                    }
                    int idx = x + y * config.wi;
                    sumColor += weight * c[idx];
                    sumWeight += weight;
                    count++;
                    debug.Add(weight * c[idx]);
                });

                Vec3 aveColor = sumColor / sumWeight;
                System.Diagnostics.Debug.Assert(0 <= aveColor.x && aveColor.x <= 1);
                System.Diagnostics.Debug.Assert(0 <= aveColor.y && aveColor.y <= 1);
                System.Diagnostics.Debug.Assert(0 <= aveColor.z && aveColor.z <= 1);
                float r = (float)aveColor.x;
                float g = (float)aveColor.y;
                float b = (float)aveColor.z;
                int oidex = k.x + k.y * output.width;
                output.pixels[oidex] = new Color(r, g, b);
            });

            Debug.Log("countAll = " + countAll);
        }

        void downscale(Image input, Image output, Parameters param)
        {
            ProgressBar.ProgressState state = new ProgressBar.ProgressState(0, 1.0f);

            int iteration = 0;
            Config config = new Config(input.width, input.height, output.width, output.height);

            MakeMeSmall.ProgressBar.ProgressBar.Show(param.ProgressBar,state, "Initialize");
            initialize(config, input);
            state.SetValue(0.1f);

            const int MaxIteration = 10;
            for (int i = 0; i < MaxIteration; i++)
            {
                iteration++;

                try
                {
                    runIteration(config, i, param.ProgressBar, state.CreateSub(0.8f / MaxIteration));
                    createOutputImage(config, output);
                    state.SetValue(0.1f + (1 + i) * 0.8f / MaxIteration);
                }
                catch (Exception ex)
                {
                    Debug.Log(ex);
                }
            }
            state.SetValue(0.9f);

            MakeMeSmall.ProgressBar.ProgressBar.Show(param.ProgressBar,state, "Create output image");
            createOutputImage(config, output);
            state.SetValue(1.0f);
        }

        void initialize(Config config, Image input)
        {
            int Rsize = (int)(4 * config.rx + 1) * (int)(4 * config.ry + 1);
            var pixels = input.pixels;

            // copy color
            c = new Vec3[config.wi * config.hi];
            System.Diagnostics.Debug.Assert(c.Length == input.width * input.height);
            for (int y = 0; y < input.height; y++)
            {
                int offset = y * input.width;
                for (int x = 0; x < input.width; x++)
                {
                    int idx = x + offset;
                    float r = pixels[idx].r;
                    float g = pixels[idx].g;
                    float b = pixels[idx].b;
                    c[idx] = new Vec3(r, g, b);
                }
            }

            // init
            w_ = new List<double[]>();
            g_ = new List<double[]>();
            m = new Vec2[config.wo * config.ho];
            S = new Mat2x2[config.wo * config.ho];
            v = new Vec3[config.wo * config.ho];
            s = new double[config.wo * config.ho];
            For.AllKernels(config, (_, k) =>
            {
                w_.Add(new double[Rsize]);
                g_.Add(new double[Rsize]);
                m[k.index] = new Vec2((0.5 + k.x) * config.rx, (0.5 + k.y) * config.ry);
                S[k.index] = new Mat2x2(config.rx / 3, 0, 0, config.ry / 3);
                v[k.index] = new Vec3(0.5, 0.5, 0.5);
                s[k.index] = 1e-4;
            });
        }

        int index(Config config, Position i, Kernel k)
        {
            int rx = (int)(4 * config.rx + 1);
            int ox = -(int)(1.5 * config.rx);
            int oy = -(int)(1.5 * config.ry);
            int x = (int)i.p.x - k.xi - ox;
            int y = (int)i.p.y - k.yi - oy;
            int w = (int)(4 * config.rx + 1);
            int h = (int)(4 * config.ry + 1);
            if (0 <= x && x < w && 0 <= y && y < h)
            {
                return x + y * (int)(4 * config.rx + 1);
            }
            return -1;
        }

        void eStep(Config config, int iteration, ProgressBar.ProgressBar progressBar, ProgressBar.ProgressState state)
        {
            double[] sum_w = new double[config.wi * config.hi];
            For.AllKernels(config, (_, k) =>
            {
                MakeMeSmall.ProgressBar.ProgressBar.Show(progressBar, state.AddValue(0.5f / config.KernelSize), "EStep (iteration = " + iteration + ")");
                double sum = 0;
                For.AllPixelsOfRegion(config, k, (_1, i) =>
                {
                    int idx = index(config, i, k);
                    System.Diagnostics.Debug.Assert(idx >= 0);
                    System.Diagnostics.Debug.Assert(idx < w(k).Length);
                    w(k)[index(config, i, k)] = calcGaussian(k, i);
                    sum += w(k)[index(config, i, k)];
                });

                For.AllPixelsOfRegion(config, k, (_1, i) =>
                {
                    w(k)[index(config, i, k)] = div(w(k)[index(config, i, k)], sum);
                    sum_w[i.index] += w(k)[index(config, i, k)];
                });
            });

            For.AllPixels(config, (_, i) =>
            {
                For.AllKernelOfPixel(config, i, (_1, k) =>
                {
                    g(k)[index(config, i, k)] = div(w(k)[index(config, i, k)], sum_w[i.index]);
                });
            });
        }

        void mStep(Config config, int iteration, ProgressBar.ProgressBar progressBar, ProgressBar.ProgressState state)
        {
            For.AllKernels(config, (_, k) =>
            {
                MakeMeSmall.ProgressBar.ProgressBar.Show(progressBar, state.AddValue(1.0f / config.KernelSize), "MStep (iteration = " + iteration + ")");
                var gsum = sumInRegion(config, k, i => g(k)[index(config, i, k)]);
                S[k.index] = sumInRegion(config, k, i => g(k)[index(config, i, k)] * Mat2x2.FromVecVec(i.p - m[k.index], i.p - m[k.index])) / gsum;
                m[k.index] = sumInRegion(config, k, i => g(k)[index(config, i, k)] * i.p) / gsum;
                v[k.index] = sumInRegion(config, k, i => g(k)[index(config, i, k)] * c[i.index]) / gsum;
            });
        }

        void cStep(Config config, int iteration, ProgressBar.ProgressBar progressBar, ProgressBar.ProgressState state)
        {
            // Spatial constraints
            var aveM = new Vec2[config.KernelSize];
            For.AllKernels(config, (_, k) =>
            {
                MakeMeSmall.ProgressBar.ProgressBar.Show(progressBar, state.AddValue(0.2f / config.KernelSize), "CStep (iteration = " + iteration + ")");
                aveM[k.index] = new Vec2(0, 0);
                var neighbors = k.Neighbors4(config);
                foreach (var n in neighbors)
                {
                    aveM[k.index] += m[n.index];
                }
                aveM[k.index] /= neighbors.Count;
            });
            For.AllKernels(config, (_, k) =>
            {
                MakeMeSmall.ProgressBar.ProgressBar.Show(progressBar, state.AddValue(0.2f / config.KernelSize), "CStep (iteration = " + iteration + ")");
                m[k.index] = 0.5 * (aveM[k.index] + m[k.index]);
                double halfWidth = 0.25 * config.rx;
                double halfHeight = 0.25 * config.ry;
                m[k.index] = clampBox(m[k.index], k.xi - halfWidth, k.yi - halfHeight, 2 * halfWidth, 2 * halfHeight);
            });

            // Constrain spatial variance

            For.AllKernels(config, (_, k) =>
            {
                MakeMeSmall.ProgressBar.ProgressBar.Show(progressBar, state.AddValue(0.2f / config.KernelSize), "CStep (iteration = " + iteration + ")");
                Mat2x2 _U, _S, _Vt;
                S[k.index].SVD(out _U, out _S, out _Vt);
                _S.m11 = clamp(_S.m11, 0.05, 0.1);
                _S.m22 = clamp(_S.m22, 0.05, 0.1);
                var newS = _U * _S * _Vt;
                if (double.IsNaN(newS.Inverse().m11) == false)
                {
                    S[k.index] = newS;
                }
            });


            // Shape constraints
            For.AllKernels(config, (_, k) =>
            {
                MakeMeSmall.ProgressBar.ProgressBar.Show(progressBar, state.AddValue(0.4f / config.KernelSize), "CStep (iteration = " + iteration + ")");
                var neighbors = k.Neighbors8(config);
                foreach (var n in neighbors)
                {
                    var d = new Vec2(n.xi - k.xi, n.yi - k.yi);
                    var sv = sumInRegion(config, k, (i) =>
                    {
                        double gki = g(k)[index(config, i, k)];
                        double dot = (i.p - m[k.index]) * d;
                        return gki * Math.Max(0, dot);
                    });
                    var f = sumInRegion(config, k, (i) =>
                    {
                        try
                        {
                            double gki = g(k)[index(config, i, k)];
                            double gni = index(config, i, n) >= 0 ? g(k)[index(config, i, n)] : 0;
                            return gki * gni;
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning(e.ToString() + "\n" + e.StackTrace);
                            return 0;
                        }
                    });
                    var o = sumInRegion(config, k, (i) =>
                    {
                        if (i.p.x >= config.wi)
                        {
                            return new Vec2(0, 0);
                        }
                        if (i.p.y >= config.hi)
                        {
                            return new Vec2(0, 0);
                        }
                        double gki = g(k)[index(config, i, k)];
                        double gni = index(config, i, n) >= 0 ? g(k)[index(config, i, n)] : 0;
                        var p10 = new Position(config, (int)i.p.x + 1, (int)i.p.y);
                        var p01 = new Position(config, (int)i.p.x, (int)i.p.y + 1);
                        double gki10 = g(k)[index(config, p10, k)];
                        double gki01 = g(k)[index(config, p01, k)];
                        double val00 = gki / (gki + gni);
                        double val10 = gki10 / (gki10 + gni);
                        double val01 = gki01 / (gki01 + gni);
                        return new Vec2(val10 - val00, val01 - val00);
                    });

                    double cos25 = Math.Cos(Math.PI * 25 / 180.0);
                    if (sv > 0.2 * config.rx || (f < 0.08 && d.NormalSafe() * o.NormalSafe() < cos25))
                    {
                        s[k.index] *= 1.1;
                        s[n.index] *= 1.1;
                    }
                }
            });
        }

        Vec2 clampBox(Vec2 p, double left, double top, double width, double height)
        {
            return new Vec2(clamp(p.x, left, left + width), clamp(p.y, top, top + height));
        }

        double clamp(double val, double min, double max)
        {
            return Math.Max(min, Math.Min(max, val));
        }

        double div(double a, double b)
        {
            if (Math.Abs(b) > 0)
            {
                return a / b;
            }
            return a;
        }

        double calcGaussian(Kernel k, Position i)
        {
            var dpos = i.p - m[k.index];
            var invS = S[k.index].Inverse();
            var posTerm = -0.5 * dpos * invS * dpos;
            var dcol = c[i.index] - v[k.index];
            var colTerm = -Vec3.DistanceSqr(c[i.index], v[k.index]) / (2 * s[k.index] * s[k.index]);
            try
            {
                double val = Math.Max(-1e2, Math.Min(1e2, posTerm + colTerm));
                var result = Math.Exp(val);
                System.Diagnostics.Debug.Assert(double.IsNaN(result) == false);
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.ToString() + "\n" + ex.StackTrace);
                return 0;
            }
        }

        double sumInRegion(Config config, Kernel k, Func<Position, double> pos2val)
        {
            double result = 0;
            For.AllPixelsOfRegion(config, k, (_, i) =>
            {
                result += pos2val(i);
            });
            return result;
        }

        Mat2x2 sumInRegion(Config config, Kernel k, Func<Position, Mat2x2> pos2val)
        {
            var result = new Mat2x2(0, 0, 0, 0);
            For.AllPixelsOfRegion(config, k, (_, i) =>
            {
                result += pos2val(i);
            });
            return result;
        }

        Vec2 sumInRegion(Config config, Kernel k, Func<Position, Vec2> pos2val)
        {
            var result = new Vec2(0, 0);
            For.AllPixelsOfRegion(config, k, (_, i) =>
            {
                result += pos2val(i);
            });
            return result;
        }


        Vec3 sumInRegion(Config config, Kernel k, Func<Position, Vec3> pos2val)
        {
            var result = new Vec3(0, 0, 0);
            For.AllPixelsOfRegion(config, k, (_, i) =>
            {
                result += pos2val(i);
            });
            return result;
        }

        System.Diagnostics.Stopwatch stopwatch = null;
        int stopwatchCnt = 0;

        void printElapsedTime(string prefix = "")
        {
            if (null == stopwatch)
            {
                stopwatch = System.Diagnostics.Stopwatch.StartNew();
            }
            Console.WriteLine("[" + stopwatchCnt + "]" + prefix + ": " + stopwatch.ElapsedMilliseconds + " ms.");
            stopwatchCnt++;
        }

    }

    namespace ContentAdaptiveInternal
    {
        class Config
        {
            public int wi, hi, wo, ho;
            public double rx, ry;
            public Config(int wi, int hi, int wo, int ho)
            {
                this.wi = wi;
                this.hi = hi;
                this.wo = wo;
                this.ho = ho;
                this.rx = (double)wi / wo;
                this.ry = (double)hi / ho;
            }
            public int KernelSize
            {
                get
                {
                    return wo * ho;
                }
            }
        }

        class Position
        {
            public Vec2 p;
            public int index;
            public void Set(Config config, int xi, int yi)
            {
                this.p = new Vec2(xi, yi);
                this.index = xi + yi * config.wi;
            }

            public Position()
            {
            }

            public Position(Config config, int xi, int yi)
            {
                Set(config, xi, yi);
            }

        }

        class Kernel
        {
            public int x, y;
            public int index;

            // min position of kernel
            public int xi, yi;
            public int indexi;

            public Kernel()
            {
            }

            public Kernel(Config config, int xo, int yo)
            {
                Set(config, xo, yo);
            }

            public void Set(Config config, int xo, int yo)
            {
                this.x = xo;
                this.y = yo;
                this.index = xo + yo * config.wo;

                this.xi = (int)((xo) * config.rx);
                this.yi = (int)((yo) * config.ry);
                this.indexi = xi + yi * config.wi;
            }

            public List<Kernel> Neighbors4(Config config)
            {
                List<Kernel> neighbors = new List<Kernel>();
                addIfValid(config, x, y - 1, neighbors);
                addIfValid(config, x - 1, y, neighbors);
                addIfValid(config, x + 1, y, neighbors);
                addIfValid(config, x, y + 1, neighbors);
                return neighbors;
            }

            public List<Kernel> Neighbors8(Config config)
            {
                List<Kernel> neighbors = new List<Kernel>();
                addIfValid(config, x - 1, y - 1, neighbors);
                addIfValid(config, x, y - 1, neighbors);
                addIfValid(config, x + 1, y - 1, neighbors);
                addIfValid(config, x - 1, y, neighbors);
                addIfValid(config, x + 1, y, neighbors);
                addIfValid(config, x - 1, y + 1, neighbors);
                addIfValid(config, x, y + 1, neighbors);
                addIfValid(config, x + 1, y + 1, neighbors);
                return neighbors;
            }

            void addIfValid(Config config, int kx, int ky, List<Kernel> ls)
            {
                if (0 <= kx && kx < config.wo && 0 <= ky && ky < config.ho)
                {
                    ls.Add(new Kernel(config, kx, ky));
                }
            }
        }

        class For
        {
            static public int AllKernels(Config config, Action<Config, Kernel> fnc)
            {
                int counter = 0;
                Kernel k = new Kernel();
                for (int ky = 0; ky < config.ho; ky++)
                {
                    for (int kx = 0; kx < config.wo; kx++)
                    {
                        k.Set(config, kx, ky);
                        fnc(config, k);
                        counter++;
                    }
                }
                return counter;
            }

            static public int AllPixels(Config config, Action<Config, Position> fnc)
            {
                int counter = 0;
                Position i = new Position();
                for (int y = 0; y < config.hi; y++)
                {
                    for (int x = 0; x < config.wi; x++)
                    {
                        i.Set(config, x, y);
                        fnc(config, i);
                        counter++;
                    }
                }
                return counter;
            }

            static public int AllPixelsOfRegion(Config config, Kernel k, Action<Config, Position> fnc)
            {
                int counter = 0;
                int baseX = (int)((k.x + 0.5) * config.rx);
                int baseY = (int)((k.y + 0.5) * config.ry);
                Position i = new Position();
                for (int dy = (int)(-2 * config.ry + 1); dy <= (int)(2 * config.ry - 1); dy++)
                {
                    int y = baseY + dy;
                    if (y < 0 || config.hi <= y)
                    {
                        continue;
                    }
                    for (int dx = (int)(-2 * config.rx + 1); dx <= (int)(2 * config.rx - 1); dx++)
                    {
                        int x = baseX + dx;
                        if (x < 0 || config.wi <= x)
                        {
                            continue;
                        }
                        i.Set(config, x, y);
                        fnc(config, i);
                        counter++;
                    }
                }
                return counter;
            }

            static public int AllKernelOfPixel(Config config, Position i, Action<Config, Kernel> fnc)
            {
                int counter = 0;
                var xo = (int)(i.p.x / config.rx);
                var yo = (int)(i.p.y / config.ry);

                var rx2 = 2 * config.rx;
                var ry2 = 2 * config.ry;

                Kernel k = new Kernel();
                for (int ky = yo - 2; ky <= yo + 2; ky++)
                {
                    if (ky < 0 || config.ho <= ky)
                    {
                        continue;
                    }
                    for (int kx = xo - 2; kx <= xo + 2; kx++)
                    {
                        if (kx < 0 || config.wo <= kx)
                        {
                            continue;
                        }
                        var x = (kx + 0.5) * config.rx;
                        var y = (ky + 0.5) * config.ry;
                        var dx = i.p.x - x;
                        var dy = i.p.y - y;
                        if (-rx2 < dx && dx < rx2 && -ry2 < dy && dy < ry2)
                        {
                            k.Set(config, kx, ky);
                            fnc(config, k);
                            counter++;
                        }
                    }
                }
                return counter;
            }
        }
    }
}