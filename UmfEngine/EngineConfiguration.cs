using System.Drawing;
using System.Numerics;

namespace UmfEngine
{
    // For quality settings, let's do something like this:
    // Low: lines with no end caps, no post processing
    // Medium: lines with end caps, no post processing
    // High: lines with end caps, bloom post processing
    // Ultra: 2x oversamping, lines with end caps, bloom post processing
    //
    // Maybe ultra is unnecessary if line drawing uses blurred lines from textures in the first place
    //
    // Something interesting to do: use a different input->output mapping; when brightness gets above a certain level, start fading to white
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
        public Color DefaultClearColor { get; set; } = Color.Black;
    }
}
