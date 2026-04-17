using Godot;
using Godot.Collections;
using System.Collections.Generic;

namespace TerceraJAM.scripts
{
    public partial class Map : Node2D
    {
        [Export] public PackedScene MapRoomScene;
        [Export] public Color AvailableColor = Colors.White;
        [Export] public Color UnavailableColor = Colors.Gray;

        private Array<Array<Room>> _mapData;
        private System.Collections.Generic.Dictionary<Room, MapRoom> _roomToMapRoom = new System.Collections.Generic.Dictionary<Room, MapRoom>();
        private HashSet<Room> _availableRooms = new HashSet<Room>();
        private Room _selectedRoom;
        private Node2D _roomsContainer;

        public override void _Ready()
        {
            if (MapRoomScene == null)
            {
                GD.PrintErr("MapRoomScene no está asignada en el inspector");
                return;
            }

            _roomsContainer = GetNodeOrNull<Node2D>("%Rooms") ?? this;

            GenerateAndDisplayMap();
        }

        private void GenerateAndDisplayMap()
        {
            var mapGenerator = new MapGenerator();
            _mapData = mapGenerator.GenerateMap();

            // Obtener puntos de inicio (piso 0)
            HashSet<Room> startingRooms = new HashSet<Room>();
            foreach (Room room in _mapData[0])
            {
                if (room.NextRooms.Count > 0)
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
                    MapRoom mapRoom = MapRoomScene.Instantiate<MapRoom>();
                    mapRoom.SetRoom(room);
                    mapRoom.Selected += OnRoomSelected;
                    _roomsContainer.AddChild(mapRoom);
                    _roomToMapRoom[room] = mapRoom;
                    
                    // Actualizar disponibilidad
                    UpdateRoomAvailability(mapRoom, _availableRooms.Contains(room));
                }
            }

            CenterRoomsInViewport();
        }

        private void CenterRoomsInViewport()
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

            Vector2 mapCenter = (min + max) * 0.5f;
            Vector2 viewportCenter = GetViewportRect().Size * 0.5f;

            // Desplaza todas las habitaciones para que el centro del mapa coincida con el centro de pantalla.
            _roomsContainer.Position = viewportCenter - mapCenter;
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
    }
}


