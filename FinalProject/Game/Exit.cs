using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer2D;

public class Exit : GameObject, IDrawable
{
    private Texture2D exitSprite;
    private TriggerCollider trigger;

    public Point Position { get; set; }
    public Rectangle BoundingBox => new Rectangle(Position.X, Position.Y, Tile.Width, Tile.Height);

    public Exit(ContentManager content)
    {
        exitSprite = content.Load<Texture2D>("Tiles/Exit");
        trigger = new TriggerCollider(this, () => BoundingBox);
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        spriteBatch.Draw(exitSprite, Position.ToVector2(), Color.White);
    }
}