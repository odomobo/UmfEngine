using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UMFE.Framework
{
    public interface ICollideable : IGameObject
    {
        Collider Collider { get; }
    }
}
