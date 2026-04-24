using Godot;
using Godot.Collections;
using System.Collections.Generic;

namespace TerceraJAM.scripts.map
{
	public partial class Map : Node2D
	{
		[Export] public PackedScene MapRoomScene;
		[Export] public PackedScene MapLineScene;
		[Export(PropertyHint.Range, "1.0,3.0,0.05")] public float MapZoom = 1.7f;
		[Export] public float ScrollStep = 80.0f;
		[Export] public float ScrollSmoothness = 10.0f;
		[Export] public float TopPadding = 120.0f;
		[Export] public float BottomPadding = 120.0f;

		[Export] private CanvasLayer _menuPausa;

		private Array<Array<Room>> _mapData;
		private System.Collections.Generic.Dictionary<Room, MapRoom> _roomToMapRoom = new System.Collections.Generic.Dictionary<Room, MapRoom>();
		private HashSet<Room> _availableRooms = new HashSet<Room>();
		private Room _selectedRoom;
		private HashSet<Room> _connectedRooms = new HashSet<Room>();
		private Node2D _roomsContainer;
		private Node2D _linesContainer;
		private Node2D _visualsContainer;
		private float _minVisualY;
		private float _maxVisualY;
		private float _targetVisualY;

		public override void _Ready()
		{
			if (MapRoomScene == null)
			{
				GD.PrintErr("MapRoomScene no está asignada en el inspector");
				return;
			}

			if (MapLineScene == null)
			{
				GD.PrintErr("MapLineScene no está asignada en el inspector");
			}

			_roomsContainer = GetNodeOrNull<Node2D>("%Rooms") ?? this;
			_linesContainer = GetNodeOrNull<Node2D>("%Lines") ?? this;
			_visualsContainer = GetNodeOrNull<Node2D>("Visuals") ?? this;

			if (_visualsContainer != null)
			{
				_visualsContainer.Scale = Vector2.One * MapZoom;
			}

			GenerateAndDisplayMap();
		}

		private void GenerateAndDisplayMap()
		{
			var mapGenerator = new MapGenerator();
			_mapData = mapGenerator.GenerateMap();
			_connectedRooms = GetConnectedRooms();

			// Obtener puntos de inicio (piso 0)
			HashSet<Room> startingRooms = new HashSet<Room>();
			foreach (Room room in _mapData[0])
			{
				if (_connectedRooms.Contains(room) && room.NextRooms.Count > 0)
				{
					startingRooms.Add(room);
				}
			}

			_availableRooms = startingRooms;

			// Instanciar MapRoom para cada Room
			for (int i = 0; i < _mapData.Count; i++)
			{
				for (int j = 0; j < _mapData[i].Count; j++)
				{
					Room room = _mapData[i][j];
					if (!_connectedRooms.Contains(room))
					{
						continue;
					}

					MapRoom mapRoom = MapRoomScene.Instantiate<MapRoom>();
					mapRoom.SetRoom(room);
					mapRoom.Selected += OnRoomSelected;
					_roomsContainer.AddChild(mapRoom);
					_roomToMapRoom[room] = mapRoom;
					
					// Actualizar disponibilidad
					UpdateRoomAvailability(mapRoom, _availableRooms.Contains(room));
				}
			}

			DrawConnections();

			SetupScrollView();
		}

		private HashSet<Room> GetConnectedRooms()
		{
			var connectedRooms = new HashSet<Room>();

			foreach (Array<Room> floor in _mapData)
			{
				foreach (Room room in floor)
				{
					if (room.NextRooms.Count > 0)
					{
						connectedRooms.Add(room);
						foreach (Room nextRoom in room.NextRooms)
						{
							connectedRooms.Add(nextRoom);
						}
					}
				}
			}

			return connectedRooms;
		}

		private void DrawConnections()
		{
			foreach (Array<Room> floor in _mapData)
			{
				foreach (Room room in floor)
				{
					if (!_connectedRooms.Contains(room))
					{
						continue;
					}

					foreach (Room nextRoom in room.NextRooms)
					{
						if (!_connectedRooms.Contains(nextRoom))
						{
							continue;
						}

						Line2D line = CreateConnectionLine();
						line.AddPoint(room.Position);
						line.AddPoint(nextRoom.Position);
						_linesContainer.AddChild(line);
					}
				}
			}
		}

		private Line2D CreateConnectionLine()
		{
			if (MapLineScene != null)
			{
				Node instance = MapLineScene.Instantiate();
				if (instance is Line2D sceneLine)
				{
					return sceneLine;
				}

				GD.PrintErr("MapLineScene no instancia un Line2D. Se usará línea por defecto.");
			}

			var fallbackLine = new Line2D
			{
				Width = 5.0f,
				DefaultColor = new Color(0.95f, 0.95f, 0.95f, 0.65f)
			};

			return fallbackLine;
		}

