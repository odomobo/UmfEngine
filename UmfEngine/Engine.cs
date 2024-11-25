using NLog;
using SDL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace UmfEngine
{
    // Note: There should only be one instance of these
    public unsafe class Engine : IDisposable
    {
        private readonly Logger _logger;
        private SDL_Window* _window;
        private SDL_Renderer* _renderer;
        private bool _shuttingDown = false;
        private readonly EngineConfiguration _configuration;
        private readonly Stopwatch _stopwatch;
        private TimeSpan _lastRenderElapsedTime;
        public float FPS { get; private set; } = 0;
        private TimeSpan _lastFpsCalculatedElapsedTime = TimeSpan.Zero;
        private int _framesSinceLastFpsTime = 0;
        public float ThreadUtilization { get; private set; } = 0;
        private TimeSpan _utilizedSinceLastFpsTime = TimeSpan.Zero;
        public Color ClearColor { get; set; }

        public Engine(EngineConfiguration configuration)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _configuration = configuration;

            if (!SDL3.SDL_Init(SDL_InitFlags.SDL_INIT_VIDEO))
                throw UmfException.From(nameof(SDL3.SDL_Init));

            SDL_WindowFlags windowFlags = default;
            if (configuration.DefaultFullscreen)
                windowFlags |= SDL_WindowFlags.SDL_WINDOW_FULLSCREEN;

            if (configuration.AllowResize)
                windowFlags |= SDL_WindowFlags.SDL_WINDOW_RESIZABLE;

            _window = SDL3.SDL_CreateWindow((Utf8String)configuration.Title, (int)configuration.DefaultResolution.X, (int)configuration.DefaultResolution.Y, windowFlags);
            if (_window == null)
                throw UmfException.From(nameof(SDL3.SDL_CreateWindow));

            _renderer = CreateRenderer(_window);
            if (_renderer == null)
                throw UmfException.From(nameof(SDL3.SDL_CreateRenderer));

            // try to set vsync, but don't raise a fuss if we can't
            TrySetVSync(configuration.DefaultVSync);

            SetCursorVisible(configuration.DefaultCursorVisible);
            ClearColor = configuration.DefaultClearColor;

            _logger.Info("UMF: Started Engine");
            _stopwatch = Stopwatch.StartNew();
            _lastRenderElapsedTime = TimeSpan.Zero;
        }

        private SDL_Renderer* CreateRenderer(SDL_Window* window)
        {
            return SDL3.SDL_CreateRenderer(window, (byte*)null);

            // Disabling this, as it doesn't seem to do anything on my system
            //using var propsBuilder = new PropsBuilder();
            //propsBuilder.SetPointerProperty(SDL3.SDL_PROP_RENDERER_CREATE_WINDOW_POINTER, window);
            //propsBuilder.SetNumberProperty(SDL3.SDL_PROP_RENDERER_CREATE_PRESENT_VSYNC_NUMBER, 1);
            //return SDL3.SDL_CreateRendererWithProperties(propsBuilder.Build());
        }

        public void Dispose()
        {
            if (_renderer != null)
            {
                SDL3.SDL_DestroyRenderer(_renderer);
                _renderer = null;
            }

            if (_window != null)
            {
                SDL3.SDL_DestroyWindow(_window);
                _window = null;
            }

            SDL3.SDL_Quit();
            _logger.Info("UMF: Stopped Engine");
        }

        #region Timing

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

        public Input GetInput()
        {
            // TODO: use text input events to get text the user has typed... I guess

            var keysPressed = new HashSet<SDL_Scancode>();
            var keysRepeated = new HashSet<SDL_Scancode>();

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
            var mouseButtons = SDL3.SDL_GetMouseState(&mouseX, &mouseY);

            // TODO: capture mouse clicked events, capture mouse down state

            return new Input(inputKeyboardState, keysPressed, keysRepeated, new Vector2(mouseX, mouseY))
            {
                ShuttingDown = _shuttingDown,
            };
        }

        #endregion Input

        #region Drawing

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

        public void CompleteDraw(TimeSpan frameTime = default)
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
                    var rightRect = new SDL_FRect { x = windowWidth-halfPadding, y = 0, w = halfPadding, h = windowHeight };

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
                    var bottomRect = new SDL_FRect { x = 0, y = windowHeight - halfPadding, w = windowWidth, h = halfPadding};

                    if (!SDL3.SDL_RenderFillRect(_renderer, &topRect))
                        throw UmfException.From(nameof(SDL3.SDL_RenderFillRect));

                    if (!SDL3.SDL_RenderFillRect(_renderer, &bottomRect))
                        throw UmfException.From(nameof(SDL3.SDL_RenderFillRect));
                }
            }

            if (!SDL3.SDL_RenderPresent(_renderer))
                throw UmfException.From(nameof(SDL3.SDL_RenderPresent));

            SleepForDurationSinceLastFrame(frameTime);
        }

        public void DrawLine(Transform t, Vector2 begin, Vector2 end, float thickness, Color color)
        {
            begin = t.TransformVector(begin);
            end = t.TransformVector(end);
            thickness = thickness * t.Scale;
            // if thickness is 1, draw thin line, otherwise draw thick line
            throw new NotImplementedException();
        }

        public void DrawThinLine(Transform t, Vector2 begin, Vector2 end, Color color)
        {
            begin = t.TransformVector(begin);
            end = t.TransformVector(end);
            InternalDrawThinLine(begin, end, color);
        }

        private void InternalDrawThinLine(Vector2 begin, Vector2 end, Color color)
        {
            SetRenderDrawColor(color);

            // TODO: allow other thicknesses
            if (!SDL3.SDL_RenderLine(_renderer, begin.X, begin.Y, end.X, end.Y))
                throw UmfException.From(nameof(SDL3.SDL_RenderLine));
        }

        private void SetRenderDrawColor(Color color)
        {
            if (!SDL3.SDL_SetRenderDrawColor(_renderer, color.R, color.G, color.B, color.A))
                throw UmfException.From(nameof(SDL3.SDL_SetRenderDrawColor));
        }

        #endregion Drawing

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

        #endregion Util
    }
}
