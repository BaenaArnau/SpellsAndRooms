using System.Collections.Generic;
using Godot;
using SpellsAndRooms.scripts.Items;


namespace SpellsAndRooms.scripts.Characters
{
	public partial class Player : Character
	{
		public int Gold { get; private set; }
		public List<ConsumableItem> Consumables = new List<ConsumableItem>();
		public List<PassiveItem> Passives = new List<PassiveItem>();

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
		
		public void RemoveGold(int amount)
		{
			Gold -= Mathf.Max(0, amount);
		}

		public override void _Ready()
		{
			base._Ready();
			Health = BaseHealth;
			Mana = BaseMana;
		}
	}
}