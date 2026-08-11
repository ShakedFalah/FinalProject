using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace FinalProject.Game.GameObjects;

public class GameObject : IDisposable
{
    private readonly List<IDisposable> resources = [];
    
    public GameObject()
    {
        GameObjectManager.RegisterObject(this);
    }

    public void AddResource(IDisposable resource)
    {
        if (!resources.Contains(resource)) resources.Add(resource);
    }
    
    public virtual void Update(GameTime gameTime) {}

    public void Dispose()
    {
        resources.ForEach(resource => resource.Dispose());
        GameObjectManager.UnregisterObject(this);
    }
}
