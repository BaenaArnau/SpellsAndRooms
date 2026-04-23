using Godot;
using Godot.Collections;


namespace SpellsAndRooms.scripts.map
{
    public partial class Room : Resource
    {
        public enum RoomType
        {
            NotAssigned,
            Monster,
            Treasure,
            Campfire,
            Shop,
            Boss
        }

        [Export] public RoomType Type;
        [Export] public int Row;
        [Export] public int Column;
        [Export] public Vector2 Position; // Posición en el mundo 2D
        [Export] public Array<Room> NextRooms = new Array<Room>();
        [Export] public bool Selected;

        // Para depuración en consola, similar al tutorial
        public override string ToString()
        {
            return $"{Column} ({Type.ToString()[0]})";
        }
    }
}