		private void SetupScrollView()
		{
			if (_roomToMapRoom.Count == 0)
			{
				return;
			}

			bool initialized = false;
			Vector2 min = Vector2.Zero;
			Vector2 max = Vector2.Zero;

			foreach (var mapRoom in _roomToMapRoom.Values)
			{
				Vector2 p = mapRoom.Position;
				if (!initialized)
				{
					min = p;
					max = p;
					initialized = true;
					continue;
				}

				min = new Vector2(Mathf.Min(min.X, p.X), Mathf.Min(min.Y, p.Y));
				max = new Vector2(Mathf.Max(max.X, p.X), Mathf.Max(max.Y, p.Y));
			}

			Vector2 viewportSize = GetViewportRect().Size;
			float scaledMinX = min.X * MapZoom;
			float scaledMaxX = max.X * MapZoom;
			float scaledMinY = min.Y * MapZoom;
			float scaledMaxY = max.Y * MapZoom;

			float mapCenterX = (scaledMinX + scaledMaxX) * 0.5f;
			float baseX = viewportSize.X * 0.5f - mapCenterX;

			float topAlignedY = TopPadding - scaledMinY;
			float bottomAlignedY = (viewportSize.Y - BottomPadding) - scaledMaxY;

			_minVisualY = Mathf.Min(bottomAlignedY, topAlignedY);
			_maxVisualY = Mathf.Max(bottomAlignedY, topAlignedY);

			float mapHeight = scaledMaxY - scaledMinY;
			float visibleHeight = viewportSize.Y - TopPadding - BottomPadding;

			// Si el mapa cabe completo en pantalla, lo centramos y desactivamos scroll efectivo.
			if (mapHeight <= visibleHeight)
			{
				float centeredY = viewportSize.Y * 0.5f - ((scaledMinY + scaledMaxY) * 0.5f);
				_minVisualY = centeredY;
				_maxVisualY = centeredY;
			}

			// Empezamos mostrando la parte baja del mapa (inicio de ruta), estilo STS.
			_targetVisualY = bottomAlignedY;
			_visualsContainer.Position = new Vector2(baseX, _targetVisualY);
		}

		public override void _Process(double delta)
		{
			if (_visualsContainer == null)
			{
				return;
			}

			float t = Mathf.Clamp((float)delta * ScrollSmoothness, 0.0f, 1.0f);
			_visualsContainer.Position = new Vector2(
				_visualsContainer.Position.X,
				Mathf.Lerp(_visualsContainer.Position.Y, _targetVisualY, t)
			);
		}

		public override void _UnhandledInput(InputEvent @event)
		{
			if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
			{
				if (mouseButton.ButtonIndex == MouseButton.WheelUp)
				{
					ScrollBy(-GetEffectiveScrollStep());
					GetViewport().SetInputAsHandled();
				}
				else if (mouseButton.ButtonIndex == MouseButton.WheelDown)
				{
					ScrollBy(GetEffectiveScrollStep());
					GetViewport().SetInputAsHandled();
				}
			}

			if (@event.IsActionPressed("ui_up"))
			{
				ScrollBy(GetEffectiveScrollStep());
			}
			else if (@event.IsActionPressed("ui_down"))
			{
				ScrollBy(-GetEffectiveScrollStep());
			}
		}

		private float GetEffectiveScrollStep()
		{
			return ScrollStep * Mathf.Max(MapZoom, 1.0f);
		}

		private void ScrollBy(float amount)
		{
			float lower = Mathf.Min(_minVisualY, _maxVisualY);
			float upper = Mathf.Max(_minVisualY, _maxVisualY);
			_targetVisualY = Mathf.Clamp(_targetVisualY + amount, lower, upper);
		}

		private void OnRoomSelected(Room selectedRoom)
		{
			_selectedRoom = selectedRoom;
			GD.Print($"Habitación seleccionada: {selectedRoom}");

			// Limpiar disponibilidad anterior
			_availableRooms.Clear();

			// Las siguientes salas disponibles son las conectadas desde la seleccionada
			foreach (Room nextRoom in selectedRoom.NextRooms)
			{
				_availableRooms.Add(nextRoom);
			}

			// Actualizar visualización de disponibilidad
			UpdateAllRoomAvailability();
		}

		private void UpdateAllRoomAvailability()
		{
			foreach (var kvp in _roomToMapRoom)
			{
				bool isAvailable = _availableRooms.Contains(kvp.Key);
				UpdateRoomAvailability(kvp.Value, isAvailable);
			}
		}

		private void UpdateRoomAvailability(MapRoom mapRoom, bool available)
		{
			mapRoom.SetAvailable(available);
		}

		public Room GetSelectedRoom()
		{
			return _selectedRoom;
		}

		public HashSet<Room> GetAvailableRooms()
		{
			return _availableRooms;
		}

        public override void _Input(InputEvent @event)
        {
            if(@event.IsActionPressed("pausa"))
			{
				GetTree().Paused = true;
				if(_menuPausa != null)
					_menuPausa.Visible = true;
				
			}
        }
	}
}
