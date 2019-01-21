using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Algorithms.Domain;
using Khronos;
using OpenGL;

namespace Algorithms.Visuals
{
    public partial class MapVisual : UserControl
    {
        private Map _value;
        private GlControl _glControl;
        private TextureMap _textureMap;
        private MapVertexBuffer _mapVertexBuffer;

        private int _tileSize = 32;
        private MapLocation _center;
        private MapLocation _computedCenter;
        private Size _visibleTiles;
        private Rectangle _tileViewport;

        private MapVisualTool _activeTool;
        private MapVisualComponentCollection<MapVisualActuator> _actuators;
        private MapVisualComponentCollection<MapVisualTool> _tools;
        private MapVisualComponentCollection<MapVisualAnnotation> _annotations;

        public int TileSize
        {
            get => _tileSize;
            set => SetTileSize(value);
        }

        public Map Value
        {
            get => _value;
            set => SetValue(value);
        }

        public MapLocation ComputedCenter => _computedCenter;

        public MapLocation Center
        {
            get => _center;
            set => SetCenter(value);
        }

        public MapVisualTool ActiveTool
        {
            get => _activeTool;
            set => SetActiveTool(value);
        }

        public TextureMap TextureMap => _textureMap;

        public MapVisualComponentCollection<MapVisualActuator> Actuators => _actuators;
        public MapVisualComponentCollection<MapVisualTool> Tools => _tools;
        public MapVisualComponentCollection<MapVisualAnnotation> Annotations => _annotations;

        public MapVisual()
        {
            InitializeComponent();
            SetStyle(ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);
            _actuators = new MapVisualComponentCollection<MapVisualActuator>(this);
            _tools = new MapVisualComponentCollection<MapVisualTool>(this);
            _annotations = new MapVisualComponentCollection<MapVisualAnnotation>(this);
            _textureMap = new TextureMap();


            if (!DesignMode)
            {
                _glControl = new GlControl();
                _glControl.Animation = true;
                _glControl.AnimationTimer = false;
                _glControl.BackColor = Color.DimGray;
                _glControl.ColorBits = 24u;
                _glControl.DepthBits = 0u;
                _glControl.Dock = DockStyle.Fill;
                _glControl.Location = new Point(0, 0);
                _glControl.MultisampleBits = 0u;
                _glControl.Name = "RenderControl";
                _glControl.Size = new Size(320, 240);
                _glControl.StencilBits = 0u;
                _glControl.TabIndex = 0;
                _glControl.ContextCreated += OnGlContextCreated;
                _glControl.ContextDestroying += OnGlContextDestroying;
                _glControl.ContextUpdate += OnGlContextUpdate;
                _glControl.Render += OnGlRender;
                _glControl.Parent = this;

                _glControl.MouseClick += HandleMouseClick;
                _glControl.MouseDown += HandleMouseDown;
                _glControl.MouseUp += HandleMouseUp;
                _glControl.MouseWheel += HandleMouseWheel;
                _glControl.MouseDoubleClick += HandleMouseDoubleClick;
                _glControl.MouseMove += HandleMouseMove;
                _glControl.MouseCaptureChanged += HandleMouseCaptureChanged;
                _glControl.KeyPress += HandleKeyPress;
                _glControl.KeyDown += HandleKeyDown;
                _glControl.KeyUp += HandleKeyUp;
                _glControl.ClientSizeChanged += _glControl_ClientSizeChanged;
            }
        }

        private void _glControl_ClientSizeChanged(object sender, EventArgs e)
        {
            if (_textureMap.IsInitialized)
            {
                Gl.MatrixMode(MatrixMode.Projection);
                Gl.LoadIdentity();
                Gl.Ortho(0.0, _glControl.ClientSize.Width, 0.0, _glControl.ClientSize.Height, 0.0, 1.0);
            }
        }

        private void OnGlContextCreated(object sender, GlControlEventArgs e)
        {
            GlControl glControl = (GlControl)sender;

            if (Gl.CurrentExtensions != null && Gl.CurrentExtensions.DebugOutput_ARB)
            {
                Gl.DebugMessageCallback(GLDebugProc, IntPtr.Zero);
                Gl.DebugMessageControl(Gl.DebugSource.DontCare, Gl.DebugType.DontCare, Gl.DebugSeverity.DontCare, 0, null, true);
            }

            Gl.MatrixMode(MatrixMode.Projection);
            Gl.LoadIdentity();
            Gl.Ortho(0.0, glControl.ClientSize.Width, 0.0, glControl.ClientSize.Height, 0.0, 1.0);

            if (Gl.CurrentVersion != null && Gl.CurrentVersion.Api == KhronosVersion.ApiGl && glControl.MultisampleBits > 0)
                Gl.Enable(EnableCap.Multisample);

            _textureMap.Initialize();
            Gl.LineWidth(8.0f);

            if (_value != null)
                _mapVertexBuffer = new MapVertexBuffer(_value, _textureMap, new Size(_tileSize, _tileSize));
        }

