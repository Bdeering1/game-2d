using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using MonoGame.Extended;

namespace Game2D;

public class Camera
{
    private const float FOLLOW_BOX_SIZE = 0.80f;
    private const float ZOOM_BOX_SIZE = 0.6f;
    private const float INNER_ZOOM_BOX_SIZE = 0.55f;
    private const int SOFT_FOLLOW_WIDTH = 120;
    private const int SOFT_FOLLOW_HEIGHT = 60;
    private const int SOFT_FOLLOW_TOLERANCE = 32;
    private const int ZOOM_INTERPOLATION_FRAMES = 150;
    private readonly float[] ZOOM_LEVELS =
        [
            1f,
            0.75f,   // 18/24
            0.625f,  // 15/24
            0.5f,    // 12/24
        ];

    public Hitbox Hitbox { get; set; }
    public Hitbox FollowBox { get; set; }
    public Hitbox SoftFollowBox { get; set; }
    public Hitbox ZoomBox { get; set; }
    public Hitbox InnerZoomBox { get; set; }

    public List<IMovable> TrackedObjects { get; set; }
    public Vector2 TrackedPosition { get; private set; }

    public float ViewScale { get; set; } = 1f;
    private Vector2 viewScaleOffset;
    private int viewScaleIdx = 0;
    private int zoomProgress = 0;
    private bool zoomingOut = false;
    private bool zoomingIn = false;

    private GraphicsDevice graphics { get; }

    public Camera(GraphicsDevice graphics)
    {
        this.graphics = graphics;

        FollowBox = new();
        SoftFollowBox = new();
        ZoomBox = new();
        InnerZoomBox = new();
        ResizeFollowBoxes();

        Hitbox = new(
            new(0, 0),
            new(graphics.Viewport.Width, graphics.Viewport.Height)
        );
        Hitbox.Center = FollowBox.Center;

        SetViewScaleOffset();
    }

    public void Update(GameTime gameTime)
    {
        var hardFollow = false;
        var outsideCount = 0;
        var insideCount = 0;

        foreach(IMovable obj in TrackedObjects)
        {
            if (!zoomingIn && viewScaleIdx + 1 < ZOOM_LEVELS.Length
            && (obj.Hitbox.X < ZoomBox.X
             || obj.Hitbox.Y < ZoomBox.Y
             || obj.Hitbox.X + obj.Hitbox.Width > ZoomBox.X + ZoomBox.Width
             || obj.Hitbox.Y + obj.Hitbox.Height > ZoomBox.Y + ZoomBox.Height)
             && ++outsideCount == 2)
            {
                zoomingOut = true;
            }

            if (!zoomingOut && viewScaleIdx > 0
             && obj.Hitbox.X > InnerZoomBox.X
             && obj.Hitbox.Y > InnerZoomBox.Y
             && obj.Hitbox.X + obj.Hitbox.Width < InnerZoomBox.X + InnerZoomBox.Width
             && obj.Hitbox.Y + obj.Hitbox.Height < InnerZoomBox.Y + InnerZoomBox.Height
             && ++insideCount == 2)
            {
                zoomingIn = true;
            }

            if (hardFollow) continue;
            if (obj.Hitbox.X < FollowBox.X)
            {
                FollowBox.X = obj.Hitbox.X;
                hardFollow = true;
            }
            else if (obj.Hitbox.X + obj.Hitbox.Width > FollowBox.X + FollowBox.Width)
            {
                FollowBox.X = obj.Hitbox.X + obj.Hitbox.Width - FollowBox.Width;
                hardFollow = true;
            }
            if (obj.Hitbox.Y < FollowBox.Y)
            {
                FollowBox.Y = obj.Hitbox.Y;
                hardFollow = true;
            }
            else if (obj.Hitbox.Y + obj.Hitbox.Height > FollowBox.Y + FollowBox.Height)
            {
                FollowBox.Y = obj.Hitbox.Y + obj.Hitbox.Height - FollowBox.Height;
                hardFollow = true;
            }
        }


        if (zoomingOut)
        {
            if (++zoomProgress < ZOOM_INTERPOLATION_FRAMES) {
                ViewScale = Utils.Interpolate(ZOOM_LEVELS[viewScaleIdx], ZOOM_LEVELS[viewScaleIdx + 1], (float)zoomProgress/ZOOM_INTERPOLATION_FRAMES, Utils.InterpolationType.EASEOUT);
            }
            else
            {
                var innerViewScale = ZOOM_LEVELS[viewScaleIdx];
                InnerZoomBox.Dimensions = new((graphics.Viewport.Width * INNER_ZOOM_BOX_SIZE) / innerViewScale,
                                              (graphics.Viewport.Height * INNER_ZOOM_BOX_SIZE) / innerViewScale);
                ViewScale = ZOOM_LEVELS[++viewScaleIdx];
                zoomProgress = 0;
                zoomingOut = false;
            }
            SetViewScaleOffset();
            ResizeFollowBoxes();
        }
        else if (zoomingIn)
        {
            if (++zoomProgress < ZOOM_INTERPOLATION_FRAMES)
            {
                ViewScale = Utils.Interpolate(ZOOM_LEVELS[viewScaleIdx], ZOOM_LEVELS[viewScaleIdx - 1], (float)zoomProgress/ZOOM_INTERPOLATION_FRAMES, Utils.InterpolationType.EASEOUT);
            }
            else
            {
                if (viewScaleIdx > 1)
                {
                    var innerViewScale = ZOOM_LEVELS[viewScaleIdx - 1];
                    InnerZoomBox.Dimensions = new((graphics.Viewport.Width * INNER_ZOOM_BOX_SIZE) / innerViewScale,
                                                  (graphics.Viewport.Height * INNER_ZOOM_BOX_SIZE) / innerViewScale);
                }
                ViewScale = ZOOM_LEVELS[--viewScaleIdx];
                zoomProgress = 0;
                zoomingIn = false;
            }
            SetViewScaleOffset();
            ResizeFollowBoxes();
        }

        if (hardFollow)
        {
            SoftFollowBox.Center = FollowBox.Center;
            Hitbox.Center = FollowBox.Center;
        } else {
            Vector2 avgPos = new
                (TrackedObjects.Average(obj => obj.Hitbox.CenterX),
                 TrackedObjects.Average(obj => obj.Hitbox.CenterY));
            TrackedPosition = GetScreenCoords(avgPos);

            var softFollow = false;
            if (avgPos.X < SoftFollowBox.X)
            {
                SoftFollowBox.X -= (SoftFollowBox.X - avgPos.X) / SOFT_FOLLOW_TOLERANCE;
                softFollow = true;
            }
            else if (avgPos.X  > SoftFollowBox.X + SoftFollowBox.Width)
            {
                SoftFollowBox.X += (avgPos.X - SoftFollowBox.X - SoftFollowBox.Width) / SOFT_FOLLOW_TOLERANCE;
                softFollow = true;
            }
            if (avgPos.Y < SoftFollowBox.Y)
            {
                SoftFollowBox.Y -= (SoftFollowBox.Y - avgPos.Y) / SOFT_FOLLOW_TOLERANCE;
                softFollow = true;
            }
            else if (avgPos.Y > SoftFollowBox.Y + SoftFollowBox.Height)
            {
                SoftFollowBox.Y += (avgPos.Y - SoftFollowBox.Y - SoftFollowBox.Height) / SOFT_FOLLOW_TOLERANCE;
                softFollow = true;
            }

            if (softFollow)
            {
                FollowBox.Center = SoftFollowBox.Center;
                Hitbox.Center = SoftFollowBox.Center;
            }
        }

        ZoomBox.Center = FollowBox.Center;
        InnerZoomBox.Center = FollowBox.Center;
    }

