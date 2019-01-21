using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Algorithms.Domain;
using Algorithms.Domain.Procedural;
using Algorithms.TestBed.Implementation;
using Algorithms.Visuals;

namespace Algorithms.TestBed
{
    public partial class Main : Form
    {
        private Map _map;
        private MapTileTemplate[] _templates;
        private RadioButton _lastActive;
        private MapPathfindingTool _pathfindingTool;
        private Pathfinder _pathfinder;

        public Main()
        {
            InitializeComponent();

            mapVisual1.Actuators.Add(new WasdActuator());
            mapVisual1.Actuators.Add(new ZoomActuator());
            _map = new Map(Atts.MaxLocsX, Atts.MaxLocsY, false);

            _templates = new MapTileTemplate[6];

            _templates[0] = new MapTileTemplate(0)
                {IsObstacle = false, Name = "Grass", TravelCost = 2, Visual = MapTileVisual.Grass};
            _templates[1] = new MapTileTemplate(1)
                { IsObstacle = false, Name = "Door", TravelCost = 5, Visual = MapTileVisual.Door };
            _templates[2] = new MapTileTemplate(2)
                { IsObstacle = true, Name = "Wall", TravelCost = 1, Visual = MapTileVisual.Wall };
            _templates[3] = new MapTileTemplate(3)
                { IsObstacle = true, Name = "Water", TravelCost = 2, Visual = MapTileVisual.Water };
            _templates[4] = new MapTileTemplate(4)
                { IsObstacle = false, Name = "Gravel", TravelCost = 3, Visual = MapTileVisual.Gravel };
            _templates[5] = new MapTileTemplate(5)
                { IsObstacle = false, Name = "Sand", TravelCost = 4, Visual = MapTileVisual.Sand };

            this.mapVisual1.Value = _map;
            mapVisual1.Center = new MapLocation(16,16);

            CreateNullTool();
            CreateBrushTool();
            _pathfindingTool = new MapPathfindingTool(_pathfinder = new Pathfinder(_map));
            mapVisual1.Tools.Add(_pathfindingTool);
            _pathfindingTool.MessageChanged += message => lblInstructions.Text = message;
        }

