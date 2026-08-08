using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer2D;

public interface IDrawable
{
    void Draw(SpriteBatch spriteBatch, GameTime gameTime);
}