using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

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
}
