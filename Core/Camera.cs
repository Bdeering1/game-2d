using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using MonoGame.Extended;
using System;

namespace Game2D;

public class Camera
{
    private const int SOFT_FOLLOW_WIDTH = 150;
    private const int SOFT_FOLLOW_HEIGHT = 50;
    private readonly float[] ZOOM_LEVELS =
        [
            1f,
            0.75f,   // 18/24
            0.625f,  // 15/24
            0.5f,    // 12/24
        ];

    public Hitbox Hitbox { get; set; }
    public Hitbox FollowBox { get; set; }
    public Hitbox SoftFollowBox { get; set; }
    public List<IMovable> TrackedObjects { get; set; }
    public Vector2 TrackedPosition { get; private set; }

    public float ViewScale { get; set; } = 1f;
    private Vector2 viewScaleOffset;
    private int viewScaleIdx = 0;
    private bool zooming = false;

    private GraphicsDevice graphics { get; }

    public Camera(GraphicsDevice graphics)
    {
        this.graphics = graphics;

        FollowBox = new();
        SoftFollowBox = new();
        ResetFollowBoxSize();

        Hitbox = new(
            new(0, 0),
            new(graphics.Viewport.Width, graphics.Viewport.Height)
        );
        Hitbox.Center = FollowBox.Center;

        viewScaleOffset = new((1200 - Hitbox.Width * ViewScale) / 2, (900 - Hitbox.Height * ViewScale) / 2);
    }

    public void Update(GameTime gameTime)
    {
        foreach(IMovable obj in TrackedObjects)
        {
            if (obj.Hitbox.X < FollowBox.X
             || obj.Hitbox.Y < FollowBox.Y
             || obj.Hitbox.X + obj.Hitbox.Width > FollowBox.X + FollowBox.Width
             || obj.Hitbox.Y + obj.Hitbox.Height > FollowBox.Y + FollowBox.Height)
            {
                if (viewScaleIdx + 1 >= ZOOM_LEVELS.Length) break; // minimum zoom case

                ViewScale = ZOOM_LEVELS[++viewScaleIdx];
                viewScaleOffset = new((1200 - Hitbox.Width * ViewScale) / 2, (900 - Hitbox.Height * ViewScale) / 2);
                ResetFollowBoxSize();
                FollowBox.Center = Hitbox.Center;
                SoftFollowBox.Center = Hitbox.Center;
                break;
            }
        }
        
        Vector2 avgPos = new
            (TrackedObjects.Average(obj => obj.Hitbox.CenterX),
             TrackedObjects.Average(obj => obj.Hitbox.CenterY));
        TrackedPosition = GetScreenCoords(avgPos);

        var hardFollow = false;
        if (avgPos.X < FollowBox.X)
        {
            FollowBox.X = avgPos.X;
            hardFollow = true;
        }
        else if (avgPos.X > FollowBox.X + FollowBox.Width)
        {
            FollowBox.X = avgPos.X - FollowBox.Width;
            hardFollow = true;
        }

        if (avgPos.Y < FollowBox.Y)
        {
            FollowBox.Y = avgPos.Y;
            hardFollow = true;
        }
        else if (avgPos.Y > FollowBox.Y + FollowBox.Height)
        {
            FollowBox.Y = avgPos.Y - FollowBox.Height;
            hardFollow = true;
        }

        if (hardFollow)
        {
            SoftFollowBox.Center = FollowBox.Center;
            Hitbox.Center = FollowBox.Center;
        }

        var softFollow = false;
        if (avgPos.X < SoftFollowBox.X)
        {
            SoftFollowBox.X -= (SoftFollowBox.X - avgPos.X) / 32;
            softFollow = true;
        }
        else if (avgPos.X  > SoftFollowBox.X + SoftFollowBox.Width)
        {
            SoftFollowBox.X += (avgPos.X - SoftFollowBox.X - SoftFollowBox.Width) / 32;
            softFollow = true;
        }
        if (avgPos.Y < SoftFollowBox.Y)
        {
            SoftFollowBox.Y -= (SoftFollowBox.Y - avgPos.Y) / 32;
            softFollow = true;
        }
        else if (avgPos.Y > SoftFollowBox.Y + SoftFollowBox.Height)
        {
            SoftFollowBox.Y += (avgPos.Y - SoftFollowBox.Y - SoftFollowBox.Height) / 32;
            softFollow = true;
        }

        if (softFollow)
        {
            FollowBox.Center = SoftFollowBox.Center;
            Hitbox.Center = SoftFollowBox.Center;
        }
    }

    public Vector2 GetScreenCoords(Vector2 pos) =>
        (pos - (Vector2)Hitbox) * ViewScale + viewScaleOffset;

    public RectangleF GetScreenCoords(RectangleF rect) =>
        new RectangleF(((Vector2)rect.Position - (Vector2)Hitbox) * ViewScale + viewScaleOffset, rect.Size * ViewScale);

    public Hitbox GetScreenCoords(Hitbox hb) =>
        new Hitbox(((Vector2)hb.XY - (Vector2)Hitbox) * ViewScale + viewScaleOffset, hb.Dimensions * ViewScale);


    public void Center()
    {
        Vector2 avgPos = new(TrackedObjects.Average(obj => obj.Hitbox.X), TrackedObjects.Average(obj => obj.Hitbox.Y));

        Hitbox.Center = avgPos;
        FollowBox.Center = Hitbox.Center;
        SoftFollowBox.Center = Hitbox.Center;
    }

    private void ResetFollowBoxSize()
    {
        var oldCenter = FollowBox.Center;
        FollowBox.Dimensions = new((graphics.Viewport.Width * 0.8f) / ViewScale, (graphics.Viewport.Height * 0.8f) / ViewScale);
        SoftFollowBox.Dimensions = new(SOFT_FOLLOW_WIDTH / ViewScale, SOFT_FOLLOW_HEIGHT / ViewScale);
        FollowBox.Center = oldCenter;
        SoftFollowBox.Center = oldCenter;
        Console.WriteLine(ViewScale);
    }
}

