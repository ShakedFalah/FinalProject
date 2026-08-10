using System;
using Microsoft.Xna.Framework;

namespace Platformer2D;

public class GameObject : IDisposable
{
    public GameObject()
    {
        GameObjectManager.RegisterObject(this);
    }
    
    public virtual void Update(GameTime gameTime) {}

    public void Dispose()
    {
        GameObjectManager.UnregisterObject(this);
    }
}
