using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer2D;

public static class GameObjectManager
{
    private static readonly List<GameObject> gameObjects = new();

    public static void RegisterObject(GameObject obj)
    {
        if (!gameObjects.Contains(obj))
        {
            gameObjects.Add(obj);
        }
    }

    public static void UnregisterObject(GameObject obj)
    {
        if (gameObjects.Contains(obj))
        {
            gameObjects.Remove(obj);
        }
    }

    public static void Update(GameTime gameTime)
    {
        // I need to shallow copy the list so it doesn't get modified during the loop
        // ToArray() is somehow the most compact shallow "copy" method I found -_-
        foreach (GameObject obj in gameObjects.ToArray())
        {
            obj.Update(gameTime);
        }
    }

    public static void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        foreach (GameObject obj in gameObjects.ToArray())
        {
            if (obj is IDrawable drawable)
            {
                drawable.Draw(spriteBatch, gameTime);
            }
        }
    }

    public static void Dispose()
    {
        foreach (GameObject obj in gameObjects.ToArray())
        {
            obj.Dispose();
        }
    }
}