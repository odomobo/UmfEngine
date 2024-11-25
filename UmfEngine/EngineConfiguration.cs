using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace UmfEngine
{
    public class EngineConfiguration
    {
        public string Title { get; set; } = "UMF Engine";
        // TODO: add logic for whether horizontal or vertical
        // TODO: add logic for whether or not fixed aspect ratio
        // for now, assuming vertical units and dynamic aspect ratio
        // Default of 36 units high at 16x9 resolution means 64x36 units
        public float ScreenSizeInUnits { get; set; } = 36;
        // TODO: allow screen size to be determined by width instead of height
        public bool HasFixedAspectRatio { get; set; } = false;
        // this is only used if HasFixedAspectRatio is true
        public float FixedAspectRatio { get; set; } = 16f/9f;
        public Color FixedAspectRatioMaskingBorderColor { get; set; } = Color.Black;
        public bool AllowResize { get; set; } = true;
        public Vector2 DefaultResolution { get; set; } = new Vector2(1920, 1080);
        public bool DefaultVSync { get; set; } = false;
        public bool DefaultFullscreen { get; set; } = false;
        public bool DefaultCursorVisible {  get; set; } = true;
        public Color DefaultClearColor { get; set; } = Color.Moccasin;
    }
}
