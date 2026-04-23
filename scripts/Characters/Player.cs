using Godot;


namespace SpellsAndRooms.scripts.Characters
{
	public partial class Player : Character
	{
		public int Gold { get; private set; }

		public Player() : base("Heroe", 1, 1, 0, 0, 1, DamageType.Physical, DamageType.Fire)
		{
		}

		public Player(string name, int health, int baseHealth, int mana, int baseMana, int damage, DamageType damageResistance, DamageType damageWeakness) : base(name, health, baseHealth, mana, baseMana, damage, damageResistance, damageWeakness)
		{
		}

		public void AddGold(int amount)
		{
			Gold += Mathf.Max(0, amount);
		}
	}
}