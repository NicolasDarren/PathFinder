using System;
using System.Drawing;
using Algorithms.Domain;
using OpenGL;

namespace Algorithms.Visuals
{
    public class MapVertexBuffer
    {
        private readonly Map _map;
        private readonly TextureMap _textureMap;
        private Size _tileSize;
        private readonly float[] _positions;
        private readonly byte[] _colors;
        private readonly float[] _textures;

        public MapVertexBuffer(Map map, TextureMap textureMap, Size tileSize)
        {
            _map = map;
            _textureMap = textureMap;
            _tileSize = tileSize;
            _positions = new float[_map.Width * _map.Height * 6 * 2];
            _colors = new byte[_map.Width * _map.Height * 6 * 4];
            _textures = new float[_map.Width * _map.Height * 6 * 2];

            for (int x = 0; x < _map.Width; x++)
            for (int y = 0; y < _map.Height; y++)
            {
                int baseIndex = _map.GetTileIndex(x, y) * 6;
                int pIndex = baseIndex * 2;

                _positions[pIndex + 0] = x * _tileSize.Width;
                _positions[pIndex + 1] = y * _tileSize.Height;
                _positions[pIndex + 2] = (x + 1) * _tileSize.Width;
                _positions[pIndex + 3] = y * _tileSize.Height;
                _positions[pIndex + 4] = (x + 1) * _tileSize.Width;
                _positions[pIndex + 5] = (y + 1) * _tileSize.Height;

                _positions[pIndex + 6] = (x + 1) * _tileSize.Width;
                _positions[pIndex + 7] = (y + 1) * _tileSize.Height;
                _positions[pIndex + 8] = x * _tileSize.Width;
                _positions[pIndex + 9] = (y + 1) * _tileSize.Height;
                _positions[pIndex + 10] = x * _tileSize.Width;
                _positions[pIndex + 11] = y * _tileSize.Height;

                var tile = _map.GetTile(new MapLocation(x, y));
                var texelCoordinates = _textureMap[tile.Visual];
                _textures[pIndex + 0] = texelCoordinates.X;
                _textures[pIndex + 1] = texelCoordinates.Y;
                _textures[pIndex + 2] = texelCoordinates.Width;
                _textures[pIndex + 3] = texelCoordinates.Y;
                _textures[pIndex + 4] = texelCoordinates.Width;
                _textures[pIndex + 5] = texelCoordinates.Height;

                _textures[pIndex + 6] = texelCoordinates.Width;
                _textures[pIndex + 7] = texelCoordinates.Height;
                _textures[pIndex + 8] = texelCoordinates.X;
                _textures[pIndex + 9] = texelCoordinates.Height;
                _textures[pIndex + 10] = texelCoordinates.X;
                _textures[pIndex + 11] = texelCoordinates.Y;
            }

            for (int i = 0; i < _colors.Length; i++)
                _colors[i] = 255;

            _map.TileChanged += OnTileChanged;
        }

        public void SetTileColor(MapLocation mapLocation, byte r, byte g, byte b, byte a)
        {
            int baseIndex = _map.GetTileIndex(mapLocation.X, mapLocation.Y) * 6;
            int pIndex = baseIndex * 4;

            for (int i = 0; i < 6; i++)
            {
                _colors[pIndex + (i * 4) + 0] = r;
                _colors[pIndex + (i * 4) + 1] = g;
                _colors[pIndex + (i * 4) + 2] = b;
                _colors[pIndex + (i * 4) + 3] = a;
            }
        }

        public void SetTileSize(int tileSize)
        {
            _tileSize = new Size(tileSize, tileSize);

            for (int x = 0; x < _map.Width; x++)
            for (int y = 0; y < _map.Height; y++)
            {
                int baseIndex = _map.GetTileIndex(x, y) * 6;
                int pIndex = baseIndex * 2;

                _positions[pIndex + 0] = x * _tileSize.Width;
                _positions[pIndex + 1] = y * _tileSize.Height;
                _positions[pIndex + 2] = (x + 1) * _tileSize.Width;
                _positions[pIndex + 3] = y * _tileSize.Height;
                _positions[pIndex + 4] = (x + 1) * _tileSize.Width;
                _positions[pIndex + 5] = (y + 1) * _tileSize.Height;

                _positions[pIndex + 6] = (x + 1) * _tileSize.Width;
                _positions[pIndex + 7] = (y + 1) * _tileSize.Height;
                _positions[pIndex + 8] = x * _tileSize.Width;
                _positions[pIndex + 9] = (y + 1) * _tileSize.Height;
                _positions[pIndex + 10] = x * _tileSize.Width;
                _positions[pIndex + 11] = y * _tileSize.Height;
            }
        }

        private void OnTileChanged(MapLocation location, MapTile oldValue, MapTile newValue)
        {
            if (oldValue.Visual != newValue.Visual)
            {
                int baseIndex = _map.GetTileIndex(location) * 6;
                int pIndex = baseIndex * 2;
                var texelCoordinates = _textureMap[newValue.Visual];
                _textures[pIndex + 0] = texelCoordinates.X;
                _textures[pIndex + 1] = texelCoordinates.Y;
                _textures[pIndex + 2] = texelCoordinates.Width;
                _textures[pIndex + 3] = texelCoordinates.Y;
                _textures[pIndex + 4] = texelCoordinates.Width;
                _textures[pIndex + 5] = texelCoordinates.Height;

                _textures[pIndex + 6] = texelCoordinates.Width;
                _textures[pIndex + 7] = texelCoordinates.Height;
                _textures[pIndex + 8] = texelCoordinates.X;
                _textures[pIndex + 9] = texelCoordinates.Height;
                _textures[pIndex + 10] = texelCoordinates.X;
                _textures[pIndex + 11] = texelCoordinates.Y;

                pIndex = baseIndex * 4;

                for (int i = 0; i < 24; i++)
                    _colors[pIndex + i] = 255;
            }
        }

        public IDisposable Activate()
        {
            return new Activation(this);
        }

        public void Render()
        {
            Gl.DrawArrays(PrimitiveType.Triangles, 0, _positions.Length / 2);
        }

        private sealed class Activation : IDisposable
        {
            private MemoryLock _positionLock;
            private MemoryLock _colorLock;
            private MemoryLock _textureLock;

            public Activation(MapVertexBuffer source)
            {
                _positionLock = new MemoryLock(source._positions);
                _colorLock = new MemoryLock(source._colors);
                _textureLock = new MemoryLock(source._textures);

                Gl.VertexPointer(2, VertexPointerType.Float, 0, _positionLock.Address);
                Gl.EnableClientState(EnableCap.VertexArray);
                Gl.ColorPointer(4, ColorPointerType.UnsignedByte, 0, _colorLock.Address);
                Gl.EnableClientState(EnableCap.ColorArray);
                Gl.TexCoordPointer(2, TexCoordPointerType.Float, 0, _textureLock.Address);
                Gl.EnableClientState(EnableCap.TextureCoordArray);
            }

            public void Dispose()
            {
                _positionLock.Dispose();
                _colorLock.Dispose();
                _textureLock.Dispose();
            }
        }
    }
}