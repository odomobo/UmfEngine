using SDL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace UmfEngine
{
    // TODO: usage will be like:
    // Begin(color);
    // DrawLine(t1, b1, e1);
    // DrawLine(t2, b2, e2);
    // DrawLine(t3, b3, e3);
    // Flush(_renderer);
    //
    // It will auto flush if it gets too many lines. Similarly, it will cause a stink if it ever gets an end without a begin and vice versa
    internal unsafe class BatchGeometryRenderer
    {
        private readonly bool _useCompatibilityMode;
        public BatchGeometryRenderer(bool useCompatibilityMode)
        {
            _useCompatibilityMode = useCompatibilityMode;
        }

        public void Begin(Color color)
        {
            throw new NotImplementedException();
        }

        // flush after every draw if _useCompatibilityMode is set
        public void DrawLine(float thickness, Vector2 begin, Vector2 end)
        {
            throw new NotImplementedException();
        }

        public void DrawCircle(float diameter, Vector2 coord)
        {
            throw new NotImplementedException();
        }

        public void Flush(SDL_Renderer* renderer)
        {
            throw new NotImplementedException();
        }
    }
}