        private void OnGlContextDestroying(object sender, GlControlEventArgs e)
        {
            _textureMap.Dispose();
        }

        private void OnGlContextUpdate(object sender, GlControlEventArgs e)
        {
        }

        private static void GLDebugProc(Gl.DebugSource source, Gl.DebugType type, uint id, Gl.DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
        {
            string strMessage;

            unsafe
            {
                strMessage = Encoding.ASCII.GetString((byte*)message.ToPointer(), length);
            }

            Console.WriteLine($"{source}, {type}, {severity}: {strMessage}");
        }

        private void OnGlRender(object sender, GlControlEventArgs e)
        {
            Control senderControl = (Control)sender;
            Gl.Viewport(0, 0, senderControl.ClientSize.Width, senderControl.ClientSize.Height);
            Gl.Clear(ClearBufferMask.ColorBufferBit);
            Gl.MatrixMode(MatrixMode.Modelview);
            Gl.LoadIdentity();
            Gl.Translate(-_tileViewport.X * _tileSize, -_tileViewport.Y * _tileSize, 0f);
            _textureMap.Activate();

            using (_mapVertexBuffer.Activate())
            {
                _mapVertexBuffer.Render();
            }

            _textureMap.Deactivate();

            foreach (var annotation in _annotations)
            {
                _textureMap.Activate();
                annotation.Paint(_tileViewport);
                _textureMap.Deactivate();
            }

            var pointToClient = PointToClient(MousePosition);
            _textureMap.Activate();
            _activeTool?.Paint(_tileViewport, GetMapLocationFromClient(pointToClient));
            _textureMap.Deactivate();

        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);
            UpdateConstraints();
        }

        private void UpdateConstraints()
        {
            if (_value == null)
                return;

            var clientSize = ClientSize;

            _visibleTiles = new Size(clientSize.Width / _tileSize +
                                     ((clientSize.Width / _tileSize * _tileSize) != clientSize.Width ? 1 : 0),
                clientSize.Height / _tileSize +
                ((clientSize.Height / _tileSize * _tileSize) != clientSize.Height ? 1 : 0));

            _computedCenter.X = Math.Max(_center.X, _visibleTiles.Width / 2);
            _computedCenter.Y = Math.Max(_center.Y, _visibleTiles.Height / 2);

            Point firstVisibleTile = new Point(_computedCenter.X - _visibleTiles.Width / 2,
                _computedCenter.Y - _visibleTiles.Height / 2);
            Size lastVisibleTile = new Size(
                Math.Min(_visibleTiles.Width + firstVisibleTile.X, _value.Width - firstVisibleTile.X),
                Math.Min(_visibleTiles.Height + firstVisibleTile.Y, _value.Height - firstVisibleTile.Y));
            _tileViewport = new Rectangle(firstVisibleTile, lastVisibleTile);
        }

        public bool IsMapLocationOnScreen(MapLocation location)
        {
            return _tileViewport.Contains(location.X, location.Y);
        }

        public Rectangle GetMapLocationOnClient(MapLocation location)
        {
            int pX = location.X * _tileSize;
            int pY = location.Y * _tileSize;
            return new Rectangle(pX, pY, _tileSize, _tileSize);
        }

        public Rectangle GetMapLocationOnScreen(MapLocation location)
        {
            int pX = (location.X - _tileViewport.X) * _tileSize;
            int pY = (location.Y - _tileViewport.Y) * _tileSize;
            return new Rectangle(pX, pY, _tileSize, _tileSize);
        }

        public MapLocation GetMapLocationFromClient(Point clientPoint)
        {
            clientPoint.Y = _glControl.ClientSize.Height - clientPoint.Y;
            var dX = clientPoint.X / _tileSize;
            var dY = clientPoint.Y / _tileSize;
            return new MapLocation(_tileViewport.X + dX, _tileViewport.Y + dY);
        }

        private void SetActiveTool(MapVisualTool value)
        {
            if (_activeTool == value) return;
            _activeTool?.NotifyToolDeactivated();
            _activeTool = value;
            _activeTool?.NotifyToolActivated();
        }

