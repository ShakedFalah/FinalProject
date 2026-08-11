using System;
using System.Collections.Generic;
using FinalProject.Game.GameObjects;
using Microsoft.Xna.Framework;

namespace FinalProject.Game.Colliders
{
    /// <summary>
    /// This only handles trigger collisions, meaning raising an event when two rectangles intersect.
    /// Actual blocking collisions as implemented in this game were too complex to implement in a generic way.
    /// </summary>
    class TriggerCollider : IDisposable
    {
        private readonly GameObject parent;
        private readonly Func<Rectangle> BoundingBox;

        public event Action<GameObject> OnTrigger;

        public TriggerCollider(GameObject parent, Func<Rectangle> boundingBox)
        {
            this.parent = parent;
            parent.AddResource(this);
            this.BoundingBox = boundingBox;
            triggers.Add(this);
        }
        
        public void Dispose()
        {
            triggers.Remove(this);
        }

        private static readonly List<TriggerCollider> triggers = [];

        public static void HandleTriggerCollisions()
        {
            for (int i = 0; i < triggers.Count; i++)
            {
                TriggerCollider first = triggers[i];
                for (int j = 0; j < triggers.Count; j++)
                {
                    if (i == j) continue;
                    TriggerCollider second = triggers[j];

                    if (first.BoundingBox().Intersects(second.BoundingBox()))
                    {
                        first.OnTrigger?.Invoke(second.parent);
                    }
                }
            }
        }
    }
}
