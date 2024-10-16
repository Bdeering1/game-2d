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

    private const int ADD_CHUNK_BTN_W = 100;
    private const long INTERACTION_COOLDOWN = 500;

    private GameServiceContainer services { get; }
    private Input input { get; }
    private GraphicsDevice graphics { get; }
    private SpriteBatch spriteBatch { get; }
    private MapTextures mapTextures { get; }

    private Stage stage;
    private List<Chunk> chunks;
    private Chunk focusedChunk;
    private InputBinding bindings = new(Keys.Left, Keys.Right, Keys.Up, Keys.Down);

    private Rectangle bgRect;
    private Rectangle textureSelector;
    private int textureSize { get; }

    private MenuButton exitButton;
    public bool exiting;
    public long timeSinceInteract;

    private Rectangle gridEditor;
    private Vector2 offset;
    private Vector2 offsetRounded;
    private List<(Rectangle, Vector2)> addChunkButtons = new();

    private uint selectedTexture = 0;

    public LevelEditor(GameServiceContainer services, Stage stage) {
        this.services = services;
        input = services.GetService<Input>();
        spriteBatch = services.GetService<SpriteBatch>();
        graphics = services.GetService<GraphicsDevice>();
        mapTextures = services.GetService<MapTextures>();
        textureSize = (int)services.GetService<ConfigurationService>().GetValue("tile", "size");

        input.AddListener(Keys.D);
        bgRect = Utils.ToAbsolute(graphics.Viewport.Bounds, new RectangleF(0.02f, 0.02f, 0.96f, 0.96f));
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
        gridEditor = Utils.ToAbsolute(bgRect, new RectangleF(0.02f, 0.02f, 0.96f, 0.96f));

        this.stage = stage;
        chunks = stage.Chunks;
    }
    
    public void Update(GameTime gameTime) {
        timeSinceInteract += gameTime.ElapsedGameTime.Milliseconds;

        var leftClick = input.Mouse.LeftButton == ButtonState.Pressed;
        var rightClick = input.Mouse.RightButton == ButtonState.Pressed;
        if ((leftClick || rightClick) && timeSinceInteract > INTERACTION_COOLDOWN) 
        {
            var mousePos = input.Mouse.Position;
            if (textureSelector.Contains(mousePos))
            {
                selectedTexture = (uint)((mousePos.Y - textureSelector.Y) / textureSize);
            }
            AddTiles(mousePos, leftClick);
        }

        //chunk focusing
        Chunk mostInView = chunks[0];
        int mostInViewOverlap = Utils.AreaOfOverlap(gridEditor, chunks[0].ChunkBounds with { Location = chunks[0].ChunkBounds.Location + offsetRounded.ToPoint() + gridEditor.Location });
        foreach (Chunk c in chunks) {
            int overlap = Utils.AreaOfOverlap(gridEditor, c.ChunkBounds with { Location = c.ChunkBounds.Location + offsetRounded.ToPoint() + gridEditor.Location });
            if (overlap > mostInViewOverlap)
            {
                mostInView = c;
                mostInViewOverlap = overlap;
            }
        }
        if (mostInViewOverlap > 0)
            focusedChunk = mostInView;
        else focusedChunk = null;
        //end chunk focusing

        //chunk deletion
        if (input.IsKeyPressed(Keys.D) && timeSinceInteract > INTERACTION_COOLDOWN)
        {
            timeSinceInteract = 0;
            chunks.Remove(focusedChunk);
        }

        //editor movement (i.e arrow key input)
        var dir = input.GetDigitalDirection(bindings);
        if (dir != Vector2.Zero)
        {
            //diagonal
            if (Math.Abs(dir.X) < 1 && Math.Abs(dir.Y) < 1) 
            {
                //just makes the vector equal to (1, 1) instead of ~(0.718, 0.718) (non-unit vector)
                offset -= new Vector2((float)(dir.X * Math.Sqrt(2)) * 0.1f, (float)(dir.Y * Math.Sqrt(2)) * 0.1f);
            } 
            else 
            { //not diagonal
                offset -= new Vector2(dir.X * 0.1f, dir.Y * 0.1f);
            }
            offsetRounded = new Vector2((int)offset.X * textureSize, (int)offset.Y * textureSize);
        }
        //end editor movement

        //update editor size
        bgRect = Utils.ToAbsolute(graphics.Viewport.Bounds, new RectangleF(0.02f, 0.02f, 0.96f, 0.96f));
        gridEditor = Utils.ToAbsolute(bgRect, new RectangleF(0.02f, 0.02f, 0.96f, 0.96f));
        textureSelector = new Rectangle(Utils.ToAbsolute(bgRect, new Vector2(0.95f, 0.05f)).ToPoint(),
                                        new Vector2(textureSize, NUM_TEXTURES * textureSize).ToPoint());
        exitButton.SetPos(Utils.ToAbsolute(bgRect, new Vector2(0.02f, 0.02f)));
        exitButton.Update();

        //update chunk adding buttons
        UpdateAddChunkButtons();
    }

    public void Draw() {
        //backgrounds
        spriteBatch.FillRectangle(bgRect, Color.DarkBlue);
        spriteBatch.FillRectangle(gridEditor, Color.DimGray);
        foreach (Chunk c in chunks)
        {
            foreach (Tile t in c.Tiles)
            {
                var tileRect = new Rectangle((int)(t.X + c.Offset.X + offsetRounded.X + gridEditor.X),
                                             (int)(t.Y + c.Offset.Y + offsetRounded.Y + gridEditor.Y),
                                             textureSize, textureSize);
                if (tileRect.Intersects(gridEditor))
                    spriteBatch.Draw(mapTextures.GetTexture(t.TextureID), tileRect, Color.White);
            }
            if(c == focusedChunk) 
            {
                // HighlightChunk(c.ChunkBounds with { Location = c.ChunkBounds.Location + gridEditor.Location + offsetRounded.ToPoint()}, gridEditor);
            }
            DrawRectInEditor(c.ChunkBounds with { Location = c.ChunkBounds.Location + gridEditor.Location + offsetRounded.ToPoint()}, gridEditor, c == focusedChunk ? Color.Green : Color.Red);
        }
        
        // Show slightly transparent block at the position where a block would be placed
        var (mouseHoveredChunk, mouseHoveredX, mouseHoveredY) = GetMousePosInChunk(input.Mouse.Position);
        if (mouseHoveredChunk != null) {
            var tileRect = new Rectangle((int)(mouseHoveredX + offsetRounded.X + gridEditor.X + mouseHoveredChunk.Offset.X),
                                         (int)(mouseHoveredY + offsetRounded.Y + gridEditor.Y + mouseHoveredChunk.Offset.Y),
                                         textureSize, textureSize);
            spriteBatch.Draw(mapTextures.GetTexture((uint)selectedTexture), tileRect, Color.White * 0.5f);
        }

        DrawAddChunkButtons();
        spriteBatch.FillRectangle(textureSelector, Color.Blue);
        for (var i = 0; i < NUM_TEXTURES; i++)
        {
            spriteBatch.Draw(mapTextures.GetTexture((uint)i), new Rectangle(textureSelector.X, textureSelector.Y + i * textureSize, textureSize, textureSize), Color.White);
        }
        spriteBatch.DrawRectangle(
            new (textureSelector.X, textureSelector.Y + selectedTexture * textureSize, textureSelector.Width, textureSize),
            Color.Green
        );
        exitButton.Draw();
    }

    private void AddTiles(Point mousePos, bool leftClick) {
        if(gridEditor.Contains(mousePos)) 
        {
            var (chk, x, y) = GetMousePosInChunk(mousePos);
            if (chk != null)
            {
                if(chk.HasTileAt(x, y))
                {
                    chk.RemoveTileAt(x, y);
                }
                if (leftClick) chk.AddTile(selectedTexture, x, y, CollisionType.Impassable);
            }
            foreach ((Rectangle btn, Vector2 chunkOffset) in addChunkButtons) {
                if (btn.Contains(mousePos))
                {
                    chunks.Add(new Chunk(services, chunkOffset));
                    timeSinceInteract = 0;
                }
            }
        }
    }

    private (Chunk, int, int) GetMousePosInChunk(Point mousePos) {
        if (gridEditor.Contains(mousePos))
        {
            foreach (Chunk c in chunks) 
            {
                var adjustedChunkBounds = c.ChunkBounds with { Location = c.ChunkBounds.Location + offsetRounded.ToPoint() + gridEditor.Location};
                if (adjustedChunkBounds.Contains(mousePos)) {
                    int xPos = (((mousePos.X - adjustedChunkBounds.X) % adjustedChunkBounds.Width) / textureSize) * textureSize;
                    int yPos = (((mousePos.Y - adjustedChunkBounds.Y) % adjustedChunkBounds.Height) / textureSize) * textureSize;
                    return (c, xPos, yPos);
                }
            }
            return (null, 0, 0);
        }

        return (null, -1, -1);
    }

    private void DrawAddChunkButtons() {
        foreach ((Rectangle, Vector2) btn in addChunkButtons)
                {
            spriteBatch.FillRectangle(btn.Item1, Color.Gray);
        }
    }

    private void UpdateAddChunkButtons() {
        addChunkButtons = [];
        foreach (Chunk c in chunks) {
            Rectangle bounds = c.ChunkBounds with { Location = c.ChunkBounds.Location + gridEditor.Location + offsetRounded.ToPoint()};
            Vector2 offsetAdj = c.Offset / textureSize;
            (Hitbox, Vector2)[] buttons = [
                (
                    new Hitbox(new Vector2(bounds.Center.X - ADD_CHUNK_BTN_W/2, bounds.Top - textureSize), new Vector2(ADD_CHUNK_BTN_W, textureSize)),
                    new Vector2(offsetAdj.X, offsetAdj.Y - Chunk.CHUNK_HEIGHT)
                ),
                (
                    new Hitbox(new Vector2(bounds.Center.X - ADD_CHUNK_BTN_W/2, bounds.Bottom), new Vector2(ADD_CHUNK_BTN_W, textureSize)),
                    new Vector2(offsetAdj.X, offsetAdj.Y + Chunk.CHUNK_HEIGHT)
                ),
                (
                    new Hitbox(new Vector2(bounds.Left - textureSize, bounds.Center.Y - textureSize/2), new Vector2(textureSize, ADD_CHUNK_BTN_W)),
                    new Vector2(offsetAdj.X - Chunk.CHUNK_WIDTH, offsetAdj.Y)
                ),
                (
                    new Hitbox(new Vector2(bounds.Right, bounds.Center.Y - textureSize/2), new Vector2(textureSize, ADD_CHUNK_BTN_W)),
                    new Vector2(offsetAdj.X + Chunk.CHUNK_WIDTH, offsetAdj.Y)
                )
            ];

            bool intersectsChunk(Hitbox hb)
            {
                foreach (Chunk c in chunks)
                {
                    if (hb.IsInside(c.ChunkBounds with { Location = c.ChunkBounds.Location + gridEditor.Location + offsetRounded.ToPoint() }))
                    {
                        return true;
                    }
                }
                return false;
            }

            foreach ((Hitbox, Vector2) btn in buttons)
            {
                if(!intersectsChunk(btn.Item1) && btn.Item1.IsInside(gridEditor))
                {
                    addChunkButtons.Add(((Rectangle)btn.Item1, btn.Item2));
                }
            }
        }
    }

    private void DrawRectInEditor(Rectangle bounds, Rectangle editor, Color color) {
        if (!bounds.Intersects(editor)) return;
        //left
        if (bounds.Left > editor.Left)
            spriteBatch.DrawLine(bounds.X, Math.Max(bounds.Y, editor.Y), bounds.X, Math.Min(bounds.Bottom, editor.Bottom), color);
        //right
        if (bounds.Right < editor.Right)
            spriteBatch.DrawLine(bounds.Right, Math.Max(bounds.Y, editor.Y), bounds.Right, Math.Min(bounds.Bottom, editor.Bottom), color);
        //top
        if (bounds.Top > editor.Top)
            spriteBatch.DrawLine(Math.Max(bounds.Left, editor.Left), bounds.Top, Math.Min(bounds.Right, editor.Right), bounds.Top, color);
        if (bounds.Bottom < editor.Bottom)
            spriteBatch.DrawLine(Math.Max(bounds.Left, editor.Left), bounds.Bottom, Math.Min(bounds.Right, editor.Right), bounds.Bottom, color);
    }

    private void HighlightChunk(Rectangle chunk, Rectangle editor) {
        spriteBatch.FillRectangle(
            Math.Max(chunk.Left, editor.Left), Math.Max(chunk.Top, editor.Top),
            Math.Min(chunk.Right, editor.Right) - Math.Max(chunk.Left, editor.Left),
            Math.Min(chunk.Bottom, editor.Bottom) - Math.Max(chunk.Top, editor.Top),
            Color.LightGreen * 0.1f);
    }

    public void Exit() {
        exiting = true;

        //purge empty chunks
        List<Chunk> cToRemove = [];
        for (int i = 0; i < chunks.Count; i++) {
            if (chunks[i].Tiles.Count == 0) {
                cToRemove.Add(chunks[i]);
            }
        }
        foreach (Chunk c in cToRemove) {
            chunks.Remove(c);
        }

        stage.Write();
        stage.Reload();
    }
}
