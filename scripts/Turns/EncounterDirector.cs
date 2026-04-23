using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using SpellsAndRooms.scripts.Characters;
using SpellsAndRooms.scripts.map;

namespace SpellsAndRooms.scripts.Turns
{
    public sealed class EncounterDirector
    {
        public const int MaxEnemiesPerRoom = 4;

        private readonly EnemyDatabase _enemyDatabase;

        public EncounterDirector(EnemyDatabase enemyDatabase)
        {
            _enemyDatabase = enemyDatabase;
        }

        public List<Enemy> BuildEncounter(Room room)
        {
            IReadOnlyList<EnemyTemplate> templates = _enemyDatabase.Templates;
            if (templates.Count == 0)
            {
                return new List<Enemy>();
            }

            int row = Mathf.Max(0, room.Row);
            int expectedDifficulty = 1 + row / 3;
            if (room.Type == Room.RoomType.Boss)
            {
                expectedDifficulty += 3;
            }

            List<EnemyTemplate> pool = templates
                .Where(t => Math.Abs(t.Difficulty - expectedDifficulty) <= 2)
                .ToList();

            if (pool.Count == 0)
            {
                pool = templates.OrderByDescending(t => t.Difficulty).ToList();
            }

            int count = room.Type == Room.RoomType.Boss
                ? 1
                : Mathf.Clamp(1 + row / 4, 1, MaxEnemiesPerRoom);

            if (room.Type == Room.RoomType.Boss)
            {
                pool = pool.OrderByDescending(t => t.Difficulty).ToList();
            }

            var enemies = new List<Enemy>(count);
            for (int i = 0; i < count; i++)
            {
                EnemyTemplate template = pool[GD.RandRange(0, pool.Count - 1)];
                Enemy enemy = CreateEnemyFromTemplate(template);
                if (enemy != null)
                {
                    enemies.Add(enemy);
                }
            }

            return enemies;
        }

        private static Enemy CreateEnemyFromTemplate(EnemyTemplate template)
        {
            if (template?.Scene == null)
            {
                return null;
            }

            Enemy enemy = template.Scene.Instantiate<Enemy>();
            if (enemy == null)
            {
                return null;
            }

            enemy.Health = enemy.BaseHealth;
            enemy.Mana = enemy.BaseMana;

            foreach (Skill skill in template.Skills)
            {
                enemy.AddSkill(skill);
            }

            return enemy;
        }
    }
}
