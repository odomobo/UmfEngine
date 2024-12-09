using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UMFE.Framework
{
    public abstract class Collider
    {
        /// <summary>
        /// Implement this for any shapes it can collide with.
        /// </summary>
        /// <param name="otherCollider">The shape to attempt to collide on.</param>
        /// <param name="penetrationDepth">If it can collide, then penetration depth should be null if no collision, or a positive value on collision.</param>
        /// <returns>False on a shape it can't collide with, and true if it can.</returns>
        protected internal abstract bool InternalTryGetCollision(ICollideable self, ICollideable other, out float? penetrationDepth);
    }
}
