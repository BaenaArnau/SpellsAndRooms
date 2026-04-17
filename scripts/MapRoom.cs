using Godot;

namespace TerceraJAM.scripts
{
    public partial class MapRoom : Area2D
    {
        [Signal] public delegate void SelectedEventHandler(Room room);

        private Sprite2D _sprite;
        private AnimationPlayer _anim;
        private bool _available;
        private Room _roomData;
        private Label _label;

        public override void _Ready()
    {
        _sprite = GetNode<Sprite2D>("Visual/Sprite2D");
        _anim = GetNode<AnimationPlayer>("AnimationPlayer");
        
        // Intentar obtener label si existe (para depuración)
        if (HasNode("Visual/Label"))
        {
            _label = GetNode<Label>("Visual/Label");
        }
        
        // Conectar evento de entrada para detectar clicks
        InputEvent += OnInputEvent;
    }

    public void SetRoom(Room room)
    {
        _roomData = room;
        Position = room.Position;
        
        // Asignar el texto/icono según el tipo de habitación
        if (_label != null)
        {
            _label.Text = room.Type.ToString()[0].ToString();
        }
        
        // Asignar modulate color según tipo
        Modulate = GetColorForRoomType(room.Type);
    }

    public void SetAvailable(bool available)
    {
        _available = available;
        
        // Cambiar la opacidad para indicar disponibilidad
        if (_available)
        {
            Modulate = Modulate with { A = 1.0f };
        }
        else
        {
            Modulate = Modulate with { A = 0.5f };
        }
    }

    private void OnInputEvent(Node viewport, InputEvent @event, long shapeIdx)
    {
        if (!_available) return;

        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
        {
            _roomData.Selected = true;
            
            // Reproducir animación si existe
            if (_anim != null && _anim.GetAnimation("select") != null)
            {
                _anim.Play("select");
            }
            
            // Emitimos la señal para que el Map.cs sepa que fue elegida
            EmitSignal(SignalName.Selected, _roomData);
        }
    }

        private Color GetColorForRoomType(Room.RoomType type)
        {
            return type switch
            {
                Room.RoomType.Monster => Colors.Red,
                Room.RoomType.Treasure => Colors.Gold,
                Room.RoomType.Campfire => Colors.Orange,
                Room.RoomType.Shop => Colors.Blue,
                Room.RoomType.Boss => Colors.DarkRed,
                _ => Colors.Gray
            };
        }
    }
}