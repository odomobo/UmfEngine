using System.Drawing;
using System.Numerics;

namespace UmfEngine
{
    public enum Quality
    {
        Low,
        High,
    }

    public class EngineConfiguration
    {
        public string Title { get; set; } = "UMF Engine";
        // Default of 36 units high at 16x9 resolution means 64x36 units
        public float ScreenSizeInUnits { get; set; } = 36;
        public bool HasFixedAspectRatio { get; set; } = false;
        // this is only used if HasFixedAspectRatio is true
        public float FixedAspectRatio { get; set; } = 16f/9f;
        public Color FixedAspectRatioMaskingBorderColor { get; set; } = Color.Black;
        public bool AllowResize { get; set; } = true;
        public Vector2 DefaultResolution { get; set; } = new Vector2(1920, 1080);
        public bool DefaultVSync { get; set; } = false;
        public Quality Quality { get; set; } = Quality.High;
        public bool UseRenderCompatibilityMode { get; set; } = false;
        public bool DefaultFullscreen { get; set; } = false;
        public bool DefaultCursorVisible {  get; set; } = true;
        public Color DefaultClearColor { get; set; } = Color.Moccasin;
    }
}
