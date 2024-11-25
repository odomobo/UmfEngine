using SDL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmfEngine
{
    internal unsafe class PropsBuilder : IDisposable
    {
        private readonly SDL_PropertiesID _props;
        private bool _disposed = false;

        public PropsBuilder() {
            _props = SDL3.SDL_CreateProperties();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            SDL3.SDL_DestroyProperties(_props);
            _disposed = true;
        }

        // not really a builder, but this fits the paradigm anyhow
        public SDL_PropertiesID Build()
        {
            return _props;
        }

        internal void SetPointerProperty(ReadOnlySpan<byte> propertyName, void* value)
        {
            fixed (byte* propertyNameBytes = propertyName)
            {
                // they kinda messed this one up, so we have to cast to a silly type
                if (!SDL3.SDL_SetPointerProperty(_props, propertyNameBytes, (nint)value))
                    throw UmfException.From(nameof(SDL3.SDL_SetPointerProperty));
            }
        }

        internal void SetNumberProperty(ReadOnlySpan<byte> propertyName, long value)
        {
            fixed (byte* propertyNameBytes = propertyName)
            {
                if (!SDL3.SDL_SetNumberProperty(_props, propertyNameBytes, value))
                    throw UmfException.From(nameof(SDL3.SDL_SetNumberProperty));
            }
        }
    }
}
