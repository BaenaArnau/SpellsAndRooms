using System.Collections.Generic;
using System.Text;

namespace SpellsAndRooms.scripts.Turns
{
    public static class CsvUtils
    {
        public static List<string> SplitLine(string line)
        {
            var values = new List<string>();
            if (line == null)
                return values;

            var current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                        inQuotes = !inQuotes;

                    continue;
                }

                if (c == ',' && !inQuotes)
                {
                    values.Add(current.ToString());
                    current.Clear();
                    continue;
                }

                current.Append(c);
            }

            values.Add(current.ToString());
            return values;
        }
    }
}

