using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Platformer2D
{
    class Player : GameObject, IDrawable
    {
        // Animations
        private Animation idleAnimation;
        private Animation runAnimation;
        private Animation jumpAnimation;
        private Animation celebrateAnimation;
        private Animation dieAnimation;
        private SpriteEffects flip = SpriteEffects.None;
        private AnimationPlayer sprite;

        // Sounds
        private SoundEffect killedSound;
        private SoundEffect jumpSound;
        private SoundEffect fallSound;

        public Level Level { get; }

        public bool IsAlive { get; private set; }

        // Physics state
        public Vector2 Position { get; set; }

        private float previousBottom;

        public Vector2 Velocity
        {
            get => velocity;
            set => velocity = value;
        }
        Vector2 velocity;

        // Constants for controlling horizontal movement
        private const float MoveAcceleration = 13000.0f;
        private const float MaxMoveSpeed = 1750.0f;
        private const float GroundDragFactor = 0.48f;
        private const float AirDragFactor = 0.58f;

        // Constants for controlling vertical movement
        private const float MaxJumpTime = 0.35f;
        private const float JumpLaunchVelocity = -3500.0f;
        private const float GravityAcceleration = 3400.0f;
        private const float MaxFallSpeed = 550.0f;
        private const float JumpControlPower = 0.14f;
        private bool jumpPressed;

        // Input configuration
        private const float MoveStickScale = 1.0f;
        private const float AccelerometerScale = 1.5f;
        private const Buttons JumpButton = Buttons.A;

        /// <summary>
        /// Gets whether the player's feet are on the ground.
        /// </summary>
        public bool IsOnGround { get; private set; }

        /// <summary>
        /// Current user movement input.
        /// </summary>
        private float movement;

        // Jumping state
        private bool isJumping;
        private float jumpTime = -1f;

        // Wall Jumping
        private bool isWallHugging = false;
        private int wallDirection = 1;
        private float wallTimer = 0f;
        private const float WallGraceTime = 0.12f;
        private const float WallSlideSpeed = 80f;
        private const float WallJumpHorizontalVelocity = 2500.0f;
        private float wallJumpSameWallCooldown = 0f;
        private const float WallJumpSameWallCooldownTime = 0.5f;
        private int lastWallJumpDirection = 0;
        private float wallJumpControlTimer = 0f;
        private const float WallJumpControlTime = 0.2f;
        private const float WallJumpInputMultiplier = 0.25f;

        // Shooting
        private bool isShootPressed = false;
        private float shootingCooldown = 0f;
        private const float MaxShootingCooldown = 1f;

        private bool isCelebrating;

        private Rectangle localBounds;
        private TriggerCollider trigger;

        public Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(Position.X - sprite.Origin.X) + localBounds.X;
                int top = (int)Math.Round(Position.Y - sprite.Origin.Y) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width, localBounds.Height);
            }
        }

        public Player(Level level, Vector2 position)
        {
            this.Level = level;
            this.trigger = new TriggerCollider(this, () => BoundingRectangle);

            LoadContent();

            Reset(position);
        }

        public void LoadContent()
        {
            idleAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Idle"), 0.1f, true);
            runAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Run"), 0.1f, true);
            jumpAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Jump"), 0.1f, false);
            celebrateAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Celebrate"), 0.1f, false);
            dieAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Die"), 0.1f, false);

            int width = (int)(idleAnimation.FrameWidth * 0.4);
            int left = (idleAnimation.FrameWidth - width) / 2;
            int height = (int)(idleAnimation.FrameHeight * 0.8);
            int top = idleAnimation.FrameHeight - height;
            localBounds = new Rectangle(left, top, width, height);

            killedSound = Level.Content.Load<SoundEffect>("Sounds/PlayerKilled");
            jumpSound = Level.Content.Load<SoundEffect>("Sounds/PlayerJump");
            fallSound = Level.Content.Load<SoundEffect>("Sounds/PlayerFall");
        }

        public void Reset(Vector2 position)
        {
            Position = position;
            Velocity = Vector2.Zero;
            IsAlive = true;
            isCelebrating = false;
            sprite.PlayAnimation(idleAnimation);
        }

        public override void Update(GameTime gameTime)
        {

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            wallTimer -= elapsed;

            wallJumpSameWallCooldown -= elapsed;

            isWallHugging = wallTimer > 0 && !IsOnGround;

            if (shootingCooldown > 0)
            {
                shootingCooldown -= elapsed;
            }
            else if (isShootPressed)
            {
                shootingCooldown = MaxShootingCooldown;
                Shoot();
            }

            if (IsAlive && !isCelebrating)
            {
                HandleInput();

                if (IsOnGround)
                {
                    sprite.PlayAnimation(Math.Abs(Velocity.X) - 0.02f > 0 ? runAnimation : idleAnimation);
                }
            }

            ApplyPhysics(gameTime);

            movement = 0.0f;
        }
        
        private void HandleInput()
        {
            KeyboardState keyboardState = Keyboard.GetState();
            GamePadState gamePadState = GamePad.GetState(0);
            AccelerometerState accelState = Accelerometer.GetState();
            
            movement = gamePadState.ThumbSticks.Left.X * MoveStickScale;
            isShootPressed = false;

            // Ignore small movements to prevent running in place.
            if (Math.Abs(movement) < 0.5f)
                movement = 0.0f;

            if (Math.Abs(accelState.Acceleration.Y) > 0.10f)
            {
                movement = MathHelper.Clamp(-accelState.Acceleration.Y * AccelerometerScale, -1f, 1f);
            }

            if (gamePadState.IsButtonDown(Buttons.DPadLeft) ||
                keyboardState.IsKeyDown(Keys.Left) ||
                keyboardState.IsKeyDown(Keys.A))
            {
                movement = -1.0f;
            }
            else if (gamePadState.IsButtonDown(Buttons.DPadRight) ||
                     keyboardState.IsKeyDown(Keys.Right) ||
                     keyboardState.IsKeyDown(Keys.D))
            {
                movement = 1.0f;
            }

            if (gamePadState.IsButtonDown(Buttons.X) ||
                keyboardState.IsKeyDown(Keys.J) ||
                keyboardState.IsKeyDown(Keys.F))
            {
                isShootPressed = true;
            }

            // Check if the player wants to jump.
            bool jumpDown =
                gamePadState.IsButtonDown(JumpButton) ||
                keyboardState.IsKeyDown(Keys.Space);

            jumpPressed = jumpDown && !isJumping;
            isJumping = jumpDown;
        }

        void ApplyPhysics(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 previousPosition = Position;

            // Base velocity is a combination of horizontal movement control and
            // acceleration downward due to gravity.
            float horizontalControl = movement;

            if (wallJumpControlTimer > 0)
            {
                if (Math.Sign(movement) != Math.Sign(velocity.X) && movement != 0)
                {
                    horizontalControl *= WallJumpInputMultiplier;
                }

                wallJumpControlTimer -= elapsed;
            }

            velocity.X += horizontalControl * MoveAcceleration * elapsed;
            velocity.Y = MathHelper.Clamp(velocity.Y + GravityAcceleration * elapsed, -MaxFallSpeed, MaxFallSpeed);

            if (isWallHugging && velocity.Y > WallSlideSpeed)
            {
                velocity.Y = WallSlideSpeed;
            }

            velocity.Y = DoJump(velocity.Y, gameTime);

            // Apply pseudo-drag horizontally.
            if (IsOnGround)
                velocity.X *= GroundDragFactor;
            else
                velocity.X *= AirDragFactor;

            // Prevent the player from running faster than his top speed.            
            velocity.X = MathHelper.Clamp(velocity.X, -MaxMoveSpeed, MaxMoveSpeed);

            // Apply velocity.
            Position += velocity * elapsed;
            Position = new Vector2((float)Math.Round(Position.X), (float)Math.Round(Position.Y));

            // If the player is now colliding with the level, separate them.
            HandleCollisions();

            // If the collision stopped us from moving, reset the velocity to zero.
            if (Position.X == previousPosition.X)
                velocity.X = 0;

            if (Position.Y == previousPosition.Y)
                velocity.Y = 0;
        }

        private float DoJump(float velocityY, GameTime gameTime)
        {
            // Start a jump
            if (jumpPressed)
            {
                if (IsOnGround)
                {
                    jumpTime = 0f;

                    jumpSound.Play();
                    sprite.PlayAnimation(jumpAnimation);
                }
                else if (isWallHugging &&
                         !(lastWallJumpDirection == wallDirection &&
                           wallJumpSameWallCooldown > 0))
                {
                    // Push away from the wall.
                    velocity.X = -wallDirection * WallJumpHorizontalVelocity;

                    lastWallJumpDirection = wallDirection;
                    wallJumpControlTimer = WallJumpControlTime;

                    wallTimer = 0;
                    isWallHugging = false;
                    wallJumpSameWallCooldown = WallJumpSameWallCooldownTime;

                    // Start the jump curve.
                    jumpTime = 0f;

                    jumpSound.Play();
                    sprite.PlayAnimation(jumpAnimation);
                }
            }

            // Continue the jump curve while the button is held.
            if (isJumping)
            {
                if (jumpTime >= 0f && jumpTime < MaxJumpTime)
                {
                    jumpTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    velocityY = JumpLaunchVelocity *
                        (1.0f - (float)Math.Pow(
                            jumpTime / MaxJumpTime,
                            JumpControlPower));
                }
                else
                {
                    jumpTime = -1f;
                }
            }
            else
            {
                // Releasing the button cuts the jump short.
                jumpTime = -1f;
            }

            return velocityY;
        }

        /// <summary>
        /// Detects and resolves all collisions between the player and his neighboring
        /// tiles. When a collision is detected, the player is pushed away along one
        /// axis to prevent overlapping. There is some special logic for the Y axis to
        /// handle platforms which behave differently depending on direction of movement.
        /// </summary>
        private void HandleCollisions()
        {
            // Get the player's bounding rectangle and find neighboring tiles.
            Rectangle bounds = BoundingRectangle;
            int leftTile = (int)Math.Floor((float)bounds.Left / Tile.Width);
            int rightTile = (int)Math.Ceiling((float)bounds.Right / Tile.Width) - 1;
            int topTile = (int)Math.Floor((float)bounds.Top / Tile.Height);
            int bottomTile = (int)Math.Ceiling((float)bounds.Bottom / Tile.Height) - 1;
            // Reset flag to search for ground collision.
            IsOnGround = false;

            // For each potentially colliding tile,
            for (int y = topTile; y <= bottomTile; ++y)
            {
                for (int x = leftTile; x <= rightTile; ++x)
                {
                    // If this tile is collidable,
                    TileCollision collision = Level.GetCollision(x, y);
                    if (collision != TileCollision.Passable)
                    {
                        // Determine collision depth (with direction) and magnitude.
                        Rectangle tileBounds = Level.GetBounds(x, y);
                        Vector2 depth = bounds.GetIntersectionDepth(tileBounds);
                        if (depth != Vector2.Zero)
                        {
                            float absDepthX = Math.Abs(depth.X);
                            float absDepthY = Math.Abs(depth.Y);

                            // Resolve the collision along the shallow axis.
                            if (absDepthY < absDepthX || collision == TileCollision.Platform)
                            {
                                // If we crossed the top of a tile, we are on the ground.
                                if (previousBottom <= tileBounds.Top)
                                {
                                    IsOnGround = true;
                                }

                                // Ignore platforms, unless we are on the ground.
                                if (collision == TileCollision.Impassable || IsOnGround)
                                {
                                    // Resolve the collision along the Y axis.
                                    Position = new Vector2(Position.X, Position.Y + depth.Y);

                                    // Perform further collisions with the new bounds.
                                    bounds = BoundingRectangle;
                                }
                            }
                            else if (collision == TileCollision.Impassable)
                            {
                                // Resolve the collision along the X axis.
                                Position = new Vector2(Position.X + depth.X, Position.Y);

                                if (!IsOnGround)
                                {
                                    // Determine wall side
                                    if (depth.X < 0)
                                    {
                                        // Wall is on the right
                                        wallDirection = 1;
                                    }
                                    else
                                    {
                                        // Wall is on the left
                                        wallDirection = -1;
                                    }

                                    wallTimer = WallGraceTime;
                                    isWallHugging = true;
                                }
                                // Perform further collisions with the new bounds.
                                bounds = BoundingRectangle;
                            }
                        }
                    }
                }
            }

            if (IsOnGround)
            {
                isWallHugging = false;
                wallTimer = 0;
            }

            // Save the new bounds bottom.
            previousBottom = bounds.Bottom;
        }

        public void OnKilled(Enemy killedBy)
        {
            if (!IsAlive) return;

            IsAlive = false;

            if (killedBy != null)
                killedSound.Play();
            else
                fallSound.Play();

            sprite.PlayAnimation(dieAnimation);
        }

        /// <summary>
        /// Called when this player reaches the level's exit.
        /// </summary>
        public void OnReachedExit()
        {
            sprite.PlayAnimation(celebrateAnimation);
            isCelebrating = true;
        }

        /// <summary>
        /// Draws the animated player.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            // Flip the sprite to face the way we are moving.
            if (Velocity.X > 0)
                flip = SpriteEffects.FlipHorizontally;
            else if (Velocity.X < 0)
                flip = SpriteEffects.None;

            // Draw that sprite.
            sprite.Draw(gameTime, spriteBatch, Position, flip);
        }

        public void Shoot()
        {
            FaceDirection direction = flip != SpriteEffects.FlipHorizontally ? FaceDirection.Left : FaceDirection.Right;
            Vector2 spawnPosition =
                Position + new Vector2((int)direction * Tile.Width * 0.5f, 0 - (BoundingRectangle.Height / 2));

            Level.AddPlayerProjectile(
                new Projectile(Level, spawnPosition, "Bullet", direction, true));
        }
    }
}