using SDL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmfEngine
{
    // TODO: give this a way to be stopped, but still allowing the engine to clean up expired audio playbacks.
    // Probably create some kind of "AudioPlaybackControl" class, which will cause this to be disposed or something
    public unsafe class AudioPlayback : IDisposable
    {
        private SDL_AudioStream* _audioStream;

        internal AudioPlayback(AudioClip clip, SDL_AudioDeviceID deviceId, float gain = 1f, float playbackSpeed = 1f)
        {
            SDL_AudioSpec audioSpec;
            byte* buf;
            uint len;

            try
            {
                audioSpec = clip.GetAudioSpec();
                buf = clip.GetAudioBuf();
                len = clip.GetAudioLen();
            }
            catch (ObjectDisposedException)
            {
                Dispose();
                throw;
            }

            _audioStream = SDL3.SDL_CreateAudioStream(&audioSpec, &audioSpec);
            if (_audioStream == null)
                throw UmfException.From(nameof(SDL3.SDL_CreateAudioStream));

            if (!SDL3.SDL_SetAudioStreamGain(_audioStream, gain))
            {
                Dispose();
                throw UmfException.From(nameof(SDL3.SDL_SetAudioStreamGain));
            }

            if (!SDL3.SDL_SetAudioStreamFrequencyRatio(_audioStream, playbackSpeed))
            {
                Dispose();
                throw UmfException.From(nameof(SDL3.SDL_SetAudioStreamFrequencyRatio));
            }

            // hopefully clip didn't get disposed by another thread before we could put the data
            if (!SDL3.SDL_PutAudioStreamData(_audioStream, (nint)buf, (int)len))
            {
                Dispose();
                throw UmfException.From(nameof(SDL3.SDL_PutAudioStreamData));
            }

            if (!SDL3.SDL_FlushAudioStream(_audioStream))
            {
                Dispose();
                throw UmfException.From(nameof(SDL3.SDL_FlushAudioStream));
            }

            if (!SDL3.SDL_BindAudioStream(deviceId, _audioStream))
            {
                Dispose();
                throw UmfException.From(nameof(SDL3.SDL_BindAudioStream));
            }
        }

        public bool Completed()
        {
            // we don't want to error after this has been disposed, just confirm that it has completed
            if (_audioStream == null)
                return true;

            var bytesAvailable = SDL3.SDL_GetAudioStreamAvailable(_audioStream);
            if (bytesAvailable == -1)
                throw UmfException.From(nameof(SDL3.SDL_GetAudioStreamAvailable));

            return bytesAvailable == 0;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_audioStream != null)
            {
                SDL3.SDL_DestroyAudioStream(_audioStream);
                _audioStream = null;
            }
        }

        ~AudioPlayback()
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
