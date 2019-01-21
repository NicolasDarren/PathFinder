//using System;
//using System.Drawing;
//using System.Runtime.InteropServices;
//using Algorithms.Domain;
//using OpenGL;

//namespace Algorithms.Visuals
//{
//    public class MapOverlayBuffer
//    {
//        private readonly Map _map;
//        private readonly TextureMap _textureMap;
//        private Size _tileSize;
//        private VertexPositionColorTexture[] _vertices;

//        public MapOverlayBuffer(Map map, TextureMap textureMap, Size tileSize)
//        {
//            _map = map;
//            _textureMap = textureMap;
//            _tileSize = tileSize;
//            _vertices = new VertexPositionColorTexture[_map.Width * _map.Height * 6];

//            for (int x = 0; x < _map.Width; x++)
//                for (int y = 0; y < _map.Height; y++)
//                {
//                    var tile = _map.GetTile(new MapLocation(x, y));
//                    var texelCoordinates = _textureMap[tile.Visual];
//                    int pIndex = _map.GetTileIndex(x, y) * 6;

//                    _vertices[pIndex + 0] = new VertexPositionColorTexture
//                    {
//                        X = x * _tileSize.Width,
//                        Y = y * _tileSize.Height,
//                        TexelX = texelCoordinates.X,
//                        TexelY = texelCoordinates.Y,
//                        Red = 255,
//                        Green = 255,
//                        Blue = 255,
//                        Alpha = 255
//                    };

//                    _vertices[pIndex + 1] = new VertexPositionColorTexture
//                    {
//                        X = (x + 1) * _tileSize.Width,
//                        Y = y * _tileSize.Height,
//                        TexelX = texelCoordinates.Width,
//                        TexelY = texelCoordinates.Y,
//                        Red = 255,
//                        Green = 255,
//                        Blue = 255,
//                        Alpha = 255
//                    };

//                    _vertices[pIndex + 2] = new VertexPositionColorTexture
//                    {
//                        X = (x + 1) * _tileSize.Width,
//                        Y = (y + 1) * _tileSize.Height,
//                        TexelX = texelCoordinates.Width,
//                        TexelY = texelCoordinates.Height,
//                        Red = 255,
//                        Green = 255,
//                        Blue = 255,
//                        Alpha = 255
//                    };

//                    _vertices[pIndex + 3] = new VertexPositionColorTexture
//                    {
//                        X = (x + 1) * _tileSize.Width,
//                        Y = (y + 1) * _tileSize.Height,
//                        TexelX = texelCoordinates.Width,
//                        TexelY = texelCoordinates.Height,
//                        Red = 255,
//                        Green = 255,
//                        Blue = 255,
//                        Alpha = 255
//                    };

//                    _vertices[pIndex + 4] = new VertexPositionColorTexture
//                    {
//                        X = x * _tileSize.Width,
//                        Y = (y + 1) * _tileSize.Height,
//                        TexelX = texelCoordinates.X,
//                        TexelY = texelCoordinates.Height,
//                        Red = 255,
//                        Green = 255,
//                        Blue = 255,
//                        Alpha = 255
//                    };

//                    _vertices[pIndex + 5] = new VertexPositionColorTexture
//                    {
//                        X = x * _tileSize.Width,
//                        Y = y * _tileSize.Height,
//                        TexelX = texelCoordinates.X,
//                        TexelY = texelCoordinates.Y,
//                        Red = 255,
//                        Green = 255,
//                        Blue = 255,
//                        Alpha = 255
//                    };
//                }
//        }

//        public void SetTileSize(int tileSize)
//        {
//            _tileSize = new Size(tileSize, tileSize);

//            for (int x = 0; x < _map.Width; x++)
//                for (int y = 0; y < _map.Height; y++)
//                {
//                    int baseIndex = _map.GetTileIndex(x, y) * 6;
//                    int pIndex = baseIndex * 2;

//                    _positions[pIndex + 0] = x * _tileSize.Width;
//                    _positions[pIndex + 1] = y * _tileSize.Height;
//                    _positions[pIndex + 2] = (x + 1) * _tileSize.Width;
//                    _positions[pIndex + 3] = y * _tileSize.Height;
//                    _positions[pIndex + 4] = (x + 1) * _tileSize.Width;
//                    _positions[pIndex + 5] = (y + 1) * _tileSize.Height;

//                    _positions[pIndex + 6] = (x + 1) * _tileSize.Width;
//                    _positions[pIndex + 7] = (y + 1) * _tileSize.Height;
//                    _positions[pIndex + 8] = x * _tileSize.Width;
//                    _positions[pIndex + 9] = (y + 1) * _tileSize.Height;
//                    _positions[pIndex + 10] = x * _tileSize.Width;
//                    _positions[pIndex + 11] = y * _tileSize.Height;
//                }
//        }

//        public IDisposable Activate()
//        {
//            return new Activation(this);
//        }

//        public void Render()
//        {
//            Gl.DrawArrays(PrimitiveType.Triangles, 0, _positions.Length / 2);
//        }

//        private sealed class Activation : IDisposable
//        {
//            private MemoryLock _vertexLock;

//            public Activation(MapOverlayBuffer source)
//            {
//                _vertexLock = new MemoryLock(source._vertices);

//                Gl.VertexPointer(2, VertexPointerType.Float, VertexPositionColorTexture.PositionStride, _vertexLock.Address + VertexPositionColorTexture.PositionOffset);
//                Gl.EnableClientState(EnableCap.VertexArray);
//                Gl.ColorPointer(4, ColorPointerType.UnsignedByte, VertexPositionColorTexture.ColorStride, _vertexLock.Address + VertexPositionColorTexture.ColorOffset);
//                Gl.EnableClientState(EnableCap.ColorArray);
//                Gl.TexCoordPointer(2, TexCoordPointerType.Float, VertexPositionColorTexture.TextureStride, _vertexLock.Address + VertexPositionColorTexture.TextureOffset);
//                Gl.EnableClientState(EnableCap.TextureCoordArray);
//            }

//            public void Dispose()
//            {
//                _vertexLock.Dispose();
//            }
//        }

//        [StructLayout(LayoutKind.Explicit, Size = SizeInBytes)]
//        public struct VertexPositionColorTexture
//        {
//            public const int SizeInBytes = 20;
//            public const int PositionOffset = 0;
//            public const int TextureOffset = 8;
//            public const int ColorOffset = 16;
//            public const int PositionStride = SizeInBytes - 8;
//            public const int TextureStride = SizeInBytes - 8;
//            public const int ColorStride = SizeInBytes - 4;

//            [FieldOffset(0)]
//            public float X;
//            [FieldOffset(4)]
//            public float Y;
//            [FieldOffset(8)]
//            public float TexelX;
//            [FieldOffset(12)]
//            public float TexelY;
//            [FieldOffset(16)]
//            public int Color;
//            [FieldOffset(16)]
//            public byte Red;
//            [FieldOffset(17)]
//            public byte Green;
//            [FieldOffset(18)]
//            public byte Blue;
//            [FieldOffset(19)]
//            public byte Alpha;
//        }
//    }
//}