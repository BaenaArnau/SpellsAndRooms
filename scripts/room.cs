using Godot;
using System;
using System.Collections.Generic;

public partial class room : Resource
{
    public enum RoomType
    {
        NOT_ASSIGNED,
        MONSTER,
        TREASURE,
        CAMPFIRE,
        SHOP,
        COMPANION,
        BOSS
    }

    [Export] public RoomType Type;
    [Export] public int Row;
    [Export] public int Column;
    [Export] public Vector2 Position;
    [Export] public room[] NextRooms;
    [Export] public bool Selected = false;

    public override string ToString()
    {
        return $"{Column} ({Type.ToString()[0]})";
    }
}
