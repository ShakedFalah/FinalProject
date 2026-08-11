using System;
using FinalProject.Game.AnimationStuff;
using FinalProject.Game.Colliders;
using FinalProject.Game.GameObjects;
using FinalProject.Game.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Platformer2D;

namespace FinalProject.Game.Enemies
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
    abstract class Enemy : GameObject, Interfaces.IDrawable
    {
        public Level Level { get; }

        public Vector2 Position => position;
        protected Vector2 position;

        protected FaceDirection direction = FaceDirection.Left;

        protected Rectangle localBounds;

        protected TriggerCollider trigger;
        
        public Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(Position.X - sprite.Origin.X) + localBounds.X;
                int top = (int)Math.Round(Position.Y - sprite.Origin.Y) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width, localBounds.Height);
            }
        }

        protected Animation runAnimation;
        protected AnimationPlayer sprite;
        private bool isAnimationLooping;

        protected virtual float MoveSpeed => 64.0f;

        public Enemy(Level level, Vector2 position, string spriteSet, bool isAnimationLooping = true)
        {
            this.Level = level;
            this.position = position;
            this.isAnimationLooping = isAnimationLooping;
            this.trigger = new TriggerCollider(this, () => BoundingRectangle);

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