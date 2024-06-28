using System;
using Microsoft.Xna.Framework;

namespace Game2D;

public class FrameCounter
{
    private int smoothingFactor;
    private double frameTimes;
    private double scalingFactor;

    public int framerate
    {
        get
        {
            return (int)Math.Round(smoothingFactor / frameTimes);
        }
    }

    public FrameCounter(int smoothingFactor)
    {
        this.smoothingFactor = smoothingFactor;
        scalingFactor = (smoothingFactor - 1.0) / (double)smoothingFactor;
    }

    public void Update(GameTime gameTime)
    {
        frameTimes = frameTimes * scalingFactor;
        frameTimes += gameTime.ElapsedGameTime.TotalSeconds;
    }

    public override string ToString() =>
        string.Format($"{framerate, 4}");
}
