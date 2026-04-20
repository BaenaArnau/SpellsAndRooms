using Godot;

namespace TerceraJAM.scripts.map
{
    public partial class MapRoom : Area2D
    {
        [Signal] public delegate void SelectedEventHandler(Room room);
        [Export] public float RoomScale = 0.34f;
        [Export] public float IconScale = 0.38f;

        private Sprite2D _sprite;
        private Line2D _outline;
        private AnimationPlayer _anim;
        private bool _available;
        private Room _roomData;
        private Label _label;
        private Color _baseColor = Colors.White;
        public override void _Ready() 
        {
            _sprite = GetNode<Sprite2D>("Visual/Sprite2D");
            _outline = GetNodeOrNull<Line2D>("Visual/Line2D");
            _anim = GetNode<AnimationPlayer>("AnimationPlayer");
            Scale = Vector2.One * RoomScale;

            if (_sprite != null)
            {
                _sprite.Scale = Vector2.One * IconScale;
            }
            
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
            
            // El icono base (placeholder) cambia color según tipo de sala.
            _baseColor = GetColorForRoomType(room.Type);
            ApplyAvailabilityVisuals();
        }

        public void SetAvailable(bool available)
        {
            _available = available;

            ApplyAvailabilityVisuals();

            if (_available)
            {
                if (_anim != null && _anim.GetAnimation("highlight") != null)
                {
                    _anim.Play("highlight");
                }
            }
            else
            {
                if (_anim != null && _anim.CurrentAnimation == "highlight")
                {
                    _anim.Play("RESET");
                }
            }
        }

        private void ApplyAvailabilityVisuals()
        {
            if (_sprite != null)
            {
                float alpha = _available ? 1.0f : 0.45f;
                _sprite.Modulate = _baseColor with { A = alpha };
            }

            if (_outline != null)
            {
                _outline.Modulate = _available ? new Color(1, 1, 1, 0.95f) : new Color(1, 1, 1, 0.25f);
            }

            if (_label != null)
            {
                _label.Modulate = _available ? Colors.White : new Color(1, 1, 1, 0.55f);
            }
        }

        private void OnInputEvent(Node viewport, InputEvent @event, long shapeIdx)
        {
            if (!_available) return;

            if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
            {
                _roomData.Selected = true;
                _available = false;
                
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
                Room.RoomType.Monster => new Color(0.83f, 0.33f, 0.33f),
                Room.RoomType.Treasure => new Color(0.93f, 0.78f, 0.22f),
                Room.RoomType.Campfire => new Color(0.96f, 0.55f, 0.2f),
                Room.RoomType.Shop => new Color(0.33f, 0.66f, 0.9f),
                Room.RoomType.Boss => new Color(0.6f, 0.18f, 0.18f),
                _ => new Color(0.55f, 0.55f, 0.55f)
            };
        }
    }
}