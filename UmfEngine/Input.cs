using SDL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace UmfEngine
{
    public record Input(
        bool[] _keyboardState, 
        HashSet<SDL_Scancode> _keysPressed, 
        HashSet<SDL_Scancode> _keysRepeated, 
        Vector2 _mousePosition)
    {
        public bool ShuttingDown { get; set; }

        public bool IsKeyDown(SDL_Scancode scancode)
        {
            if ((uint)scancode >= _keyboardState.Length)
            {
                throw new UmfException($"Tried accessing keycode outside of bounds: {scancode}");
            }
            return _keyboardState[(uint)scancode];
        }

        public bool WasKeyPressed(SDL_Scancode scancode, bool allowRepeat = false) 
        {
            if (_keysPressed.Contains(scancode))
                return true;

            if (allowRepeat && _keysRepeated.Contains(scancode))
                return true;

            return false;
        }

        public Vector2 GetMousePosition(Transform transform)
        {
            return transform.InverseTransformVector(_mousePosition);
        }
    }
}
