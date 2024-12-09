using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UmfEngine;

namespace UMFE.Framework
{
    public class TextBlock : IGameObject
    {
        public GameObjectTransform Transform { get; private set; }
        public Color Color = Color.GreenYellow;
        private string _text;
        private string[] _splitText;

        public TextBlock(GameObjectTransform? parent, string text, Vector2 position = default, float scale = 1f)
        {
            Transform = new GameObjectTransform(parent, new AffineTransformation(position, 0, scale));
            SetText(text);
        }

        public void SetText(string text)
        {
            _text = text;
            _splitText = text.Split('\n');
            _splitText = _splitText.Select(x => x.Trim('\r')).ToArray();
        }

        public void Draw(Engine e, Camera c)
        {
            var baseTransform = Transform.ToAffineTransformation();
            for (int y = 0; y < _splitText.Length; y++)
            {
                var line = _splitText[y];
                for (int x = 0; x < line.Length; x++)
                {
                    e.DrawCharacter(c, baseTransform, line[x], Color, new Vector2(x * 0.5f, y));
                }
            }
        }

        public void Update(Engine e, Camera c)
        {
            // do nothing
        }
    }
}
