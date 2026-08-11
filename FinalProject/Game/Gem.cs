using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace Platformer2D
{
    class Gem : GameObject, IDrawable
    {
        private Texture2D texture;
        private Vector2 origin;
        private SoundEffect collectedSound;

        public readonly int PointValue = 30;
        public readonly Color Color = Color.White;

        private readonly Vector2 basePosition;
        private float bounce;
        private readonly TriggerCollider trigger;

        public Level Level { get; }

        public Vector2 Position => basePosition + new Vector2(0.0f, bounce);

        public Rectangle BoundingBox => new((int)Position.X - Tile.Width / 2, (int)Position.Y - Tile.Height / 2, Tile.Width, Tile.Height);

        public Gem(Level level, Vector2 position)
        {
            this.Level = level;
            this.basePosition = position;
            this.trigger = new TriggerCollider(this, () => BoundingBox);
            trigger.OnTrigger += OnCollected;

            LoadContent();
        }

        public void LoadContent()
        {
            texture = Level.Content.Load<Texture2D>("Sprites/Gem");
            origin = new Vector2(0.5f * texture.Width, 0.5f * texture.Height);
            collectedSound = Level.Content.Load<SoundEffect>("Sounds/GemCollected");
        }

        public override void Update(GameTime gameTime)
        {
            // Bounce control constants
            const float BounceHeight = 0.18f;
            const float BounceRate = 3.0f;
            const float BounceSync = -0.75f;

            double t = gameTime.TotalGameTime.TotalSeconds * BounceRate + Position.X * BounceSync;
            bounce = (float)Math.Sin(t) * BounceHeight * texture.Height;
        }

        public void OnCollected(GameObject other)
        {
            if (other is Player)
            {
                Level.Score += PointValue;
                collectedSound.Play();
                Dispose();
            }
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Draw(texture, Position, null, Color, 0.0f, origin, 1.0f, SpriteEffects.None, 0.0f);
        }
    }
}