    public void DrawDebug(SpriteBatch spriteBatch) {
        spriteBatch.DrawPoint(Hitbox.Center - Hitbox.XY, Color.Blue, 3f);
        spriteBatch.DrawPoint(TrackedPosition, Color.Blue, 5f);
        spriteBatch.DrawRectangle(
            (Rectangle)GetScreenCoords(SoftFollowBox),
            Color.Blue,
            2);
        spriteBatch.DrawRectangle(
            (Rectangle)GetScreenCoords(FollowBox),
            Color.Blue,
            2);
        spriteBatch.DrawRectangle(
            (Rectangle)GetScreenCoords(ZoomBox),
            Color.Blue,
            2);
        if (!zoomingIn && !zoomingOut && viewScaleIdx > 0) {
            spriteBatch.DrawRectangle(
                (Rectangle)GetScreenCoords(InnerZoomBox),
                Color.Purple,
                2);
        }

    }

    public Vector2 GetScreenCoords(Vector2 pos) =>
        (pos - Hitbox.XY) * ViewScale + viewScaleOffset;

    public RectangleF GetScreenCoords(RectangleF rect) =>
        new RectangleF((rect.Position - Hitbox.XY) * ViewScale + viewScaleOffset, rect.Size * ViewScale);

    public Hitbox GetScreenCoords(Hitbox hb) =>
        new Hitbox((hb.XY - Hitbox.XY) * ViewScale + viewScaleOffset, hb.Dimensions * ViewScale);

    public void Center()
    {
        Vector2 avgPos = new(TrackedObjects.Average(obj => obj.Hitbox.X), TrackedObjects.Average(obj => obj.Hitbox.Y));

        Hitbox.Center = avgPos;
        FollowBox.Center = Hitbox.Center;
        SoftFollowBox.Center = Hitbox.Center;
        ZoomBox.Center = Hitbox.Center;
        InnerZoomBox.Center = Hitbox.Center;
    }

    private void SetViewScaleOffset() =>
        viewScaleOffset = new((graphics.Viewport.Width - Hitbox.Width * ViewScale) / 2,
                              (graphics.Viewport.Height - Hitbox.Height * ViewScale) / 2);

    private void ResizeFollowBoxes()
    {
        var oldCenter = FollowBox.Center;
        FollowBox.Dimensions = new((graphics.Viewport.Width * FOLLOW_BOX_SIZE) / ViewScale,
                                   (graphics.Viewport.Height * FOLLOW_BOX_SIZE) / ViewScale);
        ZoomBox.Dimensions = new((graphics.Viewport.Width * ZOOM_BOX_SIZE) / ViewScale,
                                 (graphics.Viewport.Height * ZOOM_BOX_SIZE) / ViewScale);
        SoftFollowBox.Dimensions = new(SOFT_FOLLOW_WIDTH / ViewScale, SOFT_FOLLOW_HEIGHT / ViewScale);
        FollowBox.Center = oldCenter;
        SoftFollowBox.Center = oldCenter;
        ZoomBox.Center = oldCenter;
    }
}

