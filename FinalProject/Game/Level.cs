using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using FinalProject.Game.Enemies;
using FinalProject.Game.Colliders;
using FinalProject.Game.GameObjects;
using FinalProject.Game;
using FinalProject.Game.Interfaces;

namespace Platformer2D
{
    class Level : GameObject, FinalProject.Game.Interfaces.IDrawable, IDisposable
    {
        // Physical structure of the level.
        private Tile[,] tiles;
        private readonly Texture2D[] layers;
        // The layer which entities are drawn on top of.
        private const int EntityLayer = 2;

        // Entities in the level.
        public Player Player { get; private set; }

        private readonly List<Gem> gems = [];
        private readonly List<Enemy> enemies = [];
        private readonly List<Projectile> enemyProjectiles = [];
        private readonly List<Projectile> playerProjectiles = [];

        // Key locations in the level.
        private Vector2 start;
        private Exit exit = null;

        public int Score { get; set; }

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
            List<string> lines = [];
            using (StreamReader reader = new(fileStream))
            {
                string line = reader.ReadLine();
                if (line == null) throw new Exception("Empty file");
                width = line.Length;
                while (line != null)
                {
                    lines.Add(line);
                    if (line.Length != width)
                        throw new Exception($"The length of line {lines.Count} is different from all preceding lines.");
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
            if (exit == null)
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
                    throw new NotSupportedException($"Unsupported tile type character '{tileType}' at position {x}, {y}.");
            }
        }

        private Tile LoadTile(string name, TileCollision collision)
        {
            return new Tile(Content.Load<Texture2D>("Tiles/" + name), collision);
        }

        private Tile LoadStartTile(int x, int y)
        {
            if (Player != null)
                throw new NotSupportedException("A level may only have one starting point.");

            start = GetBounds(x, y).GetBottomCenter();
            Player = new Player(this, start);

            return new Tile(null, TileCollision.Passable);
        }

        private Tile LoadExitTile(int x, int y)
        {
            if (exit != null)
                throw new NotSupportedException("A level may only have one exit.");

            exit = new Exit(Content) { Position = new Point(Tile.Width * x, Tile.Height * y) };

            return new Tile(null, TileCollision.Passable);
        }

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

        public new void Dispose()
        {
            GameObjectManager.Dispose();
            Content.Unload();
        }

        #endregion

        #region Bounds and collision

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

        public int Width => tiles.GetLength(0);
        public int Height => tiles.GetLength(1);

        #endregion

        #region Update

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
                    Player.OnKilled(true);
            }

            if (TimeRemaining < TimeSpan.Zero)
                TimeRemaining = TimeSpan.Zero;
        }

        public void FinishLevel()
        {
            if (!ReachedExit)
            {
                exitReachedSound.Play();
                ReachedExit = true;
            }
        }

        public void StartNewLife()
        {
            Player.Reset(start);
        }

        #endregion

        #region Draw

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            for (int i = 0; i <= EntityLayer; ++i)
                spriteBatch.Draw(layers[i], Vector2.Zero, Color.White);

            DrawTiles(spriteBatch);

            for (int i = EntityLayer + 1; i < layers.Length; ++i)
                spriteBatch.Draw(layers[i], Vector2.Zero, Color.White);
        }

        private void DrawTiles(SpriteBatch spriteBatch)
        {
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    Texture2D texture = tiles[x, y].Texture;
                    if (texture != null)
                    {
                        Vector2 position = new Vector2(x, y) * Tile.Size;
                        spriteBatch.Draw(texture, position, Color.White);
                    }
                }
            }
        }
        #endregion
    }
}