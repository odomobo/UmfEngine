using SDL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmfEngine
{
    public unsafe class AudioClip : IDisposable
    {
        private readonly SDL_AudioSpec _audioSpec;
        private byte* _audioBuf;
        private readonly uint _audioLen;

        internal AudioClip(string path)
        {
            SDL_AudioSpec audioSpec;
            byte* audioBuf;
            uint audioLen;
            if (!SDL3.SDL_LoadWAV(path, &audioSpec, &audioBuf, &audioLen))
                throw UmfException.From(nameof(SDL3.SDL_LoadWAV));

            _audioSpec = audioSpec;
            _audioBuf = audioBuf;
            _audioLen = audioLen;
        }

        internal SDL_AudioSpec GetAudioSpec()
        {
            if (_audioBuf == null)
                throw new ObjectDisposedException(nameof(AudioClip));

            return _audioSpec;
        }

        internal byte* GetAudioBuf()
        {
            if (_audioBuf == null)
                throw new ObjectDisposedException(nameof(AudioClip));

            return _audioBuf;
        }

        internal uint GetAudioLen()
        {
            if (_audioBuf == null)
                throw new ObjectDisposedException(nameof(AudioClip));

            return _audioLen;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_audioBuf != null)
            {
                SDL3.SDL_free(_audioBuf);
                _audioBuf = null;
            }
        }

        ~AudioClip()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
