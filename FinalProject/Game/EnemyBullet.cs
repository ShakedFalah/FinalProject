using Microsoft.Xna.Framework;

namespace Platformer2D
{
    internal class EnemyBullet
    {
        private float timeToLive = 2f;
        private const float MoveSpeed = 130f;
        private FaceDirection direction;
        public EnemyBullet(Level level, Vector2 spawnPosition, string spriteSet, FaceDirection direction)
        {
            this.direction = direction;
        }

        public override void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            position += Vector2.UnitX * (int)direction * MoveSpeed * elapsed;
            timeToLive -= elapsed;

            if (timeToLive < 0) 
            {
                Level.RemoveEnemy(this);   
            }
        }
    }
}