using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Game2D;

public record struct InputBinding(Keys Left, Keys Right, Keys Up, Keys Down);

public class Input
{
    private const float DEADZONE = 0.5f;

    public KeyboardState Keyboard { get; private set; }
    public MouseState Mouse { get; private set; }
    public GamePadState Gamepad { get; private set; }

    private List<Keys> listenKeys = new();
    private List<Keys> downKeys = new();

    public bool IsKeyPressed(Keys key) =>
        downKeys.Contains(key) && Keyboard.IsKeyUp(key) && listenKeys.Contains(key);

    public bool IsKeyDown(Keys key) =>
        Keyboard.IsKeyDown(key) && listenKeys.Contains(key);

    public void AddListener(Keys key) {
        if (!listenKeys.Contains(key)) listenKeys.Add(key);
    }

    public void RemoveListener(Keys key) =>
        listenKeys.Remove(key);

    public void Update()
    {
        foreach (var key in listenKeys) {
            if (Keyboard.IsKeyDown(key) && !downKeys.Contains(key)) downKeys.Add(key);
        }

        Keyboard = Microsoft.Xna.Framework.Input.Keyboard.GetState();
        Mouse = Microsoft.Xna.Framework.Input.Mouse.GetState();
        Gamepad = GamePad.GetState(PlayerIndex.One);
    }

    public void FixedUpdate()
    {
        downKeys.Clear();
    }

    public Vector2 GetDigitalDirection(InputBinding binding)
    {
        Vector2 direction = new(0,0);

        if (Keyboard.IsKeyDown(binding.Left))
            direction.X -= 1;
        if (Keyboard.IsKeyDown(binding.Right))
            direction.X += 1;
        if (Keyboard.IsKeyDown(binding.Up))
            direction.Y -= 1;
        if (Keyboard.IsKeyDown(binding.Down))
            direction.Y += 1;

        return direction;
    }

    public Vector2 GetAnalogDirection(int playerIdx = 0)
    {
        Vector2 direction = new(0,0);
        var playerIndex = playerIdx == 0 ? PlayerIndex.One : PlayerIndex.Two;
        var capabilities = GamePad.GetCapabilities(playerIdx);
        if (!capabilities.IsConnected ||
            !capabilities.HasLeftXThumbStick ||
            !capabilities.HasLeftYThumbStick) return direction;

        if (Math.Abs(Gamepad.ThumbSticks.Left.X) > DEADZONE)
            direction.X += Gamepad.ThumbSticks.Left.X;
        if (Math.Abs(Gamepad.ThumbSticks.Left.Y) > DEADZONE)
            direction.X += Gamepad.ThumbSticks.Left.Y;

        return direction;
    }
}
