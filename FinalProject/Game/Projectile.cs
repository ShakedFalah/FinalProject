using System;
using Microsoft.Xna.Framework;

namespace Platformer2D
{
    internal class Projectile : Enemy
    {
        private float timeToLive = 1.5f;
        protected override float MoveSpeed => 300f;
        private readonly bool isPlayerProjectile;
        
        public Projectile(Level level, Vector2 spawnPosition, string spriteSet, FaceDirection direction, bool isPlayerProjectile = false) : base(level, spawnPosition, spriteSet)
        {
            this.direction = direction;
            this.isPlayerProjectile = isPlayerProjectile;
            this.trigger.OnTrigger += OnTriggerEnter;
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
                }
                else
                {
                    Level.RemoveEnemyProjectile(this);
                }
                Dispose();
            }
        }

        void OnTriggerEnter(GameObject other)
        {
            if (other is Player player && !isPlayerProjectile)
            {
                Console.WriteLine("Player hit by enemy projectile");
                player.OnKilled();
                Dispose();
            }
            else if (other is Enemy enemy && isPlayerProjectile)
            {
                Console.WriteLine("Enemy hit by player projectile");
                enemy.Dispose();
                Dispose();
            }
        }
    }
}