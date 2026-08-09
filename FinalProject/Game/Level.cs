using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System.IO;

namespace Platformer2D
{
    class Level : GameObject, IDrawable, IDisposable
    {
        // Physical structure of the level.
        private Tile[,] tiles;
        private readonly Texture2D[] layers;
        // The layer which entities are drawn on top of.
        private const int EntityLayer = 2;

        // Entities in the level.
        public Player Player { get; private set; }

        private List<Gem> gems = new List<Gem>();
        private List<Enemy> enemies = new List<Enemy>();
        private List<Projectile> enemyProjectiles = new List<Projectile>();
        private List<Projectile> playerProjectiles = new List<Projectile>();

        // Key locations in the level.
        private static readonly Point InvalidPosition = new(-1, -1);
        private Vector2 start;
        private Point exit = InvalidPosition;

        public int Score { get; private set; }

        public bool ReachedExit { get; private set; }

        public TimeSpan TimeRemaining { get; private set; }

        private const int PointsPerSecond = 5;

        // Level content.        
        public ContentManager Content { get; }

        private readonly SoundEffect exitReachedSound;

        #region Loading

        public Level(IServiceProvider serviceProvider, Stream fileStream, int levelIndex)
        {
            // Create a new content manager to load content used just by this level.
            Content = new ContentManager(serviceProvider, "Content");

            TimeRemaining = TimeSpan.FromMinutes(2.0);

            LoadTiles(fileStream);

            // Load background layer textures. For now, all levels must
            // use the same backgrounds and only use the left-most part of them.
            layers = new Texture2D[3];
            int segmentIndex = levelIndex;
            for (int i = 0; i < layers.Length; ++i)
            {
                // Choose a random segment if each background layer for level variety.
                layers[i] = Content.Load<Texture2D>("Backgrounds/Layer" + i + "_" + segmentIndex);
            }

            // Load sounds.
            exitReachedSound = Content.Load<SoundEffect>("Sounds/ExitReached");
        }

        private void LoadTiles(Stream fileStream)
        {
            // Load the level and ensure all of the lines are the same length.
            int width;
            List<string> lines = new List<string>();
            using (StreamReader reader = new StreamReader(fileStream))
            {
                string line = reader.ReadLine();
                width = line.Length;
                while (line != null)
                {
                    lines.Add(line);
                    if (line.Length != width)
                        throw new Exception($"The length of line {lines.Count} is different from all preceeding lines.");
                    line = reader.ReadLine();
                }
            }

            // Allocate the tile grid.
            tiles = new Tile[width, lines.Count];

            // Loop over every tile position,
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    // to load each tile.
                    char tileType = lines[y][x];
                    tiles[x, y] = LoadTile(tileType, x, y, lines);
                }
            }

