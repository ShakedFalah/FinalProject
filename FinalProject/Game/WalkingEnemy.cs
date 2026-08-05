using Microsoft.Xna.Framework;
using System;

namespace Platformer2D
{
    /// <summary>
    /// Enemy that walks on platforms and turns when hitting the edge or a wall.
    /// </summary>
    internal class WalkingEnemy : Enemy
    {


        public WalkingEnemy(Level level, Vector2 position, string spriteSet) : base(level, position, spriteSet)
        {
        }

        /// <summary>
        /// Paces back and forth along a platform, waiting at either end.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Calculate tile position based on the side we are walking towards.
            float posX = Position.X + localBounds.Width / 2 * (int)direction;
            int tileX = (int)Math.Floor(posX / Tile.Width) - (int)direction;
            int tileY = (int)Math.Floor(Position.Y / Tile.Height);

            // If we are about to run into a wall or off a cliff, start waiting.
            if (Level.GetCollision(tileX + (int)direction, tileY - 1) == TileCollision.Impassable ||
                Level.GetCollision(tileX + (int)direction, tileY) == TileCollision.Passable)
            {
                direction = (FaceDirection)(-(int)direction);
            }
            else
            {
                // Move in the current direction.
                Vector2 velocity = new Vector2((int)direction * MoveSpeed * elapsed, 0.0f);
                position = position + velocity;
            }
        }
    }
}
