using SpellsAndRooms.scripts.Characters;

namespace SpellsAndRooms.scripts.Turns
{
    public sealed class EnemyTemplate
    {
        public string Name { get; init; } = "Enemy";
        public int Health { get; init; }
        public int Mana { get; init; }
        public int Damage { get; init; }
        public int Difficulty { get; init; }
        public int Loot { get; init; }
        public Character.DamageType DamageResistance { get; init; }
        public Character.DamageType DamageWeakness { get; init; }
        public Skill[] Skills { get; init; } = [];
    }
}

