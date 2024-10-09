using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

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
    }

    public void Update(GameTime gameTime)
    {
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
            SoftFollowBox.X -= (SoftFollowBox.X - avgPos.X)/32;
            softFollow = true;
        }
        else if (avgPos.X + TrackedObjectsSize.X > SoftFollowBox.X + SoftFollowBox.Width)
        {
            SoftFollowBox.X += (avgPos.X + TrackedObjectsSize.X - SoftFollowBox.X - SoftFollowBox.Width)/32;
            softFollow = true;
        }
        if (avgPos.Y < SoftFollowBox.Y)
        {
            SoftFollowBox.Y -= (SoftFollowBox.Y - avgPos.Y)/32;
            softFollow = true;
        }
        else if (avgPos.Y + TrackedObjectsSize.Y > SoftFollowBox.Y + SoftFollowBox.Height)
        {
            SoftFollowBox.Y += (avgPos.Y + TrackedObjectsSize.Y - SoftFollowBox.Y - SoftFollowBox.Height)/32;
            softFollow = true;
        }

        if (softFollow)
        {
            FollowBox.Center = SoftFollowBox.Center;
            Hitbox.Center = SoftFollowBox.Center;
        }
    }

    public void Center()
    {
		Vector2 avgPos = new(TrackedObjects.Average(obj => obj.Hitbox.XY.X), TrackedObjects.Average(obj => obj.Hitbox.XY.Y));

        Hitbox.Center = avgPos;
        FollowBox.Center = Hitbox.Center;
        SoftFollowBox.Center = Hitbox.Center;
    }
}

