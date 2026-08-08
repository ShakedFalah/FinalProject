using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer2D
{
    internal class Projectile : Enemy
    {
        private float timeToLive = 1.5f;
        protected override float MoveSpeed => 150f;
        private bool isPlayerProjectile;
        public Projectile(Level level, Vector2 spawnPosition, string spriteSet, FaceDirection direction, bool isPlayerProjectile = false) : base(level, spawnPosition, spriteSet)
        {
            this.direction = direction;
            this.isPlayerProjectile = isPlayerProjectile;
        }

        public override void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            position += Vector2.UnitX * (int)direction * MoveSpeed * elapsed;
            timeToLive -= elapsed;

            if (timeToLive < 0) 
            {
                if (isPlayerProjectile)
                {
                    Level.RemovePlayerProjectile(this);
                } else
                {
                    Level.RemoveEnemyProjectile(this);
                }
            }
        }
    }
}