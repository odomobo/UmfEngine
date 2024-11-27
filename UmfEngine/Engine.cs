using NLog;
using SDL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace UmfEngine
{
    // Note: There should only be one instance of these
    public unsafe class Engine : IDisposable
    {
        private readonly Logger _logger;
        private readonly EngineConfiguration _configuration;
        private bool _disposed = false;
        
        public Engine(EngineConfiguration configuration)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _configuration = configuration;

            if (!SDL3.SDL_Init(SDL_InitFlags.SDL_INIT_VIDEO | SDL_InitFlags.SDL_INIT_AUDIO))
                throw UmfException.From(nameof(SDL3.SDL_Init));

            WindowInit();
            RendererInit();
            AudioInit();
            TimingInit();

            _logger.Info("UMF: Started Engine");
            
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            AudioDispose();

            RendererDispose();

            SDL3.SDL_DestroyWindow(_window);

            SDL3.SDL_Quit();
            _logger.Info("UMF: Stopped Engine");
        }

        public void CompleteFrame(TimeSpan frameTime = default)
        {
            RenderFrame();
            AudioCleanupOldPlaybacks();
            SleepForDurationSinceLastFrame(frameTime);
        }

        #region Timing

        private Stopwatch _stopwatch;
        private TimeSpan _lastRenderElapsedTime;
        public float FPS { get; private set; } = 0;
        private TimeSpan _lastFpsCalculatedElapsedTime = TimeSpan.Zero;
        private int _framesSinceLastFpsTime = 0;
        public float ThreadUtilization { get; private set; } = 0;
        private TimeSpan _utilizedSinceLastFpsTime = TimeSpan.Zero;

        private void TimingInit()
        {
            _stopwatch = Stopwatch.StartNew();
            _lastRenderElapsedTime = TimeSpan.Zero;
        }

        private void SleepForDurationSinceLastFrame(TimeSpan duration)
        {
            var totalElapsed = _stopwatch.Elapsed;
            var deltaElapsed = totalElapsed - _lastRenderElapsedTime;

            if (deltaElapsed > duration)
            {
                // render took too long; don't sleep, and start counting from now
                _lastRenderElapsedTime = totalElapsed;
            }
            else
            {
                // render was completed in the allotted timeframe; sleep the rest of the time,
                // and regardless start counting from the last completed frame time (to reduce jitter)
                var sleepTime = duration - deltaElapsed;
                Thread.Sleep(sleepTime);
                _lastRenderElapsedTime += duration;
            }

            UpdateFpsAndThreadUtilization(_lastRenderElapsedTime, deltaElapsed);
        }

        private void UpdateFpsAndThreadUtilization(TimeSpan asOfElapsedTime, TimeSpan utilization)
        {
            _framesSinceLastFpsTime++;
            _utilizedSinceLastFpsTime += utilization;

            var timeSinceLastFpsUpdate = asOfElapsedTime - _lastFpsCalculatedElapsedTime;
            if (timeSinceLastFpsUpdate >= TimeSpan.FromSeconds(1))
            {
                FPS = (float)(_framesSinceLastFpsTime / timeSinceLastFpsUpdate.TotalSeconds);
                _lastFpsCalculatedElapsedTime = asOfElapsedTime;

                ThreadUtilization = (float)(_utilizedSinceLastFpsTime.TotalSeconds / timeSinceLastFpsUpdate.TotalSeconds);
                _utilizedSinceLastFpsTime = TimeSpan.Zero;

                _framesSinceLastFpsTime = 0;
            }
        }

        #endregion Timing

        #region Window Management

        private SDL_Window* _window;

        private void WindowInit()
        {
            SDL_WindowFlags windowFlags = default;
            if (_configuration.DefaultFullscreen)
                windowFlags |= SDL_WindowFlags.SDL_WINDOW_FULLSCREEN;

            if (_configuration.AllowResize)
                windowFlags |= SDL_WindowFlags.SDL_WINDOW_RESIZABLE;

            _window = SDL3.SDL_CreateWindow((Utf8String)_configuration.Title, (int)_configuration.DefaultResolution.X, (int)_configuration.DefaultResolution.Y, windowFlags);
            if (_window == null)
                throw UmfException.From(nameof(SDL3.SDL_CreateWindow));
        }

        private (int w, int h) GetWindowSize()
        {
            int w, h;
            if (!SDL3.SDL_GetWindowSize(_window, &w, &h))
                throw UmfException.From(nameof(SDL3.SDL_GetWindowSize));
            return (w, h);
        }

        private Rectangle GetEffectiveViewport()
        {
            // Note: respect_units_width, if that ever becomes an option

            var (windowWidth, windowHeight) = GetWindowSize();

            if (!_configuration.HasFixedAspectRatio)
            {
                return new Rectangle(0, 0, windowWidth, windowHeight);
            }
            else
            {
                var windowsAspectRatio = (float)windowWidth / windowHeight;
                // this means window is wider than it should be
                if (windowsAspectRatio > _configuration.FixedAspectRatio)
                {
                    var expectedWidth = windowHeight * _configuration.FixedAspectRatio;
                    var halfPadding = (windowWidth - expectedWidth) / 2;
                    return new Rectangle((int)halfPadding, 0, (int)expectedWidth, windowHeight);
                }
                else
                {
                    // this means the window is taller than it should be
                    var expectedHeight = windowWidth / _configuration.FixedAspectRatio;
                    var halfPadding = (windowHeight - expectedHeight) / 2;
                    return new Rectangle(0, (int)halfPadding, windowWidth, (int)expectedHeight);
                }
            }
        }

        public Vector2 GetScreenDimensionsInUnits()
        {
            // Note: respect_units_width, if that ever becomes an option

            if (_configuration.HasFixedAspectRatio)
            {
                return new Vector2(_configuration.ScreenSizeInUnits * _configuration.FixedAspectRatio, _configuration.ScreenSizeInUnits);
            }

            var (w, h) = GetWindowSize();
            float widthInTermsOfHeight = (float)w / h;
            return new Vector2(_configuration.ScreenSizeInUnits * widthInTermsOfHeight, _configuration.ScreenSizeInUnits);
        }

        public bool IsFullscreen()
        {
            var flags = SDL3.SDL_GetWindowFlags(_window);
            return flags.HasFlag(SDL_WindowFlags.SDL_WINDOW_FULLSCREEN);
        }

        public void SetFullscreen(bool fullscreen)
        {
            // Note: this can be used to enable exclusive fullscreen instead of borderless fullscreen
            #if false
            
            var display = SDL3.SDL_GetDisplayForWindow(_window);
            var displayMode = SDL3.SDL_GetCurrentDisplayMode(display);

            Console.WriteLine($"Display {displayMode->displayID}: {displayMode->w}x{displayMode->h}@{displayMode->refresh_rate}");

            if (!SDL3.SDL_SetWindowFullscreenMode(_window, displayMode))
                throw UmfException.From(nameof(SDL3.SDL_SetWindowFullscreenMode));
            #endif

            // TODO: do something if this fails???? Let the caller know or something
            SDL3.SDL_SetWindowFullscreen(_window, fullscreen);

            // if this fails... I guess no biggie
            SDL3.SDL_SyncWindow(_window);

            if (fullscreen)
                _logger.Debug("UMF: Set Fullscreen");
            else
                _logger.Debug("UMF: Set Windowed");
        }

        public void ToggleFullscreen()
        {
            SetFullscreen(!IsFullscreen());
        }

        public bool TrySetVSync(bool vsync)
        {
            int vsyncValue = vsync ? 1 : 0;
            return SDL3.SDL_SetRenderVSync(_renderer, vsyncValue);
        }

        public bool GetVSync()
        {
            int vsync;
            if (!SDL3.SDL_GetRenderVSync(_renderer, &vsync))
                throw UmfException.From(nameof(SDL3.SDL_GetRenderVSync));

            return vsync != 0;
        }

        public bool WindowHasFocus()
        {
            var flags = SDL3.SDL_GetWindowFlags(_window);
            return flags.HasFlag(SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS);
        }

        public void SetCursorVisible(bool visible)
        {
            if (visible)
                SDL3.SDL_ShowCursor();
            else
                SDL3.SDL_HideCursor();
        }

        #endregion Window Management

        #region Input

        private bool _shuttingDown = false;
        public Input Input { get; private set; }

        public Input GetInput()
        {
            // TODO: use text input events to get text the user has typed... I guess

            var keysPressed = new HashSet<SDL_Scancode>();
            var keysRepeated = new HashSet<SDL_Scancode>();
            var mouseButtonsPressed = new HashSet<SDLButton>();

            SDL_Event e;
            while (SDL3.SDL_PollEvent(&e))
            {
                switch (e.Type)
                {
                    case SDL_EventType.SDL_EVENT_QUIT:
                        _shuttingDown = true;
                        break;

                    case SDL_EventType.SDL_EVENT_KEY_DOWN:
                        if (e.key.repeat)
                        {
                            keysRepeated.Add(e.key.scancode);
                        }
                        else
                        {
                            keysPressed.Add(e.key.scancode);
                        }
                        break;

                    case SDL_EventType.SDL_EVENT_MOUSE_BUTTON_DOWN:
                        mouseButtonsPressed.Add(e.button.Button);
                        break;

                    // TODO: find all keys pressed
                }
            }

            int numKeys;
            var sdlKeyboardState = SDL3.SDL_GetKeyboardState(&numKeys);
            var inputKeyboardState = new bool[numKeys];
            for (int i = 0; i < numKeys; i++)
            {
                inputKeyboardState[i] = sdlKeyboardState[i];
            }

            float mouseX, mouseY;
            var mouseButtonsDown = SDL3.SDL_GetMouseState(&mouseX, &mouseY);

            // TODO: capture mouse clicked events, capture mouse down state

            Input = new Input(
                inputKeyboardState, 
                keysPressed, 
                keysRepeated, 
                new Vector2(mouseX, mouseY),
                mouseButtonsDown, 
                mouseButtonsPressed)
            {
                ShuttingDown = _shuttingDown,
            };

            return Input;
        }

        #endregion Input

        #region Drawing

        private SDL_Renderer* _renderer;
        public Color ClearColor { get; set; }
        public Quality Quality { get; set; }

        private void RendererInit()
        {
            _renderer = SDL3.SDL_CreateRenderer(_window, (byte*)null);
            if (_renderer == null)
                throw UmfException.From(nameof(SDL3.SDL_CreateRenderer));

            // try to set vsync, but don't raise a fuss if we can't
            TrySetVSync(_configuration.DefaultVSync);

            SetCursorVisible(_configuration.DefaultCursorVisible);
            ClearColor = _configuration.DefaultClearColor;
            Quality = _configuration.Quality;

            CreateCircleSurfaceAndTexture(512);
        }

        private void RendererDispose()
        {
            SDL3.SDL_DestroyTexture(_circleTexture);
            SDL3.SDL_DestroySurface(_circleSurface);
            SDL3.SDL_DestroyRenderer(_renderer);
        }

        private List<string> GetRenderersList()
        {
            var ret = new List<string>();
            var numberRenderers = SDL3.SDL_GetNumRenderDrivers();
            for (int i = 0; i < numberRenderers; i++)
            {
                var renderDriver = SDL3.SDL_GetRenderDriver(i);
                if (renderDriver == null)
                    throw new Exception($"Got null when calling {nameof(SDL3.SDL_GetRenderDriver)}");

                ret.Add(renderDriver);
            }
            return ret;
        }

        private SDL_Surface* _circleSurface;
        private SDL_Texture* _circleTexture;
        private void CreateCircleSurfaceAndTexture(int diameter)
        {
            float radius = diameter / 2f;
            float centerPoint = (diameter-1) / 2f; // the centerpoint is halfway between the min (0) and max (diameter-1) pixel
            float radiusSquared = radius*radius;

            _circleSurface = SDL3.SDL_CreateSurface(diameter, diameter, SDL3.SDL_PIXELFORMAT_RGBA32);
            if (_circleSurface == null)
                throw UmfException.From(nameof(SDL3.SDL_CreateSurface));

            if (!SDL3.SDL_LockSurface(_circleSurface))
                throw UmfException.From(nameof(SDL3.SDL_LockSurface));

            for (int i = 0; i < diameter; i++)
            {
                var pixel = (byte*)_circleSurface->pixels + (_circleSurface->pitch*i);
                for (int j = 0; j < diameter; j++)
                {
                    float x = i - centerPoint;
                    float y = j - centerPoint;

                    var distanceSquared = x*x + y*y;
                    if (distanceSquared <= radiusSquared)
                    {
                        // solid white color
                        *pixel++ = 0xFF;
                        *pixel++ = 0xFF;
                        *pixel++ = 0xFF;
                        *pixel++ = 0xFF;
                    }
                    else
                    {
                        // transparent white color
                        *pixel++ = 0xFF;
                        *pixel++ = 0xFF;
                        *pixel++ = 0xFF;
                        *pixel++ = 0x00;
                    }
                }
            }

            SDL3.SDL_UnlockSurface(_circleSurface);

            _circleTexture = SDL3.SDL_CreateTextureFromSurface(_renderer, _circleSurface);
            if (_circleTexture == null)
                throw UmfException.From(nameof(SDL3.SDL_CreateTextureFromSurface));
        }

        public Transform GetTransform()
        {
            // Note: respect_units_width, if that ever becomes an option

            var viewport = GetEffectiveViewport();

            var transform = new Transform();
            transform = transform.GetTranslated(viewport.X, viewport.Y);
            transform = transform.GetScaled(viewport.Height / _configuration.ScreenSizeInUnits);

            // TODO: allow for transform to be selected from other coordinates, like upper right, lower center, right center, very center, etc.
            return transform;
        }

        public void ClearScreen(Color? color = null)
        {
            if (color == null)
                color = ClearColor;

            SetRenderDrawColor(color.Value);

            if (!SDL3.SDL_RenderClear(_renderer))
                throw UmfException.From(nameof(SDL3.SDL_RenderClear));
        }

        private void DrawMaskingBorders()
        {
            // draw masking borders if fixed aspect ratio is selected
            if (_configuration.HasFixedAspectRatio)
            {
                SetRenderDrawColor(_configuration.FixedAspectRatioMaskingBorderColor);

                var (windowWidth, windowHeight) = GetWindowSize();

                var windowsAspectRatio = (float)windowWidth / windowHeight;
                // this means window is wider than it should be
                if (windowsAspectRatio > _configuration.FixedAspectRatio)
                {
                    var expectedWidth = windowHeight * _configuration.FixedAspectRatio;
                    var halfPadding = (int)((windowWidth - expectedWidth) / 2);
                    var leftRect = new SDL_FRect { x = 0, y = 0, w = halfPadding, h = windowHeight };
                    var rightRect = new SDL_FRect { x = windowWidth - halfPadding, y = 0, w = halfPadding, h = windowHeight };

                    if (!SDL3.SDL_RenderFillRect(_renderer, &leftRect))
                        throw UmfException.From(nameof(SDL3.SDL_RenderFillRect));

                    if (!SDL3.SDL_RenderFillRect(_renderer, &rightRect))
                        throw UmfException.From(nameof(SDL3.SDL_RenderFillRect));
                }
                else
                {
                    // this means the window is taller than it should be
                    var expectedHeight = windowWidth / _configuration.FixedAspectRatio;
                    var halfPadding = (int)((windowHeight - expectedHeight) / 2);

                    var topRect = new SDL_FRect { x = 0, y = 0, w = windowWidth, h = halfPadding };
                    var bottomRect = new SDL_FRect { x = 0, y = windowHeight - halfPadding, w = windowWidth, h = halfPadding };

                    if (!SDL3.SDL_RenderFillRect(_renderer, &topRect))
                        throw UmfException.From(nameof(SDL3.SDL_RenderFillRect));

                    if (!SDL3.SDL_RenderFillRect(_renderer, &bottomRect))
                        throw UmfException.From(nameof(SDL3.SDL_RenderFillRect));
                }
            }
        }

        private void RenderFrame()
        {
            DrawMaskingBorders();

            if (!SDL3.SDL_RenderPresent(_renderer))
                throw UmfException.From(nameof(SDL3.SDL_RenderPresent));
        }

        public void DrawLine(Transform t, float thickness, Color color, Vector2 begin, Vector2 end)
        {
            begin = t.TransformVector(begin);
            end = t.TransformVector(end);
            thickness = thickness * t.Scale;
            if (thickness <= 1)
            {
                // TODO: scale color opacity with thickness?
                InternalDrawLine(1, color, begin, end);
            }
            else
            {
                InternalDrawLine(thickness, color, begin, end);
            }
        }

        public void DrawLine(Transform t, float thickness, Color color, float x1, float y1, float x2, float y2)
        {
            DrawLine(t, thickness, color, new Vector2(x1, y1), new Vector2(x2, y2));
        }

        public void DrawLines(Transform t, float thickness, Color color, params Vector2[] vectors)
        {
            if (vectors.Length < 2)
                throw new InvalidOperationException($"{nameof(DrawLines)} must be called with at least 2 vectors");

            for (int i = 0; i <= vectors.Length-2; i++)
            {
                DrawLine(t, thickness, color, vectors[i], vectors[i+1]);
            }
        }

        public void DrawLinesClosed(Transform t, float thickness, Color color, params Vector2[] vectors)
        {
            if (vectors.Length < 2)
                throw new InvalidOperationException($"{nameof(DrawLinesClosed)} must be called with at least 2 vectors");

            for (int i = 0; i <= vectors.Length-2; i++)
            {
                DrawLine(t, thickness, color, vectors[i], vectors[i + 1]);
            }
            DrawLine(t, thickness, color, vectors[vectors.Length-1], vectors[0]);
        }

        public void DrawLines(Transform t, float thickness, Color color, params float[] coords)
        {
            if (coords.Length < 4)
                throw new InvalidOperationException($"{nameof(DrawLines)} must be called with at least 4 coords");

            if (coords.Length % 2 == 1)
                throw new InvalidOperationException($"{nameof(DrawLines)} must be called with an even number of coords");

            // stride of 2
            for (int i = 0; i <= coords.Length-4; i += 2)
            {
                DrawLine(t, thickness, color, coords[i], coords[i+1], coords[i+2], coords[i+3]);
            }
        }

        public void DrawLinesClosed(Transform t, float thickness, Color color, params float[] coords)
        {
            if (coords.Length < 4)
                throw new InvalidOperationException($"{nameof(DrawLinesClosed)} must be called with at least 4 coords");

            if (coords.Length % 2 == 1)
                throw new InvalidOperationException($"{nameof(DrawLinesClosed)} must be called with an even number of coords");

            for (int i = 0; i <= coords.Length-4; i += 2)
            {
                DrawLine(t, thickness, color, coords[i], coords[i+1], coords[i+2], coords[i+3]);
            }
            DrawLine(t, thickness, color, coords[coords.Length-2], coords[coords.Length-1], coords[0], coords[1]);
        }

        public void DrawThinLine(Transform t, Color color, Vector2 begin, Vector2 end)
        {
            begin = t.TransformVector(begin);
            end = t.TransformVector(end);
            
            InternalDrawLine(1, color, begin, end);
        }

        public void DrawThinLine(Transform t, Color color, float x1, float y1, float x2, float y2)
        {
            DrawThinLine(t, color, new Vector2(x1, y1), new Vector2(x2, y2));
        }

        private void InternalDrawLine(float thickness, Color color, Vector2 begin, Vector2 end)
        {
            // TODO: put this into the batch geometry renderer instead of doing it here
            if (Quality == Quality.High && thickness > 1)
            {
                InternalDrawCircle(thickness, color, begin);
                InternalDrawCircle(thickness, color, end);
            }

            // TODO: use batch geometry renderer instead of this

            var direction = end - begin;
            var perpendicular = new Vector2(direction.Y, -direction.X);
            var normalizedPerpendicular = perpendicular / perpendicular.Length();
            var offset = normalizedPerpendicular * thickness / 2;

            var fcolor = ColorToSdlFColor(color);

            // this is already in the format that SDL_RenderGeometryRaw wants; it's a struct, so its layout is well-defined
            Span<Vector2> vcoords =
            [
                begin + offset,
                begin - offset,
                end - offset,
                end + offset,
            ];

            Span<SDL_FColor> colors =
            [
                fcolor,
                fcolor,
                fcolor,
                fcolor,
            ];

            Span<byte> indices =
            [
                0,
                1,
                2,
                2,
                3,
                0,
            ];

            fixed (Vector2* xysPointer = &vcoords[0])
            fixed (SDL_FColor* colorsPointer = &colors[0])
            fixed (byte* indicesPointer = &indices[0])
            {
                if (!SDL3.SDL_RenderGeometryRaw(_renderer, null,
                    (float*)xysPointer, sizeof(float)*2,
                    colorsPointer, sizeof(SDL_FColor),
                    null, 0,
                    4,
                    (nint)indicesPointer, 6, sizeof(byte)))
                {
                    throw UmfException.From(nameof(SDL3.SDL_RenderGeometryRaw));
                }
            }
        }

        // Don't use this, it's slow. Better to use InternalDrawLine with a thickness of 1
        [Obsolete]
        private void InternalDrawThinLine(Color color, Vector2 begin, Vector2 end)
        {
            SetRenderDrawColor(color);

            // TODO: allow other thicknesses
            if (!SDL3.SDL_RenderLine(_renderer, begin.X, begin.Y, end.X, end.Y))
                throw UmfException.From(nameof(SDL3.SDL_RenderLine));
        }

        public void DrawCircle(Transform t, float diameter, Color color, float x, float y)
        {
            DrawCircle(t, diameter, color, new Vector2(x, y));
        }

        public void DrawCircle(Transform t, float diameter, Color color, Vector2 coord)
        {
            coord = t.TransformVector(coord);
            diameter = diameter * t.Scale;
            if (diameter < 1.45f)
            {
                // draw a square instead, minimum of 1.0f
                InternalDrawCircle(1.45f, color, coord);
            }
            else
            {
                InternalDrawCircle(diameter, color, coord);
            }
        }

        private void InternalDrawCircle(float diameter, Color color, Vector2 coord)
        {
            // TODO: this should add to the BatchGeometryRenderer

            float radius = diameter/2;

            var fcolor = ColorToSdlFColor(color);

            // this is already in the format that SDL_RenderGeometryRaw wants; it's a struct, so its layout is well-defined
            Span<Vector2> vcoords =
            [
                new Vector2(coord.X-radius, coord.Y-radius), // maybe reverse order???
                new Vector2(coord.X-radius, coord.Y+radius),
                new Vector2(coord.X+radius, coord.Y+radius),
                new Vector2(coord.X+radius, coord.Y-radius),
            ];

            Span<SDL_FColor> colors =
            [
                fcolor,
                fcolor,
                fcolor,
                fcolor,
            ];

            Span<float> uvs =
            [
                0, 0,
                0, 1,
                1, 1,
                1, 0,
            ];

            Span<byte> indices =
            [
                0,
                1,
                2,
                2,
                3,
                0,
            ];

            fixed (Vector2* xysPointer = &vcoords[0])
            fixed (SDL_FColor* colorsPointer = &colors[0])
            fixed (float* uvsPointer = &uvs[0])
            fixed (byte* indicesPointer = &indices[0])
            {
                if (!SDL3.SDL_RenderGeometryRaw(_renderer, _circleTexture,
                    (float*)xysPointer, sizeof(float)*2,
                    colorsPointer, sizeof(SDL_FColor),
                    uvsPointer, sizeof(float)*2,
                    4,
                    (nint)indicesPointer, 6, sizeof(byte)))
                {
                    throw UmfException.From(nameof(SDL3.SDL_RenderGeometryRaw));
                }
            }
        }

        private void SetRenderDrawColor(Color color)
        {
            if (!SDL3.SDL_SetRenderDrawColor(_renderer, color.R, color.G, color.B, color.A))
                throw UmfException.From(nameof(SDL3.SDL_SetRenderDrawColor));
        }

        private static SDL_FPoint Vector2ToSdlFPoint(Vector2 vector)
        {
            return new SDL_FPoint
            {
                x = vector.X,
                y = vector.Y,
            };
        }

        private static SDL_FColor ColorToSdlFColor(Color color)
        {
            return new SDL_FColor
            {
                r = color.R / 255f,
                g = color.G / 255f,
                b = color.B / 255f,
                a = color.A / 255f,
            };
        }

#endregion Drawing

        #region Audio

        private SDL_AudioDeviceID _audioOutDevice;
        private List<AudioPlayback> _audioPlaybacks = new List<AudioPlayback>();

        private void AudioInit()
        {
            // start audio
            _audioOutDevice = SDL3.SDL_OpenAudioDevice(SDL3.SDL_AUDIO_DEVICE_DEFAULT_PLAYBACK, null);
            if (_audioOutDevice == 0)
                throw UmfException.From(nameof(SDL3.SDL_OpenAudioDevice));
        }

        private void AudioDispose()
        {
            foreach (var audioPlayback in _audioPlaybacks)
            {
                audioPlayback.Dispose();
            }

            // this probably isn't needed
            SDL3.SDL_CloseAudioDevice(SDL3.SDL_AUDIO_DEVICE_DEFAULT_PLAYBACK);
        }

        private void AudioCleanupOldPlaybacks()
        {
            for (int i = _audioPlaybacks.Count-1; i >= 0; i--)
            {
                var audioPlayback = _audioPlaybacks[i];
                if (audioPlayback.Completed())
                {
                    audioPlayback.Dispose();
                    _audioPlaybacks.RemoveAt(i);
                }
            }
        }

        public AudioClip LoadAudioClip(string path)
        {
            return new AudioClip(path);
        }

        public void PlayAudioClip(AudioClip clip, float gain = 1f, float playbackSpeed = 1f)
        {
            _audioPlaybacks.Add(new AudioPlayback(clip, _audioOutDevice, gain, playbackSpeed));
        }

        #endregion Audio

        #region Util

        public static Color ColorFromHSV(float hue, float saturation, float value)
        {
            hue = hue % 360;
            if (hue < 0)
            {
                hue += 360;
            }
            int hi = Convert.ToInt32(MathF.Floor(hue / 60)) % 6;
            float f = hue / 60 - MathF.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }

        public static float RadiansToDegrees(float radians)
        {
            return 360f * (radians / MathF.Tau);
        }

        public static float DegreesToRadians(float degrees)
        {
            return (degrees / 360f) * MathF.Tau;
        }

        #endregion Util
    }
}
