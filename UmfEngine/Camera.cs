using System.Numerics;

namespace UmfEngine
{
    public struct Camera
    {
        private readonly Vector2 _viewportOffset;
        private readonly Vector2 _position;
        private readonly float _rotation;
        private readonly float _zoom;
        private readonly float _pixelsPerUnit;
        internal float ScalingFactor => _zoom * _pixelsPerUnit;

        // TODO: have camera that centers in different quadrants

        internal Camera(Vector2 viewportOffset, Vector2 position, float rotation, float zoom, float pixelsPerUnit)
        {
            _viewportOffset = viewportOffset;
            _position = position;
            _rotation = rotation;
            _zoom = zoom;
            _pixelsPerUnit = pixelsPerUnit;
        }

        internal static Camera CameraViewportFromScreenInfo(Vector2 viewportOffset, float screenHeightInPixels, float screenHeightInUnits)
        {
            // TODO: fix this
            var position = new Vector2(0, 0);
            var rotation = 0;
            var zoom = 1;
            var pixelsPerUnit = screenHeightInPixels / screenHeightInUnits;
            return new Camera(viewportOffset, position, rotation, zoom, pixelsPerUnit);
        }

        public Vector2 WorldToScreenSpace(Vector2 v)
        {
            v = v - _position;
            v = v.GetRotatedRadians(-_rotation);
            v = v * _zoom * _pixelsPerUnit;
            v += _viewportOffset;
            return v;
        }

        public Vector2 ScreenToWorldSpace(Vector2 v)
        {
            v -= _viewportOffset;
            v = v / (_zoom * _pixelsPerUnit);
            v = v.GetRotatedRadians(_rotation);
            v = v + _position;
            return v;
        }

        public Camera GetTranslated(Vector2 v)
        {
            return new Camera(_viewportOffset, _position + v, _rotation, _zoom, _pixelsPerUnit);
        }

        public Camera GetTranslated(float x, float y)
        {
            return GetTranslated(new Vector2(x, y));
        }

        // TODO: implement this. This should be rotated around the center of the camera, no matter where the camera's origin is from
        //public CameraViewport GetRotated(float angle)
        //{
        //    return new CameraViewport(_position, _rotation + angle, _zoom, _pixelsPerUnit);
        //}

        public Camera GetZoomed(float zoom)
        {
            return new Camera(_viewportOffset, _position, _rotation, _zoom * zoom, _pixelsPerUnit);
        }
    }
}
