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
            var templates = _enemyDatabase.Templates;
            if (templates.Count == 0)
            {
                return new List<Enemy>();
            }

            int row = Mathf.Max(0, room.Row);
            int expectedDifficulty = 1 + row / 3;
            if (room.Type == Room.RoomType.Boss)
            {
                expectedDifficulty += 2;
            }

            int maxCount = Mathf.Clamp(1 + row / 4, 1, MaxEnemiesPerRoom);
            if (room.Type == Room.RoomType.Boss)
            {
                maxCount = MaxEnemiesPerRoom;
            }

            int minCount = row >= 8 ? 2 : 1;
            int count = GD.RandRange(minCount, maxCount);

            List<EnemyTemplate> pool = templates
                .Where(t => Math.Abs(t.Difficulty - expectedDifficulty) <= 2)
                .ToList();
            if (pool.Count == 0)
            {
                pool = templates.ToList();
            }

            float statScale = 1.0f + (row * 0.08f);
            if (room.Type == Room.RoomType.Boss)
            {
                statScale += 0.3f;
            }

            var enemies = new List<Enemy>(count);
            for (int i = 0; i < count; i++)
            {
                EnemyTemplate template = pool[GD.RandRange(0, pool.Count - 1)];
                enemies.Add(CreateScaledEnemy(template, statScale));
            }

            return enemies;
        }

        private static Enemy CreateScaledEnemy(EnemyTemplate template, float scale)
        {
            int health = Mathf.Max(1, Mathf.RoundToInt(template.Health * scale));
            int mana = Mathf.Max(0, Mathf.RoundToInt(template.Mana * scale));
            int damage = Mathf.Max(1, Mathf.RoundToInt(template.Damage * scale));
            int loot = Mathf.Max(1, Mathf.RoundToInt(template.Loot * scale));

            var enemy = new Enemy(
                template.Name,
                health,
                health,
                mana,
                mana,
                damage,
                template.DamageResistance,
                template.DamageWeakness,
                template.Difficulty,
                loot);

            foreach (Skill skill in template.Skills)
            {
                enemy.AddSkill(skill);
            }

            return enemy;
        }
    }
}


