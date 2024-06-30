using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Game2D;

public class Animations
{
    public Vector2 Offset { get; }
    public SpriteSheet Sheet { get; }
    public Rectangle ClipRect;

    private double timeSinceFrame;
    private int curAnim;
    private int curFrame = 0;
    //x/y index of the current animation frame
    private int x;
    private int y;

    public Animations(GameServiceContainer services, string img, List<(int fps, int startX, int startY, int numFrames)> anims, int spriteWidth, int spriteHeight, Vector2 offset)
    {
        Sheet = new(services, img, anims, spriteWidth, spriteHeight);
        ClipRect = new Rectangle(0, 0, spriteWidth, spriteHeight);
        Offset = offset;

        curAnim = 0;
        x = 0;
        y = 0;
    }

    public void Update(GameTime gameTime)
    {
        timeSinceFrame += gameTime.ElapsedGameTime.TotalNanoseconds;
        var (startX, startY, numFrames, timePerFrame) = Sheet.anims[curAnim];

        //update frame (skip frames if needed)
        while (timeSinceFrame > timePerFrame)
        {
            curFrame++;

            if (curFrame >= numFrames)
            {
                x = startX;
                y = startY;
                curFrame = 0;
            }
            x = curFrame % Sheet.sheetWidth;
            y = curFrame / Sheet.sheetWidth;

            ClipRect.X = x * Sheet.spriteWidth + 1;
            ClipRect.Y = y * Sheet.spriteHeight + 1;
            timeSinceFrame -= timePerFrame;
        }
    }
}

public struct SpriteSheet
{
    public Texture2D img;
    public int spriteWidth;
    public int spriteHeight;

    //number of columns in this spritesheet
    public int sheetWidth;

    public List<(int startX, int startY, int numFrames, int timePerFrame)> anims;

    public SpriteSheet(GameServiceContainer services, string img, List<(int fps, int startX, int startY, int numFrames)> anims, int spriteWidth, int spriteHeight)
    {
        this.img = services.GetService<ContentManager>().Load<Texture2D>(img);
        this.spriteWidth = spriteWidth;
        this.spriteHeight = spriteHeight;

        sheetWidth = this.img.Width / spriteWidth;

        this.anims = [];
        foreach (var (fps, startX, startY, numFrames) in anims)
        {
            this.anims.Add((startX, startY, numFrames, (int)1e9 / fps));
        }
    }
}
