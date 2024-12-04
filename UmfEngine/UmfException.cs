using SDL;

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
