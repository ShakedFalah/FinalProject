using Microsoft.Xna.Framework;

namespace Platformer2D
{
    interface IColliderBounds
    {
        bool Intersects(IColliderBounds other);
    }
}