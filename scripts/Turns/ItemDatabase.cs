using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace SpellsAndRooms.scripts.Turns
{
    public sealed class ItemDatabase
    {
        public sealed class ConsumableDefinition
        {
            public string Name { get; init; } = "Item";
            public string Type { get; init; } = string.Empty;
            public string Subtype { get; init; } = string.Empty;
            public int Power { get; init; } = 0;
            public string Description { get; init; } = string.Empty;
        }

        private readonly List<ConsumableDefinition> _consumables = new List<ConsumableDefinition>();
        private const string ConsumableCsvPath = "res://Files/ConsumableItems.csv";

        public List<ConsumableDefinition> Consumables => _consumables;

        public ItemDatabase()
        {
            LoadConsumables();
        }

        private void LoadConsumables()
        {
            if (!FileAccess.FileExists(ConsumableCsvPath))
            {
                GD.PrintErr($"No se encontro {ConsumableCsvPath}. No se cargaran consumibles.");
                return;
            }

            using FileAccess file = FileAccess.Open(ConsumableCsvPath, FileAccess.ModeFlags.Read);
            if (file == null)
            {
                GD.PrintErr($"No se pudo abrir {ConsumableCsvPath}.");
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
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                _consumables.Add(new ConsumableDefinition
                {
                    Name = name,
                    Type = cols[1].Trim(),
                    Subtype = cols[2].Trim(),
                    Power = ParseInt(cols[3], 0),
                    Description = cols[4].Trim()
                });
            }
        }

        private static int ParseInt(string value, int fallback)
        {
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed) ? parsed : fallback;
        }
    }
}


