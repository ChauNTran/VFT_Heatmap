using System;

namespace MakeMeSmall.MathD
{
    public class Vec2
    {
        public double x;
        public double y;
        public Vec2(double x, double y) { this.x = x; this.y = y; }

        public override string ToString()
        {
            return "(" + x + ", " + y + ")";
        }
        public static Vec2 operator +(Vec2 v1, Vec2 v2)
        {
            return new Vec2(v1.x + v2.x, v1.y + v2.y);
        }

        public static Vec2 operator -(Vec2 v1, Vec2 v2)
        {
            return new Vec2(v1.x - v2.x, v1.y - v2.y);
        }

        public static Vec2 operator *(double value, Vec2 v2)
        {
            return new Vec2(value * v2.x, value * v2.y);
        }

        public static Vec2 operator *(Vec2 v, Mat2x2 m)
        {
            return new Vec2(v.x * m.m11 + v.y * m.m21, v.x * m.m12 + v.y * m.m22);
        }
        public static double operator *(Vec2 v1, Vec2 v2)
        {
            return v1.x * v2.x + v1.y * v2.y;
        }

        public Vec2 NormalSafe()
        {
            double lenSqr = x * x + y * y;
            if (lenSqr <= 1e-16)
            {
                return new Vec2(0, 0);
            }
            double len = (double)Math.Sqrt((double)lenSqr);
            return new Vec2(x / len, y / len);
        }
        public static Vec2 operator /(Vec2 v, double value)
        {
            return new Vec2(v.x / value, v.y / value);
        }
    }

    public class Vec3
    {
        public double x;
        public double y;
        public double z;
        public Vec3(double x, double y, double z) { this.x = x; this.y = y; this.z = z; }

        public static double DistanceSqr(Vec3 v1, Vec3 v2)
        {
            double dx = v1.x - v2.x;
            double dy = v1.y - v2.y;
            double dz = v1.z - v2.z;
            return dx * dx + dy * dy + dz * dz;
        }

        public static Vec3 operator +(Vec3 v1, Vec3 v2)
        {
            return new Vec3(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
        }


        public static Vec3 operator -(Vec3 v1, Vec3 v2)
        {
            return new Vec3(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
        }

        public static Vec3 operator *(double value, Vec3 v2)
        {
            return new Vec3(value * v2.x, value * v2.y, value * v2.z);
        }

        public static Vec3 operator /(Vec3 v, double value)
        {
            return new Vec3(v.x / value, v.y / value, v.z / value);
        }

    }

    public class Mat2x2
    {
        // [m11, m12]
        // [m21, m22]

        public double m11, m12, m21, m22;

        public static Mat2x2 FromVecVec(Vec2 v1, Vec2 v2)
        {
            return new Mat2x2(
                v1.x * v2.x,
                v1.x * v2.y,
                v1.y * v2.x,
                v1.y * v2.y);
        }

        public static Mat2x2 operator *(Mat2x2 mat1, double value)
        {
            return new Mat2x2(mat1.m11 * value, mat1.m12 * value, mat1.m21 * value, mat1.m22 * value);
        }

        public static Mat2x2 operator *(double value, Mat2x2 mat1)
        {
            return mat1 * value;
        }

        public static Mat2x2 operator +(Mat2x2 mat1, Mat2x2 mat2)
        {
            return new Mat2x2(
                mat1.m11 + mat2.m11,
                mat1.m12 + mat2.m12,
                mat1.m21 + mat2.m21,
                mat1.m22 + mat2.m22);
        }

        public static Mat2x2 operator /(Mat2x2 mat1, double value)
        {
            return new Mat2x2(mat1.m11 / value, mat1.m12 / value, mat1.m21 / value, mat1.m22 / value);
        }

        public static Mat2x2 operator *(Mat2x2 mat1, Mat2x2 mat2)
        {
            return new Mat2x2(
                mat1.m11 * mat2.m11 + mat1.m12 * mat2.m21,
                mat1.m11 * mat2.m12 + mat1.m12 * mat2.m22,
                mat1.m21 * mat2.m11 + mat1.m22 * mat2.m21,
                mat1.m21 * mat2.m12 + mat1.m22 * mat2.m22
            );
        }

        public Mat2x2(double m11, double m12, double m21, double m22)
        {
            this.m11 = m11;
            this.m12 = m12;
            this.m21 = m21;
            this.m22 = m22;
        }

        public Mat2x2 Inverse()
        {
            const double epsilon = 1e-102;
            double d = m11 * m22 - m12 * m21;
            if (Math.Abs(d) < epsilon)
            {
                return new Mat2x2(0, 0, 0, 0);
            }
            double invd = 1.0 / d;
            return new Mat2x2(m22 * invd, -m21 * invd, -m12 * invd, m11 * invd);
        }

        public void SVD(out Mat2x2 U, out Mat2x2 S, out Mat2x2 Vt)
        {
            // accoding to the web page:
            // http://www.lucidarme.me/?p=4624
            double a = m11;
            double b = m12;
            double c = m21;
            double d = m22;

            double v1 = 2 * a * c + 2 * b * d;
            double v2 = a * a + b * b - c * c - d * d;
            double theta = 0.5 * (double)Math.Atan2((double)v1, (double)v2);
            U = new Mat2x2(
                (double)Math.Cos((double)theta),
                -(double)Math.Sin((double)theta),
                (double)Math.Sin((double)theta),
                (double)Math.Cos((double)theta));

            double S1 = a * a + b * b + c * c + d * d;
            double S2 = (double)Math.Sqrt((double)(v2 * v2 + v1 * v1));
            double s1 = (double)Math.Sqrt((double)((S1 + S2) * 0.5));
            double s2 = (double)Math.Sqrt((double)((S1 - S2) * 0.5));
            S = new Mat2x2(s1, 0, 0, s2);

            double u1 = 2 * a * b + 2 * c * d;
            double u2 = a * a - b * b + c * c - d * d;
            double phi = 0.5 * (double)Math.Atan2((double)u1, (double)u2);
            double cp = (double)Math.Cos((double)phi);
            double sp = (double)Math.Sin((double)phi);
            double ct = (double)Math.Cos((double)theta);
            double st = (double)Math.Sin((double)theta);
            double s11 = (a * ct + c * st) * cp + (b * ct + d * st) * sp;
            double s22 = (a * st - c * ct) * sp + (-b * st + d * ct) * cp;
            double sign_s11 = Math.Sign(s11);
            double sign_s22 = Math.Sign(s22);
            Vt = new Mat2x2(sign_s11 * cp, sign_s11 * sp, -sign_s22 * sp, sign_s22 * cp);
        }
    }
}
