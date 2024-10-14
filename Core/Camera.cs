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

    public Hitbox Hitbox { get; set; }
    public Hitbox FollowBox { get; set; }
    public Hitbox SoftFollowBox { get; set; }
    public List<IMovable> TrackedObjects { get; set; }
    public Vector2 TrackedObjectsSize { get; set; }

    public float viewScale { get; set; }
    public Vector2 viewScaleOffset { get; set; }

    private GraphicsDevice graphics { get; }

    public Camera(GraphicsDevice graphics)
    {
        this.graphics = graphics;

        Hitbox = new(
            new(0, 0),
            new(graphics.Viewport.Width, graphics.Viewport.Height)
        );
        FollowBox = new( 
            new(0, 0),
            new(graphics.Viewport.Width / 4, graphics.Viewport.Height / 4)
        );
        SoftFollowBox = new(
            new(0, 0),
            new(SOFT_FOLLOW_WIDTH, SOFT_FOLLOW_HEIGHT)
        );
        Hitbox.Center = FollowBox.Center;

        viewScale = 1f;
        viewScaleOffset = new((1200 - Hitbox.Width * viewScale) / 2, (900 - Hitbox.Height * viewScale) / 2);
    }

    public void Update(GameTime gameTime)
    {
        foreach(IMovable obj in TrackedObjects)
        {
            var screenCoords = GetScreenCoords(obj.Hitbox.Center);
            if (screenCoords.X <= 75 || screenCoords.Y <= 75 || screenCoords.X > 1125 || screenCoords.Y > 825)
            {
                var diff = Math.Abs(Math.Min(Math.Min(screenCoords.X - 75, 1125 - screenCoords.X), Math.Min(screenCoords.Y - 75, 825 - screenCoords.Y)));
                if (viewScale >= 0.4) {
                    viewScale -= 0.00002f * diff;
                    viewScaleOffset = new((1200-Hitbox.Width * viewScale) / 2, (900 - Hitbox.Height * viewScale) / 2);
                }
            }
        }
        
        Vector2 avgPos = new(TrackedObjects.Average(obj => obj.Hitbox.XY.X), TrackedObjects.Average(obj => obj.Hitbox.XY.Y));

        var hardFollow = false;
        if (avgPos.X < FollowBox.X)
        {
            FollowBox.X = avgPos.X;
            hardFollow = true;
        }
        else if (avgPos.X + TrackedObjectsSize.X > FollowBox.X + FollowBox.Width)
        {
            FollowBox.X = avgPos.X + TrackedObjectsSize.X - FollowBox.Width;
            hardFollow = true;
        }

        if (avgPos.Y < FollowBox.Y)
        {
            FollowBox.Y = avgPos.Y;
            hardFollow = true;
        }
        else if (avgPos.Y + TrackedObjectsSize.Y > FollowBox.Y + FollowBox.Height)
        {
            FollowBox.Y = avgPos.Y + TrackedObjectsSize.Y - FollowBox.Height;
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
        else if (avgPos.X + TrackedObjectsSize.X > SoftFollowBox.X + SoftFollowBox.Width)
        {
            SoftFollowBox.X += (avgPos.X + TrackedObjectsSize.X - SoftFollowBox.X - SoftFollowBox.Width) / 32;
            softFollow = true;
        }
        if (avgPos.Y < SoftFollowBox.Y)
        {
            SoftFollowBox.Y -= (SoftFollowBox.Y - avgPos.Y) / 32;
            softFollow = true;
        }
        else if (avgPos.Y + TrackedObjectsSize.Y > SoftFollowBox.Y + SoftFollowBox.Height)
        {
            SoftFollowBox.Y += (avgPos.Y + TrackedObjectsSize.Y - SoftFollowBox.Y - SoftFollowBox.Height) / 32;
            softFollow = true;
        }

        if (softFollow)
        {
            FollowBox.Center = SoftFollowBox.Center;
            Hitbox.Center = SoftFollowBox.Center;
        }
    }

    public Vector2 GetScreenCoords(Vector2 pos) =>
        (pos - Hitbox) * viewScale + viewScaleOffset;

    public RectangleF GetScreenCoords(RectangleF rect) =>
        new RectangleF(((Vector2)rect.Position - Hitbox) * viewScale + viewScaleOffset, rect.Size * viewScale);

    public Hitbox GetScreenCoords(Hitbox hb) =>
        new Hitbox(((Vector2)hb.XY - Hitbox) * viewScale + viewScaleOffset, hb.Dimensions * viewScale);


    public void Center()
    {
        Vector2 avgPos = new(TrackedObjects.Average(obj => obj.Hitbox.X), TrackedObjects.Average(obj => obj.Hitbox.Y));

        Hitbox.Center = avgPos;
        FollowBox.Center = Hitbox.Center;
        SoftFollowBox.Center = Hitbox.Center;
    }
}

