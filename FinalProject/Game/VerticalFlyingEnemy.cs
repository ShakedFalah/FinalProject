using Microsoft.Xna.Framework;
using System;

namespace Platformer2D
{
    /// <summary>
    /// Enemy that flies up and down.
    /// </summary>
    internal class VerticalFlyingEnemy : Enemy
    {
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

            position.Y += (int)direction * MoveSpeed * elapsed;

            if (direction == FaceDirection.Left) // Moving up
            {
                if (position.Y <= upperLimit)
                {
                    position.Y = upperLimit;
                    direction = FaceDirection.Right;
                }
            }
            else // Moving down
            {
                if (position.Y >= lowerLimit)
                {
                    position.Y = lowerLimit;
                    direction = FaceDirection.Left;
                }
            }
        }
    }
}
