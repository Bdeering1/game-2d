using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace Game2D;

public class LevelEditor {

    private const int EDITOR_WIDTH = 750;
    private const int EDITOR_HEIGHT = 600;
    private const int NUM_TEXTURES = 10;

    private Input input { get; }
    private GraphicsDevice graphics { get; }
    private SpriteBatch spriteBatch { get; }
    private MapTextures mapTextures { get; }

    private Rectangle bgRect;
    private Rectangle textureSelector;
    private int textureSize { get; }

    private MenuButton exitButton;
    public bool exiting;

    private Rectangle gridEditor;
    private List<Chunk> chunks;
    private Vector2 offset;

    private int selectedTexture = 0;

    public LevelEditor(GameServiceContainer services) {
        input = services.GetService<Input>();
        spriteBatch = services.GetService<SpriteBatch>();
        graphics = services.GetService<GraphicsDevice>();
        mapTextures = services.GetService<MapTextures>();
        textureSize = (int)services.GetService<ConfigurationService>().GetValue("tile", "size");

        bgRect = Utils.ToAbsolute(graphics.Viewport.Bounds, new RectangleF(0.05f, 0.05f, 0.9f, 0.9f));
        textureSelector = new Rectangle(Utils.ToAbsolute(bgRect, new Vector2(0.95f, 0.05f)).ToPoint(),
                                        new Point(textureSize, NUM_TEXTURES * textureSize));
        exitButton = new MenuButton(services,
                                    new Rectangle(
                                            Utils.ToAbsolute(bgRect, new Vector2(0.02f, 0.02f)).ToPoint(),
                                            new Point(textureSize, textureSize)
                                            ),
                                    Exit,
                                    "X"
                                );
        gridEditor = new Rectangle(Utils.ToAbsolute(bgRect, new Vector2(0.1f, 0.1f)).ToPoint(),
                                new (Chunk.CHUNK_WIDTH * textureSize, Chunk.CHUNK_HEIGHT * textureSize));

        //load in chunks from file here
        chunks = [new Chunk(services)];
    }
    
    public void Update(GameTime gameTime) {
        if(input.Mouse.LeftButton == ButtonState.Pressed) {
            var mousePos = input.Mouse.Position;
            if (textureSelector.Contains(mousePos)) {
                selectedTexture = (mousePos.Y - textureSelector.Y) / textureSize;
            }

            if(gridEditor.Contains(mousePos)) {
                foreach (Chunk c in chunks) {
                    if (c.ChunkBounds.Contains(mousePos - gridEditor.Location)) {
                        int xPos = (int)(mousePos.X - gridEditor.X - c.Offset.X) / textureSize;
                        int yPos = (int)(mousePos.Y - gridEditor.Y - c.Offset.Y) / textureSize;

                        if(c.HasTileAt(xPos, yPos)) {
                            c.RemoveTileAt(xPos, yPos);
                        }
                        c.AddTile(selectedTexture, xPos, yPos, CollisionType.Impassable);
                    }
                }
            }
        }

        var dir = input.GetDigitalDirection();
        if (dir != Vector2.Zero) {
            //diagonal
            if (Math.Abs(dir.X) < 1 && Math.Abs(dir.Y) < 1) 
            {
                offset -= new Vector2((float)(dir.X * Math.Sqrt(2)) * 0.1f, (float)(dir.Y * Math.Sqrt(2)) * 0.1f);
            } 
            else 
            { //not diagonal
                offset -= new Vector2(dir.X * 0.1f, dir.Y * 0.1f);
            }
        }

        foreach(Chunk c in chunks) {
            var offsetRounded = new Vector2((int)offset.X, (int)offset.Y);
            c.Offset = offsetRounded * textureSize;
            c.ChunkBounds = c.ChunkBounds with {Location = c.Offset.ToPoint()};
        }

        //update editor size
        bgRect = Utils.ToAbsolute(graphics.Viewport.Bounds, new RectangleF(0.05f, 0.05f, 0.9f, 0.9f));
        textureSelector = new Rectangle(Utils.ToAbsolute(bgRect, new Vector2(0.95f, 0.05f)).ToPoint(),
                                        new Vector2(textureSize, NUM_TEXTURES * textureSize).ToPoint());
        exitButton.SetPos(Utils.ToAbsolute(bgRect, new Vector2(0.02f, 0.02f)));
        exitButton.Update();
    }

    public void Draw() {
        spriteBatch.FillRectangle(bgRect, Color.DarkBlue);
        spriteBatch.FillRectangle(gridEditor, Color.DimGray);
        foreach (Chunk c in chunks) {
            foreach (Tile t in c.Tiles) {
                var tileRect = new Rectangle(
                                                (int)(t.X * textureSize + c.Offset.X + gridEditor.X),
                                                (int)(t.Y * textureSize + c.Offset.Y + gridEditor.Y), 
                                                textureSize, textureSize);
                if (tileRect.Intersects(gridEditor))
                    spriteBatch.Draw(t.Texture, tileRect, Color.White);
            }
            drawRectInEditor(c.ChunkBounds with { Location = c.ChunkBounds.Location + gridEditor.Location}, gridEditor);
        }
        spriteBatch.FillRectangle(textureSelector, Color.Blue);
        for (int i = 0; i < NUM_TEXTURES; i++) {
            spriteBatch.Draw(mapTextures.GetTexture(i), new Rectangle(textureSelector.X, textureSelector.Y + i * textureSize, textureSize, textureSize), Color.White);
        }
        spriteBatch.DrawRectangle(
            new (textureSelector.X, textureSelector.Y + selectedTexture * textureSize, textureSelector.Width, textureSize),
            Color.Green
        );
        exitButton.Draw();
    }

    private void drawRectInEditor(Rectangle bounds, Rectangle editor) {
        if (!bounds.Intersects(editor)) return;
        //left
        if (bounds.Left > editor.Left)
            spriteBatch.DrawLine(bounds.X, Math.Max(bounds.Y, editor.Y), bounds.X, Math.Min(bounds.Bottom, editor.Bottom), Color.Red);
        //right
        if (bounds.Right < editor.Right)
            spriteBatch.DrawLine(bounds.Right, Math.Max(bounds.Y, editor.Y), bounds.Right, Math.Min(bounds.Bottom, editor.Bottom), Color.Red);
        //top
        if (bounds.Top > editor.Top)
            spriteBatch.DrawLine(Math.Max(bounds.Left, editor.Left), bounds.Top, Math.Min(bounds.Right, editor.Right), bounds.Top, Color.Red);
        if (bounds.Bottom < editor.Bottom)
            spriteBatch.DrawLine(Math.Max(bounds.Left, editor.Left), bounds.Bottom, Math.Min(bounds.Right, editor.Right), bounds.Bottom, Color.Red);

    }

    public void Exit() {
        exiting = true;
    }
}