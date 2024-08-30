using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game2D;

public class Camera
{
    private const int SOFT_FOLLOW_WIDTH = 150;
    private const int SOFT_FOLLOW_HEIGHT = 0;

    public Hitbox Hitbox { get; set; }
    public Hitbox FollowBox { get; set; }
    public Hitbox SoftFollowBox { get; set; }
    public IMovable TrackedObject { get; set; }

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
            new(graphics.Viewport.Width / 2, graphics.Viewport.Height / 2)
        );
        SoftFollowBox = new(
            new(0, 0),
            new(SOFT_FOLLOW_WIDTH, SOFT_FOLLOW_HEIGHT)
        );
        Hitbox.Center = FollowBox.Center;
    }

    public void Update(GameTime gameTime)
    {
        var hardFollow = false;
        if (TrackedObject.Hitbox.X < FollowBox.X)
        {
            FollowBox.X = TrackedObject.Hitbox.X;
            hardFollow = true;
        }
        else if (TrackedObject.Hitbox.X + TrackedObject.Hitbox.Width > FollowBox.X + FollowBox.Width)
        {
            FollowBox.X = TrackedObject.Hitbox.X + TrackedObject.Hitbox.Width - FollowBox.Width;
            hardFollow = true;
        }
        if (TrackedObject.Hitbox.Y < FollowBox.Y)
        {
            FollowBox.Y = TrackedObject.Hitbox.Y;
            hardFollow = true;
        }
        else if (TrackedObject.Hitbox.Y + TrackedObject.Hitbox.Height > FollowBox.Y + FollowBox.Height)
        {
            FollowBox.Y = TrackedObject.Hitbox.Y + TrackedObject.Hitbox.Height - FollowBox.Height;
            hardFollow = true;
        }

        if (hardFollow)
        {
            SoftFollowBox.Center = FollowBox.Center;
            Hitbox.Center = FollowBox.Center;
        }

        var softFollow = false;
        if (TrackedObject.Hitbox.Center.X < SoftFollowBox.X)
        {
            SoftFollowBox.X--;
            softFollow = true;
        }
        else if (TrackedObject.Hitbox.Center.X > SoftFollowBox.X + SoftFollowBox.Width)
        {
            SoftFollowBox.X++;
            softFollow = true;
        }
        if (TrackedObject.Hitbox.Center.Y < SoftFollowBox.Y)
        {
            SoftFollowBox.Y--;
            softFollow = true;
        }
        else if (TrackedObject.Hitbox.Center.Y > SoftFollowBox.Y + SoftFollowBox.Height)
        {
            SoftFollowBox.Y++;
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
        Hitbox.Center = TrackedObject.Hitbox.Center;
        FollowBox.Center = Hitbox.Center;
        SoftFollowBox.Center = Hitbox.Center;
    }
}
