using Microsoft.Xna.Framework;
using System;

namespace Platformer2D
{
    /// <summary>
    /// Enemy that flies up and down.
    /// </summary>
    internal class VerticalFlyingEnemy : Enemy
    {
        /// <summary>
        /// How much distance the enemy flies before turning around.
        /// </summary>
        private readonly float startY;
        private readonly float upperLimit;
        private readonly float lowerLimit;

        public VerticalFlyingEnemy(Level level, Vector2 position, string spriteSet, int flyDistanceUp, int flyDistanceDown) : base(level, position, spriteSet)
        {
            startY = position.Y;
            upperLimit = startY - flyDistanceUp * Tile.Height;
            lowerLimit = startY + flyDistanceDown * Tile.Height;

            direction = FaceDirection.Left; // Up (-1)
        }

        public override void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (waitTime > 0)
            {
                // Wait for some amount of time.
                waitTime = Math.Max(0.0f, waitTime - (float)gameTime.ElapsedGameTime.TotalSeconds);
                if (waitTime <= 0.0f)
                {
                    // Then turn around.
                    direction = (FaceDirection)(-(int)direction);
                }
            }
            else
            {
                position.Y += (int)direction * MoveSpeed * elapsed;

                if ((int)direction < 0) // Moving up
                {
                    if (position.Y <= upperLimit)
                    {
                        position.Y = upperLimit;
                        waitTime = MaxWaitTime;
                    }
                }
                else // Moving down
                {
                    if (position.Y >= lowerLimit)
                    {
                        position.Y = lowerLimit;
                        waitTime = MaxWaitTime;
                    }
                }

            }

        }
    }
}
