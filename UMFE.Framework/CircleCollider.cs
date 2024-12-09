using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UMFE.Framework
{
    public class CircleCollider : Collider
    {
        public readonly float Radius;

        public CircleCollider(float radius)
        {
            Radius = radius;
        }

        protected internal override bool InternalTryGetCollision(ICollideable self, ICollideable other, out float? penetrationDepth)
        {
            switch (other.Collider)
            {
                case CircleCollider c:
                    var adjustedSelfRadius = Radius * self.Transform.ScaleRelativeToWorld;
                    var adjustedOtherRadius = c.Radius * other.Transform.ScaleRelativeToWorld;
                    var radiusSum = adjustedSelfRadius + adjustedOtherRadius;

                    var selfPosition = self.Transform.PositionRelativeToWorld;
                    var otherPosition = other.Transform.PositionRelativeToWorld;
                    var distance = (selfPosition - otherPosition).Length();
                    if (radiusSum >= distance)
                    {
                        penetrationDepth = radiusSum - distance;
                    }
                    else
                    {
                        penetrationDepth = null;
                    }

                    return true;

                default:
                    penetrationDepth = null;
                    return false;
            }
        }
    }
}
