using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Game2D;

public class Input
{
    private const float DEADZONE = 0.5f;

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
        Vector2 direction = new(0,0);
        var capabilities = GamePad.GetCapabilities(PlayerIndex.One);
        if (!capabilities.IsConnected ||
            !capabilities.HasLeftXThumbStick ||
            !capabilities.HasLeftYThumbStick) return direction;

        if (Math.Abs(gamepad.ThumbSticks.Left.X) > DEADZONE)
            direction.X += gamepad.ThumbSticks.Left.X;
        if (Math.Abs(gamepad.ThumbSticks.Left.Y) > DEADZONE)
            direction.X += gamepad.ThumbSticks.Left.Y;

        return direction;
    }

    public void Update() {
        keyboard = Keyboard.GetState();
        mouse = Mouse.GetState();
        gamepad = GamePad.GetState(PlayerIndex.One);
    }
}
