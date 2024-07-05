using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.IO;

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

    public static string GetDebugRoot() =>
         Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;

    public static string GetDebugContentDir() =>
        Path.Combine(GetDebugRoot(), "Content");
}
