using FinalProject.Game.AnimationStuff;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Platformer2D;

namespace FinalProject.Game.Enemies
{
    internal class JumpingEnemy : Enemy
    {
        private Vector2 velocity;
        private bool isJumping;
        private Vector2 jumpTarget;
        private float waitTime = 1f;
        private const float MaxWaitTime = 1f;
        private const float Gravity = 800f; // Adjusted for a smoother arc
        private Animation idleAnimation;

        public JumpingEnemy(Level level, Vector2 position, string spriteSet)
            : base(level, position, spriteSet, false)
        {
        }

        public override void LoadContent(string spriteSet)
        {
            base.LoadContent(spriteSet);
            spriteSet = "Sprites/" + spriteSet + "/";
            idleAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Idle"), 0.1f, true);
        }

        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (waitTime > 0)
            {
                sprite.PlayAnimation(idleAnimation);
                waitTime -= dt;
                return;
            }

            if (!isJumping)
            {
                sprite.PlayAnimation(runAnimation);
                BeginJump();
            }
            else
            {
                // Apply gravity and movement
                velocity.Y += Gravity * dt;
                position += velocity * dt;

                // Land when falling back down to or past the target tile height
                if (velocity.Y > 0 && position.Y >= jumpTarget.Y)
                {
                    position = jumpTarget;
                    velocity = Vector2.Zero;
                    isJumping = false;
                    waitTime = MaxWaitTime; // Pause briefly upon landing
                }
            }
        }

        private void BeginJump()
        {
            // Calculate current tile coordinates based on position
            int startTileX = (int)(position.X / Tile.Width);
            int startTileY = (int)(position.Y / Tile.Height) - 1;

            bool found = false;
            int targetTileX = startTileX;

            // Look 3 tiles ahead, then 2, then 1
            for (int i = 3; i >= 1; i--)
            {
                int checkX = startTileX + (i * (int)direction);

                if (checkX < 0 || checkX >= Level.Width)
                    continue;

                // Target tile must be passable
                if (Level.GetCollision(checkX, startTileY) != TileCollision.Passable)
                    continue;

                // Tile below the target must be solid/impassable to land on
                if (Level.GetCollision(checkX, startTileY + 1) == TileCollision.Passable)
                    continue;

                targetTileX = checkX;
                found = true;
                break;
            }

            // If no valid tile in front, wait and switch direction
            if (!found)
            {
                direction = (direction == FaceDirection.Left) ? FaceDirection.Right : FaceDirection.Left;
                return;
            }

            // Keep Y anchor identical to prevent sudden Y-shifts
            jumpTarget = new Vector2(targetTileX * Tile.Width, (startTileY + 1) * Tile.Height);

            // Calculate exact velocities required to hit jumpTarget in jumpTime seconds
            float jumpTime = 1f;
            float dx = jumpTarget.X - position.X;

            velocity.X = dx / jumpTime;
            velocity.Y = -0.5f * Gravity * jumpTime;

            isJumping = true;
        }
    }
}