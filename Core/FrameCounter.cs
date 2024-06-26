using System;
using Microsoft.Xna.Framework;

namespace Game2D;

public class FrameCounter
{
    private double currentFrametimes;
    private double weight;
    private int numerator;

    public int framerate
    {
        get
        {
            return (int)Math.Round(numerator / currentFrametimes);
        }
    }

    public FrameCounter(int oldFrameWeight)
    {
        numerator = oldFrameWeight;
        weight = (double)oldFrameWeight / (oldFrameWeight - 1.0);
    }

    public void Update(GameTime gameTime)
    {
        currentFrametimes = currentFrametimes / weight;
        currentFrametimes += gameTime.ElapsedGameTime.TotalSeconds;
    }

    public override string ToString() =>
        string.Format($"{framerate, 4} fps");
}
