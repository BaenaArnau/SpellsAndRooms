using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;
using SpellsAndRooms.scripts.Characters;

namespace SpellsAndRooms.scripts.Turns
{
    public sealed class EnemyDatabase
    {
        private readonly List<EnemyTemplate> _templates = new List<EnemyTemplate>();
        private const string EnemyCsvPath = "res://Files/Enemy.csv";

        public IReadOnlyList<EnemyTemplate> Templates => _templates;

        public EnemyDatabase()
        {
            LoadFromCsv();
        }

        private void LoadFromCsv()
        {
            if (!FileAccess.FileExists(EnemyCsvPath))
            {
                GD.PrintErr($"No se encontro {EnemyCsvPath}. Se usara una lista vacia de enemigos.");
                return;
            }

            using FileAccess file = FileAccess.Open(EnemyCsvPath, FileAccess.ModeFlags.Read);
            if (file == null)
            {
                GD.PrintErr($"No se pudo abrir {EnemyCsvPath}.");
                return;
            }

            bool isHeader = true;
            while (!file.EofReached())
            {
                string line = file.GetLine().Trim();
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (isHeader)
                {
                    isHeader = false;
                    continue;
                }

                string[] cols = line.Split(',');
                if (cols.Length < 8)
                {
                    continue;
                }

                string name = cols[0].Trim();
                int health = ParseInt(cols[1], 20);
                int mana = ParseInt(cols[2], 0);
                int damage = ParseInt(cols[3], 5);
                Character.DamageType resistance = ParseDamageType(cols[4], Character.DamageType.Physical, name + "res");
                Character.DamageType weakness = ParseDamageType(cols[5], Character.DamageType.Fire, name + "weak");
                int difficulty = ParseInt(cols[6], 1);
                int loot = ParseInt(cols[7], difficulty * 8);

                var skills = new List<Skill>();
                for (int i = 8; i <= 11 && i < cols.Length; i++)
                {
                    Skill skill = BuildSkillFromName(cols[i].Trim(), damage);
                    if (skill != null)
                    {
                        skills.Add(skill);
                    }
                }

                if (skills.Count == 0)
                {
                    skills.Add(new Skill("Golpe", 0, damage, Character.DamageType.Physical));
                }

                _templates.Add(new EnemyTemplate
                {
                    Name = string.IsNullOrWhiteSpace(name) ? "Enemy" : name,
                    Health = Mathf.Max(1, health),
                    Mana = Mathf.Max(0, mana),
                    Damage = Mathf.Max(1, damage),
                    Difficulty = Mathf.Max(1, difficulty),
                    Loot = Mathf.Max(1, loot),
                    DamageResistance = resistance,
                    DamageWeakness = weakness,
                    Skills = skills.ToArray()
                });
            }
        }

        private static Skill BuildSkillFromName(string rawName, int baseDamage)
        {
            if (string.IsNullOrWhiteSpace(rawName) || rawName.Equals("null", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            string name = rawName.Trim();
            bool isHealing = name.Equals("Cure", StringComparison.OrdinalIgnoreCase);
            Character.DamageType type = ParseDamageType(name, Character.DamageType.Physical, name);
            int manaCost = Mathf.Clamp(baseDamage / 4, 0, 15);
            int power = isHealing ? Mathf.Max(4, baseDamage / 2) : Mathf.Max(4, baseDamage + 3);

            return new Skill(name, manaCost, power, type, false, isHealing);
        }

        private static Character.DamageType ParseDamageType(string value, Character.DamageType fallback, string seed)
        {
            string normalized = (value ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalized) || normalized.Equals("null", StringComparison.OrdinalIgnoreCase))
            {
                return fallback;
            }

            if (Enum.TryParse(normalized, true, out Character.DamageType parsed))
            {
                return parsed;
            }

            string lowered = normalized.ToLowerInvariant();
            if (lowered.Contains("fire") || lowered.Contains("pyro"))
            {
                return Character.DamageType.Fire;
            }
            if (lowered.Contains("water") || lowered.Contains("aqua") || lowered.Contains("agua"))
            {
                return Character.DamageType.Water;
            }
            if (lowered.Contains("earth") || lowered.Contains("terra") || lowered.Contains("tierra"))
            {
                return Character.DamageType.Earth;
            }

            if (float.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out float numeric))
            {
                int index = Mathf.Abs(Mathf.RoundToInt(numeric * 10.0f) + seed.GetHashCode()) % 4;
                return (Character.DamageType)index;
            }

            return fallback;
        }

        private static int ParseInt(string value, int fallback)
        {
            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed))
            {
                return parsed;
            }

            return fallback;
        }
    }
}

