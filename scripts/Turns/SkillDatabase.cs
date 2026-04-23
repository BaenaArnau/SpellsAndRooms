using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;
using SpellsAndRooms.scripts.Characters;

namespace SpellsAndRooms.scripts.Turns
{
    public sealed class SkillDatabase
    {
        private readonly Dictionary<string, Skill> _skills = new Dictionary<string, Skill>(StringComparer.OrdinalIgnoreCase);
        private const string SkillCsvPath = "res://Files/Skill.csv";

        public IReadOnlyDictionary<string, Skill> Skills => _skills;

        public SkillDatabase()
        {
            LoadFromCsv();
        }

        public bool TryGetSkill(string skillName, out Skill skill)
        {
            return _skills.TryGetValue(Normalize(skillName), out skill);
        }

        public Skill GetSkillOrDefault(string skillName, Skill fallback = null)
        {
            return TryGetSkill(skillName, out Skill skill) ? skill : fallback;
        }

        public List<Skill> ResolveSkills(IEnumerable<string> skillNames)
        {
            var result = new List<Skill>();
            if (skillNames == null)
                return result;

            foreach (string name in skillNames)
            {
                if (TryGetSkill(name, out Skill skill) && skill != null)
                    result.Add(skill);
            }

            return result;
        }

        private void LoadFromCsv()
        {
            if (!FileAccess.FileExists(SkillCsvPath))
            {
                GD.PrintErr($"No se encontro {SkillCsvPath}. No se cargaran skills desde CSV.");
                return;
            }

            using FileAccess file = FileAccess.Open(SkillCsvPath, FileAccess.ModeFlags.Read);
            if (file == null)
            {
                GD.PrintErr($"No se pudo abrir {SkillCsvPath}.");
                return;
            }

            bool isHeader = true;
            while (!file.EofReached())
            {
                string line = file.GetLine().Trim();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (isHeader)
                {
                    isHeader = false;
                    continue;
                }

                List<string> cols = CsvUtils.SplitLine(line);
                if (cols.Count < 5)
                    continue;

                string name = cols[0].Trim();
                Character.DamageType damageType = ParseDamageType(cols[1].Trim());
                int damage = ParseInt(cols[2], 1);
                int manaCost = ParseInt(cols[3], 0);
                bool multiHit = ParseBool(cols[4]);
                bool isHealing = damageType == Character.DamageType.None || cols[1].Trim().Equals("Healing", StringComparison.OrdinalIgnoreCase) || name.Equals("Cure", StringComparison.OrdinalIgnoreCase);

                if (string.IsNullOrWhiteSpace(name))
                    continue;

                _skills[Normalize(name)] = new Skill(
                    name,
                    manaCost,
                    damage,
                    damageType,
                    multiHit,
                    isHealing);
            }
        }

        private static Character.DamageType ParseDamageType(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Character.DamageType.Physical;

            if (Enum.TryParse(value, true, out Character.DamageType parsed))
                return parsed;

            if (value.Equals("Healing", StringComparison.OrdinalIgnoreCase))
                return Character.DamageType.None;

            return Character.DamageType.Physical;
        }

        private static bool ParseBool(string value)
        {
            return bool.TryParse(value, out bool parsed) && parsed;
        }

        private static int ParseInt(string value, int fallback)
        {
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed) ? parsed : fallback;
        }

        private static string Normalize(string value)
        {
            return (value ?? string.Empty).Trim();
        }
    }
}

