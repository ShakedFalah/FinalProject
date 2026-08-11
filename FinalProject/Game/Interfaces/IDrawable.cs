using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FinalProject.Game.Interfaces;

public interface IDrawable
{
    void Draw(SpriteBatch spriteBatch, GameTime gameTime);
}