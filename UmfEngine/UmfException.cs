using SDL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmfEngine
{
    public class UmfException : Exception
    {
        public UmfException(string message) : base(message) { }

        public static UmfException From(string methodName) {
            return new UmfException($"Error calling {methodName}: {SDL3.SDL_GetError() ?? "N/A"}");
        }
    }
}
