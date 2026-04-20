using Godot;
using System;
using System.Collections.Generic;


namespace TerceraJAM.scripts.Characters
{
	public partial class Character : Sprite2D
	{
		private string _name;
		private int _health;
		private int _baseHealth;
		private int _mana;
		private int _baseMana;
		private List<Skill> _skills;
		
		public override void _Process(double delta)
		{
		}
		
		public void TakeDamage(int amount)
		{
			_health -= amount;
		}

		public void UseSkill(Skill skill)
		{
			
		}
	}
}

