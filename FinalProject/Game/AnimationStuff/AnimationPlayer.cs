using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FinalProject.Game.AnimationStuff
{
    struct AnimationPlayer
    {
        public Animation CurrentAnimation { get; private set; }
        public int FrameIndex { get; private set; }

        /// <summary>
        /// The amount of time in seconds that the current frame has been shown for.
        /// </summary>
        private float time;

        /// <summary>
        /// Gets a texture origin at the bottom center of each frame.
        /// </summary>
        public Vector2 Origin => new(0.5f * CurrentAnimation.FrameWidth, CurrentAnimation.FrameHeight);
        
        public void PlayAnimation(Animation animation)
        {
            // If this animation is already running, do not restart it.
            if (this.CurrentAnimation == animation)
                return;

            // Start the new animation.
            this.CurrentAnimation = animation;
            this.FrameIndex = 0;
            this.time = 0.0f;
        }

        /// <summary>
        /// Advances the time position and draws the current frame of the animation.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Vector2 position, SpriteEffects spriteEffects)
        {
            if (CurrentAnimation == null)
                throw new NotSupportedException("No animation is currently playing.");

            time += (float)gameTime.ElapsedGameTime.TotalSeconds;
            while (time > CurrentAnimation.FrameTime)
            {
                time -= CurrentAnimation.FrameTime;

                // Advance the frame index; looping or clamping as appropriate.
                if (CurrentAnimation.IsLooping)
                {
                    FrameIndex = (FrameIndex + 1) % CurrentAnimation.FrameCount;
                }
                else
                {
                    FrameIndex = Math.Min(FrameIndex + 1, CurrentAnimation.FrameCount - 1);
                }
            }

            // Calculate the source rectangle of the current frame.
            Rectangle source = new Rectangle(FrameIndex * CurrentAnimation.Texture.Height, 0, CurrentAnimation.Texture.Height, CurrentAnimation.Texture.Height);

            // Draw the current frame.
            spriteBatch.Draw(CurrentAnimation.Texture, position, source, Color.White, 0.0f, Origin, 1.0f, spriteEffects, 0.0f);
        }
    }
}