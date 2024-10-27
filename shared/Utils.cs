using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.IO;
using System.Collections.Generic;

namespace Game2D;

public class Utils
{
    public static Texture2D CreateRect(GraphicsDevice device, int width, int height, Color color)
    {
        var rect = new Texture2D(device, width, height);
        var data = new Color[width * height];

        Array.Fill(data, color);
        rect.SetData(data);

        return rect;
    }

    public static int AreaOfOverlap(Rectangle a, Rectangle b) {
        int x_dist = Math.Min(a.Right, b.Right) - Math.Max(a.Left, b.Left);
        int y_dist = Math.Min(a.Bottom, b.Bottom) - Math.Max(a.Top, b.Top);
        if (x_dist > 0 && y_dist > 0)
        {
            return x_dist * y_dist;
        }
        else
        {
            return 0;
        }
    }

    //translates relative coordinates (i.e 0-1) to absolute coordinates (0-width/height)
    public static Rectangle ToAbsolute(Rectangle viewport, RectangleF rect) =>
        new ((int)(rect.X * viewport.Width) + viewport.Location.X,
            (int)(rect.Y * viewport.Height) + viewport.Location.Y,
            (int)(rect.Width * viewport.Width),
            (int)(rect.Height * viewport.Height));

    public static Rectangle ToAbsolute(RectangleF viewport, RectangleF rect) =>
        new ((int)(rect.X * viewport.Width + viewport.X),
            (int)(rect.Y * viewport.Height + viewport.Y),
            (int)(rect.Width * viewport.Width),
            (int)(rect.Height * viewport.Height));
    
    public static Vector2 ToAbsolute(Rectangle viewport, Vector2 vec) =>
        new (vec.X * viewport.Width + viewport.X,
                    vec.Y * viewport.Height + viewport.Y);
    public static Vector2 ToAbsolute(RectangleF viewport, Vector2 vec) =>
        new (vec.X * viewport.Width + viewport.X,
                    vec.Y * viewport.Height + viewport.Y);


    public enum InterpolationType {
        LINEAR,
        EASE_IN,
        EASE_OUT,
        EASE_IN_OUT,
        CUSTOM_BEZIER,
    }

    // takes start/end and progress (0-1) and optional interpolation type
    // rather than the incremental approach as it used to (i.e progress was an int referring to some value 0-endVal)
    public static float Interpolate(float start, float end, float progress, InterpolationType type = InterpolationType.LINEAR)
    {
        switch(type) {
            case InterpolationType.LINEAR:
                return start + (end - start) * progress; // linear
            case InterpolationType.EASE_IN:
                return start + (1f - (float)Math.Sin(((1 + progress) * Math.PI)/2)) * (end - start); // sine ease-in
            case InterpolationType.EASE_OUT:
                return start + (float)Math.Sin((progress * Math.PI)/2) * (end - start); // sine ease-out
            case InterpolationType.EASE_IN_OUT:
                return start + (progress * progress * (3.0f - 2.0f * progress)) * (end - start); // bezier ease-in-out
            case InterpolationType.CUSTOM_BEZIER:
                return start + CubicBezier(progress, 0.2f, 0f, 0.7f, 1f) * (end - start);
            default:
                return start + (end - start) * progress;
        }
    }

    public static float CubicBezier(float x, float p1x, float p1y, float p2x, float p2y)
    {
        Vector2 p0 = new(0, 0);
        Vector2 p1 = new(p1x, p1y);
        Vector2 p2 = new(p2x, p2y);
        Vector2 p3 = new(1, 1);
        
        float t = -1;
        foreach (var r in cubicRoots(x, p0.X, p1.X, p2.X, p3.X))
        {
            if (r < 0 || r > 1) continue;
            t = r;
            break;
        }
        if (t == -1) return 0f;

        float cy = 3 * (p1.Y - p0.Y);
        float by = 3 * (p2.Y - p1.Y) - cy;
        float ay = p3.Y - p0.Y - cy - by;

        return (ay * t * t * t) + (by * t * t) + (cy * t) + p0.Y;
    }

    // Find the roots for a cubic polynomial with bernstein coefficients
    // adapted from: https://stackoverflow.com/a/51883347/15292198
    private static List<float> cubicRoots(float x, float pa, float pb, float pc, float pd)
    {
        float a = -pa + 3 * pb - 3 * pc + pd;
        float b =  3 * pa - 6 * pb + 3 * pc;
        float c = -3 * pa + 3 * pb;
        float d =  pa - x;

        if (approx(a, 0))
        { // not cubic
            if (approx(b, 0))
            { // not quadratic
                if (approx(c, 0))
                { // no solutions
                    return [];
                }
                return [-d / c];
            }
            var qq = (float)Math.Sqrt(c * c - 4 * b * d);
            return [
                (qq - c) / (2 * b),
                (-c - qq) / (2 * b)
            ];
        }

        b /= a;
        c /= a;
        d /= a;

        float b3 = b / 3;
        float p = (3 * c - b*b) / 3;
        float p3 = p / 3;
        float q = (2 * b*b*b - 9 * b * c + 27 * d) / 27;
        float q2 = q / 2;
        float discriminant = q2 * q2 + p3 * p3 * p3;

        // case 1: three real roots
        if (discriminant < 0)
        {
            float r = (float)Math.Sqrt(-p3 * (-p3) * (-p3));
            float t = -q / (2 * r);
            float cosphi = t < -1 ? -1 : t > 1 ? 1 : t;
            float phi = (float)Math.Acos(cosphi);
            float crtr = cubeRoot(r);
            float t1 = 2 * crtr;

            return [
                t1 * (float)Math.Cos(phi/3) - b3,
                t1 * (float)Math.Cos((phi + 2 * Math.PI) / 3) - b3,
                t1 * (float)Math.Cos((phi + 4 * Math.PI) / 3) - b3
            ];
        }
        // case 2: two real roots
        else if (discriminant == 0) {
            float u1 = q2 < 0 ? cubeRoot(-q2) : -cubeRoot(q2);
            return [
                2 * u1 - b3,
                -u1 - b3
            ];
        }
        // case 3: one real root
        else {
            float sd = (float)Math.Sqrt(discriminant);
            float u1 = cubeRoot(-q2 + sd);
            float v1 = cubeRoot(q2 + sd);
            return [u1 - v1 - b3];
        }
    }

    private static bool approx(float a, float b) => Math.Abs(a - b) < 0.000001;

    private static float cubeRoot(float x) => x < 0 ? -(float)Math.Pow(-x, 1f/3f) : (float)Math.Pow(x, 1f/3f);

    public static string GetDebugRoot() =>
         Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;

    public static string GetDebugContentDir() =>
        Path.Combine(GetDebugRoot(), "Content");
}