        private void RandomMap()
        {
            var asd = new DungeonLevel(0, true);
            var tiles = asd.GetMap();

            for (int x = 0; x < Atts.MaxLocsX; x++)
            for (int y = 0; y < Atts.MaxLocsY; y++)
            {
                MapTileTemplate selectedTemplate;

                switch (tiles[x, y].LocationType)
                {
                    case Atts.LocationType.Empty:
                        selectedTemplate = _templates[3];
                        break;
                    case Atts.LocationType.WallPoint:
                        selectedTemplate = _templates[2];
                        break;
                    case Atts.LocationType.TempDoor:
                    case Atts.LocationType.DoorPoint:
                        selectedTemplate = _templates[1];
                        break;
                    case Atts.LocationType.TempCorridor:
                    case Atts.LocationType.Corridor:
                        selectedTemplate = _templates[4];
                        break;
                    case Atts.LocationType.RoomSpace:
                        selectedTemplate = _templates[4];
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                _map.SetTile(new MapLocation(x, y), selectedTemplate, true);
            }
        }

        private void CreateNullTool()
        {
            FlowLayoutPanel container = new FlowLayoutPanel();
            container.FlowDirection = FlowDirection.LeftToRight;
            container.WrapContents = false;
            container.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            container.AutoSize = true;
            var toolSelector = new RadioButton();
            toolSelector.Text = "None";
            toolSelector.Checked = true;
            toolSelector.Location = new Point(0,0);
            container.Controls.Add(toolSelector);
            flpEditTools.Controls.Add(container);
            toolSelector.CheckedChanged += OnToolSelected;
            _lastActive = toolSelector;
        }

        private void OnToolSelected(object sender, EventArgs e)
        {
            var selected = ((RadioButton) sender).Checked;
            var tool = ((RadioButton)sender).Tag as MapVisualTool;

            if (mapVisual1.ActiveTool != tool && selected)
            {
                if (_lastActive != null && _lastActive != sender)
                {
                    _lastActive.Checked = false;
                }

                _lastActive = sender as RadioButton;
                mapVisual1.ActiveTool = tool;
            }
        }

        private void CreateBrushTool()
        {
            FlowLayoutPanel container = new FlowLayoutPanel();
            container.FlowDirection = FlowDirection.LeftToRight;
            container.WrapContents = false;
            container.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            container.AutoSize = true;

            var toolSelector = new RadioButton();
            toolSelector.Text = "Tile Brush";
            toolSelector.Checked = false;
            toolSelector.Location = new Point(0, 0);
            container.Controls.Add(toolSelector);

            var mapVisualTool = new MapBrushTool(_templates);
            toolSelector.Tag = mapVisualTool;
            mapVisual1.Tools.Add(mapVisualTool);

            var templateSelector = new ComboBox();
            templateSelector.DropDownStyle = ComboBoxStyle.DropDownList;

            for (int i = 0; i < _templates.Length; i++)
                templateSelector.Items.Add(_templates[i]);
            templateSelector.SelectedIndex = 0;

            templateSelector.SelectedIndexChanged += (sender, args) =>
            {
                if (templateSelector.SelectedIndex >= 0)
                    mapVisualTool.ActiveTemplate =
                        templateSelector.Items[templateSelector.SelectedIndex] as MapTileTemplate;
            };

            Label hint = new Label();
            hint.Text = "Hold shift while using the mouse wheel to change brush size.";
            hint.AutoSize = true;

            
            container.Controls.Add(templateSelector);
            
            flpEditTools.Controls.Add(container);
            flpEditTools.Controls.Add(hint);
            toolSelector.CheckedChanged += OnToolSelected;
        }

        private void mapVisual1_MouseClick(object sender, MouseEventArgs e)
        {
        }

        private void newToolStripButton_Click(object sender, EventArgs e)
        {
            for (int x = 0; x < Atts.MaxLocsX; x++)
            for (int y = 0; y < Atts.MaxLocsY; y++)
                _map.SetTile(new MapLocation(x, y), _templates[0], false);
        }

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            if (sfdMap.ShowDialog(this) == DialogResult.OK)
            {
                _map.Save(sfdMap.FileName);
            }
        }

        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            if (ofdMap.ShowDialog(this) == DialogResult.OK)
            {
                mapVisual1.ActiveTool = null;
                mapVisual1.Value = null;
                _map.Load(ofdMap.FileName);
                mapVisual1.Value = _map;
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            RandomMap();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                for (int x = 0; x < Atts.MaxLocsX; x++)
                for (int y = 0; y < Atts.MaxLocsY; y++)
                {
                    var mapLocation = new MapLocation(x, y);
                    if (_map.GetTile(mapLocation).IsObstacle)
                    {
                        mapVisual1.SetTileColor(mapLocation, Color.Red);
                    }
                }
            }
            else
            {
                for (int x = 0; x < Atts.MaxLocsX; x++)
                for (int y = 0; y < Atts.MaxLocsY; y++)
                {
                    var mapLocation = new MapLocation(x, y);
                    if (_map.GetTile(mapLocation).IsObstacle)
                    {
                        mapVisual1.SetTileColor(mapLocation, Color.White);
                    }
                }
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                for (int x = 0; x < Atts.MaxLocsX; x++)
                for (int y = 0; y < Atts.MaxLocsY; y++)
                {
                    var mapLocation = new MapLocation(x, y);
                    var mapTile = _map.GetTile(mapLocation);
                    if (!mapTile.IsObstacle)
                    {
                        
                        switch (mapTile.TravelCost)
                        {
                            case 1:
                                mapVisual1.SetTileColor(mapLocation, Color.White);
                                break;
                            case 2:
                                mapVisual1.SetTileColor(mapLocation, Color.LightGreen);
                                break;
                            case 3:
                                mapVisual1.SetTileColor(mapLocation, Color.DarkGreen);
                                break;
                            case 4:
                                mapVisual1.SetTileColor(mapLocation, Color.DarkBlue);
                                break;
                            case 5:
                                mapVisual1.SetTileColor(mapLocation, Color.DarkOrange);
                                break;
                        }


                    }
                }
            }
            else
            {
                for (int x = 0; x < Atts.MaxLocsX; x++)
                for (int y = 0; y < Atts.MaxLocsY; y++)
                {
                    var mapLocation = new MapLocation(x, y);
                    if (!_map.GetTile(mapLocation).IsObstacle)
                    {
                        mapVisual1.SetTileColor(mapLocation, Color.White);

                    }
                }
            }
        }

        private void btnPathfind_Click(object sender, EventArgs e)
        {
            mapVisual1.ActiveTool = null;
            mapVisual1.ActiveTool = _pathfindingTool;
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            _map.AllowDiagonalMovement = checkBox4.Checked;
        }
    }
}
