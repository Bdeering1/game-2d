using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game2D;

public class Camera
{
    public Hitbox Position { get; set; }
    public IMovable TrackedObject { get; set; }

    private GraphicsDevice graphics { get; }

    public Camera(GraphicsDevice graphics)
    {
        this.graphics = graphics;

        Position = new(
            new(0, 0),
            new(graphics.Viewport.Width, graphics.Viewport.Height)
        );
    }

    public void Update(GameTime gameTime)
    {
        Position.Center = TrackedObject.Position.Center;
    }
}
