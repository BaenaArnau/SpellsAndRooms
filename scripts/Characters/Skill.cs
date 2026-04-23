using Godot;

namespace SpellsAndRooms.scripts.Characters
{
	public class Skill
	{
		public string Name { get; }
		public int ManaCost { get; }
		public int Damage { get; }
		public Character.DamageType DamageType { get; }
		public bool MultiAttack { get; }
		public bool IsHealing { get; }

		public Skill(string name, int manaCost, int damage, Character.DamageType damageType, bool multiAttack = false, bool isHealing = false)
		{
			Name = string.IsNullOrWhiteSpace(name) ? "Attack" : name;
			ManaCost = Mathf.Max(0, manaCost);
			Damage = Mathf.Max(1, damage);
			DamageType = damageType;
			MultiAttack = multiAttack;
			IsHealing = isHealing;
		}
	}
}