            // Verify that the level has a beginning and an end.
            if (Player == null)
                throw new NotSupportedException("A level must have a starting point.");
            if (exit == InvalidPosition)
                throw new NotSupportedException("A level must have an exit.");

        }

        private Tile LoadTile(char tileType, int x, int y, List<string> lines)
        {
            switch (tileType)
            {
                // Blank space
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case '.':
                    return new Tile(null, TileCollision.Passable);

                // Exit
                case 'X':
                    return LoadExitTile(x, y);

                // Gem
                case 'G':
                    return LoadGemTile(x, y);

                // Floating platform
                case '-':
                    return LoadTile("Platform", TileCollision.Platform);

                // Various enemies
                case 'A':
                    return LoadWalkingEnemyTile(x, y, "MonsterA");
                case 'B':
                    {
                        int flyUp = 0;
                        int flyDown = 0;

                        // Read digit directly above
                        if (y > 0 && char.IsDigit(lines[y - 1][x]))
                            flyUp = lines[y - 1][x] - '0';

                        // Read digit directly below
                        if (y < Height - 1 && char.IsDigit(lines[y + 1][x]))
                            flyDown = lines[y + 1][x] - '0';

                        return LoadVerticalFlyingEnemy(x, y, "MonsterB", flyUp, flyDown);
                    }
                case 'C':
                    return LoadJumpingEnemy(x, y, "MonsterC");
                case 'D':
                    return LoadShootingEnemyTile(x, y, "MonsterA");

                // Platform block
                case '~':
                    return LoadTile("BlockB", TileCollision.Platform);

                // Player 1 start point
                case 'P':
                    return LoadStartTile(x, y);

                // Impassable block
                case '#':
                    return LoadTile("BlockA", TileCollision.Impassable);

                // Unknown tile type character
                default:
                    throw new NotSupportedException(String.Format("Unsupported tile type character '{0}' at position {1}, {2}.", tileType, x, y));
            }
        }

        private Tile LoadTile(string name, TileCollision collision)
        {
            return new Tile(Content.Load<Texture2D>("Tiles/" + name), collision);
        }

        /// <summary>
        /// Instantiates a player, puts him in the level, and remembers where to put him when he is resurrected.
        /// </summary>
        private Tile LoadStartTile(int x, int y)
        {
            if (Player != null)
                throw new NotSupportedException("A level may only have one starting point.");

            start = GetBounds(x, y).GetBottomCenter();
            Player = new Player(this, start);

            return new Tile(null, TileCollision.Passable);
        }

        /// <summary>
        /// Remembers the location of the level's exit.
        /// </summary>
        private Tile LoadExitTile(int x, int y)
        {
            if (exit != InvalidPosition)
                throw new NotSupportedException("A level may only have one exit.");

            exit = GetBounds(x, y).Center;

            return LoadTile("Exit", TileCollision.Passable);
        }

        /// <summary>
        /// Instantiates an enemy and puts him in the level.
        /// </summary>
        private Tile LoadWalkingEnemyTile(int x, int y, string spriteSet)
        {
            Vector2 position = GetBounds(x, y).GetBottomCenter();
            enemies.Add(new WalkingEnemy(this, position, spriteSet));

            return new Tile(null, TileCollision.Passable);
        }

        private Tile LoadVerticalFlyingEnemy(int x, int y, string spriteSet, int flyUp, int flyDown)
        {
            Vector2 position = GetBounds(x, y).GetBottomCenter();
            enemies.Add(new VerticalFlyingEnemy(this, position, spriteSet, flyUp, flyDown));

            return new Tile(null, TileCollision.Passable);
        }

        private Tile LoadJumpingEnemy(int x, int y, string spriteSet)
        {
            Vector2 position = GetBounds(x, y).GetBottomCenter();
            enemies.Add(new JumpingEnemy(this, position, spriteSet));

            return new Tile(null, TileCollision.Passable);
        }

        private Tile LoadShootingEnemyTile(int x, int y, string spriteSet)
        {
            Vector2 position = GetBounds(x, y).GetBottomCenter();
            enemies.Add(new ShootingEnemy(this, position, spriteSet));

            return new Tile(null, TileCollision.Passable);
        }


        /// <summary>
        /// Instantiates a gem and puts it in the level.
        /// </summary>
        private Tile LoadGemTile(int x, int y)
        {
            Point position = GetBounds(x, y).Center;
            gems.Add(new Gem(this, new Vector2(position.X, position.Y)));

            return new Tile(null, TileCollision.Passable);
        }

        public void AddEnemyProjectile(Projectile bullet)
        {
            enemyProjectiles.Add(bullet);
        }

        internal void AddPlayerProjectile(Projectile projectile)
        {
            playerProjectiles.Add(projectile);
        }

        public void RemoveEnemyProjectile(Projectile bullet)
        {
            enemyProjectiles.Remove(bullet);
        }

        public void RemovePlayerProjectile(Projectile bullet)
        {
            playerProjectiles.Remove(bullet);
        }

        public void RemoveEnemy(Enemy enemy)
        {
            enemies.Remove(enemy);
        }

        public void Dispose()
        {
            Content.Unload();
        }

        #endregion

        #region Bounds and collision

        /// <summary>
        /// Gets the collision mode of the tile at a particular location.
        /// This method handles tiles outside of the levels boundries by making it
        /// impossible to escape past the left or right edges, but allowing things
        /// to jump beyond the top of the level and fall off the bottom.
        /// </summary>
        public TileCollision GetCollision(int x, int y)
        {
            // Prevent escaping past the level ends.
            if (x < 0 || x >= Width)
                return TileCollision.Impassable;
            // Allow jumping past the level top and falling through the bottom.
            if (y < 0 || y >= Height)
                return TileCollision.Passable;

            return tiles[x, y].Collision;
        }

        public static Rectangle GetBounds(int x, int y)
        {
            return new Rectangle(x * Tile.Width, y * Tile.Height, Tile.Width, Tile.Height);
        }

        /// <summary>
        /// Width of level measured in tiles.
        /// </summary>
        public int Width => tiles.GetLength(0);

        /// <summary>
        /// Height of the level measured in tiles.
        /// </summary>
        public int Height => tiles.GetLength(1);

        #endregion

        #region Update

        /// <summary>
        /// Updates all objects in the world, performs collision between them,
        /// and handles the time limit with scoring.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            if (ReachedExit)
            {
                // Animate the time being converted into points.
                int seconds = (int)Math.Round(gameTime.ElapsedGameTime.TotalSeconds * 100.0f);
                seconds = Math.Min(seconds, (int)Math.Ceiling(TimeRemaining.TotalSeconds));
                TimeRemaining -= TimeSpan.FromSeconds(seconds);
                Score += seconds * PointsPerSecond;
            }
            else
            {
                TimeRemaining -= gameTime.ElapsedGameTime;

                // Falling off the bottom of the level kills the player.
                if (Player.BoundingRectangle.Top >= Height * Tile.Height)
                    Player.OnKilled(null);
                
                HandleTriggerCollisions();
            }

            // Clamp the time remaining at zero.
            if (TimeRemaining < TimeSpan.Zero)
                TimeRemaining = TimeSpan.Zero;
        }

        void HandleTriggerCollisions()
        {
            // TODO move to colliders
            
            for (int i = 0; i < gems.Count; ++i)
            {
                Gem gem = gems[i];
                
                if (gem.BoundingCircle.Intersects(Player.BoundingRectangle))
                {
                    gems.RemoveAt(i--);
                    Score += gem.PointValue;

                    gem.OnCollected(Player);
                }
            }
            
            foreach (Enemy enemy in enemies)
            {
                // Touching an enemy instantly kills the player
                if (enemy.BoundingRectangle.Intersects(Player.BoundingRectangle))
                {
                    Player.OnKilled(enemy);
                }
            }
            
            if (Player.IsAlive &&
                Player.IsOnGround &&
                Player.BoundingRectangle.Contains(exit))
            {
                Player.OnReachedExit();
                exitReachedSound.Play();
                ReachedExit = true;
            }

            for (int i = 0; i < enemyProjectiles.Count; i++)
            {
                Projectile projectile = enemyProjectiles[i];

                if (projectile.BoundingRectangle.Intersects(Player.BoundingRectangle))
                {
                    Player.OnKilled(projectile);
                }
            }

            for (int i = 0; i < playerProjectiles.Count; i++)
            {
                Projectile projectile = playerProjectiles[i];

                for (int j = 0; j < enemies.Count; j++)
                {
                    Enemy enemy = enemies[j];
                    if (projectile.BoundingRectangle.Intersects(enemy.BoundingRectangle))
                    {
                        playerProjectiles.RemoveAt(i);
                        projectile.Destroy();
                        enemies.RemoveAt(j);
                        enemy.Destroy();
                        i--;
                        break;
                    }
                }
            }

        }

        /// <summary>
        /// Restores the player to the starting point to try the level again.
        /// </summary>
        public void StartNewLife()
        {
            Player.Reset(start);
        }

        #endregion

        #region Draw

        /// <summary>
        /// Draw everything in the level from background to foreground.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            for (int i = 0; i <= EntityLayer; ++i)
                spriteBatch.Draw(layers[i], Vector2.Zero, Color.White);

            DrawTiles(spriteBatch);

            for (int i = EntityLayer + 1; i < layers.Length; ++i)
                spriteBatch.Draw(layers[i], Vector2.Zero, Color.White);
        }

        /// <summary>
        /// Draws each tile in the level.
        /// </summary>
        private void DrawTiles(SpriteBatch spriteBatch)
        {
            // For each tile position
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    // If there is a visible tile in that position
                    Texture2D texture = tiles[x, y].Texture;
                    if (texture != null)
                    {
                        // Draw it in screen space.
                        Vector2 position = new Vector2(x, y) * Tile.Size;
                        spriteBatch.Draw(texture, position, Color.White);
                    }
                }
            }
        }
        #endregion
    }
}