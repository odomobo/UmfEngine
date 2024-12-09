using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UMFE.Framework
{
    public static class Extensions
    {
        public static bool TryGetCollision(this ICollideable self, ICollideable other, out float penetrationDepth)
        {
            var result = self.Collider.InternalTryGetCollision(self, other, out var penetrationDepthTmp);
            if (result == false)
            {
                result = other.Collider.InternalTryGetCollision(other, self, out penetrationDepthTmp);
            }
            if (result == false)
            {
                throw new UmfEngine.UmfException($"The following types cannot collide with each other: {self.Collider.GetType().FullName} and {other.Collider.GetType().FullName}");
            }

            if (penetrationDepthTmp != null)
            {
                penetrationDepth = penetrationDepthTmp.Value;
                return true;
            }
            else
            {
                penetrationDepth = 0;
                return false;
            }
        }
    }
}
