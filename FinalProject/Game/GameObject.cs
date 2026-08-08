using Microsoft.Xna.Framework;

namespace Platformer2D;

public class GameObject
{
    public GameObject()
    {
        GameObjectManager.RegisterObject(this);
    }
    
    public virtual void Update(GameTime gameTime) {}

    public void Destroy()
    {
        GameObjectManager.UnregisterObject(this);
    }
}
