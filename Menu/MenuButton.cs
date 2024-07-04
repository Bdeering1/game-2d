using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Game2D;

public class MenuButton {

    private Input input { get; }
    private SpriteBatch spriteBatch { get; }

    private Rectangle ClickRect { get; set; }
    private Texture2D Texture { get; }
    private String Text { get; }
    private SpriteFont Font { get; }
    private OnClickCallback onClick { get; }
    private bool isClicked = false;
    private float scale { get; } = 1f;
    private Vector2 offset { get; }

    public delegate void OnClickCallback();

    public MenuButton(GameServiceContainer services, Rectangle clickRect, OnClickCallback onClick, String text = ""): this(
        services,
        clickRect,
        onClick,
        Utils.CreateRect(services.GetService<GraphicsDevice>(), clickRect.Width, clickRect.Height, Color.Aqua),
        text
    ) {}

    public MenuButton(GameServiceContainer services, Rectangle clickRect, OnClickCallback onClick, Texture2D texture, String text) {
        Texture = texture;
        ClickRect = clickRect;
        input = services.GetService<Input>();
        spriteBatch = services.GetService<SpriteBatch>();
        this.onClick = onClick;
        Text = text;

        Font = services.GetService<ContentManager>().Load<SpriteFont>("arial-50");

        scale = Math.Min(clickRect.Width / (Font.MeasureString(text).X + 10f), clickRect.Height / Font.MeasureString(text).Y);
        offset = (clickRect.Size.ToVector2() - Font.MeasureString(text) * scale) / 2f;
    }

    public void Update() {
        if (!isClicked && input.Mouse.LeftButton == ButtonState.Pressed && ClickRect.Contains(input.Mouse.Position)) {
            onClick();
            isClicked = true;
        } else if (input.Mouse.LeftButton == ButtonState.Released || !ClickRect.Contains(input.Mouse.Position)) {
            isClicked = false;
        }
    }

    public void Draw() {
        spriteBatch.Draw(Texture, ClickRect, Color.Brown);
        spriteBatch.DrawString(Font, Text, ClickRect.Location.ToVector2() + offset, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }

    public void SetPos(Vector2 pos) {
        ClickRect = ClickRect with {Location = pos.ToPoint()};
    }
}