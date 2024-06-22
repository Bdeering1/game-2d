using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Game2D;

public class Input
{
    private KeyboardState keyboard;
    private MouseState mouse;
    private GamePadState gamepad;

    public Vector2 GetDigitalDirection()
    {
        Vector2 direction = new(0,0);

        if (keyboard.IsKeyDown(Keys.Left))
            direction.X -= 1;
        if (keyboard.IsKeyDown(Keys.Right))
            direction.X += 1;
        if (keyboard.IsKeyDown(Keys.Up))
            direction.Y -= 1;
        if (keyboard.IsKeyDown(Keys.Down))
            direction.Y += 1;

        return direction;
    }

    public Vector2 GetAnalogDirection()
    {
        return new(0,0);
    }

    public void Update() {
        keyboard = Keyboard.GetState();
        mouse = Mouse.GetState();
        gamepad = GamePad.GetState(0);
    }
}
