using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer2D
{
    /// <summary>
    /// Facing direction along the X axis.
    /// </summary>
    enum FaceDirection
    {
        Left = -1,
        Right = 1,
    }

    /// <summary>
    /// Abstract class for enemies that move and turn around
    /// </summary>
    abstract class Enemy : GameObject, IDrawable
    {
        public Level Level { get; }

        /// <summary>
        /// Position in world space of the bottom center of this enemy.
        /// </summary>
        public Vector2 Position
        {
            get => position;
        }
        protected Vector2 position;

        /// <summary>
        /// The direction this enemy is facing and moving along the X axis.
        /// </summary>
        protected FaceDirection direction = FaceDirection.Left;

        protected Rectangle localBounds;
        /// <summary>
        /// Gets a rectangle which bounds this enemy in world space.
        /// </summary>
        public Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(Position.X - sprite.Origin.X) + localBounds.X;
                int top = (int)Math.Round(Position.Y - sprite.Origin.Y) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width, localBounds.Height);
            }
        }

        // Animations
        protected Animation runAnimation;
        protected AnimationPlayer sprite;
        protected bool isAnimationLooping = true;

        /// <summary>
        /// The speed at which this enemy moves along the X axis.
        /// </summary>
        protected virtual float MoveSpeed => 64.0f;

        public bool IsWalking => Level.Player.IsAlive &&
                !Level.ReachedExit &&
                Level.TimeRemaining > TimeSpan.Zero;

        /// <summary>
        /// Constructs a new Enemy.
        /// </summary>
        public Enemy(Level level, Vector2 position, string spriteSet, bool isAnimationLooping = true)
        {
            this.Level = level;
            this.position = position;
            this.isAnimationLooping = isAnimationLooping;

            LoadContent(spriteSet);
        }

        /// <summary>
        /// Loads a particular enemy sprite sheet and sounds.
        /// </summary>
        public virtual void LoadContent(string spriteSet)
        {
            // Load animations.
            spriteSet = "Sprites/" + spriteSet + "/";
            runAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Run"), 0.1f, isAnimationLooping);
            sprite.PlayAnimation(runAnimation);

            // Calculate bounds within texture size.
            int width = (int)(runAnimation.FrameWidth * 0.35);
            int left = (runAnimation.FrameWidth - width) / 2;
            int height = (int)(runAnimation.FrameHeight * 0.7);
            int top = runAnimation.FrameHeight - height;
            localBounds = new Rectangle(left, top, width, height);
        }

        /// <summary>
        /// Draws the animated enemy.
        /// </summary>
        public virtual void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            // Draw facing the way the enemy is moving.
            SpriteEffects flip = direction > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            sprite.Draw(gameTime, spriteBatch, Position, flip);
        }
    }
}