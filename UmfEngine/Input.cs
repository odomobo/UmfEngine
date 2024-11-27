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
    public enum MouseButton
    {
        Left,
        Right,
        Middle,
    }

    public record Input(
        bool[] _keyboardState, 
        HashSet<SDL_Scancode> _keysPressed, 
        HashSet<SDL_Scancode> _keysRepeated, 
        Vector2 _mousePosition,
        SDL_MouseButtonFlags _mouseButtonsDown,
        HashSet<SDLButton> _mouseButtonsPressed)
    {
        // TODO: pass this above I guess
        public bool ShuttingDown { get; set; }

        public bool IsKeyDown(SDL_Scancode scancode)
        {
            if ((uint)scancode >= _keyboardState.Length)
            {
                throw new UmfException($"Tried accessing keycode outside of bounds: {scancode}");
            }
            return _keyboardState[(uint)scancode];
        }

        // TODO: I think use SDL_Keycode instead of SDL_Scancode, so ctrl+c will register differently than c
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

        // TODO: implement IsMouseButtonDown

        public bool WasMouseButtonPressed(MouseButton mouseButton)
        {
            switch (mouseButton)
            {
                case MouseButton.Left:
                    return _mouseButtonsPressed.Contains(SDLButton.SDL_BUTTON_LEFT);
                case MouseButton.Right:
                    return _mouseButtonsPressed.Contains(SDLButton.SDL_BUTTON_RIGHT);
                case MouseButton.Middle:
                    return _mouseButtonsPressed.Contains(SDLButton.SDL_BUTTON_MIDDLE);
            }

            throw new Exception("Shouldn't be able to reach here");
        }
    }
}
