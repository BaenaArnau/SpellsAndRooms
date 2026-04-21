using Godot;


namespace TerceraJAM.scripts.Characters
{
    public partial class Enemy : Character
    {
        public int Difficulty { get; }
        public int MoneyLoot { get; }

        public Enemy() : base("Enemy", 1, 1, 0, 0, 1, DamageType.Physical, DamageType.Fire)
        {
            Difficulty = 1;
            MoneyLoot = 0;
        }

        public Enemy(string name, int health, int baseHealth, int mana, int baseMana, int damage, DamageType damageResistance, DamageType damageWeakness, int difficulty, int moneyLoot)
            : base(name, health, baseHealth, mana, baseMana, damage, damageResistance, damageWeakness)
        {
            Difficulty = Mathf.Max(1, difficulty);
            MoneyLoot = Mathf.Max(0, moneyLoot);
        }
    }
}