        private void SetTileSize(int value)
        {
            if (value < 8) value = 8;
            if (value > 64) value = 64;
            if (_tileSize == value) return;
            _tileSize = value;
            _mapVertexBuffer?.SetTileSize(_tileSize);
            UpdateConstraints();
            Invalidate();
        }

        private void SetCenter(MapLocation value)
        {
            if (_center == value) return;
            _center = value;
            UpdateConstraints();
            Invalidate();
        }

        private void SetValue(Map value)
        {
            if (_value == value) return;

            _value = value;

            if (_value != null)
            {
                if (_textureMap.IsInitialized)
                    _mapVertexBuffer = new MapVertexBuffer(_value, _textureMap, new Size(_tileSize, _tileSize));
            }

            UpdateConstraints();
            Invalidate();
        }

        private IEnumerable<MapVisualActuator> GetActuatorsInEventNotificationOrder()
        {
            if (_activeTool != null)
                yield return _activeTool;

            foreach (var actuator in _actuators)
                yield return actuator;
        }

        private void HandleMouseDown(object sender, MouseEventArgs e)
        {
            foreach (var actuator in GetActuatorsInEventNotificationOrder())
                if (actuator.NotifyMouseDown(e))
                    break;
        }

        private void HandleMouseUp(object sender, MouseEventArgs e)
        {
            foreach (var actuator in GetActuatorsInEventNotificationOrder())
                if (actuator.NotifyMouseUp(e))
                    break;
        }

        private void HandleMouseClick(object sender, MouseEventArgs e)
        {
            foreach (var actuator in GetActuatorsInEventNotificationOrder())
                if (actuator.NotifyMouseClick(e))
                    break;

            OnMouseClick(e);
        }

        private void HandleMouseMove(object sender, MouseEventArgs e)
        {
            foreach (var actuator in GetActuatorsInEventNotificationOrder())
                actuator.NotifyMouseMove(e);
        }

        private void HandleMouseDoubleClick(object sender, MouseEventArgs e)
        {
            foreach (var actuator in GetActuatorsInEventNotificationOrder())
                if (actuator.NotifyMouseDoubleClick(e))
                    break;
        }

        private void HandleMouseCaptureChanged(object sender, EventArgs e)
        {
            foreach (var actuator in GetActuatorsInEventNotificationOrder())
                actuator.NotifyMouseCaptureChanged(e);
        }

        private void HandleMouseWheel(object sender, MouseEventArgs e)
        {
            foreach (var actuator in GetActuatorsInEventNotificationOrder())
                if (actuator.NotifyMouseWheel(e))
                    break;
        }

        private void HandleKeyPress(object sender, KeyPressEventArgs e)
        {
            foreach (var actuator in GetActuatorsInEventNotificationOrder())
                if (actuator.NotifyKeyPress(e))
                    break;
        }

        private void HandleKeyDown(object sender, KeyEventArgs e)
        {
            foreach (var actuator in GetActuatorsInEventNotificationOrder())
                if (actuator.NotifyKeyDown(e))
                    break;
        }

        private void HandleKeyUp(object sender, KeyEventArgs e)
        {
            foreach (var actuator in GetActuatorsInEventNotificationOrder())
                actuator.NotifyKeyUp(e);
        }

        public sealed class MapVisualComponentCollection<TComponent> : Collection<TComponent> where TComponent : MapVisualComponent
        {
            private MapVisual _owner;

            public MapVisualComponentCollection(MapVisual owner)
            {
                _owner = owner;
            }

            protected override void InsertItem(int index, TComponent item)
            {
                base.InsertItem(index, item);
                OnElementChanged(null, item);
            }

            protected override void SetItem(int index, TComponent item)
            {
                var existing = this[index];
                base.SetItem(index, item);
                OnElementChanged(existing, item);
            }

            protected override void RemoveItem(int index)
            {
                OnElementChanged(this[index], null);
                base.RemoveItem(index);
            }

            private void OnElementChanged(MapVisualComponent oldElement, TComponent newElement)
            {
                oldElement?.NotifyDeactivated();
                newElement?.NotifyActivated(_owner);
            }

            protected override void ClearItems()
            {
                base.ClearItems();
                _owner.Invalidate();
            }
        }

        public void SetTileColor(MapLocation mapLocation, Color color)
        {
            _mapVertexBuffer.SetTileColor(mapLocation, color.R, color.G, color.B, color.A);
        }
    }
}
