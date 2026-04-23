using System.Collections.Generic;

namespace SpellsAndRooms.scripts.Turns
{
    public sealed class BattleResult
    {
        public bool PlayerWon { get; set; }
        public int EarnedGold { get; set; }
        public List<string> Log { get; } = new List<string>();
    }
}


