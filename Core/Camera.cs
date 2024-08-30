using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game2D;

public class Camera
{
    public Hitbox Hitbox { get; set; }
    public Hitbox FollowBox { get; set; }
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
            new(graphics.Viewport.Width / 4, graphics.Viewport.Height / 4)
        );
        Hitbox.Center = FollowBox.Center;
    }

    public void Update(GameTime gameTime)
    {
        if (TrackedObject.Hitbox.X < FollowBox.X)
        {
            FollowBox.X = TrackedObject.Hitbox.X;
            Hitbox.Center = FollowBox.Center;
        }
        else if (TrackedObject.Hitbox.X + TrackedObject.Hitbox.Width > FollowBox.X + FollowBox.Width)
        {
            FollowBox.X = TrackedObject.Hitbox.X + TrackedObject.Hitbox.Width - FollowBox.Width;
            Hitbox.Center = FollowBox.Center;
        }
        if (TrackedObject.Hitbox.Y < FollowBox.Y)
        {
            FollowBox.Y = TrackedObject.Hitbox.Y;
            Hitbox.Center = FollowBox.Center;
        }
        else if (TrackedObject.Hitbox.Y + TrackedObject.Hitbox.Height > FollowBox.Y + FollowBox.Height)
        {
            FollowBox.Y = TrackedObject.Hitbox.Y + TrackedObject.Hitbox.Height - FollowBox.Height;
            Hitbox.Center = FollowBox.Center;
        }
    }

    public void Center()
    {
        Hitbox.Center = TrackedObject.Hitbox.Center;
        FollowBox.Center = TrackedObject.Hitbox.Center;
    }
}
