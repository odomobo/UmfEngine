using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UmfEngine;

namespace Game
{
    internal interface IGameObject
    {
        void Update(Engine e, Transform t);
        void Draw(Engine e, Transform t);
    }
}
