using Microsoft.Xna.Framework;
using Platformer2D;
using System;

namespace FinalProject.Game.Enemies
{
    /// <summary>
    /// Enemy that walks on platforms and turns when hitting the edge or a wall.
    /// Stops to shoot at the player if in front of him.
    /// </summary>
    internal class ShootingEnemy : Enemy
    {
        private const int DetectionRangeTiles = 5;
        private const float ShootCooldown = 1.0f;

        private float shootTimer;

        public ShootingEnemy(Level level, Vector2 position, string spriteSet) : base(level, position, spriteSet)
        {
        }

        /// <summary>
        /// Patrol on the platform going back and forth, stops to shoot at the player if in range.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (PlayerInRange())
            {
                shootTimer -= elapsed;

                if (shootTimer <= 0)
                {
                    Shoot();
                    shootTimer = ShootCooldown;
                }

                return;
            }

            Patrol(elapsed);
        }

        private void Patrol(float elapsed)
        {
            float posX = Position.X + localBounds.Width / 2 * (int)direction;
            int tileX = (int)Math.Floor(posX / Tile.Width) - (int)direction;
            int tileY = (int)Math.Floor(Position.Y / Tile.Height);

            if (Level.GetCollision(tileX + (int)direction, tileY - 1) == TileCollision.Impassable ||
                Level.GetCollision(tileX + (int)direction, tileY) == TileCollision.Passable)
            {
                direction = (FaceDirection)(-(int)direction);
            }
            else
            {
                Vector2 velocity = new Vector2((int)direction * MoveSpeed * elapsed, 0.0f);
                position += velocity;
            }
        }

        private bool PlayerInRange()
        {
            Player player = Level.Player;

            float dx = player.Position.X - Position.X;
            float dy = Math.Abs(player.Position.Y - Position.Y);

            // Don't shoot if the player is much higher or lower.
            if (dy > Tile.Height)
                return false;

            // Check the player is in front.
            if (Math.Sign(dx) != (int)direction)
                return false;

            // Check the distance.
            return Math.Abs(dx) <= DetectionRangeTiles * Tile.Width;
        }

        private void Shoot()
        {
            Vector2 spawnPosition =
                Position + new Vector2((int)direction * Tile.Width * 0.5f, 0 - (BoundingRectangle.Height / 2));

            Level.AddEnemyProjectile(
                new Projectile(Level, spawnPosition, "Bullet", direction));
        }
    }
}
