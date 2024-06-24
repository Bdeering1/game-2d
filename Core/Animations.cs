using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Game2D;

public class Animations {

	private double timeSinceFrame;
	private int curAnim;
	private int curFrame = 0;

	private readonly Animation[] animations;
	private readonly int spriteWidth;
	private readonly int spriteHeight;
	//the source rectangle for the current animation frame
	public Rectangle clipRect;
	//the image for the current animation frame
	public Texture2D curTexture;

	public Animations(ContentManager content, List<(string path, int fps, int startX, int startY, int numFrames)> anims, int spriteWidth, int spriteHeight) {
		this.spriteWidth = spriteWidth;
		this.spriteHeight = spriteHeight;
		clipRect = new Rectangle(0, 0, spriteWidth, spriteHeight);
		curAnim = 0;
		
		animations = new Animation[anims.Count];
		for (int i = 0; i < anims.Count; i++) {
			animations[i] = new Animation(content, anims[i].path, anims[i].fps, anims[i].startX, anims[i].startY, spriteWidth, anims[i].numFrames);
		}
		curTexture = animations[0].img;
	}

	public void Update(GameTime gameTime) {
		timeSinceFrame += gameTime.ElapsedGameTime.TotalNanoseconds;
		Animation current = animations[curAnim];
		curTexture = current.img;

		//update frame (skip frames if needed)
		while (timeSinceFrame > current.timePerFrame){
			curFrame++;
			current.x++;

			if(curFrame >= current.numFrames) {
				current.Reset();
				curFrame = 0;
			} else if(current.x+1 > current.framesPerRow) {
				current.x = 0;
				current.y++;
			}
			clipRect.X = current.x * spriteWidth + 1;
			clipRect.Y = current.y * spriteHeight + 1;
			timeSinceFrame -= current.timePerFrame;
		}
	}
}

public class Animation {
	public Texture2D img;
	public int fps;
	public int startX;
	public int startY;
	public int x;
	public int y;
	public int numFrames;
	public int framesPerRow;
	public int timePerFrame;

	public Animation(ContentManager content, string path, int fps, int startX, int startY, int spriteWidth, int numFrames) {
		img = content.Load<Texture2D>(path);
		this.fps = fps;
		this.startX = startX;
		this.startY = startY;
		x = startX;
		y = startY;
		this.numFrames = numFrames;

		framesPerRow = img.Width/spriteWidth;
		timePerFrame = (int)1e9/fps;
	}

	public void Reset() {
		x = startX;
		y = startY;
	}
}