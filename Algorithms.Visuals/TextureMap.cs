using System;
using System.Drawing;
using System.Drawing.Imaging;
using Algorithms.Domain;
using OpenGL;
using PixelFormat = OpenGL.PixelFormat;

namespace Algorithms.Visuals
{
    public sealed class TextureMap : IDisposable
    {
        private bool _isInitialized;
        private uint _textureId;
        private RectangleF[] _textureCoordinates;

        public RectangleF this[MapTileVisual visual] => _textureCoordinates[(int) visual];

        public bool IsInitialized => _isInitialized;

        public void Initialize()
        {
            var visuals = Enum.GetValues(typeof(MapTileVisual)) as MapTileVisual[];
            var textureMap = new Bitmap(512, 512, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            _textureCoordinates = new RectangleF[visuals.Length];

            using (var g = Graphics.FromImage(textureMap))
            {
                g.Clear(Color.Black);
                int x = 0;
                int y = 0;

                for (int i = 0; i < visuals.Length; i++)
                {
                    var currentVisual = visuals[i];

                    using (var visualImage = currentVisual.GetImage())
                    {
                        g.DrawImage(visualImage, new Rectangle(x, y, 64, 64),
                            new Rectangle(0, 0, visualImage.Width, visualImage.Height), GraphicsUnit.Pixel);
                    }

                    _textureCoordinates[i] = new RectangleF((float)(x / 512.0), (float)(y / 512.0),
                        (float)((x + 64.0) / 512.0), (float)((y + 64.0) / 512.0));

                    x += 64;

                    if (x >= 512)
                    {
                        x = 0;
                        y += 64;
                    }
                }
            }

            var data = textureMap.LockBits(new Rectangle(0, 0, 512, 512), ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            Gl.Enable(EnableCap.Texture2d);
            _textureId = Gl.GenTexture();
            Gl.BindTexture(TextureTarget.Texture2d, _textureId);
            Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
            Gl.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, 512, 512, 0, PixelFormat.Bgra, PixelType.UnsignedByte,
                data.Scan0);
            Gl.BindTexture(TextureTarget.Texture2d, 0);
            textureMap.UnlockBits(data);
            textureMap.Dispose();
            _isInitialized = true;
        }

        public void Activate()
        {
            Gl.Enable(EnableCap.Texture2d);
            Gl.BindTexture(TextureTarget.Texture2d, _textureId);
            Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
            Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        }

        public void Deactivate()
        {
            Gl.BindTexture(TextureTarget.Texture2d, 0);
            Gl.Disable(EnableCap.Texture2d);
        }

        public void Dispose()
        {
            if (_textureId != 0)
            {
                Gl.DeleteTextures(_textureId);
                _textureId = 0;
            }
        }
    }
}
