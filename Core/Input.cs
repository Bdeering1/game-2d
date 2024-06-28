using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Game2D;

public class Input
{
    private const float DEADZONE = 0.5f;

    public KeyboardState Keyboard { get; private set; }
    public MouseState Mouse { get; private set; }
    public GamePadState Gamepad { get; private set; }

    private KeyboardState prevKeyboard;
    private GamePadState prevGamepad;

    public bool IsKeyPressed(Keys key) =>
        prevKeyboard.IsKeyDown(key) && Keyboard.IsKeyUp(key);

    public Vector2 GetDigitalDirection()
    {
        Vector2 direction = new(0,0);

        if (Keyboard.IsKeyDown(Keys.Left))
            direction.X -= 1;
        if (Keyboard.IsKeyDown(Keys.Right))
            direction.X += 1;
        if (Keyboard.IsKeyDown(Keys.Up))
            direction.Y -= 1;
        if (Keyboard.IsKeyDown(Keys.Down))
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

        if (Math.Abs(Gamepad.ThumbSticks.Left.X) > DEADZONE)
            direction.X += Gamepad.ThumbSticks.Left.X;
        if (Math.Abs(Gamepad.ThumbSticks.Left.Y) > DEADZONE)
            direction.X += Gamepad.ThumbSticks.Left.Y;

        return direction;
    }

    public void Update() {
        prevKeyboard = Keyboard;
        prevGamepad = Gamepad;

        Keyboard = Microsoft.Xna.Framework.Input.Keyboard.GetState();
        Mouse = Microsoft.Xna.Framework.Input.Mouse.GetState();
        Gamepad = GamePad.GetState(PlayerIndex.One);
    }
}
